import { useTranslation } from "react-i18next";
import { Badge } from "@/components/ui/badge";
import { RESULT_TONE } from "../apply/labels";

/** Detalhes que são códigos do back (trava de escrita) viram texto; o resto (mensagem do SAP) aparece como veio. */
const DETAIL_CODES = new Set([
  "Sap.WriteBlockedProductionClient",
  "Sap.WriteBlockedProduction",
  "Sap.WriteBlockedCategoryUnknown",
]);

export function isApplyFailure(result?: string | null) {
  const tone = result ? RESULT_TONE[result] : undefined;
  return tone === "danger" || tone === "warning";
}

/** Texto do detalhe de uma tentativa (código da trava traduzido; mensagem do SAP como veio). */
export function useApplyDetail() {
  const { t } = useTranslation("remediation");
  return (detail?: string | null) => (detail && DETAIL_CODES.has(detail) ? t(`items.sap.detail.${detail}`) : detail);
}

/**
 * Resultado da última tentativa no SAP. "Confirmado" é só a decisão: sem isto, um item cuja gravação falhou
 * (lock, request, trava de produção) parecia resolvido. `full` = sem cortar o motivo (detalhe do item).
 */
export function ApplicationResult({
  result,
  detail,
  full = false,
}: {
  result?: string | null;
  detail?: string | null;
  full?: boolean;
}) {
  const { t } = useTranslation("remediation");
  const text = useApplyDetail()(detail);
  if (!result) return <span className="text-sm text-muted-foreground">{t("items.sap.none")}</span>;
  return (
    <div className="space-y-1">
      <Badge tone={RESULT_TONE[result] ?? "neutral"}>{t(`apply.result.${result}`, { defaultValue: result })}</Badge>
      {isApplyFailure(result) && text && (
        <p className={full ? "text-sm text-destructive" : "line-clamp-2 text-xs text-destructive"} title={text}>
          {text}
        </p>
      )}
    </div>
  );
}
