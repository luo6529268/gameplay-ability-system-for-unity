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
