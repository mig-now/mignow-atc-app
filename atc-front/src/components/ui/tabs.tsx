import * as TabsPrimitive from "@radix-ui/react-tabs";
import { type ComponentProps, useRef, useState } from "react";
import { cn } from "@/lib/cn";

/**
 * Abas que preservam a rolagem: ao trocar de aba, a altura já ocupada vira altura mínima. Sem isso, a aba nova (ou o "carregando"
 * dela) encolhe a página e o navegador sobe a barra de rolagem até o topo.
 */
export function Tabs({ onValueChange, style, ...props }: ComponentProps<typeof TabsPrimitive.Root>) {
  const ref = useRef<HTMLDivElement>(null);
  const [minHeight, setMinHeight] = useState<number>();
  return (
    <TabsPrimitive.Root
      {...props}
      ref={ref}
      style={{ ...style, minHeight }}
      onValueChange={(value) => {
        const height = ref.current?.offsetHeight ?? 0;
        if (height > 0) setMinHeight((m) => Math.max(m ?? 0, height));
        onValueChange?.(value);
      }}
    />
  );
}

export function TabsList({ className, ...props }: ComponentProps<typeof TabsPrimitive.List>) {
  return (
    <TabsPrimitive.List
      className={cn("inline-flex items-center gap-1 rounded-md bg-muted p-1", className)}
      {...props}
    />
  );
}

export function TabsTrigger({ className, ...props }: ComponentProps<typeof TabsPrimitive.Trigger>) {
  return (
    <TabsPrimitive.Trigger
      className={cn(
        "rounded-sm px-3 py-1 text-sm text-muted-foreground transition-colors data-[state=active]:bg-background data-[state=active]:text-foreground data-[state=active]:shadow-sm",
        className,
      )}
      {...props}
    />
  );
}

export const TabsContent = TabsPrimitive.Content;
