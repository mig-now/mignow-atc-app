import { useTranslation } from "react-i18next";
import { Link } from "react-router";
import type { KbCard } from "@/api/generated/atc/model";
import { Alert } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { formatNumber } from "@/lib/format";

const COMPLEXITY_TONE = { simples: "success", media: "warning", complexa: "danger" } as const;

function Code({ label, text }: { label: string; text: string }) {
  return (
    <div className="min-w-0 space-y-1">
      <p className="text-xs font-medium text-muted-foreground">{label}</p>
      <pre className="max-h-96 overflow-auto rounded-md border border-border bg-muted p-3 font-mono text-xs whitespace-pre">
        {text || "—"}
      </pre>
    </div>
  );
}

/** Cartão da KB: problema (ABAP original) e solução (ABAP novo) lado a lado, padrão, ocorrências, alertas e substituição. */
export function KbCardView({ card, elementLink }: { card: KbCard; elementLink?: string }) {
  const { t, i18n } = useTranslation("kb");
  const complexityTone = COMPLEXITY_TONE[card.complexity as keyof typeof COMPLEXITY_TONE] ?? "neutral";
  return (
    <Card aria-label={card.id}>
      <CardHeader>
        <div className="flex min-w-0 flex-wrap items-center gap-2">
          <CardTitle className="font-mono">
            {elementLink ? (
              <Link to={elementLink} className="underline-offset-2 hover:underline">
                {card.id}
              </Link>
            ) : (
              card.id
            )}
          </CardTitle>
          {card.isContribution ? <Badge tone="info">{t("card.contribution")}</Badge> : null}
          <Badge tone={complexityTone}>{t("card.complexity", { value: card.complexity })}</Badge>
          <Badge>{t("card.occurrences", { n: formatNumber(card.occurrences, i18n.language) })}</Badge>
          {card.replacedBy ? <Badge tone="warning">{t("card.replacedBy", { id: card.replacedBy })}</Badge> : null}
        </div>
      </CardHeader>
      <CardContent className="space-y-3">
        <p className="text-sm">
          <span className="text-muted-foreground">{t("card.pattern")}: </span>
          {card.pattern}
        </p>
        {card.alerts.map((a) => (
          <Alert key={a} tone="warning">
            {a}
          </Alert>
        ))}
        <div className="grid gap-3 lg:grid-cols-2">
          <Code label={t("card.problem")} text={card.original} />
          <Code label={t("card.solution")} text={card.new} />
        </div>
        <p className="text-xs text-muted-foreground">
          {t("card.source")}: <span className="font-mono">{card.kbSource}</span>
        </p>
      </CardContent>
    </Card>
  );
}
