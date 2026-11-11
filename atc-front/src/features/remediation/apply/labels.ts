import type { BadgeTone } from "@/components/ui/badge";

/** Tom do selo por resultado da aplicação (`ApplyResults` do back). */
export const RESULT_TONE: Record<string, BadgeTone> = {
  SIMULADO: "info",
  ATIVADO: "success",
  ROLLBACK_ATIVADO: "success",
  ERRO: "danger",
  ERRO_ATIVACAO: "danger",
  ERRO_ROLLBACK_ATIVACAO: "danger",
  ERRO_GRAVACAO: "danger",
  ERRO_LOCK: "danger",
  ERRO_REQUEST: "danger",
  ERRO_VALIDACAO: "danger",
  ERRO_CRIACAO: "danger",
  ERRO_PREPARACAO: "danger",
  ABORTADO_FONTE_MUDOU: "warning",
  ABORTADO_ESTRUTURA_TABELA: "warning",
  NADA_A_FAZER: "neutral",
  NAO_PRONTO: "warning",
  ROLLBACK_INDISPONIVEL: "warning",
};

export const STATUS_TONE: Record<string, BadgeTone> = {
  "": "neutral",
  "Confirmado KB": "success",
  "Confirmado IA": "primary",
  Cancelado: "neutral",
  Isenção: "warning",
};

/** Tipos de objeto de dicionário (aceitam os parâmetros de criação DDIC). */
export const DDIC_TYPES = new Set(["TABL", "STRU", "DTEL", "DOMA", "TTYP", "VIEW"]);

/** Categoria da KB (`/kb/:category/:element`) pelo tipo de referência do check. */
export function kbCategory(refType?: string | null) {
  switch (refType) {
    case "CLAS":
    case "INTF":
      return "classes";
    case "FUNC":
      return "funcoes";
    case "DTEL":
    case "TTYP":
    case "DOMA":
      return "tipos";
    default:
      return "tabelas";
  }
}

/** O parecer da revisão por IA que bloqueia a gravação (veredito `bloquear`, igual ao back). */
export const isBlockingVerdict = (verdict?: string | null) => verdict?.trim().toLowerCase() === "bloquear";
