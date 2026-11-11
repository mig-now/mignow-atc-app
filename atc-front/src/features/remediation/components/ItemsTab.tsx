import { Play } from "lucide-react";
import { useRef, useState } from "react";
import { useTranslation } from "react-i18next";
import type { ItemResponse, RunResponse, SessionResponse } from "@/api/generated/atc/model";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import { Dialog, DialogContent, DialogFooter } from "@/components/ui/dialog";
import { Checkbox, Input, NativeSelect } from "@/components/ui/input";
import { Loading } from "@/components/ui/loading";
import { Table, TBody, Td, THead, Th, Tr } from "@/components/ui/table";
import { ApiErrorAlert } from "@/features/shared/api-error-alert";
import { EmptyState } from "@/features/shared/empty-state";
import { Pagination } from "@/features/shared/pagination";
import { useTriageRules } from "@/features/triage";
import { useSessionItems } from "../hooks/useSessions";
import { ApplicationResult } from "./ApplicationResult";
import { AutoRemediationDialog } from "./AutoRemediationDialog";
import { DecisionDialog } from "./DecisionDialog";
import { ItemDialog } from "./ItemDialog";
import type { SessionSearch, SetSessionSearch } from "./search";
import { APPLY_ERROR_FILTER, ITEM_FILTER, ITEM_SITUATIONS, ItemStatusBadge } from "./status";

const PAGE_SIZE = 20;

/**
 * Aba Itens: tabela paginada, filtros na URL, diálogo do item, decisão simples dos itens marcados e o play da remediação automática
 * (itens marcados; sem marcação, todos os pendentes).
 */
