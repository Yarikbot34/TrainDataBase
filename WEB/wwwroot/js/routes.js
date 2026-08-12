"use strict";

const routesApiUrl = "/api/v1/TableView/routes";
const routesFilterApiUrl = "/api/v1/TableView/routes/filter";
const trainsApiBaseUrl = "/api/v1/TableView/trains";
const contentApiBaseUrl = "/api/v1/content";

const filterEndpoints = {
    years: `${contentApiBaseUrl}/writedYears`,
    months: `${contentApiBaseUrl}/writedMonths`,
    numbers: `${contentApiBaseUrl}/writedNumbers`,
    stations: `${contentApiBaseUrl}/writedStations`
};

const routesColumnCount = 22;
const trainsColumnCount = 8;

const workspace = document.getElementById("workspace");
const tableBody = document.getElementById("routesTableBody");
const recordsCounter = document.getElementById("recordsCounter");
const searchInput = document.getElementById("searchInput");
const refreshButton = document.getElementById("refreshButton");
const trainsTableBody = document.getElementById("trainsTableBody");
const trainsCounter = document.getElementById("trainsCounter");
const selectedRouteTitle = document.getElementById("selectedRouteTitle");
const closeDetailsButton = document.getElementById("closeDetailsButton");
const notification = document.getElementById("notification");
const filtersBar = document.getElementById("filtersBar");
const resetFiltersButton = document.getElementById("resetFiltersButton");

const integerFormatter = new Intl.NumberFormat("ru-RU", {
    maximumFractionDigits: 0
});
const decimalFormatter = new Intl.NumberFormat("ru-RU", {
    minimumFractionDigits: 0,
    maximumFractionDigits: 2
});
const moneyFormatter = new Intl.NumberFormat("ru-RU", {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2
});

let routes = [];
let notificationTimer = null;
let selectedRouteRequestId = 0;

function toNumber(value) {
    const number = Number(value);
    return Number.isFinite(number) ? number : 0;
}

function formatYear(year) {
    const numericYear = toNumber(year);
    if (numericYear >= 0 && numericYear < 100) {
        return String(2000 + numericYear);
    }
    return String(numericYear);
}

function getApiYear(year) {
    return String(year ?? "");
}

function formatMonth(month) {
    return String(toNumber(month)).padStart(2, "0");
}

function getApiMonth(month) {
    return String(toNumber(month));
}

function formatInteger(value) {
    return integerFormatter.format(toNumber(value));
}

function formatDecimal(value) {
    return decimalFormatter.format(toNumber(value));
}

function formatMoney(value) {
    return moneyFormatter.format(toNumber(value));
}

function formatTime(value) {
    if (!value) {
        return "—";
    }
    return String(value).slice(0, 5);
}

function getCategory(route, categoryName) {
    const category = route?.[categoryName];
    return {
        count: toNumber(category?.count),
        payment: toNumber(category?.payment),
        wayLength: toNumber(category?.wayLength),
        paymentBySubject: toNumber(category?.paymentBySubject)
    };
}

function extractItems(payload) {
    if (Array.isArray(payload)) {
        return payload;
    }
    if (Array.isArray(payload?.result)) {
        return payload.result;
    }
    if (Array.isArray(payload?.data)) {
        return payload.data;
    }
    if (Array.isArray(payload?.items)) {
        return payload.items;
    }
    if (Array.isArray(payload?.content)) {
        return payload.content;
    }
    return [];
}



const filterDefinitions = [
    {
        key: "year",
        label: "Год",
        placeholder: "Все годы",
        endpoint: filterEndpoints.years,
        format: formatYear,
        compare: (a, b) => toNumber(b) - toNumber(a)
    },
    {
        key: "month",
        label: "Месяц",
        placeholder: "Все месяцы",
        endpoint: filterEndpoints.months,
        format: formatMonth,
        compare: (a, b) => toNumber(a) - toNumber(b)
    },
    {
        key: "number",
        label: "Номер маршрута",
        placeholder: "Все номера",
        endpoint: filterEndpoints.numbers,
        format: (value) => String(value),
        compare: (a, b) => String(a).localeCompare(String(b), "ru", { numeric: true, sensitivity: "base" })
    },
    {
        key: "stationFrom",
        label: "Станция отправления",
        placeholder: "Все станции",
        endpoint: filterEndpoints.stations,
        format: (value) => String(value),
        compare: (a, b) => String(a).localeCompare(String(b), "ru", { sensitivity: "base" })
    },
    {
        key: "stationTo",
        label: "Станция прибытия",
        placeholder: "Все станции",
        endpoint: filterEndpoints.stations,
        format: (value) => String(value),
        compare: (a, b) => String(a).localeCompare(String(b), "ru", { sensitivity: "base" })
    }
];

