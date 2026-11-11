import { reactRouter } from "@react-router/dev/vite";
import tailwindcss from "@tailwindcss/vite";
import { defineConfig } from "vite";

// Paths do tsconfig ("@/…") resolvidos nativamente pelo Vite 8 (sem plugin).
export default defineConfig({ plugins: [tailwindcss(), reactRouter()], resolve: { tsconfigPaths: true } });
