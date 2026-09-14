/**
 * 输入（wire：{ memoryBlockReference, expression }）
 * 对应后端 Cike.Workflow.Core.Models.Input / Input<T>
 *
 * 后端 Input<T> 的泛型参数不上 wire（Input.Type 为 [JsonIgnore]），
 * 属性编辑器需要的类型信息由 ActivityDescriptor（swagger 侧）提供。
 */

import { MemoryBlockReference, type MemoryBlockReferenceJson } from './memory-block-reference'
import { Expression, EXPRESSION_TYPE_OUTPUT, EXPRESSION_TYPE_VARIABLE, type ExpressionJson } from './expression'
import { Variable } from './variable'
import { Output } from './output'

export interface InputJson {
    memoryBlockReference?: MemoryBlockReferenceJson | null
    expression?: ExpressionJson | null
}

export class Input {
    memoryBlockReference: MemoryBlockReference
    expression: Expression | null

    constructor(memoryBlockReference: MemoryBlockReference = new MemoryBlockReference(), expression: Expression | null = null) {
        this.memoryBlockReference = memoryBlockReference
        this.expression = expression
    }

    /** 字面量输入，对应 C# `new Input<T>(value)` */
    static literal(value: unknown): Input {
        return new Input(new MemoryBlockReference(), Expression.literal(value))
    }

    /** 变量引用输入，对应 C# `new Input<T>(Variable)` */
    static fromVariable(variable: Variable): Input {
        return new Input(
            new MemoryBlockReference(variable.id),
            new Expression(EXPRESSION_TYPE_VARIABLE, variable),
        )
    }

    /** 输出引用输入，对应 C# `new Input<T>(Output)` */
    static fromOutput(output: Output): Input {
        return new Input(
            new MemoryBlockReference(output.memoryBlockReference.id),
            new Expression(EXPRESSION_TYPE_OUTPUT, output),
        )
    }

    /** 自由表达式输入（如 C# `new Input<T>(expression)`） */
    static fromExpression(expression: Expression): Input {
        return new Input(new MemoryBlockReference(), expression)
    }

    isLiteral(): boolean {
        return this.expression?.isLiteral() ?? false
    }

    /** 字面量值；非字面量输入返回 undefined */
    getLiteralValue(): unknown | undefined {
        return this.isLiteral() ? this.expression?.value : undefined
    }

    static fromJson(json: InputJson | null | undefined): Input {
        return new Input(
            MemoryBlockReference.fromJson(json?.memoryBlockReference),
            Expression.fromJsonOrNull(json?.expression),
        )
    }

    toJson(): InputJson {
        return {
            memoryBlockReference: this.memoryBlockReference.toJson(),
            expression: this.expression ? this.expression.toJson() : null,
        }
    }
}

/**
 * 从活动 wire JSON 中读取一个 Input 属性。
 * 与后端 STJ 行为对齐：键缺失 → 保留字段默认值（fallback）；显式 null → null；对象 → 解析。
 */
export function readInput(json: Record<string, unknown>, key: string, fallback: Input | null = null): Input | null {
    if (!(key in json) || json[key] === undefined) {
        return fallback
    }
    const raw = json[key] as InputJson | null
    return raw === null ? null : Input.fromJson(raw)
}
