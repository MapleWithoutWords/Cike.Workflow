import { describe, expect, it } from "vitest"
import { resolveRevealPath } from "./reveal"
import { fromWireActivity } from "./serialization"

/**
 * Seam: pure reveal-path resolution from a NodeId chain (or activityId fallback).
 * The NodeId chain mirrors the backend activity path (ADR 0003); a loop's `body`
 * is its own segment but is consumed by a single drill (drillInto lands on the
 * body). Expected drill targets / target ids are known-good literals.
 */

function makeTree() {
  return fromWireActivity({
    type: "Cike.Flowchart",
    id: "fc-root",
    activities: [
      { type: "Cike.Start", id: "a-start" },
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
  })
}

describe("resolveRevealPath", () => {
  it("DirectChild_NoDrill_SelectsTarget", () => {
    const path = resolveRevealPath(makeTree(), "fc-root:a-start", null)
    expect(path).not.toBeNull()
    expect(path!.drillTargets).toEqual([])
    expect(path!.targetId).toBe("a-start")
  })

  it("NestedBody_DrillsLoopOnce_SelectsInnerTarget", () => {
    const path = resolveRevealPath(makeTree(), "fc-root:a-for:body-fc:inner-start", null)
    expect(path).not.toBeNull()
    // The loop's body segment is consumed by the single drill into the loop.
    expect(path!.drillTargets.map((a) => a.id)).toEqual(["a-for"])
    expect(path!.targetId).toBe("inner-start")
  })

  it("RootOnly_NoDrill_TargetsRoot", () => {
    const path = resolveRevealPath(makeTree(), "fc-root", null)
    expect(path).not.toBeNull()
    expect(path!.drillTargets).toEqual([])
    expect(path!.targetId).toBe("fc-root")
  })

  it("StaleChain_ReturnsNull", () => {
    expect(resolveRevealPath(makeTree(), "fc-root:ghost:inner-start", null)).toBeNull()
  })

  it("RootMismatch_ReturnsNull", () => {
    expect(resolveRevealPath(makeTree(), "other-root:a-start", null)).toBeNull()
  })

  it("ActivityIdFallback_ResolvesSamePath", () => {
    const path = resolveRevealPath(makeTree(), null, "inner-start")
    expect(path).not.toBeNull()
    expect(path!.drillTargets.map((a) => a.id)).toEqual(["a-for"])
    expect(path!.targetId).toBe("inner-start")
  })

  it("ActivityIdFallback_Unknown_ReturnsNull", () => {
    expect(resolveRevealPath(makeTree(), null, "does-not-exist")).toBeNull()
  })

  it("NoLocator_ReturnsNull", () => {
    expect(resolveRevealPath(makeTree(), null, null)).toBeNull()
  })
})
