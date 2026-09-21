# 活动校验采用"活动自校验、校验器只触发"的架构

发布前的画布校验最初集中在 Core 层 WorkflowValidator 一个类里，但子活动的持有形态有三种（容器的 `Activities` 列表、Composite 的 `Root`、For/While/ForEach 的 `Body`），中心式遍历无法穷举——Body 子树当时根本走不到必填校验，是实际发生过的缺陷。现改为每个活动通过 `IActivity.Validate` 校验自己，容器类活动负责递归自己的子活动，WorkflowValidator 收缩为触发器（Root 前置判断 + 根校验 + 变量定义检查）。`Validate` 从此是公开扩展点签名，第三方活动可携带自己的校验规则。

## Considered Options

- 保留中心式校验器：被否。遍历知识必须与持有子活动的类型同步演进，中心类做不到；新增一种子活动持有形态就要回来改校验器，扩展点也不存在。
