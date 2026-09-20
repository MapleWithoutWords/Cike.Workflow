import { describe, expect, it } from "vitest";
import { Flowchart } from "../activities/Flowchart";
import { Start } from "../activities/Start";
import { End } from "../activities/End";
import { If } from "../activities/If";
import { ActivityEndpoint } from "../models/ActivityEndpoint";
import { ActivityConnection } from "../models/ActivityConnection";
import { CommandStack, makeMoveNodeCommand, makeRemoveNodeCommand } from "./commands";
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
