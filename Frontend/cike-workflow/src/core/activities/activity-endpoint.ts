/**
 * 连线端点。对应后端 Cike.Workflow.Core.Activities.FlowchartActivity.Models.ActivityEndpoint。
 * port 为来源活动的出口端口名（如 Done / True / case label），目标端通常为空。
 */

export interface ActivityEndpointJson {
    activityId?: string | null
    port?: string | null
}

export class ActivityEndpoint {
    activityId: string | null = null
    port: string | null = null

    constructor(activityId: string | null = null, port: string | null = null) {
        this.activityId = activityId
        this.port = port
    }

    static fromJson(json: ActivityEndpointJson | null | undefined): ActivityEndpoint {
        return new ActivityEndpoint(json?.activityId ?? null, json?.port ?? null)
    }

    static parseList(raw: unknown): ActivityEndpoint[] {
        if (!Array.isArray(raw)) {
            return []
        }
        return raw.map(item => ActivityEndpoint.fromJson(item as ActivityEndpointJson))
    }

    toJson(): ActivityEndpointJson {
        return { activityId: this.activityId, port: this.port }
    }
}
