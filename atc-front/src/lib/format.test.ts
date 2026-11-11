import { describe, expect, it } from "vitest";
import { formatMoney } from "./format";

describe("formatMoney", () => {
  it("não arredonda para zero o custo abaixo de 1 centavo", () => {
    expect(formatMoney(0.00000073, "en")).toBe("$0.00000073");
    expect(formatMoney(0.000796, "en")).toBe("$0.0008");
  });

  it("mantém até 4 casas nos demais valores", () => {
    expect(formatMoney(0, "en")).toBe("$0.00");
    expect(formatMoney(0.0123, "en")).toBe("$0.0123");
    expect(formatMoney(1.5, "en")).toBe("$1.50");
  });
});
