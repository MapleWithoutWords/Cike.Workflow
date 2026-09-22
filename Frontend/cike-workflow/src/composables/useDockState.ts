import { ref, type Ref } from "vue"

/**
 * Shared dock state layer for the designer's three dock panels (see CONTEXT.md
 * glossary: 停靠面板 / 固定 Pin / 自动隐藏 Auto-hide, and docs/adr/0006).
 *
 * Semantics per panel:
 * - pinned: resident lock. Open state persists across sessions and the panel is
 *   immune to all passive auto expand/collapse.
 * - unpinned: session-level open state driven by the panel's relevance rule;
 *   auto behavior only fires on relevance transitions and never fights a
 *   manual open/close while the relevance input stays still.
 *
 * Arbitration: passive auto behavior obeys pin; reveals initiated by the user's
 * own actions (publish blocked → revealProblems) override pin.
 *
 * Persistence: localStorage at global granularity, one key per panel, holding
 * the pin flag plus the open state while pinned. Unpinned open state is never
 * persisted; a session starts from the relevance initial values.
 */

export type DockPanelId = "palette" | "property" | "problems"

export interface DockStorage {
  get(key: string): string | null
  set(key: string, value: string): void
}

export interface DockState {
  open: Record<DockPanelId, Ref<boolean>>
  pinned: Record<DockPanelId, Ref<boolean>>
  /** Manual open/close; always allowed, persisted only while pinned. */
  setOpen(panel: DockPanelId, open: boolean): void
  toggleOpen(panel: DockPanelId): void
  togglePin(panel: DockPanelId): void
  /** Property panel relevance input: selected node identity (null = none). */
  notifySelectionChanged(selectedId: string | null): void
  /** Problem list relevance input: validation problem count. */
  notifyProblemCount(count: number): void
  /** Active reveal (publish blocked): opens the problem dock even when pinned. */
  revealProblems(): void
}

const STORAGE_PREFIX = "cike.dock."

/** Relevance-driven initial open state for an unpinned session. */
const SESSION_INITIAL_OPEN: Record<DockPanelId, boolean> = {
  palette: true,
  property: false,
  problems: false,
}

function defaultStorage(): DockStorage {
  if (typeof localStorage === "undefined") {
    const map = new Map<string, string>()
    return {
      get: (key) => map.get(key) ?? null,
      set: (key, value) => {
        map.set(key, value)
      },
    }
  }
  return {
    get: (key) => localStorage.getItem(key),
    set: (key, value) => localStorage.setItem(key, value),
  }
}

function readPersisted(storage: DockStorage, panel: DockPanelId): { pinned: boolean; open: boolean } {
  const raw = storage.get(STORAGE_PREFIX + panel)
  if (!raw) return { pinned: false, open: SESSION_INITIAL_OPEN[panel] }
  try {
    const parsed = JSON.parse(raw) as { pinned?: unknown; open?: unknown }
    const pinned = parsed.pinned === true
    const open = pinned ? parsed.open === true : SESSION_INITIAL_OPEN[panel]
    return { pinned, open }
  } catch {
    return { pinned: false, open: SESSION_INITIAL_OPEN[panel] }
  }
}

export function createDockState(storage: DockStorage = defaultStorage()): DockState {
  const open = {} as Record<DockPanelId, Ref<boolean>>
  const pinned = {} as Record<DockPanelId, Ref<boolean>>
  for (const panel of Object.keys(SESSION_INITIAL_OPEN) as DockPanelId[]) {
    const restored = readPersisted(storage, panel)
    open[panel] = ref(restored.open)
    pinned[panel] = ref(restored.pinned)
  }

  function persist(panel: DockPanelId): void {
    const isPinned = pinned[panel].value
    const payload = {
      pinned: isPinned,
      open: isPinned ? open[panel].value : SESSION_INITIAL_OPEN[panel],
    }
    storage.set(STORAGE_PREFIX + panel, JSON.stringify(payload))
  }

  function setOpen(panel: DockPanelId, value: boolean): void {
    open[panel].value = value
    if (pinned[panel].value) persist(panel)
  }

  function toggleOpen(panel: DockPanelId): void {
    setOpen(panel, !open[panel].value)
  }

  function togglePin(panel: DockPanelId): void {
    pinned[panel].value = !pinned[panel].value
    persist(panel)
  }

  let lastSelectedId: string | null = null
  function notifySelectionChanged(selectedId: string | null): void {
    if (selectedId === lastSelectedId) return
    lastSelectedId = selectedId
    if (pinned.property.value) return
    open.property.value = selectedId !== null
  }

  let lastProblemCount = 0
  function notifyProblemCount(count: number): void {
    if (count === lastProblemCount) return
    const prev = lastProblemCount
    lastProblemCount = count
    if (pinned.problems.value) return
    if (prev === 0 && count > 0) open.problems.value = true
    else if (count === 0) open.problems.value = false
    // N→M (still non-zero): keep the user's manual state, do not disturb.
  }

  function revealProblems(): void {
    setOpen("problems", true)
  }

  return { open, pinned, setOpen, toggleOpen, togglePin, notifySelectionChanged, notifyProblemCount, revealProblems }
}

let singleton: DockState | null = null

/** Module-level singleton: one shared dock state across the whole app. */
export function useDockState(): DockState {
  if (!singleton) singleton = createDockState()
  return singleton
}
