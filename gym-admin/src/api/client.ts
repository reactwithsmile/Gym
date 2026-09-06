const API_URL = import.meta.env.VITE_API_URL ?? "http://localhost:44357";
const TOKEN_KEY = "gym_admin_token";

export function apiUrl(path: string): string {
  return `${API_URL}${path}`;
}

// Use sessionStorage to keep tokens scoped to the browser session (more secure than localStorage)
export function getToken(): string | null {
  return sessionStorage.getItem(TOKEN_KEY);
}

export function setToken(token: string): void {
  sessionStorage.setItem(TOKEN_KEY, token);
}

export function clearToken(): void {
  sessionStorage.removeItem(TOKEN_KEY);
}

export async function apiFetch<T>(path: string, options: RequestInit = {}): Promise<T> {
  const headers = new Headers(options.headers);
  if (!headers.has("Content-Type") && options.body) {
    headers.set("Content-Type", "application/json");
  }

  const token = getToken();
  if (token) {
    headers.set("Authorization", `Bearer ${token}`);
  }

  const response = await fetch(apiUrl(path), { ...options, headers });

  if (!response.ok) {
    let message = "Request failed.";
    try {
      const payload = (await response.json()) as { message?: string };
      if (payload.message) {
        message = payload.message;
      }
    } catch {
      // keep default message
    }

    if (response.status === 401) {
      clearToken();
    }

    throw new Error(message);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}
