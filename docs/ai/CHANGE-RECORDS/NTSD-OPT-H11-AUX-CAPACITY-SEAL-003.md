<!-- CHANGE-RECORD
id: NTSD-OPT-H11-AUX-CAPACITY-SEAL-003
status: RUNTIME_PENDING
change-kind: CODE
code-path: Assets/NTSD/Scripts/Animation/Rendering/BattleCentralRenderSystem.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/BattlePixelFramePlan.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/BattleFootMarkerBatchBackend.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/BattleHealthBarBatchBackend.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleAuxiliaryPresentationCapacitySealEditorTests.cs
authority: user approval to execute next documented H-11 presentation-only capacity batch; formal336B44 rules unchanged
evidence: artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH03-AUX-CAPACITY-SEAL-20261006/REPORT.md
-->

# H-11辅助marker/bar缓存容量封口

[Task](../TASKS/NTSD-OPTIMIZATION-BATCH03-AUX-CAPACITY-SEAL-20261006.md)；
[Operation](../FILE-OPERATIONS/NTSD-OPTIMIZATION-BATCH03-AUX-CAPACITY-SEAL-20261006/RECORD.md)。
原状：FootMarker.BuildFromFrame/HealthBar.BuildFromFrame/Build在热调用PrepareCapacity；
预热但无逻辑sealed门。实际是否已分配/耗时是待测，不把静态可达当每帧热点。
只新增缓存准入metadata及计数预检；非模拟字段，无checksum/native规则变更。

Foot/Health：PrepareCapacity、Seal/Unseal、CanBuildFrame与Build前边界；按原eligible条件计量；
Health direct Build也先拒绝。PixelFramePlan.BattleCentralSubmission原Seal/Unseal接入辅助owner。
Central原capture前/command-materialize后准入补辅助检查，复用原整帧fail-closed。
正常顶点/颜色/UV/锚点/顺序原样保留；不变segment/fence/lease/retire/Publish/11阶段。
原End解除仅metadata，无新owner/queue/worker；诊断unsealed fallback保留。

验收与回滚见Task：新RED→GREEN、精确limit/physical余量、filtered/disabled、旧frame/storage/
mesh mutation/CPU lease保持、局部0B、旧具名几何和两批回归、原Editorcompile/保护SHA/Ledger。
完整0GC/GPU/native/Scene/设备仍pending，未批准专项保持。备份含前两批dirty，不用HEAD替代。
当前尚未改脚本或运行本批测试；所有结果按实际追加。

2026-10-06 test-first：仅新聚焦test已写，计划15具名case；四生产文件仍为本批before原状。
用reflection访问未来封口API保证原状可编译后RED，不新增public测试入口或读取Q06方法体。
CODE_WRITTEN仅指测试代码，尚无生产修复/运行通过结论。

2026-10-06 原Editor test-first job21d2fb44b417485c8a9ead86e1d55121：
实际15/15预期RED，10缺少SealCapacity、1辅助sealed字段不存在、4生产缺提前容量拒绝而走到camera前置。
RED原件保存。随后四生产文件已写：辅助eligible count在任何缓存/mesh mutation前拒绝，
sealed禁Prepare，slot原Seal/Unseal接辅助；central capture前/command materialize后加入预检。
未改变正常几何/UV/颜色/顺序、无新owner/fence/生命周期；生产编译/GREEN尚待执行。

2026-10-06 实际编译19:25:09/12，原Editor idle/error CS0；
GREEN job221ae817583f4a99978d6c96059221cc新15/15通过，0 skipped/failed。
边界/filtered/disabled/两slotSeal与End/四生产fail-closed出口/read lease保持均已聚焦通过；
64次交错帧Foot+Health BuildFromFrame（含自身预检和mesh上传）局部0B，不等于完整渲染链。
旧61具名回归请求已提交，结果尚待回收；父H-11/Scene/GPU/设备与新增扫描CPU耗时仍pending。

2026-10-06 收尾更正：旧回归jobfdfc71935b064d4f81a7a6cdcb4d2feb实际61/61通过，
regression-result.json保存原件；上句为请求后快照。新15/旧61共76去重具名EditMode case。
四生产hunk已与本轮dirty精确备份比对；只是准入/生命周期接线，没有几何或规则改写。
新增预检CPU耗时未知；无Scene/资源/ProjectSettings/Q06 body/EXT-1/PERF/ATLAS/MONO修改。
最终Ledger/diff/保护和备份SHA/Editor非Play状态存本包报告及Operation after，不晋升父项。
