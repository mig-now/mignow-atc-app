import { createContext, type ReactNode, useContext, useEffect, useState } from "react";

type Theme = "dark" | "light";
const ThemeContext = createContext<{ theme: Theme; toggle: () => void; setTheme: (t: Theme) => void } | null>(null);

/** Fonte única da classe de tema no <html> (o script anti-FOUC do root aplica antes do 1º paint). */
export function ThemeProvider({ children }: { children: ReactNode }) {
  const [theme, setThemeState] = useState<Theme>(() => {
    if (typeof window === "undefined") return "light";
    try {
      return localStorage.getItem("migatc-theme") === "dark" ? "dark" : "light"; // claro é o padrão
    } catch {
      return "light";
    }
  });
  const setTheme = (next: Theme) => {
    setThemeState(next);
    try {
      localStorage.setItem("migatc-theme", next);
    } catch {
      /* storage bloqueado */
    }
  };
  const toggle = () => setTheme(theme === "light" ? "dark" : "light");
  useEffect(() => {
    const r = document.documentElement;
    r.classList.remove("light", "dark");
    r.classList.add(theme);
  }, [theme]);
  return <ThemeContext.Provider value={{ theme, toggle, setTheme }}>{children}</ThemeContext.Provider>;
}

export function useTheme() {
  const c = useContext(ThemeContext);
  if (!c) throw new Error("useTheme fora do ThemeProvider");
  return c;
}
