import { InteractionStatus, InteractionType } from "@azure/msal-browser";
import { useMsal, useMsalAuthentication } from "@azure/msal-react";
import { useEffect } from "react";
import { Navigate, Outlet, useLocation } from "react-router";
import { isEntraConfigured, loginRequest } from "@/auth/msal";
import { useAuth } from "@/auth/use-auth";
import { Loading } from "@/components/ui/loading";

// ⚠️ Anti-loop (blueprint §8.4): ÚNICO ponto de disparo do login + circuit breaker em sessionStorage.
const AUTO_LOGIN_KEY = "migatc-autologin-attempts";
const AUTO_LOGIN_WINDOW_MS = 60_000;
const AUTO_LOGIN_MAX_ATTEMPTS = 2;

function recentAttempts(): number[] {
  try {
    const raw = window.sessionStorage.getItem(AUTO_LOGIN_KEY);
    const list = raw ? (JSON.parse(raw) as number[]) : [];
    return list.filter((t) => Date.now() - t < AUTO_LOGIN_WINDOW_MS);
  } catch {
    return [];
  }
}

function canAutoLogin() {
  return recentAttempts().length < AUTO_LOGIN_MAX_ATTEMPTS;
}

function recordAutoLoginAttempt() {
  try {
    window.sessionStorage.setItem(AUTO_LOGIN_KEY, JSON.stringify([...recentAttempts(), Date.now()]));
  } catch {
    /* storage bloqueado */
  }
}

function clearAutoLoginAttempts() {
  try {
    window.sessionStorage.removeItem(AUTO_LOGIN_KEY);
  } catch {
    /* storage bloqueado */
  }
}

function AutoLogin() {
  const location = useLocation();
  const { error } = useMsalAuthentication(InteractionType.Redirect, {
    ...loginRequest,
    redirectStartPage:
      typeof window !== "undefined" ? window.location.origin + location.pathname + location.search : undefined,
  });
  useEffect(() => {
    if (!error) recordAutoLoginAttempt();
  }, [error]);
  if (error) {
    console.error("Auto-login falhou:", error.errorCode); // nunca logar token
    return <Navigate to="/login" state={{ from: `${location.pathname}${location.search}` }} replace />;
  }
  return <Loading />;
}

export function RequireAuth() {
  const { inProgress } = useMsal();
  const { isAuthenticated } = useAuth();
  const location = useLocation();
  useEffect(() => {
    if (isAuthenticated) clearAutoLoginAttempts();
  }, [isAuthenticated]);

  if (inProgress !== InteractionStatus.None) return <Loading />; // NUNCA navegar aqui (processa o #code=)
  if (isAuthenticated) return <Outlet />;
  if (!isEntraConfigured) return <Navigate to="/login" replace />;
  if (!canAutoLogin())
    return <Navigate to="/login" state={{ from: `${location.pathname}${location.search}` }} replace />;
  return <AutoLogin />;
}
