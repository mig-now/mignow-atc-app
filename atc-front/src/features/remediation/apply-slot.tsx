import { lazy, Suspense } from "react";
import { Loading } from "@/components/ui/loading";

/** Ponto de montagem da aba "Propostas e aplicação" (implementada em `./apply`, por outro agente). */
const SessionApplyTab = lazy(() => import("./apply").then((m) => ({ default: m.SessionApplyTab })));

export function ApplySlot({ sessionId }: { sessionId: string }) {
  return (
    <Suspense fallback={<Loading />}>
      <SessionApplyTab sessionId={sessionId} />
    </Suspense>
  );
}
