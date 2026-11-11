import type { EvolutionPoint } from "@/api/generated/atc/model";

/** Gráficos SVG simples (sem biblioteca), cores só por tokens do tema. */
export type Series = { key: string; label: string; fill: string; stroke: string };

const PAD = { l: 44, r: 12, t: 12, b: 28 };
const W = 640;
const H = 240;

function niceMax(v: number) {
  if (v <= 5) return Math.max(1, v);
  const p = 10 ** Math.floor(Math.log10(v));
  return Math.ceil(v / p) * p;
}

function Axis({ max, fmt }: { max: number; fmt: (n: number) => string }) {
  const ticks = [0, 0.25, 0.5, 0.75, 1];
  return (
    <g className="text-muted-foreground" fontSize="10">
      {ticks.map((f) => {
        const y = H - PAD.b - f * (H - PAD.t - PAD.b);
        return (
          <g key={f}>
            <line x1={PAD.l} x2={W - PAD.r} y1={y} y2={y} className="stroke-border" strokeWidth={1} />
            <text x={PAD.l - 6} y={y + 3} textAnchor="end" fill="currentColor">
              {fmt(Math.round(max * f))}
            </text>
          </g>
        );
      })}
    </g>
  );
}

/** Barras empilhadas por ponto da evolução (findings por situação). */
export function StackedBars({
  points,
  series,
  label,
  fmt,
}: {
  points: EvolutionPoint[];
  series: Series[];
  label: string;
  fmt: (n: number) => string;
}) {
  const get = (p: EvolutionPoint, k: string) => Number((p as unknown as Record<string, number>)[k] ?? 0);
  const max = niceMax(Math.max(1, ...points.map((p) => series.reduce((s, x) => s + get(p, x.key), 0))));
  const slot = (W - PAD.l - PAD.r) / Math.max(1, points.length);
  const bw = Math.min(48, slot * 0.7);
  const scale = (H - PAD.t - PAD.b) / max;
  return (
    <svg role="img" aria-label={label} viewBox={`0 0 ${W} ${H}`} className="h-auto w-full">
      <Axis max={max} fmt={fmt} />
      {points.map((p, i) => {
        const x = PAD.l + slot * i + (slot - bw) / 2;
        let y = H - PAD.b;
        return (
          <g key={`${p.t}-${p.rot}`}>
            {series.map((s) => {
              const v = get(p, s.key);
              if (v <= 0) return null;
              y -= v * scale;
              return (
                <rect key={s.key} x={x} y={y} width={bw} height={v * scale} className={s.fill}>
                  <title>{`${p.rot} · ${s.label}: ${fmt(v)}`}</title>
                </rect>
              );
            })}
            <text x={x + bw / 2} y={H - 10} textAnchor="middle" fontSize="9" className="fill-muted-foreground">
              {i + 1}
            </text>
          </g>
        );
      })}
    </svg>
  );
}

/** Linhas (uma por série) ao longo dos pontos da evolução. */
export function LineChart({
  points,
  series,
  label,
  fmt,
}: {
  points: EvolutionPoint[];
  series: { key: string; label: string; stroke: string }[];
  label: string;
  fmt: (n: number) => string;
}) {
  const get = (p: EvolutionPoint, k: string) => Number((p as unknown as Record<string, number | null>)[k] ?? 0);
  const max = niceMax(Math.max(1, ...points.flatMap((p) => series.map((s) => get(p, s.key)))));
  const step = points.length > 1 ? (W - PAD.l - PAD.r) / (points.length - 1) : 0;
  const x = (i: number) => PAD.l + (points.length > 1 ? step * i : (W - PAD.l - PAD.r) / 2);
  const y = (v: number) => H - PAD.b - (v / max) * (H - PAD.t - PAD.b);
  return (
    <svg role="img" aria-label={label} viewBox={`0 0 ${W} ${H}`} className="h-auto w-full">
      <Axis max={max} fmt={fmt} />
      {series.map((s) => (
        <g key={s.key}>
          <polyline
            fill="none"
            strokeWidth={2}
            className={s.stroke}
            points={points.map((p, i) => `${x(i)},${y(get(p, s.key))}`).join(" ")}
          />
          {points.map((p, i) => (
            <circle
              key={`${p.t}-${p.rot}`}
              cx={x(i)}
              cy={y(get(p, s.key))}
              r={3}
              className={`${s.stroke} fill-card`}
              strokeWidth={2}
            >
              <title>{`${p.rot} · ${s.label}: ${fmt(get(p, s.key))}`}</title>
            </circle>
          ))}
        </g>
      ))}
      {points.map((p, i) => (
        <text
          key={`${p.t}-${p.rot}`}
          x={x(i)}
          y={H - 10}
          textAnchor="middle"
          fontSize="9"
          className="fill-muted-foreground"
        >
          {i + 1}
        </text>
      ))}
    </svg>
  );
}

export function Legend({ items }: { items: { key: string; label: string; swatch: string }[] }) {
  return (
    <ul className="flex flex-wrap gap-x-4 gap-y-1 text-xs text-muted-foreground">
      {items.map((i) => (
        <li key={i.key} className="flex items-center gap-1.5">
          <span aria-hidden className={`inline-block size-2.5 rounded-sm ${i.swatch}`} />
          {i.label}
        </li>
      ))}
    </ul>
  );
}
