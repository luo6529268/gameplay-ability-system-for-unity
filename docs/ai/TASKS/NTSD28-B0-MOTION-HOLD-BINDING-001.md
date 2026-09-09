# Task Contract — NTSD28-B0-MOTION-HOLD-BINDING-001

> 状态：`FOCUSED_TEST_PASS / BUILD-0-0 / UNITY-COMPILE-0 / JOINT-6-OF-6 / REAL-DIFFERENCE-CLOSED`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B0`  
> 建立日期：2026-09-03

## 目标

将 `combat.motionHoldTimer` 从 MISSING 绑定到 `NTSDEntityRuntime.FrameDelay`。权威
`EntityState28::motion_hold_timer` 与 Unity `FrameDelay` 均为有符号停帧计时器：非零时每 tick 向 0 推进，
阻止物理/帧推进（type-3 特例保持），并由 hit、cpoint、holder mirror、respawn 等生产路径写入。

## 允许文件

- `Tools/NTSD28Parity/EntityFieldContract.cs`
- `Assets/NTSD/Scripts/Simulation/Diagnostics/NTSD28UnityEntityRawCapture.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28UnityEntityRawCaptureEditorTests.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditorTests.cs`
- 本 Task/Change、Ledger、STATE、handoff、总表

## 不变量与验收

- 只补 diagnostic binding；不改 `FrameDelay`、physics/frame pass、hit、cpoint、holder 或 respawn 生产逻辑。
- maturity 从 27 verified / 10 candidate / 10 missing 变为 28 / 10 / 9。
- 不同时裁决 `hitReactionTimer`；`FrameDelay` 与 `HitStop/HitStateCount` 不得交叉占用。
- 工具 build 0/0、21/21、5/5、format；Unity compile 0、联合6/6。
- focused 必须分别证明正值3和负值-5的原样投影。
- 真实 neutral raw 中该 missing 差异关闭：目标 unique 12→11、equal 35→36、occurrence72→66。
- Ledger validator 与 scoped diff check 通过。

## 回滚

恢复 `combat.motionHoldTimer` 的 MISSING/null、maturity 27/10/10 和对应测试断言；不触碰生产逻辑。

## 实际结果

- contract/projection 已绑定 `FrameDelay`，maturity 28/10/9；contract SHA
  `52A0284F9F125B47DEA86D634F0900602A578412681683367F33B5A94C28AEF2`。
- 工具 Release build 0 warning/0 error、general self-test 21/21、raw self-test 5/5、format PASS。
- Unity fresh compile 0 error；focused job `0f6ee3e497964378b1d2b8d9b7c141b8` 6/6 PASS，明确覆盖
  `motionHoldTimer=3` 与 `-5`。
- 公共双实体 raw SHA `362F1811010FABB0DE83731DED20F3A716B87B6B3476E22E193B57A697C09D95`；
  真实差异12→11、equal35→36、difference occurrences72→66，首差异仍为 `vitals.baseMaxMp`。
- 生产 physics/frame/hit/cpoint/holder/respawn、DAT、资源、Scene 和 authority 均未改。
