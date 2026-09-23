# ExpressionEditor 组件职责边界与 Literal 插槽

节点表单里每个 `Input` 参数都是一个表达式（类型 + 值），需要一个统一的编辑器。我们决定由 `ExpressionEditor` 组件**独占**类型选择器（数据来自后端 ExpressionDescriptors 端点，修复此前前端写死含未注册 CSharp、漏 WorkflowInput/Variable 的缺陷）、**全部四个非 Literal 编辑器**（JavaScript/Liquid 用 Monaco 懒加载，Variable/WorkflowInput 用按名字的下拉选择器），以及**所有写模型的动作**（经命令栈提交，参与 undo/redo）。

调用方（节点表单）**只提供 Literal 的值编辑器**，通过默认插槽传入，并用 `literal-default` prop 告知该字段的具体零值/默认值——因为 Literal 的值是带类型的具体值，只有节点知道 `Input<T>` 的 T。

拒绝的方案：做成 `v-model:type` / `v-model:value` 的"哑组件"、由每个调用方自行接命令栈——那会把 undo/redo 管线推回每个表单，违背"通用"初衷。后果：各专用表单（SendHttpRequest/For/Switch/RunJavaScript 等）以"现有控件进 `#literal` 插槽、外层包 ExpressionEditor"的机械方式接入。
