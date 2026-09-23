import { describe, expect, it } from "vitest"
import { Code, Type as TypeIcon } from "@lucide/vue"
import { FALLBACK_EXPRESSION_ICON, resolveExpressionIcon } from "./expressionIcons"

describe("resolveExpressionIcon", () => {
  it("maps a bundled backend icon name to its lucide component", () => {
    expect(resolveExpressionIcon("type")).toBe(TypeIcon)
    expect(resolveExpressionIcon("code")).toBe(Code)
  })

  it("falls back for unbundled names", () => {
    expect(resolveExpressionIcon("no-such-icon")).toBe(FALLBACK_EXPRESSION_ICON)
  })

  it("falls back for null/undefined", () => {
    expect(resolveExpressionIcon(null)).toBe(FALLBACK_EXPRESSION_ICON)
    expect(resolveExpressionIcon(undefined)).toBe(FALLBACK_EXPRESSION_ICON)
  })
})
