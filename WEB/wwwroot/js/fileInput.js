"use strict";

const fileInputApiUrl = `/api/v1/file/input`;
const addDescriptionApiUrl = `/api/v1/file/input/addDesc`;

const uploadForm = document.getElementById("uploadForm");
const yearInput = document.getElementById("yearInput");
const monthInput = document.getElementById("monthInput");

const fileInput = document.getElementById("fileInput");
const fileDropZone = document.getElementById("fileDropZone");
const fileZoneTitle = document.getElementById("fileZoneTitle");

const selectedFile = document.getElementById("selectedFile");
const selectedFileName = document.getElementById("selectedFileName");
const selectedFileSize = document.getElementById("selectedFileSize");
const removeFileButton = document.getElementById("removeFileButton");

const validationMessage = document.getElementById("validationMessage");
const uploadButton = document.getElementById("uploadButton");
const resetButton = document.getElementById("resetButton");

const serverResult = document.getElementById("serverResult");
const serverResultTitle = document.getElementById("serverResultTitle");
const serverResultMessage = document.getElementById("serverResultMessage");
const closeResultButton = document.getElementById("closeResultButton");

const descriptionModal = document.getElementById("descriptionModal");
const descriptionProgress = document.getElementById("descriptionProgress");
const descriptionTrainNumber = document.getElementById("descriptionTrainNumber");
const descriptionPeriod = document.getElementById("descriptionPeriod");
const descriptionStations = document.getElementById("descriptionStations");
const descriptionInput = document.getElementById("descriptionInput");
const descriptionModalError = document.getElementById("descriptionModalError");

const pasteDescriptionButton = document.getElementById("pasteDescriptionButton");
const submitDescriptionButton = document.getElementById("submitDescriptionButton");
const skipDescriptionButton = document.getElementById("skipDescriptionButton");
const skipDescriptionTextButton = document.getElementById("skipDescriptionTextButton");

let currentFile = null;
let isUploading = false;
let importedRecords = [];
let currentRecordIndex = 0;
let isDescriptionSending = false;

function setDefaultPeriod() {
    const currentDate = new Date();

    yearInput.value = String(currentDate.getFullYear() % 100);
    monthInput.value = String(currentDate.getMonth() + 1);
}

function isXlsxFile(file) {
    if (!(file instanceof File)) {
        return false;
    }

    return file.name.toLocaleLowerCase("ru").endsWith(".xlsx");
}

function formatFileSize(size) {
    if (!Number.isFinite(size) || size <= 0) {
        return "Размер файла не определён";
    }

    const units = ["Б", "КБ", "МБ", "ГБ"];
    const unitIndex = Math.min(
        Math.floor(Math.log(size) / Math.log(1024)),
        units.length - 1
    );

    const value = size / Math.pow(1024, unitIndex);

    return `${new Intl.NumberFormat("ru-RU", {
        maximumFractionDigits: unitIndex === 0 ? 0 : 2
    }).format(value)} ${units[unitIndex]}`;
}

function setSelectedFile(file) {
    if (!isXlsxFile(file)) {
        clearSelectedFile();
        fileDropZone.classList.add("is-invalid");
        showValidation("Можно выбрать только один файл с расширением .xlsx.");
        return;
    }

    currentFile = file;

    selectedFileName.textContent = file.name;
    selectedFileSize.textContent = formatFileSize(file.size);
    selectedFile.hidden = false;

    fileZoneTitle.textContent = "Файл выбран";
    fileDropZone.classList.remove("is-invalid");

    hideValidation();
    hideServerResult();
}

function clearSelectedFile() {
    currentFile = null;
    fileInput.value = "";

    selectedFile.hidden = true;
    selectedFileName.textContent = "";
    selectedFileSize.textContent = "";

    fileZoneTitle.textContent = "Перетащите XLSX-файл сюда";
    fileDropZone.classList.remove("is-invalid", "is-dragging");
}

function showValidation(message) {
    validationMessage.textContent = message;
    validationMessage.hidden = false;
}

function hideValidation() {
    validationMessage.textContent = "";
    validationMessage.hidden = true;

    yearInput.classList.remove("is-invalid");
    monthInput.classList.remove("is-invalid");
    fileDropZone.classList.remove("is-invalid");
}

function validateUploadForm() {
    hideValidation();

    const year = Number(yearInput.value);
    const month = Number(monthInput.value);

    if (
        yearInput.value.trim() === "" ||
        !Number.isInteger(year) ||
        year < 0 ||
        year > 99
    ) {
        yearInput.classList.add("is-invalid");
        yearInput.focus();
        showValidation("Укажите последние две цифры года: от 0 до 99.");
        return false;
    }

    if (!Number.isInteger(month) || month < 1 || month > 12) {
        monthInput.classList.add("is-invalid");
        monthInput.focus();
        showValidation("Выберите месяц загрузки.");
        return false;
    }

    if (!currentFile || !isXlsxFile(currentFile)) {
        fileDropZone.classList.add("is-invalid");
        fileDropZone.focus();
        showValidation("Выберите один файл формата XLSX.");
        return false;
    }

    return true;
}

