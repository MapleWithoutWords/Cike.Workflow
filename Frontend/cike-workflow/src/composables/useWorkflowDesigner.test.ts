import { beforeEach, describe, expect, it, vi } from "vitest"
import type { WireActivity } from "@/core/designer/serialization"

/**
 * Seam: the designer state orchestration composable. Tests mock the generated
 * API client and assert externally observable behavior (exposed refs + which
 * API calls fire with which bodies) — never internal implementation details.
 */

const getById = vi.fn()
const getDescriptors = vi.fn()
const save = vi.fn()
const publish = vi.fn()
const rollback = vi.fn()
const versionList = vi.fn()

vi.mock("@/api/generated", () => ({
  getApiV1WorkflowDefinitionsById: (...args: unknown[]) => getById(...args),
  getApiV1CommonsActivityDescriptors: (...args: unknown[]) => getDescriptors(...args),
  postApiV1WorkflowDefinitionsSaveById: (...args: unknown[]) => save(...args),
  postApiV1WorkflowDefinitionsPublishById: (...args: unknown[]) => publish(...args),
  postApiV1WorkflowDefinitionsRollback: (...args: unknown[]) => rollback(...args),
  getApiV1WorkflowDefinitionsVersionList: (...args: unknown[]) => versionList(...args),
}))

import { useWorkflowDesigner } from "@/composables/useWorkflowDesigner"

function makeRoot(): WireActivity {
  return {
    type: "Cike.Flowchart",
    id: "fc-root",
    name: "示例流程",
    activities: [{ type: "Cike.Start", id: "a-start", name: "开始" }],
    connections: [],
  }
}

interface DetailOverrides {
  id?: string
  isLatest?: boolean
  isPublished?: boolean
  version?: number
}

function makeDetail(overrides: DetailOverrides = {}) {
  return {
    id: overrides.id ?? "100",
    definitionId: "def-1",
    name: "月度报销审批",
    description: "每月员工报销单据的逐级审批流程",
    type: 1,
    usableAsActivity: false,
    materializerName: "Elsa.Workflows.Serialization",
    version: overrides.version ?? 1,
    isLatest: overrides.isLatest ?? true,
    isPublished: overrides.isPublished ?? false,
    folderId: "folder-1",
    root: makeRoot(),
    options: { variables: [] },
  }
}

beforeEach(() => {
  getById.mockReset()
  getDescriptors.mockReset()
  save.mockReset()
  publish.mockReset()
  rollback.mockReset()
  versionList.mockReset()
  getDescriptors.mockResolvedValue({ data: [] })
  versionList.mockResolvedValue({
    data: [{ id: "100", version: 1, isLatest: true, isPublished: false }],
  })
  getById.mockResolvedValue({ data: makeDetail(), error: undefined })
})

describe("useWorkflowDesigner version state", () => {
  it("Load_PopulatesVersionState", async () => {
    const designer = useWorkflowDesigner()
    await designer.load("100")
    expect(designer.rowId.value).toBe("100")
    expect(designer.definitionId.value).toBe("def-1")
    expect(designer.version.value).toBe(1)
    expect(designer.isLatest.value).toBe(true)
    expect(designer.isPublished.value).toBe(false)
    expect(designer.readonly.value).toBe(false)
  })

  it("ViewHistoricVersion_NonLatest_IsReadonly", async () => {
    getById.mockResolvedValue({ data: makeDetail({ id: "90", version: 1, isLatest: false }), error: undefined })
    const designer = useWorkflowDesigner()
    await designer.viewVersion("90")
    expect(designer.rowId.value).toBe("90")
    expect(designer.readonly.value).toBe(true)
  })
})

