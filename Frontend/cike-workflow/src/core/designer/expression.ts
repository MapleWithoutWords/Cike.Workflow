import type { DesignerCommand } from "./commands";

/**
 * Structural view of an Expression ({ type, value }). Both the domain class and
 * the plain wire objects satisfy it, so callers need no casts.
 */
export interface ExpressionLike {
  type: string;
  value?: unknown;
}

/**
 * Expression editing domain logic (ADR 0004 / 0005). The five backend-registered
 * expression types are discovered at runtime from the ExpressionDescriptors
 * endpoint, so nothing here hardcodes the list. Only `Literal` carries a typed
 * concrete value; every other type stores a string (JS script / Liquid template
 * / variable name / workflow-input name).
 */

/** The one expression type whose value is a concrete typed value, not a string. */
export const LITERAL_TYPE = "Literal";

/** True when the given expression type is the concrete-valued Literal type. */
export function isLiteral(type: string): boolean {
  return type === LITERAL_TYPE;
}

/**
 * The neutral value an expression resets to when its type is switched (ADR 0005).
 * Non-Literal types all store strings → "". Literal stores a concrete typed
 * value → the caller-provided default (the component resolves descriptor
 * defaultValue / type zero-value and passes it in; it is never a bare null).
 * No value is ever carried across types: a JS script ≠ a Liquid template ≠ a name.
 */
export function neutralValueFor(type: string, literalDefault: unknown): unknown {
  return isLiteral(type) ? literalDefault : "";
}

/**
 * Composite, single-undo command that switches an expression's type and resets
 * its value in one step (ADR 0005). One Ctrl+Z fully reverts both fields, so the
 * model never sits in a "type changed, value stale" half-state.
 */
export function makeSwitchExpressionTypeCommand(
  expression: ExpressionLike,
  targetType: string,
  literalDefault: unknown,
): DesignerCommand {
  const fromType = expression.type;
  const fromValue = expression.value;
  const toValue = neutralValueFor(targetType, literalDefault);
  const apply = (): void => {
    expression.type = targetType;
    expression.value = toValue;
  };
  const revert = (): void => {
    expression.type = fromType;
    expression.value = fromValue;
  };
  return {
    label: "切换表达式类型",
    apply,
    undo: revert,
    redo: apply,
  };
}
