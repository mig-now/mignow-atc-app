import { useCustomersList } from "@/api/generated/atc/customers/customers";
import { useExtractionsList } from "@/api/generated/atc/extractions/extractions";
import type { ExtractionsListParams, SapSystemsListParams } from "@/api/generated/atc/model";
import { useSapSystemsList } from "@/api/generated/atc/sap-systems/sap-systems";
import { unwrapped } from "@/api/unwrapped";

/** Clientes ativos (filtro do monitor). */
export function useCustomerOptions() {
  return unwrapped(useCustomersList({ page: 1, pageSize: 200, active: true }));
}

/** Ambientes (para filtro da lista e escolha na criação). */
export function useSapSystemOptions(params: SapSystemsListParams = { page: 1, pageSize: 200 }) {
  return unwrapped(useSapSystemsList(params));
}

/** Extrações de um ambiente (só consulta quando há ambiente escolhido). */
export function useExtractionOptions(sapSystemId: string, params: Omit<ExtractionsListParams, "sapSystemId"> = {}) {
  return unwrapped(
    useExtractionsList({ page: 1, pageSize: 100, ...params, sapSystemId }, { query: { enabled: !!sapSystemId } }),
  );
}
