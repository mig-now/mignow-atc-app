import { ChevronsUpDown, LogOut } from "lucide-react";
import { useTranslation } from "react-i18next";
import { useAuth } from "@/auth/use-auth";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Badge } from "@/components/ui/badge";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";

function initials(name?: string) {
  if (!name) return "?";
  const p = name.trim().split(/\s+/);
  return ((p[0]?.[0] ?? "") + (p.length > 1 ? (p.at(-1)?.[0] ?? "") : "")).toUpperCase() || "?";
}

export function UserMenu({ collapsed = false }: { collapsed?: boolean }) {
  const { t } = useTranslation();
  const { name, email, logout, isMock } = useAuth();
  const roleLabel = t("user.role"); // só existe o papel Consultor
  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <button
          type="button"
          aria-label={t("user.menuLabel")}
          data-collapsed={collapsed}
          className="flex w-full items-center gap-2 rounded-md p-2 text-left outline-none transition-colors hover:bg-sidebar-accent/60 focus-visible:ring-2 focus-visible:ring-ring data-[collapsed=true]:justify-center"
        >
          <Avatar className="size-8">
            <AvatarFallback className="bg-primary text-xs text-primary-foreground">{initials(name)}</AvatarFallback>
          </Avatar>
          {!collapsed && (
            <>
              <div className="min-w-0 flex-1">
                <p className="truncate text-sm font-medium text-sidebar-foreground">{name ?? "—"}</p>
                <p className="truncate text-xs text-sidebar-foreground/60">{roleLabel}</p>
              </div>
              <ChevronsUpDown className="size-4 shrink-0 text-sidebar-foreground/60" />
            </>
          )}
        </button>
      </DropdownMenuTrigger>
      <DropdownMenuContent side={collapsed ? "right" : "top"} align={collapsed ? "end" : "start"} className="min-w-60">
        <div className="px-2 py-1.5">
          <p className="truncate text-sm font-medium">{name ?? "—"}</p>
          <p className="truncate text-xs text-muted-foreground">{email ?? "—"}</p>
          <Badge tone="primary" className="mt-1.5">
            {roleLabel}
          </Badge>
        </div>
        {!isMock && (
          <>
            <DropdownMenuSeparator />
            <DropdownMenuItem onSelect={() => logout()}>
              <LogOut />
              {t("user.signOut")}
            </DropdownMenuItem>
          </>
        )}
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
