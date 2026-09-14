/**
 * 集合遍历。对应后端 Cike.Workflow.Core.Activities.ForEach（非泛型，type 串 "Cike.ForEach"）。
 * 线格式属性：items（Input<ICollection>，默认空数组）、body（[Port]）、currentValue（[Output]）。
 *
 * 后端泛型 ForEach<T> 会生成 "ForEach<...>" 形态的类型名，画布上不出现，故不建模。
 */

import { Activity, type ActivityJson } from './activity'
import { parsePort } from './registry'
import { Input, readInput } from '../memory/input'
import { Output, readOutput } from '../memory/output'
import type { ActivityDiagnostic } from '../validation'
import { error } from '../validation'

export class ForEachActivity extends Activity {
    static override readonly typeName = 'Cike.ForEach'

    items: Input | null = Input.literal([])
    body: Activity | null = null
    currentValue: Output | null = null

    constructor() {
        super(ForEachActivity.typeName)
    }

    static fromJson(json: ActivityJson): ForEachActivity {
        const activity = new ForEachActivity()
        activity.applyBaseJson(json)
        activity.items = readInput(json, 'items', activity.items)
        activity.body = parsePort(json['body'])
        activity.currentValue = readOutput(json, 'currentValue')
        return activity
    }

    override toJson(): ActivityJson {
        return {
            ...this.baseJson(),
            items: this.items ? this.items.toJson() : null,
            body: this.body ? this.body.toJson() : null,
            currentValue: this.currentValue ? this.currentValue.toJson() : null,
        }
    }

    /** 后端 ForEach 的 Body 为 [Port]，纳入子活动校验递归 */
    protected override childActivities(): Activity[] {
        return this.body ? [this.body] : []
    }

    override validate(): ActivityDiagnostic[] {
        const diagnostics = super.validate()
        if (this.items === null) {
            diagnostics.push(error('FOREACH_ITEMS_MISSING', 'ForEach 节点缺少 items 输入', 'items', this))
        }
        return diagnostics
    }
}
