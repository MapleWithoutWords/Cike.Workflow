import { describe, expect, it } from "vitest";
import { Expression } from "../models/Expression";
import {
  LITERAL_TYPE,
  isLiteral,
  makeSwitchExpressionTypeCommand,
  neutralValueFor,
} from "./expression";

describe("isLiteral", () => {
  it("IsTrueOnlyForLiteral", () => {
    expect(isLiteral(LITERAL_TYPE)).toBe(true);
    expect(isLiteral("JavaScript")).toBe(false);
    expect(isLiteral("Liquid")).toBe(false);
    expect(isLiteral("Variable")).toBe(false);
    expect(isLiteral("WorkflowInput")).toBe(false);
  });
});

describe("neutralValueFor", () => {
  it("ResetsEveryNonLiteralTypeToEmptyString", () => {
    for (const type of ["JavaScript", "Liquid", "Variable", "WorkflowInput"]) {
      expect(neutralValueFor(type, 42)).toBe("");
    }
  });

  it("UsesCallerConcreteDefaultForLiteral", () => {
    expect(neutralValueFor(LITERAL_TYPE, 0)).toBe(0);
    expect(neutralValueFor(LITERAL_TYPE, "text")).toBe("text");
    expect(neutralValueFor(LITERAL_TYPE, false)).toBe(false);
  });
});

describe("makeSwitchExpressionTypeCommand", () => {
  it("Apply_SwitchesTypeAndResetsValueInOneStep", () => {
    const expression = new Expression("JavaScript", "return 1;");
    makeSwitchExpressionTypeCommand(expression, "Variable", 0).apply();
    expect(expression.type).toBe("Variable");
    expect(expression.value).toBe("");
  });

  it("Apply_ToLiteralUsesCallerDefaultNotNull", () => {
    const expression = new Expression("Liquid", "{{ x | upcase }}");
    makeSwitchExpressionTypeCommand(expression, LITERAL_TYPE, 7).apply();
    expect(expression.type).toBe(LITERAL_TYPE);
    expect(expression.value).toBe(7);
  });

  it("NeverCarriesValueAcrossIncompatibleTypes", () => {
    const expression = new Expression("JavaScript", "return a + b;");
    makeSwitchExpressionTypeCommand(expression, "Liquid", "").apply();
    expect(expression.value).toBe("");
  });

  it("Undo_RestoresOriginalTypeAndValueExactly", () => {
    const expression = new Expression(LITERAL_TYPE, 5);
    const command = makeSwitchExpressionTypeCommand(expression, "JavaScript", 5);
    command.apply();
    command.undo();
    expect(expression.type).toBe(LITERAL_TYPE);
    expect(expression.value).toBe(5);
  });

  it("Redo_ReappliesTheSwitch", () => {
    const expression = new Expression(LITERAL_TYPE, 5);
    const command = makeSwitchExpressionTypeCommand(expression, "JavaScript", 5);
    command.apply();
    command.undo();
    command.redo?.();
    expect(expression.type).toBe("JavaScript");
    expect(expression.value).toBe("");
  });

  it("PreservesObjectLiteralValueOnUndo", () => {
    const original = { a: 1 };
    const expression = new Expression(LITERAL_TYPE, original);
    const command = makeSwitchExpressionTypeCommand(expression, "WorkflowInput", original);
    command.apply();
    expect(expression.value).toBe("");
    command.undo();
    expect(expression.value).toBe(original);
  });
});
