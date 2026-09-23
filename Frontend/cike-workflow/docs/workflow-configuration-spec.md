# 设计器右侧工作流配置与参数编辑

## Problem Statement

工作流设计器右侧当前只承载节点属性，未选中节点时仅显示空态。工作流定义的基本信息无法在设计器内集中查看；变量依赖独立对话框；工作流输入、输出和结果虽然属于 `WorkflowDefinitionOptionsValueObject`，却没有统一的查看与编辑入口。用户难以从一个位置理解和维护工作流对外接口及运行配置。

## Solution

把右侧区域升级为“窄工具栏 + 单内容面板”的右侧工具区，提供“工作流配置”和“节点属性”两个入口。工作流配置只读展示基本信息，并以分区列表编辑变量、输入、输出和工作流结果；节点属性继续承载当前节点表单。所有配置修改即时进入现有命令栈，支持撤销、重做、保存、发布和后端画布校验。`customProperties` 继续透传但不开放编辑。

## User Stories

1. As a 工作流设计者, I want to 从右侧工具栏打开工作流配置, so that I can 在画布内管理定义级信息。
2. As a 工作流设计者, I want to 在未选中节点时看到工作流配置, so that I can 避免面对无意义的空面板。
3. As a 工作流设计者, I want to 在选中节点后自动切到节点属性, so that I can 立即编辑当前活动。
4. As a 工作流设计者, I want to 在取消节点选择后返回工作流配置, so that I can 让右侧内容跟随当前编辑上下文。
5. As a 工作流设计者, I want to 保留手动折叠与固定行为, so that I can 自主管理画布空间。
6. As a 工作流设计者, I want to 查看名称、描述、定义 ID、版本、类型、发布状态和可作为活动使用状态, so that I can 确认当前工作流身份。
7. As a 工作流设计者, I want to 继续通过既有入口编辑基本信息, so that I can 避免右侧面板出现两套元数据提交方式。
8. As a 工作流设计者, I want to 新增、编辑、删除和排序变量, so that I can 管理工作流状态。
9. As a 工作流设计者, I want to 编辑变量的完整契约字段, so that I can 保留默认值与存储设置。
10. As a 工作流设计者, I want to 新增、编辑、删除和排序工作流输入, so that I can 定义调用方需要提供的数据。
11. As a 工作流设计者, I want to 配置输入的名称、显示名、说明、类型、数组标志、默认表达式和存储驱动, so that I can 完整表达输入契约。
12. As a 工作流设计者, I want to 让输入默认值只提供 Literal、Liquid、JavaScript, so that I can 避免配置不允许的名称引用类型。
13. As a 工作流设计者, I want to 新增、编辑、删除和排序工作流输出, so that I can 定义调用方收到的数据。
14. As a 工作流设计者, I want to 让输出默认值支持全部五种表达式, so that I can 从最终输入或变量取得输出值。
15. As a 工作流设计者, I want to 独立管理工作流结果, so that I can 区分控制流结果和数据输出。
16. As a 工作流设计者, I want to 从后端类型列表选择参数与变量类型, so that I can 使用已注册类型且避免拼写错误。
17. As a 工作流设计者, I want to 默认只看到常用字段并按需展开高级字段, so that I can 在窄面板内保持清晰。
18. As a 工作流设计者, I want to 让每次修改立即支持撤销和重做, so that I can 获得与画布编辑一致的体验。
19. As a 工作流设计者, I want to 在重命名输入或变量时同步修改结构化引用, so that I can 避免安全可识别的引用失效。
20. As a 工作流设计者, I want to 保持 JavaScript 与 Liquid 源码不被自动改写, so that I can 避免不安全的文本替换。
21. As a 工作流设计者, I want to 在问题清单中看到非法名称、同类重名和悬空结构化引用, so that I can 从统一出口修复问题。
22. As a 工作流设计者, I want to 让变量、输入、输出和结果使用独立名称空间, so that I can 在不同类别中复用名称。
23. As a 工作流设计者, I want to 用 `name` 保存机器名称并用 `displayName` 展示人类可读名称, so that I can 同时保证引用稳定与界面友好。
24. As a 历史版本查看者, I want to 只读查看完整工作流配置, so that I can 审核旧版本而不产生修改。
25. As a 工作流设计者, I want to 在加载、保存和发布时保留未开放的选项字段, so that I can 避免前端破坏后端扩展数据。

