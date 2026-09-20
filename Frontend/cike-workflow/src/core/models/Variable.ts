export class Variable {
  id: string;
  name: string;
  typeName: string;
  isArray: boolean;
  defaultValue?: string;
  storageDriverType?: string;

  constructor(
    name: string,
    typeName: string,
    isArray: boolean = false,
    defaultValue?: string,
    storageDriverType?: string,
  ) {
    this.id = crypto.randomUUID();
    this.name = name;
    this.typeName = typeName;
    this.isArray = isArray;
    this.defaultValue = defaultValue;
    this.storageDriverType = storageDriverType;
  }
}
