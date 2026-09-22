import { describe, expect, it } from "vitest"

/**
 * Seam: the shared dock state layer — the single composable seam agreed for the
 * dock pin feature (GitHub issue #16). Tests drive it with event sequences
 * (selection identity changes, problem-count transitions, active reveals,
 * manual toggles, pin toggles, simulated session restarts) and assert only
 * externally observable results: per-panel open/pinned state and what reaches
 * the storage stub.
 */

import { createDockState, type DockStorage } from "@/composables/useDockState"

function makeStorage(): DockStorage & { writes: Map<string, string> } {
  const map = new Map<string, string>()
  return {
    writes: map,
    get: (key: string) => map.get(key) ?? null,
    set: (key: string, value: string) => {
      map.set(key, value)
    },
  }
}

describe("session initial values", () => {
  it("starts unpinned with relevance-driven initial open state", () => {
    const dock = createDockState(makeStorage())
    expect(dock.open.palette.value).toBe(true)
    expect(dock.open.property.value).toBe(false)
    expect(dock.open.problems.value).toBe(false)
    expect(dock.pinned.palette.value).toBe(false)
    expect(dock.pinned.property.value).toBe(false)
    expect(dock.pinned.problems.value).toBe(false)
  })
})

describe("property panel relevance rule (unpinned)", () => {
  it("opens on selection appearing or switching, closes on deselect", () => {
    const dock = createDockState(makeStorage())
    dock.notifySelectionChanged(null)
    expect(dock.open.property.value).toBe(false)
    dock.notifySelectionChanged("a")
    expect(dock.open.property.value).toBe(true)
    dock.notifySelectionChanged("b")
    expect(dock.open.property.value).toBe(true)
    dock.notifySelectionChanged(null)
    expect(dock.open.property.value).toBe(false)
  })

  it("keeps manual open/close while the selection stays still", () => {
    const dock = createDockState(makeStorage())
    dock.notifySelectionChanged("a")
    dock.setOpen("property", false)
    dock.notifySelectionChanged("a")
    expect(dock.open.property.value).toBe(false)
    dock.setOpen("property", true)
    dock.notifySelectionChanged("a")
    expect(dock.open.property.value).toBe(true)
  })
})

describe("problem list relevance rule (unpinned)", () => {
  it("expands 0→N, collapses N→0, keeps manual state on N→M", () => {
    const dock = createDockState(makeStorage())
    dock.notifyProblemCount(2)
    expect(dock.open.problems.value).toBe(true)
    dock.setOpen("problems", false)
    dock.notifyProblemCount(3)
    expect(dock.open.problems.value).toBe(false)
    dock.setOpen("problems", true)
    dock.notifyProblemCount(0)
    expect(dock.open.problems.value).toBe(false)
  })
})

describe("pin immunity to passive auto behavior", () => {
  it("pinned-collapsed problems stay collapsed on 0→N", () => {
    const dock = createDockState(makeStorage())
    dock.togglePin("problems")
    expect(dock.open.problems.value).toBe(false)
    dock.notifyProblemCount(2)
    expect(dock.open.problems.value).toBe(false)
  })

  it("pinned-expanded problems stay expanded on N→0", () => {
    const dock = createDockState(makeStorage())
    dock.setOpen("problems", true)
    dock.togglePin("problems")
    dock.notifyProblemCount(2)
    dock.notifyProblemCount(0)
    expect(dock.open.problems.value).toBe(true)
  })

  it("pinned property ignores selection transitions", () => {
    const dock = createDockState(makeStorage())
    dock.togglePin("property")
    dock.notifySelectionChanged("a")
    expect(dock.open.property.value).toBe(false)
    dock.setOpen("property", true)
    dock.notifySelectionChanged(null)
    expect(dock.open.property.value).toBe(true)
  })

  it("pinned palette is unaffected by manual-only semantics and stays put", () => {
    const dock = createDockState(makeStorage())
    dock.setOpen("palette", false)
    dock.togglePin("palette")
    expect(dock.open.palette.value).toBe(false)
    expect(dock.pinned.palette.value).toBe(true)
  })
})

describe("active reveal overrides pin", () => {
  it("revealProblems opens a pinned-collapsed problem dock", () => {
    const dock = createDockState(makeStorage())
    dock.togglePin("problems")
    dock.revealProblems()
    expect(dock.open.problems.value).toBe(true)
  })
})

describe("manual toggles work regardless of pin", () => {
  it("toggleOpen flips a pinned panel and persists it", () => {
    const storage = makeStorage()
    const dock = createDockState(storage)
    dock.togglePin("palette")
    dock.toggleOpen("palette")
    expect(dock.open.palette.value).toBe(false)
    const restarted = createDockState(storage)
    expect(restarted.open.palette.value).toBe(false)
  })
})

describe("persistence", () => {
  it("does not write unpinned open changes", () => {
    const storage = makeStorage()
    const dock = createDockState(storage)
    dock.setOpen("palette", false)
    dock.setOpen("property", true)
    dock.notifySelectionChanged("a")
    dock.notifyProblemCount(1)
    expect(storage.writes.size).toBe(0)
  })

  it("persists pin flag plus pinned open state and restores on restart", () => {
    const storage = makeStorage()
    const dock = createDockState(storage)
    dock.notifySelectionChanged("a")
    dock.togglePin("property")
    dock.setOpen("property", false)
    const restarted = createDockState(storage)
    expect(restarted.pinned.property.value).toBe(true)
    expect(restarted.open.property.value).toBe(false)
  })

  it("unpinned restart falls back to relevance initial values", () => {
    const storage = makeStorage()
    const dock = createDockState(storage)
    dock.setOpen("palette", false)
    const restarted = createDockState(storage)
    expect(restarted.pinned.palette.value).toBe(false)
    expect(restarted.open.palette.value).toBe(true)
  })

  it("survives corrupted storage payloads", () => {
    const storage = makeStorage()
    storage.set("cike.dock.property", "{not json")
    const dock = createDockState(storage)
    expect(dock.pinned.property.value).toBe(false)
    expect(dock.open.property.value).toBe(false)
  })
})

describe("singleton accessor", () => {
  it("useDockState returns one shared instance", async () => {
    const { useDockState } = await import("@/composables/useDockState")
    const a = useDockState()
    const b = useDockState()
    expect(a).toBe(b)
    expect(a.open.palette).toBe(b.open.palette)
  })
})