const filterState = {
    year: null,
    month: null,
    number: null,
    stationFrom: null,
    stationTo: null
};

const comboboxes = [];

function hasActiveFilters() {
    return Object.values(filterState).some((value) => value !== null && value !== "");
}

function buildRoutesRequestUrl() {
    if (!hasActiveFilters()) {
        return routesApiUrl;
    }

    const params = new URLSearchParams();

    if (filterState.year !== null) {
        params.set("year", String(filterState.year));
    }
    if (filterState.month !== null) {
        params.set("month", String(filterState.month));
    }
    if (filterState.number) {
        params.set("number", String(filterState.number));
    }
    if (filterState.stationFrom) {
        params.set("stationFrom", String(filterState.stationFrom));
    }
    if (filterState.stationTo) {
        params.set("stationTo", String(filterState.stationTo));
    }

    return `${routesFilterApiUrl}?${params.toString()}`;
}



function createFilterCombobox(definition) {
    const root = document.createElement("div");
    root.className = "filter-combobox";
    root.dataset.filter = definition.key;

    root.innerHTML = `
        <span class="filter-combobox__label">${definition.label}</span>
        <div class="filter-combobox__control">
            <button type="button" class="filter-combobox__button" aria-haspopup="listbox" aria-expanded="false">
                <span class="filter-combobox__value is-placeholder">${definition.placeholder}</span>
                <svg class="filter-combobox__caret" viewBox="0 0 24 24" aria-hidden="true">
                    <path d="M7 10l5 5 5-5z"></path>
                </svg>
            </button>
            <div class="filter-combobox__dropdown" hidden>
                <div class="filter-combobox__search-wrap">
                    <svg viewBox="0 0 24 24" aria-hidden="true">
                        <path d="M15.5 14h-.79l-.28-.27C15.41 12.59 16 11.11 16 9.5 16 5.91 13.09 3 9.5 3S3 5.91 3 9.5 5.91 16 9.5 16c1.61 0 3.09-.59 4.23-1.57l.27.28v.79l5 4.99L20.49 19l-4.99-5zm-6 0C7.01 14 5 11.99 5 9.5S7.01 5 9.5 5 14 7.01 14 9.5 11.99 14 9.5 14z"></path>
                    </svg>
                    <input type="search" class="filter-combobox__search" placeholder="Поиск…" autocomplete="off" />
                </div>
                <ul class="filter-combobox__list" role="listbox"></ul>
                <div class="filter-combobox__empty" hidden>Ничего не найдено</div>
            </div>
        </div>
    `;

    const combobox = {
        definition,
        root,
        options: [],
        button: root.querySelector(".filter-combobox__button"),
        valueSpan: root.querySelector(".filter-combobox__value"),
        dropdown: root.querySelector(".filter-combobox__dropdown"),
        searchInput: root.querySelector(".filter-combobox__search"),
        list: root.querySelector(".filter-combobox__list"),
        emptyState: root.querySelector(".filter-combobox__empty")
    };

    combobox.button.addEventListener("click", () => {
        if (combobox.dropdown.hidden) {
            openDropdown(combobox);
        } else {
            closeDropdown(combobox);
        }
    });

    combobox.searchInput.addEventListener("input", () => {
        renderOptions(combobox, combobox.searchInput.value);
    });

    combobox.searchInput.addEventListener("keydown", (event) => {
        if (event.key === "Escape") {
            event.stopPropagation();
            closeDropdown(combobox);
        }
    });

    combobox.list.addEventListener("click", (event) => {
        const option = event.target.closest(".filter-combobox__option");
        if (!option) return;
        selectOption(combobox, option);
    });

    return combobox;
}

