import { Download } from "lucide-react";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import { Alert } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Field } from "@/components/ui/field";
import { NativeSelect } from "@/components/ui/input";
import { Table, TBody, Td, THead, Th, Tr } from "@/components/ui/table";
import { ApiErrorAlert } from "@/features/shared/api-error-alert";
import { useExportCsv, useReuse, useSessionList } from "../hooks/useSessions";

/** Aba Exportar: baixa o atc_status.csv e reaproveita fichas de outra sessão do mesmo ambiente. */
export function ExportTab({ sessionId, sapSystemId }: { sessionId: string; sapSystemId: string }) {
  const { t } = useTranslation("remediation");
  const exportCsv = useExportCsv(sessionId);
  const reuse = useReuse(sessionId);
  const others = useSessionList({ sapSystemId, page: 1, pageSize: 100 });
  const [from, setFrom] = useState("");
  const candidates = (others.data?.items ?? []).filter((s) => s.id !== sessionId);
  const result = reuse.data;

  return (
    <div className="space-y-4">
      <Card>
        <CardHeader>
          <CardTitle>{t("export.title")}</CardTitle>
        </CardHeader>
        <CardContent className="space-y-3">
          <p className="text-sm text-muted-foreground">{t("export.description")}</p>
          <ApiErrorAlert error={exportCsv.error} />
          <Button disabled={exportCsv.isPending} onClick={() => exportCsv.mutate()}>
            <Download />
            {t("export.download")}
          </Button>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>{t("reuse.title")}</CardTitle>
        </CardHeader>
        <CardContent className="space-y-3">
          <p className="text-sm text-muted-foreground">{t("reuse.description")}</p>
          <Field id="reuse-from" label={t("reuse.from")}>
            <NativeSelect id="reuse-from" className="max-w-md" value={from} onChange={(e) => setFrom(e.target.value)}>
              <option value="">{t("reuse.choose")}</option>
              {candidates.map((s) => (
                <option key={s.id} value={s.id}>
                  {t("reuse.option", { cycle: s.cycle, request: s.request ?? "—", total: s.counts.total })}
                </option>
              ))}
            </NativeSelect>
          </Field>
          {!others.isLoading && candidates.length === 0 && <Alert>{t("reuse.noOther")}</Alert>}
          <ApiErrorAlert error={reuse.error} />
          <Button disabled={!from || reuse.isPending} onClick={() => reuse.mutate({ fromSessionId: from })}>
            {t("reuse.run")}
          </Button>
          {result && (
            <div className="space-y-3">
              <ul className="flex flex-wrap gap-x-4 gap-y-1 text-sm">
                {Object.entries(result.summary).map(([k, v]) => (
                  <li key={k}>
                    <span className="text-muted-foreground">{k}</span> <span className="font-medium">{v}</span>
                  </li>
                ))}
              </ul>
              {result.items.length === 0 ? (
                <p className="text-sm text-muted-foreground">{t("reuse.empty")}</p>
              ) : (
                <Table>
                  <THead>
                    <Tr>
                      <Th>{t("reuse.col.n")}</Th>
                      <Th>{t("reuse.col.object")}</Th>
                      <Th>{t("reuse.col.situation")}</Th>
                      <Th>{t("reuse.col.origin")}</Th>
                      <Th>{t("reuse.col.lines")}</Th>
                    </Tr>
                  </THead>
                  <TBody>
                    {result.items.map((c) => (
                      <Tr key={c.n}>
                        <Td>{c.n}</Td>
                        <Td>
                          {c.objectName} <span className="text-xs text-muted-foreground">{c.ref}</span>
                        </Td>
                        <Td>{c.situation}</Td>
                        <Td>
                          {c.originN != null ? `#${c.originN}` : "—"}
                          {c.originStatus ? ` · ${c.originStatus}` : ""}
                        </Td>
                        <Td>
                          {c.linesBefore != null || c.linesAfter != null
                            ? `${c.linesBefore ?? 0} → ${c.linesAfter ?? 0}`
                            : "—"}
                        </Td>
                      </Tr>
                    ))}
                  </TBody>
                </Table>
              )}
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
