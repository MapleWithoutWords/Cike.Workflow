import { describe, expect, it } from "vitest";
import { Flowchart } from "../activities/Flowchart";
import { Start } from "../activities/Start";
import { End } from "../activities/End";
import { If } from "../activities/If";
import { ActivityEndpoint } from "../models/ActivityEndpoint";
import { ActivityConnection } from "../models/ActivityConnection";
import { CommandStack, makeMoveNodeCommand, makeRemoveNodeCommand, makeAddNodeCommand, makeConnectCommand, makeDisconnectCommand } from "./commands";
import { getNodePosition } from "./metadata";

function makeFlowchart(): { flowchart: Flowchart; start: Start; decision: If; end: End } {
  const flowchart = new Flowchart();
  const start = new Start();
  const decision = new If();
  const end = new End();
  flowchart.activities = [start, decision, end];
  flowchart.connections = [
    new ActivityConnection(new ActivityEndpoint(start.id), new ActivityEndpoint(decision.id)),
    new ActivityConnection(new ActivityEndpoint(decision.id, "True"), new ActivityEndpoint(end.id)),
  ];
  return { flowchart, start, decision, end };
}

describe("CommandStack", () => {
  it("Execute_UndoRedo_RestoresAndReapplies", () => {
    const stack = new CommandStack();
    const activity = new Start();
    expect(stack.canUndo()).toBe(false);
    stack.execute(makeMoveNodeCommand(activity, { x: 1, y: 1 }, { x: 9, y: 9 }));
    expect(getNodePosition(activity)).toEqual({ x: 9, y: 9 });
    expect(stack.canUndo()).toBe(true);
    stack.undo();
    expect(getNodePosition(activity)).toEqual({ x: 1, y: 1 });
    expect(stack.canRedo()).toBe(true);
    stack.redo();
    expect(getNodePosition(activity)).toEqual({ x: 9, y: 9 });
  });

  it("Execute_AfterUndo_TruncatesRedoBranch", () => {
    const stack = new CommandStack();
    const activity = new Start();
    stack.execute(makeMoveNodeCommand(activity, null, { x: 1, y: 1 }));
    stack.undo();
    stack.execute(makeMoveNodeCommand(activity, null, { x: 2, y: 2 }));
    expect(stack.canRedo()).toBe(false);
    stack.undo();
    expect(getNodePosition(activity)).toBeNull();
  });
});

describe("MoveNodeCommand", () => {
  it("Move_FromNullPosition_UndoRestoresNull", () => {
    const activity = new Start();
    const command = makeMoveNodeCommand(activity, null, { x: 5, y: 6 });
    command.apply();
    expect(getNodePosition(activity)).toEqual({ x: 5, y: 6 });
    command.undo();
    expect(getNodePosition(activity)).toBeNull();
  });
});

describe("RemoveNodeCommand", () => {
  it("Remove_MiddleNode_StripsChildAndConnections", () => {
    const { flowchart, decision } = makeFlowchart();
    const command = makeRemoveNodeCommand(flowchart, decision.id)!;
    const info = command.removedConnectionCount;
    expect(info).toBe(2);
    command.apply();
    expect(flowchart.activities.map((a) => a.id)).not.toContain(decision.id);
    expect(flowchart.connections.length).toBe(0);
    command.undo();
    expect(flowchart.activities.map((a) => a.id)).toEqual([flowchart.activities[0].id, decision.id, flowchart.activities[2].id]);
    expect(flowchart.connections.length).toBe(2);
    expect(flowchart.connections[1].source.port).toBe("True");
    command.redo?.();
    expect(flowchart.connections.length).toBe(0);
  });

  it("Remove_UnknownChild_ReturnsNull", () => {
    const { flowchart } = makeFlowchart();
    expect(makeRemoveNodeCommand(flowchart, "nope")).toBeNull();
  });
});

describe("AddNodeCommand", () => {
  it("Add_AppendsWithPosition_UndoRemoves", () => {
    const { flowchart, end } = makeFlowchart();
    const fresh = new Start();
    const command = makeAddNodeCommand(flowchart, fresh, { x: 400, y: 200 });
    command.apply();
    expect(flowchart.activities.length).toBe(4);
    expect(getNodePosition(flowchart.activities[3])).toEqual({ x: 400, y: 200 });
    command.undo();
    expect(flowchart.activities.length).toBe(3);
    expect(flowchart.activities).not.toContain(fresh);
    expect(flowchart.connections.length).toBe(2);
    command.redo?.();
    expect(flowchart.activities.length).toBe(4);
    command.undo();
    void end;
  });
});

describe("ConnectCommand", () => {
  it("Connect_AppendPort_UndoRemoves", () => {
    const { flowchart, start, decision } = makeFlowchart();
    const command = makeConnectCommand(flowchart, new ActivityConnection(new ActivityEndpoint(start.id, "Done"), new ActivityEndpoint(decision.id)));
    command.apply();
    expect(flowchart.connections.length).toBe(3);
    command.undo();
    expect(flowchart.connections.length).toBe(2);
    command.redo?.();
    expect(flowchart.connections.length).toBe(3);
  });
});

describe("DisconnectCommand", () => {
  it("Disconnect_RemoveByReference_UndoRestoresAtOriginalIndex", () => {
    const { flowchart } = makeFlowchart();
    const target = flowchart.connections[0];
    const command = makeDisconnectCommand(flowchart, target);
    command.apply();
    expect(flowchart.connections.length).toBe(1);
    command.undo();
    expect(flowchart.connections.length).toBe(2);
    expect(flowchart.connections[0]).toBe(target);
  });

  it("Disconnect_AlreadyRemoved_NoopSafe", () => {
    const { flowchart } = makeFlowchart();
    const target = flowchart.connections[0];
    const command = makeDisconnectCommand(flowchart, target);
    command.apply();
    command.apply();
    expect(flowchart.connections.length).toBe(1);
  });
});