function openDropdown(combobox) {
    comboboxes.forEach((cb) => {
        if (cb !== combobox) closeDropdown(cb);
    });

    combobox.root.classList.add("is-open");
    combobox.dropdown.hidden = false;
    combobox.button.setAttribute("aria-expanded", "true");
    combobox.searchInput.value = "";
    renderOptions(combobox, "");
    combobox.searchInput.focus();
}

function closeDropdown(combobox) {
    if (combobox.dropdown.hidden) return;
    combobox.root.classList.remove("is-open");
    combobox.dropdown.hidden = true;
    combobox.button.setAttribute("aria-expanded", "false");
}

function renderOptions(combobox, query) {
    const normalized = String(query ?? "").trim().toLocaleLowerCase("ru");

    combobox.list.textContent = "";

    const fragment = document.createDocumentFragment();
    let visibleCount = 0;

    if (!normalized) {
        const allOption = document.createElement("li");
        allOption.className = "filter-combobox__option filter-combobox__option--all";
        allOption.setAttribute("role", "option");
        allOption.dataset.index = "-1";
        allOption.textContent = combobox.definition.placeholder;
        if (filterState[combobox.definition.key] === null) {
            allOption.classList.add("is-selected");
        }
        fragment.append(allOption);
        visibleCount += 1;
    }

    combobox.options.forEach((raw, index) => {
        const display = combobox.definition.format(raw);
        if (normalized && !display.toLocaleLowerCase("ru").includes(normalized)) {
            return;
        }

        const option = document.createElement("li");
        option.className = "filter-combobox__option";
        option.setAttribute("role", "option");
        option.dataset.index = String(index);
        option.textContent = display;

        if (String(filterState[combobox.definition.key]) === String(raw)) {
            option.classList.add("is-selected");
        }

        fragment.append(option);
        visibleCount += 1;
    });

    combobox.list.append(fragment);
    combobox.emptyState.hidden = visibleCount !== 0;
}

function selectOption(combobox, optionElement) {
    const index = Number(optionElement.dataset.index);
    const key = combobox.definition.key;

    filterState[key] = index === -1 ? null : combobox.options[index];

    updateValueLabel(combobox);
    updateResetButtonState();
    closeDropdown(combobox);
    loadRoutes();
}

function updateValueLabel(combobox) {
    const raw = filterState[combobox.definition.key];

    if (raw === null || raw === undefined || raw === "") {
        combobox.valueSpan.textContent = combobox.definition.placeholder;
        combobox.valueSpan.classList.add("is-placeholder");
    } else {
        combobox.valueSpan.textContent = combobox.definition.format(raw);
        combobox.valueSpan.classList.remove("is-placeholder");
    }
}

async function loadComboboxOptions(combobox) {
    try {
        const response = await fetch(combobox.definition.endpoint, {
            method: "GET",
            headers: {
                Accept: "application/json"
            },
            credentials: "same-origin",
            cache: "no-store"
        });

        if (!response.ok) {
            throw new Error(`Сервер вернул ошибку ${response.status}.`);
        }

        const payload = await response.json();
        const items = extractItems(payload);

        combobox.options = combobox.definition.compare
            ? [...items].sort(combobox.definition.compare)
            : items;
    } catch (error) {
        console.error(`Ошибка загрузки справочника "${combobox.definition.label}":`, error);
        combobox.options = [];
    }
}

function updateResetButtonState() {
    resetFiltersButton.classList.toggle("is-active", hasActiveFilters());
}

function resetFilters() {
    filterDefinitions.forEach((definition) => {
        filterState[definition.key] = null;
    });

    comboboxes.forEach(updateValueLabel);
    updateResetButtonState();
    loadRoutes();
}

function initFilters() {
    filterDefinitions.forEach((definition) => {
        const combobox = createFilterCombobox(definition);
        comboboxes.push(combobox);
        filtersBar.insertBefore(combobox.root, resetFiltersButton);
        loadComboboxOptions(combobox);
    });

    resetFiltersButton.addEventListener("click", resetFilters);

    document.addEventListener("click", (event) => {
        comboboxes.forEach((cb) => {
            if (!cb.root.contains(event.target)) {
                closeDropdown(cb);
            }
        });
    });

    updateResetButtonState();
}



