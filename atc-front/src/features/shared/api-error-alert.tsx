import { useTranslation } from "react-i18next";
import { isApiError } from "@/api/client";
import { Alert } from "@/components/ui/alert";

/** Erro da API: o título já vem localizado do backend (Accept-Language); sem resposta, mensagem de rede. */
export function ApiErrorAlert({ error }: { error: unknown }) {
  const { t } = useTranslation();
  if (!error) return null;
  const title = isApiError(error) ? error.title : t("errors:network");
  return <Alert tone="danger" title={title} />;
}

export function errorTitle(error: unknown, fallback: string) {
  return isApiError(error) ? error.title : fallback;
}
