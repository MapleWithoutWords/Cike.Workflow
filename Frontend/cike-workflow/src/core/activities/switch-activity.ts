/**
 * 多路分支。对应后端 Cike.Workflow.Core.Activities.Switch（[FlowNode("Default")]）。
 * 线格式属性：cases（FlowSwitchCase[]）、mode（Input<SwitchMode>，默认 MatchFirst）。
 */

import { Activity, type ActivityJson } from './activity'
import { Input, readInput } from '../memory/input'
import { FlowSwitchCase } from './flow-switch-case'
import { SWITCH_MODE_VALUES, SwitchMode, type SwitchMode as SwitchModeType } from './enums'
import type { ActivityDiagnostic } from '../validation'
import { error } from '../validation'

export class SwitchActivity extends Activity {
    static override readonly typeName = 'Cike.Switch'
    static override readonly outcomes = ['Default'] as const

    cases: FlowSwitchCase[] = []
    mode: Input | null

    /** 不传参 = 后端默认值 MatchFirst 字面量；显式传 null 表示无输入 */
    constructor(mode: Input | null | undefined = Input.literal(SwitchMode.MatchFirst)) {
        super(SwitchActivity.typeName)
        this.mode = mode
    }

    /** 后端 [FlowNode("Default")] + 每个 case 的 label 都是出口端口 */
    override getFlowOutcomes(): string[] {
        return [...this.cases.map(c => c.label).filter(l => l.length > 0), 'Default']
    }

    /**
     * 读取 mode 的 SwitchMode 值。
     * 后端 HTTP 响应可能把枚举写成序号（如 0/1），活动 JSON 写字符串；此处统一归一为字符串。
     */
    readMode(): SwitchModeType | null {
        const value = this.mode?.getLiteralValue()
        if (typeof value === 'string') {
            const match = SWITCH_MODE_VALUES.find(m => m.toLowerCase() === value.toLowerCase())
            return match ?? null
        }
        if (typeof value === 'number' && Number.isInteger(value) && value >= 0 && value < SWITCH_MODE_VALUES.length) {
            return SWITCH_MODE_VALUES[value]
        }
        return null
    }

    static fromJson(json: ActivityJson): SwitchActivity {
        const activity = new SwitchActivity()
        activity.applyBaseJson(json)
        activity.cases = FlowSwitchCase.parseList(json.cases)
        activity.mode = readInput(json, 'mode', Input.literal(SwitchMode.MatchFirst))
        return activity
    }

    override toJson(): ActivityJson {
        return {
            ...this.baseJson(),
            cases: this.cases.map(c => c.toJson()),
            mode: this.mode ? this.mode.toJson() : null,
        }
    }

    override validate(): ActivityDiagnostic[] {
        const diagnostics = super.validate()
        const seen = new Set<string>()
        for (const [i, item] of this.cases.entries()) {
            if (item.label.length === 0) {
                diagnostics.push(error('SWITCH_CASE_LABEL_MISSING', `Switch 第 ${i + 1} 个 case 缺少 label`, `cases[${i}].label`, this))
            } else if (seen.has(item.label)) {
                diagnostics.push(error('SWITCH_CASE_LABEL_DUPLICATED', `Switch case label 重复：${item.label}`, `cases[${i}].label`, this))
            }
            seen.add(item.label)
        }
        if (this.mode === null) {
            diagnostics.push(error('SWITCH_MODE_MISSING', 'Switch 节点缺少 mode 输入', 'mode', this))
        } else if (this.readMode() === null) {
            diagnostics.push(error('SWITCH_MODE_INVALID', 'Switch mode 取值非法（MatchFirst / MatchAny）', 'mode', this))
        }
        return diagnostics
    }
}