function sortRoutes(items) {
    return [...items].sort((first, second) => {
        const yearDifference = toNumber(second.year) - toNumber(first.year);
        if (yearDifference !== 0) {
            return yearDifference;
        }
        const monthDifference = toNumber(second.month) - toNumber(first.month);
        if (monthDifference !== 0) {
            return monthDifference;
        }
        return String(first.routeNumber ?? "").localeCompare(
            String(second.routeNumber ?? ""),
            "ru",
            {
                numeric: true,
                sensitivity: "base"
            }
        );
    });
}

function createCell(value, className = "") {
    const cell = document.createElement("td");
    if (className) {
        cell.className = className;
    }
    cell.textContent = value;
    return cell;
}

function createRouteButton(route) {
    const cell = document.createElement("td");
    cell.className = "sticky-column sticky-column--route route-cell";

    const button = document.createElement("button");
    button.type = "button";
    button.className = "route-link";
    button.textContent = route.routeNumber ?? "—";
    button.title = "Открыть данные по поездам маршрута";
    button.addEventListener("click", () => {
        openDetailsPanel(route);
    });

    cell.append(button);
    return cell;
}

function createRouteRow(route) {
    const row = document.createElement("tr");

    const casual = getCategory(route, "casual");
    const student = getCategory(route, "student");
    const fedBenefit = getCategory(route, "fedBenefit");
    const regBenefit = getCategory(route, "regBenefit");
    const another = getCategory(route, "another");

    row.append(
        createCell(formatYear(route.year), "sticky-column sticky-column--year"),
        createCell(formatMonth(route.month), "sticky-column sticky-column--month"),
        createRouteButton(route),
        createCell(formatInteger(casual.count), "numeric-cell"),
        createCell(formatInteger(student.count), "numeric-cell"),
        createCell(formatInteger(fedBenefit.count), "numeric-cell"),
        createCell(formatInteger(regBenefit.count), "numeric-cell"),
        createCell(formatInteger(another.count), "numeric-cell"),
        createCell(formatDecimal(casual.wayLength), "numeric-cell"),
        createCell(formatDecimal(student.wayLength), "numeric-cell"),
        createCell(formatDecimal(fedBenefit.wayLength), "numeric-cell"),
        createCell(formatDecimal(regBenefit.wayLength), "numeric-cell"),
        createCell(formatDecimal(another.wayLength), "numeric-cell"),
        createCell(formatMoney(casual.payment), "numeric-cell"),
        createCell(formatMoney(student.payment), "numeric-cell"),
        createCell(formatMoney(fedBenefit.payment), "numeric-cell"),
        createCell(formatMoney(regBenefit.payment), "numeric-cell"),
        createCell(formatMoney(another.payment), "numeric-cell"),
        createCell(formatMoney(student.paymentBySubject), "numeric-cell"),
        createCell(formatMoney(fedBenefit.paymentBySubject), "numeric-cell"),
        createCell(formatMoney(regBenefit.paymentBySubject), "numeric-cell"),
        createCell(formatMoney(another.paymentBySubject), "numeric-cell")
    );

    return row;
}

function renderRoutes(items) {
    tableBody.replaceChildren();

    if (items.length === 0) {
        const row = document.createElement("tr");
        const cell = document.createElement("td");
        cell.colSpan = routesColumnCount;
        cell.className = "state-cell";
        cell.textContent = "Маршруты не найдены";
        row.append(cell);
        tableBody.append(row);
        return;
    }

    const fragment = document.createDocumentFragment();
    items.forEach((route) => {
        fragment.append(createRouteRow(route));
    });
    tableBody.append(fragment);
}

function updateRoutesCounter(visibleCount, totalCount = routes.length) {
    if (visibleCount === totalCount) {
        recordsCounter.textContent =
            `Всего записей: ${integerFormatter.format(totalCount)}`;
        return;
    }
    recordsCounter.textContent =
        `Показано: ${integerFormatter.format(visibleCount)} из ${integerFormatter.format(totalCount)}`;
}

