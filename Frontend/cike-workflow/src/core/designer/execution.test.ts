import { describe, expect, it } from "vitest"
import { buildActivityStatusMap } from "./execution"

describe("execution", () => {
  it("BuildStatusMap_MapsByActivityId", () => {
    const map = buildActivityStatusMap([
      { activityId: "a-start", status: 2 },
      { activityId: "a-if", status: 1 },
    ])
    expect(map.get("a-start")).toBe(2)
    expect(map.get("a-if")).toBe(1)
    expect(map.get("missing")).toBeUndefined()
  })

  it("BuildStatusMap_LaterRecordWins_FinalState", () => {
    const map = buildActivityStatusMap([
      { activityId: "a-approval", status: 1 },
      { activityId: "a-approval", status: 2 },
    ])
    expect(map.get("a-approval")).toBe(2)
  })

  it("BuildStatusMap_SkipsIncompleteRecords", () => {
    const map = buildActivityStatusMap([
      { activityId: undefined, status: 2 },
      { activityId: "a-x", status: undefined },
    ])
    expect(map.size).toBe(0)
  })
})
