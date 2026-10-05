<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-D024-SPARK-VIEW-ANCHOR-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleNativeHitSparkWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06HitSparkEditorTests.cs
authority: user D-024 same-screen-fraction decision; current 336B44 playable append_confirmed_native_spark and render_snapshot; paired C040 natural tick25 presentation first difference
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-D024-SPARK-VIEW-ANCHOR-001.md
-->

# D-024 正式 kind0 火花生成时源事件 Y→视图 Y

本记录于上述三个脚本修改**之前**建立。当前生产在 `BattleNativeHitSparkWriter.Append` 以已投影 target Z + 未投影源 hitY/jitter 写整数记录，ECS 预测器同样混合；[自然第25 tick 首差](../../../artifacts/diagnostics/NTSD28-336B44-Q07-D024-VERTICAL-FRACTION-20261005/SPARK-FIRST-DIFFERENCE.md)已测 48/730 对 48/1152。完整正式调用链、预期副作用、三脚本所有权、源历史缺失兼容、测试、风险、保留边界与手工逆向回滚见 [Task](../TASKS/NTSD28-336B44-Q07-D024-SPARK-VIEW-ANCHOR-001.md)。状态 `PLANNED`；截至此记录尚未改脚本、编译或重跑 Play，旧自然证据是 RED 基线而非本包 GREEN。

2026-10-05 测试先行：只在已声明的 `NTSD28Q06HitSparkEditorTests` 新增 `SourceSparkEventYProjectsOnceAtFixedView` 两参数实例。复用现有正式源火花 fixture 的 index3（源事件Y10、target源Z20、CRT两次），identity期望记录10；固定2048×1152视口设置明确 SourceRuleZ20 与物理 Z31，期望全事件投影后整值15。生产尚未改；本测试是当前混合写者的预期 RED，原 Editor 运行及生成编译待。

2026-10-05 最终状态 `VERIFIED (scoped)`，覆盖上述 `PLANNED` 快照。先修测试的 `ulong CrtCalls` 类型错误，生成 Editor 编译从2个测试类型错误到0 error；原 Editor 测试先行 job `6b278df4410949b19baff71bf52ce13d` 为 identity PASS/固定视口 expected15 actual21 RED。随后仅修改已声明两生产脚本：活跃写者先保持原有 hitY、target 源 Z、两次 CRT 顺序，用 `ProjectSourceEventY` 合成源整数事件再通过共用 `BattleSpatialProjection` 投成视图整数；预测器复用该函数。源历史缺失时从精确物理 Z 逆投影作为兼容回退，不声称该回退已获自然阳性同态。

原 Editor 刷新后新增例 job `de5cbf0bbffd40d28c30648ab2680ee0` 2/2 PASS，旧身份域数组/CRT job `02b7a87427be41e699c4540cdba2ca3a` 2/2 PASS。原 Battle Scene 同一 C040 40 tick 的规则样本0差，火花 world Y `-7.88000059→-7.60000038`，逻辑相对 host 约 `48→76` 视图像素；X、pic、sort、宽高不变。退出 Play、Scene clean/SHA稳、Console 0 error。详情与边界见 [限定验收](../../../artifacts/diagnostics/NTSD28-336B44-Q07-D024-VERTICAL-FRACTION-20261005/SPARK-ACCEPTANCE.md)。未改 DAT/Scene/角色图片/非战斗；本包无需自动回滚。父 Q07/D-024、Q09、Q12及总目标仍开放；legacy 直调和缺源历史路径只按实际阳性条件门处理。
