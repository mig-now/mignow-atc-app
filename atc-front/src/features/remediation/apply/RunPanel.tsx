import { useEffect, useRef } from "react";
import type { RunResponse } from "@/api/generated/atc/model";
import { RunMonitorPanel, useRunWatcher } from "@/features/runs";

/**
 * Painel de um Run disparado pela aba: monitor completo + callback único quando o run termina
 * (concluído, falho ou cancelado) e `children(run)` para o resultado.
 */
export function RunPanel({
  runId,
  onDone,
  children,
  onRetried,
}: {
  runId: string;
  onDone?: (run: RunResponse) => void;
  children?: (run: RunResponse) => React.ReactNode;
  onRetried?: (run: RunResponse) => void;
}) {
  const w = useRunWatcher(runId);
  const notified = useRef<string | null>(null);
  const done = w.run && !w.active ? w.run : undefined;
  useEffect(() => {
    if (done && notified.current !== done.id) {
      notified.current = done.id;
      onDone?.(done);
    }
  }, [done, onDone]);
  return (
    <div className="space-y-4">
      <RunMonitorPanel runId={runId} onRetried={onRetried} />
      {done && children?.(done)}
    </div>
  );
}
