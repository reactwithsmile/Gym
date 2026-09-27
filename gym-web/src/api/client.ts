const API_URL = import.meta.env.VITE_API_URL;

export function apiUrl(path: string): string {
  if (!API_URL) {
    throw new Error("VITE_API_URL is not configured.");
  }
  return `${API_URL}${path}`;
}
