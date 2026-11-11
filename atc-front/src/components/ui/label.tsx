import type { ComponentProps } from "react";
import { cn } from "@/lib/cn";

export function Label({ className, ...props }: ComponentProps<"label">) {
  // biome-ignore lint/a11y/noLabelWithoutControl: o controle é associado por htmlFor em quem usa
  return <label className={cn("text-sm font-medium text-foreground", className)} {...props} />;
}
