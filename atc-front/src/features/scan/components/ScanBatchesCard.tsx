import { useTranslation } from "react-i18next";
import type { ScanResponse } from "@/api/generated/atc/model";
import { Alert } from "@/components/ui/alert";
import { Badge, type BadgeTone } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Table, TBody, Td, THead, Th, Tr } from "@/components/ui/table";
import { EmptyState } from "@/features/shared/empty-state";
import { formatNumber } from "@/lib/format";

function statusTone(status: string | null | undefined): BadgeTone {
  if (!status) return "neutral";
  return status === "OK" ? "success" : "danger";
}

/** Classes temporárias que ficaram no SAP: lote executado (status) sem a classe removida. */
export function residueClasses(scan: ScanResponse) {
  return scan.batches.filter((b) => b.status != null && b.className !== "" && !b.classRemoved);
}

/** Lotes: peso, objetos, status e se a classe temporária foi removida; alerta forte para resíduos. */
export function ScanBatchesCard({
  scan,
  locked,
  onCleanup,
  cleaning,
}: {
  scan: ScanResponse;
  locked: boolean;
  onCleanup: () => void;
  cleaning: boolean;
}) {
  const { t, i18n } = useTranslation("scan");
  const lang = i18n.language;
  const residue = residueClasses(scan);
  return (
    <Card>
      <CardHeader>
        <CardTitle>{t("batches.title")}</CardTitle>
      </CardHeader>
      <CardContent className="space-y-3">
        {residue.length > 0 ? (
          <Alert tone="danger" title={t("batches.residueTitle", { n: residue.length })}>
            <p>{t("batches.residueText", { classes: residue.map((b) => b.className).join(", ") })}</p>
            <Button className="mt-2" size="sm" variant="destructive" disabled={locked || cleaning} onClick={onCleanup}>
              {t("batches.cleanup")}
            </Button>
          </Alert>
        ) : null}
        {scan.batches.length === 0 ? (
          <EmptyState text={t("batches.empty")} />
        ) : (
          <div className="overflow-x-auto rounded-md border border-border">
            <Table>
              <THead>
                <Tr>
                  <Th>{t("batches.number")}</Th>
                  <Th className="text-right">{t("batches.weight")}</Th>
                  <Th className="text-right">{t("batches.objects")}</Th>
                  <Th>{t("batches.status")}</Th>
                  <Th className="text-right">{t("batches.occurrences")}</Th>
                  <Th>{t("batches.class")}</Th>
                  <Th>{t("batches.removed")}</Th>
                </Tr>
              </THead>
              <TBody>
                {scan.batches.map((b) => (
                  <Tr key={b.number}>
                    <Td>{b.number}</Td>
                    <Td className="text-right">{formatNumber(b.weight, lang)}</Td>
                    <Td className="text-right">{formatNumber(b.objectCount, lang)}</Td>
                    <Td>
                      <Badge tone={statusTone(b.status)}>{b.status ?? t("batches.notRun")}</Badge>
                    </Td>
                    <Td className="text-right">{b.stats ? formatNumber(b.stats.occurrences, lang) : "—"}</Td>
                    <Td className="font-mono">{b.className || "—"}</Td>
                    <Td>
                      {b.status == null ? (
                        "—"
                      ) : b.classRemoved ? (
                        <Badge tone="success">{t("batches.removedYes")}</Badge>
                      ) : (
                        <Badge tone="danger">{t("batches.removedNo")}</Badge>
                      )}
                    </Td>
                  </Tr>
                ))}
              </TBody>
            </Table>
          </div>
        )}
      </CardContent>
    </Card>
  );
}
