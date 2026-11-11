import { existsSync, readdirSync, readFileSync } from "node:fs";
import { join } from "node:path";

// Regra nº 1 virando verificação: os 3 idiomas têm as mesmas chaves em cada namespace, e todo namespace está registrado.
const root = join(process.cwd(), "src", "i18n", "locales");
const languages = ["pt-BR", "en", "es"];
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
const configSource = readFileSync(join(process.cwd(), "src", "i18n", "config.ts"), "utf8");

describe("paridade de chaves i18n", () => {
  it("pt-BR tem os namespaces do contrato (e nenhum a mais)", () => {
    const found = readdirSync(join(root, "pt-BR"))
      .filter((f: string) => f.endsWith(".json"))
      .map((f: string) => f.replace(".json", ""))
      .sort();
    expect(found).toEqual([...NAMESPACES].sort());
  });

  for (const ns of NAMESPACES) {
    it(`${ns}: arquivo nos 3 idiomas, registrado no i18n config, mesmas chaves`, () => {
      for (const l of languages) expect(existsSync(join(root, l, `${ns}.json`))).toBe(true);
      for (const l of languages) expect(configSource).toContain(`./locales/${l}/${ns}.json`);
      const keys = languages.map((l) =>
        Object.keys(JSON.parse(readFileSync(join(root, l, `${ns}.json`), "utf8")) as object).sort(),
      );
      expect(keys[1]).toEqual(keys[0]);
      expect(keys[2]).toEqual(keys[0]);
    });
  }
});
