import { useTranslation } from "react-i18next";
import type { ConnectionTestResponse } from "@/api/generated/atc/model";
import { Alert } from "@/components/ui/alert";
import { CodeMessage } from "@/features/shared/CodeMessage";
import { formatDateTime } from "@/lib/format";

/** Resultado do teste de conexão: latência, categoria do mandante, release, thumbprint fixado; produção = escrita bloqueada. */
export function ConnectionTestResult({
  result,
  isProduction,
}: {
  result: ConnectionTestResponse;
  isProduction: boolean;
}) {
  const { t, i18n } = useTranslation("config");
  const rows: [string, string][] = [
    [t("test.latency"), `${result.latencyMs} ms`],
    [t("test.clientCategory"), result.clientCategory ?? "—"],
    [t("test.release"), result.release ?? "—"],
    [t("test.thumbprint"), result.certThumbprint ?? "—"],
    [t("test.testedAt"), formatDateTime(result.testedAt, i18n.language)],
  ];
  return (
    <div className="space-y-3">
      <Alert tone={result.ok ? "info" : "danger"} title={result.ok ? t("test.ok") : t("test.failed")}>
        {!result.ok && <p>{result.messageCode ? <CodeMessage code={result.messageCode} /> : (result.message ?? "")}</p>}
      </Alert>
      <dl className="grid grid-cols-[auto_1fr] gap-x-4 gap-y-1 text-sm">
        {rows.map(([k, v]) => (
          <div key={k} className="contents">
            <dt className="text-muted-foreground">{k}</dt>
            <dd className="break-all font-mono">{v}</dd>
          </div>
        ))}
      </dl>
      {isProduction && (
        <Alert tone="warning" title={t("test.productionTitle")}>
          <p>{t("test.productionBody")}</p>
        </Alert>
      )}
    </div>
  );
}
