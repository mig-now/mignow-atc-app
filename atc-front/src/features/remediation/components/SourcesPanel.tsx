import { X } from "lucide-react";
import { useTranslation } from "react-i18next";
import { Alert } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { RunMonitorPanel } from "@/features/runs";
import { CodeMessage } from "@/features/shared/CodeMessage";

/**
 * Leitura automática dos fontes depois de carregar os itens (sem aba nem disparo manual): acompanha o run ReadSources ou, se ele não começou
 * (ex.: ambiente ocupado), diz o motivo – o play lê os fontes que faltarem antes de propor.
 */
export function SourcesPanel({
  runId,
  notStarted,
  onRunChange,
  onClose,
}: {
  runId: string;
  notStarted: string;
  onRunChange: (id: string) => void;
  onClose: () => void;
}) {
  const { t } = useTranslation("remediation");
  return (
    <Card className="mb-4">
      <CardHeader>
        <CardTitle>{t("sources.autoTitle")}</CardTitle>
        <Button variant="ghost" size="icon" aria-label={t("sources.close")} onClick={onClose}>
          <X />
        </Button>
      </CardHeader>
      <CardContent className="space-y-3">
        {runId ? (
          <>
            <p className="text-sm text-muted-foreground">{t("sources.autoDescription")}</p>
            <RunMonitorPanel runId={runId} onRetried={(r) => onRunChange(r.id)} />
          </>
        ) : (
          <Alert tone="warning" title={t("sources.notStarted")}>
            <p className="text-sm">
              <CodeMessage code={notStarted} />
            </p>
            <p className="text-sm">{t("sources.notStartedHint")}</p>
          </Alert>
        )}
      </CardContent>
    </Card>
  );
}
