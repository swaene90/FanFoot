let token: string | null = null;

async function csrf() {
  if (token) return token;
  const response = await fetch("/api/auth/antiforgery", { credentials: "same-origin" });
  token = (await response.json()).token;
  return token!;
}

export async function api<T>(path: string, init: RequestInit = {}): Promise<T> {
  const headers = new Headers(init.headers);
  if (init.body) headers.set("Content-Type", "application/json");
  if (init.method && !["GET", "HEAD", "OPTIONS"].includes(init.method)) headers.set("X-CSRF-TOKEN", await csrf());
  const response = await fetch(`/api${path}`, { ...init, headers, credentials: "same-origin" });
  if (!response.ok) {
    const body = await response.json().catch(() => null);
    throw new Error(body?.error || (response.status === 401 ? "Please sign in." : "Request failed."));
  }
  return response.status === 204 ? undefined as T : response.json();
}
