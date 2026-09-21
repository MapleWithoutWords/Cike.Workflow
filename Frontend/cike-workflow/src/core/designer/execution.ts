import type { ActivityStatus } from "@/api/generated"

/**
 * Pure mapping from a workflow instance's activity execution records to the
 * per-activity run status shown on the read-only instance canvas. Kept free of
 * DOM/X6 so it is testable in isolation, mirroring the projection module.
 */

export interface ExecutionRecordLike {
  activityId?: string
  status?: ActivityStatus
}

/**
 * activityId → latest ActivityStatus. Records arrive chronologically, so a
 * later record for the same activity overrides an earlier one (final state).
 */
export function buildActivityStatusMap(records: ExecutionRecordLike[]): Map<string, ActivityStatus> {
  const map = new Map<string, ActivityStatus>()
  for (const record of records) {
    if (!record.activityId || record.status == null) continue
    map.set(record.activityId, record.status)
  }
  return map
}

/** UI label + semantic token class per ActivityStatus (0..4). */
export const ACTIVITY_STATUS_UI: Record<ActivityStatus, { label: string; class: string }> = {
  0: { label: "等待", class: "bg-muted text-muted-foreground" },
  1: { label: "运行中", class: "bg-info/15 text-info" },
  2: { label: "已完成", class: "bg-success/15 text-success" },
  3: { label: "已取消", class: "bg-muted text-muted-foreground" },
  4: { label: "故障", class: "bg-destructive/15 text-destructive" },
}
