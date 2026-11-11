import { InteractionRequiredAuthError, type IPublicClientApplication } from "@azure/msal-browser";
import { MsalProvider, useMsal } from "@azure/msal-react";
import { type ReactNode, useEffect } from "react";
import { setTokenProvider } from "@/api/client";
import { setHubTokenProvider } from "@/api/token";
import { createMsalInstance, loginRequest } from "./msal";

const msalInstance: IPublicClientApplication | null = typeof window === "undefined" ? null : createMsalInstance();

function TokenBridge({ children }: { children: ReactNode }) {
  const { instance, accounts } = useMsal();
  useEffect(() => {
    const provider = async () => {
      const account = instance.getActiveAccount() ?? accounts[0] ?? null;
      if (!account) return null;
      try {
        return (await instance.acquireTokenSilent({ ...loginRequest, account })).accessToken;
      } catch (err) {
        // NÃO dispare redirect daqui (perderia o estado da tela). Sem token → 401 → handler global.
        if (err instanceof InteractionRequiredAuthError) return null;
        return null;
      }
    };
    setTokenProvider(provider);
    setHubTokenProvider(provider);
  }, [instance, accounts]);
  return <>{children}</>;
}

export function AuthProvider({ children }: { children: ReactNode }) {
  if (!msalInstance) return <>{children}</>;
  return (
    <MsalProvider instance={msalInstance}>
      <TokenBridge>{children}</TokenBridge>
    </MsalProvider>
  );
}
