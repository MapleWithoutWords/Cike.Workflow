import type { IActivity } from "../abstracts/Activity"
import type { Expression } from "@/api/generated"

/**
 * Rename cascade logic (ADR 0008 / spec decision):
 * When an input or variable is renamed, update all structured Expression
 * references of matching type throughout the activity tree. JavaScript/Liquid
 * free-text is never rewritten — those only surface errors at runtime.
 */

export type ReferenceKind = "Variable" | "Input"

export interface RenameTarget {
  kind: ReferenceKind
  oldName: string
  newName: string
}

/**
 * Recursively walks the activity tree and rewrites Expression nodes whose
 * `type` matches the rename kind and whose `value` (string) matches oldName.
 * Returns the total number of expressions rewritten.
 */
export function cascadeRename(root: IActivity, target: RenameTarget): number {
  let count = 0
  visitActivity(root, target, () => count++)
  return count
}

function visitActivity(activity: IActivity, target: RenameTarget, onHit: () => void): void {
  // Walk all own properties looking for Expression-shaped values.
  const record = activity as unknown as Record<string, unknown>
  for (const key of Object.keys(record)) {
    if (key === "type" || key === "id" || key === "nodeId") continue
    const value = record[key]
    visitValue(value, target, onHit)
  }
}

function visitValue(value: unknown, target: RenameTarget, onHit: () => void): void {
  if (isExpressionLike(value)) {
    rewriteExpression(value, target, onHit)
  } else if (Array.isArray(value)) {
    for (const item of value) {
      visitValue(item, target, onHit)
    }
  } else if (isActivityLike(value)) {
    visitActivity(value as unknown as IActivity, target, onHit)
  } else if (value != null && typeof value === "object") {
    visitPlainObject(value as Record<string, unknown>, target, onHit)
  }
}

function visitPlainObject(obj: Record<string, unknown>, target: RenameTarget, onHit: () => void): void {
  for (const key of Object.keys(obj)) {
    visitValue(obj[key], target, onHit)
  }
}

function rewriteExpression(expr: { type?: string; value?: unknown }, target: RenameTarget, onHit: () => void): void {
  if (expr.type !== target.kind) return
  if (typeof expr.value !== "string") return
  if (expr.value !== target.oldName) return
  expr.value = target.newName
  onHit()
}

function isExpressionLike(value: unknown): value is Expression {
  return (
    value != null &&
    typeof value === "object" &&
    !Array.isArray(value) &&
    "type" in (value as object) &&
    "value" in (value as object)
  )
}

function isActivityLike(value: unknown): boolean {
  return (
    value != null &&
    typeof value === "object" &&
    !Array.isArray(value) &&
    "type" in (value as object) &&
    "id" in (value as object)
  )
}
