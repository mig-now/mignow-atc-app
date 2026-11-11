import type { ComponentProps } from "react";
import { cn } from "@/lib/cn";

export function Input({ className, ...props }: ComponentProps<"input">) {
  return (
    <input
      className={cn(
        "field-focus h-9 w-full rounded-md border border-input bg-card/50 px-3 text-sm outline-none transition-colors placeholder:text-muted-foreground disabled:opacity-60",
        className,
      )}
      {...props}
    />
  );
}

export function Textarea({ className, ...props }: ComponentProps<"textarea">) {
  return (
    <textarea
      className={cn(
        "field-focus min-h-20 w-full rounded-md border border-input bg-card/50 px-3 py-2 text-sm outline-none transition-colors placeholder:text-muted-foreground",
        className,
      )}
      {...props}
    />
  );
}

export function NativeSelect({ className, ...props }: ComponentProps<"select">) {
  return (
    <select
      className={cn(
        "field-focus h-9 w-full rounded-md border border-input bg-card px-2 text-sm outline-none",
        className,
      )}
      {...props}
    />
  );
}

export function Checkbox({ className, ...props }: Omit<ComponentProps<"input">, "type">) {
  return (
    <input
      type="checkbox"
      className={cn("size-4 rounded border-input accent-[var(--primary)]", className)}
      {...props}
    />
  );
}
