import { Activity } from "../abstracts/Activity";
import { ActivityPort } from "../models/ActivityPort";
import { Input } from "../models/Input";

export class If extends Activity {
  condition: Input<boolean> = new Input<boolean>("Literal", false);
  override getPorts(): ActivityPort[] {
    return [
      new ActivityPort("True", "True"),
      new ActivityPort("False", "False"),
    ];
  }
}
