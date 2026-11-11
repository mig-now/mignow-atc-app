import { useTranslation } from "react-i18next";
import { NavLink, Outlet } from "react-router";
import { PageHeader } from "@/features/shared/page-header";
import { cn } from "@/lib/cn";

const TABS = [
  ["clientes", "tabs.customers"],
  ["ambientes", "tabs.sapSystems"],
  ["ia", "tabs.ai"],
  ["parametros", "tabs.settings"],
  ["execucoes", "tabs.runs"],
] as const;

/** Engrenagem ⚙: cadastros e parâmetros (qualquer Consultor). Abas = sub-rotas de /config. */
export function ConfigLayout() {
  const { t } = useTranslation("config");
  return (
    <>
      <PageHeader title={t("title")} />
      <nav aria-label={t("navLabel")} className="mb-5 flex flex-wrap gap-1 border-b border-border">
        {TABS.map(([to, key]) => (
          <NavLink
            key={to}
            to={to}
            className={({ isActive }) =>
              cn(
                "-mb-px border-b-2 px-3 py-2 text-sm transition-colors",
                isActive
                  ? "border-primary font-medium text-foreground"
                  : "border-transparent text-muted-foreground hover:text-foreground",
              )
            }
          >
            {t(key)}
          </NavLink>
        ))}
      </nav>
      <Outlet />
    </>
  );
}
