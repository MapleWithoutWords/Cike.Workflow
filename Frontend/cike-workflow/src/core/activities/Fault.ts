import { Activity } from "../abstracts/Activity";
import { Input } from "../models/Input";

export class Fault extends Activity {
  faultCode: Input<string> = new Input<string>("Literal", "");
  category: Input<string> = new Input<string>("Literal", "");
  faultType: Input<string> = new Input<string>("Literal", "");
  message: Input<string | null> = new Input<string | null>("Literal", null);
}