describe("useWorkflowDesigner save", () => {
  it("SaveDraftLatest_SameId_NoReload", async () => {
    getById.mockResolvedValue({ data: makeDetail({ isPublished: false }), error: undefined })
    save.mockResolvedValue({ data: "100", error: undefined })
    const designer = useWorkflowDesigner()
    await designer.load("100")
    const callsAfterLoad = getById.mock.calls.length
    await designer.save()
    // In-place save: no full canvas reload, version row unchanged.
    expect(getById.mock.calls.length).toBe(callsAfterLoad)
    expect(designer.rowId.value).toBe("100")
    expect(save).toHaveBeenCalledTimes(1)
    const body = save.mock.calls[0][0].body
    expect(body).toHaveProperty("root")
    expect(body).toHaveProperty("options")
  })

  it("SavePublishedVersion_ForksNewVersion_ReloadsCanvas", async () => {
    // Opening a published version, then saving forks a new draft version id.
    getById.mockImplementation(({ path }: { path: { id: string } }) =>
      Promise.resolve({
        data: makeDetail({ id: path.id, isPublished: path.id === "100", isLatest: true, version: path.id === "100" ? 1 : 2 }),
        error: undefined,
      }),
    )
    save.mockResolvedValue({ data: "101", error: undefined })
    const designer = useWorkflowDesigner()
    await designer.load("100")
    expect(designer.isPublished.value).toBe(true)
    await designer.save()
    // Backend forked a new draft version → canvas reloaded onto the new row id.
    expect(designer.rowId.value).toBe("101")
    expect(designer.isPublished.value).toBe(false)
    const lastGetById = getById.mock.calls.at(-1)![0]
    expect(lastGetById.path.id).toBe("101")
  })

  it("Save_WhenReadonly_DoesNotCallApi", async () => {
    getById.mockResolvedValue({ data: makeDetail({ id: "90", isLatest: false }), error: undefined })
    const designer = useWorkflowDesigner()
    await designer.viewVersion("90")
    await designer.save()
    expect(save).not.toHaveBeenCalled()
  })
})

describe("useWorkflowDesigner publish", () => {
  it("Publish_SendsRootOptionsAndNote", async () => {
    publish.mockResolvedValue({ data: "100", error: undefined })
    const designer = useWorkflowDesigner()
    await designer.load("100")
    await designer.publish("增加多级审批")
    expect(publish).toHaveBeenCalledTimes(1)
    const arg = publish.mock.calls[0][0]
    expect(arg.path.id).toBe("100")
    expect(arg.body).toHaveProperty("root")
    expect(arg.body).toHaveProperty("options")
    expect(arg.body.publishedNote).toBe("增加多级审批")
  })

  it("Publish_WhenReadonly_DoesNotCallApi", async () => {
    getById.mockResolvedValue({ data: makeDetail({ id: "90", isLatest: false }), error: undefined })
    const designer = useWorkflowDesigner()
    await designer.viewVersion("90")
    await designer.publish()
    expect(publish).not.toHaveBeenCalled()
  })
})

describe("useWorkflowDesigner versions", () => {
  it("Rollback_PostsDefinitionIdAndVersionId", async () => {
    rollback.mockResolvedValue({ data: undefined, error: undefined })
    const designer = useWorkflowDesigner()
    await designer.load("100")
    await designer.rollback("90")
    expect(rollback).toHaveBeenCalledTimes(1)
    const body = rollback.mock.calls[0][0].body
    expect(body.definitionId).toBe("def-1")
    expect(body.definitionVersionId).toBe("90")
  })

  it("ReturnToLatest_LoadsLatestVersionRow", async () => {
    versionList.mockResolvedValue({
      data: [
        { id: "200", version: 2, isLatest: true, isPublished: false },
        { id: "100", version: 1, isLatest: false, isPublished: true },
      ],
    })
    // Start by viewing the old version, then return to latest.
    getById.mockImplementation(({ path }: { path: { id: string } }) =>
      Promise.resolve({
        data: makeDetail({ id: path.id, isLatest: path.id === "200", version: path.id === "200" ? 2 : 1 }),
        error: undefined,
      }),
    )
    const designer = useWorkflowDesigner()
    await designer.viewVersion("100")
    expect(designer.readonly.value).toBe(true)
    await designer.returnToLatest()
    expect(designer.rowId.value).toBe("200")
    expect(designer.readonly.value).toBe(false)
  })
})
