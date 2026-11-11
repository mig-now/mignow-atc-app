import { useMsal } from "@azure/msal-react";
import { useTranslation } from "react-i18next";
import { Navigate } from "react-router";
import { isEntraConfigured, loginRequest } from "@/auth/msal";
import { useAuth } from "@/auth/use-auth";
import { BrandSymbol } from "@/components/layout/brand";
import { Button } from "@/components/ui/button";

/** Única tela pública: pouso pós-logout e "Entrar" manual (o login automático é do RequireAuth). */
export function LoginPage() {
  const { t } = useTranslation();
  const { instance } = useMsal();
  const { isAuthenticated } = useAuth();
  if (isAuthenticated) return <Navigate to="/" replace />;
  return (
    <main className="flex min-h-dvh items-center justify-center p-6">
      <div className="w-full max-w-sm rounded-lg border border-border bg-card p-8 text-center">
        <BrandSymbol className="mx-auto size-12" />
        <h1 className="mt-4 text-xl font-semibold">{t("login.title")}</h1>
        <p className="mt-1 text-sm text-muted-foreground">{t("login.subtitle")}</p>
        <Button
          className="mt-6 w-full"
          disabled={!isEntraConfigured}
          onClick={() => void instance.loginRedirect(loginRequest)}
        >
          {t("user.signIn")}
        </Button>
      </div>
    </main>
  );
}
