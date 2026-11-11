import { Menu, PanelLeft } from "lucide-react";
import { useTranslation } from "react-i18next";
import { LanguageSwitcher } from "./language-switcher";
import { ThemeToggle } from "./theme-toggle";

export function Topbar({
  onToggleSidebar,
  onToggleCollapse,
}: {
  onToggleSidebar: () => void;
  onToggleCollapse: () => void;
}) {
  const { t } = useTranslation();
  return (
    <header className="sticky top-0 z-30 flex h-16 items-center gap-2 border-b border-border bg-background/80 px-3 backdrop-blur sm:gap-3 sm:px-4">
      <button
        type="button"
        onClick={onToggleSidebar}
        aria-label={t("nav.openMenu")}
        className="inline-flex size-9 items-center justify-center rounded-md text-muted-foreground transition-colors hover:bg-accent hover:text-foreground md:hidden"
      >
        <Menu className="size-5" />
      </button>
      <button
        type="button"
        onClick={onToggleCollapse}
        aria-label={t("nav.toggleMenu")}
        title={t("nav.toggleMenu")}
        className="hidden size-9 items-center justify-center rounded-md text-muted-foreground transition-colors hover:bg-accent hover:text-foreground md:inline-flex"
      >
        <PanelLeft className="size-5" />
      </button>
      <div className="ml-auto flex items-center gap-2 sm:gap-3">
        <ThemeToggle />
        <LanguageSwitcher />
      </div>
    </header>
  );
}
