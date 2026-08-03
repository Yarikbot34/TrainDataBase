"use strict";

const apiUrl = "http://localhost:5286/api/v1/TableView/routes";
const columnCount = 22;

const workspace = document.getElementById("workspace");
const tableBody = document.getElementById("routesTableBody");
const recordsCounter = document.getElementById("recordsCounter");
const searchInput = document.getElementById("searchInput");
const refreshButton = document.getElementById("refreshButton");
const closeDetailsButton = document.getElementById("closeDetailsButton");
const selectedRouteTitle = document.getElementById("selectedRouteTitle");
const selectedRouteNumber = document.getElementById("selectedRouteNumber");
const selectedRoutePeriod = document.getElementById("selectedRoutePeriod");
const notification = document.getElementById("notification");

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

function formatMonth(month) {
    return String(toNumber(month)).padStart(2, "0");
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

function getCategory(route, categoryName) {
    const category = route?.[categoryName];

    return {
        count: toNumber(category?.count),
        payment: toNumber(category?.payment),
        wayLength: toNumber(category?.wayLength),
        paymentBySubject: toNumber(category?.paymentBySubject)
    };
}

function extractRoutes(payload) {
    if (Array.isArray(payload)) {
        return payload;
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
    button.title = "Открыть подробные данные по маршруту";

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

        cell.colSpan = columnCount;
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

function updateCounter(visibleCount, totalCount = routes.length) {
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
        updateCounter(routes.length);
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
    updateCounter(filteredRoutes.length, routes.length);
}

function showLoadingState() {
    tableBody.innerHTML = `
        <tr>
            <td class="state-cell" colspan="${columnCount}">
                <div class="loading-state">
                    <span class="spinner" aria-hidden="true"></span>
                    <span>Загрузка маршрутов…</span>
                </div>
            </td>
        </tr>
    `;

    recordsCounter.textContent = "Загрузка данных…";
}

function showErrorState(message) {
    tableBody.innerHTML = `
        <tr>
            <td class="state-cell" colspan="${columnCount}">
                <div class="error-state">
                    <strong>Не удалось загрузить данные</strong>
                    <span>${message}</span>
                    <button type="button" id="retryButton">Повторить</button>
                </div>
            </td>
        </tr>
    `;

    recordsCounter.textContent = "Ошибка загрузки";

    document.getElementById("retryButton")
        ?.addEventListener("click", loadRoutes);
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

function openDetailsPanel(route) {
    const routeNumber = route.routeNumber ?? "—";
    const period = `${formatMonth(route.month)}.${formatYear(route.year)}`;

    selectedRouteTitle.textContent = `Поезд № ${routeNumber}`;
    selectedRouteNumber.textContent = routeNumber;
    selectedRoutePeriod.textContent = period;

    workspace.classList.add("is-split");
}

function closeDetailsPanel() {
    workspace.classList.remove("is-split");

    selectedRouteTitle.textContent = "Маршрут не выбран";
    selectedRouteNumber.textContent = "—";
    selectedRoutePeriod.textContent = "—";
}

async function loadRoutes() {
    showLoadingState();

    refreshButton.disabled = true;
    refreshButton.classList.add("is-loading");

    try {
        const response = await fetch(apiUrl, {
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

        routes = sortRoutes(extractRoutes(payload));

        filterRoutes();

        showNotification(
            `Данные загружены. Записей: ${integerFormatter.format(routes.length)}`
        );
    } catch (error) {
        console.error("Ошибка загрузки маршрутов:", error);

        const message = error instanceof Error
            ? error.message
            : "Неизвестная ошибка.";

        showErrorState(message);
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

loadRoutes();