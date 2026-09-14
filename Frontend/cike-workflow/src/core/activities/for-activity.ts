/**
 * 计数循环。对应后端 Cike.Workflow.Core.Activities.For（[Activity("Cike", "Looping")]）。
 * 线格式属性：start / end / step（Input<int>）、outerBoundInclusive（Input<bool>，默认 true）、
 * body（[Port] IActivity?）、currentValue（[Output]）。
 */

import { Activity, type ActivityJson } from './activity'
import { parsePort } from './registry'
import { Input, readInput } from '../memory/input'
import { Output, readOutput } from '../memory/output'

export class ForActivity extends Activity {
    static override readonly typeName = 'Cike.For'

    start: Input | null = Input.literal(0)
    end: Input | null = Input.literal(0)
    step: Input | null = Input.literal(1)
    outerBoundInclusive: Input | null = Input.literal(true)
    body: Activity | null = null
    currentValue: Output | null = null

    constructor() {
        super(ForActivity.typeName)
    }

    static fromJson(json: ActivityJson): ForActivity {
        const activity = new ForActivity()
        activity.applyBaseJson(json)
        activity.start = readInput(json, 'start', activity.start)
        activity.end = readInput(json, 'end', activity.end)
        activity.step = readInput(json, 'step', activity.step)
        activity.outerBoundInclusive = readInput(json, 'outerBoundInclusive', activity.outerBoundInclusive)
        activity.body = parsePort(json['body'])
        activity.currentValue = readOutput(json, 'currentValue')
        return activity
    }

    override toJson(): ActivityJson {
        return {
            ...this.baseJson(),
            start: this.start ? this.start.toJson() : null,
            end: this.end ? this.end.toJson() : null,
            step: this.step ? this.step.toJson() : null,
            outerBoundInclusive: this.outerBoundInclusive ? this.outerBoundInclusive.toJson() : null,
            body: this.body ? this.body.toJson() : null,
            currentValue: this.currentValue ? this.currentValue.toJson() : null,
        }
    }

    /** 后端 For 的 Body 为 [Port]，纳入子活动校验递归 */
    protected override childActivities(): Activity[] {
        return this.body ? [this.body] : []
    }
}
