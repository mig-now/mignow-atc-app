import { useTranslation } from "react-i18next";
import { Alert } from "@/components/ui/alert";

/** A varredura é opcional e só extrai: nunca altera os objetos encontrados. */
export function ScanOptionalNotice() {
  const { t } = useTranslation("scan");
  return (
    <Alert tone="info" title={t("notice.title")}>
      <p>{t("notice.optional")}</p>
      <p>{t("notice.readOnly")}</p>
    </Alert>
  );
}
