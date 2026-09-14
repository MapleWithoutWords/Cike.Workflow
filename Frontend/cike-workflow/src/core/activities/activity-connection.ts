/**
 * 活动连线。对应后端 Cike.Workflow.Core.Activities.FlowchartActivity.Models.ActivityConnection。
 * 连线不参与执行判定逻辑本身，仅提供 Flowchart 的拓扑（source.port 匹配来源活动发出的 outcome）。
 */

import { ActivityEndpoint, type ActivityEndpointJson } from './activity-endpoint'
import type { Activity } from './activity'

export interface ActivityConnectionJson {
    source?: ActivityEndpointJson | null
    target?: ActivityEndpointJson | null
    customProperties?: Record<string, unknown> | null
}

export class ActivityConnection {
    source: ActivityEndpoint
    target: ActivityEndpoint
    customProperties: Record<string, unknown> = {}

    constructor(source: ActivityEndpoint, target: ActivityEndpoint) {
        this.source = source
        this.target = target
    }

    /** 对应 C# `ActivityConnection(IActivity source, IActivity target)`：不带端口的两点直连 */
    static between(source: Activity, target: Activity): ActivityConnection {
        return new ActivityConnection(
            new ActivityEndpoint(source.id),
            new ActivityEndpoint(target.id),
        )
    }

    /** 带端口构造 */
    static from(source: Activity, port: string | null, target: Activity): ActivityConnection {
        return new ActivityConnection(
            new ActivityEndpoint(source.id, port),
            new ActivityEndpoint(target.id),
        )
    }

    static fromJson(json: ActivityConnectionJson | null | undefined): ActivityConnection {
        return new ActivityConnection(
            ActivityEndpoint.fromJson(json?.source),
            ActivityEndpoint.fromJson(json?.target),
        )
    }

    static parseList(raw: unknown): ActivityConnection[] {
        if (!Array.isArray(raw)) {
            return []
        }
        return raw.map(item => ActivityConnection.fromJson(item as ActivityConnectionJson))
    }

    toJson(): ActivityConnectionJson {
        return {
            source: this.source.toJson(),
            target: this.target.toJson(),
            customProperties: { ...this.customProperties },
        }
    }

    /** 对应后端 ToString 的展示形式：src[:port]->dst[:port] */
    toString(): string {
        const src = `${this.source.activityId ?? ''}${this.source.port ? `:${this.source.port}` : ''}`
        const dst = `${this.target.activityId ?? ''}${this.target.port ? `:${this.target.port}` : ''}`
        return `${src}->${dst}`
    }
}
