import { defineConfig } from "vitest/config";

export default defineConfig({
  resolve: { tsconfigPaths: true },
  test: {
    environment: "jsdom",
    globals: true,
    setupFiles: ["./src/test/setup.ts"],
    env: {
      VITE_API_URL: "https://api.test.local",
      VITE_ENTRA_CLIENT_ID: "00000000-0000-0000-0000-000000000000",
      VITE_ENTRA_TENANT_ID: "00000000-0000-0000-0000-000000000000",
      VITE_ENTRA_API_SCOPE: "api://00000000-0000-0000-0000-000000000000/access_as_user",
    },
    coverage: {
      provider: "v8",
      reporter: ["text-summary"],
      include: ["src/**/*.{ts,tsx}"],
      exclude: ["src/api/generated/**", "src/test/**", "**/*.test.{ts,tsx}", "**/*.d.ts", "src/components/ui/**"],
      // PISO (não meta): trava regressão. Ajustar ao medir a suíte real.
      thresholds: { lines: 50, statements: 48, functions: 45, branches: 42 }, // medido em 26/10/2026: 55/53/50/47
    },
  },
});
