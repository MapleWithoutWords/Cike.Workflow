import { Activity } from "../abstracts/Activity";
import { Input } from "../models/Input";
import { Flowchart } from "./Flowchart";

export class ForEach extends Activity {
  items: Input<any[]> = new Input<any[]>("Literal", []);
  body: Flowchart | null = null;
  currentValue: any | null = null;
}
