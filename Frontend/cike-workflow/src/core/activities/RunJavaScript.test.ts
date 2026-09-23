import { describe, expect, it } from "vitest"
import { RunJavaScript } from "./RunJavaScript"

function withOutcomes(value: unknown): RunJavaScript {
  const activity = new RunJavaScript()
  activity.possibleOutcomes.expression.value = value
  return activity
}

describe("RunJavaScript.getOutPorts", () => {
  it("Empty_LiteralFallsBackToDone", () => {
    const ports = withOutcomes([]).getOutPorts()
    expect(ports.map((p) => p.name)).toEqual(["Done"])
  })

  it("StringArrayLiteralYieldsNamedPorts", () => {
    const ports = withOutcomes(["Ok", "Fail"]).getOutPorts()
    expect(ports.map((p) => p.name)).toEqual(["Ok", "Fail"])
  })

  it("NonLiteralStringValueDoesNotCrash", () => {
    // Switching the Input to Variable/JavaScript makes value a string; the port
    // projection must fall back to "Done" instead of throwing `names.map`.
    const ports = withOutcomes("myOutcomesVariable").getOutPorts()
    expect(ports.map((p) => p.name)).toEqual(["Done"])
  })

  it("NullValueFallsBackToDone", () => {
    const ports = withOutcomes(null).getOutPorts()
    expect(ports.map((p) => p.name)).toEqual(["Done"])
  })

  it("FiltersOutNonStringEntries", () => {
    const ports = withOutcomes(["Ok", 42, null, "Fail"]).getOutPorts()
    expect(ports.map((p) => p.name)).toEqual(["Ok", "Fail"])
  })
})
