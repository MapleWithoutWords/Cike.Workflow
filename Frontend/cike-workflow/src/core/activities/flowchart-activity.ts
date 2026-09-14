/**
 * 流程图：子活动 + 连线的集合，设计器主画布对应的活动类型。
 * 对应后端 Cike.Workflow.Core.Activities.FlowchartActivity.Flowchart（[Activity("Cike","Flow")]）。
 *
 * 线格式属性：start（[Port] IActivity?，入口活动）、connections（ActivityConnection[]）。
 * 连线拓扑语义：connection.source.port 需匹配来源活动完成时发出的 outcome
 * （默认 Done；If 为 True/False；Switch 为 case label / Default）。
 */

import { ContainerActivity } from './container-activity'
import { parsePort } from './registry'
import { Activity, type ActivityJson } from './activity'
import { ActivityConnection } from './activity-connection'
import type { ActivityDiagnostic } from '../validation'
import { error, warning } from '../validation'

export class FlowchartActivity extends ContainerActivity {
    static override readonly typeName = 'Cike.Flowchart'

    /** 显式入口活动（须为 activities 成员）；null 时后端按 Start 标记 / 无入边节点等规则推断 */
    start: Activity | null = null
    connections: ActivityConnection[] = []

    constructor() {
        super(FlowchartActivity.typeName)
    }

    static fromJson(json: ActivityJson): FlowchartActivity {
        const activity = new FlowchartActivity()
        activity.applyBaseJson(json)
        activity.applyContainerJson(json)
        activity.start = parsePort(json['start'])
        activity.connections = ActivityConnection.parseList(json.connections)
        return activity
    }

    override toJson(): ActivityJson {
        return {
            ...this.baseJson(),
            ...this.containerJson(),
            start: this.start ? this.start.toJson() : null,
            connections: this.connections.map(c => c.toJson()),
        }
    }

    /** 添加连线，自动去重（source+port+target 全等时不重复添加） */
    addConnection(connection: ActivityConnection): void {
        const duplicate = this.connections.some(
            c => c.source.activityId === connection.source.activityId
                && c.source.port === connection.source.port
                && c.target.activityId === connection.target.activityId,
        )
        if (!duplicate) {
            this.connections.push(connection)
        }
    }

    /** 移除两节点间（含端口）的全部连线，返回移除数量 */
    removeConnections(sourceId: string, targetId: string): number {
        const before = this.connections.length
        this.connections = this.connections.filter(
            c => !(c.source.activityId === sourceId && c.target.activityId === targetId),
        )
        return before - this.connections.length
    }

    /** 入口推断镜像后端 GetStartActivity 的前两级规则：显式 start → 标记 canStartWorkflow 的节点 */
    resolveStartActivity(): Activity | null {
        if (this.start !== null) {
            return this.start
        }
        return this.activities.find(a => a.getCanStartWorkflow()) ?? null
    }

    override validate(): ActivityDiagnostic[] {
        const diagnostics = super.validate()
        const byId = new Map<string, Activity>()
        for (const child of this.activities) {
            if (child.id && byId.has(child.id)) {
                diagnostics.push(error('FLOWCHART_DUPLICATE_ID', `Flowchart 内存在重复节点 id：${child.id}`, 'activities', this))
            }
            if (child.id) {
                byId.set(child.id, child)
            }
        }
        if (this.start !== null && !this.activities.some(a => a === this.start)) {
            diagnostics.push(error('FLOWCHART_START_NOT_IN_ACTIVITIES', 'Flowchart 的 start 节点必须是 activities 成员', 'start', this))
        }
        for (const [i, connection] of this.connections.entries()) {
            const source = connection.source.activityId ? byId.get(connection.source.activityId) : undefined
            const target = connection.target.activityId ? byId.get(connection.target.activityId) : undefined
            if (!source) {
                diagnostics.push(error('FLOWCHART_CONNECTION_SOURCE_MISSING', `连线 ${connection.toString()} 的源节点不存在`, `connections[${i}].source`, this))
            }
            if (!target) {
                diagnostics.push(error('FLOWCHART_CONNECTION_TARGET_MISSING', `连线 ${connection.toString()} 的目标节点不存在`, `connections[${i}].target`, this))
            }
            if (source && connection.source.port && !source.getFlowOutcomes().includes(connection.source.port)) {
                diagnostics.push(warning(
                    'FLOWCHART_CONNECTION_PORT_UNKNOWN',
                    `连线源端口 ${connection.source.port} 不在节点 ${source.id} 的出口端口中`,
                    `connections[${i}].source.port`,
                    this,
                ))
            }
        }
        return diagnostics
    }
}
