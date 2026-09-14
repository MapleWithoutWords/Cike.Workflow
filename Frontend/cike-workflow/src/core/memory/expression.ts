/**
 * 表达式（wire 基元）
 * 对应后端 Cike.Workflow.Expressions.Models.Expression：{ type, value }
 *
 * 后端目前注册的求值器只有 "Literal"（见 Cike.Workflow.Expression 的
 * LiteralExpressionHandler）；"Variable"/"Output" 形态由 Input 工厂构造，
 * value 里携带的是被引用对象本身，其求值行为完全由后端决定。
 */

export interface ExpressionJson {
    type?: string | null
    value?: unknown
}

export const EXPRESSION_TYPE_LITERAL = 'Literal'
export const EXPRESSION_TYPE_VARIABLE = 'Variable'
export const EXPRESSION_TYPE_OUTPUT = 'Output'

/**
 * 序列化任意 wire 值：模型类（自带 toJson() 的对象）递归转纯 JSON，
 * 数组/普通对象逐层处理，原始值原样返回。
 */
export function serializeWireValue(value: unknown): unknown {
    if (value === null || value === undefined) {
        return value ?? null
    }
    if (Array.isArray(value)) {
        return value.map(serializeWireValue)
    }
    if (typeof value === 'object') {
        const withToJson = value as { toJson?: () => unknown }
        if (typeof withToJson.toJson === 'function') {
            return withToJson.toJson()
        }
        const result: Record<string, unknown> = {}
        for (const [k, v] of Object.entries(value as Record<string, unknown>)) {
            result[k] = serializeWireValue(v)
        }
        return result
    }
    return value
}

export class Expression {
    type: string
    value: unknown

    constructor(type: string = EXPRESSION_TYPE_LITERAL, value: unknown = null) {
        this.type = type
        this.value = value
    }

    /** 对应后端 Expression.LiteralExpression(value) */
    static literal(value: unknown): Expression {
        return new Expression(EXPRESSION_TYPE_LITERAL, value)
    }

    isLiteral(): boolean {
        return this.type === EXPRESSION_TYPE_LITERAL
    }

    static fromJson(json: ExpressionJson | null | undefined): Expression {
        return new Expression(json?.type ?? EXPRESSION_TYPE_LITERAL, json?.value ?? null)
    }

    static fromJsonOrNull(json: ExpressionJson | null | undefined): Expression | null {
        if (json === null || json === undefined) {
            return null
        }
        return Expression.fromJson(json)
    }

    toJson(): ExpressionJson {
        return { type: this.type, value: serializeWireValue(this.value) }
    }
}
