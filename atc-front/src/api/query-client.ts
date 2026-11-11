import { MutationCache, QueryCache, QueryClient } from "@tanstack/react-query";
import { isApiError } from "./client";

/** QueryClient com o 401 global: qualquer query/mutation 401 chama `onUnauthorized` (navegação SPA para /login). */
export function makeQueryClient(onUnauthorized: () => void) {
  const handle = (error: unknown) => {
    if (isApiError(error) && error.status === 401) onUnauthorized();
  };
  return new QueryClient({
    queryCache: new QueryCache({ onError: handle }),
    mutationCache: new MutationCache({ onError: handle }),
    defaultOptions: {
      queries: {
        staleTime: 15_000,
        refetchOnWindowFocus: false,
        retry: (count, error) => !(isApiError(error) && error.status < 500) && count < 2,
      },
    },
  });
}
