import type { FieldValues, Path, UseFormSetError } from "react-hook-form";
import { isApiError } from "@/api/client";

/** Erros por campo do backend (ValidationProblemDetails) aparecem nos campos do formulário. */
export function applyServerErrors<T extends FieldValues>(
  error: unknown,
  setError: UseFormSetError<T>,
  fields: readonly string[],
): boolean {
  if (!isApiError(error) || !error.errors) return false;
  let applied = false;
  for (const [field, messages] of Object.entries(error.errors)) {
    if (fields.includes(field) && messages[0]) {
      setError(field as Path<T>, { type: "server", message: messages[0] });
      applied = true;
    }
  }
  return applied;
}

/** "" vira null (campos opcionais). */
export const orNull = (v: string | null | undefined) => (v && v.trim() !== "" ? v.trim() : null);

/** Executa a ação; se a API recusar, o erro fica no estado da mutation (mostrado pelo ApiErrorAlert), sem rejeição solta. */
export async function attempt(action: () => Promise<unknown>) {
  try {
    await action();
  } catch {
    /* exibido pelo ApiErrorAlert da tela */
  }
}
