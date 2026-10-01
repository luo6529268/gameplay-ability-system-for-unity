# NTSD28-336B44-Q07-C045-RECOVER-COVER-001

状态：`VERIFIED_SCOPED_UNITY / NATURAL_SCENE_PENDING`。父目标NTSD28-UNITY-BATTLE-REALIGNMENT-001，BATCH-04/Q07/C045。当前正式336B44 `GameSession28::step()`→`BattleWorld28::settle_catch_relations()` 的OID65 action348/X550及357/X700自然抓取伤害已由源码完整120tick和根同LFR各2640字段零差确认：伤害tick18/23，抓取者hold2、被抓者-3；近/远报告见[源/根证据](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C045-NATURAL-REACH-001/REPORT.md)。正式ank.dat action376 injury100/cover1/recover默认0与staged同SHA。

Unity `BattleCpointWriter.ApplyHeldInjury` 当前错误按 Cover 控制双方停顿；`BattleCatchPointValue.Recover` 已存在并由正式DAT投影。只修改以下代码：

- `Assets/NTSD/Scripts/Test/Editor/NTSD28B6HeldInjuryAccountingCoverProductionEditorTests.cs`：把停顿矩阵改成 recover主控、cover独立正反，先原Editor RED；伤害/资源/层级其它测试继续。
- `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleCpointWriter.cs`：仅正伤害后计时条件读取 Recover，不碰 cover 的Z/朝向定位。
- `Assets/NTSD/Scripts/Test/Editor/BattleGrabCpointLinkPlayModeProbeEditor.cs`：如需增加原Battle Scene受控Play的cover1/recover0与反例，使用该既有探针并记录结果。

验收：原Editor先RED后GREEN，聚焦HeldInjury类及相邻抓取、Battle Scene受控Play；正式自然场景同OID65近/远逐tick首差独立后置，不能拿合成Play冒充正式根自然或全Q07。无DAT、Scene、GAS、非战斗、用户比例修改。保护旧脏工作与C042/C044同文件差量；编译/审计/四SHA验证。回滚仅审阅本包代码差量，不执行恢复原文件。

原Editor HeldInjury组40例中cover/recover差异5例如期RED、相邻35例PASS（job f8624bcb8a1c4323b21204ce55297546）；共用writer三处停顿分支已改读Recover，修后编译/同组/Play待。

修后原Editor Tundra build success、0 error；job 9770a3ccb0a546b4990537e53aad4dd8 HeldInjury相关40/40 PASS，包含新增九例及邻近CaughtAct事件/资源/伤害。既有原Battle Scene抓取Play探针已改正式差异条件cover1/recover0并换独立输出路径，刷新/Play待。

2026-10-01 原Editor新编译后在原Battle Scene运行既有抓取Play探针，显式cover1/recover0，结果PASS：catcher FrameDelay2/victim -3，持有位置116/19/201符合期望；临时对象、slot与两池回到基线，cleanupCompleted=true。已退出Play且Editor idle/非编译，Battle/Menu/GameConfig/ProjectBattleModeConfig四SHA与运行前一致。受控Unity机制限定验收见[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C045-RECOVER-COVER-001/ACCEPTANCE.md)；自然Unity近远逐tick仍待，父C045/Q07开放。上段“刷新/Play待”为运行前阶段，不再是当前状态。
