(function () {
    'use strict';

    const MONTHS_SHORT = ['янв', 'фев', 'мар', 'апр', 'май', 'июн', 'июл', 'авг', 'сен', 'окт', 'ноя', 'дек'];
    
    const serverurl = 'http://localhost:5286';
    
    const API = {
        years: serverurl + '/api/v1/content/writedYears',
        passengers: (year) => `${serverurl}/api/v1/statistics/passengers/byYearInMonth/${year}`,
        payment: (year) => `${serverurl}/api/v1/statistics/payment/byYearInMonth/${year}`
    };

    const KINDS = ['passengers', 'payment'];

    const state = {};
    KINDS.forEach((kind) => {
        state[kind] = { year: null, data: [], view: 'both', chart: null };
    });



    const $ = (selector, root) => (root || document).querySelector(selector);
    const $$ = (selector, root) => Array.from((root || document).querySelectorAll(selector));

    function fmt(value, digits) {
        if (value === null || value === undefined || Number.isNaN(value)) return '—';
        const options = digits
            ? { minimumFractionDigits: digits, maximumFractionDigits: digits }
            : { maximumFractionDigits: 0 };
        return Number(value).toLocaleString('ru-RU', options);
    }

    function escapeHtml(text) {
        return String(text)
            .replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;');
    }

    const monthLabel = (row) => `${MONTHS_SHORT[(row.month || 1) - 1]} ${row.year}`;

    async function fetchJson(url) {
        const response = await fetch(url, { headers: { accept: 'application/json' } });
        if (!response.ok) throw new Error(`Сервер вернул ошибку ${response.status}`);
        return response.json();
    }



    const TABLE_COLUMNS = {
        passengers: {
            compact: [
                { title: 'Год', get: (r) => r.year },
                { title: 'Месяц', get: (r) => r.month },
                { title: 'Общее кол-во', num: true, get: (r) => fmt(r.sumPassengerCount) },
                { title: 'Без льгот', num: true, get: (r) => fmt(r.casualCount) },
                { title: 'С льготами', num: true, get: (r) => fmt(r.sumBenefitCount) },
                { title: '% льготников', num: true, get: (r) => `${fmt(r.sumBenefitPercent, 2)}%` }
            ],
            full: [
                { title: 'Год', get: (r) => r.year },
                { title: 'Месяц', get: (r) => r.month },
                { title: 'Без льгот', num: true, get: (r) => fmt(r.casualCount) },
                { title: 'Без льгот %', num: true, get: (r) => `${fmt(r.casualPercent, 2)}%` },
                { title: 'Студенты', num: true, get: (r) => fmt(r.studentCount) },
                { title: 'Студенты %', num: true, get: (r) => `${fmt(r.studentPercent, 2)}%` },
                { title: 'Фед. льготы', num: true, get: (r) => fmt(r.fedBenefitCount) },
                { title: 'Фед. льготы %', num: true, get: (r) => `${fmt(r.fedBenefitPercent, 2)}%` },
                { title: 'Рег. льготы', num: true, get: (r) => fmt(r.regBenefitCount) },
                { title: 'Рег. льготы %', num: true, get: (r) => `${fmt(r.regBenefitPercent, 2)}%` },
                { title: 'Прочие льготы', num: true, get: (r) => fmt(r.anotherBenefitCount) },
                { title: 'Прочие льготы %', num: true, get: (r) => `${fmt(r.anotherBenefitPercent, 2)}%` },
                { title: 'Всего с льготами', num: true, get: (r) => fmt(r.sumBenefitCount) },
                { title: 'Всего с льготами %', num: true, get: (r) => `${fmt(r.sumBenefitPercent, 2)}%` },
                { title: 'Всего пассажиров', num: true, get: (r) => fmt(r.sumPassengerCount) }
            ]
        },
        payment: {
            compact: [
                { title: 'Год', get: (r) => r.year },
                { title: 'Месяц', get: (r) => r.month },
                { title: 'Общий доход', num: true, get: (r) => fmt(r.summPayment) },
                { title: 'Доход (без льгот)', num: true, get: (r) => fmt(r.casualPayment) },
                { title: 'Доход (льготники)', num: true, get: (r) => fmt((r.summPayment ?? 0) - (r.casualPayment ?? 0)) },
                { title: 'Вагоно-км', num: true, get: (r) => fmt(r.trainKmPerMonth) }
            ],
            full: [
                { title: 'Год', get: (r) => r.year },
                { title: 'Месяц', get: (r) => r.month },
                { title: 'Без льгот', num: true, get: (r) => fmt(r.casualPayment) },
                { title: 'Студенты', num: true, get: (r) => fmt(r.studentPayment) },
                { title: 'Фед. льготы', num: true, get: (r) => fmt(r.fedBenefitPayment) },
                { title: 'Рег. льготы', num: true, get: (r) => fmt(r.regBenefitPayment) },
                { title: 'Прочие', num: true, get: (r) => fmt(r.anotherPayment) },
                { title: 'Общий доход', num: true, get: (r) => fmt(r.summPayment) },
                { title: 'Вагоно-км', num: true, get: (r) => fmt(r.trainKmPerMonth) }
            ]
        }
    };


    function buildChartOptions(kind, rows, height) {
        const base = {
            chart: {
                type: 'line',
                height: height,
                fontFamily: 'Inter, "Segoe UI", Arial, sans-serif',
                zoom: { enabled: false },
                toolbar: {
                    show: true,
                    tools: {
                        download: false, selection: false, pan: false,
                        reset: false, zoom: false, zoomin: false, zoomout: false
                    }
                }
            },
            colors: ['#1769e0', '#22a06b', '#e58a1f'],
            stroke: { curve: 'smooth', width: 2.5 },
            markers: { size: 3 },
            xaxis: {
                categories: rows.map(monthLabel),
                labels: { rotate: -45 }
            },
            grid: { borderColor: '#e4e9f0', strokeDashArray: 4 },
            legend: { position: 'top', horizontalAlign: 'left', offsetY: -6 },
            tooltip: { shared: true, intersect: false }
        };

        if (kind === 'passengers') {
            return {
                ...base,
                series: [
                    { name: 'Всего пассажиров', data: rows.map((r) => r.sumPassengerCount ?? 0) },
                    { name: 'Без льгот', data: rows.map((r) => r.casualCount ?? 0) },
                    { name: 'С льготами', data: rows.map((r) => r.sumBenefitCount ?? 0) }
                ],
                yaxis: { labels: { formatter: (v) => fmt(v) } },
                tooltip: { ...base.tooltip, y: { formatter: (v) => `${fmt(v)} чел.` } }
            };
        }

        return {
            ...base,
            series: [
                { name: 'Общий доход', data: rows.map((r) => r.summPayment ?? 0) },
                { name: 'Без льгот (casual)', data: rows.map((r) => r.casualPayment ?? 0) },
                { name: 'С льготами', data: rows.map((r) => (r.summPayment ?? 0) - (r.casualPayment ?? 0)) }
            ],
            yaxis: { labels: { formatter: (v) => fmt(v) } },
            tooltip: { ...base.tooltip, y: { formatter: (v) => `${fmt(v)} ₽` } }
        };
    }

    function showLoading(kind) {
        const card = $(`.stats-card[data-kind="${kind}"]`);
        $('.stats-chart', card).innerHTML =
            '<div class="stats-state"><div class="loading-state"><div class="spinner"></div><span>Загрузка графика…</span></div></div>';
        const table = $('.stats-table', card);
        table.tHead.innerHTML = '';
        table.tBodies[0].innerHTML =
            '<tr><td class="stats-state-cell"><div class="loading-state"><div class="spinner"></div><span>Загрузка…</span></div></td></tr>';
    }

    function showError(kind, error) {
        const card = $(`.stats-card[data-kind="${kind}"]`);
        $('.stats-chart', card).innerHTML = `
            <div class="stats-state">
                <div class="error-state">
                    <strong>Не удалось загрузить данные</strong>
                    <span>${escapeHtml((error && error.message) || 'Неизвестная ошибка')}</span>
                    <button type="button" data-retry="${kind}">Повторить</button>
                </div>
            </div>`;
        $('.stats-table', card).tBodies[0].innerHTML =
            '<tr><td class="stats-state-cell">Данные недоступны</td></tr>';
    }


    function renderTable(kind) {
        const s = state[kind];
        const card = $(`.stats-card[data-kind="${kind}"]`);
        const table = $('.stats-table', card);
        const columns = TABLE_COLUMNS[kind][s.view === 'table' ? 'full' : 'compact'];

        table.tHead.innerHTML =
            `<tr>${columns.map((c) => `<th scope="col">${c.title}</th>`).join('')}</tr>`;

        if (!s.data.length) {
            table.tBodies[0].innerHTML =
                `<tr><td class="stats-state-cell" colspan="${columns.length}">Нет данных за выбранный период</td></tr>`;
            return;
        }

        table.tBodies[0].innerHTML = s.data.map((row) =>
            `<tr>${columns.map((c) => `<td${c.num ? ' class="numeric-cell"' : ''}>${c.get(row)}</td>`).join('')}</tr>`
        ).join('');
    }

    function renderChart(kind) {
        const s = state[kind];
        const card = $(`.stats-card[data-kind="${kind}"]`);
        const el = $('.stats-chart', card);

        if (s.chart) { s.chart.destroy(); s.chart = null; }
        el.innerHTML = '';

        if (!s.data.length) {
            el.innerHTML =
                '<div class="stats-state"><div class="empty-state"><span>Нет данных за выбранный период</span></div></div>';
            return;
        }

        const height = s.view === 'chart' ? 430 : 350;
        s.chart = new ApexCharts(el, buildChartOptions(kind, s.data, height));
        s.chart.render();
    }

    async function loadKind(kind) {
        const s = state[kind];
        if (s.year === null) return;

        showLoading(kind);
        try {
            const url = (kind === 'passengers' ? API.passengers : API.payment)(s.year);
            const rows = await fetchJson(url);
            s.data = Array.isArray(rows) ? rows : [];
            s.data.sort((a, b) => (a.year - b.year) || (a.month - b.month));

            renderTable(kind);
            if (s.view !== 'table') renderChart(kind);
        } catch (error) {
            showError(kind, error);
        }
    }



    function setView(kind, view) {
        const s = state[kind];
        if (s.view === view) return;
        s.view = view;

        const card = $(`.stats-card[data-kind="${kind}"]`);
        const content = $('.stats-card__content', card);
        content.classList.remove('view-both', 'view-table', 'view-chart');
        content.classList.add(`view-${view}`);
        $$('.stats-view-btn', card).forEach((btn) =>
            btn.classList.toggle('is-active', btn.dataset.view === view));

        if (view !== 'chart') renderTable(kind);   
        if (view !== 'table') renderChart(kind);   
    }
    

    function fillYearSelect(kind, years, defaultYear) {
        const card = $(`.stats-card[data-kind="${kind}"]`);
        const select = $('.stats-year-select', card);

        select.innerHTML = years.map((y) => {
            const full = y < 100 ? 2000 + Number(y) : y; // 26 → 2026
            return `<option value="${y}">${full}</option>`;
        }).join('');

        const preferred = years.includes(defaultYear) ? defaultYear : years[0];
        select.value = String(preferred);
        state[kind].year = Number(preferred);

        select.addEventListener('change', () => {
            state[kind].year = Number(select.value);
            loadKind(kind);
        });
    }

    async function init() {
        const page = $('.stats-page');
        if (!page) return;

        const defaultYear = Number(page.dataset.defaultYear || (new Date().getFullYear() % 100));

        let years = [];
        try {
            years = await fetchJson(API.years);
        } catch {
            years = [defaultYear];
        }
        if (!Array.isArray(years) || !years.length) years = [defaultYear];

        KINDS.forEach((kind) => {
            const card = $(`.stats-card[data-kind="${kind}"]`);

            fillYearSelect(kind, years, defaultYear);

            $$('.stats-view-btn', card).forEach((btn) => {
                btn.addEventListener('click', () => setView(kind, btn.dataset.view));
            });

            loadKind(kind);
        });
        
        document.addEventListener('click', (event) => {
            const retry = event.target.closest('[data-retry]');
            if (retry) loadKind(retry.dataset.retry);
        });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();