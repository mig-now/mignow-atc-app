import type { AuthenticationResult } from "@azure/msal-browser";
import { EventType, PublicClientApplication } from "@azure/msal-browser";
import { env } from "@/lib/env";

// Entra "configurado" = GUIDs reais (não os placeholders 00000000-…). Sem isso, usuário mock de DEV.
const PLACEHOLDER_GUID = /^00000000-/;
export const isEntraConfigured =
  !PLACEHOLDER_GUID.test(env.VITE_ENTRA_CLIENT_ID) && !PLACEHOLDER_GUID.test(env.VITE_ENTRA_TENANT_ID);

export function createMsalInstance(): PublicClientApplication {
  const instance = new PublicClientApplication({
    auth: {
      clientId: env.VITE_ENTRA_CLIENT_ID,
      authority: `https://login.microsoftonline.com/${env.VITE_ENTRA_TENANT_ID}`,
      redirectUri: typeof window !== "undefined" ? window.location.origin : undefined,
      postLogoutRedirectUri: "/login",
    },
    // sessionStorage: o redirect é navegação full-page e o cache precisa sobreviver. NÃO usar localStorage.
    cache: { cacheLocation: "sessionStorage" },
  });
  instance.addEventCallback((e) => {
    if (e.eventType === EventType.LOGIN_SUCCESS && e.payload) {
      const acc = (e.payload as AuthenticationResult).account;
      if (acc) instance.setActiveAccount(acc);
    }
  });
  return instance;
}

export const loginRequest = { scopes: [env.VITE_ENTRA_API_SCOPE] };
