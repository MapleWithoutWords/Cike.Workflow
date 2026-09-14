/**
 * 内置活动注册表。设计器启动时（导入本模块）即完成注册，与后端 ActivityRegistry 的内置集对齐。
 * 后续扩展业务节点（审批、Agent 等）时，在此追加 registerActivity(...) 即可被 parseActivity 识别。
 */

import { registerActivity } from './registry'
import { StartActivity } from './start-activity'
import { EndActivity } from './end-activity'
import { BreakActivity } from './break-activity'
import { WriteLineActivity } from './write-line-activity'
import { FaultActivity } from './fault-activity'
import { IfActivity } from './if-activity'
import { SwitchActivity } from './switch-activity'
import { WhileActivity } from './while-activity'
import { ForActivity } from './for-activity'
import { ForEachActivity } from './for-each-activity'
import { SequenceActivity } from './sequence-activity'
import { ParallelActivity } from './parallel-activity'
import { FlowchartActivity } from './flowchart-activity'
import { WorkflowActivity } from './workflow-activity'

export const BUILTIN_ACTIVITY_CLASSES = [
    StartActivity,
    EndActivity,
    BreakActivity,
    WriteLineActivity,
    FaultActivity,
    IfActivity,
    SwitchActivity,
    WhileActivity,
    ForActivity,
    ForEachActivity,
    SequenceActivity,
    ParallelActivity,
    FlowchartActivity,
    WorkflowActivity,
] as const

for (const activityClass of BUILTIN_ACTIVITY_CLASSES) {
    registerActivity(activityClass)
}
