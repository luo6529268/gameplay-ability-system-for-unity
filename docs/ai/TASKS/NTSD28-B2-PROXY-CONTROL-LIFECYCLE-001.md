# Task Contract — NTSD28-B2-PROXY-CONTROL-LIFECYCLE-001

> 状态：`FOCUSED_TEST_PASS / CONTROL_CORE_READY / PRODUCTION_UNCONNECTED`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B2`  
> 建立日期：2026-09-03

## 目标

建立NTSD 2.8 input-proxy三个控制字段的exact lifecycle核心：confirmed status写counter、confirmed-hit
join/mimic tail启用source/enabled、FUN_0040D000 body内positive-HP递减，以及FUN_004503C5无条件
expiry cleanup。本包不提前接入尚未对齐的hit或global tail pass。

## 允许修改

- `Assets/NTSD/Scripts/Simulation/Input/NTSD28InputProxyControlLifecycle.cs`（新增）及meta
- `Assets/NTSD/Scripts/Test/Editor/NTSD28InputProxyControlLifecycleEditorTests.cs`（新增）及meta
- 本 Task/Change/Ledger/STATE/handoff/总表

## 不变量

- mimic counter只在上游encoded-status确认成功后按payload原值写入；本核心不自行消费RNG。
- 仅attacker/target均type0、counter>0且enabled!=1时，source写attacker slot且enabled写1。
- counter仅在native entity body未skip且HP>0时递减；同一次tail中递减到0后立即无条件清enabled。
- expiry只在enabled恰为1时清0，不清source，不清exact input block；dead/held body skip仍执行expiry cleanup。
- 不修改现有hit resolver、timer pass顺序、AI、proxy copy、DAT或Scene。

## 验收

test-first、所有gate/写入/冻结/expiry/source保留、enabled非1边界与4096次零分配通过；随后运行
carrier/combo相关回归、compile、SelfCheck、Console、Ledger与diff。

## 回滚

删除新增core/test及meta并移除本包治理记录；carrier字段保持不变。

## 实施与验证结果

- test-first：Unity仅产生14个预期`CS0103`，全部指向缺失的新lifecycle类型。
- 实施：confirmed payload写counter；type0/type0、positive counter、enabled!=1、有效attacker slot时一次性
  写source/enabled；body未skip且HP>0时递减；随后无条件执行`counter<=0 && enabled==1`清理。
- expiry保持source和exact block；enabled为非1值不被错误清理；dead/held freeze与already-expired cleanup均覆盖。
- focused job `f1b8e93cd3a34b2980555e6d27a0ffd1`：13/13。
- control/combo/proxy/carrier job `c86c3b07e6244f88bd9c479aa8eb4706`：44/44。
- 4096次lifecycle hot call零托管分配。
- 完整`BattleRuntimeSelfCheck`：2026-09-03 06:59:38 `PASS`；预期负向夹具清除后Console Error 0。
- 未验收：confirmed hit writer与B3 global reaction tail placement；本核心当前无production caller。
