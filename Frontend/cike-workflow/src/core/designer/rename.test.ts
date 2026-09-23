import { describe, expect, it } from "vitest"
import { cascadeRename, type RenameTarget } from "./rename"

function makeActivity(overrides: Record<string, unknown> = {}) {
  return { type: "Cike.Test", id: "a1", ...overrides } as never
}

describe("cascadeRename", () => {
  it("rewrites matching Variable expression", () => {
    const expr = { type: "Variable", value: "counter" }
    const activity = makeActivity({ myInput: { expression: expr } })
    const target: RenameTarget = { kind: "Variable", oldName: "counter", newName: "total" }
    const count = cascadeRename(activity, target)
    expect(count).toBe(1)
    expect(expr.value).toBe("total")
  })

  it("rewrites matching Input expression", () => {
    const expr = { type: "Input", value: "userId" }
    const activity = makeActivity({ someField: { expression: expr } })
    const target: RenameTarget = { kind: "Input", oldName: "userId", newName: "accountId" }
    const count = cascadeRename(activity, target)
    expect(count).toBe(1)
    expect(expr.value).toBe("accountId")
  })

  it("does not rewrite non-matching type", () => {
    const expr = { type: "JavaScript", value: "counter" }
    const activity = makeActivity({ myInput: { expression: expr } })
    const target: RenameTarget = { kind: "Variable", oldName: "counter", newName: "total" }
    const count = cascadeRename(activity, target)
    expect(count).toBe(0)
    expect(expr.value).toBe("counter")
  })

  it("does not rewrite non-matching value", () => {
    const expr = { type: "Variable", value: "other" }
    const activity = makeActivity({ myInput: { expression: expr } })
    const target: RenameTarget = { kind: "Variable", oldName: "counter", newName: "total" }
    const count = cascadeRename(activity, target)
    expect(count).toBe(0)
    expect(expr.value).toBe("other")
  })

  it("cascades into nested child activities", () => {
    const childExpr = { type: "Variable", value: "x" }
    const child = makeActivity({ input: { expression: childExpr } })
    const parent = makeActivity({ activities: [child], connections: [] })
    const target: RenameTarget = { kind: "Variable", oldName: "x", newName: "y" }
    const count = cascadeRename(parent, target)
    expect(count).toBe(1)
    expect(childExpr.value).toBe("y")
  })

  it("cascades into deeply nested body containers", () => {
    const deepExpr = { type: "Input", value: "amount" }
    const deepChild = makeActivity({ field: { expression: deepExpr } })
    const body = makeActivity({ activities: [deepChild], connections: [] })
    const loop = makeActivity({ body })
    const root = makeActivity({ activities: [loop], connections: [] })
    const target: RenameTarget = { kind: "Input", oldName: "amount", newName: "total" }
    const count = cascadeRename(root, target)
    expect(count).toBe(1)
    expect(deepExpr.value).toBe("total")
  })

  it("rewrites multiple matching expressions", () => {
    const expr1 = { type: "Variable", value: "v" }
    const expr2 = { type: "Variable", value: "v" }
    const expr3 = { type: "Variable", value: "other" }
    const activity = makeActivity({
      a: { expression: expr1 },
      b: { expression: expr2 },
      c: { expression: expr3 },
    })
    const target: RenameTarget = { kind: "Variable", oldName: "v", newName: "w" }
    const count = cascadeRename(activity, target)
    expect(count).toBe(2)
    expect(expr1.value).toBe("w")
    expect(expr2.value).toBe("w")
    expect(expr3.value).toBe("other")
  })

  it("leaves Liquid and JavaScript expressions untouched", () => {
    const jsExpr = { type: "JavaScript", value: "getVariable('counter')" }
    const liquidExpr = { type: "Liquid", value: "{{ counter }}" }
    const activity = makeActivity({
      script: { expression: jsExpr },
      template: { expression: liquidExpr },
    })
    const target: RenameTarget = { kind: "Variable", oldName: "counter", newName: "total" }
    const count = cascadeRename(activity, target)
    expect(count).toBe(0)
    expect(jsExpr.value).toBe("getVariable('counter')")
    expect(liquidExpr.value).toBe("{{ counter }}")
  })

  it("handles arrays of expressions in activity fields", () => {
    const expr1 = { type: "Variable", value: "a" }
    const expr2 = { type: "Input", value: "a" }
    const activity = makeActivity({ items: [{ expression: expr1 }, { expression: expr2 }] })
    const target: RenameTarget = { kind: "Variable", oldName: "a", newName: "b" }
    const count = cascadeRename(activity, target)
    expect(count).toBe(1)
    expect(expr1.value).toBe("b")
    expect(expr2.value).toBe("a") // Input kind not matched
  })
})
