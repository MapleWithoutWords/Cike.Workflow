/**
 * 条件 builder 的读取 / 老数据迁移种子 / 默认值（ADR 0010）。
 *
 * 编辑真源存于活动 customProperties.customExpression：If 用 ifCondition、Switch 用 caseConditions[]。
 * 无该容器时从既有条件表达式反推种子（Javascript/Liquid 代码、Literal true 保留为逃生舱；
 * 其余——含全新节点的默认 Literal false——回退为空的可视化 builder，满足"默认可视化"）。
 * 纯函数、无副作用，便于 vitest 覆盖。
 */

import type { ConditionGroup, ConditionSpec } from "./conditionCompile";
import type { ExpressionLike } from "./expression";

/** Switch 分支条件：一个 ConditionSpec 外加投影成出端口的 label。 */
export interface CaseCondition extends ConditionSpec {
  label: string;
}

/** customProperties.customExpression 的容器形状（If / Switch 各用其一，不合并）。 */
export interface CustomExpressionContainer {
  ifCondition?: ConditionSpec;
  caseConditions?: CaseCondition[];
}

/** 空条件组（and、无比较、无子组）。 */
export function emptyGroup(): ConditionGroup {
  return { conditionType: "and", conditions: [] };
}

/** 全新条件的默认编辑态：空的可视化 builder。 */
export function emptySpec(): ConditionSpec {
  return { type: "custom", value: emptyGroup() };
}

/**
 * 从既有（老数据）条件表达式反推种子 ConditionSpec。
 * 只有"承载真实内容"的表达式才落成逃生舱，其余回退为默认可视化 builder。
 */
export function seedSpecFromExpression(expr: ExpressionLike | null | undefined): ConditionSpec {
  if (!expr) return emptySpec();
  const value = expr.value;
  if (expr.type === "Liquid" && typeof value === "string" && value.trim() !== "") {
    return { type: "Liquid", value };
  }
  if (expr.type === "Javascript" && typeof value === "string" && value.trim() !== "") {
    return { type: "Javascript", value };
  }
  if (expr.type === "Literal" && value === true) {
    return { type: "Literal", value: true };
  }
  // 全新节点默认（Literal false）、空代码、缺失 → 默认可视化 builder。
  return emptySpec();
}

interface IfLike {
  customProperties: Record<string, any>;
  condition?: { expression?: ExpressionLike };
}

interface SwitchLike {
  customProperties: Record<string, any>;
  cases?: Array<{ label?: string; value?: ExpressionLike }>;
}

/** 读取 If 的条件编辑态：优先 customProperties.ifCondition，缺失则从既有 condition 反推。 */
export function readIfCondition(activity: IfLike): ConditionSpec {
  const container = activity.customProperties?.["customExpression"] as CustomExpressionContainer | undefined;
  if (container && container.ifCondition) return container.ifCondition;
  return seedSpecFromExpression(activity.condition?.expression);
}

/** 读取 Switch 的分支条件编辑态：优先 customProperties.caseConditions，缺失则从既有 cases 反推。 */
export function readCaseConditions(activity: SwitchLike): CaseCondition[] {
  const container = activity.customProperties?.["customExpression"] as CustomExpressionContainer | undefined;
  if (container && Array.isArray(container.caseConditions)) return container.caseConditions;
  return (activity.cases ?? []).map((entry) => ({
    label: String(entry.label ?? ""),
    ...seedSpecFromExpression(entry.value),
  }));
}
