import { useState } from "react";
import { useTranslation } from "react-i18next";
import type { ExtractionResponse } from "@/api/generated/atc/model";
import { Badge } from "@/components/ui/badge";
import { Dialog, DialogContent } from "@/components/ui/dialog";
import { Input, NativeSelect } from "@/components/ui/input";
import { Loading } from "@/components/ui/loading";
import { Table, TBody, Td, THead, Th, Tr } from "@/components/ui/table";
import { ApiErrorAlert } from "@/features/shared/api-error-alert";
import { EmptyState } from "@/features/shared/empty-state";
import { Pagination } from "@/features/shared/pagination";
import { useExtractionFindings } from "../hooks/useExtraction";

const PAGE_SIZE = 20;

/** Lista paginada de findings da extração, com filtros de prioridade, objeto e baseline. */
export function FindingsDialog({
  extraction,
  onClose,
}: {
  extraction: ExtractionResponse | null;
  onClose: () => void;
}) {
  const { t } = useTranslation("extraction");
  const [priority, setPriority] = useState("");
  const [object, setObject] = useState("");
  const [baseline, setBaseline] = useState("");
  const [page, setPage] = useState(1);
  const query = useExtractionFindings(extraction?.id ?? "", {
    page,
    pageSize: PAGE_SIZE,
    priority: priority ? Number(priority) : undefined,
    object: object.trim() || undefined,
    inBaseline: baseline === "" ? undefined : baseline === "yes",
  });
  const filter = (set: (v: string) => void) => (v: string) => {
    set(v);
    setPage(1);
  };
  return (
    <Dialog open={!!extraction} onOpenChange={(o) => !o && onClose()}>
      <DialogContent
        wide
        title={t("findings.title", { id: extraction?.displayId ?? "" })}
        description={t("findings.description")}
      >
        <div className="mb-3 grid gap-3 sm:grid-cols-3">
          <NativeSelect
            aria-label={t("findings.priority")}
            value={priority}
            onChange={(e) => filter(setPriority)(e.target.value)}
          >
            <option value="">{t("findings.allPriorities")}</option>
            {[1, 2, 3, 4].map((p) => (
              <option key={p} value={p}>{`P${p}`}</option>
            ))}
          </NativeSelect>
          <Input
            aria-label={t("findings.object")}
            placeholder={t("findings.objectPlaceholder")}
            value={object}
            onChange={(e) => filter(setObject)(e.target.value)}
          />
          <NativeSelect
            aria-label={t("findings.baseline")}
            value={baseline}
            onChange={(e) => filter(setBaseline)(e.target.value)}
          >
            <option value="">{t("findings.baselineAll")}</option>
            <option value="yes">{t("findings.baselineYes")}</option>
            <option value="no">{t("findings.baselineNo")}</option>
          </NativeSelect>
        </div>
        <ApiErrorAlert error={query.error} />
        {query.isLoading ? (
          <Loading />
        ) : !query.data?.items.length ? (
          <EmptyState text={t("findings.empty")} />
        ) : (
          <>
            <Table>
              <THead>
                <Tr>
                  <Th>{t("findings.col.priority")}</Th>
                  <Th>{t("findings.col.check")}</Th>
                  <Th>{t("findings.col.message")}</Th>
                  <Th>{t("findings.col.object")}</Th>
                  <Th>{t("findings.col.ref")}</Th>
                  <Th>{t("findings.col.note")}</Th>
                  <Th>{t("findings.col.baseline")}</Th>
                </Tr>
              </THead>
              <TBody>
                {query.data.items.map((f) => (
                  <Tr key={f.id}>
                    <Td>{f.priority ? `P${f.priority}` : "—"}</Td>
                    <Td>{f.checkTitle ?? "—"}</Td>
                    <Td className="max-w-sm">{f.messageTitle ?? "—"}</Td>
                    <Td className="font-mono">
                      {f.objectName}
                      {f.objectType ? <span className="text-muted-foreground"> ({f.objectType})</span> : null}
                    </Td>
                    <Td className="font-mono">
                      {f.refObjectName ?? "—"}
                      {f.refObjectType ? <span className="text-muted-foreground"> ({f.refObjectType})</span> : null}
                    </Td>
                    <Td className="font-mono">{f.note ?? "—"}</Td>
                    <Td>{f.inBaseline ? <Badge tone="neutral">{t("findings.baselineYes")}</Badge> : "—"}</Td>
                  </Tr>
                ))}
              </TBody>
            </Table>
            <Pagination page={page} pageSize={PAGE_SIZE} total={query.data.total} onPage={setPage} />
          </>
        )}
      </DialogContent>
    </Dialog>
  );
}
