export function getDOM() {
  return {
    tableBody: document.getElementById("tableBody"),
    fileInput: document.getElementById("fileInput"),
    form: document.getElementById("fileInputForm"),
    selectedFile: document.getElementById("selectedFile"),
    submitBtn: document.getElementById("submitBtn"),
    toastContainer: document.getElementById("toastContainer"),
  };
}
