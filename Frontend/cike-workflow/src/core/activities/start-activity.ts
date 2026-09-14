/** 流程起点。对应后端 Cike.Workflow.Core.Activities.Start（[Activity("Cike")]）。 */

import { Activity, type ActivityJson } from './activity'

export class StartActivity extends Activity {
    static override readonly typeName = 'Cike.Start'

    constructor(id?: string) {
        super(StartActivity.typeName)
        if (id !== undefined) {
            this.id = id
        }
    }

    static fromJson(json: ActivityJson): StartActivity {
        const activity = new StartActivity()
        activity.applyBaseJson(json)
        return activity
    }

    override toJson(): ActivityJson {
        return this.baseJson()
    }
}
