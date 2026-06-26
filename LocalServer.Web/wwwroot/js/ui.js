import { formatSize } from "./utils.js";

function createRow(file) {
  return `
    <tr>
        <td>${file.fileName}</td>
        <td>${file.contentType}</td>
        <td>${formatSize(file.size)}</td>
        <td>
            <button
                class="download"
                data-id="${file.id}"
                data-name="${file.fileName}">
            Download
            </button>

            <button
                class="delete"
                data-id="${file.id}">
            Delete
            </button>
        </td>
    </tr>
    `;
}

export function renderFiles(files, tableBody) {
  tableBody.innerHTML = files.map(createRow).join("");
}
