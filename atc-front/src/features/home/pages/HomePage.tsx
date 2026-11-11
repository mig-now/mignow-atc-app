import { BookOpen, Download, FileText, Filter, type LucideIcon, ScanSearch, ShieldOff, Wrench } from "lucide-react";
import { useTranslation } from "react-i18next";
import { Link } from "react-router";
import { useAuth } from "@/auth/use-auth";
import { Card } from "@/components/ui/card";
import { PageHeader } from "@/features/shared/page-header";

const SHORTCUTS: { to: string; key: string; Icon: LucideIcon }[] = [
  { to: "/extracao", key: "nav.extraction", Icon: Download },
  { to: "/remediacao", key: "nav.remediation", Icon: Wrench },
  { to: "/isencao", key: "nav.exemption", Icon: ShieldOff },
  { to: "/varredura", key: "nav.scan", Icon: ScanSearch },
  { to: "/relatorio", key: "nav.report", Icon: FileText },
  { to: "/regras-triagem", key: "nav.triageRules", Icon: Filter },
  { to: "/kb", key: "nav.kb", Icon: BookOpen },
];

/** Início: atalhos dos módulos (os ambientes ficam no monitor de extração e na ⚙ Ambientes). */
export function HomePage() {
  const { t } = useTranslation();
  const { name } = useAuth();
  return (
    <>
      <PageHeader title={t("home.title", { name: name ?? "" })} description={t("home.subtitle")} />
      <h2 className="mb-2 text-base font-semibold">{t("home.modules")}</h2>
      <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
        {SHORTCUTS.map(({ to, key, Icon }) => (
          <Link key={to} to={to} className="group">
            <Card className="h-full p-4 transition-colors group-hover:border-primary/50">
              <Icon className="size-5 text-primary" />
              <h3 className="mt-3 text-base font-semibold">{t(key)}</h3>
            </Card>
          </Link>
        ))}
      </div>
    </>
  );
}
