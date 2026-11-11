import { useIsAuthenticated, useMsal } from "@azure/msal-react";
import { isEntraConfigured } from "@/auth/msal";

const DEV_MOCK_USER = { oid: "dev-mock-oid", name: "Usuário Dev", email: "dev@migatc.local" } as const;

/** Claims só para EXIBIÇÃO; a autoria de auditoria deriva do JWT no backend, nunca do front. */
export function useAuth() {
  const { instance, accounts } = useMsal();
  const isAuthenticated = useIsAuthenticated();
  const account = instance.getActiveAccount() ?? accounts[0] ?? null;
  if (import.meta.env.DEV && !account && !isEntraConfigured)
    return { ...DEV_MOCK_USER, isAuthenticated: true, isMock: true, logout: () => {} };
  const claims = account?.idTokenClaims as { oid?: string; name?: string } | undefined;
  return {
    oid: claims?.oid,
    name: claims?.name ?? account?.name,
    email: account?.username,
    isAuthenticated,
    isMock: false,
    logout: () => {
      void instance.logoutRedirect();
    },
  };
}
