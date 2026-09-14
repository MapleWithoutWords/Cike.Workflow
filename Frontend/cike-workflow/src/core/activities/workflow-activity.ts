/**
 * 材料化工作流（CompositeActivity）：后端把定义物化为可执行工作流时的根活动。
 * 对应后端 Cike.Workflow.Core.Activities.WorkflowActivity（[Activity(Namespace="Cike", Type="Workflow")]）。
 *
 * 设计器保存的 Root 通常是 Flowchart；本类用于解析 materialized 形态
 * （如执行记录 ActivityState 中出现的完整工作流对象）。
 *
 * 注意后端 CompositeActivity.Variables 为 [JsonIgnore]，不上 wire——
 * 画布级变量在 SaveWorkflowDefinitionDto.options.variables（swagger 类型）。
 */

import { Activity, type ActivityJson } from './activity'
import { parsePort } from './registry'
import type { ExpressionJson } from '../memory/expression'
import type { ActivityDiagnostic } from '../validation'
import { error } from '../validation'

export interface ArgumentDefinitionJson {
    type?: string | null
    isArray?: boolean | null
    name?: string | null
    displayName?: string | null
    description?: string | null
    defaultValue?: ExpressionJson | null
    storageDriverType?: string | null
}

/** 对应后端 WorkflowDefinitionInfo；long 字段经 HTTP 层序列化为字符串 */
export interface WorkflowDefinitionInfoJson {
    id?: string | null
    definitionId?: string | null
    version?: number | null
    tenantId?: string | null
    isLatest?: boolean | null
    isPublished?: boolean | null
    name?: string | null
    description?: string | null
    usableAsActivity?: boolean | null
    isReadonly?: boolean | null
    isSystem?: boolean | null
}

export class WorkflowActivity extends Activity {
    static override readonly typeName = 'Cike.Workflow'

    /** [Port] 根活动 */
    root: Activity | null = null
    inputs: ArgumentDefinitionJson[] = []
    outputs: ArgumentDefinitionJson[] = []
    /** 工作流可选的完成结局名 */
    outcomes: string[] = []
    isReadonly = false
    isSystem = false
    definitionInfo: WorkflowDefinitionInfoJson | null = null

    constructor() {
        super(WorkflowActivity.typeName)
    }

    static fromJson(json: ActivityJson): WorkflowActivity {
        const activity = new WorkflowActivity()
        activity.applyBaseJson(json)
        activity.root = parsePort(json['root'])
        activity.inputs = Array.isArray(json.inputs) ? (json.inputs as ArgumentDefinitionJson[]) : []
        activity.outputs = Array.isArray(json.outputs) ? (json.outputs as ArgumentDefinitionJson[]) : []
        activity.outcomes = Array.isArray(json.outcomes) ? (json.outcomes as string[]) : []
        activity.isReadonly = (json['isReadonly'] as boolean | null | undefined) ?? false
        activity.isSystem = (json['isSystem'] as boolean | null | undefined) ?? false
        activity.definitionInfo = (json['definitionInfo'] as WorkflowDefinitionInfoJson | null | undefined) ?? null
        return activity
    }

    override toJson(): ActivityJson {
        return {
            ...this.baseJson(),
            root: this.root ? this.root.toJson() : null,
            inputs: this.inputs,
            outputs: this.outputs,
            outcomes: this.outcomes,
            isReadonly: this.isReadonly,
            isSystem: this.isSystem,
            definitionInfo: this.definitionInfo,
        }
    }

    protected override childActivities(): Activity[] {
        return this.root ? [this.root] : []
    }

    override validate(): ActivityDiagnostic[] {
        const diagnostics = super.validate()
        if (this.root === null) {
            diagnostics.push(error('WORKFLOW_ROOT_MISSING', 'Workflow 节点缺少 root 活动', 'root', this))
        }
        return diagnostics
    }
}
