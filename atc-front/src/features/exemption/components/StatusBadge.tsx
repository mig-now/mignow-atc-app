import { useTranslation } from "react-i18next";
import { Badge } from "@/components/ui/badge";
import { STATUS_TONE } from "../lib";

/** Status do pedido/lote (`SIMULADO`, `PEDIDO`…); o rótulo vem de `exemption:status.<CÓDIGO>`. */
export function StatusBadge({ status }: { status: string }) {
  const { t, i18n } = useTranslation("exemption");
  const key = `status.${status}`;
  return <Badge tone={STATUS_TONE[status] ?? "neutral"}>{i18n.exists(`exemption:${key}`) ? t(key) : status}</Badge>;
}
