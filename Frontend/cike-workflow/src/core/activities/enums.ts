/**
 * 活动 JSON 层（画布 wire）使用的字符串枚举。
 * 后端活动序列化启用 JsonStringEnumConverter，所以这些枚举在线上都是字符串；
 * HTTP DTO 层的枚举（如 WorkflowDefinitionType）是数字，归 swagger 生成类型管。
 */

/** 对应后端 Cike.Workflow.Core.Activities.FlowchartActivity.Models.MergeMode */
export const MergeMode = {
    None: 'None',
    Stream: 'Stream',
    Merge: 'Merge',
    Converge: 'Converge',
    Cascade: 'Cascade',
    Race: 'Race',
} as const
export type MergeMode = typeof MergeMode[keyof typeof MergeMode]

export const MERGE_MODE_VALUES: readonly MergeMode[] = Object.values(MergeMode)

export function isMergeMode(value: unknown): value is MergeMode {
    return typeof value === 'string' && (MERGE_MODE_VALUES as readonly string[]).includes(value)
}

/** 对应后端 Cike.Workflow.Core.Activities.SwitchMode（wire 上可能为字符串或序号，读取时用 SwitchActivity.readMode 归一） */
export const SwitchMode = {
    MatchFirst: 'MatchFirst',
    MatchAny: 'MatchAny',
} as const
export type SwitchMode = typeof SwitchMode[keyof typeof SwitchMode]

/** 按 C# 枚举声明顺序，用于把序号归一为字符串 */
export const SWITCH_MODE_VALUES: readonly SwitchMode[] = [SwitchMode.MatchFirst, SwitchMode.MatchAny]
