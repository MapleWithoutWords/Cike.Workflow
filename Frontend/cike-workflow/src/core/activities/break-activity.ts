/**
 * 跳出循环（终端节点）。对应后端 Cike.Workflow.Core.Activities.Break（[Activity("Cike", "Looping")]）。
 * 后端 Break 发送 BreakSignal 并由 ITerminalNode 语义终止外层容器。
 */

import { Activity, type ActivityJson } from './activity'

export class BreakActivity extends Activity {
    static override readonly typeName = 'Cike.Break'

    constructor(id?: string) {
        super(BreakActivity.typeName)
        if (id !== undefined) {
            this.id = id
        }
    }

    static fromJson(json: ActivityJson): BreakActivity {
        const activity = new BreakActivity()
        activity.applyBaseJson(json)
        return activity
    }

    override toJson(): ActivityJson {
        return this.baseJson()
    }
}
