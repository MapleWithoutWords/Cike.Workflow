# 画布校验以后端 ValidateCanvas 为唯一真源，前端不复制规则

设计器需要一个"问题清单"来展示画布的校验问题。后端已提供 `POST /api/v1/WorkflowDefinitions/ValidateCanvas`，跑的是与发布前置**同一套**严格校验（活动自校验，见根 `docs/adr/0001`），返回结构化问题列表 `{ activityId?, nodeId?, name?, message }[]`（空集即通过，全部为错误级）。

我们决定：校验规则的唯一真源是后端，前端**只消费与渲染**，不在客户端复制任何规则。据此**移除原有的 `missingStartNode` 客户端闸门**——它只检查当前下钻层、只认 `Start` 类型，是后端"开始节点"规则（全树、含 `CanStartWorkflow`）的弱化近似，双轨并存会对用户显示矛盾信号。前端在编辑（新增/修改节点、连线、节点表单）后防抖重校验、发布前预校验、加载后校验一次；问题清单是校验问题的唯一呈现出口。

## Considered Options

- 保留客户端 `missingStartNode` 即时闸门与后端校验并存：被否。口径不一致（当前层 vs 全树、`Start` 类型 vs 含 `CanStartWorkflow`），且后端校验已是发布的硬闸门，客户端重复规则没有收益。
- 复用 Save/Publish 的 400 ProblemDetails 作为问题来源：被否。那是拍平的字符串，无 `activityId`、无法定位到画布节点，也不支持"编辑即校验"。

## Consequences

- 校验依赖后端往返：离线或后端不可用时问题清单为空，不代表画布正确。以顶部传输错误条兜底提示。
- 编辑防抖（约 500ms 合并连续改动）避免每键一次请求；需处理竞态（以最新一次请求结果为准）。
