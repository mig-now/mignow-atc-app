import { useMutation, useQueryClient } from "@tanstack/react-query";
import type { ApiError } from "@/api/client";
import {
  customersCreate,
  customersDelete,
  customersUpdate,
  getCustomersListQueryKey,
  useCustomersList,
} from "@/api/generated/atc/customers/customers";
import type { CustomerResponse, CustomersListParams } from "@/api/generated/atc/model";
import { payload, unwrapped } from "@/api/unwrapped";

export function useCustomers(params: CustomersListParams) {
  return unwrapped(useCustomersList(params));
}

export type CustomerInput = { name: string; code: string | null; active: boolean };

/** Cria ou altera (id presente). Invalida a lista no sucesso. */
export function useSaveCustomer() {
  const queryClient = useQueryClient();
  return useMutation<CustomerResponse, ApiError, { id?: string; input: CustomerInput }>({
    mutationFn: ({ id, input }) =>
      id ? payload(customersUpdate(id, input)) : payload(customersCreate({ name: input.name, code: input.code })),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: getCustomersListQueryKey().slice(0, 1) }),
  });
}

export function useDeleteCustomer() {
  const queryClient = useQueryClient();
  return useMutation<unknown, ApiError, string>({
    mutationFn: (id) => customersDelete(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: getCustomersListQueryKey().slice(0, 1) }),
  });
}
