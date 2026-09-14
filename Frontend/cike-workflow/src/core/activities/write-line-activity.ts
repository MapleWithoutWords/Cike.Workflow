/**
 * 输出一行文本。对应后端 Cike.Workflow.Core.Activities.WriteLine（[Activity("Cike", "Console")]）。
 * 线格式属性名：text（Input<string>）。
 */

import { Activity, type ActivityJson } from './activity'
import { Input, readInput } from '../memory/input'
import type { ActivityDiagnostic } from '../validation'
import { error } from '../validation'

export class WriteLineActivity extends Activity {
    static override readonly typeName = 'Cike.WriteLine'

    text: Input | null

    constructor(text?: Input | null) {
        super(WriteLineActivity.typeName)
        this.text = text ?? null
    }

    static fromJson(json: ActivityJson): WriteLineActivity {
        const activity = new WriteLineActivity()
        activity.applyBaseJson(json)
        activity.text = readInput(json, 'text')
        return activity
    }

    override toJson(): ActivityJson {
        return { ...this.baseJson(), text: this.text ? this.text.toJson() : null }
    }

    override validate(): ActivityDiagnostic[] {
        const diagnostics = super.validate()
        if (this.text === null) {
            diagnostics.push(error('WRITELINE_TEXT_MISSING', 'WriteLine 节点缺少 text 输入', 'text', this))
        }
        return diagnostics
    }
}
