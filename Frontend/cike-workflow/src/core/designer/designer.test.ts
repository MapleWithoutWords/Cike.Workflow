import { describe, expect, it } from "vitest";
import { If } from "../activities/If";
import { ForEach } from "../activities/ForEach";
import { Flowchart } from "../activities/Flowchart";
import { Start } from "../activities/Start";
import { End } from "../activities/End";
import { ActivityEndpoint } from "../models/ActivityEndpoint";
import { ActivityConnection } from "../models/ActivityConnection";
import { computeAutoLayout } from "./layout";
import { getNodePosition, setNodePosition } from "./metadata";
import { fromWireActivity, toWireActivity, GenericActivity, type WireActivity } from "./serialization";
import { canDrillInto, getPortsOf, projectFlowchart, projectOrderedChain } from "./projection";
import { ensureDrillTarget, isChainContainer } from "./drill";

function makeFlowchartWire(): WireActivity {
  return {
    type: "Cike.Flowchart",
    id: "fc-root",
    name: "示例流程",
    activities: [
      { type: "Cike.Start", id: "a-start", name: "开始" },
      { type: "Cike.If", id: "a-if", name: "判断", condition: { memoryBlockReference: { id: "ref-1" }, expression: { type: "Literal", value: true } } },
      { type: "Cike.End", id: "a-end" },
      { type: "Cike.SomeFutureActivity", id: "a-unknown", futureField: "keep-me" },
    ],
    connections: [
      { source: { activityId: "a-start" }, target: { activityId: "a-if" } },
      { source: { activityId: "a-if", port: "True" }, target: { activityId: "a-end" } },
      { source: { activityId: "a-if", port: "False" }, target: { activityId: "a-unknown" } },
    ],
  };
}

describe("serialization", () => {
  it("FromWire_MirroredTypes_ProduceClassInstances", () => {
    const root = fromWireActivity(makeFlowchartWire());
    expect(root).toBeInstanceOf(Flowchart);
    const flowchart = root as Flowchart;
    expect(flowchart.activities.length).toBe(4);
    expect(flowchart.activities[0]).toBeInstanceOf(Start);
    expect(flowchart.activities[1]).toBeInstanceOf(If);
    expect(flowchart.activities[3]).toBeInstanceOf(GenericActivity);
  });

  it("FromWire_Connections_RestoreSourcePorts", () => {
    const flowchart = fromWireActivity(makeFlowchartWire()) as Flowchart;
    expect(flowchart.connections.length).toBe(3);
    expect(flowchart.connections[1].source.port).toBe("True");
    expect(flowchart.connections[1].target.activityId).toBe("a-end");
  });

  it("FromWire_UnknownType_PreservesExtraFields", () => {
    const activity = fromWireActivity({ type: "Cike.Mystery", id: "m1", futureField: 42 });
    const wire = toWireActivity(activity);
    expect(wire.futureField).toBe(42);
    expect(wire.type).toBe("Cike.Mystery");
  });

  it("ToWire_Roundtrip_PreservesBaseAndSpecificFields", () => {
    const root = fromWireActivity(makeFlowchartWire());
    const wire = toWireActivity(root);
    expect(wire.type).toBe("Cike.Flowchart");
    expect(wire.name).toBe("示例流程");
    const activities = wire.activities as WireActivity[];
    expect(activities.length).toBe(4);
    const ifWire = activities[1];
    expect(ifWire.type).toBe("Cike.If");
    expect((ifWire.condition as { expression: { type: string } }).expression.type).toBe("Literal");
    const connections = wire.connections as Array<{ source: { activityId: string; port?: string } }>;
    expect(connections[1].source.port).toBe("True");
  });

  it("ToWire_Metadata_PersistedAndOverridesRaw", () => {
    const root = fromWireActivity(makeFlowchartWire()) as Flowchart;
    setNodePosition(root.activities[0], { x: 100, y: 200 });
    const wire = toWireActivity(root);
    const startWire = (wire.activities as WireActivity[])[0];
    expect((startWire.metadata as { designer: { x: number; y: number } }).designer).toEqual({ x: 100, y: 200 });
  });

  it("FromWire_NestedBody_HydratesAsActivity", () => {
    const forEach = fromWireActivity({
      type: "Cike.ForEach",
      id: "fe1",
      body: { type: "Cike.Flowchart", id: "body-1", activities: [], connections: [] },
    }) as ForEach;
    expect(forEach.body).toBeInstanceOf(Flowchart);
    expect((forEach.body as Flowchart).id).toBe("body-1");
  });
});

describe("metadata", () => {
  it("NodePosition_Roundtrip", () => {
    const activity = new Start();
    expect(getNodePosition(activity)).toBeNull();
    setNodePosition(activity, { x: 10, y: 20 });
    expect(getNodePosition(activity)).toEqual({ x: 10, y: 20 });
    const wire = toWireActivity(activity);
    expect((wire.metadata as { designer: object }).designer).toEqual({ x: 10, y: 20 });
  });
});

