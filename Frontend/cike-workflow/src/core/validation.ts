/**
 * 节点校验诊断的公共定义。
 * 校验行为内聚在各 Activity 类的 validate() 中，这里只提供契约与工厂函数。
 */

export type DiagnosticSeverity = 'error' | 'warning'

export interface ActivityDiagnostic {
    severity: DiagnosticSeverity
    /** 机器可读的稳定编码，供 UI 本地化或定位 */
    code: string
    /** 面向用户的中文描述 */
    message: string
    /** 关联的活动实例（校验时所在节点） */
    activity?: ActivityLike
    /** 关联的属性名（如 condition / text） */
    property?: string
}

/** 诊断里只读取 id/type，避免对 Activity 产生类型环 */
export interface ActivityLike {
    id: string | null
    type: string
}

export function error(code: string, message: string, property?: string, activity?: ActivityLike): ActivityDiagnostic {
    return { severity: 'error', code, message, property, activity }
}

export function warning(code: string, message: string, property?: string, activity?: ActivityLike): ActivityDiagnostic {
    return { severity: 'warning', code, message, property, activity }
}
