import { ArrowLeft } from "lucide-react";
import { useTranslation } from "react-i18next";
import { Link, useParams } from "react-router";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Loading } from "@/components/ui/loading";
import { ApiErrorAlert } from "@/features/shared/api-error-alert";
import { EmptyState } from "@/features/shared/empty-state";
import { PageHeader } from "@/features/shared/page-header";
import { formatDate } from "@/lib/format";
import { KbCardView } from "../components/KbCardView";
import { useKbElement } from "../hooks/useKb";

/** Elemento da KB: soluções (cartões), contribuições C-*, fatos F-* e substituições. */
export function KbElementPage() {
  const { t, i18n } = useTranslation("kb");
  const { category = "", element = "" } = useParams();
  const q = useKbElement(category, element);
  const d = q.data;

  const back = (
    <Button asChild variant="ghost">
      <Link to="/kb">
        <ArrowLeft aria-hidden /> {t("element.back")}
      </Link>
    </Button>
  );
  if (q.isLoading) return <Loading />;
  if (q.error || !d) {
    return (
      <>
        <PageHeader title={t("element.title")} actions={back} />
        <ApiErrorAlert error={q.error ?? new Error("not-found")} />
      </>
    );
  }
  return (
    <>
      <PageHeader
        title={d.element}
        description={`${t("element.title")} · ${t(`category.${d.category}`, { defaultValue: d.category })}`}
        actions={back}
      />
      <div className="space-y-6">
        <section className="space-y-3" aria-labelledby="kb-cards">
          <h2 id="kb-cards" className="text-base font-semibold">
            {t("element.cards", { count: d.cards.length })}
          </h2>
          {d.cards.length === 0 ? (
            <EmptyState text={t("element.noCards")} />
          ) : (
            d.cards.map((c) => <KbCardView key={c.id} card={c} />)
          )}
        </section>

        {d.contributions.length > 0 ? (
          <section className="space-y-3" aria-labelledby="kb-contrib">
            <h2 id="kb-contrib" className="text-base font-semibold">
              {t("element.contributions", { count: d.contributions.length })}
            </h2>
            {d.contributions.map((c) => (
              <KbCardView key={c.id} card={c} />
            ))}
          </section>
        ) : null}

        {d.facts.length > 0 ? (
          <Card>
            <CardHeader>
              <CardTitle>{t("element.facts", { count: d.facts.length })}</CardTitle>
            </CardHeader>
            <CardContent>
              <ul className="space-y-2 text-sm">
                {d.facts.map((f) => (
                  <li key={f.id}>
                    <span className="font-mono text-xs text-muted-foreground">{f.id}</span>{" "}
                    <span className="text-xs text-muted-foreground">{formatDate(f.date, i18n.language)}</span>
                    <p>{f.text}</p>
                  </li>
                ))}
              </ul>
            </CardContent>
          </Card>
        ) : null}

        {d.replacements.length > 0 ? (
          <Card>
            <CardHeader>
              <CardTitle>{t("element.replacements")}</CardTitle>
            </CardHeader>
            <CardContent>
              <ul className="space-y-2 text-sm">
                {d.replacements.map((r) => (
                  <li key={`${r.cardId}-${r.replacedBy}`}>
                    {t("element.replacement", {
                      card: r.cardId,
                      by: r.replacedBy,
                      date: formatDate(r.date, i18n.language),
                    })}
                    {r.reason ? <span className="text-muted-foreground"> — {r.reason}</span> : null}
                  </li>
                ))}
              </ul>
            </CardContent>
          </Card>
        ) : null}
      </div>
    </>
  );
}
