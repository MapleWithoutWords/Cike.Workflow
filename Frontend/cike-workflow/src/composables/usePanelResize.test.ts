import { describe, expect, it } from "vitest"
import { computeSize, effectiveMax, usePanelResize } from "./usePanelResize"

describe("computeSize", () => {
  it("正向边缘：尺寸随指针 delta 增长", () => {
    expect(computeSize(224, 40, { min: 160, max: 420 })).toBe(264)
  })

  it("反向边缘：尺寸随指针 delta 反向增长", () => {
    expect(computeSize(288, -40, { min: 240, max: 480, invert: true })).toBe(328)
  })

  it("下限 clamp：不会小于 min", () => {
    expect(computeSize(160, -500, { min: 160, max: 420 })).toBe(160)
  })

  it("上限 clamp：不会大于 max", () => {
    expect(computeSize(400, 500, { min: 160, max: 420 })).toBe(420)
  })
})

describe("effectiveMax", () => {
  it("带视口：取 max 与视口比例封顶的较小者", () => {
    expect(effectiveMax(480, 0.5, 800)).toBe(400)
  })

  it("无视口或无比例：回退 max", () => {
    expect(effectiveMax(480, 0.5, undefined)).toBe(480)
    expect(effectiveMax(480, undefined, 800)).toBe(480)
  })
})

describe("usePanelResize", () => {
  it("存储不可用时（node 环境）回退 defaultSize", () => {
    const { size } = usePanelResize({
      axis: "width",
      defaultSize: 224,
      min: 160,
      max: 420,
      storageKey: "cike.dock.size.palette",
    })
    expect(size.value).toBe(224)
  })

  it("双击复位为 defaultSize", () => {
    const resize = usePanelResize({ axis: "height", invert: true, defaultSize: 200, min: 120, max: 480 })
    resize.size.value = 360
    resize.onDoubleClick()
    expect(resize.size.value).toBe(200)
  })
})
