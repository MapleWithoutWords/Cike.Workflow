import { ActivityWithResult } from "../abstracts/Activity";
import { ActivityPort } from "../models/ActivityPort";
import { Input } from "../models/Input";

export class RunJavaScript extends ActivityWithResult<any> {
  script: Input<string> = new Input<string>("Literal", "");
  possibleOutcomes: Input<string[]> = new Input<string[]>("Literal", []);

  override getOutPorts(): ActivityPort[] {
    const outcomes: string[] = this.possibleOutcomes.expression.value ?? [];
    const names = outcomes.length > 0 ? outcomes : ["Done"];
    return names.map((name) => new ActivityPort(name, name));
  }
}
