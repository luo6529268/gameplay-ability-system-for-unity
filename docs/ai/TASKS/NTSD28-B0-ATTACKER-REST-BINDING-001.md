# Task Contract — NTSD28-B0-ATTACKER-REST-BINDING-001

> 状态：`FOCUSED_TEST_PASS / BUILD-0-0 / UNITY-COMPILE-0 / JOINT-6-OF-6 / REAL-BASELINE-UNCHANGED`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B0`  
> 建立日期：2026-09-03

## 目标

将 `combat.attackerRest` 从 candidate 晋级 VERIFIED，绑定保持为 `NTSDEntityRuntime.AttackExempt`。
两端均由 arest/vrest 规则写入，作为攻击者全局重复碰撞 gate；当前帧不能攻击时清零，并仅在停帧允许时递减。

## 允许文件

- `Tools/NTSD28Parity/EntityFieldContract.cs`
- `Assets/NTSD/Scripts/Simulation/Diagnostics/NTSD28UnityEntityRawCapture.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28UnityEntityRawCaptureEditorTests.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditorTests.cs`
- 本 Task/Change、Ledger、STATE、handoff、总表

## 不变量与验收

- 只改 maturity/测试；projection字段不换，production AttackExempt/ItrRest 不改。
- maturity32/8/7→33/7/7；neutral真实差异9/equal38/occurrence54保持不变。
- focused使用 AttackExempt=5，且与独立 ItrRest 状态不混淆。
- 工具build0/0、21/21、5/5、format；Unity compile0、联合6/6；Ledger/diff通过。

## 回滚

恢复 candidate maturity32/8/7；不触碰production collision/hit/rest。

## 实际结果

- contract晋级VERIFIED，maturity33/7/7；contract SHA
  `F0D6EA067FC14A3C7949AA8A5F3776F6260177623741AC04DA911E0053700FEA`。
- 工具build0/0、21/21、5/5、format PASS；Unity compile0；job
  `d43cf6e368094a6c81d2151055a9e787` 6/6 PASS，AttackExempt5投影通过。
- Unity raw SHA `B9CFC67D8429527609F7C1CD7954C35B7A6219E84D5A17D2C981565BC11A7E87`；
  neutral真实差异保持9/equal38/occurrence54，首差异baseMaxMp。
- production collision/hit/rest、DAT、资源与Scene未改。
- Change Ledger validator 81 records / 14 governed code files PASS；scoped diff check PASS。
