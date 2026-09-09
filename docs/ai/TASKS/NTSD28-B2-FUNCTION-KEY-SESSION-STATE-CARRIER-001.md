# Task Contract — NTSD28-B2-FUNCTION-KEY-SESSION-STATE-CARRIER-001

> 状态：`FOCUSED_TEST_PASS / SESSION-CARRIER-READY / SNAPSHOT-CHECKSUM-RESTORE-READY / PRODUCTION-UNCONNECTED / NEXT-PRODUCTION-INTEGRATION`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B2 / FUNCTION KEYS`  
> 建立日期：2026-09-04

## 目标

建立Authority等价的F3/F6～F9 Session状态、event mask/queue/fixed dispatch、accepted计数与pending handoff载体，
并把这些确定性字段纳入BattleRuntime reset、core/full snapshot restore和lockstep checksum；本包不读取physical key，
不执行F6～F9对实体/资源/对象的下游效果。

## Authority 与原状

- Authority：`native_function_keys.h`中的runtime state/session acceptance/event mask与
  `game_session.cpp:2197-2204,2444-2471,2632-2664,3200-3227`。
- 前置：`NTSD28-B2-FUNCTION-KEY-ROUTE-CONTRACT-001` pure route已focused ready，但production未接。
- Unity旧`InitStatsRequest/Mode2Request`不包含F3 lock、F6 hit-resource/count、accepted byte，也无法表达Authority F7
  与F8/F9共享pending语义；不得复用旧字段冒充新carrier。

## 允许修改

- 新建`Assets/NTSD/Scripts/Simulation/Input/NTSD28NativeFunctionKeySessionState.cs`。
- 修改`Assets/NTSD/Scripts/Simulation/Runtime/BattleRuntimeState.cs`接入root/reset。
- 修改`Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleWorldCoreScalarSnapshot.cs`、
  `BattleStateSnapshot.cs`、`BattleStateSnapshotRestore.cs`。
- 修改`Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleLockstepChecksumModule.cs`。
- 新建`Assets/NTSD/Scripts/Test/Editor/NTSD28NativeFunctionKeySessionStateEditorTests.cs`；最小扩展
  `BattleStateSnapshotRestoreEditorTests.cs`与两个旧schema expectation tests。
- 上述Unity生成`.meta`以及本Task/Record、Ledger、STATE、handoff、总表。

不得修改`SimulationTickDriver`、physical latch、B1 Host、GameConfig/asset、Input Actions、Scene/Prefab、DAT、Authority、
旧F7/F8/F9 effect、hit/resource/object/audio consumer或pass顺序。

## 实现合同

- `KnownEventMask=0xF4`；F3/F6/F7/F8/F9 mask分别`0x04/0x10/0x20/0x40/0x80`。
- Queue按bit折叠；每Dispatch先快照并清queued、清last accepted，再固定F3→F6→F7→F8→F9。
- Session gate顺序：none→battle→main→F3 one-way lock；其他命令再受lock；仅F8/F9受delay。
- F6 toggle+count；F7 count+pending bool；F8/F9各计数且共享pending command。same-window F8+F9必须F9胜出。
- reset默认lock0、hit-resource true、计数0、pending/bytes0；可显式传初始hit-resource值。
- snapshot/checksum逐字段，不以对象引用/enum hash替代；core/full/checksum schema随字段集递增并同步旧expectation。
- carrier纯内存、warm dispatch/capture/checksum不得新增每tick分配。

## 验收

- test-first missing-type/schema red；focused覆盖reset、mask/queue折叠、gate、F3 fixed-order lock、F6～F9副作用、
  same-window F9、consume handoff、snapshot/checksum/restore与warm zero-allocation。
- Unity compile 0；新focused全pass；snapshot/restore/checksum/ring与B1/B2 FunctionKey相关回归全pass。
- full SelfCheck PASS；Console 0 error；Change Ledger validator通过。
- 状态最多`FOCUSED_TEST_PASS / CARRIER_READY / PRODUCTION_UNCONNECTED`，不得宣称physical/effect/runtime parity。

## 回滚

删除新carrier/test，移除root/snapshot/restore/checksum字段并把core/full/checksum schema恢复到4/6/9；
回退两个旧schema expectation。production仍未接，无Scene/asset迁移。

## 完成证据

- test-first fresh compile：50条预期missing carrier/root/snapshot `CS0246/CS0103/CS1061`。
- carrier完成mask `0xF4`、bit折叠、固定F3→F6→F7→F8→F9 dispatch、Session gate、F3单向lock、F6～F9
  counts/pending与consume handoff；4096 warm dispatch/consume为0 allocation。
- BattleRuntime root/reset、core/full snapshot restore与checksum逐字段闭合；schema精确推进为core5/full7/checksum10。
- Unity scripts compile 0；新focused `9/9`（job `387b1a255c834421bc3d8ac29d14de45`）；最终router/carrier/
  old FunctionKey/B1 Host/snapshot/restore/checksum/ring/旧carrier schema联合 `65/65`
  （job `ed8980300085420fab8edfb17e10c509`）。
- full BattleRuntimeSelfCheck：2026-09-04 20:19:23 `PASS`；7条既有negative-path预期错误审阅后清空，Console 0。
- validator：`PASSED / Records 155 / governed code files 110`；physical/effects仍未接。
