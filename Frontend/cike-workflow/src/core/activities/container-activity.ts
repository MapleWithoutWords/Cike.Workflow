/**
 * 容器活动基类：持有子活动集合与变量集合。
 * 对应后端 Cike.Workflow.Core.Activities.Abstracts.ContainerActivity
 */

import { Activity, type ActivityJson } from './activity'
import { parseActivityList } from './registry'
import { Variable } from '../memory/variable'

export abstract class ContainerActivity extends Activity {
    activities: Activity[] = []
    variables: Variable[] = []

    protected constructor(type: string) {
        super(type)
    }

    protected applyContainerJson(json: ActivityJson): void {
        this.activities = parseActivityList(json.activities)
        this.variables = Variable.parseList(json.variables)
    }

    protected containerJson(): ActivityJson {
        return {
            activities: this.activities.map(a => a.toJson()),
            variables: this.variables.map(v => v.toJson()),
        }
    }

    /** 容器默认把全部子活动纳入校验递归；Flowchart 等可覆写 childActivities */
    protected override childActivities(): Activity[] {
        return this.activities
    }

    /** 按 id 查找直接子活动 */
    findChildById(id: string): Activity | undefined {
        return this.activities.find(a => a.id === id)
    }
}
