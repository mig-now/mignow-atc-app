// Baixa o swagger.json do atc-back rodando localmente (dotnet run --launch-profile https, porta 7091) para openapi/atc.json.
// Depois: orval regenera src/api/generated/atc. Commite openapi/ junto com o client gerado.
import { mkdir, writeFile } from "node:fs/promises";

process.env.NODE_TLS_REJECT_UNAUTHORIZED = "0"; // certificado de desenvolvimento do dotnet
const base = process.env.VITE_API_URL ?? "https://localhost:7091";
const res = await fetch(`${base}/swagger/v1/swagger.json`);
if (!res.ok) throw new Error(`atc: HTTP ${res.status}`);
await mkdir("openapi", { recursive: true });
await writeFile("openapi/atc.json", JSON.stringify(await res.json(), null, 2));
console.log("openapi/atc.json atualizado");
