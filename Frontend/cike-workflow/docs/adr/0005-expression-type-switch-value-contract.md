# 表达式类型切换的 value 契约：统一重置 + 单条复合撤销命令

切换 `expression.type` 时，value **一律重置到目标类型的中性值，不做任何跨类型搬运或转换**：四个非 Literal 类型（JavaScript / Liquid / Variable / WorkflowInput）的 value 都是字符串，重置为 `""`；Literal 的 value 是具体的带类型值，重置为调用方经 `literal-default` 传入的具体零值/默认值（fallback：descriptor.defaultValue → 类型零值），**绝不使用裸 null**。整次切换（type + value）提交为**一条复合可撤销命令**，一次撤销完整回滚。

理由：字符串只是存储形态，语义不可跨类型移植（JS 脚本 ≠ Liquid 模板 ≠ 名字引用），跨类型"转换"只会产出后端校验失败的脏数据；统一重置最可预测、最好测试，误切靠撤销找回。

拒绝的方案：跨类型自动转换；JS↔Liquid 保留文本（Liquid 是模板不是代码）；记住切换前的具体值再恢复（缓存旧值使撤销语义变绕）；Literal 重置为裸 null（Literal 恒为具体值）；用两条独立 property 命令分别改 type 与 value（会留下"类型变了值没变"的半状态）。
