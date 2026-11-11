import { useTranslation } from "react-i18next";
import type { ExemptionListingNote, ExemptionListingReport } from "@/api/generated/atc/model";
import { Table, TBody, Td, THead, Th, Tr } from "@/components/ui/table";
import { formatNumber } from "@/lib/format";

function Notes({ title, hint, notes }: { title: string; hint?: string; notes: ExemptionListingNote[] }) {
  const { t } = useTranslation("exemption");
  if (!notes.length) return null;
  return (
    <section className="rounded-md border border-border p-3">
      <h4 className="text-sm font-semibold">
        {title} <span className="text-muted-foreground">({notes.length})</span>
      </h4>
      {hint ? <p className="mt-1 text-xs text-muted-foreground">{hint}</p> : null}
      <ul className="mt-2 max-h-32 space-y-1 overflow-y-auto text-sm">
        {notes.map((n) => (
          <li key={`${n.itemNumber}-${n.markerId ?? ""}`} className="font-mono">
            {t("report.itemNumber", { n: n.itemNumber })}
            {n.objectName ? ` · ${n.objectName}` : ""}
            {n.markerId ? ` · ${n.markerId}` : ""}
            {n.detail ? <span className="font-sans text-muted-foreground"> — {n.detail}</span> : null}
          </li>
        ))}
      </ul>
    </section>
  );
}

function Stat({ label, value }: { label: string; value: string }) {
  return (
    <div>
      <dt className="text-xs text-muted-foreground">{label}</dt>
      <dd className="text-lg font-semibold tabular-nums">{value}</dd>
    </div>
  );
}

/** Relatório do "listar" (porte do `listar.py`): vinculados, sem vínculo, já isentos/pedidos e prováveis sem escopo. */
export function ListingReport({ report }: { report: ExemptionListingReport }) {
  const { t, i18n } = useTranslation("exemption");
  const lang = i18n.language;
  return (
    <div className="space-y-4">
      <dl className="grid grid-cols-2 gap-3 sm:grid-cols-4">
        <Stat label={t("report.selectedItems")} value={formatNumber(report.selectedItems, lang)} />
        <Stat label={t("report.findings")} value={formatNumber(report.findings, lang)} />
        <Stat label={t("report.outOfScope")} value={formatNumber(report.outOfScope, lang)} />
        <Stat label={t("report.priorities")} value={report.priorities.map((p) => `P${p}`).join(", ") || "—"} />
      </dl>
      <p className="text-sm">
        {t("report.byOrigin", {
          link: report.byOrigin.finding_id ?? 0,
          cross: report.byOrigin.cruzamento ?? 0,
        })}
      </p>
      <div className="grid gap-3 sm:grid-cols-2">
        <Notes title={t("report.withoutLink")} hint={t("report.withoutLinkHint")} notes={report.withoutLink} />
        <Notes
          title={t("report.probablyNoScope")}
          hint={t("report.probablyNoScopeHint")}
          notes={report.probablyNoScope}
        />
        <Notes
          title={t("report.withoutPriority")}
          hint={t("report.withoutPriorityHint")}
          notes={report.withoutPriority}
        />
        <Notes title={t("report.alreadyExempt")} notes={report.alreadyExempt} />
        <Notes title={t("report.alreadyRequested")} notes={report.alreadyRequested} />
      </div>
      {report.divergentGroups.length > 0 && (
        <section className="rounded-md border border-warning/30 bg-warning/10 p-3 text-sm">
          <h4 className="text-sm font-semibold">{t("report.divergent")}</h4>
          <ul className="mt-1 space-y-1 font-mono">
            {report.divergentGroups.map((g) => (
              <li key={`${g.objectName}-${g.refObject ?? ""}`}>
                {g.objectName}
                {g.refObject ? ` · ${g.refObject}` : ""} —{" "}
                {t("report.divergentCount", { items: g.itemNumbers.length, findings: g.findings })}
              </li>
            ))}
          </ul>
        </section>
      )}
      {report.sample.length > 0 && (
        <div>
          <h4 className="mb-1 text-sm font-semibold">{t("report.sample")}</h4>
          <Table>
            <THead>
              <Tr>
                <Th>{t("col.seq")}</Th>
                <Th>{t("col.item")}</Th>
                <Th>{t("col.object")}</Th>
                <Th>{t("report.include")}</Th>
                <Th>{t("col.ref")}</Th>
                <Th>{t("col.marker")}</Th>
              </Tr>
            </THead>
            <TBody>
              {report.sample.map((s) => (
                <Tr key={s.seq}>
                  <Td>{s.seq}</Td>
                  <Td>{s.itemNumber}</Td>
                  <Td className="font-mono">{s.objectName}</Td>
                  <Td className="font-mono">
                    {s.include ?? "—"}
                    {s.line ? `:${s.line}` : ""}
                  </Td>
                  <Td className="font-mono">{s.refObject ?? "—"}</Td>
                  <Td className="font-mono">{s.markerId}</Td>
                </Tr>
              ))}
            </TBody>
          </Table>
        </div>
      )}
    </div>
  );
}
