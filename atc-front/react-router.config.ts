import type { Config } from "@react-router/dev/config";

export default {
  ssr: false, // SPA: saída estática, sem servidor Node em prod
  splitRouteModules: true,
  // SEM prerender: toda rota (exceto /login) é protegida.
} satisfies Config;
