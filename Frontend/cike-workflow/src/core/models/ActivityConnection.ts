import { ActivityEndpoint } from "./ActivityEndpoint";

export class ActivityConnection {
  source: ActivityEndpoint;
  target: ActivityEndpoint;

  constructor(source: ActivityEndpoint, target: ActivityEndpoint) {
    this.source = source;
    this.target = target;
  }
}
