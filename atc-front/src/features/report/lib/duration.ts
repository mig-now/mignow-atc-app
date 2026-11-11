/** Segundos -> "1h 02min 03s" (só as partes necessárias). */
export function formatSeconds(total: number | null | undefined) {
  if (total == null) return "—";
  const s = Math.max(0, Math.round(total));
  const h = Math.floor(s / 3600);
  const m = Math.floor((s % 3600) / 60);
  const sec = s % 60;
  if (h) return `${h}h ${String(m).padStart(2, "0")}min ${String(sec).padStart(2, "0")}s`;
  if (m) return `${m}min ${String(sec).padStart(2, "0")}s`;
  return `${sec}s`;
}
