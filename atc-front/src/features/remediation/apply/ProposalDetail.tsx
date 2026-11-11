import { useTranslation } from "react-i18next";
import { Link } from "react-router";
import type { ItemResponse } from "@/api/generated/atc/model";
import { Alert } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Loading } from "@/components/ui/loading";
import { ApiErrorAlert, Markdown, SourceDiff } from "@/features/shared";
import { useProposal } from "./hooks";
import { isBlockingVerdict, kbCategory, STATUS_TONE } from "./labels";

const ACTION_TONE = { corrigir: "success", isencao: "warning", cancelar: "neutral", pendente: "info" } as const;

/** Proposta de um item: origem, cartão KB, ação, alertas, sintaxe, marcadores, diff lado a lado, modelo e tokens. */
export function ProposalDetail({
  sessionId,
  item,
  busy,
  onConfirmKb,
  onConfirmIa,
  onCancel,
  onExemption,
  onAskIa,
}: {
  sessionId: string;
  item: ItemResponse;
  busy: boolean;
  onConfirmKb: () => void;
  onConfirmIa: () => void;
  onCancel: () => void;
  onExemption: () => void;
  onAskIa: () => void;
}) {
  const { t } = useTranslation("remediation");
  const q = useProposal(sessionId, item.id);
  if (q.isLoading) return <Loading />;
  if (q.error || !q.data) return <ApiErrorAlert error={q.error ?? new Error("not-found")} />;
  const d = q.data;
  const p = d.proposal;
  const before = d.preparation?.source ?? p?.blocks.map((b) => b.before.join("\n")).join("\n") ?? "";
  const after = d.preparation?.newSource ?? p?.newSource ?? p?.blocks.map((b) => b.after.join("\n")).join("\n") ?? "";
  const alerts = d.alerts.length ? d.alerts : (p?.alerts ?? []);
  const syntaxMessages = p?.syntaxMessages ?? [];
  const decided = d.status === "Confirmado KB" || d.status === "Confirmado IA";
  const applied = d.application?.result === "ATIVADO";

  return (
    <article aria-label={t("apply.review.proposalOf", { n: item.n })} className="space-y-4">
      <header className="flex flex-wrap items-center gap-2">
        <h3 className="text-base font-semibold">
          #{item.n} {item.objectName}
        </h3>
        <Badge tone={STATUS_TONE[d.status] ?? "neutral"}>{d.status || t("apply.status.pending")}</Badge>
        {p && (
          <>
            <Badge tone={p.origin === "KB" ? "success" : "primary"}>
              {t("apply.review.origin")}: {p.origin}
            </Badge>
            <Badge tone={ACTION_TONE[p.action as keyof typeof ACTION_TONE] ?? "neutral"}>
              {t(`apply.action.${p.action}`, { defaultValue: p.action })}
            </Badge>
          </>
        )}
      </header>

      {!p ? (
        <Alert tone="info">{t("apply.review.noProposal")}</Alert>
      ) : (
        <>
          {p.kbCardId && (
            <p className="text-sm">
              {t("apply.review.kbCard")}:{" "}
              {item.refObject ? (
                <Link
                  className="font-medium text-primary underline-offset-4 hover:underline"
                  to={`/kb/${kbCategory(item.refType)}/${encodeURIComponent(item.refObject)}`}
                >
                  {p.kbCardId}
                </Link>
              ) : (
                <span className="font-medium">{p.kbCardId}</span>
              )}
            </p>
          )}
          {p.justification && (
            <section>
              <h4 className="text-sm font-semibold">{t("apply.review.justification")}</h4>
              <p className="text-sm whitespace-pre-wrap">{p.justification}</p>
            </section>
          )}
          {alerts.length > 0 && (
            <Alert tone="warning" title={t("apply.review.kbAlerts")}>
              <ul className="list-disc pl-5">
                {alerts.map((a) => (
                  <li key={a}>{a}</li>
                ))}
              </ul>
            </Alert>
          )}
          <p className="text-sm">
            {t("apply.review.syntax")}:{" "}
            {d.syntaxOk == null ? (
              <Badge>{t("apply.review.syntaxUnknown")}</Badge>
            ) : d.syntaxOk ? (
              <Badge tone="success">{t("apply.review.syntaxOk")}</Badge>
            ) : (
              <Badge tone="danger">{t("apply.review.syntaxError")}</Badge>
            )}
          </p>
          {syntaxMessages.length > 0 && (
            <ul className="list-disc pl-5 text-sm text-muted-foreground">
              {syntaxMessages.map((m) => (
                <li key={m}>{m}</li>
              ))}
            </ul>
          )}
          {p.markedBlock && (
            <section>
              <h4 className="mb-1 text-sm font-semibold">{t("apply.review.markedBlock")}</h4>
              <pre className="max-h-60 overflow-auto rounded-md border border-border bg-muted p-3 font-mono text-sm">
                {p.markedBlock}
              </pre>
            </section>
          )}
          {(before || after) && (
            <section>
              <h4 className="mb-1 text-sm font-semibold">{t("apply.review.diff")}</h4>
              <SourceDiff before={before} after={after} />
            </section>
          )}
          <p className="text-xs text-muted-foreground">
            {t("apply.review.model")}: {p.model ?? "—"} · {t("apply.review.tokens")}: {p.tokens ?? "—"}
          </p>
        </>
      )}

      {d.review && (
        <section>
          <h4 className="mb-1 text-sm font-semibold">
            {t("apply.review.aiReview")}{" "}
            {d.reviewVerdict && (
              <Badge tone={isBlockingVerdict(d.reviewVerdict) ? "danger" : "info"}>{d.reviewVerdict}</Badge>
            )}
          </h4>
          <Markdown text={d.review} />
        </section>
      )}

      {applied ? (
        <Alert tone="info">{t("apply.review.alreadyApplied")}</Alert>
      ) : (
        <div className="flex flex-wrap gap-2">
          <Button size="sm" disabled={busy || !p || p.origin !== "KB"} onClick={onConfirmKb}>
            {t("apply.review.confirmKb")}
          </Button>
          <Button size="sm" disabled={busy || !p || p.origin !== "IA"} onClick={onConfirmIa}>
            {t("apply.review.confirmIa")}
          </Button>
          <Button size="sm" variant="outline" disabled={busy} onClick={onAskIa}>
            {t("apply.review.askIa")}
          </Button>
          <Button size="sm" variant="outline" disabled={busy} onClick={onExemption}>
            {t("apply.review.exemption")}
          </Button>
          <Button size="sm" variant="destructive" disabled={busy} onClick={onCancel}>
            {t("apply.review.cancel")}
          </Button>
          {decided && <span className="self-center text-xs text-muted-foreground">{t("apply.review.redecide")}</span>}
        </div>
      )}
    </article>
  );
}
