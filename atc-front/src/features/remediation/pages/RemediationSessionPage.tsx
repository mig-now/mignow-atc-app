import { useTranslation } from "react-i18next";
import { Link, useParams } from "react-router";
import { Button } from "@/components/ui/button";
import { Loading } from "@/components/ui/loading";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { ApiErrorAlert } from "@/features/shared/api-error-alert";
import { PageHeader } from "@/features/shared/page-header";
import { useSearchState } from "@/lib/use-search-state";
import { ApplySlot } from "../apply-slot";
import { AutoRemediationPanel } from "../components/AutoRemediationPanel";
import { ExportTab } from "../components/ExportTab";
import { GroupsTab } from "../components/GroupsTab";
import { HistoryTab } from "../components/HistoryTab";
import { ItemsTab } from "../components/ItemsTab";
import { SessionHeader } from "../components/SessionHeader";
import { SourcesPanel } from "../components/SourcesPanel";
import { SourcesTab } from "../components/SourcesTab";
import { SESSION_DEFAULTS, TABS } from "../components/search";
import { TriageTab } from "../components/TriageTab";
import { useSession } from "../hooks/useSessions";

/** `/remediacao/:sessionId`: cabeçalho + abas (aba, filtros, item e run na URL). */
export function RemediationSessionPage() {
  const { t } = useTranslation("remediation");
  const { sessionId = "" } = useParams();
  const [state, set] = useSearchState(SESSION_DEFAULTS);
  const query = useSession(sessionId);
  const session = query.data;
  const tab = (TABS as readonly string[]).includes(state.tab) ? state.tab : "items";

  return (
    <>
      <PageHeader
        title={
          session ? t("session.titleOf", { system: session.sapSystemName, cycle: session.cycle }) : t("session.title")
        }
        actions={
          <Button asChild variant="outline">
            <Link to="/remediacao">{t("session.backToMonitor")}</Link>
          </Button>
        }
      />
      <ApiErrorAlert error={query.error} />
      {query.isLoading && <Loading />}
      {session && (
        <>
          <SessionHeader session={session} />
          {(state.sources || state.sourcesNotStarted) && (
            <SourcesPanel
              runId={state.sources}
              notStarted={state.sourcesNotStarted}
              onRunChange={(id) => set({ sources: id })}
              onClose={() => set({ sources: "", sourcesNotStarted: "" })}
            />
          )}
          {state.auto && (
            <AutoRemediationPanel
              sessionId={sessionId}
              runId={state.auto}
              onRunChange={(id) => set({ auto: id })}
              onHistory={() => set({ tab: "history" })}
              onClose={() => set({ auto: "" })}
            />
          )}
          <Tabs value={tab} onValueChange={(v) => set({ tab: v, run: "", item: "" })}>
            <TabsList>
              {TABS.map((k) => (
                <TabsTrigger key={k} value={k}>
                  {t(`tabs.${k}`)}
                </TabsTrigger>
              ))}
            </TabsList>
            <TabsContent value="items">
              <ItemsTab session={session} state={state} set={set} onAutoStarted={(run) => set({ auto: run.id })} />
            </TabsContent>
            <TabsContent value="sources">
              <SourcesTab sessionId={sessionId} />
            </TabsContent>
            <TabsContent value="triage">
              <TriageTab session={session} />
            </TabsContent>
            <TabsContent value="groups">
              <GroupsTab sessionId={sessionId} set={set} />
            </TabsContent>
            <TabsContent value="apply">
              <ApplySlot sessionId={sessionId} />
            </TabsContent>
            <TabsContent value="export">
              <ExportTab sessionId={sessionId} sapSystemId={session.sapSystemId} />
            </TabsContent>
            <TabsContent value="history">
              <HistoryTab sessionId={sessionId} />
            </TabsContent>
          </Tabs>
        </>
      )}
    </>
  );
}
