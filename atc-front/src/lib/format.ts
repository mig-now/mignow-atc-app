/** Data/hora no idioma ativo. */
export function formatDateTime(value: string | null | undefined, lang: string) {
  if (!value) return "—";
  return new Intl.DateTimeFormat(lang, { dateStyle: "short", timeStyle: "short" }).format(new Date(value));
}

export function formatDate(value: string | null | undefined, lang: string) {
  if (!value) return "—";
  return new Intl.DateTimeFormat(lang, { dateStyle: "short" }).format(new Date(`${value}T00:00:00`));
}

export function formatNumber(value: number, lang: string) {
  return new Intl.NumberFormat(lang).format(value);
}

/** Abaixo de 1 centavo (ex.: embedding), 2 algarismos significativos em vez de arredondar para zero. */
export function formatMoney(value: number, lang: string) {
  const tiny = value > 0 && value < 0.01;
  return new Intl.NumberFormat(lang, {
    style: "currency",
    currency: "USD",
    ...(tiny ? { maximumSignificantDigits: 2 } : { maximumFractionDigits: 4 }),
  }).format(value);
}

export function formatPercent(value: number, lang: string) {
  return new Intl.NumberFormat(lang, { style: "percent", maximumFractionDigits: 0 }).format(value);
}
