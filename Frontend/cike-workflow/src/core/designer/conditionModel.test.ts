import { describe, expect, it } from "vitest"
import { makeBatchCommand, makeEditPropertyCommand } from "./commands"
import { makeSetExpressionCommand, type ExpressionLike } from "./expression"
import {
  emptyGroup,
  emptySpec,
  readCaseConditions,
  readIfCondition,
  seedSpecFromExpression,
} from "./conditionModel"

describe("seedSpecFromExpression", () => {
  it("keeps meaningful Javascript code as an escape hatch", () => {
    expect(seedSpecFromExpression({ type: "Javascript", value: "getVariable('x') > 1" })).toEqual({
      type: "Javascript",
      value: "getVariable('x') > 1",
    })
  })

  it("keeps meaningful Liquid as an escape hatch", () => {
    expect(seedSpecFromExpression({ type: "Liquid", value: "{{ x }}" })).toEqual({ type: "Liquid", value: "{{ x }}" })
  })

  it("keeps Literal true", () => {
    expect(seedSpecFromExpression({ type: "Literal", value: true })).toEqual({ type: "Literal", value: true })
  })

  it("defaults a fresh Literal false / empty / absent to the visual builder", () => {
    expect(seedSpecFromExpression({ type: "Literal", value: false })).toEqual(emptySpec())
    expect(seedSpecFromExpression({ type: "Javascript", value: "" })).toEqual(emptySpec())
    expect(seedSpecFromExpression(undefined)).toEqual(emptySpec())
    expect(emptySpec()).toEqual({ type: "custom", value: emptyGroup() })
  })
})

describe("readIfCondition", () => {
  it("returns the stored ifCondition when present", () => {
    const activity = {
      customProperties: { customExpression: { ifCondition: { type: "Literal", value: true } } },
      condition: { expression: { type: "Javascript", value: "old" } },
    }
    expect(readIfCondition(activity)).toEqual({ type: "Literal", value: true })
  })

  it("seeds from the existing condition when no container is stored", () => {
    const activity = {
      customProperties: {},
      condition: { expression: { type: "Javascript", value: "getVariable('age') >= 18" } },
    }
    expect(readIfCondition(activity)).toEqual({ type: "Javascript", value: "getVariable('age') >= 18" })
  })
})

describe("readCaseConditions", () => {
  it("returns stored caseConditions when present", () => {
    const activity = {
      customProperties: {
        customExpression: { caseConditions: [{ label: "A", type: "custom", value: emptyGroup() }] },
      },
      cases: [{ label: "ignored", value: { type: "Literal", value: "x" } }],
    }
    expect(readCaseConditions(activity)).toEqual([{ label: "A", type: "custom", value: emptyGroup() }])
  })

  it("seeds from existing cases, preserving labels", () => {
    const activity = {
      customProperties: {},
      cases: [
        { label: "High", value: { type: "Javascript", value: "getVariable('score') > 90" } },
        { label: "Low", value: { type: "Literal", value: false } },
      ],
    }
    expect(readCaseConditions(activity)).toEqual([
      { label: "High", type: "Javascript", value: "getVariable('score') > 90" },
      { label: "Low", type: "custom", value: emptyGroup() },
    ])
  })
})

describe("makeBatchCommand", () => {
  it("applies, undoes (reverse) and redoes all children as one step", () => {
    const target: Record<string, unknown> = { a: 1, b: 2 }
    const batch = makeBatchCommand("批量", [
      makeEditPropertyCommand(target, "a", 1, 10),
      makeEditPropertyCommand(target, "b", 2, 20),
    ])
    batch.apply()
    expect(target).toEqual({ a: 10, b: 20 })
    batch.undo()
    expect(target).toEqual({ a: 1, b: 2 })
    batch.redo?.()
    expect(target).toEqual({ a: 10, b: 20 })
  })
})

describe("makeSetExpressionCommand", () => {
  it("overwrites type/value in place and reverts", () => {
    const expression: ExpressionLike = { type: "Literal", value: false }
    const command = makeSetExpressionCommand(expression, { type: "Javascript", value: "true" })
    command.apply()
    expect(expression).toEqual({ type: "Javascript", value: "true" })
    command.undo()
    expect(expression).toEqual({ type: "Literal", value: false })
    command.redo?.()
    expect(expression).toEqual({ type: "Javascript", value: "true" })
  })
})
