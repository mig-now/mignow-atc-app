import { useState } from "react";
import { useTranslation } from "react-i18next";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import { Checkbox } from "@/components/ui/input";
import { Loading } from "@/components/ui/loading";
import { Table, TBody, Td, THead, Th, Tr } from "@/components/ui/table";
import { ApiErrorAlert } from "@/features/shared/api-error-alert";
import { EmptyState } from "@/features/shared/empty-state";
import { useGroups } from "../hooks/useSessions";
import { DecisionDialog } from "./DecisionDialog";
import type { SetSessionSearch } from "./search";

const COMPLEXITY_TONE = { simples: "success", média: "warning", media: "warning", complexa: "danger" } as const;

/** Aba Grupos e decisões: grupos do `cmd_grupos` (cartão KB top, complexidade, modelo sugerido) e decisão simples. */
export function GroupsTab({ sessionId, set }: { sessionId: string; set: SetSessionSearch }) {
  const { t } = useTranslation("remediation");
  const query = useGroups(sessionId);
  const [selected, setSelected] = useState<Set<string>>(new Set());
  const [deciding, setDeciding] = useState(false);

  if (query.isLoading) return <Loading />;
  const data = query.data;
  const groups = data?.groups ?? [];
  const chosen = groups.filter((g) => selected.has(g.key));
  const toggle = (key: string) =>
    setSelected((s) => {
      const n = new Set(s);
      if (n.has(key)) n.delete(key);
      else n.add(key);
      return n;
    });

  return (
    <div className="space-y-3">
      <ApiErrorAlert error={query.error} />
      {data && (
        <div className="flex flex-wrap items-center justify-between gap-2">
          <p className="text-sm text-muted-foreground">
            {t("groups.summary", {
              pending: data.pending,
              batch: data.batch,
              items: data.itemsInBatch,
              objects: data.objectsInBatch,
            })}
          </p>
          <Button disabled={chosen.length === 0} onClick={() => setDeciding(true)}>
            {t("groups.decide", { count: chosen.length })}
          </Button>
        </div>
      )}
      <Card>
        {groups.length === 0 ? (
          <EmptyState text={t("groups.empty")} />
        ) : (
          <Table>
            <THead>
              <Tr>
                <Th className="w-8" />
                <Th>{t("groups.col.ref")}</Th>
                <Th>{t("groups.col.callForm")}</Th>
                <Th>{t("groups.col.items")}</Th>
                <Th>{t("groups.col.objects")}</Th>
                <Th>{t("groups.col.kb")}</Th>
                <Th>{t("groups.col.complexity")}</Th>
              </Tr>
            </THead>
            <TBody>
              {groups.map((g) => (
                <Tr key={g.key}>
                  <Td>
                    <Checkbox
                      aria-label={t("groups.select", { ref: g.ref })}
                      checked={selected.has(g.key)}
                      onChange={() => toggle(g.key)}
                    />
                  </Td>
                  <Td>
                    <span className="font-medium">{g.ref}</span>{" "}
                    <span className="text-xs text-muted-foreground">{g.refType}</span>
                    {g.standard && (
                      <Badge tone="warning" className="ml-2">
                        {t("groups.standard")}
                      </Badge>
                    )}
                  </Td>
                  <Td>{g.callForm}</Td>
                  <Td>
                    <Button
                      variant="link"
                      size="sm"
                      className="px-0"
                      aria-label={t("groups.viewItems", { ref: g.ref })}
                      onClick={() => set({ tab: "items", group: g.ref, status: "pending", run: "" })}
                    >
                      {g.items.length}
                    </Button>
                  </Td>
                  <Td>
                    <details>
                      <summary className="cursor-pointer text-sm">{g.objects.length}</summary>
                      <ul className="mt-1 text-sm text-muted-foreground">
                        {g.objects.map((o) => (
                          <li key={o}>{o}</li>
                        ))}
                      </ul>
                    </details>
                  </Td>
                  <Td>
                    {g.kbTop ? (
                      <span>
                        <Badge tone="success">{g.kbTop.id}</Badge>{" "}
                        <span className="text-xs text-muted-foreground">
                          {t("groups.kbSolutions", { count: g.kbSolutions })}
                        </span>
                        {g.kbTop.alerts.length > 0 && (
                          <span className="block text-xs text-warning">{g.kbTop.alerts.join(" · ")}</span>
                        )}
                      </span>
                    ) : (
                      <span className="text-sm text-muted-foreground">{t("groups.noKb")}</span>
                    )}
                  </Td>
                  <Td>
                    <Badge tone={COMPLEXITY_TONE[g.complexity as keyof typeof COMPLEXITY_TONE] ?? "neutral"}>
                      {g.complexity}
                    </Badge>
                  </Td>
                </Tr>
              ))}
            </TBody>
          </Table>
        )}
      </Card>
      {deciding && (
        <DecisionDialog
          sessionId={sessionId}
          target={{ groups: chosen, count: chosen.reduce((s, g) => s + g.items.length, 0) }}
          onClose={(done) => {
            setDeciding(false);
            if (done) setSelected(new Set());
          }}
        />
      )}
    </div>
  );
}
