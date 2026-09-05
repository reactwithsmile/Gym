const API_URL = import.meta.env.VITE_API_URL ?? "http://localhost:5182";

export function apiUrl(path: string): string {
  return `${API_URL}${path}`;
}
