/**
 * Switch 的单个 case 条目。对应后端 Cike.Workflow.Core.Activities.FlowSwitchCase。
 * condition 是 Expression（非 Input），后端默认 false 字面量。
 */

import { Expression, type ExpressionJson } from '../memory/expression'

export interface FlowSwitchCaseJson {
    label?: string | null
    condition?: ExpressionJson | null
}

export class FlowSwitchCase {
    /** 命中时发出的 outcome 名（画布出口端口） */
    label: string
    condition: Expression

    constructor(label = '', condition?: Expression) {
        this.label = label
        this.condition = condition ?? Expression.literal(false)
    }

    static fromJson(json: FlowSwitchCaseJson): FlowSwitchCase {
        return new FlowSwitchCase(json.label ?? '', Expression.fromJson(json.condition))
    }

    static parseList(raw: unknown): FlowSwitchCase[] {
        if (!Array.isArray(raw)) {
            return []
        }
        return raw.map(item => FlowSwitchCase.fromJson(item as FlowSwitchCaseJson))
    }

    toJson(): FlowSwitchCaseJson {
        return { label: this.label, condition: this.condition.toJson() }
    }
}
