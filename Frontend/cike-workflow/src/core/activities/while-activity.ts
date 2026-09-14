/**
 * 条件循环。对应后端 Cike.Workflow.Core.Activities.While（[Activity("Cike", "Looping")]）。
 * 线格式属性：condition（Input<bool>，默认 false）、body（[Port] IActivity?）。
 */

import { Activity, type ActivityJson } from './activity'
import { parsePort } from './registry'
import { Input, readInput } from '../memory/input'
import type { ActivityDiagnostic } from '../validation'
import { error } from '../validation'

export class WhileActivity extends Activity {
    static override readonly typeName = 'Cike.While'

    condition: Input | null
    body: Activity | null = null

    /** 不传参 = 后端默认值 false 字面量；显式传 null 表示无输入 */
    constructor(condition: Input | null | undefined = Input.literal(false)) {
        super(WhileActivity.typeName)
        this.condition = condition
    }

    static fromJson(json: ActivityJson): WhileActivity {
        const activity = new WhileActivity()
        activity.applyBaseJson(json)
        activity.condition = readInput(json, 'condition', Input.literal(false))
        activity.body = parsePort(json['body'])
        return activity
    }

    override toJson(): ActivityJson {
        return {
            ...this.baseJson(),
            condition: this.condition ? this.condition.toJson() : null,
            body: this.body ? this.body.toJson() : null,
        }
    }

    /** 后端 While 的 Body 为 [Port]，纳入子活动校验递归 */
    protected override childActivities(): Activity[] {
        return this.body ? [this.body] : []
    }

    override validate(): ActivityDiagnostic[] {
        const diagnostics = super.validate()
        if (this.condition === null) {
            diagnostics.push(error('WHILE_CONDITION_MISSING', 'While 节点缺少 condition 输入', 'condition', this))
        }
        return diagnostics
    }
}
