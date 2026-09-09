# Task Contract — NTSD28-B2-NATIVE-INPUT-STATE-CARRIER-001

> 状态：`FOCUSED_TEST_PASS / CARRIER_READY / WRITERS_UNCONNECTED`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B2`  
> 建立日期：2026-09-03

## 目标

为每个 `NTSDEntityRuntime` 建立 NTSD 2.8 exact input-proxy 状态所有权：已验证的0x21-byte block，
以及 `input_proxy_counter_14c/source_slot_178/enabled_17c` 三个控制字段。闭合对象池reset、canonical
deep-copy、battle snapshot/restore和allocation-free lockstep checksum；本包不接production writer。

## 允许修改

- `Assets/NTSD/Scripts/Simulation/Input/NTSD28InputProxyBlock.cs`
- `Assets/NTSD/Scripts/Simulation/Core/NTSDEntityRuntime.cs`
- `Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleWorldEntityRuntimeSnapshot.cs`
- `Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleStateSnapshot.cs`
- `Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleLockstepChecksumModule.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28NativeInputStateCarrierEditorTests.cs`（新增）及meta
- `Assets/NTSD/Scripts/Test/Editor/BattleWorldEntityRuntimeSnapshotEditorTests.cs`（更新既有全字段反射守卫以识别exact block）
- 本 Task/Change/Ledger/STATE/handoff/总表

## 不变量

- exact block固定33 bytes；carrier不把pending/history/run加入proxy block。
- `source_slot` reset默认-1，counter/enabled默认0；`ResetInputState`清block，完整`Reset`再清控制字段。
- canonical copy必须复制值且不共享block数组；snapshot buffer预分配路径不增加每次capture分配。
- checksum覆盖三控制字段与33个byte，schema同步提升；不改旧Key/Prev/Cd/Combo字段。
- 不实现hit/control writer、counter递减、enabled cleanup、AI prepass或proxy copy调用；这些仍是后续包。

## 验收

1. test-first red只指向缺失runtime carrier/control字段；
2. distinct 33-byte block+3 control字段canonical copy精确且无alias；
3. ResetInputState与完整Reset后置条件精确；
4. battle entity-runtime snapshot/fresh restore保留全部状态；
5. runtime checksum对block与每个control字段敏感，warm capture/copy零分配不回归；
6. focused与snapshot/checksum/pooled reuse相关回归、full SelfCheck、compile/Console/Ledger/diff通过。

## 回滚

移除runtime carrier/control字段及copy/reset/checksum/schema接线并删除新测试；保留独立proxy block和前序B2包。

## 实施与验证结果

- test-first：Unity refresh仅产生46个预期`CS1061`，全部指向尚未存在的carrier/control成员。
- 实施：每个runtime拥有独立exact block与三个控制字段；input/full reset、canonical deep-copy、entity snapshot、
  aggregate snapshot schema和checksum schema均已闭合；未连接任何production writer。
- focused job `045f4d3c0c35488eadb2d5cdbf1c7a95`：5/5。
- related job `da0d9981526b4d5fbd09042fc87fab2b`：38/38；首次扩大回归准确发现并随后修复
  `BattleWorldEntityRuntimeSnapshotEditorTests`未分类新exact block的维护性守卫。
- pooled/zero-allocation job `25b807c5faa946229652fde96e470be7`：23/23。
- 完整`BattleRuntimeSelfCheck`：`Temp/NTSD_BattleRuntimeSelfCheck.result`于2026-09-03 06:34:20写入`PASS`；
  7条registration/rest负向夹具Error属于预期，清空后Console Error为0。
- 未验收：native combo writer、counter/source/enabled lifecycle、AI first pass与proxy/routing second pass、双端joint trace。
