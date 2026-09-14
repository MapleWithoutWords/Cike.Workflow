/**
 * 使工作流故障。对应后端 Cike.Workflow.Core.Activities.Fault（[Activity("Cike", "Primitives")]）。
 * 线格式属性：code / category / faultType / message（均为 Input<string>，注意 C# 属性名 FaultType 序列化为 faultType）。
 */

import { Activity, type ActivityJson } from './activity'
import { Input, readInput } from '../memory/input'
import type { ActivityDiagnostic } from '../validation'
import { error } from '../validation'

export class FaultActivity extends Activity {
    static override readonly typeName: string = 'Cike.Fault'

    /**
     * 故障码。对应后端 Fault.Code（Input<string>）。
     * 注意：后端 Fault 用同名 Input 属性遮蔽了基类 Activity.Code（string）——
     * 这是 C# 侧 CS0108 隐藏行为，wire 上 "code" 键对 Fault 节点即指本字段。
     * 因此这里命名为 faultCode 以避开基类字段，序列化仍写回 "code"。
     */
    faultCode: Input | null = null
    /** 故障分类（HTTP / Alteration 等） */
    category: Input | null = null
    /** 故障类型（System / Business / Integration 等） */
    faultType: Input | null = null
    /** 故障消息 */
    message: Input | null = null

    constructor() {
        super(FaultActivity.typeName)
    }

    static fromJson(json: ActivityJson): FaultActivity {
        const activity = new FaultActivity()
        activity.applyBaseJson(json)
        activity.faultCode = readInput(json, 'code')
        activity.category = readInput(json, 'category')
        activity.faultType = readInput(json, 'faultType')
        activity.message = readInput(json, 'message')
        return activity
    }

    override toJson(): ActivityJson {
        // code 键被故障码 Input 遮蔽（基类 code 字符串在此丢失），与后端行为一致
        return {
            ...this.baseJson(),
            code: this.faultCode ? this.faultCode.toJson() : null,
            category: this.category ? this.category.toJson() : null,
            faultType: this.faultType ? this.faultType.toJson() : null,
            message: this.message ? this.message.toJson() : null,
        }
    }

    override validate(): ActivityDiagnostic[] {
        const diagnostics = super.validate()
        if (this.faultCode === null) {
            diagnostics.push(error('FAULT_CODE_MISSING', 'Fault 节点缺少 code 输入', 'code', this))
        }
        if (this.category === null) {
            diagnostics.push(error('FAULT_CATEGORY_MISSING', 'Fault 节点缺少 category 输入', 'category', this))
        }
        if (this.faultType === null) {
            diagnostics.push(error('FAULT_TYPE_MISSING', 'Fault 节点缺少 faultType 输入', 'faultType', this))
        }
        return diagnostics
    }
}
