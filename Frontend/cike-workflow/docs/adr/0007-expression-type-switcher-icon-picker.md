# 表达式类型切换器：图标触发的单选 popover（收敛 ADR 0004 的下拉框）

ADR 0004 让 `ExpressionEditor` 独占类型选择器，初版实现为一个 `w-40` 的文字下拉框，钉在每个 Input 行右上角。它丑、且面板一挤就溢出。我们改为：类型切换器是一个**只显示当前类型图标的图标按钮**（图标名来自后端 `ExpressionDescriptor.icon`，lucide kebab-case，由前端 `resolveExpressionIcon` 映射到 `@lucide/vue` 组件），置于 **value 控件同行的右侧**（Monaco 多行时图标顶部对齐）；点击展开一个列出全部类型 `[图标 + 名称]` 的单选 popover，选中即触发既有的复合可撤销切换命令（value 按 ADR 0005 重置）。当前类型名不再常驻界面，仅由图标按钮的原生 `title` 与菜单项名称承载。

底座**复用现有 reka-ui `Select`**（"从一组里单选一个类型"正是 Select 的语义），触发器改为无 chevron、无边框的 ghost 图标按钮——而**不是**新增 `dropdown-menu` 组件（那是动作列表语义，且要多加一套 `ui/` 组件）。图标名映射沿用 activity `icons.ts` 的模式：前端只解析自己打包的有限名字集，未命中/拉取前用 fallback 图标兜底。

拒绝的方案：点击图标在类型间循环（5 种类型要多次点，且每次切换都按 ADR 0005 重置 value、反复污染 undo 栈）；图标绝对定位悬浮在 value 编辑区右上角（会遮住 Monaco 内容）；图标旁常驻类型文字（又把"占宽/溢出"的老问题拾回来）；新增 dropdown-menu 组件（语义不符且组件面扩大）。
