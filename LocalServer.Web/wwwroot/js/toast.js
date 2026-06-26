function show(container, message, className) {
  const toast = document.createElement("div");
  toast.className = `toast ${className}`;
  toast.textContent = message;

  container.appendChild(toast);

  setTimeout(() => {
    toast.remove();
  }, 3000);
}

export function createToast(container) {
  return {
    success(message) {
      show(container, message, "success");
    },
    error(message) {
      show(container, message, "error");
    },
  };
}
