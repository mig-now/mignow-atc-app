import { getAccessTokenForHub } from "@/api/token";
import { env } from "@/lib/env";

async function fetchBlob(url: string): Promise<Blob> {
  const token = await getAccessTokenForHub();
  const res = await fetch(`${env.VITE_API_URL}${url}`, {
    headers: token ? { Authorization: `Bearer ${token}` } : undefined,
  });
  if (!res.ok) throw new Error(`HTTP ${res.status}`);
  return res.blob();
}

/** Baixa um arquivo do back (com o token Bearer; o client gerado só entende JSON). `url` = caminho gerado (`getXxxUrl`). */
export async function downloadFile(url: string, fileName: string) {
  const blob = await fetchBlob(url);
  const href = URL.createObjectURL(blob);
  const a = document.createElement("a");
  a.href = href;
  a.download = fileName;
  document.body.appendChild(a);
  a.click();
  a.remove();
  setTimeout(() => URL.revokeObjectURL(href), 10_000);
}

/** Abre um HTML do back numa aba nova (o blob carrega o token, que um link simples não envia). */
export async function openHtml(url: string) {
  const blob = await fetchBlob(url);
  const href = URL.createObjectURL(new Blob([blob], { type: "text/html" }));
  window.open(href, "_blank", "noopener");
  setTimeout(() => URL.revokeObjectURL(href), 60_000);
}
