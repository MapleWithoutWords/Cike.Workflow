import type { IActivity } from "../abstracts/Activity";
import type { ActivityConnection } from "../models/ActivityConnection";
import { clearNodePosition, setNodePosition, type DesignerNodeMeta } from "./metadata";

/**
 * Model-layer command stack (spec decision: undo/redo lives in the model, not
 * in X6 History). Commands mutate the domain model; the canvas layer listens
 * and re-projects. Each command carries its own inverse.
 */

export interface DesignerCommand {
  readonly label: string;
  apply(): void;
  undo(): void;
  /** Optional forward re-application kept separate from first apply(). */
  redo?(): void;
  /** Structural feedback for dialogs (e.g. connections removed by a delete). */
  readonly removedConnectionCount?: number;
}

/** Duck-typed container the commands operate on. */
export interface NodeContainer {
  activities: IActivity[];
  connections?: ActivityConnection[];
}

export class CommandStack {
  private undoStack: DesignerCommand[] = [];
  private redoStack: DesignerCommand[] = [];

  execute(command: DesignerCommand): void {
    command.apply();
    this.undoStack.push(command);
    this.redoStack = [];
  }

  undo(): DesignerCommand | null {
    const command = this.undoStack.pop();
    if (!command) return null;
    command.undo();
    this.redoStack.push(command);
    return command;
  }

  redo(): DesignerCommand | null {
    const command = this.redoStack.pop();
    if (!command) return null;
    (command.redo ?? command.apply).call(command);
    this.undoStack.push(command);
    return command;
  }

  canUndo(): boolean {
    return this.undoStack.length > 0;
  }

  canRedo(): boolean {
    return this.redoStack.length > 0;
  }

  clear(): void {
    this.undoStack = [];
    this.redoStack = [];
  }
}

export function makeMoveNodeCommand(
  activity: IActivity,
  from: DesignerNodeMeta | null,
  to: DesignerNodeMeta,
): DesignerCommand {
  return {
    label: "移动节点",
    apply: () => setNodePosition(activity, to),
    undo: () => {
      if (from) setNodePosition(activity, from);
      else clearNodePosition(activity);
    },
    redo: () => setNodePosition(activity, to),
  };
}

/** Generic property edit command: sets target[key] from → to with inverse. */
export function makeEditPropertyCommand(
  target: Record<string, unknown>,
  key: string,
  from: unknown,
  to: unknown,
): DesignerCommand {
  return {
    label: `修改 ${key}`,
    apply: () => {
      if (to == null) delete target[key];
      else target[key] = to;
    },
    undo: () => {
      if (from == null) delete target[key];
      else target[key] = from;
    },
    redo: () => {
      if (to == null) delete target[key];
      else target[key] = to;
    },
  };
}

export function makeAddNodeCommand(
  parent: NodeContainer,
  activity: IActivity,
  position: DesignerNodeMeta,
): DesignerCommand {
  return {
    label: "新增节点",
    apply: () => {
      parent.activities.push(activity);
      setNodePosition(activity, position);
    },
    undo: () => {
      const at = parent.activities.indexOf(activity);
      if (at >= 0) parent.activities.splice(at, 1);
    },
    redo: () => {
      parent.activities.push(activity);
      setNodePosition(activity, position);
    },
  };
}

export function makeConnectCommand(
  parent: NodeContainer,
  connection: ActivityConnection,
): DesignerCommand {
  const connections = parent.connections ?? [];
  return {
    label: "添加连线",
    apply: () => {
      connections.push(connection);
    },
    undo: () => {
      const at = connections.indexOf(connection);
      if (at >= 0) connections.splice(at, 1);
    },
    redo: () => {
      connections.push(connection);
    },
  };
}

export function makeDisconnectCommand(
  parent: NodeContainer,
  connection: ActivityConnection,
): DesignerCommand {
  const connections = parent.connections ?? [];
  const originalIndex = connections.indexOf(connection);
  return {
    label: "删除连线",
    apply: () => {
      const at = connections.indexOf(connection);
      if (at >= 0) connections.splice(at, 1);
    },
    undo: () => {
      connections.splice(Math.max(originalIndex, 0), 0, connection);
    },
    redo: () => {
      const at = connections.indexOf(connection);
      if (at >= 0) connections.splice(at, 1);
    },
  };
}

export function makeRemoveNodeCommand(parent: NodeContainer, childId: string): (DesignerCommand & { removedConnectionCount: number }) | null {
  const childIndex = parent.activities.findIndex((child) => child.id === childId);
  if (childIndex < 0) return null;
  const child = parent.activities[childIndex];
  const connections = parent.connections ?? [];
  const connectionIndexes: number[] = [];
  connections.forEach((connection, index) => {
    if (connection.source.activityId === childId || connection.target.activityId === childId)
      connectionIndexes.push(index);
  });
  const removedConnections = connectionIndexes.map((index) => connections[index]);

  return {
    label: "删除节点",
    removedConnectionCount: removedConnections.length,
    apply: () => {
      parent.activities.splice(childIndex, 1);
      for (const connection of removedConnections) {
        const at = connections.indexOf(connection);
        if (at >= 0) connections.splice(at, 1);
      }
    },
    undo: () => {
      parent.activities.splice(childIndex, 0, child);
      removedConnections.forEach((connection, offset) => {
        connections.splice(connectionIndexes[offset], 0, connection);
      });
    },
    redo: () => {
      const at = parent.activities.indexOf(child);
      if (at >= 0) parent.activities.splice(at, 1);
      for (const connection of removedConnections) {
        const atConnection = connections.indexOf(connection);
        if (atConnection >= 0) connections.splice(atConnection, 1);
      }
    },
  };
}
