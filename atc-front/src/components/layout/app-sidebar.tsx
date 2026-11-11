import {
  BookOpen,
  Building2,
  Cpu,
  Download,
  FileText,
  Filter,
  LayoutGrid,
  ListChecks,
  type LucideIcon,
  ScanSearch,
  Server,
  Settings,
  ShieldOff,
  SlidersHorizontal,
  Wrench,
} from "lucide-react";
import { useTranslation } from "react-i18next";
import { Link, NavLink } from "react-router";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { cn } from "@/lib/cn";
import { BrandSymbol } from "./brand";
import { UserMenu } from "./user-menu";

type Item = { to: string; key: string; end?: boolean; Icon: LucideIcon };

/** Módulos do menu lateral (§9 do desenho). */
const MODULES: Item[] = [
  { to: "/", key: "nav.home", end: true, Icon: LayoutGrid },
  { to: "/extracao", key: "nav.extraction", Icon: Download },
  { to: "/remediacao", key: "nav.remediation", Icon: Wrench },
  { to: "/isencao", key: "nav.exemption", Icon: ShieldOff },
  { to: "/varredura", key: "nav.scan", Icon: ScanSearch },
  { to: "/relatorio", key: "nav.report", Icon: FileText },
  { to: "/regras-triagem", key: "nav.triageRules", Icon: Filter },
  { to: "/kb", key: "nav.kb", Icon: BookOpen },
];

/** Engrenagem ⚙ do rodapé. */
const CONFIG: Item[] = [
  { to: "/config/clientes", key: "nav.config.customers", Icon: Building2 },
  { to: "/config/ambientes", key: "nav.config.sapSystems", Icon: Server },
  { to: "/config/ia", key: "nav.config.ai", Icon: Cpu },
  { to: "/config/parametros", key: "nav.config.settings", Icon: SlidersHorizontal },
  { to: "/config/execucoes", key: "nav.config.runs", Icon: ListChecks },
];

const linkClass = (collapsed: boolean, active: boolean) =>
  cn(
    "flex items-center gap-3 rounded-md py-2 text-sm transition-colors",
    collapsed ? "justify-center px-0" : "px-3",
    active
      ? "bg-sidebar-accent font-medium text-sidebar-accent-foreground"
      : "text-sidebar-foreground/70 hover:bg-sidebar-accent/50 hover:text-sidebar-foreground",
  );

function NavItem({ item, collapsed, onNavigate }: { item: Item; collapsed: boolean; onNavigate?: () => void }) {
  const { t } = useTranslation();
  return (
    <NavLink
      to={item.to}
      end={item.end}
      onClick={onNavigate}
      title={collapsed ? t(item.key) : undefined}
      className={({ isActive }) => linkClass(collapsed, isActive)}
    >
      <item.Icon className="size-4 shrink-0" />
      {collapsed ? <span className="sr-only">{t(item.key)}</span> : t(item.key)}
    </NavLink>
  );
}

function ConfigMenu({ collapsed, onNavigate }: { collapsed: boolean; onNavigate?: () => void }) {
  const { t } = useTranslation();
  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <button
          type="button"
          aria-label={t("nav.config")}
          title={collapsed ? t("nav.config") : undefined}
          className={cn("w-full", linkClass(collapsed, false))}
        >
          <Settings className="size-4 shrink-0" />
          {!collapsed && t("nav.config")}
        </button>
      </DropdownMenuTrigger>
      <DropdownMenuContent side={collapsed ? "right" : "top"} align="start" className="min-w-52">
        {CONFIG.map(({ to, key, Icon }) => (
          <DropdownMenuItem key={to} asChild>
            <Link to={to} onClick={onNavigate}>
              <Icon />
              {t(key)}
            </Link>
          </DropdownMenuItem>
        ))}
      </DropdownMenuContent>
    </DropdownMenu>
  );
}

export function AppSidebar({
  className,
  collapsed = false,
  onNavigate,
}: {
  className?: string;
  collapsed?: boolean;
  onNavigate?: () => void;
}) {
  const { t } = useTranslation();
  return (
    <div
      className={cn(
        "flex h-dvh flex-col border-r border-sidebar-border bg-sidebar text-sidebar-foreground transition-[width] duration-200",
        collapsed ? "w-16" : "w-64",
        className,
      )}
    >
      <div className={cn("flex h-16 shrink-0 items-center", collapsed ? "justify-center px-0" : "px-5")}>
        <NavLink to="/" onClick={onNavigate} className="flex items-center gap-2" aria-label={t("nav.brandHome")}>
          <BrandSymbol className="size-8 shrink-0" />
          {!collapsed && (
            <span className="font-sans text-2xl font-bold tracking-tight">
              <span className="text-sidebar-foreground">Mig</span>
              <span className="text-sidebar-primary">ATC</span>
            </span>
          )}
        </NavLink>
      </div>
      <nav aria-label={t("nav.main")} className="flex-1 space-y-1 overflow-y-auto px-3 py-2">
        {MODULES.map((m) => (
          <NavItem key={m.to} item={m} collapsed={collapsed} onNavigate={onNavigate} />
        ))}
      </nav>
      <div className={cn("shrink-0 space-y-1 border-t border-sidebar-border", collapsed ? "p-2" : "p-3")}>
        <ConfigMenu collapsed={collapsed} onNavigate={onNavigate} />
        <UserMenu collapsed={collapsed} />
      </div>
    </div>
  );
}
