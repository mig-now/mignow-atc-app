import { createContext, type ReactNode, useCallback, useContext, useState } from "react";
import { cn } from "@/lib/cn";

type Toast = { id: number; title: string; description?: string; tone: "success" | "error" | "info" };
const ToastContext = createContext<((t: Omit<Toast, "id">) => void) | null>(null);

/** Avisos curtos (sucesso/erro) no canto da tela. */
export function ToastProvider({ children }: { children: ReactNode }) {
  const [toasts, setToasts] = useState<Toast[]>([]);
  const push = useCallback((t: Omit<Toast, "id">) => {
    const id = Date.now() + Math.random();
    setToasts((list) => [...list, { ...t, id }]);
    setTimeout(() => setToasts((list) => list.filter((x) => x.id !== id)), 5000);
  }, []);
  return (
    <ToastContext.Provider value={push}>
      {children}
      <div aria-live="polite" className="pointer-events-none fixed right-4 bottom-4 z-[60] flex w-80 flex-col gap-2">
        {toasts.map((t) => (
          <div
            key={t.id}
            role="status"
            className={cn(
              "pointer-events-auto rounded-md border bg-popover px-4 py-3 text-sm shadow-lg",
              t.tone === "success" && "border-success/40",
              t.tone === "error" && "border-destructive/40",
            )}
          >
            <p className="font-medium">{t.title}</p>
            {t.description ? <p className="mt-0.5 text-muted-foreground">{t.description}</p> : null}
          </div>
        ))}
      </div>
    </ToastContext.Provider>
  );
}

export function useToast() {
  const c = useContext(ToastContext);
  if (!c) throw new Error("useToast fora do ToastProvider");
  return c;
}
