import { useTranslation } from "react-i18next";
import type { ExemptionBatchResponse, RunResponse } from "@/api/generated/atc/model";
import { Alert } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { asDiscovery, isBlocked, noteText } from "../lib";
import { RunStepPanel } from "./RunStepPanel";

function Info({ label, children }: { label: string; children: React.ReactNode }) {
  return (
    <div className="min-w-0">
      <dt className="text-xs text-muted-foreground">{label}</dt>
      <dd className="text-sm">{children}</dd>
    </div>
  );
}

/** Etapa 1: descoberta (somente leitura no SAP): política, motivos, aprovadores e serviço ADT; bloqueia se a política ≠ A. */
export function DiscoveryStep({ batch, runs }: { batch: ExemptionBatchResponse; runs: RunResponse[] }) {
  const { t } = useTranslation("exemption");
  const d = asDiscovery(batch.discovery);
  const blocked = isBlocked(d);
  return (
    <div className="space-y-4">
      <p className="text-sm text-muted-foreground">{t("discovery.intro")}</p>
      <RunStepPanel batch={batch} runs={runs} action="discover" label={d ? t("discovery.again") : t("discovery.run")} />
      {d && (
        <>
          {blocked ? (
            <Alert tone="danger" title={t("discovery.blockedTitle")}>
              <p className="mt-1 text-sm">
                {t("discovery.blockedPolicy", { policy: d.policy?.display ?? d.policy?.value ?? "—" })}
              </p>
              {d.blockers?.length ? (
                <ul className="mt-2 list-disc pl-5 text-sm">
                  {d.blockers.map((b) => (
                    <li key={noteText(b)}>{noteText(b)}</li>
                  ))}
                </ul>
              ) : null}
            </Alert>
          ) : (
            <Alert tone="info" title={t("discovery.okTitle")}>
              <p className="mt-1 text-sm">{t("discovery.okText")}</p>
            </Alert>
          )}
          {d.warnings?.length ? (
            <Alert tone="warning" title={t("discovery.warnings")}>
              <ul className="mt-1 list-disc pl-5 text-sm">
                {d.warnings.map((w) => (
                  <li key={noteText(w)}>{noteText(w)}</li>
                ))}
              </ul>
            </Alert>
          ) : null}
          <dl className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
            <Info label={t("discovery.policy")}>
              <Badge tone={d.policy?.value === "A" ? "success" : "danger"}>
                {d.policy?.display ?? d.policy?.value ?? "—"}
              </Badge>
            </Info>
            <Info label={t("discovery.service")}>
              <Badge tone={d.adtApply ? "success" : "danger"}>
                {d.adtApply ? t("discovery.servicePublished") : t("discovery.serviceMissing")}
              </Badge>
            </Info>
            <Info label={t("discovery.approvers")}>
              {t("discovery.approversValue", { users: d.approvers ?? 0, areas: d.approvalAreas ?? 0 })}
            </Info>
            <Info label={t("discovery.userExemptions")}>{d.userExemptions ?? 0}</Info>
          </dl>
          <div className="grid gap-4 sm:grid-cols-2">
            <section>
              <h4 className="mb-1 text-sm font-semibold">{t("discovery.reasons")}</h4>
              <ul className="space-y-1 text-sm">
                {d.reasons?.map((r) => (
                  <li key={r.id}>
                    <span className="font-mono">{r.id}</span> — {r.title}
                    {r.justificationMandatory ? (
                      <span className="ml-1 text-xs text-muted-foreground">
                        ({t("discovery.justificationMandatory")})
                      </span>
                    ) : null}
                  </li>
                ))}
              </ul>
            </section>
            <section>
              <h4 className="mb-1 text-sm font-semibold">{t("discovery.validities")}</h4>
              <ul className="space-y-1 text-sm">
                {d.validities?.map((v) => (
                  <li key={v} className="font-mono text-sm">
                    {v}
                  </li>
                ))}
              </ul>
            </section>
          </div>
        </>
      )}
    </div>
  );
}
