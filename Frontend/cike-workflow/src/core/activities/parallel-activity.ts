/**
 * 并行执行全部子活动。对应后端 Cike.Workflow.Core.Activities.Parallel（[Activity("Cike","Workflows")]）。
 */

import { ContainerActivity } from './container-activity'
import type { ActivityJson } from './activity'

export class ParallelActivity extends ContainerActivity {
    static override readonly typeName = 'Cike.Parallel'

    constructor() {
        super(ParallelActivity.typeName)
    }

    static fromJson(json: ActivityJson): ParallelActivity {
        const activity = new ParallelActivity()
        activity.applyBaseJson(json)
        activity.applyContainerJson(json)
        return activity
    }

    override toJson(): ActivityJson {
        return { ...this.baseJson(), ...this.containerJson() }
    }
}
