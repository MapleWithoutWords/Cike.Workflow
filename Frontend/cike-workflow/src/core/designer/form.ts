import type { IActivity } from "../abstracts/Activity";

/**
 * Descriptor-driven generic form logic: resolve which activity fields are
 * editable inputs and how a descriptor maps onto the (plain-object) model.
 */

export interface InputFieldDescriptorInput {
  name?: string;
  clrName?: string;
  displayName?: string | null;
  description?: string | null;
  isReadOnly?: boolean | null;
}

export interface ResolvedInputField {
  /** Model field on the activity instance (camelCase). */
  field: string;
  label: string;
  description: string | null;
  readOnly: boolean;
  /** The Input-shaped value: { memoryBlockReference, expression: { type, value } }. */
  input: { expression: { type: string; value?: unknown } } | null;
}

function isInputShaped(value: unknown): value is { expression: { type: string; value?: unknown } } {
  return (
    typeof value === "object" &&
    value !== null &&
    "expression" in (value as object) &&
    typeof (value as { expression?: unknown }).expression === "object" &&
    (value as { expression?: unknown }).expression !== null
  );
}

function lowerFirst(name: string): string {
  return name ? name[0].toLowerCase() + name.slice(1) : name;
}

export function resolveInputFields(activity: IActivity, descriptors: InputFieldDescriptorInput[]): ResolvedInputField[] {
  const resolved: ResolvedInputField[] = [];
  for (const descriptor of descriptors) {
    const candidates = [descriptor.clrName, descriptor.name].filter((name): name is string => !!name);
    let field = candidates.map(lowerFirst).find((name) => name in activity) ?? null;
    if (!field) continue;
    const value = (activity as unknown as Record<string, unknown>)[field];
    resolved.push({
      field,
      label: descriptor.displayName ?? descriptor.name ?? field,
      description: descriptor.description ?? null,
      readOnly: descriptor.isReadOnly === true,
      input: isInputShaped(value) ? value : null,
    });
  }
  return resolved;
}

export const MERGE_MODES = ["Stream", "Merge", "Converge", "Cascade", "Race"] as const;

export function getMergeMode(activity: IActivity): string | null {
  const value = activity.customProperties?.["mergeMode"];
  return typeof value === "string" && value ? value : null;
}

export function setMergeMode(activity: IActivity, mode: string | null): void {
  const customProperties = (activity.customProperties ??= {}) as Record<string, unknown>;
  if (!mode) delete customProperties["mergeMode"];
  else customProperties["mergeMode"] = mode;
}

/** Options-level workflow variables (mirrors backend WorkflowVariableDefinition). */
export interface WorkflowVariableInput {
  name?: string;
  typeName?: string;
  isArray?: boolean;
}

/** Lightweight, non-blocking validation matching the backend publish rules. */
export function variableNameIssues(variables: WorkflowVariableInput[]): string[] {
  const issues: string[] = [];
  variables.forEach((variable, index) => {
    if (!variable.name) issues.push(`第 ${index + 1} 个变量名称为空`);
  });
  const seen = new Map<string, number>();
  for (const variable of variables) {
    if (!variable.name) continue;
    seen.set(variable.name, (seen.get(variable.name) ?? 0) + 1);
  }
  for (const [name, count] of seen) {
    if (count > 1) issues.push(`变量名称 [${name}] 重复`);
  }
  return issues;
}

/**
 * Coerce a text-edited literal back into the shape of the previous value so
 * numbers/booleans/objects survive a roundtrip through an input element.
 * Returns undefined when an object payload fails to parse (caller no-ops).
 */
export function coerceLiteralValue(from: unknown, raw: string): unknown {
  if (typeof from === "number") return raw === "" ? null : Number(raw);
  if (typeof from === "boolean") return raw === "true";
  if (from != null && typeof from === "object") {
    try {
      return raw ? JSON.parse(raw) : null;
    } catch {
      return undefined;
    }
  }
  return raw === "" ? null : raw;
}
