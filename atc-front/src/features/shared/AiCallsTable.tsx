import { useTranslation } from "react-i18next";
import { Table, TBody, Td, THead, Th, Tr } from "@/components/ui/table";
import { formatMoney, formatNumber } from "@/lib/format";
import type { AiCall } from "./types";

/**
 * Consumo de IA chamada a chamada (por tentativa), com o total no fim. O custo é o que o serviço informa;
 * "não informado" quando ele só devolve tokens (o total soma só os informados).
 */
export function AiCallsTable({ calls }: { calls: AiCall[] }) {
  const { t, i18n } = useTranslation();
  const lang = i18n.language;
  if (calls.length === 0) return <p className="text-sm text-muted-foreground">{t("aiCalls.empty")}</p>;
  const sum = (f: (c: AiCall) => number) => calls.reduce((acc, c) => acc + f(c), 0);
  return (
    <Table>
      <THead>
        <Tr>
          <Th>{t("aiCalls.attempt")}</Th>
          <Th>{t("aiCalls.usage")}</Th>
          <Th>{t("aiCalls.model")}</Th>
          <Th className="text-right">{t("aiCalls.input")}</Th>
          <Th className="text-right">{t("aiCalls.output")}</Th>
          <Th className="text-right">{t("aiCalls.cost")}</Th>
        </Tr>
      </THead>
      <TBody>
        {calls.map((c, i) => (
          // biome-ignore lint/suspicious/noArrayIndexKey: chamadas não têm id público
          <Tr key={i}>
            <Td>{c.attempt ?? "—"}</Td>
            <Td>{t(`aiUsage.${c.usage}`)}</Td>
            <Td className="font-mono">{c.model}</Td>
            <Td className="text-right tabular-nums">{formatNumber(c.inputTokens, lang)}</Td>
            <Td className="text-right tabular-nums">{formatNumber(c.outputTokens, lang)}</Td>
            <Td className="text-right tabular-nums">
              {c.cost == null ? t("aiCalls.costUnknown") : formatMoney(c.cost, lang)}
            </Td>
          </Tr>
        ))}
        <Tr className="font-semibold">
          <Td colSpan={3}>{t("aiCalls.total")}</Td>
          <Td className="text-right tabular-nums">
            {formatNumber(
              sum((c) => c.inputTokens),
              lang,
            )}
          </Td>
          <Td className="text-right tabular-nums">
            {formatNumber(
              sum((c) => c.outputTokens),
              lang,
            )}
          </Td>
          <Td className="text-right tabular-nums">
            {formatMoney(
              sum((c) => c.cost ?? 0),
              lang,
            )}
          </Td>
        </Tr>
      </TBody>
    </Table>
  );
}
