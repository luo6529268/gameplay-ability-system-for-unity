# Task Contract — NTSD28-B3-C25C-E-ENTITY-CARRIERS-001

> 状态：`VERIFIED / ENTITY-CARRIERS / SNAPSHOT-CHECKSUM-CLOSED / ALGORITHM-EXCLUDED`
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B3 C25c-e`
> 依赖：`NTSD28-B3-C25C-E-CURRENT-MP-BINDING-CORRECTION-001 / VERIFIED`

## 目标

新增C25c-e已经确认但Unity缺失的实体级原生状态载体，并完整接入构造默认、Reset、canonical deep copy、entity runtime snapshot、full snapshot schema、lockstep checksum与parity JSON；本包不执行resource/display算法。

## 字段合同

- resource/status：WeakTimer12C、MpRegenBonusTimer1A4、EffectiveMaxRegenDouble1A8、HpRegenDouble1AC、FullRestoreTimer1B0、OrdinaryCreditGate2F4(-1)。
- attribution/damage：IncomingDamageScale340(0)、ModeDamageScalePercent(100)、InputScoreTotal348、KnockoutCount358、CatchSourceSlot90(-1)、EnvironmentSourceSlot160(-1)、ImpactSourceSlot164(-1)。
- C25d：DisplayScore1F0/Step1F4、DisplayDamageTotal1F8/Step1FC、DisplayCurrentHp200/Step204、DisplayEffectiveMaxHp208/Step20C。

## 允许代码路径

- `Assets/NTSD/Scripts/Simulation/Core/NTSDEntityRuntime.cs`
- `Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleWorldEntityRuntimeSnapshot.cs`
- `Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleStateSnapshot.cs`
- `Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleLockstepChecksumModule.cs`
- `Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleParitySnapshot.cs`
- 新增`Assets/NTSD/Scripts/Test/Editor/NTSD28C25ResourceDisplayCarrierEditorTests.cs`及meta。
- 更新只断言schema版本的C23/C24、FunctionKey、Environment/Action carrier测试，以及reflection snapshot测试所需断言。
- 本Task/Record、Ledger、STATE、handoff、总表与C25c-e manifest。

## 不做

- 不实现C25c/d/e算法，不改旧recovery，不增加world resource rules。
- 不增加definition stats/bmp/frame schema，不改Config/DAT/Scene/Prefab/Authority。
- 不把CatchSource新carrier自动等同于现有CatcherSlot；producer留B5。
- 不把新增字段写回Transform/presentation或现有旧MP/PpDisplay字段。

## 验收

- test-first因字段不存在或schema旧值取得红灯。
- 所有字段默认值、Reset与TryCopyCanonicalStateTo准确；reflection全字段snapshot通过且0 allocation。
- 任一新增字段变化都会改变checksum；parity JSON明确输出独立nativeResourceDisplay组。
- nested entity/full/checksum schema分别升级5/9/12并更新所有精确断言。
- compile0、focused、snapshot/checksum相关组、NTSD28 broad、SelfCheck通过；无Play需求，Scene unchanged、Editor not playing、Console0。

## 回滚

删除新增字段与测试，恢复schema4/8/11及checksum/parity组；不得回退current_mp纠正或C25a-b。

## 实施与证据

- test-first在生产字段加入前取得100个预期missing-field编译错误；未用运行时假值绕过红灯。
- `NTSDEntityRuntime`已新增全部21个字段，并闭合field initializer、`ResetNativeResourceDisplayCarriers`、`Reset`与`TryCopyCanonicalStateTo`。
- entity runtime/full/checksum schema已分别升级为5/9/12；lockstep checksum逐字段写入，full parity JSON新增独立`nativeResourceDisplay`组。
- focused job `1c9acfafa03e4618a3f1852ecf7883e0`：4/4 PASS；snapshot/checksum/action/environment联合job `7fc4a6910983464bbbb6aab4ee647eb7`：29/29 PASS。
- 精确枚举57个`NTSD28*`测试类的broad job `602a215500bf4952904303fb36a9cd54`：405/405 PASS。
- Unity refresh/domain reload后compiler error=0；最终`BattleRuntimeSelfCheck`于2026-09-05 08:16:42 PASS；post-clear Console error=0。
- 本包未进入Play、未修改Scene/Prefab/Config/DAT/Authority；`NTSD_Battle.unity` SHA-256保持`0D74E174D37AF673CB717C699D13F3D115C65EDD332E373B6673757D5E323D77`。
- C25c/d/e算法、world rules、definition/frame内容schema、attribution producer与display step producer仍是后续工作，本包不构成C25c-e全行为对齐结论。
