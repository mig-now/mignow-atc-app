import { useTranslation } from "react-i18next";
import { cn } from "@/lib/cn";

const LANGUAGES = [
  { code: "pt-BR", label: "PT" },
  { code: "en", label: "EN" },
  { code: "es", label: "ES" },
] as const;
const base = (code: string) => code.split("-")[0];

export function LanguageSwitcher() {
  const { t, i18n } = useTranslation();
  return (
    // biome-ignore lint/a11y/useSemanticElements: grupo de botões toggle — role="group" + aria-label é o ARIA adequado.
    <div role="group" aria-label={t("a11y.language")} className="flex items-center gap-0.5 rounded-full bg-muted p-0.5">
      {LANGUAGES.map((lang) => {
        const active = base(i18n.language) === base(lang.code);
        return (
          <button
            key={lang.code}
            type="button"
            aria-pressed={active}
            onClick={() => {
              if (!active) void i18n.changeLanguage(lang.code);
            }}
            className={cn(
              "rounded-full px-2.5 py-1 text-xs font-medium transition-colors",
              active ? "bg-background text-foreground shadow-sm" : "text-muted-foreground hover:text-foreground",
            )}
          >
            {lang.label}
          </button>
        );
      })}
    </div>
  );
}
