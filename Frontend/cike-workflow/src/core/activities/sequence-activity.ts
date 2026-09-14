/**
 * 顺序执行子活动。对应后端 Cike.Workflow.Core.Activities.Sequence（[Activity("Cike","Workflows")]）。
 * 子活动集合来自 ContainerActivity.Activities；遇到 ITerminalNode（End/Break）整体提前完成。
 */

import { ContainerActivity } from './container-activity'
import type { ActivityJson } from './activity'

export class SequenceActivity extends ContainerActivity {
    static override readonly typeName = 'Cike.Sequence'

    constructor() {
        super(SequenceActivity.typeName)
    }

    static fromJson(json: ActivityJson): SequenceActivity {
        const activity = new SequenceActivity()
        activity.applyBaseJson(json)
        activity.applyContainerJson(json)
        return activity
    }

    override toJson(): ActivityJson {
        return { ...this.baseJson(), ...this.containerJson() }
    }
}
