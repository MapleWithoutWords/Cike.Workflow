/**
 * 变量（wire：继承 MemoryBlockReference，附带 name/value/storageDriverType）
 * 对应后端 Cike.Workflow.Core.Variables.Variable
 */

import { MemoryBlockReference, type MemoryBlockReferenceJson } from './memory-block-reference'
import { serializeWireValue } from './expression'

export interface VariableJson extends MemoryBlockReferenceJson {
    name?: string | null
    value?: unknown
    storageDriverType?: string | null
}

/** 对应 C# Humanizer 的 Camelize：首字母小写 */
function camelize(name: string): string {
    if (name.length === 0) {
        return name
    }
    return name.charAt(0).toLowerCase() + name.slice(1)
}

export class Variable extends MemoryBlockReference {
    name: string
    value: unknown
    storageDriverType: string | null

    constructor(name = '', value: unknown = null, id?: string | null) {
        // id 为 undefined 时按后端约定从 name 推导；显式传入 null（后端无 id）则原样保留
        super(id === undefined ? Variable.idFromName(name) : id)
        this.name = name
        this.value = value
        this.storageDriverType = null
    }

    /** 对应后端 Variable.GetIdFromName：`${name.camelize()}Variable` */
    static idFromName(name: string): string {
        return `${camelize(name)}Variable`
    }

    static override fromJson(json: VariableJson | null | undefined): Variable {
        const variable = new Variable(json?.name ?? '', json?.value ?? null, json?.id)
        variable.storageDriverType = json?.storageDriverType ?? null
        return variable
    }

    static parseList(raw: unknown): Variable[] {
        if (!Array.isArray(raw)) {
            return []
        }
        return raw.map(item => Variable.fromJson(item as VariableJson | null))
    }

    override toJson(): VariableJson {
        return {
            id: this.id,
            name: this.name,
            value: serializeWireValue(this.value),
            storageDriverType: this.storageDriverType,
        }
    }
}
