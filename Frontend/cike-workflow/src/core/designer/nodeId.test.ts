import { describe, expect, it } from "vitest"
import { computeNodeId } from "./nodeId"
import { fromWireActivity } from "./serialization"
import { Flowchart } from "../activities/Flowchart"
import { For } from "../activities/For"

/**
 * Seam: pure NodeId computation + the deserialization recursion that assigns it.
 * Expected values are known-good literals derived from the backend
 * ActivityNode.NodeId scheme (ancestor ids joined by ":" then own id; root = own
 * id) per ADR 0003 — an independent source of truth, not recomputed here.
 */

describe("computeNodeId", () => {
  it("Root_NoParent_IsOwnId", () => {
    expect(computeNodeId(null, "fc-root")).toBe("fc-root")
  })

  it("Child_JoinsParentAndOwnWithColon", () => {
    expect(computeNodeId("fc-root", "a-start")).toBe("fc-root:a-start")
  })

  it("NestedChild_AccumulatesAncestorPath", () => {
    expect(computeNodeId("fc-root:a-for:body-fc", "inner")).toBe("fc-root:a-for:body-fc:inner")
  })

  it("UndefinedParent_TreatedAsRoot", () => {
    expect(computeNodeId(undefined, "fc-root")).toBe("fc-root")
  })
})

describe("fromWireActivity nodeId", () => {
  it("Root_NodeIdIsOwnId", () => {
    const root = fromWireActivity({ type: "Cike.Flowchart", id: "fc-root", activities: [], connections: [] })
    expect(root.nodeId).toBe("fc-root")
  })

  it("ContainerChild_NodeIdIsParentColonOwn", () => {
    const root = fromWireActivity({
      type: "Cike.Flowchart",
      id: "fc-root",
      activities: [{ type: "Cike.Start", id: "a-start" }],
      connections: [],
    }) as Flowchart
    expect(root.activities[0]!.nodeId).toBe("fc-root:a-start")
  })

  it("NestedBody_NodeIdAccumulatesAcrossLevels", () => {
    const root = fromWireActivity({
      type: "Cike.Flowchart",
      id: "fc-root",
      activities: [
        {
          type: "Cike.For",
          id: "a-for",
          body: {
            type: "Cike.Flowchart",
            id: "body-fc",
            activities: [{ type: "Cike.Start", id: "inner-start" }],
            connections: [],
          },
        },
      ],
      connections: [],
    }) as Flowchart
    const forAct = root.activities[0] as For
    expect(forAct.nodeId).toBe("fc-root:a-for")
    expect(forAct.body!.nodeId).toBe("fc-root:a-for:body-fc")
    expect(forAct.body!.activities[0]!.nodeId).toBe("fc-root:a-for:body-fc:inner-start")
  })

  it("StaleWireNodeId_IsRecomputedFromStructure", () => {
    const root = fromWireActivity({
      type: "Cike.Flowchart",
      id: "fc-root",
      nodeId: "stale-root",
      activities: [{ type: "Cike.Start", id: "a-start", nodeId: "stale-child" }],
      connections: [],
    }) as Flowchart
    expect(root.nodeId).toBe("fc-root")
    expect(root.activities[0]!.nodeId).toBe("fc-root:a-start")
  })
})
