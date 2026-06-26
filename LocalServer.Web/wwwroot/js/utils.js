export function formatSize(bytes) {
  const units = ["B", "KB", "MB", "GB"];

  let index = 0;

  let size = bytes;

  while (size >= 1024 && index < units.length - 1) {
    size /= 1024;
    index++;
  }

  return `${size.toFixed(1)} ${units[index]}`;
}