describe("layout", () => {
  it("AutoLayout_LayersByBfsDepth", () => {
    const a1 = new Start();
    const a2 = new If();
    const a3 = new End();
    const connections = [
      new ActivityConnection(new ActivityEndpoint(a1.id), new ActivityEndpoint(a2.id)),
      new ActivityConnection(new ActivityEndpoint(a2.id, "True"), new ActivityEndpoint(a3.id)),
    ];
    const positions = computeAutoLayout([a1, a2, a3], connections);
    const p1 = positions.get(a1.id)!;
    const p2 = positions.get(a2.id)!;
    const p3 = positions.get(a3.id)!;
    expect(p1.x).toBeLessThan(p2.x);
    expect(p2.x).toBeLessThan(p3.x);
    expect(p1.y).toBe(p2.y);
  });

  it("AutoLayout_CycleOnlyGraph_PlacesAllNodes", () => {
    const a1 = new Start();
    const a2 = new End();
    const connections = [
      new ActivityConnection(new ActivityEndpoint(a1.id), new ActivityEndpoint(a2.id)),
      new ActivityConnection(new ActivityEndpoint(a2.id), new ActivityEndpoint(a1.id)),
    ];
    const positions = computeAutoLayout([a1, a2], connections);
    expect(positions.size).toBe(2);
    expect(positions.get(a1.id)).toBeDefined();
    expect(positions.get(a2.id)).toBeDefined();
  });
});

describe("projection", () => {
  it("ProjectFlowchart_UsesSavedPositionsAndPorts", () => {
    const flowchart = fromWireActivity(makeFlowchartWire()) as Flowchart;
    setNodePosition(flowchart.activities[1], { x: 500, y: 300 });
    const projection = projectFlowchart(flowchart);
    expect(projection.nodes.length).toBe(4);
    const ifNode = projection.nodes.find((n) => n.data.typeShort === "If")!;
    expect(ifNode.x).toBe(500);
    expect(ifNode.data.ports).toEqual(["True", "False"]);
    const startNode = projection.nodes.find((n) => n.data.typeShort === "Start")!;
    expect(startNode.x).toBeLessThan(ifNode.x);
    expect(projection.edges.length).toBe(3);
    expect(projection.edges[1].sourcePort).toBe("True");
  });

  it("ProjectFlowchart_MarkedGenericAndDrillable", () => {
    const flowchart = fromWireActivity(makeFlowchartWire()) as Flowchart;
    const projection = projectFlowchart(flowchart);
    const unknown = projection.nodes.find((n) => n.data.isGeneric)!;
    expect(unknown.data.type).toBe("Cike.SomeFutureActivity");
    const forEachActivity = new ForEach();
    expect(canDrillInto(forEachActivity)).toBe(true);
  });

  it("GetPortsOf_UnknownInstance_FallsBackToDone", () => {
    const generic = new GenericActivity("Cike.Mystery", {});
    expect(getPortsOf(generic)).toEqual(["Done"]);
  });

  it("ProjectOrderedChain_VerticalWithVisualEdges", () => {
    const children = [new Start(), new End()];
    const projection = projectOrderedChain(children);
    expect(projection.nodes.length).toBe(2);
    expect(projection.nodes[1].y).toBeGreaterThan(projection.nodes[0].y);
    expect(projection.edges.length).toBe(1);
    expect(projection.edges[0].visual).toBe(true);
  });

  it("ProjectFlowchart_Empty_ReturnsEmptyProjection", () => {
    const flowchart = new Flowchart();
    expect(projectFlowchart(flowchart)).toEqual({ nodes: [], edges: [] });
  });
});

describe("drill", () => {
  it("DrillTarget_ForEachNullBody_CreatesEmptyFlowchart", () => {
    const forEach = new ForEach();
    const target = ensureDrillTarget(forEach);
    expect(target).toBeInstanceOf(Flowchart);
    expect(forEach.body).toBe(target);
  });

  it("DrillTarget_ExistingBody_ReturnsItUnchanged", () => {
    const forEach = new ForEach();
    const existing = new Flowchart();
    forEach.body = existing;
    expect(ensureDrillTarget(forEach)).toBe(existing);
  });

  it("DrillTarget_GenericBody_FallsBackToNull", () => {
    const forEach = new ForEach();
    forEach.body = null;
    (forEach as unknown as { body: unknown }).body = { type: "Cike.Mystery", id: "x" };
    expect(ensureDrillTarget(forEach)).toBeNull();
  });
});

describe("chain", () => {
  it("IsChainContainer_UnmirroredSequence_True", () => {
    // Real hydration path: unmirrored Sequence arrives as GenericActivity
    // (activities hydrated, connections defaulted to []).
    const sequence = fromWireActivity({
      type: "Cike.Sequence",
      id: "s1",
      activities: [
        { type: "Cike.Start", id: "s-start" },
        { type: "Cike.End", id: "s-end" },
      ],
    }) as GenericActivity;
    expect(canDrillInto(sequence)).toBe(true);
    expect(isChainContainer(sequence)).toBe(true);
    expect(isChainContainer(new Flowchart())).toBe(false);
  });

  it("CanDrillInto_ScalarGeneric_False", () => {
    const scalar = fromWireActivity({ type: "Cike.WriteLine", id: "w1", text: { type: "Literal", value: "hi" } });
    expect(canDrillInto(scalar)).toBe(false);
  });

  it("CanDrillInto_GenericWithConnections_True", () => {
    const container = fromWireActivity({
      type: "Cike.CustomFlow",
      id: "c1",
      activities: [{ type: "Cike.Start", id: "c-start" }],
      connections: [{ source: { activityId: "c-start" }, target: { activityId: "c-start" } }],
    });
    expect(canDrillInto(container)).toBe(true);
    expect(isChainContainer(container as GenericActivity)).toBe(false);
  });
});
