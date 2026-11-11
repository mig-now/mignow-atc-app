/**
 * O Orval tipa as respostas com o envelope { success, data, message }; em runtime o mutator já devolve o `data`.
 * Estes helpers reconciliam o tipo (blueprint §9).
 */
export type Unwrapped<T> = T extends { data?: infer D } ? NonNullable<D> : T;

/** A query gerada, com `data` já tipado como o conteúdo do envelope. */
export function unwrapped<Q extends { data?: unknown }>(
  query: Q,
): Omit<Q, "data"> & { data: Unwrapped<NonNullable<Q["data"]>> | undefined } {
  return query as unknown as Omit<Q, "data"> & { data: Unwrapped<NonNullable<Q["data"]>> | undefined };
}

/** Resultado de uma função gerada (o mutator já desembrulhou o envelope). */
export async function payload<T>(promise: Promise<T>): Promise<Unwrapped<T>> {
  return (await promise) as unknown as Unwrapped<T>;
}
