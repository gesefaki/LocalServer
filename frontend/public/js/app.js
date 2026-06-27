import * as api from "./api.js";
import { getDOM } from "./dom.js";
import { createToast } from "./toast.js";
import { renderFiles } from "./ui.js";

const DOM = getDOM();
const toast = createToast(DOM.toastContainer);

async function loadFiles() {
  try {
    const files = await api.getFiles();
    renderFiles(files, DOM.tableBody);
  } catch (err) {
    console.error(err);
    toast.error("Cannot load files");
  }
}

async function handleUpload(event) {
  try {
    event.preventDefault();

    DOM.submitBtn.disabled = true;
    DOM.submitBtn.textContent = "Uploading...";

    const file = DOM.fileInput.files[0];

    if (!file) {
      toast.error("Choose file");
      return;
    }

    await api.uploadFile(file);

    DOM.form.reset();

    DOM.selectedFile.textContent = "No file selected";

    await loadFiles();
  } catch (err) {
    console.error(err);
    toast.error("Cannot upload file.");
  } finally {
    DOM.submitBtn.disabled = false;
    DOM.submitBtn.textContent = "Upload";
  }
}

async function handleDownload(id, fileName) {
  try {
    const blob = await api.downloadFile(id);
    const url = URL.createObjectURL(blob);
    const link = document.createElement("a");

    link.href = url;
    link.download = fileName;
    document.body.appendChild(link);

    link.click();
    link.remove();

    URL.revokeObjectURL(url);
  } catch (err) {
    console.error(err);
    toast.error("Cannot download file.");
  }
}

async function handleDelete(id) {
  try {
    await api.deleteFile(id);
    await loadFiles();
  } catch (err) {
    console.error(err);
    toast.error("Cannot delete file.");
  }
}

async function handleTableClick(event) {
  const button = event.target.closest("button");
  if (!button) {
    return;
  }

  if (button.classList.contains("download")) {
    await handleDownload(button.dataset.id, button.dataset.name);
  }
  if (button.classList.contains("delete")) {
    await handleDelete(button.dataset.id);
  }
}

function handleFileSelection() {
  try {
    const file = DOM.fileInput.files[0];

    DOM.selectedFile.textContent = file ? file.name : "No file selected";
  } catch (err) {
    console.error(err);
    toast.error("Error");
  }
}

export async function initializeApp() {
  DOM.form.addEventListener("submit", handleUpload);

  DOM.tableBody.addEventListener("click", handleTableClick);

  DOM.fileInput.addEventListener("change", handleFileSelection);

  await loadFiles();
}
