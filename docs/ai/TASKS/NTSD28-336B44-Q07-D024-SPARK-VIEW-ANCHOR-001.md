# NTSD28-336B44-Q07-D024-SPARK-VIEW-ANCHOR-001

状态：`VERIFIED (scoped)`，仅本次活跃 kind0 火花纵向生成出口通过；父 Q07/D-024、Q09、Q12与总目标仍开放。[限定验收](../../../artifacts/diagnostics/NTSD28-336B44-Q07-D024-VERTICAL-FRACTION-20261005/SPARK-ACCEPTANCE.md)记录原始 RED/GREEN、自然 Play、规则样本及 Scene 保护。唯一规则权威为根正式 EXE SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 与对应 playable `BattleWorld28::append_confirmed_native_spark`、`render_snapshot.cpp`；内容使用当前正式 LoganRuntime，项目相机/背景、地图、1.5 倍角色图片尺寸及 DAT 数值不变。

触发首差：[同一自然 C040 命中第25 tick 逐项配对](../../../artifacts/diagnostics/NTSD28-336B44-Q07-D024-VERTICAL-FRACTION-20261005/SPARK-FIRST-DIFFERENCE.md)。正式可绘制 ID0 火花相对 host 地面为 -48/730；原 Battle Scene Unity 命令相对同一 host 地面为约 -48/1152，少约27.75视图像素。正式源先以规则整数算 `targetZ + hitY + CRT jitter`，现 Unity 活跃 `BattleNativeHitSparkWriter.Append` 混用已投影视图 `victim.ZInt` 与源 `hitY+jitter`，`BattleEcsHitExecutionPlan.ProjectKind0HitRecord` 也预测同一混合值；中央发布只读记录锚点。

本包只把**实际活跃的正式 kind0 火花写者及其预测器**改为共用入口：先完成现有源规则 hitY/目标源 Z/CRT 顺序，合成源整数事件 Y，再经 `world.SpatialProjection.SourceToViewZ` 一次生成视图记录 Y。目标拥有 SourceRuleZInt 时使用它；默认恒等投影保持旧结果；缺源历史时按现有碰撞层约定从物理 Z 逆投影作为兼容回退，不以反推值声称正式源精确。火花宿主、ID、X、RNG 计数、容量门、年龄/回收、命中、伤害、DAT、Scene、资源和非战斗逻辑不变。本包只修改 `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleNativeHitSparkWriter.cs`、`Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs` 和一处现有聚焦测试 `Assets/NTSD/Scripts/Test/Editor/NTSD28Q06HitSparkEditorTests.cs`；如果需要改额外脚本，先改本 Task 和 Change Record。

`LF2CharacterDatHitResolver.SpawnSpark` 可从旧直调入口触达，但其历史随机流及几何还不是本次同 tick 自然事件的已证 writer；单独留作条件门，不用本包擅改 legacy hit 规则。Q07/D-024 父项仍须检查这条路径的真实阳性及其它剩余出口，不能因本包通过称火花/战斗完全一致。

风险：错误地把已投影 target Z 二次放大、改变整数截断或两次 CRT 顺序、预测器与实际写者不同步、源历史缺失时写出新差异。先在既有源火花夹具加一条固定视口单例 RED，用同一源事件 Y/随机数结果比较预测的视图锚点，identity 对照不变；再改共用出口。验收按最窄顺序：生成 Editor 编译0错、原 Editor 精确新增测试及一条既有 identity 命中/CRT 邻例、同输入原 Battle Scene 单次自然第25 tick 火花命令，正式源 -48/730 与 Unity 目标约 -75.75/1152 比例一致、规则样本及随机数调用不变、退出 Scene clean/SHA稳。实际不能运行的门如实保留 `RUNTIME_PENDING`。回滚只手工逆向本 Change Record 的 3 个脚本增量，保留已有 D-024 Y 修复及用户其它工作，不执行 Git 破坏性命令。
