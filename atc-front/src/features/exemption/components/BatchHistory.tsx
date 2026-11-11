import { useTranslation } from "react-i18next";
import { Link } from "react-router";
import type { RunResponse } from "@/api/generated/atc/model";
import { Card, CardHeader, CardTitle } from "@/components/ui/card";
import { Table, TBody, Td, THead, Th, Tr } from "@/components/ui/table";
import { EmptyState } from "@/features/shared/empty-state";
import { RunStatusBadge } from "@/features/shared/status";
import { formatDateTime } from "@/lib/format";
import { resultOf } from "../lib";
import { StatusBadge } from "./StatusBadge";

/** Histórico do lote: um run por linha, com o resumo `ResultJson.history` (totais por status depois do run). */
export function BatchHistory({ runs }: { runs: RunResponse[] }) {
  const { t, i18n } = useTranslation("exemption");
  return (
    <Card>
      <CardHeader>
        <CardTitle>{t("history.title")}</CardTitle>
      </CardHeader>
      {runs.length === 0 ? (
        <EmptyState text={t("history.empty")} />
      ) : (
        <Table>
          <THead>
            <Tr>
              <Th>{t("history.col.when")}</Th>
              <Th>{t("history.col.kind")}</Th>
              <Th>{t("history.col.status")}</Th>
              <Th>{t("history.col.by")}</Th>
              <Th>{t("history.col.totals")}</Th>
            </Tr>
          </THead>
          <TBody>
            {runs.map((r) => {
              const totals = Object.entries(resultOf(r)?.history?.totals ?? {});
              return (
                <Tr key={r.id}>
                  <Td>
                    <Link className="text-primary underline-offset-4 hover:underline" to={`/execucoes/${r.id}`}>
                      {formatDateTime(r.queuedAt, i18n.language)}
                    </Link>
                  </Td>
                  <Td>{t(`runs:kind.${r.kind}`)}</Td>
                  <Td>
                    <RunStatusBadge status={r.status} />
                  </Td>
                  <Td>{r.startedBy}</Td>
                  <Td>
                    <div className="flex flex-wrap gap-2">
                      {totals.map(([status, n]) => (
                        <span key={status} className="inline-flex items-center gap-1 text-sm">
                          <StatusBadge status={status} />
                          {n}
                        </span>
                      ))}
                    </div>
                  </Td>
                </Tr>
              );
            })}
          </TBody>
        </Table>
      )}
    </Card>
  );
}
