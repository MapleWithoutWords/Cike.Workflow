import { Flowchart } from "../activities/Flowchart";
import type { Variable } from "../models/Variable";
import { Activity, type IActivity } from "./Activity";

export abstract class CompositeActivity extends Activity {
  variables: Variable[] = [];
  root: IActivity = new Flowchart();
}