## Implementation Decisions

- 右侧采用一个停靠工具区：窄工具栏常驻，内容区同一时间只显示“工作流配置”或“节点属性”。
- 未选中节点时激活工作流配置；选中节点时激活节点属性，并在 pin 规则允许时自动展开；取消选择后切回工作流配置并保持当前开合状态。
- 继续复用已有的固定、尺寸拖拽、尺寸持久化和主动揭示机制，不建立第二套 dock 状态。
- 工作流配置包含基本信息、变量、输入、输出和工作流结果五个区块；基本信息只读，`customProperties` 不展示、不编辑。
- 变量、输入、输出、结果各自拥有独立名称空间；同类名称必填且唯一，不同类别允许同名。`name` 是机器名称，`displayName` 承载展示文本。
- 新增、删除、排序与字段提交均立即写入设计器命令栈，参与撤销、重做和防抖画布校验。
- 删除操作可撤销，不增加确认弹窗；悬空结构化引用由后端画布校验进入问题清单。
- 输入或变量重命名时，前端级联更新 Type 为 Input/Variable 的结构化名称引用，并合并为一次撤销操作；JavaScript/Liquid 文本不做替换。
- 常用字段直接显示；说明、默认表达式和存储驱动等低频字段放入可展开的高级设置。
- 参数和变量类型选择器消费后端现有类型描述接口。当前前端 OpenAPI 文档与生成客户端尚未包含该接口，实施前先更新接口定义。
- 输入默认表达式只展示 Literal、Liquid、JavaScript；输出默认表达式展示 ExpressionDescriptors 返回的全部类型。
- 更新 options 时必须保留 `customProperties` 和其他未知键，保存与发布继续发送完整 options。
- 右侧编辑器完成后，现有独立变量对话框和顶部变量入口退出主编辑路径，避免双重入口。
- 历史版本复用同一展示结构，但所有修改入口禁用。
- 本规格只修改前端。输入、输出默认表达式的后端运行时求值与后端校验增强由独立任务承接。

## Testing Decisions

- 主要自动化测试缝采用现有 `useWorkflowDesigner` composable 层，验证用户可观察的状态和序列化结果，不断言组件内部实现。
- 覆盖加载变量、输入、输出、结果以及保留未知 options 字段。
- 覆盖新增、编辑、删除、排序、撤销、重做以及只读模式禁止修改。
- 覆盖保存和发布载荷包含最新配置且不丢失 `customProperties`。
- 覆盖输入、变量重命名会更新结构化引用，而 JavaScript/Liquid 文本保持不变。
- 覆盖输入默认表达式类型受限、输出默认表达式类型完整。
- 扩展现有面板状态测试，覆盖未选节点、选中节点、取消选择、手动折叠和 pin 仲裁。
- 浏览器验收覆盖右侧 rail、窄宽度溢出、键盘焦点、明暗主题和历史版本只读表现。
- 测试参考现有设计器 composable、命令栈、面板尺寸和表达式编辑器测试模式。

## Out of Scope

- 编辑 `customProperties`。
- 实现输入或输出默认表达式的后端运行逻辑。
- 修改后端画布校验规则。
- 自动改写或静态分析 JavaScript/Liquid 源码中的名称引用。
- 新增活动输出直接绑定工作流输出的界面。
- 重构活动面板、问题清单或画布节点视觉。
- 在右侧面板内直接编辑工作流基本信息。

## Further Notes

- `WorkflowDefinitionOptionsValueObject` 是选项契约的权威来源，包含 variables、inputs、outputs、outcomes、customProperties。
- `InputDefinition` 与 `OutputDefinition` 共享 type、isArray、name、displayName、description、defaultValue；输入额外包含 storageDriverType。
- `VariableDefinition` 保持自身独立字段结构，不转换成 ArgumentDefinition。
- 工作流结果属于控制流，不等同于承载数据的工作流输出。
- “显式 null 不触发默认值”等运行语义已记录在领域文档和 ADR 中，但其后端实现不属于本前端 Issue。
