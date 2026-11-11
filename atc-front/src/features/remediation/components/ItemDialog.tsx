import { useTranslation } from "react-i18next";
import type { ItemDetailResponse } from "@/api/generated/atc/model";
import { Alert } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { Dialog, DialogContent } from "@/components/ui/dialog";
import { Loading } from "@/components/ui/loading";
import { ApiErrorAlert } from "@/features/shared/api-error-alert";
import { Markdown } from "@/features/shared/Markdown";
import { formatDateTime } from "@/lib/format";
import { useSessionItem } from "../hooks/useSessions";
import { ApplicationResult, isApplyFailure, useApplyDetail } from "./ApplicationResult";
import { ItemStatusBadge } from "./status";

function Info({ label, children }: { label: string; children: React.ReactNode }) {
  return (
    <div className="min-w-0">
      <dt className="text-xs text-muted-foreground">{label}</dt>
      <dd className="text-sm break-words">{children ?? "—"}</dd>
    </div>
  );
}

function Section({ title, children }: { title: string; children: React.ReactNode }) {
  return (
    <section className="space-y-2">
      <h3 className="text-base font-semibold">{title}</h3>
      {children}
    </section>
  );
}

function Detail({ d }: { d: ItemDetailResponse }) {
  const { t, i18n } = useTranslation("remediation");
  const { item, proposal, application, preparation } = d;
  const lang = i18n.language;
  const detailText = useApplyDetail();
  return (
    <div className="space-y-5">
      {isApplyFailure(item.applicationResult) && (
        <Alert tone="danger" title={t("item.applyFailedTitle")}>
          {detailText(item.applicationDetail)}
        </Alert>
      )}
      {item.awaitingApproval && (
        <Alert
          tone="warning"
          title={t("item.approvalTitle", { suggestion: t(`suggestion.${item.proposalAction ?? "pendente"}`) })}
        >
          {item.proposalNote}
        </Alert>
      )}
      <Section title={t("item.finding")}>
        <dl className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
          <Info label={t("items.col.object")}>
            {item.objectName} ({item.objectType})
          </Info>
          <Info label={t("items.col.priority")}>{item.priority ? `P${item.priority}` : null}</Info>
          <Info label={t("items.col.status")}>
            <ItemStatusBadge status={item.status} approval={item.awaitingApproval} suggestion={item.proposalAction} />
          </Info>
          <Info label={t("items.col.sap")}>
            <ApplicationResult result={item.applicationResult} detail={item.applicationDetail} full />
          </Info>
          <Info label={t("item.check")}>{item.check}</Info>
          <Info label={t("item.ref")}>
            {item.refObject} {item.refType ? `(${item.refType})` : ""}
          </Info>
          <Info label={t("item.note")}>{item.note}</Info>
          <Info label={t("item.package")}>{item.package}</Info>
          <Info label={t("item.classification")}>{item.classification}</Info>
          <Info label={t("item.findingId")}>{item.findingId}</Info>
          <Info label={t("item.origin")}>{item.origin}</Info>
          <Info label={t("item.statusDate")}>{formatDateTime(item.statusDate, lang)}</Info>
        </dl>
        <Info label={t("item.message")}>{item.message}</Info>
        {item.text && <Info label={t("item.text")}>{item.text}</Info>}
        {item.obs && <Info label={t("item.obs")}>{item.obs}</Info>}
        {item.triageFlags.length > 0 && (
          <div className="flex flex-wrap gap-1">
            {item.triageFlags.map((f) => (
              <Badge key={f} tone="info">
                {f}
              </Badge>
            ))}
          </div>
        )}
      </Section>

      <Section title={t("item.sheet")}>
        {d.correctionSheetMd ? (
          <Markdown text={d.correctionSheetMd} />
        ) : (
          <p className="text-sm text-muted-foreground">{t("item.sheetEmpty")}</p>
        )}
      </Section>

      <Section title={t("item.history")}>
        {d.statusHistory.length === 0 ? (
          <p className="text-sm text-muted-foreground">{t("item.historyEmpty")}</p>
        ) : (
          <ul className="space-y-1 text-sm">
            {d.statusHistory.map((h) => (
              <li key={`${h.at}-${h.status}`} className="flex items-center gap-2">
                <span className="text-xs text-muted-foreground">{formatDateTime(h.at, lang)}</span>
                <ItemStatusBadge status={h.status} />
              </li>
            ))}
          </ul>
        )}
      </Section>

      <Section title={t("item.proposal")}>
        {proposal ? (
          <dl className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
            <Info label={t("item.proposal.origin")}>{proposal.origin}</Info>
            <Info label={t("item.proposal.kbCard")}>{proposal.kbCardId}</Info>
            <Info label={t("item.proposal.action")}>{proposal.action}</Info>
            <Info label={t("item.proposal.blocks")}>{proposal.blocks.length}</Info>
            <Info label={t("item.proposal.syntax")}>
              {proposal.syntaxOk == null ? null : proposal.syntaxOk ? t("item.ok") : t("item.notOk")}
            </Info>
            <Info label={t("item.proposal.model")}>{proposal.model}</Info>
            {proposal.alerts.length > 0 && <Info label={t("item.proposal.alerts")}>{proposal.alerts.join(" · ")}</Info>}
            {proposal.justification && <Info label={t("item.proposal.justification")}>{proposal.justification}</Info>}
          </dl>
        ) : (
          <p className="text-sm text-muted-foreground">{t("item.proposalEmpty")}</p>
        )}
        {preparation && (
          <p className="text-xs text-muted-foreground">
            {t("item.prepared", { at: formatDateTime(preparation.preparedAt, lang) })}
          </p>
        )}
      </Section>

      <Section title={t("item.application")}>
        {application ? (
          <dl className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
            <Info label={t("item.application.result")}>
              <ApplicationResult result={application.result} detail={application.history.at(-1)?.detail} full />
            </Info>
            <Info label={t("item.application.request")}>{application.request}</Info>
            <Info label={t("item.application.errorDecision")}>{application.errorDecision}</Info>
            <Info label={t("item.application.attempts")}>{application.history.length}</Info>
            {application.messages && application.messages.length > 0 && (
              <Info label={t("item.application.messages")}>{application.messages.join(" · ")}</Info>
            )}
          </dl>
        ) : (
          <p className="text-sm text-muted-foreground">{t("item.applicationEmpty")}</p>
        )}
        {application && application.history.length > 0 && (
          <ul className="space-y-1 text-sm">
            {[...application.history].reverse().map((h) => (
              <li key={`${h.at}-${h.mode}`} className="flex flex-wrap items-baseline gap-2">
                <span className="text-muted-foreground">{formatDateTime(h.at, lang)}</span>
                <span>{t(`item.application.mode.${h.mode}`, { defaultValue: h.mode })}</span>
                <Badge tone={isApplyFailure(h.result) ? "danger" : "neutral"}>
                  {t(`apply.result.${h.result}`, { defaultValue: h.result })}
                </Badge>
                {h.request && <span className="text-muted-foreground">{h.request}</span>}
                {h.detail && (
                  <span className={isApplyFailure(h.result) ? "text-destructive" : ""}>{detailText(h.detail)}</span>
                )}
              </li>
            ))}
          </ul>
        )}
      </Section>
    </div>
  );
}

/** Diálogo do item: dados do finding, ficha em markdown, histórico de status, proposta/aplicação resumidas. */
export function ItemDialog({ sessionId, itemId, onClose }: { sessionId: string; itemId: string; onClose: () => void }) {
  const { t } = useTranslation("remediation");
  const q = useSessionItem(sessionId, itemId);
  return (
    <Dialog open onOpenChange={(o) => !o && onClose()}>
      <DialogContent wide title={q.data ? t("item.title", { n: q.data.item.n }) : t("item.titleLoading")}>
        {q.isLoading ? <Loading /> : q.error || !q.data ? <ApiErrorAlert error={q.error} /> : <Detail d={q.data} />}
      </DialogContent>
    </Dialog>
  );
}
