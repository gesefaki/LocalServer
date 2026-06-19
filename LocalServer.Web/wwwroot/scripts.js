const API_URL = "http://192.168.0.158:5144/api/files";

const filesTable = document.getElementById("filesTable");
const tableBody = document.getElementById("tableBody");
const fileInputForm = document.getElementById("fileInputForm");

let currentFiles = [];

document.addEventListener("DOMContentLoaded", getFiles);
fileInputForm.addEventListener("submit", async function (event) {
  event.preventDefault();
  await uploadFile(event).then(() => getFiles());
});

async function getFiles() {
  tableBody.innerHTML = "";

  try {
    const response = await fetch(API_URL);
    if (!response.ok) throw new Error(`HTTP ERROR: ${response.status}`);
    const files = await response.json();

    renderTable(files);
  } catch (err) {
    console.error(`HTTP ERROR: ${err}`);
  }
}

function renderTable(files) {
  tableBody.innerHTML = files
    .map(
      (file) => `
    <tr>
      <td>${file.id}</td>
      <td>${file.fileName}</td>
      <td>${file.contentType}</td>
      <td>${file.size}</td>
      <td>${file.savePath}</td>
      <td>
        <button class="action-btn download" data-id="${file.id}" data-name=${file.fileName}>Download</button>
        <button class="action-btn delete" data-id="${file.id}">Delete</button>
      </td>
    </tr>
    `,
    )
    .join("");

  document.querySelectorAll(".download").forEach((btn) => {
    btn.addEventListener("click", function () {
      const id = this.dataset.id;
      const name = this.dataset.name;
      downloadFile(id, name);
    });
  });

  document.querySelectorAll(".delete").forEach((btn) => {
    btn.addEventListener("click", function () {
      const id = this.dataset.id;
      deleteFile(id);
    });
  });
}

async function uploadFile(event) {
  const fileInput = document.getElementById("fileInput");
  const file = fileInput.files[0];

  if (!file) {
    alert("dobav file bistro");
    return;
  }

  const formData = new FormData();
  formData.append("file", file);

  try {
    const response = await fetch(`${API_URL}/`, {
      method: "POST",
      body: formData,
    });

    if (!response.ok) throw new Error(`HTTP ERROR: ${response.status}`);
  } catch (err) {
    console.error(`CREATE ERROR: ${err}`);
  }
}

async function downloadFile(fileId, fileName) {
  try {
    const response = await fetch(`${API_URL}/${fileId}`);

    if (!response.ok) throw new Error(`HTTP ERROR: ${response.status}`);

    const blob = await response.blob();

    const url = window.URL.createObjectURL(blob);
    const link = document.createElement("a");
    link.href = url;
    link.download = fileName || "file";
    document.body.appendChild(link);
    link.click();

    document.body.removeChild(link);
    window.URL.revokeObjectURL(url);
  } catch (err) {
    console.error(`DOWNLOAD ERROR: ${err}`);
  }
}

async function deleteFile(fileId) {
  try {
    const response = await fetch(`${API_URL}/${fileId}`, {
      method: "DELETE",
    });

    if (!response.ok) throw new Error(`HTTP ERROR: ${response.status}`);

    await getFiles();
  } catch (err) {
    console.error(`DELETE ERROR: ${err}`);
  }
}
