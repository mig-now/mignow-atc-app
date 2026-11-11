import { useState } from "react";
import { useTranslation } from "react-i18next";
import { Link } from "react-router";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input, NativeSelect } from "@/components/ui/input";
import { Loading } from "@/components/ui/loading";
import { Table, TBody, Td, THead, Th, Tr } from "@/components/ui/table";
import { ApiErrorAlert } from "@/features/shared/api-error-alert";
import { EmptyState } from "@/features/shared/empty-state";
import { PageHeader } from "@/features/shared/page-header";
import { Pagination } from "@/features/shared/pagination";
import { formatNumber } from "@/lib/format";
import { useSearchState } from "@/lib/use-search-state";
import { KbLookupPanel } from "../components/KbLookupPanel";
import { useKbIndex } from "../hooks/useKb";

export const KB_CATEGORIES = ["tabelas", "funcoes", "classes", "tipos", "transacoes", "programas", "fatos"];
const PAGE_SIZE = 50;
const DEFAULTS = { q: "", category: "", page: "1", ref: "", robt: "", top: "3" };

/** Índice da KB: busca/categoria (estado na URL), padrões P01..P99, lookup rápido e lista paginada de elementos. */
export function KbIndexPage() {
  const { t, i18n } = useTranslation("kb");
  const lang = i18n.language;
  const [s, set] = useSearchState(DEFAULTS);
  const [draft, setDraft] = useState(s.q);
  const index = useKbIndex({ category: s.category, search: s.q });
  const page = Math.max(1, Number(s.page) || 1);
  const elements = index.data?.elements ?? [];
  const slice = elements.slice((page - 1) * PAGE_SIZE, page * PAGE_SIZE);

  return (
    <>
      <PageHeader title={t("title")} description={t("description")} />
      <div className="space-y-4">
        <KbLookupPanel
          key={`${s.ref}|${s.robt}|${s.top}`}
          reference={s.ref}
          robt={s.robt}
          top={Number(s.top) || 3}
          onSubmit={(v) => set({ ref: v.ref, robt: v.robt, top: String(v.top) })}
        />

        <Card>
          <CardHeader>
            <CardTitle>{t("elements.title")}</CardTitle>
          </CardHeader>
          <CardContent className="space-y-3">
            <form
              className="flex flex-wrap items-center gap-2"
              onSubmit={(e) => {
                e.preventDefault();
                set({ q: draft.trim() });
              }}
            >
              <Input
                className="max-w-sm"
                value={draft}
                placeholder={t("elements.searchPlaceholder")}
                aria-label={t("elements.search")}
                onChange={(e) => setDraft(e.target.value)}
              />
              <NativeSelect
                className="w-48"
                aria-label={t("elements.category")}
                value={s.category}
                onChange={(e) => set({ category: e.target.value })}
              >
                <option value="">{t("elements.allCategories")}</option>
                {KB_CATEGORIES.map((c) => (
                  <option key={c} value={c}>
                    {t(`category.${c}`)}
                  </option>
                ))}
              </NativeSelect>
              <Button type="submit">{t("elements.searchAction")}</Button>
            </form>
            {index.isLoading ? (
              <Loading />
            ) : index.error ? (
              <ApiErrorAlert error={index.error} />
            ) : slice.length === 0 ? (
              <EmptyState text={t("elements.empty")} />
            ) : (
              <div className="overflow-x-auto rounded-md border border-border">
                <Table>
                  <THead>
                    <Tr>
                      <Th>{t("elements.element")}</Th>
                      <Th>{t("elements.category")}</Th>
                      <Th className="text-right">{t("elements.cards")}</Th>
                      <Th className="text-right">{t("elements.contributions")}</Th>
                      <Th className="text-right">{t("elements.facts")}</Th>
                    </Tr>
                  </THead>
                  <TBody>
                    {slice.map((e) => (
                      <Tr key={`${e.category}/${e.slug}`}>
                        <Td>
                          <Link
                            className="font-mono text-sm underline-offset-2 hover:underline"
                            to={`/kb/${e.category}/${encodeURIComponent(e.slug)}`}
                          >
                            {e.element}
                          </Link>
                        </Td>
                        <Td>
                          <Badge>{t(`category.${e.category}`, { defaultValue: e.category })}</Badge>
                        </Td>
                        <Td className="text-right">{formatNumber(e.cards, lang)}</Td>
                        <Td className="text-right">{formatNumber(e.contributions, lang)}</Td>
                        <Td className="text-right">{formatNumber(e.facts, lang)}</Td>
                      </Tr>
                    ))}
                  </TBody>
                </Table>
                <Pagination
                  page={page}
                  pageSize={PAGE_SIZE}
                  total={elements.length}
                  onPage={(p) => set({ page: String(p) })}
                />
              </div>
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>{t("patterns.title")}</CardTitle>
          </CardHeader>
          <CardContent>
            {!index.data || index.data.patterns.length === 0 ? (
              <EmptyState text={t("patterns.empty")} />
            ) : (
              <div className="overflow-x-auto rounded-md border border-border">
                <Table>
                  <THead>
                    <Tr>
                      <Th>{t("patterns.id")}</Th>
                      <Th>{t("patterns.description")}</Th>
                      <Th className="text-right">{t("patterns.solutions")}</Th>
                      <Th className="text-right">{t("patterns.occurrences")}</Th>
                    </Tr>
                  </THead>
                  <TBody>
                    {index.data.patterns.map((p) => (
                      <Tr key={p.id}>
                        <Td className="font-mono">{p.id}</Td>
                        <Td>{p.description}</Td>
                        <Td className="text-right">{formatNumber(p.solutions, lang)}</Td>
                        <Td className="text-right">{formatNumber(p.occurrences, lang)}</Td>
                      </Tr>
                    ))}
                  </TBody>
                </Table>
              </div>
            )}
          </CardContent>
        </Card>
      </div>
    </>
  );
}
