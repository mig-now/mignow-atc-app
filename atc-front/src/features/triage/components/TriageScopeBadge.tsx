import { useTranslation } from "react-i18next";
import type { TriageScope } from "@/api/generated/atc/model";
import { Badge } from "@/components/ui/badge";

/** Escopo da regra de triagem: todos os projetos ou específica de upgrade/conversão. */
export function TriageScopeBadge({ scope }: { scope: TriageScope }) {
  const { t } = useTranslation("triage");
  return <Badge tone={scope === "All" ? "neutral" : "info"}>{t(`scope.${scope}`)}</Badge>;
}
