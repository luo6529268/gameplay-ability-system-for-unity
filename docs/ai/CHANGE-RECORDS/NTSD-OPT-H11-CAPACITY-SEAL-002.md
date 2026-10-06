<!-- CHANGE-RECORD
id: NTSD-OPT-H11-CAPACITY-SEAL-002
status: RUNTIME_PENDING
change-kind: CODE
code-path: Assets/NTSD/Scripts/Animation/Rendering/BattleCentralRenderSystem.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/BattleDynamicMeshBackend.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/BattlePixelFramePlan.cs
code-path: Assets/NTSD/Scripts/Simulation/Presentation/BattlePresentationDisplayMotion.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattlePresentationCapacitySealEditorTests.cs
authority: user approval to execute next documented optimization batch; H-11 capacity fail-closed, presentation-only; formal336B44 rules unchanged
evidence: artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH02-CAPACITY-SEAL-20261006/REPORT.md
-->

# H-11第二子批：中央表现容量封口

[Task](../TASKS/NTSD-OPTIMIZATION-BATCH02-CAPACITY-SEAL-20261006.md)；
[Operation](../FILE-OPERATIONS/NTSD-OPTIMIZATION-BATCH02-CAPACITY-SEAL-20261006/RECORD.md)。
原状：上一批已有预热；slot CaptureFrame.CopyFrom、mesh.Build与DisplayMotion仍无逻辑硬限，
物理数组余量不是准入上限。当前原Editor Menu clean/idle/nonPlay，errorCS0。
新字段都是缓存准入元数据，不是战斗规则真值；不需要新native语义，绝不参与checksum。

职责：CentralRenderSystem.PrepareBattleCapacity/EndBattleCapacitySeal接入封口/解除，
PrepareFrameImmediate在copy/command消费之前无分配预检，复用CommitCentralFailurePlan。
PixelFramePlan.BattleCentralSubmission在CopyFrom之前按公开count预检；无重排lease/retire/Publish。
DynamicMeshBackend.Build在mutation之前按逻辑command limit检查；独立诊断unsealed仍兼容。
DisplayMotion.Prepare在generation更新前检查prior/current slot；两代合法样本/alpha语义不变。
新增test用reflection访问内部边界，不新增public测试API，不改Q06内部。

副作用：超限时整帧拒绝，保留旧有效提交/无central像素，禁止隐式扩容与部分新帧。
正常接受的quad/segment/采样顺序无变；不改变原fail-closed合同或GPU完成判据。
封口期间再prewarm拒绝；复用旧End解封，只保留缓存，无新owner/worker/关闭阶段。
准确backup含第一批dirty，11个逐文件SHA匹配，不用HEAD替代原状。

验收见Task：RED→GREEN、准确输入和storage身份、局部0B及旧回归、原Editorcompile、
ChangeLedger/scoped diff/保护SHA。完整0GC/GPU/真实Scene/1000 AI/Android保持pending。
失败如实保留；超限的错误直接API抛异常不当作生产零分配正例。
回滚与不可回退边界：不触Scene/资源/DAT/ProjectSettings/Server/PERF/ATLAS/MONO/EXT-1，
无数据迁移；另获批后精确逆向hunk，保留第一批及其它工作。

2026-10-06 test-first：仅新增聚焦test（实际16个具名case），当时尚未修改生产四文件。
封口API用reflection保证原状可编译并RED；counter用既有私有setter构造拒绝边界，
不读Q06方法体。实际运行结果待回收，CODE_WRITTEN仅指test，不表示生产修复完成。

2026-10-06 RED已完成16项，job7d7889c02e8948ce929d9a32b7af3bb1：16预期失败，
14缺少封口API、2生产容量边界尚未提前拒绝而走到camera前置。total9044仅全测试树计数。
随后四生产文件已补：精确logical cap、先copy/先geometry/generation预检、sealed期间禁止Prepare，
接原Prepare/End封口、整帧refusal常量reason、last-good及read lease不改；待原Editor编译/GREEN。
封口测试覆盖public counters，但MaterializeCommands内其它缓存仍未封闭，不当全路径0GC。

2026-10-06 GREEN原Editor编译19:03:08/09，errorCS查询0；
jobbd0e15457fe3477db5e49914eedaf862新16/16通过。17/4096/4097 command硬限、
0容量、sealed Prepare拒绝/End后重预热、prior/current最高槽位及alpha1越界拒绝、
五count的copy前拒绝、无last-good/带read lease两生产拒绝出口均实际通过。
17槽逻辑限与32实际数组长度不混用；geometry mutation/旧frame/count/storage/lease保留。
varying alpha 64次DisplayMotion.Prepare包含自己的预检0B，非全物化/上传/提交0GC。
旧30项聚焦回归正在执行，尚不算通过；父H-11/Scene/设备门继续pending。

2026-10-06 收尾更正（上句为结果回收前快照）：旧回归job519904dbcbba4b988b9db5929971f694
实际30/30通过、无失败或跳过，原件regression-result.json。新增16与旧30去重共46具名case。
原Editor Menu clean/nonPlay/idle，error CS查询0；未开第二Editor/切Scene/进入Play。
与本轮before-backups逐hunk复核四生产文件，不混入第一批代码；新增容量预检的CPU扫描
成本未测，不宣称帧率提升或完整热路径0GC。Q06 body/两个shader/Scene/配置/EXT-1保持。
Tools/Validate-ChangeLedger.ps1实际通过，当前6个governed diff文件含第一批；历史warning
不等于本包错误。最终备份/保护SHA、diff与文档检查结果见报告及Operation after清单。
