import { defineConfig } from "orval";

// Um único client (`atc`), lido de openapi/atc.json (gerado por `pnpm sync:api` a partir do backend rodando).
// O client gerado (src/api/generated) NÃO é versionado à mão: regenere com `pnpm gen:api`.
export default defineConfig({
  atc: {
    input: "./openapi/atc.json",
    output: {
      mode: "tags-split",
      target: "./src/api/generated/atc",
      schemas: "./src/api/generated/atc/model",
      client: "react-query",
      httpClient: "fetch",
      clean: true,
      override: {
        mutator: { path: "./src/api/client.ts", name: "apiFetch" },
        // O mutator já devolve o conteúdo do envelope (data); GET vira useQuery, o resto useMutation.
        fetch: { includeHttpResponseReturnType: false },
      },
    },
  },
});
