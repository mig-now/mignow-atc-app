import i18next from "i18next";
import { initReactI18next } from "react-i18next";
import { setLanguageProvider } from "@/api/client";
import enCommon from "./locales/en/common.json";
import enConfig from "./locales/en/config.json";
import enErrors from "./locales/en/errors.json";
import enExemption from "./locales/en/exemption.json";
import enExtraction from "./locales/en/extraction.json";
import enKb from "./locales/en/kb.json";
import enRemediation from "./locales/en/remediation.json";
import enReport from "./locales/en/report.json";
import enRuns from "./locales/en/runs.json";
import enScan from "./locales/en/scan.json";
import enTriage from "./locales/en/triage.json";
import enValidation from "./locales/en/validation.json";
import esCommon from "./locales/es/common.json";
import esConfig from "./locales/es/config.json";
import esErrors from "./locales/es/errors.json";
import esExemption from "./locales/es/exemption.json";
import esExtraction from "./locales/es/extraction.json";
import esKb from "./locales/es/kb.json";
import esRemediation from "./locales/es/remediation.json";
import esReport from "./locales/es/report.json";
import esRuns from "./locales/es/runs.json";
import esScan from "./locales/es/scan.json";
import esTriage from "./locales/es/triage.json";
import esValidation from "./locales/es/validation.json";
import ptBRCommon from "./locales/pt-BR/common.json";
import ptBRConfig from "./locales/pt-BR/config.json";
import ptBRErrors from "./locales/pt-BR/errors.json";
import ptBRExemption from "./locales/pt-BR/exemption.json";
import ptBRExtraction from "./locales/pt-BR/extraction.json";
import ptBRKb from "./locales/pt-BR/kb.json";
import ptBRRemediation from "./locales/pt-BR/remediation.json";
import ptBRReport from "./locales/pt-BR/report.json";
import ptBRRuns from "./locales/pt-BR/runs.json";
import ptBRScan from "./locales/pt-BR/scan.json";
import ptBRTriage from "./locales/pt-BR/triage.json";
import ptBRValidation from "./locales/pt-BR/validation.json";

/** Um arquivo por namespace e por idioma em ./locales/<idioma>/<ns>.json (cada módulo edita só os seus). */
const NAMESPACES = [
  "common",
  "config",
  "extraction",
  "remediation",
  "exemption",
  "scan",
  "report",
  "kb",
  "triage",
  "runs",
  "errors",
  "validation",
];
const STORAGE_KEY = "migatc-lang";
const SUPPORTED = ["pt-BR", "en", "es"];

function initialLanguage(): string {
  if (typeof window === "undefined") return "pt-BR";
  try {
    const s = window.localStorage.getItem(STORAGE_KEY);
    return s && SUPPORTED.includes(s) ? s : "pt-BR";
  } catch {
    return "pt-BR";
  }
}

const i18n = i18next.createInstance();
void i18n.use(initReactI18next).init({
  lng: initialLanguage(),
  fallbackLng: "pt-BR",
  defaultNS: "common",
  ns: NAMESPACES,
  keySeparator: false, // chaves planas com pontos literais
  nsSeparator: ":",
  resources: {
    "pt-BR": {
      common: ptBRCommon,
      config: ptBRConfig,
      extraction: ptBRExtraction,
      remediation: ptBRRemediation,
      exemption: ptBRExemption,
      scan: ptBRScan,
      triage: ptBRTriage,
      report: ptBRReport,
      kb: ptBRKb,
      runs: ptBRRuns,
      errors: ptBRErrors,
      validation: ptBRValidation,
    },
    en: {
      common: enCommon,
      config: enConfig,
      extraction: enExtraction,
      remediation: enRemediation,
      exemption: enExemption,
      scan: enScan,
      triage: enTriage,
      report: enReport,
      kb: enKb,
      runs: enRuns,
      errors: enErrors,
      validation: enValidation,
    },
    es: {
      common: esCommon,
      config: esConfig,
      extraction: esExtraction,
      remediation: esRemediation,
      exemption: esExemption,
      scan: esScan,
      triage: esTriage,
      report: esReport,
      kb: esKb,
      runs: esRuns,
      errors: esErrors,
      validation: esValidation,
    },
  },
  interpolation: { escapeValue: false },
});

// Accept-Language do backend segue o idioma ativo (na cultura que o back suporta).
const BACKEND_CULTURES: Record<string, string> = { "pt-BR": "pt-BR", en: "en-US", es: "es-ES" };
setLanguageProvider(() => BACKEND_CULTURES[i18n.language] ?? i18n.language);

i18n.on("languageChanged", (lng) => {
  if (typeof window === "undefined") return;
  try {
    window.localStorage.setItem(STORAGE_KEY, lng);
  } catch {
    /* storage bloqueado */
  }
  document.documentElement.lang = lng;
});

export default i18n;
