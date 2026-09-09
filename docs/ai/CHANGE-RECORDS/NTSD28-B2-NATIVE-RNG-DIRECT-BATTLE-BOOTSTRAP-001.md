# NTSD28-B2-NATIVE-RNG-DIRECT-BATTLE-BOOTSTRAP-001 — direct battle synchronized pre-draw

<!-- CHANGE-RECORD
id: NTSD28-B2-NATIVE-RNG-DIRECT-BATTLE-BOOTSTRAP-001
status: FOCUSED_TEST_PASS
change-kind: CODE_AND_TEST
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSD28NativeRandom.cs
code-path: Assets/NTSD/Scripts/Simulation/Host/SimulationTickDriver.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Session/InProcessBattleWorldBootstrap.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28NativeRandomWorldStateEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28NativeRandomBattleBootstrapEditorTests.cs
authority: NTSD 2.8-Logan playable GameSession28 fresh reset_from_seed followed by direct-battle resolve_native_bgm_selection synchronized call-site 0x004021E0 under current formal random-BGM configuration.
evidence: TASK-CONTRACT-CREATED / TEST-FIRST-4-CS1061 / COMPILE-0 / FOCUSED-28-OF-28 / LOCKSTEP-17-OF-17 / B2-JOINT-INITIAL-RNG-EQUAL / NEXT-FIRST-DIFFERENCE-TICK2-SLOT1-DEFEND-REENTRY-3-VS-0 / AUTHORITY-LAST-CALLSITE-004021E0 / SHARED-DIRECT-BATTLE-RESET-WRITTEN / LOCAL-LOCKSTEP-DIAGNOSTIC-CALLERS-CONNECTED / SELFCHECK-173834-PASS / CONSOLE-0 / CURRENT-DIRECT-BATTLE-ONLY / AUDIO-SELECTION-DEFERRED-B10 / GENERIC-RESET-AND-RESTORE-UNCHANGED / AUTHORITY-READ-ONLY / CONFIG-DAT-SCENE-UNCHANGED
-->

> 状态：`FOCUSED_TEST_PASS / DIRECT-BATTLE-RNG-CURSOR-READY / JOINT-FIRST-DIFFERENCE-CLOSED / AUDIO-SELECTION-B10`

## 改前事实

- authority fresh direct battle seed后进行一次random BGM synchronized draw；B2 header为counter/index/calls1，
  lastSite0x004021E0。
- Unity local host、in-process lockstep和diagnostic bootstrap只seed，header为0/0/0/0。
- 两端CRT state/calls/table hash已相等，差异严格位于post-seed synchronized pre-draw。

## 预期改后职责

- `NTSD28NativeRandom`提供显式fresh direct-battle初始化事务；三个caller共享。
- generic reset/restore继续表示纯RNG状态操作，不偷加BGM行为。
- B10继续拥有可听BGM index和播放；本包只闭合后续战斗RNG游标。

## 验证记录

- Task Contract与Change Record已在任何本包脚本修改前建立。
- test-first仅修改new bootstrap tests、lockstep expected与B2 exporter header断言；Unity精确得到4个CS1061，
  均为`ResetForDirectBattle`缺失。
- 实际实现：`NTSD28NativeRandom.ResetForDirectBattle`先调用generic reset，再以bound1消费一次
  call-site0x004021E0；`SimulationTickDriver.ApplyMatchConfig`、`InProcessBattleWorldBootstrap`和
  Editor diagnostic exporter三处统一使用该helper。
- Unity域重载后Console compile error为0。bootstrap/native RNG/world/exporter focused共28/28 PASS，job
  `8f7109eca3fb4b2cbae0b0e968182245`；lockstep/in-process related 17/17 PASS，job
  `5764a09a225e4836900fbff558bca1a7`。
- input-common B2 raw重跑为valid 3 ticks/6 entities；Unity capture SHA-256
  `4F7EF3995966002AC71719F9FC6846FB13C03FD3A83CC27B448FB8624C5B3914`。initial CRT与synchronized
  counter/index/calls/lastSite/tableHash全部相等，首差已下移到completed tick2 slot1
  `defendReentryCooldown` authority3/Unity0；comparison SHA-256
  `9B60F67662F3492F79F1DE2629F53E4942D5893B136374B5C4BD5448B8FB93F8`。
- 17:38:34 full SelfCheck PASS；其中7条预期失败路径error清除后Console0。
- generic `ResetFromSeed`/restore、selection/loading与audio代码未改。本包没有audio/完整selection runtime结论。

## 回滚说明

按Task Contract恢复caller与移除helper/tests；不回退B2诊断schema或其他RNG consumer。
