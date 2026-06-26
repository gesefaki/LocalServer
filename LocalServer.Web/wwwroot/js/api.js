const API_URL = "http://192.168.0.158:5144/api/files";

// extracting common request logic
async function request(url, options = {}) {
  const response = await fetch(url, options);

  if (!response.ok) {
    throw new Error(`HTTP ${response.status}`);
  }

  return response;
}

export async function getFiles() {
  const response = await request(API_URL);
  return await response.json();
}

export async function uploadFile(file) {
  const formData = new FormData();

  formData.append("file", file);

  await request(API_URL, {
    method: "POST",
    body: formData,
  });
}

export async function deleteFile(id) {
  await request(`${API_URL}/${id}`, {
    method: "DELETE",
  });
}

export async function downloadFile(id) {
  const response = await request(`${API_URL}/${id}`);
  return await response.blob();
}
