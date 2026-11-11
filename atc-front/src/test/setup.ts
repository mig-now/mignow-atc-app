import "@testing-library/jest-dom/vitest";
import { cleanup } from "@testing-library/react";
import { afterAll, afterEach, beforeAll } from "vitest";
import i18n from "@/i18n/zod-error-map"; // i18n + mensagens do Zod no idioma ativo, como no root
import { server } from "./msw/server";

// Polyfills de jsdom para Radix.
class ResizeObserverStub {
  observe() {}
  unobserve() {}
  disconnect() {}
}
globalThis.ResizeObserver ??= ResizeObserverStub as unknown as typeof ResizeObserver;
Element.prototype.scrollIntoView ??= () => {};
Element.prototype.hasPointerCapture ??= () => false;
Element.prototype.releasePointerCapture ??= () => {};

beforeAll(async () => {
  await i18n.changeLanguage("pt-BR");
  server.listen({ onUnhandledRequest: "error" }); // request sem handler falha o teste
});
afterEach(() => {
  server.resetHandlers();
  cleanup();
  sessionStorage.clear();
});
afterAll(() => server.close());
