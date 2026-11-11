import { useTranslation } from "react-i18next";
import type { SessionResponse } from "@/api/generated/atc/model";
import { Badge } from "@/components/ui/badge";
import { Card } from "@/components/ui/card";
import { Loading } from "@/components/ui/loading";
import { Table, TBody, Td, THead, Th, Tr } from "@/components/ui/table";
import { ApiErrorAlert } from "@/features/shared/api-error-alert";
import { EmptyState } from "@/features/shared/empty-state";
import { TriageScopeBadge, useTriageRules } from "@/features/triage";

const KNOWN_EFFECTS = ["Cancelado", "Confirmado KB", "Confirmado IA", "Isenção", "signal"];

/**
 * Aba Triagem: só exibe as regras que a triagem considera nesta sessão (ativas do cadastro que valem para o tipo de projeto), na ordem de
 * avaliação, com a condição do código. A triagem roda no play; não há aplicação manual nem conferência da R1 aqui.
 */
export function TriageTab({ session }: { session: SessionResponse }) {
  const { t } = useTranslation("remediation");
  const query = useTriageRules(session.projectType);
  const rules = [...(query.data ?? [])].sort((a, b) => a.order - b.order);
  return (
    <div className="space-y-3">
      <p className="text-sm text-muted-foreground">
        {t("triage.readOnly", { projectType: t(`projectType.${session.projectType}`) })}
      </p>
      <ApiErrorAlert error={query.error} />
      <Card>
        {query.isLoading ? (
          <Loading />
        ) : rules.length === 0 ? (
          <EmptyState text={t("triage.noRules")} />
        ) : (
          <Table>
            <THead>
              <Tr>
                <Th className="w-16">{t("triage.col.order")}</Th>
                <Th>{t("triage.col.rule")}</Th>
                <Th>{t("triage.col.condition")}</Th>
                <Th>{t("triage.col.effect")}</Th>
                <Th>{t("triage.col.scope")}</Th>
              </Tr>
            </THead>
            <TBody>
              {rules.map((r) => (
                <Tr key={r.code}>
                  <Td className="tabular-nums">{r.order}</Td>
                  <Td className="font-medium">{r.code}</Td>
                  <Td className="max-w-xl">
                    <p>{r.description}</p>
                    <p className="mt-1 text-muted-foreground">{r.condition}</p>
                  </Td>
                  <Td>
                    <Badge tone={r.effect === "signal" ? "info" : "warning"}>
                      {KNOWN_EFFECTS.includes(r.effect) ? t(`triage.effect.${r.effect}`) : r.effect}
                    </Badge>
                  </Td>
                  <Td>
                    <TriageScopeBadge scope={r.scope} />
                  </Td>
                </Tr>
              ))}
            </TBody>
          </Table>
        )}
      </Card>
    </div>
  );
}
