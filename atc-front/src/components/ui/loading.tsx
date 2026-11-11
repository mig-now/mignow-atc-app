import { Loader2 } from "lucide-react";
import { useTranslation } from "react-i18next";

export function Loading({ label }: { label?: string }) {
  const { t } = useTranslation();
  return (
    <output className="flex items-center justify-center gap-2 p-8 text-sm text-muted-foreground">
      <Loader2 className="size-4 animate-spin" />
      {label ?? t("common.loading")}
    </output>
  );
}
