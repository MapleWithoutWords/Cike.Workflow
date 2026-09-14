/**
 * 活动注册表：typeName → 类 的分发中心。
 * 对应后端 ActivityRegistry + ActivityJsonConverter 的按 type 路由。
 *
 * 本模块不 import 任何具体节点类；内置注册在 builtin-activities.ts 完成，
 * 避免「容器类 → 注册表 → 容器类」的循环。
 */

import type { Activity, ActivityClass, ActivityJson } from './activity'
import { NotFoundActivity } from './not-found-activity'

const classes = new Map<string, ActivityClass>()

export function registerActivity(activityClass: ActivityClass): void {
    classes.set(activityClass.typeName, activityClass)
}

export function resolveActivityClass(typeName: string): ActivityClass | undefined {
    return classes.get(typeName)
}

export function registeredActivityTypes(): readonly string[] {
    return [...classes.keys()]
}

/** 解析单个活动；未注册类型回退 NotFoundActivity，与后端行为镜像 */
export function parseActivity(json: ActivityJson): Activity {
    const typeName = typeof json.type === 'string' ? json.type : ''
    const activityClass = typeName ? classes.get(typeName) : undefined
    return activityClass ? activityClass.fromJson(json) : NotFoundActivity.fromUnknown(json)
}

/** 解析活动数组；容忍 null / 非数组（后端缺省键时） */
export function parseActivityList(raw: unknown): Activity[] {
    if (!Array.isArray(raw)) {
        return []
    }
    return raw
        .filter((item): item is ActivityJson => typeof item === 'object' && item !== null && !Array.isArray(item))
        .map(item => parseActivity(item))
}

/** 解析单个 [Port] 属性（活动引用）；null / 缺失 / 非对象返回 null */
export function parsePort(raw: unknown): Activity | null {
    if (typeof raw === 'object' && raw !== null && !Array.isArray(raw)) {
        return parseActivity(raw as ActivityJson)
    }
    return null
}
