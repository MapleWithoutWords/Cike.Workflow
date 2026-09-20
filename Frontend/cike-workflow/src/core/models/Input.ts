import { Expression } from "./Expression";
import { MemoryBlockReference } from "./MemoryBlockReference";

export class Input<T> {
  // Phantom marker: keeps the type parameter used without runtime cost.
  declare readonly _typeMarker?: T;
  memoryBlockReference: MemoryBlockReference;
  expression: Expression;

  constructor(type: string, value?: any) {
    this.memoryBlockReference = new MemoryBlockReference(crypto.randomUUID());
    this.expression = new Expression(type, value);
  }
}
