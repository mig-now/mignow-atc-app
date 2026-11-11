import { useMutation, useQueryClient } from "@tanstack/react-query";
import type { ApiError } from "@/api/client";
import type { ProjectType, TriageRuleDefinitionResponse, TriageRuleRequest } from "@/api/generated/atc/model";
import {
  getTriageRulesListQueryKey,
  triageRulesCreate,
  triageRulesDelete,
  triageRulesUpdate,
  useTriageRulesList,
} from "@/api/generated/atc/triage-rules/triage-rules";
import { payload, unwrapped } from "@/api/unwrapped";

/** Cadastro das regras de triagem. Com o tipo de projeto, só as ativas que valem para ele (as que o play aplica). */
export function useTriageRules(projectType?: ProjectType, enabled = true) {
  return unwrapped(useTriageRulesList(projectType ? { projectType } : undefined, { query: { enabled } }));
}

/** Cria (sem `code` atual) ou altera a regra de código `code`; `input.code` diferente renomeia. Padrão: o efeito fica no código. */
export function useSaveTriageRule() {
  const queryClient = useQueryClient();
  return useMutation<TriageRuleDefinitionResponse, ApiError, { code?: string; input: TriageRuleRequest }>({
    mutationFn: ({ code, input }) => payload(code ? triageRulesUpdate(code, input) : triageRulesCreate(input)),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: getTriageRulesListQueryKey() }),
  });
}

/** Exclui uma regra nova (padrão não se exclui: desativar). */
export function useDeleteTriageRule() {
  const queryClient = useQueryClient();
  return useMutation<unknown, ApiError, string>({
    mutationFn: (code) => triageRulesDelete(code),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: getTriageRulesListQueryKey() }),
  });
}
