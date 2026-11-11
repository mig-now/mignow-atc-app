import { useTranslation } from "react-i18next";
import { Badge, type BadgeTone } from "@/components/ui/badge";

const RUN: Record<string, BadgeTone> = {
  Queued: "neutral",
  Running: "info",
  Cancelling: "warning",
  Cancelled: "neutral",
  Completed: "success",
  Failed: "danger",
};
const STEP: Record<string, BadgeTone> = {
  Pending: "neutral",
  Processing: "info",
  Completed: "success",
  Error: "danger",
  Skipped: "neutral",
};

/** Status do Run (`Queued|Running|Cancelling|Cancelled|Completed|Failed`, §3.2 do desenho). */
export function RunStatusBadge({ status }: { status: string }) {
  const { t } = useTranslation();
  return <Badge tone={RUN[status] ?? "neutral"}>{t(`runStatus.${status}`)}</Badge>;
}

export function StepStatusBadge({ status }: { status: string }) {
  const { t } = useTranslation();
  return <Badge tone={STEP[status] ?? "neutral"}>{t(`stepStatus.${status}`)}</Badge>;
}

export function ActiveBadge({ active }: { active: boolean }) {
  const { t } = useTranslation();
  return <Badge tone={active ? "success" : "neutral"}>{active ? t("common.active") : t("common.inactive")}</Badge>;
}

export const ACTIVE_RUN_STATUSES = ["Queued", "Running", "Cancelling"];

export function isRunActive(status: string) {
  return ACTIVE_RUN_STATUSES.includes(status);
}
