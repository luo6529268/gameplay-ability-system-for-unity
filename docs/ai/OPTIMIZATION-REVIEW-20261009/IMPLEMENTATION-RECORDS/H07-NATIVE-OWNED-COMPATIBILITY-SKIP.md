> **2026-10-10 NTSD-H07-NATIVE-OWNED-COMPAT-SKIP-20261010：FAILED_GC_QUALIFICATION / FINITE_TASK_STOPPED；Change NTSD-H07-NATIVE-OWNED-COMPAT-SKIP-001 BLOCKED，候选默认OFF。** 已批准Core冷接口替换后五脚本候选已写，最终生产19/19及资格4项通过、固定1000capture1000→0/原skip保留/纯pass0GC；唯一无细分OFF120+180完整Driver校准有效但steady2events(tick130/162)，停止余下ON/ON/OFF，收益与实景ON等价未测，不建议准入。300tick旧同fixture14字段0差、17字段原值已存但未配对；容量/关闭三0/Menu clean8/设置恢复，15,558其它保护稳+已批准Core，16备份及五源码窗前后稳。无新调用栈/归因/重测/新优化，producer暂缓/H06不续/95OFF/最终门保持。报告 artifacts/diagnostics/NTSD-H07-NATIVE-OWNED-COMPAT-SKIP-20261010/REPORT.md；下方原状态保留历史。

## 本次实际实施与结果（取代下方尚未实施状态）
2026-10-10：FAILED_GC_QUALIFICATION / CANDIDATE_IMPLEMENTED_DEFAULT_OFF。
原代码每exact AI调用完整progress resolver，capture17、original复制、native no-op判断、比较后skip。新代码仅在原DataOriented/exact AI/runtime加同World、native、显式DataOrientedCanonical、CanEvaluateCanonicalDecision全部成立时记录一次旧skip和fastskip，保留外围ComboUpdate和原tail；false保持resolver。flag/long属于既有pass，Reset关/清，Core只22行冷facade；普通class不改partial，Diagnostics未写。
解决的是已被native接管后的兼容空转，不改变AI计算/查询/native动作/publication/规则。最终生产focused19/19、资格4项，17字段与tail、fixed1000capture1000→0/fastskip0→1000/旧skip1000、warm纯pass0GC成立。唯一实景OFF120+180完整Driver稳态2event130/162（校准有效），停止余三窗。14字段旧基线0差和17原值是OFF证据，ON等价与可辨收益未测；不能建议限定准入。窗口前后五源码稳、16备份有效、关闭三0/Scene/settings恢复。完整命令/历史测试失败/边界见本Task REPORT；不重跑或扩展分配修复。
后继依赖由人类决定；默认OFF，H07/Android最终门开放。下方旧评审伪代码和REPAIR_READY仅为事前历史。

> 2026-10-10 H07 native-owned candidate: NTSD-H07-NATIVE-OWNED-COMPAT-SKIP-001 BLOCKED / 写域确认待答。仅新增一项测试、原Editor预期RED；生产未改、GREEN/ABBA未运行。World非partial且注释明确禁止该拆分，撤回Diagnostics中World partial计划；待用户确认将冷配置/计数入口放到Core现有诊断入口附近。原Task范围外不实施，不恢复Goal/队列；最终门保持。

# H07输入兼容外壳空转消除

状态：REPAIR_READY / IMPLEMENTATION_NOT_STARTED / PRODUCTION_DEFAULT_OFF。
这是原H07-INPUT子项，未新增父项。证据：[CPU诊断报告](../../../../artifacts/diagnostics/NTSD-H07-INPUT-SPIKE-CPU-DIAG-20261010/REPORT.md)、此前input-flow报告/8项focused。

## 优化点 → 原来如何处理
BattleEcsCharacterInputPass.TryExecute对exact AI调用ApplyFrameInputFromRuntimeProgress：读取17字段input，复制original，进入native已接管而不执行combo/direct/release的分支，ContentEquals后记录skip；随后仍执行原frame-motion tail。实际两窗每tick1000 skip/0commit，共各180000 steady skip。

## 拟改成什么
仅在同World/native拥有、DataOriented pass+DataOrientedCanonical、exact LF2Character AI、runtime有效且当前input store owner绑定有效时，直接记录一次原skip，绕开这次兼容resolver，继续原frame-motion tail和后续原生输入处理。任一条件失败回原调用。不在共享resolver全局提前return；CanEvaluateCanonicalDecision不含profile，必须另验profile。

```csharp
// 方案伪代码；尚未实施。外层DataOriented/exact AI/runtime门维持原样。
if (candidateEnabled && sameWorld && nativeInputOwned &&
    profile == DataOrientedCanonical && canonicalOwnerIsValid)
{
    world.RecordCharacterInputProgressCommitForDiagnostics(false);
}
else
{
    resolver.ApplyFrameInputFromRuntimeProgress(character, writer, diagnostics);
}
// 原applyFrameMotionTail分支与后续native routing/row publication保持。
```

## 解决原来的什么
消除一个已被原生管线接管的兼容阶段中的重复字段捕获/复制/比较；没有消除AI思考、目标查询、原生组合键/动作处理或producer刷新。无Profiler、周期provider OFF但elapsed细分ON的父段约0.479ms/tick，属于诊断时间，不保证全额可省，更不保证12ms整体归零。

## 验收和边界
先guard正/反例、完整17字段/正确skip/尾部、外来与重绑owner/mixed/derived focused，再一次未启Profiler/细分/cycles的受控ABBA及逐tick输入双RNG完整checksum、完整Driver0GC/容量/有序关闭。已知新首OFF2events130/162、第二诊断窗0不抵消；不因候选实现正确放宽0GC，不自动默认推广。失败/不可辨收益保留OFF结束此版。
实施会话完成后在本文件追加实际前后代码、命令、结果和生产准入结论；目前不得写VERIFIED或“优化已完成”。
