import { CheckCircle2, Circle, CircleSlash, Loader2, XCircle } from "lucide-react";
import { useTranslation } from "react-i18next";
import { cn } from "@/lib/cn";
import { CodeMessage } from "./CodeMessage";
import type { RunStep } from "./types";

const ICONS = {
  Pending: Circle,
  Processing: Loader2,
  Completed: CheckCircle2,
  Error: XCircle,
  Skipped: CircleSlash,
} as const;

/**
 * Passos do pipeline com o status de cada um. O rótulo vem de `runs:steps.<code>` (o módulo `runs` mantém a lista);
 * sem tradução, mostra o código.
 */
export function StepsTimeline({ steps }: { steps: RunStep[] }) {
  const { t, i18n } = useTranslation();
  return (
    <ol className="space-y-2" aria-label={t("steps.title")}>
      {[...steps]
        .sort((a, b) => a.order - b.order)
        .map((s) => {
          const Icon = ICONS[s.status as keyof typeof ICONS] ?? Circle;
          const label = i18n.exists(`runs:steps.${s.code}`) ? t(`runs:steps.${s.code}`) : s.code;
          return (
            <li key={s.code} className="flex items-start gap-3">
              <Icon
                aria-hidden="true"
                className={cn(
                  "mt-0.5 size-4 shrink-0",
                  s.status === "Processing" && "animate-spin text-info",
                  s.status === "Completed" && "text-success",
                  s.status === "Error" && "text-destructive",
                  (s.status === "Pending" || s.status === "Skipped") && "text-muted-foreground",
                )}
              />
              <div className="min-w-0">
                <p className={cn("text-sm", s.status === "Pending" ? "text-muted-foreground" : "font-medium")}>
                  {label}
                  {s.progress && s.progress.total > 0 && (
                    <span className="ml-2 text-xs font-normal text-muted-foreground tabular-nums">
                      {s.progress.done}/{s.progress.total}
                    </span>
                  )}
                  <span className="sr-only"> — {t(`stepStatus.${s.status}`)}</span>
                </p>
                {s.messageCode && (
                  <p className={cn("text-sm", s.status === "Error" ? "text-destructive" : "text-muted-foreground")}>
                    <CodeMessage code={s.messageCode} args={s.messageArgs} />
                  </p>
                )}
              </div>
            </li>
          );
        })}
    </ol>
  );
}
