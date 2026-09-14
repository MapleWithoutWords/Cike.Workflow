/**
 * 活动（节点）基类
 * 对应后端 Cike.Workflow.Core.Activities.Abstracts.Activity / IActivity
 *
 * wire 形状与后端 ActivityJsonConverter 输出一致：
 * 公共字段（id/nodeId/code/name/type/version/customProperties/metadata）总是存在，
 * 未赋值时显式为 null——这是该转换器手写序列化（绕过 WhenWritingNull）的结果。
 */

import type { ActivityDiagnostic } from '../validation'
import { error } from '../validation'
import type { MergeMode } from './enums'

export interface ActivityJson {
    id?: string | null
    nodeId?: string | null
    /** 基类为 string；个别节点（Fault）用 Input 遮蔽同名属性，故放宽为 unknown */
    code?: unknown
    name?: string | null
    type?: string | null
    version?: number | null
    customProperties?: Record<string, unknown> | null
    metadata?: Record<string, unknown> | null
    [key: string]: unknown
}

/**
 * 活动类的静态契约：注册表按 typeName 分发反序列化与元信息。
 */
export interface ActivityClass {
    readonly typeName: string
    /** 画布出口端口（默认 Done），对应后端 [FlowNode(...)] */
    readonly outcomes: readonly string[]
    fromJson(json: ActivityJson): Activity
    new (): Activity
}

export abstract class Activity {
    /** 子类必须覆写为对应的后端 type 判别串（如 "Cike.Start"）；基类不直接实例化 */
    static readonly typeName: string = 'Cike.Activity'
    static readonly outcomes: readonly string[] = ['Done']

    id: string | null = null
    /** 节点 Path，后端在标识图构建时赋值 */
    nodeId: string | null = null
    code: string | null = null
    name: string | null = null
    type: string
    version = 1
    customProperties: Record<string, unknown> = {}
    metadata: Record<string, unknown> = {}

    protected constructor(type: string) {
        this.type = type
    }

    /** 创建实例并应用 wire 公共字段（fromJson 各子类实现复用） */
    protected applyBaseJson(json: ActivityJson): void {
        this.id = json.id ?? null
        this.nodeId = json.nodeId ?? null
        // 个别节点（如 Fault）用同名 Input 遮蔽了基类的 string code；
        // 仅当值确为字符串时才落到基类字段，对象形态留给子类自身解析。
        this.code = typeof json.code === 'string' ? json.code : null
        this.name = json.name ?? null
        this.version = json.version ?? 1
        this.customProperties = { ...json.customProperties }
        this.metadata = { ...json.metadata }
    }

    protected baseJson(): ActivityJson {
        return {
            id: this.id,
            nodeId: this.nodeId,
            code: this.code,
            name: this.name,
            type: this.type,
            version: this.version,
            customProperties: { ...this.customProperties },
            metadata: { ...this.metadata },
        }
    }

    /** 对应后端 IActivity.GetMergeMode/SetMergeMode：MergeMode.None 等价于未设置 */
    getMergeMode(): MergeMode | null {
        const value = this.customProperties['mergeMode']
        if (typeof value !== 'string' || value.length === 0) {
            return null
        }
        const match = ['None', 'Stream', 'Merge', 'Converge', 'Cascade', 'Race'].find(
            m => m.toLowerCase() === value.toLowerCase(),
        )
        return match && match !== 'None' ? (match as MergeMode) : null
    }

    setMergeMode(mode: MergeMode | null): void {
        if (mode === null || mode === 'None') {
            delete this.customProperties['mergeMode']
        } else {
            this.customProperties['mergeMode'] = mode
        }
    }

    /** 对应后端 IActivity.GetCanStartWorkflow/SetCanStartWorkflow（两种大小写键都读取） */
    getCanStartWorkflow(): boolean {
        const camel = this.customProperties['canStartWorkflow']
        const pascal = this.customProperties['CanStartWorkflow']
        const value = camel ?? pascal
        return typeof value === 'boolean' ? value : false
    }

    setCanStartWorkflow(value: boolean): void {
        this.customProperties['canStartWorkflow'] = value
    }

    /**
     * 实例级出口端口。默认取类静态的 outcomes（对应后端 [FlowNode(...)]），
     * Switch 等按内容动态产生端口的节点覆写此方法。
     */
    getFlowOutcomes(): readonly string[] {
        return (this.constructor as ActivityClass).outcomes
    }

    /**
     * 参与递归校验的子活动集合。容器与带 [Port] 的节点覆写此方法；
     * 默认无子活动。
     */
    protected childActivities(): Activity[] {
        return []
    }

    /** 子类覆写时先取 super.validate() 再追加自身规则；子活动校验自动递归 */
    validate(): ActivityDiagnostic[] {
        const diagnostics: ActivityDiagnostic[] = []
        if (!this.id) {
            diagnostics.push(error('ACTIVITY_ID_MISSING', '节点缺少 id', 'id', this))
        }
        for (const child of this.childActivities()) {
            diagnostics.push(...child.validate())
        }
        return diagnostics
    }

    abstract toJson(): ActivityJson
}
