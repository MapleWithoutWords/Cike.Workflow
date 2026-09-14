/**
 * 条件分支。对应后端 Cike.Workflow.Core.Activities.If（[FlowNode("True","False")]，DisplayName=Decision）。
 * 线格式属性：condition（Input<bool>），后端默认 false 字面量。
 */

import { Activity, type ActivityJson } from './activity'
import { Input, readInput } from '../memory/input'
import type { ActivityDiagnostic } from '../validation'
import { error } from '../validation'

export class IfActivity extends Activity {
    static override readonly typeName = 'Cike.If'
    static override readonly outcomes = ['True', 'False'] as const

    condition: Input | null

    /** 不传参 = 后端默认值 false 字面量；显式传 null 表示无输入 */
    constructor(condition: Input | null | undefined = Input.literal(false)) {
        super(IfActivity.typeName)
        this.condition = condition
    }

    static fromJson(json: ActivityJson): IfActivity {
        const activity = new IfActivity()
        activity.applyBaseJson(json)
        activity.condition = readInput(json, 'condition', Input.literal(false))
        return activity
    }

    override toJson(): ActivityJson {
        return { ...this.baseJson(), condition: this.condition ? this.condition.toJson() : null }
    }

    override validate(): ActivityDiagnostic[] {
        const diagnostics = super.validate()
        if (this.condition === null) {
            diagnostics.push(error('IF_CONDITION_MISSING', 'If 节点缺少 condition 输入', 'condition', this))
        }
        return diagnostics
    }
}
