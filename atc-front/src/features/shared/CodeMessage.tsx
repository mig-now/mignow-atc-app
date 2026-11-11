import { useTranslation } from "react-i18next";
import type { MessageArgs } from "./types";

/**
 * Mensagem do backend por código (passos, log, observações): traduzida no idioma ativo com os argumentos.
 * Chaves em `errors:code.<Código>`; sem tradução, mostra o próprio código.
 */
export function useCodeMessage() {
  const { t, i18n } = useTranslation();
  return (code: string | null | undefined, args?: MessageArgs) => {
    if (!code) return "";
    const key = `errors:code.${code}`;
    const values = Object.fromEntries(Object.entries(args ?? {}).map(([k, v]) => [k, v ?? ""]));
    return i18n.exists(key) ? t(key, values) : code;
  };
}

export function CodeMessage({ code, args }: { code: string | null | undefined; args?: MessageArgs }) {
  return <>{useCodeMessage()(code, args)}</>;
}
