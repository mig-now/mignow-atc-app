import { z } from "zod";
import i18n from "./config";

// Validação traduz sozinha: códigos do Zod → namespace "validation" no idioma ativo. Não escreva `message:` nos schemas.
z.config({
  customError: (issue) => {
    switch (issue.code) {
      case "too_small":
        return issue.minimum === 1 && issue.origin === "string"
          ? i18n.t("validation:required")
          : i18n.t("validation:tooSmall", { minimum: String(issue.minimum) });
      case "too_big":
        return i18n.t("validation:tooBig", { maximum: String(issue.maximum) });
      case "invalid_format":
        return i18n.t(issue.format === "email" ? "validation:email" : "validation:format");
      case "invalid_type":
        return i18n.t("validation:required");
      default:
        return i18n.t("validation:invalid");
    }
  },
});

export default i18n;
