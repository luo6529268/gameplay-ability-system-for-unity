<!-- CHANGE-RECORD
id: NTSD-H06-FIND-EFFECT-ZEROGC-001
status: IN_PROGRESS
code-path: Assets/NTSD/Scripts/DatParser/Runtime/Models/LoganKindCatalog.cs
code-path: Assets/NTSD/Scripts/Test/Editor/H06FindEffectAllocationEditorTests.cs
authority: current user explicit FindEffect allocation repair; battle rules unchanged
evidence: docs/ai/TASKS/NTSD-H06-FIND-EFFECT-ZEROGC-20261009.md
-->
# FindEffect局部0GC修复

脚本前登记；准确符号、需求/原状、验收、副作用/关闭及回滚见Task。准备证据仅证明FindEffect71为100对象66个40B事件的首managed frame，分配对象类型/装箱机制仍推断。旧千人234与95不能自动归同因。状态不可提前晋升。

实施进展：新增H06FindEffectAllocationEditorTests，首轮缺using编译错误已修正；原Editor Assembly-CSharp-Editor.dll 2026-10-09T13:25:12Z晚于源码、reload后idle。生产尚未改，先派发原查找语义与100基线job 2813c3da7a3c471eb540ba0d4b99182b。测试状态/缓存仅fixture owner，finally解除seal、注销100实体、Dispose recorder，校验objects/slots0及Profiler设置不变。
生产实际增量：FindEffect foreach改Records.Count索引读取，只该方法；无新状态/缓存/owner/容量、失效或十一阶段关闭责任。CODE_WRITTEN，前基线2/2通过；修复后门待执行。
