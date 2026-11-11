import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { useQueryClient } from "@tanstack/react-query";
import { useEffect } from "react";
import { env } from "@/lib/env";
import { getAccessTokenForHub } from "./token";

/**
 * Progresso ao vivo da execução (SignalR `/hubs/runs`, grupo por runId via `JoinRun`).
 * Eventos `RunChanged` e `StepChanged`: a cada um, invalida as consultas informadas (e chama `onEvent`, se houver).
 * Falha de conexão não é erro: as telas também fazem polling enquanto a execução está ativa.
 */
export function useRunLiveUpdates(
  runId: number | string | undefined,
  active: boolean,
  queryKeys: readonly (readonly unknown[])[],
  onEvent?: (event: "RunChanged" | "StepChanged") => void,
) {
  const queryClient = useQueryClient();
  // biome-ignore lint/correctness/useExhaustiveDependencies: as chaves e o callback são derivados do runId
  useEffect(() => {
    if (runId === undefined || !active || typeof window === "undefined") return;
    const connection = new HubConnectionBuilder()
      .withUrl(`${env.VITE_API_URL}/hubs/runs`, {
        accessTokenFactory: async () => (await getAccessTokenForHub()) ?? "",
      })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build();
    const refresh = (event: "RunChanged" | "StepChanged") => () => {
      for (const key of queryKeys) void queryClient.invalidateQueries({ queryKey: key });
      onEvent?.(event);
    };
    connection.on("RunChanged", refresh("RunChanged"));
    connection.on("StepChanged", refresh("StepChanged"));
    let stopped = false;
    connection
      .start()
      .then(() => (stopped ? undefined : connection.invoke("JoinRun", runId)))
      .catch(() => undefined);
    return () => {
      stopped = true;
      void connection.stop();
    };
  }, [runId, active, queryClient]);
}
