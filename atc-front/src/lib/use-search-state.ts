import { useCallback } from "react";
import { useSearchParams } from "react-router";

/** Estado de tabela na URL (página, busca, filtros): não quebra link nem o voltar do navegador. */
export function useSearchState<K extends string>(defaults: Record<K, string>) {
  const [params, setParams] = useSearchParams();
  const values = Object.fromEntries(Object.entries(defaults).map(([k, d]) => [k, params.get(k) ?? d])) as Record<
    K,
    string
  >;
  const set = useCallback(
    (patch: Partial<Record<K, string>>) =>
      setParams(
        (prev) => {
          const next = new URLSearchParams(prev);
          for (const [k, v] of Object.entries(patch) as [K, string | undefined][]) {
            // Só o valor padrão sai da URL: "" num filtro com padrão (ex.: "Todas as situações") fica como `status=`.
            const value = v ?? defaults[k];
            if (value === defaults[k]) next.delete(k);
            else next.set(k, value);
          }
          if (!("page" in patch)) next.delete("page");
          return next;
        },
        { replace: true, preventScrollReset: true },
      ),
    [setParams, defaults],
  );
  return [values, set] as const;
}
