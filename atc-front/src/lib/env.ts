import { z } from "zod";

const schema = z.object({
  VITE_API_URL: z.url(),
  VITE_ENTRA_CLIENT_ID: z.string().min(1),
  VITE_ENTRA_TENANT_ID: z.string().min(1),
  VITE_ENTRA_API_SCOPE: z.string().min(1),
  VITE_APPINSIGHTS_CONNECTION_STRING: z.string().optional(),
});

export type Env = z.infer<typeof schema>;

export function parseEnv(raw: Record<string, unknown>): Env {
  const r = schema.safeParse(raw);
  if (!r.success) throw new Error(`Env inválida: ${r.error.issues.map((i) => i.path.join(".")).join(", ")}`);
  return r.data;
}

export const env: Env = parseEnv(import.meta.env as Record<string, unknown>);
