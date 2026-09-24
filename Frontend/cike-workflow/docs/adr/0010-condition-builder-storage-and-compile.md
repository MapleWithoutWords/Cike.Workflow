# If/Switch 条件用前端 builder 树（conditions + combineCondition 递归）存 customProperties，单向编译为 Javascript 表达式执行

设计器要为 If 的条件与 Switch 每个分支的条件提供可视化配置。后端表达式类型集合冻结（Literal / Javascript / Liquid / Variable / WorkflowInput），既不新增"条件"类型、也不解释任何前端结构。我们决定：**编辑真源是一棵前端专属的 builder 树，存进活动的 `customProperties`（后端不透明扩展袋，原样存还、从不解释）；每次变更单向编译为 `{ type:"Javascript", value:"<js>" }` 写入真正的条件表达式（If→`condition`，Switch→`case.value`），后端只执行编译产物。**

树的形状**直接对应交互布局**：一个条件组 = 一排扁平比较行 `conditions[]` + 至多一个内缩子组 `combineCondition`，二者由组上的 `conditionType`(and/or) 接合；递归 `combineCondition` 得到任意深度嵌套。每个条件（If 的 `ifCondition`、Switch 的 `caseConditions[i]`）带判别子 `type`：`custom` 走 builder 树，其余是"逃生舱"直接编辑裸表达式。

## Schema（存于 customProperties.customExpression）

```jsonc
ifCondition / caseConditions[i] = {
  type: "custom" | "Literal" | "Javascript" | "Liquid",
  value:
    | { conditionType: "and" | "or",
        conditions: [ { left: Operand, operator: Op, right: Operand } ],
        combineCondition?: <同形状递归> }   // type = custom
    | true | false                          // type = Literal
    | "<code>"                              // type = Javascript / Liquid
}
Operand = { type: "Literal" | "Javascript", value: any, dataType?: "string"|"number"|"boolean"|"datetime" }
```

Switch 的 `caseConditions[i].label` 投影为出端口；`mode`（首个优先/匹配所有）仍留在 Switch 活动上，不进本容器。If 用 `ifCondition`、Switch 用 `caseConditions[]`，两字段分开、不合并。

## 编译契约（前端纯函数）

- **操作数**：`Literal` 按 `dataType` 出 JS 字面量（string 加引号转义、number 原样、boolean `true/false`、datetime→`new Date('ISO').getTime()`）；`Javascript` 原样内联。
- **运算符**：`=→===`、`!=→!==`、`> >= < <=` 原样；字符串另有 `contains/notContains/startsWith/endsWith`（编译为 `String(l).includes(r)` 等）与一元的 `empty/notEmpty`。
- **组**：`conditions[]` 用 `conditionType` 的 `&&`/`||` 连接，再与 `"(" + compile(combineCondition) + ")"` 用同一操作符接合；嵌套子组整体加括号保证优先级；某侧为空退化为恒等（空组 = `true`）。
- **运算符/控件按 dataType 分集**：number、datetime = `= != > >= < <=`；string 另加 `contains notContains startsWith endsWith empty notEmpty`；boolean = `= !=`。Literal 控件按 dataType 渲染（文本/数字/开关/日期时间）。

## Considered Options

- 后端新增原生 Condition 表达式类型直接存树：被否。用户明确"后端不处理前端的东西"，且突破后端冻结约束。
- 通用 n-ary 条件 AST（children 混排叶子与子组）：被否。表达力虽全，但模型与界面之间隔一层翻译；本产品的交互就是"扁平行 + 一个内缩子组 + 接合操作符"，`conditions + combineCondition` 正是该交互的直接序列化，渲染/编辑零翻译，且已被生产系统（Octopus 指标告警条件配置）验证。
- 受控 activeType + 漂移检测决定显示 builder 还是代码：被否。判别子 `type` 已存于数据，一次字段读取即可，无需状态机。
- builder 操作数支持 Liquid：被否。编译产物是 Javascript、由 Jint 执行，Jint 内无 Liquid 渲染器，Liquid 操作数无法内联；Liquid 仅保留为整条件逃生舱。

## Consequences

- `customProperties` 承载编辑真源并随定义进版本；老数据无 `customExpression` 时从现有 condition 反推种子（Javascript→Javascript、Liquid→Liquid、Literal 布尔→Literal）。
- 显示哪种编辑器由 `type` 一次读取决定：`custom`→builder，其余→ExpressionEditor（allowedTypes 限 Javascript/Liquid/Literal，Literal 渲染真/假）。
- 编译单向：改树即重编译覆盖表达式；不反向解析 JS 回树。
- 新增纯逻辑缝 `core/designer/conditionCompile`（树→JS），vitest 覆盖；ConditionEditor UI 另计。
