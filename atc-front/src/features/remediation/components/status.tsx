import { useTranslation } from "react-i18next";
import type { SituationCounts } from "@/api/generated/atc/model";
import { Badge, type BadgeTone } from "@/components/ui/badge";

/** Situações do item, na ordem de exibição; o texto exato do status vem da skill (`""` = pendente). */
export const SITUATIONS = ["pending", "kb", "ai", "exemption", "cancelled"] as const;
export type Situation = (typeof SITUATIONS)[number];

const STATUS_TO_SITUATION: Record<string, Situation> = {
  "": "pending",
  "Confirmado KB": "kb",
  "Confirmado IA": "ai",
  Isenção: "exemption",
  Cancelado: "cancelled",
};

/** Valor aceito pelo filtro `status` de GET items para cada situação. */
export const STATUS_FILTER: Record<Situation, string> = {
  pending: "pending",
  kb: "confirmedKb",
  ai: "confirmedAi",
  exemption: "exemption",
  cancelled: "cancelled",
};

/**
 * Situações da aba Itens: "pendente" se divide em ainda não processado (`pending`) e já analisado pela IA, que devolveu ao consultor
 * pendente de aprovação (`approval`: já tem proposta esperando a decisão do consultor). O status gravado dos dois continua vazio
 * (layout da skill).
 */
export const ITEM_SITUATIONS = ["pending", "approval", "kb", "ai", "exemption", "cancelled"] as const;
export type ItemSituation = (typeof ITEM_SITUATIONS)[number];

/** Filtro `status` de GET items por situação da aba Itens. */
export const ITEM_FILTER: Record<ItemSituation, string> = {
  pending: "unprocessed",
  approval: "approval",
  kb: "confirmedKb",
  ai: "confirmedAi",
  exemption: "exemption",
  cancelled: "cancelled",
};

/** Filtro `status` de GET items para os itens cuja última tentativa no SAP falhou (qualquer situação). */
export const APPLY_ERROR_FILTER = "applyError";

export const TONE: Record<Situation, BadgeTone> = {
  pending: "neutral",
  kb: "success",
  ai: "info",
  exemption: "warning",
  cancelled: "danger",
};

const BAR: Record<Situation, string> = {
  pending: "bg-muted-foreground/30",
  kb: "bg-success",
  ai: "bg-info",
  exemption: "bg-warning",
  cancelled: "bg-destructive",
};

export function situationOf(status: string): Situation {
  return STATUS_TO_SITUATION[status] ?? "pending";
}

/**
 * `approval`: pendente de aprovação – a proposta espera a decisão do consultor (não confundir com "ainda não processado");
 * `suggestion`: o que a proposta recomenda (corrigir | isencao | cancelar | pendente), mostrado ao lado.
 */
export function ItemStatusBadge({
  status,
  approval = false,
  suggestion,
}: {
  status: string;
  approval?: boolean;
  suggestion?: string | null;
}) {
  const { t } = useTranslation("remediation");
  const s = situationOf(status);
  if (s === "pending" && approval)
    return (
      <span className="inline-flex flex-wrap items-center gap-1">
        <Badge tone="primary">{t("situation.approval")}</Badge>
        {suggestion && <Badge tone="neutral">{t(`suggestion.${suggestion}`, { defaultValue: suggestion })}</Badge>}
      </span>
    );
  return <Badge tone={TONE[s]}>{t(`situation.${s}`)}</Badge>;
}

/** Contagens por situação com barra empilhada. */
export function CountsBar({ counts }: { counts: SituationCounts }) {
  const { t, i18n } = useTranslation("remediation");
  const total = counts.total || 1;
  const errors = counts.applyErrors ?? 0;
  // "pendente" na barra = ainda não processado; os que esperam a decisão do consultor aparecem à parte ("Pendente de aprovação")
  const approval = Math.min(counts.awaitingApproval ?? 0, counts.pending);
  const value = (s: Situation) => (s === "pending" ? counts.pending - approval : counts[s]);
  const summary = [
    ...SITUATIONS.map((s) => `${t(`situation.${s}`)}: ${value(s)}`),
    ...(approval > 0 ? [`${t("situation.approval")}: ${approval}`] : []),
  ].join(", ");
  return (
    <div className="space-y-2">
      <div role="img" aria-label={summary} className="flex h-2.5 w-full overflow-hidden rounded-full bg-muted">
        {SITUATIONS.map((s) =>
          value(s) > 0 ? <div key={s} className={BAR[s]} style={{ width: `${(value(s) / total) * 100}%` }} /> : null,
        )}
        {approval > 0 && <div className="bg-primary" style={{ width: `${(approval / total) * 100}%` }} />}
      </div>
      <ul className="flex flex-wrap gap-x-4 gap-y-1 text-xs">
        {SITUATIONS.map((s) => (
          <li key={s} className="flex items-center gap-1.5">
            <span className={`size-2 rounded-full ${BAR[s]}`} aria-hidden />
            <span className="text-muted-foreground">{t(`situation.${s}`)}</span>
            <span className="font-medium">{new Intl.NumberFormat(i18n.language).format(value(s))}</span>
          </li>
        ))}
        {approval > 0 && (
          <li className="flex items-center gap-1.5">
            <span className="size-2 rounded-full bg-primary" aria-hidden />
            <span className="text-muted-foreground">{t("situation.approval")}</span>
            <span className="font-medium">{new Intl.NumberFormat(i18n.language).format(approval)}</span>
          </li>
        )}
        {errors > 0 && (
          <li className="flex items-center gap-1.5 text-destructive">
            <span className="size-2 rounded-full bg-destructive" aria-hidden />
            <span>{t("counts.applyErrors")}</span>
            <span className="font-medium">{new Intl.NumberFormat(i18n.language).format(errors)}</span>
          </li>
        )}
      </ul>
    </div>
  );
}

/** "1,2,3" -> "P1 + P2 + P3". */
export function formatPriorities(p: string) {
  return p
    .split(",")
    .map((x) => `P${x.trim()}`)
    .join(" + ");
}
