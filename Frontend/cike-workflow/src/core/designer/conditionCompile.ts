/**
 * 条件 builder 树 → Javascript 表达式的编译器（ADR 0010）。
 *
 * 编辑真源是存于 customProperties.customExpression 的 builder 树；本模块把它单向编译为
 * 后端可执行的 Expression（编译目标钉死 Javascript）。纯函数、无副作用，便于 vitest 覆盖。
 */

export type ConditionDataType = "string" | "number" | "boolean" | "datetime"

/** builder 操作数类型。Liquid 不可作为操作数（Jint 内无 Liquid 渲染器），仅作整条件逃生舱。 */
export type ConditionOperandType = "Literal" | "Javascript"

export type ConditionOperator =
  | "="
  | "!="
  | ">"
  | ">="
  | "<"
  | "<="
  | "contains"
  | "notContains"
  | "startsWith"
  | "endsWith"
  | "empty"
  | "notEmpty"

export interface ConditionOperand {
  type: ConditionOperandType
  value?: unknown
  dataType?: ConditionDataType
}

export interface ConditionComparison {
  left: ConditionOperand
  operator: ConditionOperator
  right: ConditionOperand
}

export interface ConditionGroup {
  conditionType: "and" | "or"
  conditions: ConditionComparison[]
  combineCondition?: ConditionGroup
}

/** 单个条件（If 的 ifCondition / Switch 的 caseConditions[i]）的编辑态。 */
export interface ConditionSpec {
  type: "custom" | "Literal" | "Javascript" | "Liquid"
  value: ConditionGroup | boolean | string
}

/** 编译产物：与后端 Expression { type, value } 同构。 */
export interface CompiledExpression {
  type: string
  value: unknown
}

/** 各数据类型可用的运算符全集（UI 与校验共用）。 */
export const OPERATORS_BY_DATATYPE: Record<ConditionDataType, ConditionOperator[]> = {
  number: ["=", "!=", ">", ">=", "<", "<="],
  datetime: ["=", "!=", ">", ">=", "<", "<="],
  string: ["=", "!=", "contains", "notContains", "startsWith", "endsWith", "empty", "notEmpty"],
  boolean: ["=", "!="],
}

function compileOperand(operand: ConditionOperand): string {
  if (operand.type === "Javascript") return String(operand.value ?? "")
  switch (operand.dataType) {
    case "number":
      return String(operand.value)
    case "boolean":
      return operand.value === true ? "true" : "false"
    case "datetime":
      return `new Date(${JSON.stringify(String(operand.value ?? ""))}).getTime()`
    case "string":
    default:
      return JSON.stringify(operand.value == null ? "" : String(operand.value))
  }
}

function compileComparison(comparison: ConditionComparison): string {
  const left = compileOperand(comparison.left)
  const right = compileOperand(comparison.right)
  switch (comparison.operator) {
    case "=":
      return `(${left} === ${right})`
    case "!=":
      return `(${left} !== ${right})`
    case ">":
      return `(${left} > ${right})`
    case ">=":
      return `(${left} >= ${right})`
    case "<":
      return `(${left} < ${right})`
    case "<=":
      return `(${left} <= ${right})`
    case "contains":
      return `(String(${left}).includes(${right}))`
    case "notContains":
      return `(!String(${left}).includes(${right}))`
    case "startsWith":
      return `(String(${left}).startsWith(${right}))`
    case "endsWith":
      return `(String(${left}).endsWith(${right}))`
    case "empty":
      return `(${left} == null || ${left} === "")`
    case "notEmpty":
      return `(!(${left} == null || ${left} === ""))`
    default:
      return `(${left} === ${right})`
  }
}

/** 组 = conditions[] 用 conditionType 接合，再与（加括号的）combineCondition 用同一操作符接合。 */
export function compileGroup(group: ConditionGroup): string {
  const op = group.conditionType === "or" ? "||" : "&&"
  const parts = (group.conditions ?? []).map(compileComparison)
  if (group.combineCondition) parts.push(`(${compileGroup(group.combineCondition)})`)
  if (parts.length === 0) return "true"
  return parts.join(` ${op} `)
}

/** 整个条件（含逃生舱类型）→ 后端 Expression。 */
export function compileCondition(spec: ConditionSpec): CompiledExpression {
  switch (spec.type) {
    case "custom":
      return { type: "Javascript", value: compileGroup(spec.value as ConditionGroup) }
    case "Literal":
      return { type: "Literal", value: spec.value === true }
    case "Liquid":
      return { type: "Liquid", value: String(spec.value ?? "") }
    case "Javascript":
    default:
      return { type: "Javascript", value: String(spec.value ?? "") }
  }
}
