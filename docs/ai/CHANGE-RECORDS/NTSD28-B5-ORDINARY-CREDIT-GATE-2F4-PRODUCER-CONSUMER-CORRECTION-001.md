# NTSD28-B5-ORDINARY-CREDIT-GATE-2F4-PRODUCER-CONSUMER-CORRECTION-001

<!-- CHANGE-RECORD
id: NTSD28-B5-ORDINARY-CREDIT-GATE-2F4-PRODUCER-CONSUMER-CORRECTION-001
status: VERIFIED
change-kind: BEHAVIOR_CORRECTION
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleCpointWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Passes/BattleEcsCharacterRecoveryPass.cs
code-path: Assets/NTSD/Scripts/Animation/Character/LF2ObjectPointFactory.cs
code-path: Assets/NTSD/Scripts/Simulation/Runtime/BattleLogicEntityFactory.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5OrdinaryCreditGate2F4CorrectionEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: NTSD 2.8-Logan battle_world.cpp ordinary/reduced/caught damage, C25 MP threshold and OPoint materialization read/write only Entity28+0x2F4; EXE B1E13AE1, closure 39DDDA15.
evidence: Task/Change created before scripts. Focused RED exactly 0/5 across standard/reduced/CPoint damage, data and legacy recovery, logic OPoint and source closure. Rebound four actual damage checks, one CPoint check, four HitPlan checks and two recovery checks to OrdinaryCreditGate2F4; both OPoint factories now write exact propagation only for type0 and no longer write KillCount. Final focused 5/5 and related 287/287 passed. Runtime/editor builds are 47 warnings 0 errors and 104 warnings 0 errors. Targeted NTSD_Battle Play passed 5 cases with Console 0 error and scene SHA 50FD4D8F unchanged, dirty false, roots 13. Full SelfCheck reaches the same unrelated CPoint throw mode0 Vz blocker. One excluded B6 CPoint fixture cannot start because its pre-existing Category contains a prohibited hyphen; direct package CPoint test passes. Legacy stats fields/writers, flute branch and carrier/schema remain excluded.
-->

> 状态：`VERIFIED / RED_0_OF_5 / FOCUSED_5_OF_5 / RELATED_287_OF_287 / TARGETED_PLAY_5_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED / EXACT_2F4_READERS_PRODUCERS / LEGACY_STATS_WRITER_NEXT`

完整范围、Authority、不变量、验收与回滚见
`docs/ai/TASKS/NTSD28-B5-ORDINARY-CREDIT-GATE-2F4-PRODUCER-CONSUMER-CORRECTION-001.md`。

## 实际改动

- standard/reduced actual、CPoint、HitPlan与legacy/ECS recovery共11个reader改读exact +0x2F4。
- presentation/logic两个OPoint factory仅对type0 child写
  `parent +0x2F4 > -1 ? parent +0x2F4 : parent physical slot`；non-type0不写，legacy KillCount不写。
- damage/stat数值与顺序、flute legacy writer、state501 helper及全部carrier/schema未修改。
- 既有SelfCheck中三处以`KillCount=0`表达旧stats gate的夹具同步显式设置exact +0x2F4=0；revival等故意
  证明KillCount无关的夹具不改。

## 验证结果

- RED：focused精确`0/5`；五项分别覆盖standard、reduced/CPoint、data/legacy recovery、OPoint与source closure。
- GREEN：focused `5/5`；HitPlan、recovery、W05 presentation OPoint生命周期、B0 logic OPoint、kind4、
  reduced、unarmored与armor联合回归`287/287`。
- 另一次包含`NTSD28B6CpointThrowAtomicProductionEditorTests`的尝试在该既有fixture OneTimeSetUp处失败：
  Category name含Unity Test Runner禁止的`-`。本包未修改该文件；本包direct CPoint writer正反sentinel已通过。
- build：`Assembly-CSharp.csproj`为`47 warnings / 0 errors`；
  `Assembly-CSharp-Editor.csproj`为`104 warnings / 0 errors`。
- 2026-09-09 06:37 +08前后真实`NTSD_Battle` Play通过5个聚合case；Console 0 error；Scene SHA-256
  `50FD4D8FAF2CC5C630886CEFD4742A5FD068E1F7AADEE89809AD19B7B85AFF3C`、dirty=false、rootCount=13不变。
- full SelfCheck仍在既有`CheckCpointThrowRawAndTransformMatrix()` character raw throw mode=0 victim Vz断言
  停止；与此前阻塞点相同且早于本包后段检查。
- 生产扫描确认本包四个damage actual、一个CPoint、四个HitPlan、两个recovery及两个factory的旧绑定归零；
  只剩route6 flute stats、B3 compatibility helper和carrier/schema引用。
