import "../src/styles/app.css";
import "@/i18n/zod-error-map"; // registra i18n + error map do Zod antes do render
import { QueryClientProvider } from "@tanstack/react-query";
import { type ReactNode, useEffect, useRef, useState } from "react";
import { I18nextProvider } from "react-i18next";
import {
  Links,
  Meta,
  type MetaDescriptor,
  Outlet,
  Scripts,
  ScrollRestoration,
  useNavigate,
  useRouteError,
} from "react-router";
import { makeQueryClient } from "@/api/query-client";
import { AuthProvider } from "@/auth/auth-provider";
import { ThemeProvider } from "@/components/theme-provider";
import { ToastProvider } from "@/components/ui/toast";
import i18n from "@/i18n/config";

// BLOQUEANTE (antes de <Links/>): aplica .dark/.light no <html> antes do 1º paint. Claro é o padrão (escuro só se o usuário escolheu).
const THEME_INIT_SCRIPT =
  "(function(){try{var t=localStorage.getItem('migatc-theme')==='dark'?'dark':'light';var r=document.documentElement;r.classList.remove('light','dark');r.classList.add(t)}catch(e){}})()";

export function meta(): MetaDescriptor[] {
  return [{ title: "MigATC" }];
}

function Document({ children }: { children: ReactNode }) {
  return (
    <html lang="pt-BR" suppressHydrationWarning>
      <head>
        <meta charSet="utf-8" />
        <meta name="viewport" content="width=device-width, initial-scale=1" />
        {/* biome-ignore lint/security/noDangerouslySetInnerHtml: tema init — string literal constante */}
        <script dangerouslySetInnerHTML={{ __html: THEME_INIT_SCRIPT }} />
        <Meta />
        <Links />
      </head>
      <body suppressHydrationWarning>
        {children}
        <ScrollRestoration />
        <Scripts />
      </body>
    </html>
  );
}

export function Layout({ children }: { children: ReactNode }) {
  return <Document>{children}</Document>;
}

export default function Root() {
  const navigate = useNavigate();
  const navigateRef = useRef(navigate);
  useEffect(() => {
    navigateRef.current = navigate;
  }, [navigate]);

  // QueryClient criado UMA vez; o 401 global navega em SPA até /login (preserva a sessão MSAL).
  const [queryClient] = useState(() =>
    makeQueryClient(() => {
      if (window.location.pathname === "/login") return;
      const from = `${window.location.pathname}${window.location.search}`;
      navigateRef.current("/login", { state: { from }, replace: true });
    }),
  );

  return (
    <I18nextProvider i18n={i18n}>
      <AuthProvider>
        <QueryClientProvider client={queryClient}>
          <ThemeProvider>
            <ToastProvider>
              <Outlet />
            </ToastProvider>
          </ThemeProvider>
        </QueryClientProvider>
      </AuthProvider>
    </I18nextProvider>
  );
}

export function HydrateFallback() {
  return null;
}

export function ErrorBoundary() {
  const error = useRouteError();
  console.error(error);
  return (
    <main className="p-8">
      <h1 className="text-lg font-semibold">{i18n.t("errors:unexpected")}</h1>
    </main>
  );
}