function setUploadingState(uploading) {
    isUploading = uploading;

    uploadButton.disabled = uploading;
    resetButton.disabled = uploading;

    yearInput.disabled = uploading;
    monthInput.disabled = uploading;
    fileInput.disabled = uploading;
    removeFileButton.disabled = uploading;

    uploadButton.classList.toggle("is-loading", uploading);
    fileDropZone.setAttribute("aria-disabled", String(uploading));
}

function extractRecords(payload) {
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

function getServerErrorMessage(payload, fallbackMessage) {
    if (typeof payload === "string" && payload.trim()) {
        return payload.trim();
    }

    if (typeof payload?.message === "string") {
        return payload.message;
    }

    if (typeof payload?.detail === "string") {
        return payload.detail;
    }

    if (typeof payload?.title === "string") {
        return payload.title;
    }

    return fallbackMessage;
}

async function readResponse(response) {
    const contentType = response.headers.get("content-type") ?? "";

    if (contentType.includes("application/json")) {
        return await response.json();
    }

    return await response.text();
}

function showServerResult(type, message) {
    const isSuccess = type === "success";

    serverResult.classList.remove(
        "server-result--success",
        "server-result--error"
    );

    serverResult.classList.add(
        isSuccess ? "server-result--success" : "server-result--error"
    );

    serverResultTitle.textContent = isSuccess
        ? "Файл обработан"
        : "Ошибка обработки";

    serverResultMessage.textContent = message;
    serverResult.hidden = false;

    serverResult.scrollIntoView({
        behavior: "smooth",
        block: "nearest"
    });
}

function hideServerResult() {
    serverResult.hidden = true;
    serverResultMessage.textContent = "";

    serverResult.classList.remove(
        "server-result--success",
        "server-result--error"
    );
}

function formatStations(record) {
    const stations = [
        record.stationFrom,
        record.stationMiddle,
        record.stationTo
    ].filter((station) => typeof station === "string" && station.trim());

    return stations.length > 0
        ? stations.join(" — ")
        : "Станции не указаны";
}

function setDescriptionSendingState(sending) {
    isDescriptionSending = sending;

    descriptionInput.disabled = sending;
    pasteDescriptionButton.disabled = sending;
    submitDescriptionButton.disabled = sending;
    skipDescriptionButton.disabled = sending;
    skipDescriptionTextButton.disabled = sending;

    submitDescriptionButton.classList.toggle("is-loading", sending);
}

function showDescriptionError(message) {
    descriptionModalError.textContent = message;
    descriptionModalError.hidden = false;
}

function hideDescriptionError() {
    descriptionModalError.textContent = "";
    descriptionModalError.hidden = true;
}

function openCurrentDescriptionModal() {
    if (currentRecordIndex >= importedRecords.length) {
        closeDescriptionModal();
        showServerResult(
            "success",
            "Файл записан в базу данных. Описания для всех полученных записей обработаны."
        );
        return;
    }

    const record = importedRecords[currentRecordIndex];

    descriptionProgress.textContent =
        `Запись ${currentRecordIndex + 1} из ${importedRecords.length}`;

    descriptionTrainNumber.textContent = record.number || "—";
    descriptionPeriod.textContent = record.period || "—";
    descriptionStations.textContent = formatStations(record);

    descriptionInput.value = record.description ?? "";

    hideDescriptionError();
    setDescriptionSendingState(false);

    descriptionModal.hidden = false;
    document.body.classList.add("description-modal-open");

    window.setTimeout(() => {
        descriptionInput.focus();
    }, 50);
}

function closeDescriptionModal() {
    descriptionModal.hidden = true;
    document.body.classList.remove("description-modal-open");

    importedRecords = [];
    currentRecordIndex = 0;
    hideDescriptionError();
    setDescriptionSendingState(false);
}

function moveToNextRecord() {
    currentRecordIndex += 1;
    openCurrentDescriptionModal();
}

async function pasteFromClipboard() {
    hideDescriptionError();

    try {
        if (!navigator.clipboard?.readText) {
            throw new Error("Clipboard API недоступен в текущем браузере.");
        }

        const text = await navigator.clipboard.readText();

        descriptionInput.value = text;
        descriptionInput.focus();
    } catch (error) {
        console.error("Ошибка чтения буфера обмена:", error);

        showDescriptionError(
            "Не удалось прочитать текст из буфера обмена. Разрешите доступ к буферу или вставьте текст вручную."
        );
    }
}

async function sendDescription() {
    if (isDescriptionSending) {
        return;
    }

    const currentRecord = importedRecords[currentRecordIndex];

    if (!currentRecord || currentRecord.id === undefined || currentRecord.id === null) {
        showDescriptionError("Не удалось определить идентификатор записи.");
        return;
    }

    hideDescriptionError();
    setDescriptionSendingState(true);

    /*
     * В запрос передаётся полный исходный объект.
     * Изменяется только его поле description.
     */
    const recordToSend = {
        ...currentRecord,
        description: descriptionInput.value
    };

    try {
        const response = await fetch(
            `${addDescriptionApiUrl}/${encodeURIComponent(currentRecord.id)}`,
            {
                method: "PATCH",
                headers: {
                    Accept: "application/json, text/plain, */*",
                    "Content-Type": "application/json"
                },
                body: JSON.stringify(recordToSend)
            }
        );

        const responsePayload = await readResponse(response);

        if (!response.ok) {
            throw new Error(
                getServerErrorMessage(
                    responsePayload,
                    `Сервер вернул ошибку ${response.status}.`
                )
            );
        }

        importedRecords[currentRecordIndex] = recordToSend;
        moveToNextRecord();
    } catch (error) {
        console.error("Ошибка отправки описания:", error);

        const message = error instanceof Error
            ? error.message
            : "Не удалось сохранить описание.";

        showDescriptionError(message);
        setDescriptionSendingState(false);
    }
}

async function uploadFile() {
    if (isUploading || !validateUploadForm()) {
        return;
    }

    hideServerResult();
    setUploadingState(true);

    const year = String(Number(yearInput.value)).padStart(2, "0");
    const month = String(Number(monthInput.value));

    const requestUrl = new URL(fileInputApiUrl, window.location.origin);

    requestUrl.searchParams.set("year", year);
    requestUrl.searchParams.set("month", month);

    const formData = new FormData();

    /*
     * Имя multipart-поля соответствует параметру файла на endpoint.
     */
    formData.append("file", currentFile, currentFile.name);

    try {
        const response = await fetch(requestUrl.toString(), {
            method: "POST",
            headers: {
                Accept: "application/json, text/plain, */*"
            },
            body: formData
        });

        const responsePayload = await readResponse(response);

        if (!response.ok) {
            throw new Error(
                getServerErrorMessage(
                    responsePayload,
                    `Сервер вернул ошибку ${response.status}.`
                )
            );
        }

        importedRecords = extractRecords(responsePayload);
        currentRecordIndex = 0;

        clearSelectedFile();

        const recordsCount = importedRecords.length;

        showServerResult(
            "success",
            `Файл успешно записан в базу данных. Получено записей: ${recordsCount}.`
        );

        if (recordsCount > 0) {
            openCurrentDescriptionModal();
        }
    } catch (error) {
        console.error("Ошибка загрузки файла:", error);

        const message = error instanceof Error
            ? error.message
            : "Неизвестная сетевая ошибка.";

        showServerResult("error", `Не удалось отправить файл. ${message}`);
    } finally {
        setUploadingState(false);
    }
}

function skipCurrentDescription() {
    if (isDescriptionSending) {
        return;
    }

    moveToNextRecord();
}

uploadForm.addEventListener("submit", (event) => {
    event.preventDefault();
    uploadFile();
});

uploadForm.addEventListener("reset", () => {
    window.setTimeout(() => {
        clearSelectedFile();
        hideValidation();
        hideServerResult();
        setDefaultPeriod();
    }, 0);
});

fileInput.addEventListener("change", () => {
    const file = fileInput.files?.[0];

    if (file) {
        setSelectedFile(file);
    }
});

fileDropZone.addEventListener("click", () => {
    if (!isUploading) {
        fileInput.click();
    }
});

fileDropZone.addEventListener("keydown", (event) => {
    if (
        !isUploading &&
        (event.key === "Enter" || event.key === " ")
    ) {
        event.preventDefault();
        fileInput.click();
    }
});

fileDropZone.addEventListener("dragenter", (event) => {
    event.preventDefault();

    if (!isUploading) {
        fileDropZone.classList.add("is-dragging");
    }
});

fileDropZone.addEventListener("dragover", (event) => {
    event.preventDefault();

    if (!isUploading) {
        fileDropZone.classList.add("is-dragging");
    }
});

fileDropZone.addEventListener("dragleave", (event) => {
    if (!fileDropZone.contains(event.relatedTarget)) {
        fileDropZone.classList.remove("is-dragging");
    }
});

fileDropZone.addEventListener("drop", (event) => {
    event.preventDefault();
    fileDropZone.classList.remove("is-dragging");

    if (isUploading) {
        return;
    }

    const files = event.dataTransfer?.files;

    if (!files || files.length !== 1) {
        showValidation("Можно загрузить строго один XLSX-файл.");
        fileDropZone.classList.add("is-invalid");
        return;
    }

    setSelectedFile(files[0]);
});

removeFileButton.addEventListener("click", () => {
    if (!isUploading) {
        clearSelectedFile();
        hideValidation();
    }
});

closeResultButton.addEventListener("click", hideServerResult);
pasteDescriptionButton.addEventListener("click", pasteFromClipboard);
submitDescriptionButton.addEventListener("click", sendDescription);
skipDescriptionButton.addEventListener("click", skipCurrentDescription);
skipDescriptionTextButton.addEventListener("click", skipCurrentDescription);

descriptionInput.addEventListener("keydown", (event) => {
    if (event.ctrlKey && event.key === "Enter") {
        event.preventDefault();
        sendDescription();
    }
});

document.addEventListener("keydown", (event) => {
    if (
        event.key === "Escape" &&
        !descriptionModal.hidden &&
        !isDescriptionSending
    ) {
        skipCurrentDescription();
    }
});

setDefaultPeriod();