export function ItemsTab({
  session,
  state,
  set,
  onAutoStarted,
}: {
  session: SessionResponse;
  state: SessionSearch;
  set: SetSessionSearch;
  onAutoStarted: (run: RunResponse) => void;
}) {
  const sessionId = session.id;
  const { t } = useTranslation("remediation");
  const query = useSessionItems(sessionId, {
    page: Number(state.page),
    pageSize: PAGE_SIZE,
    status: state.status || undefined,
    priority: state.priority || undefined,
    objectName: state.objectName || undefined,
    group: state.group || undefined,
    triage: state.triage || undefined,
  });
  // filtro por regra: códigos do cadastro de triagem (renomeáveis) – as marcações dos itens usam o código
  const triageRules = useTriageRules();
  const triageCodes = (triageRules.data ?? []).map((r) => r.code);
  const [selected, setSelected] = useState<Set<string>>(new Set());
  const [deciding, setDeciding] = useState(false);
  const [playing, setPlaying] = useState(false);
  // sem item marcado, o play vale para todos os pendentes e com erro: confirma antes
  const [confirmAll, setConfirmAll] = useState(false);
  const items = query.data?.items ?? [];
  // itens já vistos (a seleção pode atravessar páginas): o diálogo de decisão mostra a sugestão de cada um
  const seen = useRef(new Map<string, ItemResponse>());
  for (const i of items) seen.current.set(i.id, i);

  const toggle = (id: string) =>
    setSelected((s) => {
      const n = new Set(s);
      if (n.has(id)) n.delete(id);
      else n.add(id);
      return n;
    });
  const allOnPage = items.length > 0 && items.every((i) => selected.has(i.id));
  const togglePage = () =>
    setSelected((s) => {
      const n = new Set(s);
      for (const i of items) {
        if (allOnPage) n.delete(i.id);
        else n.add(i.id);
      }
      return n;
    });

  return (
    <div className="space-y-3">
      <div className="flex flex-wrap items-end gap-2">
        <NativeSelect
          className="max-w-44"
          aria-label={t("items.filter.status")}
          value={state.status}
          onChange={(e) => set({ status: e.target.value })}
        >
          <option value="">{t("items.filter.allStatuses")}</option>
          {ITEM_SITUATIONS.map((s) => (
            <option key={s} value={ITEM_FILTER[s]}>
              {t(`situation.${s}`)}
            </option>
          ))}
          <option value={APPLY_ERROR_FILTER}>{t("items.filter.applyError")}</option>
        </NativeSelect>
        <NativeSelect
          className="max-w-36"
          aria-label={t("items.filter.priority")}
          value={state.priority}
          onChange={(e) => set({ priority: e.target.value })}
        >
          <option value="">{t("items.filter.allPriorities")}</option>
          {["1", "2", "3"].map((p) => (
            <option key={p} value={p}>
              P{p}
            </option>
          ))}
        </NativeSelect>
        <NativeSelect
          className="max-w-52"
          aria-label={t("items.filter.triage")}
          value={state.triage}
          onChange={(e) => set({ triage: e.target.value })}
        >
          <option value="">{t("items.filter.allTriage")}</option>
          {triageCodes.map((r) => (
            <option key={r} value={r}>
              {r}
            </option>
          ))}
        </NativeSelect>
        <Input
          className="max-w-44"
          aria-label={t("items.filter.object")}
          placeholder={t("items.filter.object")}
          defaultValue={state.objectName}
          onBlur={(e) => e.target.value !== state.objectName && set({ objectName: e.target.value.trim() })}
          onKeyDown={(e) => e.key === "Enter" && set({ objectName: e.currentTarget.value.trim() })}
        />
        <Input
          className="max-w-44"
          aria-label={t("items.filter.group")}
          placeholder={t("items.filter.group")}
          defaultValue={state.group}
          onBlur={(e) => e.target.value !== state.group && set({ group: e.target.value.trim() })}
          onKeyDown={(e) => e.key === "Enter" && set({ group: e.currentTarget.value.trim() })}
        />
        <div className="ml-auto flex flex-wrap gap-2">
          <Button variant="outline" disabled={selected.size === 0} onClick={() => setDeciding(true)}>
            {t("items.decide", { count: selected.size })}
          </Button>
          <Button
            disabled={
              session.archived || (selected.size === 0 && session.counts.pending + session.counts.applyErrors === 0)
            }
            onClick={() => (selected.size ? setPlaying(true) : setConfirmAll(true))}
          >
            <Play />
            {selected.size ? t("auto.play", { count: selected.size }) : t("auto.playAll")}
          </Button>
        </div>
      </div>
      <ApiErrorAlert error={query.error} />
      <Card>
        {query.isLoading ? (
          <Loading />
        ) : items.length === 0 ? (
          <EmptyState text={t("items.empty")} />
        ) : (
          <>
            <Table>
              <THead>
                <Tr>
                  <Th className="w-8">
                    <Checkbox aria-label={t("items.selectPage")} checked={allOnPage} onChange={togglePage} />
                  </Th>
                  <Th>{t("items.col.n")}</Th>
                  <Th>{t("items.col.priority")}</Th>
                  <Th>{t("items.col.object")}</Th>
                  <Th>{t("items.col.ref")}</Th>
                  <Th>{t("items.col.check")}</Th>
                  <Th>{t("items.col.status")}</Th>
                  <Th>{t("items.col.sap")}</Th>
                  <Th>{t("items.col.triage")}</Th>
                </Tr>
              </THead>
              <TBody>
                {items.map((i) => (
                  <Tr key={i.id}>
                    <Td>
                      <Checkbox
                        aria-label={t("items.select", { n: i.n })}
                        checked={selected.has(i.id)}
                        onChange={() => toggle(i.id)}
                      />
                    </Td>
                    <Td>
                      <Button
                        variant="link"
                        size="sm"
                        className="px-0"
                        aria-label={t("items.open", { n: i.n })}
                        onClick={() => set({ item: i.id })}
                      >
                        {i.n}
                      </Button>
                    </Td>
                    <Td>{i.priority ? `P${i.priority}` : "—"}</Td>
                    <Td>
                      <span className="font-medium">{i.objectName}</span>{" "}
                      <span className="text-xs text-muted-foreground">{i.objectType}</span>
                    </Td>
                    <Td>{i.refObject ?? "—"}</Td>
                    <Td className="max-w-xs truncate" title={i.message ?? undefined}>
                      {i.check ?? "—"}
                    </Td>
                    <Td>
                      <ItemStatusBadge status={i.status} approval={i.awaitingApproval} suggestion={i.proposalAction} />
                    </Td>
                    <Td className="max-w-64">
                      <ApplicationResult result={i.applicationResult} detail={i.applicationDetail} />
                    </Td>
                    <Td>
                      <div className="flex flex-wrap gap-1">
                        {i.triageFlags.map((f) => (
                          <Badge key={f} tone="info">
                            {f}
                          </Badge>
                        ))}
                      </div>
                    </Td>
                  </Tr>
                ))}
              </TBody>
            </Table>
            <Pagination
              page={Number(state.page)}
              pageSize={PAGE_SIZE}
              total={query.data?.total ?? 0}
              onPage={(p) => set({ page: String(p) })}
            />
          </>
        )}
      </Card>
      {state.item && <ItemDialog sessionId={sessionId} itemId={state.item} onClose={() => set({ item: "" })} />}
      <Dialog open={confirmAll} onOpenChange={setConfirmAll}>
        <DialogContent
          title={t("auto.confirmAll.title")}
          description={t("auto.confirmAll.description", {
            pending: session.counts.pending,
            errors: session.counts.applyErrors,
          })}
        >
          <p className="text-sm text-muted-foreground">{t("auto.confirmAll.hint")}</p>
          <DialogFooter>
            <Button variant="outline" onClick={() => setConfirmAll(false)}>
              {t("common:actions.cancel")}
            </Button>
            <Button
              onClick={() => {
                setConfirmAll(false);
                setPlaying(true);
              }}
            >
              {t("auto.confirmAll.ok")}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
      <AutoRemediationDialog
        session={session}
        itemIds={[...selected]}
        open={playing}
        onOpenChange={setPlaying}
        onStarted={(run) => {
          setPlaying(false);
          setSelected(new Set());
          onAutoStarted(run);
        }}
      />
      {deciding && (
        <DecisionDialog
          sessionId={sessionId}
          target={{
            itemIds: [...selected],
            count: selected.size,
            items: [...selected].map((id) => seen.current.get(id)).filter((i): i is ItemResponse => !!i),
          }}
          onClose={(done) => {
            setDeciding(false);
            if (done) setSelected(new Set());
          }}
        />
      )}
    </div>
  );
}
