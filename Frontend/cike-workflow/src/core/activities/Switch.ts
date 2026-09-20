import { Activity } from "../abstracts/Activity";
import { ActivityPort } from "../models/ActivityPort";
import { Expression } from "../models/Expression";
import { Input } from "../models/Input";

export enum SwitchModel {
  MatchFirst = 0,

  MatchAny = 1,
}

export class SwitchCase {
  label: string;
  value: Expression;

  constructor(label: string, value: Expression) {
    this.label = label;
    this.value = value;
  }
}

export class Switch extends Activity {
  mode: Input<SwitchModel> = new Input<SwitchModel>(
    "Literal",
    SwitchModel.MatchFirst,
  );
  cases: SwitchCase[] = [];

  override getOutPorts(): ActivityPort[] {
    return this.cases
      .map((c) => new ActivityPort(c.label, c.label))
      .concat(new ActivityPort("Default", "Default"));
  }
}
