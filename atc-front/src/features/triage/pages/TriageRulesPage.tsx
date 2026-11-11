import { Pencil, Plus } from "lucide-react";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import type { ProjectType, TriageRuleDefinitionResponse } from "@/api/generated/atc/model";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import { Dialog, DialogContent } from "@/components/ui/dialog";
import { NativeSelect } from "@/components/ui/input";
import { Loading } from "@/components/ui/loading";
import { Table, TBody, Td, THead, Th, Tr } from "@/components/ui/table";
import { ApiErrorAlert } from "@/features/shared/api-error-alert";
import { DeleteButton } from "@/features/shared/delete-button";
import { EmptyState } from "@/features/shared/empty-state";
import { PageHeader } from "@/features/shared/page-header";
import { ActiveBadge } from "@/features/shared/status";
import { EFFECTS, TriageRuleForm } from "../components/TriageRuleForm";
import { TriageScopeBadge } from "../components/TriageScopeBadge";
import { useDeleteTriageRule, useTriageRules } from "../hooks/useTriageRules";

const PROJECT_TYPES: ProjectType[] = ["Conversion", "Upgrade"];

/**
 * `/regras-triagem`: cadastro das regras de triagem. Padrão (lógica no código): edita código, descrição, escopo, ativa, objetos da R7/R9,
 * parâmetros da lógica e filtro adicional.
 * Nova: critérios + efeito; só a nova se exclui. O play da remediação aplica as ativas do tipo de projeto da sessão.
 */
export function TriageRulesPage() {
  const { t } = useTranslation("triage");
  const [projectType, setProjectType] = useState<ProjectType | "">("");
  const query = useTriageRules(projectType || undefined);
  const remove = useDeleteTriageRule();
  const [editing, setEditing] = useState<TriageRuleDefinitionResponse | "new" | null>(null);
  return (
    <>
      <PageHeader
        title={t("title")}
        description={t("intro")}
        actions={
          <Button onClick={() => setEditing("new")}>
            <Plus />
            {t("new")}
          </Button>
        }
      />
      <div className="mb-3 flex flex-wrap items-end justify-end gap-4">
        <NativeSelect
          className="max-w-52"
          aria-label={t("filter")}
          value={projectType}
          onChange={(e) => setProjectType(e.target.value as ProjectType | "")}
        >
          <option value="">{t("allProjects")}</option>
          {PROJECT_TYPES.map((p) => (
            <option key={p} value={p}>
              {t(`projectType.${p}`)}
            </option>
          ))}
        </NativeSelect>
      </div>
      <ApiErrorAlert error={query.error ?? remove.error} />
      <Card>
        {query.isLoading ? (
          <Loading />
        ) : !query.data?.length ? (
          <EmptyState text={t("empty")} />
        ) : (
          <Table>
            <THead>
              <Tr>
                <Th className="w-16">{t("order")}</Th>
                <Th>{t("code")}</Th>
                <Th>{t("description")}</Th>
                <Th>{t("effect")}</Th>
                <Th>{t("scope")}</Th>
                <Th>{t("origin")}</Th>
                <Th>{t("common:common.status")}</Th>
                <Th className="w-24">
                  <span className="sr-only">{t("common:common.actions")}</span>
                </Th>
              </Tr>
            </THead>
            <TBody>
              {[...query.data]
                .sort((a, b) => a.order - b.order)
                .map((r) => (
                  <Tr key={r.code}>
                    <Td className="tabular-nums">{r.order}</Td>
                    <Td className="font-medium">
                      {r.code}
                      {r.key !== r.code && (
                        <p className="text-xs font-normal text-muted-foreground">{t("keyOf", { key: r.key })}</p>
                      )}
                    </Td>
                    <Td className="max-w-xl">
                      <p>{r.description}</p>
                      <p className="mt-1 text-muted-foreground">
                        <span className="font-medium text-foreground">{t("condition")}:</span> {r.condition}
                      </p>
                    </Td>
                    <Td>
                      <Badge tone={r.effect === "signal" ? "info" : "warning"}>
                        {EFFECTS.includes(r.effect as (typeof EFFECTS)[number]) ? t(`effect.${r.effect}`) : r.effect}
                      </Badge>
                    </Td>
                    <Td>
                      <TriageScopeBadge scope={r.scope} />
                    </Td>
                    <Td>
                      <Badge tone={r.builtIn ? "neutral" : "primary"}>{r.builtIn ? t("builtIn") : t("custom")}</Badge>
                    </Td>
                    <Td>
                      <ActiveBadge active={r.active} />
                    </Td>
                    <Td>
                      <Button
                        variant="ghost"
                        size="icon"
                        aria-label={t("common:actions.editItem", { name: r.code })}
                        onClick={() => setEditing(r)}
                      >
                        <Pencil />
                      </Button>
                      {!r.builtIn && <DeleteButton name={r.code} onDelete={() => remove.mutateAsync(r.code)} />}
                    </Td>
                  </Tr>
                ))}
            </TBody>
          </Table>
        )}
      </Card>
      <Dialog open={editing !== null} onOpenChange={(o) => !o && setEditing(null)}>
        {editing !== null && (
          <DialogContent title={editing === "new" ? t("new") : t("edit", { code: editing.code })}>
            <TriageRuleForm rule={editing === "new" ? undefined : editing} onDone={() => setEditing(null)} />
          </DialogContent>
        )}
      </Dialog>
    </>
  );
}
