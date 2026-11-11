import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import type { ScanResponse } from "@/api/generated/atc/model";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input, NativeSelect } from "@/components/ui/input";
import { Loading } from "@/components/ui/loading";
import { Table, TBody, Td, THead, Th, Tr } from "@/components/ui/table";
import { ApiErrorAlert } from "@/features/shared/api-error-alert";
import { EmptyState } from "@/features/shared/empty-state";
import { Pagination } from "@/features/shared/pagination";
import { useSearchState } from "@/lib/use-search-state";
import { OCCURRENCES_PAGE_SIZE, useOccurrences } from "../hooks/useScans";
import { CROSS_CLASSES } from "./ScanCrossCard";

const DEFAULTS = { rule: "", cross: "", status: "", object: "", page: "1" };
const STATUSES = [
  { value: "P", key: "pendente" },
  { value: "R", key: "ja_remediado" },
  { value: "I", key: "informativo" },
];

/** Ocorrências paginadas (filtros na URL: regra, cruzamento, situação, objeto) com o trecho do código. */
export function ScanOccurrencesCard({ scan }: { scan: ScanResponse }) {
  const { t } = useTranslation("scan");
  const [f, set] = useSearchState(DEFAULTS);
  const page = Math.max(1, Number(f.page) || 1);
  const [objectDraft, setObjectDraft] = useState(f.object);
  useEffect(() => setObjectDraft(f.object), [f.object]);
  const q = useOccurrences(scan.id, { ruleId: f.rule, cross: f.cross, status: f.status, objectName: f.object }, page);

  return (
    <Card>
      <CardHeader>
        <CardTitle>{t("occ.title")}</CardTitle>
      </CardHeader>
      <CardContent className="space-y-3">
        <form
          className="flex flex-wrap gap-2"
          onSubmit={(e) => {
            e.preventDefault();
            set({ object: objectDraft.trim() });
          }}
        >
          <NativeSelect
            className="w-44"
            aria-label={t("occ.rule")}
            value={f.rule}
            onChange={(e) => set({ rule: e.target.value })}
          >
            <option value="">{t("occ.allRules")}</option>
            {scan.activeRuleIds.map((id) => (
              <option key={id} value={id}>
                {id}
              </option>
            ))}
          </NativeSelect>
          <NativeSelect
            className="w-64"
            aria-label={t("occ.cross")}
            value={f.cross}
            onChange={(e) => set({ cross: e.target.value })}
          >
            <option value="">{t("occ.allCross")}</option>
            {CROSS_CLASSES.map((c) => (
              <option key={c.key} value={c.key}>
                {t(`cross.${c.key}`)}
              </option>
            ))}
          </NativeSelect>
          <NativeSelect
            className="w-44"
            aria-label={t("occ.status")}
            value={f.status}
            onChange={(e) => set({ status: e.target.value })}
          >
            <option value="">{t("occ.allStatus")}</option>
            {STATUSES.map((s) => (
              <option key={s.value} value={s.value}>
                {t(`status.${s.key}`)}
              </option>
            ))}
          </NativeSelect>
          <Input
            className="w-56"
            aria-label={t("occ.object")}
            placeholder={t("occ.objectPlaceholder")}
            value={objectDraft}
            onChange={(e) => setObjectDraft(e.target.value)}
          />
        </form>
        {q.isLoading ? (
          <Loading />
        ) : q.error ? (
          <ApiErrorAlert error={q.error} />
        ) : !q.data || q.data.items.length === 0 ? (
          <EmptyState text={t("occ.empty")} />
        ) : (
          <div className="overflow-x-auto rounded-md border border-border">
            <Table>
              <THead>
                <Tr>
                  <Th>{t("occ.rule")}</Th>
                  <Th>{t("occ.objectCol")}</Th>
                  <Th>{t("occ.line")}</Th>
                  <Th>{t("occ.element")}</Th>
                  <Th>{t("occ.status")}</Th>
                  <Th>{t("occ.cross")}</Th>
                  <Th>{t("occ.snippet")}</Th>
                </Tr>
              </THead>
              <TBody>
                {q.data.items.map((o) => (
                  <Tr key={o.id}>
                    <Td className="font-mono">{o.ruleId}</Td>
                    <Td>
                      <span className="font-mono">{o.objectName}</span>
                      <span className="ml-1 text-xs text-muted-foreground">{o.objectType}</span>
                      {o.include ? <div className="text-xs text-muted-foreground">{o.include}</div> : null}
                    </Td>
                    <Td className="tabular-nums">
                      {o.lineEnd && o.lineEnd !== o.line ? `${o.line}–${o.lineEnd}` : o.line}
                    </Td>
                    <Td className="font-mono">{o.element}</Td>
                    <Td>
                      <Badge>{t(`status.${o.status}`, { defaultValue: o.status })}</Badge>
                    </Td>
                    <Td>{o.cross ? t(`cross.${o.cross}`, { defaultValue: o.cross }) : "—"}</Td>
                    <Td className="max-w-md">
                      {o.snippet ? (
                        <pre className="max-h-32 overflow-auto rounded-sm bg-muted p-2 font-mono text-xs whitespace-pre-wrap">
                          {o.snippet}
                        </pre>
                      ) : (
                        "—"
                      )}
                    </Td>
                  </Tr>
                ))}
              </TBody>
            </Table>
            <Pagination
              page={page}
              pageSize={OCCURRENCES_PAGE_SIZE}
              total={q.data.total}
              onPage={(p) => set({ page: String(p) })}
            />
          </div>
        )}
      </CardContent>
    </Card>
  );
}
