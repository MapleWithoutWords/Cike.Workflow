# 前端设计器计算 NodeId（与后端物化同构），据此修订 glossary

校验问题需定位到画布节点。后端 `ActivityNode.NodeId` 的算法是「祖先活动 Id 以 `:` 连接、再接自身 Id」（根节点即自身 Id），图由 `ActivityVisitor` **经端口解析**遍历——容器的 `Activities`、Composite 的 `Root`、For/While/ForEach 的 `Body` 都算子节点、都进路径。

原 glossary 规定 NodeId「由后端在物化时统一赋值，创建方不填」。但 `ValidateCanvas` **不物化**，NodeId 只从 wire 原样回显——前端新建节点的 `nodeId` 为 null，校验回来也是 null，无法用于定位。我们决定：**前端设计器负责计算 nodeId**——新增节点时 O(1) 由父 nodeId 直接算 `父nodeId + ":" + 自身id`（根 = 自身 id）；加载既有画布时，把计算**折进反序列化本就要走的整树递归**（沿容器 activities / For/While/ForEach body / Composite root 下传父 nodeId），无独立回填遍历、无额外一趟，使前端 NodeId 与后端物化结果逐字一致。这要求修订 glossary 的 NodeId 定义（创建方——前端设计器——也参与赋值）。

## Considered Options

- 前端不算 NodeId，改用 Activity Id 在活动树里搜祖先链定位：技术上可行且更省事（Activity Id 恒有值、是设计期身份），但要求前后端 NodeId 逻辑统一、由 NodeId 承载层级定位，故未采用。
- 后端在 `ValidateCanvas` 内先跑 identity graph 补齐 NodeId：被否。校验路径不应引入物化副作用，且 identity graph 可能重排 Id，与设计期身份发散。

## Consequences

- 前端反序列化（`fromWireActivity`）与新增路径必须维护 NodeId 计算，且**必须与后端 `ActivityNode.NodeId` 算法保持一致**；后端若调整 NodeId 方案，前端需同步。
- 后端端口按「包含关系」解析（`ActivityPortRegistry` 只把 `IActivity` / `IEnumerable<IActivity>` 属性当端口，`Connections` 不入图），故**连线/断线/移动/改属性都不改 NodeId**；反序列化递归须覆盖全部子形态（容器 activities、Composite root、For/While/ForEach body），否则漏算的节点校验回来时 nodeId 为 null。
- 无独立「回填整树」遍历——NodeId 计算搭载在反序列化本就要做的那一趟递归上；新增为 O(1)、不遍历树。
- 目前无跨容器移动（reparent）能力；一旦引入，移动子树后必须重算该子树的 NodeId。
