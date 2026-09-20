import { Activity } from "../abstracts/Activity";
import type { ActivityPort } from "../models/ActivityPort";

export class End extends Activity {
  // Terminal activity: nothing connects out of it, so no output port.
  override getOutPorts(): ActivityPort[] {
    return [];
  }
}
