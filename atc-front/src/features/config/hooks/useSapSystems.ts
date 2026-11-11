import { useMutation, useQueryClient } from "@tanstack/react-query";
import type { ApiError } from "@/api/client";
import type {
  ConnectionTestResponse,
  CreateSapSystemCommand,
  SapSystemResponse,
  SapSystemsListParams,
  UpdateSapSystemRequest,
} from "@/api/generated/atc/model";
import {
  getSapSystemsListQueryKey,
  sapSystemsChangePassword,
  sapSystemsCreate,
  sapSystemsDelete,
  sapSystemsTest,
  sapSystemsUpdate,
  useSapSystemsList,
} from "@/api/generated/atc/sap-systems/sap-systems";
import { payload, unwrapped } from "@/api/unwrapped";

export function useSapSystems(params: SapSystemsListParams) {
  return unwrapped(useSapSystemsList(params));
}

const listKey = () => getSapSystemsListQueryKey().slice(0, 1);

export type SapSystemUpdateInput = UpdateSapSystemRequest;
export type SapSystemCreateInput = CreateSapSystemCommand;

/** Cria (com senha) ou altera (id presente; a senha não vai aqui). Invalida a lista no sucesso. */
export function useSaveSapSystem() {
  const queryClient = useQueryClient();
  return useMutation<
    SapSystemResponse,
    ApiError,
    { id: string; input: SapSystemUpdateInput } | { id?: undefined; input: SapSystemCreateInput }
  >({
    mutationFn: (v) =>
      v.id === undefined ? payload(sapSystemsCreate(v.input)) : payload(sapSystemsUpdate(v.id, v.input)),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: listKey() }),
  });
}

export function useDeleteSapSystem() {
  const queryClient = useQueryClient();
  return useMutation<unknown, ApiError, string>({
    mutationFn: (id) => sapSystemsDelete(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: listKey() }),
  });
}

/** Senha vai para o cofre e nunca volta pela API. */
export function useChangeSapPassword() {
  const queryClient = useQueryClient();
  return useMutation<unknown, ApiError, { id: string; password: string }>({
    mutationFn: ({ id, password }) => sapSystemsChangePassword(id, { password }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: listKey() }),
  });
}

/** Teste de conexão síncrono (o resultado também é gravado no ambiente: atualiza a lista). */
export function useTestSapConnection() {
  const queryClient = useQueryClient();
  return useMutation<ConnectionTestResponse, ApiError, string>({
    mutationFn: (id) => payload(sapSystemsTest(id)),
    onSettled: () => queryClient.invalidateQueries({ queryKey: listKey() }),
  });
}
