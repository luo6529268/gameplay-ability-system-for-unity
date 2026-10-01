# NTSD28-336B44-Q07-C056-FUSION-HOLD-COUNTER-001

状态：`SCOPED_SCENE_PASS / RUNTIME_PENDING`。父目标 `NTSD28-UNITY-BATTLE-REALIGNMENT-001`，G1/BATCH-04/Q07/C056。原Battle Scene正反两条件已由后继[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C056-FUSION-SCENE-PLAY-001/REPORT.md)验到结构出生和退出池；正式根EXE同条件、源活体数及Scene关闭阶段仍待。

权威：336B44 正式 playable `BattleWorld28::advance_native_fusions` 在合体改动动作后保留主角原有 `action_latch`、`tick_action_snapshot` 和 `frame_counter`；同 tick 后续帧 pass 与 `frame_counter==0` 的 OPoint 门决定子体是否出生。正式融合记录 7/8→51 的 OID51 action290 含 kind2 OPoint。须先取得当前源码完整 tick 的非零计数及停帧正例，旧 B1E13 融合 JSON 不作新版预期。

Unity 原状：`BattleOid5152RuntimeModule.TryMerge` 在 `PublishDefinition` 后调用共用 `SetAction`，它把 `AttackingCounter` 清零且将 `Prev2`/`PrevFrame2` 改为新动作，可能让合体帧的 OPoint 提前出生。先以正式数据、非零计数和停顿确认完整 tick 的首差，再在原 Battle Scene 做同初态复现。

预定范围：只改 `Assets/NTSD/Scripts/Simulation/Passes/Oid5152/BattleOid5152RuntimeModule.cs` 的合体动作写入，及 `Assets/NTSD/Scripts/Test/Editor/NTSD28Q06FusionRecordTransactionEditorTests.cs` 中一个聚焦断言。诊断脚本只新建在 `Tools/NTSD28Q07Diagnostics/`，不改正式源码、DAT、Scene、Prefab、相机、旧诊断及非战斗逻辑。原版背景和模式 DAT 继续排除。

验证：正式源码双跑同初态并记录合体前后计数/锁存、OPoint 出生及后继 tick；测试先 RED，最小修复后生成工程编译、原 Editor 聚焦，再按可达性运行原 Battle Scene 和有序退出。旧版融合夹具若与新版相冲突须标为历史，不能拿旧预期阻止或假称本项通过。只有源码/Unity/正式根和必要真实场景证据完整才关闭 C056。回滚仅逐行审阅本 ID 增量；现有未提交文件与用户工作不可清理或覆盖。

实际：当前正式源码完整 GameSession 计数7/停顿3 的前三tick出生数0/0/0，对照计数0/停顿3为1/1/0；两次CSV同SHA。Unity先加聚焦断言，再将合体动作写入改为已有保留计数入口，生成Editor工程编译0错。测试先写但原Editor未运行，因此未实测RED/GREEN；正式根EXE与原Battle Scene仍待。[诊断与验证层级](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C056-FUSION-HOLD-COUNTER-001/REPORT.md)。

后续原Editor已编译并运行：C056新聚焦1/1、旧融合四行在新版三字段投影后4/4、相邻C054解融合1/1均PASS；旧JSON原件保留，修订前行0失败留证。原Scene完整Driver/OPoint与正式根仍待，C056/Q07不关闭。[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C056-FUSION-HOLD-COUNTER-001/REPORT.md)。