function filterRoutes() {
    const query = searchInput.value.trim().toLocaleLowerCase("ru");

    if (!query) {
        renderRoutes(routes);
        updateRoutesCounter(routes.length);
        return;
    }

    const filteredRoutes = routes.filter((route) => {
        const routeNumber = String(route.routeNumber ?? "").toLocaleLowerCase("ru");
        const year = formatYear(route.year);
        const month = formatMonth(route.month);
        const routeId = String(route.routeId ?? "");
        const id = String(route.id ?? "");

        return routeNumber.includes(query) ||
            year.includes(query) ||
            month.includes(query) ||
            routeId.includes(query) ||
            id.includes(query);
    });

    renderRoutes(filteredRoutes);
    updateRoutesCounter(filteredRoutes.length, routes.length);
}

function showRoutesLoadingState() {
    tableBody.innerHTML = `
        <tr>
            <td class="state-cell" colspan="${routesColumnCount}">
                <div class="loading-state">
                    <span class="spinner" aria-hidden="true"></span>
                    <span>Загрузка маршрутов…</span>
                </div>
            </td>
        </tr>`;
    recordsCounter.textContent = "Загрузка данных…";
}

function showRoutesErrorState(message) {
    tableBody.innerHTML = `
        <tr>
            <td class="state-cell" colspan="${routesColumnCount}">
                <div class="error-state">
                    <strong>Не удалось загрузить данные</strong>
                    <span>${message}</span>
                    <button type="button" id="retryButton">Повторить</button>
                </div>
            </td>
        </tr>`;
    recordsCounter.textContent = "Ошибка загрузки";
    document.getElementById("retryButton")
        ?.addEventListener("click", loadRoutes);
}



function getStationName(station) {
    if (!station) {
        return null;
    }
    if (typeof station === "string") {
        return station;
    }
    return station.name ??
        station.stationName ??
        station.title ??
        station.fullName ??
        null;
}

function formatStations(train) {
    const from = getStationName(train.stationFrom);
    const middle = getStationName(train.stationMiddle);
    const to = getStationName(train.stationTo);

    if (from && middle && to) {
        return `${from} — ${middle} — ${to}`;
    }
    if (from && to) {
        return `${from} — ${to}`;
    }
    if (from) {
        return `${from} — станция назначения не указана`;
    }
    if (to) {
        return `Станция отправления не указана — ${to}`;
    }
    return "Станции не указаны";
}

function formatTrainTimes(train) {
    const from = formatTime(train.timeFrom);
    const to = formatTime(train.timeTo);

    if (from === "—" && to === "—") {
        return "Время не указано";
    }
    return `${from} — ${to}`;
}

function createTrainRow(train) {
    const row = document.createElement("tr");
    row.append(
        createCell(train.number ?? "—", "train-number-cell"),
        createCell(formatStations(train), "train-stations-cell"),
        createCell(formatTrainTimes(train), "train-time-cell"),
        createCell(formatDecimal(train.distance), "numeric-cell"),
        createCell(formatInteger(train.railcarCount), "numeric-cell"),
        createCell(formatInteger(train.rangePerDay), "numeric-cell"),
        createCell(formatInteger(train.dayInRaise), "numeric-cell"),
        createCell(formatInteger(train.rangePerMonth), "numeric-cell"),
        createCell(train.description, "train-stations-cell")
    );
    return row;
}

function renderTrains(items) {
    trainsTableBody.replaceChildren();

    if (items.length === 0) {
        const row = document.createElement("tr");
        const cell = document.createElement("td");
        cell.colSpan = trainsColumnCount;
        cell.className = "trains-state-cell";
        cell.textContent = "По выбранному номеру поезда данные не найдены.";
        row.append(cell);
        trainsTableBody.append(row);
        return;
    }

    const fragment = document.createDocumentFragment();
    items.forEach((train) => {
        fragment.append(createTrainRow(train));
    });
    trainsTableBody.append(fragment);
}

function showTrainsLoadingState() {
    trainsTableBody.innerHTML = `
        <tr>
            <td class="trains-state-cell" colspan="${trainsColumnCount}">
                <div class="loading-state">
                    <span class="spinner" aria-hidden="true"></span>
                    <span>Загрузка данных по поездам…</span>
                </div>
            </td>
        </tr>`;
    trainsCounter.textContent = "Загрузка данных…";
}

function showTrainsErrorState(message) {
    trainsTableBody.innerHTML = `
        <tr>
            <td class="trains-state-cell" colspan="${trainsColumnCount}">
                <div class="error-state">
                    <strong>Не удалось загрузить данные по поездам</strong>
                    <span>${message}</span>
                </div>
            </td>
        </tr>`;
    trainsCounter.textContent = "Ошибка загрузки";
}

