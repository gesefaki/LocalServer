import { formatSize } from "./utils.js";

function createRow(file) {
  return `
    <tr>
        <td>${file.fileName}</td>
        <td>${file.contentType}</td>
        <td>${formatSize(file.size)}</td>
        <td>
            <button
                class="action-btn download"
                data-id="${file.id}"
                data-name="${file.fileName}">
            <i class="fi fi-rr-download"></i>
            </button>

            <button
                class="action-btn delete"
                data-id="${file.id}">
            <i class="fi fi-rr-trash"></i>
            </button>
        </td>
    </tr>
    `;
}

export function renderFiles(files, tableBody) {
  console.log(typeof files);
  if (!files || files.length === 0) {
    tableBody.innerHTML = `
      <tr>
        <td colspan="4" style="text-align: center; padding: 20px;">
          No uploaded files
        </td>
      </tr>
    `;
    return;
  }
  tableBody.innerHTML = files.map(createRow).join("");
}
