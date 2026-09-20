import type { Variable } from "../models/Variable";
import { Activity, type IActivity } from "./Activity";

export abstract class ContainerActivity extends Activity {
  activities: IActivity[] = [];
  variables: Variable[] = [];
}
