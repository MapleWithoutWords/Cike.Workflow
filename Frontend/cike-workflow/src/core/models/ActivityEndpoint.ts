export class ActivityEndpoint {
  activityId: string;
  port?: string;

  constructor(activityId: string, port?: string) {
    this.activityId = activityId;
    this.port = port;
  }
}
