import { HttpResponse, type RequestHandler } from "msw";

/** Mesmo valor de VITE_API_URL no vitest.config.ts. */
export const API = "https://api.test.local";

/** Envelope de sucesso igual ao do backend: { success, data, message }. */
export const ok = <T>(data: T, init?: ResponseInit) => HttpResponse.json({ success: true, data, message: null }, init);

export const problem = (status: number, title: string, code: string, errors?: Record<string, string[]>) =>
  HttpResponse.json({ status, title, detail: code, errors }, { status });

export const paged = <T>(items: T[]) => ({ items, page: 1, pageSize: 20, total: items.length });

/** Linha de base (vazia): cada teste acrescenta o que precisa com server.use(...). Request sem handler falha o teste. */
export const baselineHandlers: RequestHandler[] = [];
