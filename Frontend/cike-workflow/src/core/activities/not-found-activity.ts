/**
 * 类型未注册时的占位活动，保留原始 JSON 以便画布不丢数据。
 * 对应后端 Cike.Workflow.Core.Activities.NotFoundActivity（[Activity(Namespace="Cike", Type="NotFound")]）。
 */

import { Activity, type ActivityJson } from './activity'
import type { ActivityDiagnostic } from '../validation'
import { error } from '../validation'

export class NotFoundActivity extends Activity {
    static override readonly typeName = 'Cike.NotFound'

    /** 缺失的活动类型名 */
    missingTypeName: string | null = null
    missingTypeVersion = 0
    /** 原始活动 JSON（后端保留为字符串；前端同样保留，保证回写不丢数据） */
    originalActivityJson: string | null = null

    constructor() {
        super(NotFoundActivity.typeName)
    }

    /** 供 registry 在类型未注册时构造占位 */
    static fromUnknown(json: ActivityJson): NotFoundActivity {
        const activity = new NotFoundActivity()
        activity.applyBaseJson(json)
        activity.missingTypeName = typeof json.type === 'string' ? json.type : null
        activity.originalActivityJson = JSON.stringify(json)
        return activity
    }

    /** 解析保留的原始 JSON；解析失败返回 null */
    parseOriginal(): ActivityJson | null {
        if (this.originalActivityJson === null) {
            return null
        }
        try {
            return JSON.parse(this.originalActivityJson) as ActivityJson
        } catch {
            return null
        }
    }

    static fromJson(json: ActivityJson): NotFoundActivity {
        const activity = new NotFoundActivity()
        activity.applyBaseJson(json)
        activity.missingTypeName = (json['missingTypeName'] as string | null | undefined) ?? null
        activity.missingTypeVersion = (json['missingTypeVersion'] as number | null | undefined) ?? 0
        activity.originalActivityJson = (json['originalActivityJson'] as string | null | undefined) ?? null
        return activity
    }

    override toJson(): ActivityJson {
        return {
            ...this.baseJson(),
            missingTypeName: this.missingTypeName,
            missingTypeVersion: this.missingTypeVersion,
            originalActivityJson: this.originalActivityJson,
        }
    }

    override validate(): ActivityDiagnostic[] {
        const diagnostics = super.validate()
        diagnostics.push(error('ACTIVITY_TYPE_NOT_FOUND', `未注册的活动类型：${this.missingTypeName ?? '(未知)'}`, 'type', this))
        return diagnostics
    }
}
