/**
 * 工作流设计器核心数据模型。
 *
 * - `memory/`：表达式 / 输入输出 / 变量等 wire 基元
 * - `activities/`：节点（活动）类，校验等行为内聚在各节点类中
 * - `validation`：节点诊断契约（各节点 validate() 的返回类型）
 *
 * 节点 JSON 的形状与后端 Cike.Workflow.Core 的序列化（ActivityJsonConverter，
 * camelCase + "type" 多态判别）逐字段对齐，可直接用于提交的请求体与响应回填。
 * definitions / folders / workspaces / instances 等接口 DTO 以
 * `src/api/generated` 的 swagger 生成类型为准，此处不重复定义。
 */

export * from './memory'
export * from './validation'
export * from './activities'
