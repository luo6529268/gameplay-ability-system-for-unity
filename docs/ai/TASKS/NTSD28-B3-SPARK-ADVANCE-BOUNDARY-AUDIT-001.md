# Task Contract — NTSD28-B3-SPARK-ADVANCE-BOUNDARY-AUDIT-001

> 状态：`VERIFIED / PRESENTATION-DRIVEN-LIFECYCLE-CONFIRMED / EXISTING-CALL-NOT-MOVABLE / NEXT-NATIVE-SPARK-LIFECYCLE-CORE`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B3 C01`  
> 依赖：`NTSD28-B3-ACTUAL-PHASE-SEQUENCE-BASELINE-001 / FOCUSED_TEST_PASS`  
> 建立日期：2026-09-04

## 目标

只读闭合Authority C01 `advance_native_sparks()`与Unity hit-record/spark production链，区分：

- 逻辑年龄推进/尾部删除；
- 本tick命中新记录的base-age写入；
- immutable presentation capture；
- renderer materialization；
- worker与`buildPresentation=false`路径。

判断是否可以在B3仅移动逻辑边界并保持B9视觉资源/绘制算法不变；若可以，选出最小test-first包，
若不可以则把B9依赖写成显式阻断，不能为消除首差做空checkpoint。

## 允许操作

- 只读Authority `advance_native_sparks`、hit spark生成、GameSession/main snapshot链及相关tests。
- 只读Unity `BattlePresentationShadowBuild`、`LF2Entity` hit records、TickSystem、StageRender、worker、
  snapshot/checksum与相关tests。
- 修改本Task/Record、Ledger、STATE、handoff、总表和B3 manifest。
- `code-path: NONE`；不得修改C#/C++/tool source、Config/DAT、Scene/Prefab、ProjectSettings、Packages或authority。

## 验收

- 冻结Authority旧spark推进、新spark生成、snapshot三者精确时序和尾部删除规则。
- 冻结Unity capture→finalize/writeback、no-publication与worker路径的真实所有权。
- 确认logical hit-record字段是否进lockstep snapshot/checksum，避免presentation可用性决定规则状态。
- 输出可安全移动的最小边界、需要保留的API兼容面、red tests与回滚。
- 不把“存在类似age++”当作时序已对齐。

## 回滚

仅回滚治理文字；production与authority零修改。

## 审计结论

- Authority `BattleWorld28::advance_native_sparks()`在C01遍历完整slot表；`id 9..99 && id%10==9`为
  terminal cell。terminal非尾槽保留，terminal尾槽每个pass最多弹出一个；其他`0..98`递增，负数和`>=100`
  保持。现有authority tests锁定`9,17→9,18→9,19→9→empty`及`30→...→39→empty`。
- 普通hit在C14才追加新spark；host按depth、tie高slot，容量10；初始logical native id由显式100～199或
  `(group*2 + fallGate)*10`决定。新spark位于C01之后，因此本tick不递增。
- completed render snapshot在GameSession G09～G16之后读取当前native ids；画面资源缺失只影响drawable，
  不阻止逻辑age/lifecycle。
- Unity `BeginFrame`先冻结hit-record ages，`FinalizePublishedHitRecordCycle`随后按capture校验并修改实体age；
  `buildPresentation=false`又走`AdvanceHitRecordsWithoutPublication`。两条逻辑writer都位于U25，发生在本tick
  hit创建后，因此新hit本tick就被逻辑推进（尽管已冻结的画面副本仍显示base age）。
- Unity lifecycle catalog只认`0..4 / 10..14 / 20..28 / 30..38`，不是native terminal-last-digit-9；
  旧writer还受CommonVisualCatalog/RuntimeDataCatalog可用性控制，资源缺失会冻结逻辑hit-record。
- dedicated worker同样在`RenderDispatch`内capture并立即finalize，未提供不同语义。
- hit-record count/age/x/z/last-advance-tick进入full state snapshot/restore和parity presentation projection；当前
  lockstep checksum未写这些字段。这是后续checksum完整性风险，但不在本只读包顺手改schema。
- 因此不能直接把现有presentation writer搬到tick-start。唯一下一包为
  `NTSD28-B3-NATIVE-SPARK-LIFECYCLE-CORE-001`：先在`LF2Entity`建立纯logical native cell/tail primitive与
  focused tests，保持production unconnected；之后另包接C01并撤除RenderDispatch production writeback。
