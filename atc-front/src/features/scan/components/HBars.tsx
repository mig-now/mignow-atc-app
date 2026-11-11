/** Barras horizontais simples (cores por token). */
export type Bar = {
  key: string;
  label: string;
  value: number;
  tone: "success" | "warning" | "info" | "destructive" | "primary";
};

const BG: Record<Bar["tone"], string> = {
  success: "bg-success",
  warning: "bg-warning",
  info: "bg-info",
  destructive: "bg-destructive",
  primary: "bg-primary",
};

export function HBars({ bars, label, format }: { bars: Bar[]; label: string; format: (n: number) => string }) {
  const max = Math.max(1, ...bars.map((b) => b.value));
  return (
    <ul aria-label={label} className="space-y-2">
      {bars.map((b) => (
        <li key={b.key} className="grid grid-cols-[minmax(0,14rem)_1fr_auto] items-center gap-3 text-sm">
          <span className="truncate" title={b.label}>
            {b.label}
          </span>
          <span className="h-3 overflow-hidden rounded-sm bg-muted">
            <span className={`block h-full ${BG[b.tone]}`} style={{ width: `${(b.value / max) * 100}%` }} />
          </span>
          <span className="tabular-nums">{format(b.value)}</span>
        </li>
      ))}
    </ul>
  );
}
