import { describe, expect, it } from "vitest";
import { If } from "../activities/If";
import { getMergeMode, resolveInputFields, setMergeMode, variableNameIssues } from "./form";

describe("form", () => {
  it("ResolveInputFields_MapsClrNameToLowerFirst", () => {
    const activity = new If();
    const fields = resolveInputFields(activity, [
      { clrName: "Condition", displayName: "条件", name: "Condition" },
      { clrName: "NonExistent" },
    ]);
    expect(fields.length).toBe(1);
    expect(fields[0].field).toBe("condition");
    expect(fields[0].label).toBe("条件");
    expect(fields[0].input).not.toBeNull();
    expect(fields[0].input!.expression.type).toBe("Literal");
  });

  it("MergeMode_SetGetRemove", () => {
    const activity = new If();
    expect(getMergeMode(activity)).toBeNull();
    setMergeMode(activity, "Merge");
    expect(getMergeMode(activity)).toBe("Merge");
    expect(activity.customProperties["mergeMode"]).toBe("Merge");
    setMergeMode(activity, null);
    expect(getMergeMode(activity)).toBeNull();
  });
});

describe("variableNameIssues", () => {
  it("FlagsEmptyAndDuplicateNames", () => {
    const issues = variableNameIssues([
      { name: "amount", typeName: "String" },
      { name: "", typeName: "String" },
      { name: "amount", typeName: "Int32" },
    ]);
    expect(issues.length).toBe(2);
    expect(issues[0]).toContain("空");
    expect(issues[1]).toContain("amount");
  });
});