function updateTrainsCounter(count) {
    trainsCounter.textContent =
        `Найдено поездов: ${integerFormatter.format(count)}`;
}


function showNotification(message, type = "success") {
    clearTimeout(notificationTimer);
    notification.textContent = message;
    notification.classList.toggle("is-error", type === "error");
    notification.classList.add("is-visible");

    notificationTimer = window.setTimeout(() => {
        notification.classList.remove("is-visible");
    }, 3200);
}



async function loadTrains(route) {
    const requestId = ++selectedRouteRequestId;

    const year = getApiYear(route.year);
    const month = getApiMonth(route.month);
    const number = String(route.routeNumber ?? "");

    if (!year || !month || !number) {
        showTrainsErrorState("Не удалось сформировать параметры запроса.");
        return;
    }

    const requestUrl =
        `${trainsApiBaseUrl}/${encodeURIComponent(year)}/${encodeURIComponent(month)}/${encodeURIComponent(number)}`;

    showTrainsLoadingState();

    try {
        const response = await fetch(requestUrl, {
            method: "GET",
            headers: {
                Accept: "application/json"
            },
            credentials: "same-origin",
            cache: "no-store"
        });

        if (!response.ok) {
            throw new Error(`Сервер вернул ошибку ${response.status}.`);
        }

        const payload = await response.json();

        if (requestId !== selectedRouteRequestId) {
            return;
        }

        const trains = extractItems(payload);
        renderTrains(trains);
        updateTrainsCounter(trains.length);
    } catch (error) {
        if (requestId !== selectedRouteRequestId) {
            return;
        }
        console.error("Ошибка загрузки поездов:", error);
        const message = error instanceof Error
            ? error.message
            : "Неизвестная ошибка.";
        showTrainsErrorState(message);
        showNotification("Не удалось загрузить данные по поездам.", "error");
    }
}

function openDetailsPanel(route) {
    const routeNumber = route.routeNumber ?? "—";
    const period = `${formatMonth(route.month)}.${formatYear(route.year)}`;

    selectedRouteTitle.textContent = `Поезда маршрута ${routeNumber} за ${period}`;
    workspace.classList.add("is-split");
    loadTrains(route);
}

function closeDetailsPanel() {
    selectedRouteRequestId += 1;
    workspace.classList.remove("is-split");
    selectedRouteTitle.textContent = "Поезда не выбраны";
    trainsCounter.textContent = "Выберите номер поезда в верхней таблице";
    trainsTableBody.innerHTML = `
        <tr>
            <td class="trains-state-cell" colspan="${trainsColumnCount}">
                Выберите номер поезда в верхней таблице.
            </td>
        </tr>
    `;
}

async function loadRoutes() {
    showRoutesLoadingState();
    refreshButton.disabled = true;
    refreshButton.classList.add("is-loading");

    try {
        const response = await fetch(buildRoutesRequestUrl(), {
            method: "GET",
            headers: {
                Accept: "application/json"
            },
            credentials: "same-origin",
            cache: "no-store"
        });

        if (!response.ok) {
            throw new Error(`Сервер вернул ошибку ${response.status}.`);
        }

        const payload = await response.json();
        routes = sortRoutes(extractItems(payload));
        filterRoutes();
        showNotification(
            `Данные загружены. Записей: ${integerFormatter.format(routes.length)}`
        );
    } catch (error) {
        console.error("Ошибка загрузки маршрутов:", error);
        const message = error instanceof Error
            ? error.message
            : "Неизвестная ошибка.";
        showRoutesErrorState(message);
        showNotification("Не удалось загрузить маршруты.", "error");
    } finally {
        refreshButton.disabled = false;
        refreshButton.classList.remove("is-loading");
    }
}


searchInput.addEventListener("input", filterRoutes);
refreshButton.addEventListener("click", loadRoutes);
closeDetailsButton.addEventListener("click", closeDetailsPanel);

document.addEventListener("keydown", (event) => {
    if (event.key === "Escape" && workspace.classList.contains("is-split")) {
        closeDetailsPanel();
    }
});

initFilters();
loadRoutes();