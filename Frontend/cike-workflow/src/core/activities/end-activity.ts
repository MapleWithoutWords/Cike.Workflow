/**
 * 流程终点（终端节点）。对应后端 Cike.Workflow.Core.Activities.End（[Activity("Cike")]）。
 * 后端 End 实现 ITerminalNode：执行到它时所在 Sequence/Flowchart 立即整体完成。
 */

import { Activity, type ActivityJson } from './activity'

export class EndActivity extends Activity {
    static override readonly typeName = 'Cike.End'

    constructor(id?: string) {
        super(EndActivity.typeName)
        if (id !== undefined) {
            this.id = id
        }
    }

    static fromJson(json: ActivityJson): EndActivity {
        const activity = new EndActivity()
        activity.applyBaseJson(json)
        return activity
    }

    override toJson(): ActivityJson {
        return this.baseJson()
    }
}
