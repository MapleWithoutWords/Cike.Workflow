/**
 * 输出引用（wire：{ memoryBlockReference }）
 * 对应后端 Cike.Workflow.Core.Models.Output
 *
 * 注意：C# 侧 Output<T> 的泛型参数不上 wire，后端 Input.Type 亦为 [JsonIgnore]。
 */

import { MemoryBlockReference, type MemoryBlockReferenceJson } from './memory-block-reference'

export interface OutputJson {
    memoryBlockReference?: MemoryBlockReferenceJson | null
}

export class Output {
    memoryBlockReference: MemoryBlockReference

    constructor(memoryBlockReference: MemoryBlockReference = new MemoryBlockReference()) {
        this.memoryBlockReference = memoryBlockReference
    }

    static fromJson(json: OutputJson | null | undefined): Output {
        return new Output(MemoryBlockReference.fromJson(json?.memoryBlockReference))
    }

    toJson(): OutputJson {
        return { memoryBlockReference: this.memoryBlockReference.toJson() }
    }
}

/** 从活动 wire JSON 中读取一个 Output 属性（与 readInput 同样区分缺失 / null） */
export function readOutput(json: Record<string, unknown>, key: string, fallback: Output | null = null): Output | null {
    if (!(key in json) || json[key] === undefined) {
        return fallback
    }
    const raw = json[key] as OutputJson | null
    return raw === null ? null : Output.fromJson(raw)
}
