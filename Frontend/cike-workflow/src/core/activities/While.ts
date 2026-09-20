import { Activity } from "../abstracts/Activity";
import { Input } from "../models/Input";
import { Flowchart } from "./Flowchart";

export class While extends Activity {
  condition: Input<boolean> = new Input<boolean>("Literal", false);
  body: Flowchart | null = null;
}
