# Task Contract — NTSD28-B2-DEFEND-REENTRY-EXACT-FRAME-REFRESH-001

> 状态：`FOCUSED_TEST_PASS / EXACT-REFRESH-READY / INPUT-COMMON-JOINT-EQUAL / FORMAL-PER-CALL-PENDING`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B2 / EXACT-INPUT`  
> 建立日期：2026-09-04

## 目标

闭合 NTSD 2.8-Logan frame-state pass 在结果 action 110/114 时，将 exact
`defend_reentry_cooldown`刷新为3的生产写入。Unity现有frame pass已经调用
`BattleCharacterInputWriter.SetDefendLock(runtime, 3)`，但writer只同步legacy
`CdDefendLock`，没有同步B2 exact `NativeInputProxy.DefendReentryCooldown`。

## Authority 与当前证据

- playable live source `battle_world.cpp:7753-7758`：每个surviving frame-state pass在结果action
  110/114时写`defend_reentry_cooldown=3`。
- `input_routing.cpp:70-75`：后续input pass在edge detection前递减该byte。
- Unity `BattleEcsCharacterFrameTickPass.cs:193-200`及legacy-compatible
  `LF2Entity.cs:5940-5946`已经在action110/114调用相同writer；不是缺少frame gate或pass位置。
- latest input-common B2 joint首差为completed tick2 slot1 cooldown authority3/Unity0；初始化双RNG及其余
  earlier exact字段均已相等。

## 允许修改

- `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleCharacterInputWriter.cs`
- 新增`Assets/NTSD/Scripts/Test/Editor/NTSD28DefendReentryExactFrameRefreshEditorTests.cs`及`.meta`
- 本Task、Change Record、Ledger、STATE、handoff与总表

禁止修改frame pass顺序、input decrement、action routing、legacy profile、Config/DAT、Scene/Prefab、
ProjectSettings、authority和诊断schema。

## 不变量

- `SetDefendLock`是既有frame110/114的统一事务边界；更新后legacy、SoA store与exact carrier获得同一byte。
- generic exact input decrement/clear语义不变；本包不在frame pass外增加刷新。
- action不为110/114时不会因本包产生任何写入。
- 无每tick分配，无Unity presentation依赖。

## 验收

- test-first在production未改时精确失败：legacy为3但exact仍为旧值；
- 实现后new focused、input writer/native combo/type0 production相关测试通过且compile0；
- input-common B2 joint 3 ticks/6 entities全量equal，或记录新的更晚first difference；
- SelfCheck PASS、Console0、Change Ledger与diff check通过。

## 回滚

移除writer对exact carrier的镜像写入及本包测试；不回退既有frame pass gate、B2 schema或前序RNG修复。

## 当前证据

- test-first new3/3预期失败，job `2036ab44931240e2a3e31aeea7254109`；三个case的legacy值均正确，
  exact分别保留255/0/0，缺口严格位于exact mirror。
- writer已在既有non-null事务中同步`CdDefendLock`与`NativeInputProxy.DefendReentryCooldown`；
  frame110/114 gate、input decrement和action routing不变。
- Unity compile0；new/writer/combo/type0 production focused75/75 PASS，job
  `cc77e669de7647899a7cf25bec5406a0`。
- input-common B2 exact comparison为3 ticks/6 pairs全equal、`firstDifference=null`。17:47:38 SelfCheck
  PASS，预期7条error清除后Console0。formal EXE/per-call RNG仍另待B2证据。
