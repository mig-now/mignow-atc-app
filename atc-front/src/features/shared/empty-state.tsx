import { Inbox } from "lucide-react";

export function EmptyState({ text }: { text: string }) {
  return (
    <div className="flex flex-col items-center gap-2 p-10 text-sm text-muted-foreground">
      <Inbox className="size-6" />
      {text}
    </div>
  );
}
