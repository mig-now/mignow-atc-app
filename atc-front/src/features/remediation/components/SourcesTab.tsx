import { useState } from "react";
import { useTranslation } from "react-i18next";
import type { SessionSourceInclude } from "@/api/generated/atc/model";
import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import { Dialog, DialogContent } from "@/components/ui/dialog";
import { Loading } from "@/components/ui/loading";
import { Table, TBody, Td, THead, Th, Tr } from "@/components/ui/table";
import { ApiErrorAlert } from "@/features/shared/api-error-alert";
import { EmptyState } from "@/features/shared/empty-state";
import { useSessionSource, useSessionSources } from "../hooks/useSessions";

/**
 * Aba Fontes (só consulta): objetos da sessão com fonte lido – a leitura é automática ao carregar os itens e no play, sem disparo manual.
 * Clicar num include abre o fonte num popup.
 */
export function SourcesTab({ sessionId }: { sessionId: string }) {
  const { t, i18n } = useTranslation("remediation");
  const query = useSessionSources(sessionId);
  const [open, setOpen] = useState<{ object: string; include: SessionSourceInclude } | null>(null);
  const data = query.data;
  return (
    <div className="space-y-3">
      {data && (
        <p className="text-sm text-muted-foreground">
          {t("sources.summary", { read: data.read, total: data.objects })}
        </p>
      )}
      <ApiErrorAlert error={query.error} />
      <Card>
        {query.isLoading ? (
          <Loading />
        ) : !data?.items.length ? (
          <EmptyState text={t("sources.empty")} />
        ) : (
          <Table>
            <THead>
              <Tr>
                <Th>{t("sources.col.object")}</Th>
                <Th>{t("sources.col.items")}</Th>
                <Th>{t("sources.col.includes")}</Th>
              </Tr>
            </THead>
            <TBody>
              {data.items.map((o) => (
                <Tr key={`${o.objectType}/${o.objectName}`}>
                  <Td>
                    <span className="font-medium">{o.objectName}</span>{" "}
                    <span className="text-xs text-muted-foreground">{o.objectType}</span>
                  </Td>
                  <Td className="tabular-nums">{t("sources.itemsOf", { count: o.itemCount, pending: o.pending })}</Td>
                  <Td>
                    <ul className="space-y-1">
                      {o.includes.map((inc) => (
                        <li key={inc.id} className="flex flex-wrap items-baseline gap-2">
                          <Button
                            variant="link"
                            size="sm"
                            className="h-auto px-0"
                            aria-label={t("sources.open", { name: inc.includeName })}
                            onClick={() => setOpen({ object: o.objectName, include: inc })}
                          >
                            {inc.includeName}
                          </Button>
                          <span className="text-xs text-muted-foreground">
                            {t("sources.lines", { count: inc.lines })} ·{" "}
                            {new Date(inc.readAt).toLocaleString(i18n.language)}
                          </span>
                        </li>
                      ))}
                    </ul>
                  </Td>
                </Tr>
              ))}
            </TBody>
          </Table>
        )}
      </Card>
      <Dialog open={open !== null} onOpenChange={(o) => !o && setOpen(null)}>
        {open && (
          <DialogContent
            wide
            title={open.include.includeName}
            description={t("sources.viewerDescription", { object: open.object, count: open.include.lines })}
          >
            <SourceViewer sessionId={sessionId} sourceId={open.include.id} />
          </DialogContent>
        )}
      </Dialog>
    </div>
  );
}

/** Fonte com número de linha, rolagem própria (somente leitura). */
function SourceViewer({ sessionId, sourceId }: { sessionId: string; sourceId: string }) {
  const query = useSessionSource(sessionId, sourceId);
  if (query.isLoading) return <Loading />;
  if (query.error || !query.data) return <ApiErrorAlert error={query.error} />;
  const lines = query.data.source.split("\n");
  if (lines.at(-1) === "") lines.pop();
  return (
    <div className="max-h-[70vh] overflow-auto rounded-md border bg-muted/30">
      <pre className="font-mono text-xs leading-5">
        {lines.map((line, i) => (
          // biome-ignore lint/suspicious/noArrayIndexKey: linhas do fonte são posicionais e não mudam
          <div key={i} className="flex">
            <span className="w-12 shrink-0 select-none pr-3 text-right text-muted-foreground">{i + 1}</span>
            <span className="whitespace-pre">{line}</span>
          </div>
        ))}
      </pre>
    </div>
  );
}
