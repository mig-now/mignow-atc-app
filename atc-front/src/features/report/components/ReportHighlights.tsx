import { Plus, Trash2 } from "lucide-react";
import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import type { HighlightText, PendingText } from "@/api/generated/atc/model";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Field } from "@/components/ui/field";
import { Input, Textarea } from "@/components/ui/input";
import { Loading } from "@/components/ui/loading";
import { useToast } from "@/components/ui/toast";
import { ApiErrorAlert } from "@/features/shared/api-error-alert";
import { useHighlights, useSaveHighlights } from "../hooks/useReport";

type Row<T> = T & { uid: string };
const withUid = <T,>(v: T): Row<T> => ({ ...v, uid: crypto.randomUUID() });
const strip = <T extends { uid: string }>({ uid: _uid, ...rest }: T) => rest;

/** Destaques e pendências manuais do relatório (GET/PUT highlights); a premissa carregada é devolvida como veio. */
export function ReportHighlights({ customerId }: { customerId: string }) {
  const { t } = useTranslation("report");
  const toast = useToast();
  const q = useHighlights(customerId);
  const save = useSaveHighlights(customerId);
  const [highlights, setHighlights] = useState<Row<HighlightText>[]>([]);
  const [pending, setPending] = useState<Row<PendingText>[]>([]);
  useEffect(() => {
    if (q.data) {
      setHighlights(q.data.highlights.map(withUid));
      setPending(q.data.pending.map(withUid));
    }
  }, [q.data]);

  if (q.isLoading) return <Loading />;
  if (q.error) return <ApiErrorAlert error={q.error} />;

  const patchH = (i: number, patch: Partial<HighlightText>) =>
    setHighlights((l) => l.map((x, j) => (j === i ? { ...x, ...patch } : x)));
  const patchP = (i: number, patch: Partial<PendingText>) =>
    setPending((l) => l.map((x, j) => (j === i ? { ...x, ...patch } : x)));

  return (
    <div className="space-y-4">
      <p className="text-sm text-muted-foreground">{t("highlights.hint")}</p>
      <Card>
        <CardHeader>
          <CardTitle>{t("highlights.highlights")}</CardTitle>
          <Button
            size="sm"
            variant="outline"
            onClick={() => setHighlights((l) => [...l, { ...withUid({ texto: "", fonte: null }) }])}
          >
            <Plus aria-hidden /> {t("highlights.add")}
          </Button>
        </CardHeader>
        <CardContent className="space-y-3">
          {highlights.length === 0 ? <p className="text-sm text-muted-foreground">{t("highlights.none")}</p> : null}
          {highlights.map((h, i) => (
            <div key={h.uid} className="grid items-start gap-2 md:grid-cols-[1fr_14rem_auto]">
              <Field id={`hl-text-${i}`} label={t("highlights.text")}>
                <Textarea
                  id={`hl-text-${i}`}
                  rows={2}
                  value={h.texto}
                  onChange={(e) => patchH(i, { texto: e.target.value })}
                />
              </Field>
              <Field id={`hl-src-${i}`} label={t("highlights.source")}>
                <Input
                  id={`hl-src-${i}`}
                  value={h.fonte ?? ""}
                  onChange={(e) => patchH(i, { fonte: e.target.value || null })}
                />
              </Field>
              <Button
                className="mt-6"
                variant="ghost"
                size="icon"
                aria-label={t("highlights.remove")}
                onClick={() => setHighlights((l) => l.filter((_, j) => j !== i))}
              >
                <Trash2 />
              </Button>
            </div>
          ))}
        </CardContent>
      </Card>
      <Card>
        <CardHeader>
          <CardTitle>{t("highlights.pending")}</CardTitle>
          <Button
            size="sm"
            variant="outline"
            onClick={() => setPending((l) => [...l, { ...withUid({ titulo: "", texto: "", fonte: null }) }])}
          >
            <Plus aria-hidden /> {t("highlights.add")}
          </Button>
        </CardHeader>
        <CardContent className="space-y-3">
          {pending.length === 0 ? <p className="text-sm text-muted-foreground">{t("highlights.none")}</p> : null}
          {pending.map((p, i) => (
            <div key={p.uid} className="grid items-start gap-2 md:grid-cols-[12rem_1fr_12rem_auto]">
              <Field id={`pd-title-${i}`} label={t("highlights.title")}>
                <Input id={`pd-title-${i}`} value={p.titulo} onChange={(e) => patchP(i, { titulo: e.target.value })} />
              </Field>
              <Field id={`pd-text-${i}`} label={t("highlights.text")}>
                <Textarea
                  id={`pd-text-${i}`}
                  rows={2}
                  value={p.texto}
                  onChange={(e) => patchP(i, { texto: e.target.value })}
                />
              </Field>
              <Field id={`pd-src-${i}`} label={t("highlights.source")}>
                <Input
                  id={`pd-src-${i}`}
                  value={p.fonte ?? ""}
                  onChange={(e) => patchP(i, { fonte: e.target.value || null })}
                />
              </Field>
              <Button
                className="mt-6"
                variant="ghost"
                size="icon"
                aria-label={t("highlights.remove")}
                onClick={() => setPending((l) => l.filter((_, j) => j !== i))}
              >
                <Trash2 />
              </Button>
            </div>
          ))}
        </CardContent>
      </Card>
      {save.error ? <ApiErrorAlert error={save.error} /> : null}
      <Button
        disabled={save.isPending}
        onClick={() =>
          save.mutate(
            {
              highlights: highlights.filter((h) => h.texto.trim() !== "").map(strip),
              pending: pending.filter((p) => p.titulo.trim() !== "" || p.texto.trim() !== "").map(strip),
              premise: q.data?.premise,
            },
            { onSuccess: () => toast({ tone: "success", title: t("highlights.saved") }) },
          )
        }
      >
        {t("highlights.save")}
      </Button>
    </div>
  );
}
