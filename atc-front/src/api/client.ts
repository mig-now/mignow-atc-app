import { env } from "@/lib/env";

/** Erro tipado (ProblemDetails do back: title localizado, detail = código estável). */
export type ApiError = { status: number; title: string; code: string; errors?: Record<string, string[]> };

export function isApiError(e: unknown): e is ApiError {
  return typeof e === "object" && e !== null && "status" in e && "code" in e;
}

let getAccessToken: () => Promise<string | null> = async () => null;
export function setTokenProvider(fn: () => Promise<string | null>) {
  getAccessToken = fn;
}

let getLanguage: () => string = () => "pt-BR";
export function setLanguageProvider(fn: () => string) {
  getLanguage = fn;
}

function traceparent() {
  const hex = (n: number) =>
    Array.from(crypto.getRandomValues(new Uint8Array(n)), (b) => b.toString(16).padStart(2, "0")).join("");
  return `00-${hex(16)}-${hex(8)}-01`;
}

function camel(key: string) {
  return key.charAt(0).toLowerCase() + key.slice(1);
}

/**
 * Mutator único do Orval (`apiFetch`): token Bearer, Accept-Language, traceparent; desembrulha o envelope
 * `{ success, data, message }` e transforma ProblemDetails em `ApiError`. Use `unwrapped()`/`payload()` nos hooks.
 */
export async function apiFetch<T>(url: string, options: RequestInit = {}): Promise<T> {
  const token = await getAccessToken();
  const headers = new Headers(options.headers);
  headers.set("Accept", "application/json");
  headers.set("Accept-Language", getLanguage());
  headers.set("traceparent", traceparent());
  if (token) headers.set("Authorization", `Bearer ${token}`); // NUNCA logar o token
  const res = await fetch(`${env.VITE_API_URL}${url}`, { ...options, headers });
  const body: unknown = res.status === 204 ? null : await res.json().catch(() => null);
  if (!res.ok) {
    const b = body as { title?: string; detail?: string; errors?: Record<string, string[]> } | null;
    const errors = b?.errors ? Object.fromEntries(Object.entries(b.errors).map(([k, v]) => [camel(k), v])) : undefined;
    throw {
      status: res.status,
      title: b?.title ?? "Erro inesperado",
      code: b?.detail ?? "Unknown",
      errors,
    } satisfies ApiError;
  }
  const envelope = body as { data?: unknown } | null;
  return (envelope && typeof envelope === "object" && "data" in envelope ? envelope.data : body) as T;
}
