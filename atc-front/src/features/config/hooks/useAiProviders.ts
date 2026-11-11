import { useMutation, useQueryClient } from "@tanstack/react-query";
import type { ApiError } from "@/api/client";
import {
  aiProvidersCreate,
  aiProvidersDelete,
  aiProvidersUpdate,
  getAiProvidersListQueryKey,
  useAiProvidersList,
} from "@/api/generated/atc/ai-providers/ai-providers";
import type { AiProviderKind, AiProviderResponse } from "@/api/generated/atc/model";
import { payload, unwrapped } from "@/api/unwrapped";

export function useAiProviders() {
  return unwrapped(useAiProvidersList());
}

export type AiProviderInput = {
  name: string;
  kind: AiProviderKind;
  model: string;
  /** Só na criação/troca; null mantém a chave atual. */
  apiKey: string | null;
  isDefault: boolean;
  active: boolean;
};

export function useSaveAiProvider() {
  const queryClient = useQueryClient();
  return useMutation<AiProviderResponse, ApiError, { id?: string; input: AiProviderInput }>({
    mutationFn: ({ id, input }) =>
      id
        ? payload(aiProvidersUpdate(id, input))
        : payload(
            aiProvidersCreate({
              name: input.name,
              kind: input.kind,
              model: input.model,
              apiKey: input.apiKey,
              isDefault: input.isDefault,
            }),
          ),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: getAiProvidersListQueryKey() }),
  });
}

export function useDeleteAiProvider() {
  const queryClient = useQueryClient();
  return useMutation<unknown, ApiError, string>({
    mutationFn: (id) => aiProvidersDelete(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: getAiProvidersListQueryKey() }),
  });
}
