import { ref, type Ref } from "vue"

/**
 * 停靠面板边缘拖拽调尺寸的共享逻辑。
 *
 * 三个设计器停靠面板（活动/属性/问题清单）共用：指针在边缘拖拽条上
 * 按下后监听 window 的 pointermove/pointerup 实时改尺寸，双击复位默认值；
 * 尺寸可选地持久化到 localStorage，跨会话保持。
 */

export interface PanelResizeBounds {
  min: number
  max: number
  /** 面板增长方向与指针移动方向相反时为 true（右缘/顶缘锚定的面板）。 */
  invert?: boolean
}

export interface UsePanelResizeOptions extends PanelResizeBounds {
  /** 拖拽改变的维度：侧栏为 width，底部 dock 为 height。 */
  axis: "width" | "height"
  defaultSize: number
  storageKey?: string
  /** 高度上限另取视口高度的比例封顶（小屏保护），仅 height 轴使用。 */
  maxViewportRatio?: number
}

export interface PanelResize {
  size: Ref<number>
  dragging: Ref<boolean>
  onPointerDown: (event: PointerEvent) => void
  onDoubleClick: () => void
}

/** 纯 clamp + 方向换算；node 环境下的测试缝。 */
export function computeSize(startSize: number, delta: number, bounds: PanelResizeBounds): number {
  const grown = startSize + (bounds.invert ? -delta : delta)
  return Math.min(bounds.max, Math.max(bounds.min, grown))
}

/** 视口比例封顶：小屏上防止 dock 把画布压没；无视口（node 环境）回退 max。 */
export function effectiveMax(max: number, ratio: number | undefined, viewportHeight: number | undefined): number {
  if (ratio === undefined || viewportHeight === undefined) return max
  return Math.min(max, Math.round(viewportHeight * ratio))
}

function viewportHeight(): number | undefined {
  return typeof window === "undefined" ? undefined : window.innerHeight
}

/** 结合视口封顶后的实际夹取边界。 */
function effectiveBounds(options: UsePanelResizeOptions): PanelResizeBounds {
  return {
    min: options.min,
    max: effectiveMax(options.max, options.maxViewportRatio, viewportHeight()),
    invert: options.invert,
  }
}

function readPersisted(storageKey: string | undefined, fallback: number, options: UsePanelResizeOptions): number {
  if (!storageKey) return fallback
  try {
    const raw = localStorage.getItem(storageKey)
    if (raw === null) return fallback
    const parsed = Number.parseInt(raw, 10)
    return Number.isFinite(parsed) ? computeSize(parsed, 0, effectiveBounds(options)) : fallback
  } catch {
    // node 环境 / 隐私模式下 localStorage 不可用，回退会话初值。
    return fallback
  }
}

function persist(storageKey: string | undefined, size: number): void {
  if (!storageKey) return
  try {
    localStorage.setItem(storageKey, String(size))
  } catch {
    // 存储不可用时仅保持内存态，拖拽本身不受影响。
  }
}

export function usePanelResize(options: UsePanelResizeOptions): PanelResize {
  const size = ref(readPersisted(options.storageKey, options.defaultSize, options))
  const dragging = ref(false)

  function onPointerDown(event: PointerEvent): void {
    if (dragging.value) return
    event.preventDefault()
    const startX = event.clientX
    const startY = event.clientY
    const startSize = size.value
    // 拖拽开始时定格视口封顶边界，避免拖拽中窗口变化引起尺寸跳变。
    const bounds = effectiveBounds(options)
    dragging.value = true
    if (typeof document !== "undefined") {
      // 拖拽期间禁文本选中并把全局光标锁成对应 resize 形态，避免指针滑出拖拽条后光标跳变。
      document.body.style.userSelect = "none"
      document.body.style.cursor = options.axis === "width" ? "col-resize" : "row-resize"
    }
    const onMove = (move: PointerEvent): void => {
      const delta = options.axis === "width" ? move.clientX - startX : move.clientY - startY
      size.value = computeSize(startSize, delta, bounds)
    }
    const onUp = (): void => {
      window.removeEventListener("pointermove", onMove)
      window.removeEventListener("pointerup", onUp)
      dragging.value = false
      if (typeof document !== "undefined") {
        document.body.style.userSelect = ""
        document.body.style.cursor = ""
      }
      persist(options.storageKey, size.value)
    }
    window.addEventListener("pointermove", onMove)
    window.addEventListener("pointerup", onUp)
  }

  /** 双击拖拽条复位默认尺寸。 */
  function onDoubleClick(): void {
    size.value = options.defaultSize
    persist(options.storageKey, options.defaultSize)
  }

  return { size, dragging, onPointerDown, onDoubleClick }
}
