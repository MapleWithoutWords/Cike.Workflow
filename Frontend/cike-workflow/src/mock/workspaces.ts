// 临时 mock 数据 —— 待接入后端 API 后由请求结果替换。
// 这是工作空间列表的唯一来源，列表页 / 详情页标题 / 面包屑都从这里按 id 取名。

export interface MockWorkspace {
  id: string
  code: string
  name: string
  description: string
  updatedAt: string
}

export const mockWorkspaces: MockWorkspace[] = [
  { id: "1", code: "FIN", name: "财务系统", description: "报销、付款、预算审批等工作流", updatedAt: "2026-09-14" },
  { id: "2", code: "CRM", name: "CRM 客户管理", description: "客户跟进、商机转化、合同审批", updatedAt: "2026-09-12" },
  { id: "3", code: "HR", name: "HR 考勤", description: "请假审批、加班申请、入职流程", updatedAt: "2026-09-01" },
]

export function getWorkspaceName(id: string | null | undefined): string {
  if (!id) return ""
  return mockWorkspaces.find((ws) => ws.id === id)?.name ?? id
}
