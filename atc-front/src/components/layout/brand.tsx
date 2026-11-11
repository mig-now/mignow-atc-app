/** Símbolo provisório (degradê da marca). Trocar pelo asset oficial em src/assets/brand quando houver. */
export function BrandSymbol({ className }: { className?: string }) {
  return (
    <svg viewBox="0 0 32 32" className={className} aria-hidden="true">
      <defs>
        <linearGradient id="migatc-grad" x1="0" y1="0" x2="1" y2="1">
          <stop offset="0%" stopColor="#42b6fe" />
          <stop offset="52%" stopColor="#7a74ff" />
          <stop offset="100%" stopColor="#a245ff" />
        </linearGradient>
      </defs>
      <rect width="32" height="32" rx="8" fill="url(#migatc-grad)" />
      <path
        d="M9 21V11l7 6 7-6v10"
        fill="none"
        stroke="white"
        strokeWidth="2.4"
        strokeLinecap="round"
        strokeLinejoin="round"
      />
    </svg>
  );
}
