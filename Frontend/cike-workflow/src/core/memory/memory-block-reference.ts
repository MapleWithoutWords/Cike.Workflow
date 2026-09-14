/**
 * 内存块引用（wire 基元）
 * 对应后端 Cike.Workflow.Expressions.Models.MemoryBlockReference
 */

export interface MemoryBlockReferenceJson {
    id?: string | null
}

export class MemoryBlockReference {
    id: string | null

    constructor(id: string | null = null) {
        this.id = id
    }

    static fromJson(json: MemoryBlockReferenceJson | null | undefined): MemoryBlockReference {
        return new MemoryBlockReference(json?.id ?? null)
    }

    toJson(): MemoryBlockReferenceJson {
        return { id: this.id }
    }
}
