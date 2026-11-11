import { useTranslation } from "react-i18next";
import type { ItemResponse } from "@/api/generated/atc/model";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Loading } from "@/components/ui/loading";
import { ApiErrorAlert, Markdown } from "@/features/shared";
import { useProposal } from "./hooks";
import { isBlockingVerdict, RESULT_TONE } from "./labels";

/** Por item: parecer da revisão por IA, resultado da simulação/aplicação, backup e, em ERRO_ATIVACAO, a decisão. */
export function ItemApplyResult({
  sessionId,
  item,
  busy,
  onDecide,
}: {
  sessionId: string;
  item: ItemResponse;
  busy: boolean;
  onDecide: (decision: "keepInactive" | "rollback") => void;
}) {
  const { t } = useTranslation("remediation");
  const q = useProposal(sessionId, item.id);
  const header = (
    <h4 className="text-sm font-semibold">
      #{item.n} {item.objectName}
    </h4>
  );
  if (q.isLoading) return <Loading />;
  if (q.error || !q.data) return <ApiErrorAlert error={q.error ?? new Error("not-found")} />;
  const d = q.data;
  const app = d.application;
  const result = app?.result;
  const needsDecision = result === "ERRO_ATIVACAO" && !app?.errorDecision;
  return (
    <li className="space-y-2 rounded-md border border-border p-3">
      <div className="flex flex-wrap items-center gap-2">
        {header}
        {result ? (
          <Badge tone={RESULT_TONE[result] ?? "neutral"}>{t(`apply.result.${result}`, { defaultValue: result })}</Badge>
        ) : (
          <Badge>{t("apply.result.none")}</Badge>
        )}
        {app?.request && (
          <span className="text-xs text-muted-foreground">{t("apply.result.request", { request: app.request })}</span>
        )}
        <Badge tone={app?.backupSource ? "success" : "neutral"}>
          {app?.backupSource ? t("apply.result.backupDone") : t("apply.result.backupNone")}
        </Badge>
        {app?.errorDecision && (
          <Badge tone="info">
            {t(`apply.result.decision.${app.errorDecision}`, { defaultValue: app.errorDecision })}
          </Badge>
        )}
      </div>
      {d.reviewVerdict && (
        <p className="text-sm">
          {t("apply.result.review")}:{" "}
          <Badge tone={isBlockingVerdict(d.reviewVerdict) ? "danger" : "info"}>{d.reviewVerdict}</Badge>
        </p>
      )}
      {d.review && (
        <details>
          <summary className="cursor-pointer text-sm text-muted-foreground">{t("apply.result.reviewPoints")}</summary>
          <Markdown text={d.review} />
        </details>
      )}
      {app?.messages && app.messages.length > 0 && (
        <ul className="list-disc pl-5 text-sm text-muted-foreground">
          {app.messages.map((m) => (
            <li key={m}>{m}</li>
          ))}
        </ul>
      )}
      {needsDecision && (
        <div className="flex flex-wrap items-center gap-2">
          <span className="text-sm">{t("apply.result.activationQuestion")}</span>
          <Button size="sm" variant="outline" disabled={busy} onClick={() => onDecide("keepInactive")}>
            {t("apply.result.keepInactive")}
          </Button>
          <Button size="sm" variant="destructive" disabled={busy} onClick={() => onDecide("rollback")}>
            {t("apply.result.rollback")}
          </Button>
        </div>
      )}
    </li>
  );
}
