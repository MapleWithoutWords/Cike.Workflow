import { Activity } from "../abstracts/Activity";
import type { ActivityPort } from "../models/ActivityPort";

export class Start extends Activity {
  // Entry activity: nothing connects into it, so no input port.
  override getInPorts(): ActivityPort[] {
    return [];
  }
}
