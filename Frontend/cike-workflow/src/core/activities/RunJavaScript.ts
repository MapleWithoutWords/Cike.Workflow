import { ActivityWithResult } from "../abstracts/Activity";
import { ActivityPort } from "../models/ActivityPort";
import { Input } from "../models/Input";

export class RunJavaScript extends ActivityWithResult<any> {
  script: Input<string> = new Input<string>("Literal", "");
  possibleOutcomes: Input<string[]> = new Input<string[]>("Literal", []);

  override getOutPorts(): ActivityPort[] {
    // `possibleOutcomes` is an Input, so its expression may be switched to a
    // non-Literal type whose value is a string (or still be loading). Only a
    // concrete string[] Literal yields named ports; anything else falls back to
    // the single "Done" port instead of crashing the projection.
    const raw = this.possibleOutcomes.expression.value;
    const outcomes = Array.isArray(raw) ? raw.filter((name): name is string => typeof name === "string") : [];
    const names = outcomes.length > 0 ? outcomes : ["Done"];
    return names.map((name) => new ActivityPort(name, name));
  }
}
