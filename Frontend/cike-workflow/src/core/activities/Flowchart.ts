import { ContainerActivity } from "../abstracts/ContainerActivity";
import type { ActivityConnection } from "../models/ActivityConnection";

export class Flowchart extends ContainerActivity {
  connections: ActivityConnection[] = [];
}
