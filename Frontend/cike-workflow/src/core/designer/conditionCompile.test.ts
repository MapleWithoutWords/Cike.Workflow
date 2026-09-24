import { describe, expect, it } from "vitest"
import {
  compileCondition,
  compileGroup,
  type ConditionComparison,
  type ConditionGroup,
  type ConditionOperand,
} from "./conditionCompile"

function js(value: string): ConditionOperand {
  return { type: "Javascript", value }
}

function lit(value: unknown, dataType: ConditionOperand["dataType"]): ConditionOperand {
  return { type: "Literal", value, dataType }
}

function cmp(left: ConditionOperand, operator: ConditionComparison["operator"], right: ConditionOperand): ConditionComparison {
  return { left, operator, right }
}

describe("compileGroup", () => {
  it("compiles a flat and-group of comparisons", () => {
    const group: ConditionGroup = {
      conditionType: "and",
      conditions: [
        cmp(js("getVariable('age')"), ">", lit(18, "number")),
        cmp(js("getVariable('name')"), "=", lit("tom", "string")),
      ],
    }
    expect(compileGroup(group)).toBe(
      `(getVariable('age') > 18) && (getVariable('name') === "tom")`,
    )
  })

  it("joins conditions and combineCondition with the group operator, parenthesizing the subgroup", () => {
    const group: ConditionGroup = {
      conditionType: "and",
      conditions: [cmp(js("a"), ">", lit(1, "number"))],
      combineCondition: {
        conditionType: "or",
        conditions: [
          cmp(js("b"), "=", lit(true, "boolean")),
          cmp(js("c"), "<", lit(2, "number")),
        ],
      },
    }
    expect(compileGroup(group)).toBe(`(a > 1) && ((b === true) || (c < 2))`)
  })

  it("degrades an empty group to the identity true", () => {
    expect(compileGroup({ conditionType: "and", conditions: [] })).toBe("true")
  })

  it("compiles string helpers and unary empty", () => {
    const group: ConditionGroup = {
      conditionType: "or",
      conditions: [
        cmp(js("getVariable('code')"), "contains", lit("VIP", "string")),
        cmp(js("getVariable('note')"), "empty", lit("", "string")),
      ],
    }
    expect(compileGroup(group)).toBe(
      `(String(getVariable('code')).includes("VIP")) || (getVariable('note') == null || getVariable('note') === "")`,
    )
  })

  it("compiles datetime literals to epoch milliseconds", () => {
    const group: ConditionGroup = {
      conditionType: "and",
      conditions: [cmp(js("getVariable('at')"), ">=", lit("2026-01-01T00:00:00Z", "datetime"))],
    }
    expect(compileGroup(group)).toContain(`new Date("2026-01-01T00:00:00Z").getTime()`)
  })
})

describe("compileCondition", () => {
  it("compiles a custom tree into a Javascript expression", () => {
    const compiled = compileCondition({
      type: "custom",
      value: { conditionType: "or", conditions: [cmp(js("x"), "=", lit(1, "number"))] },
    })
    expect(compiled).toEqual({ type: "Javascript", value: "(x === 1)" })
  })

  it("passes escape-hatch types through unchanged", () => {
    expect(compileCondition({ type: "Literal", value: true })).toEqual({ type: "Literal", value: true })
    expect(compileCondition({ type: "Liquid", value: "{{ ok }}" })).toEqual({ type: "Liquid", value: "{{ ok }}" })
    expect(compileCondition({ type: "Javascript", value: "a > 1" })).toEqual({ type: "Javascript", value: "a > 1" })
  })
})
