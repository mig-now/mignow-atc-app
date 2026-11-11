import { useTranslation } from "react-i18next";
import { Badge } from "@/components/ui/badge";
import { Loading } from "@/components/ui/loading";
import { formatDateTime } from "@/lib/format";
import { ApiErrorAlert } from "./api-error-alert";
import { CodeMessage } from "./CodeMessage";
import { EmptyState } from "./empty-state";
import { Pagination } from "./pagination";
import type { RunEvent } from "./types";

const TONE = { Info: "info", Warning: "warning", Error: "danger" } as const;

type Paging = { page: number; pageSize: number; total: number; onPage: (page: number) => void };

/** Log da execução (a ordem é a recebida). Apresentacional: o módulo busca os eventos e passa a paginação. */
export function EventsLog({
  events,
  loading = false,
  error,
  paging,
}: {
  events: RunEvent[];
  loading?: boolean;
  error?: unknown;
  paging?: Paging;
}) {
  const { t, i18n } = useTranslation();
  if (loading) return <Loading />;
  return (
    <>
      <ApiErrorAlert error={error} />
      {events.length === 0 ? (
        <EmptyState text={t("log.empty")} />
      ) : (
        <ol className="divide-y divide-border">
          {events.map((e) => (
            <li key={e.id} className="flex flex-wrap items-start gap-x-3 gap-y-1 px-3 py-2 text-sm">
              <span className="w-32 shrink-0 text-xs text-muted-foreground">{formatDateTime(e.at, i18n.language)}</span>
              <Badge tone={TONE[e.level as keyof typeof TONE] ?? "neutral"}>{t(`eventLevel.${e.level}`)}</Badge>
              {e.itemRef && <span className="font-mono">{e.itemRef}</span>}
              <span className="min-w-0 flex-1">
                <CodeMessage code={e.code} args={e.args} />
              </span>
            </li>
          ))}
        </ol>
      )}
      {paging && events.length > 0 && <Pagination {...paging} />}
    </>
  );
}
