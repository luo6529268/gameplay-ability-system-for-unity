# Task Contract — NTSD28-B0-HIT-REACTION-BINDING-001

> 状态：`FOCUSED_TEST_PASS / BUILD-0-0 / UNITY-COMPILE-0 / JOINT-6-OF-6 / REAL-BASELINE-UNCHANGED`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B0`  
> 建立日期：2026-09-03

## 目标

把 `combat.hitReactionTimer` 从错误 candidate `NTSDEntityRuntime.HitStop` 更正为
`NTSDEntityRuntime.Fall` 并晋级 VERIFIED。权威字段值域20/40/60/80、命中累加/分档/死亡80与逐 tick
递减，对应 Unity Fall；HitStop 是另一条生命周期/显示计时链，不能继续占用该字段。

## 允许文件

- `Tools/NTSD28Parity/EntityFieldContract.cs`
- `Assets/NTSD/Scripts/Simulation/Diagnostics/NTSD28UnityEntityRawCapture.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28UnityEntityRawCaptureEditorTests.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditorTests.cs`
- 本 Task/Change、Ledger、STATE、handoff、总表

## 不变量与验收

- 只改 diagnostic；不改 Fall/HitStop/HitStateCount production 逻辑。
- maturity 31/9/7→32/8/7；当前真实差异仍为9、equal仍为38，不能虚构差异减少。
- focused 必须以 Fall=60、HitStop=4 证明输出60而非4。
- 工具build0/0、21/21、5/5、format；Unity compile0、联合6/6；raw差异集合不变。
- Ledger validator与scoped diff check通过。

## 回滚

恢复旧 candidate 文本、projection `HitStop` 与31/9/7；不触碰production hit链。

## 实际结果

- contract/projection 已改为 `Fall` VERIFIED，maturity32/8/7；contract SHA
  `D28F1BBB8B8325A488F5D4AF6D3DD32CE36F58A60B92149E3AC3E7FB838632E0`。
- 工具build0/0、21/21、5/5、format PASS；Unity fresh compile0；job
  `42b910d17b5a47fd8aa2b4614aaa12fb` 6/6 PASS，Fall60/HitStop4输出60。
- Unity raw SHA `E0C56A7BD89D7A24001A37E0D5D2F1CAF045663D399C5B07F7309B4CD381C055`；
  neutral真实差异如预期保持9、equal38、occurrence54，首差异baseMaxMp。
- production Fall/HitStop/HitStateCount、DAT、资源与Scene未改。
- Change Ledger validator 80 records / 14 governed code files PASS；scoped diff check PASS。
