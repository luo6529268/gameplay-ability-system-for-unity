# Task Contract — NTSD28-B2-NATIVE-RNG-DIRECT-BATTLE-BOOTSTRAP-001

> 状态：`FOCUSED_TEST_PASS / DIRECT-BATTLE-RNG-CURSOR-READY / JOINT-FIRST-DIFFERENCE-CLOSED / AUDIO-SELECTION-B10`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B2 / NATIVE-DUAL-RNG`  
> 建立日期：2026-09-04

## 目标

让当前Unity直接进入战斗的fresh-world bootstrap与NTSD 2.8-Logan正式direct-battle初始化拥有相同native
RNG起点：先以match seed生成CRT与3000-byte synchronized table，再消费一次call-site `0x004021E0`的
Random BGM selection，使初始synchronized counter/index/calls为1、lastCallSite为`0x004021E0`。

只闭合当前direct-battle fresh bootstrap的RNG游标；不实现BGM音频选择、不实现完整selection/loading loop，
不改变snapshot restore/continuation或普通`ResetFromSeed`基元。

## Authority 与当前事实

- playable `GameSession28::reset()`创建fresh world后调用`random().reset_from_seed(config_.random_seed)`。
- `GameSession28::initialize()`在非selection-loading direct battle路径随后调用
  `resolve_native_bgm_selection()`；当前正式配置`bgm_selection_49f18c=0`且native BGM catalog非空，因此
  `synchronized_next(0x004021E0, catalogCount)`恰好一次。
- selection→battle continuation会恢复旧process-wide RNG，再按当次BGM策略处理；完整selection flow已由用户
  排除，不可把fresh direct bootstrap helper复用于snapshot restore或未来continuation。
- Unity `SimulationTickDriver.ApplyMatchConfig`与`InProcessBattleWorldBootstrap.PrepareWorldForHost`当前只
  `ResetFromSeed`；B2 joint raw首差为initial synchronized counter1/0。
- synchronized的counter/index/calls/lastCallSite推进与upperBound无关。本包用bound1只保存正确游标；实际
  BGM index/播放留B10，不能声称音频已对齐。

## 允许修改

- `Assets/NTSD/Scripts/Simulation/Core/NTSD28NativeRandom.cs`
- `Assets/NTSD/Scripts/Simulation/Host/SimulationTickDriver.cs`
- `Assets/NTSD/Scripts/Simulation/Lockstep/Session/InProcessBattleWorldBootstrap.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditor.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditorTests.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28NativeRandomWorldStateEditorTests.cs`
- 新增`Assets/NTSD/Scripts/Test/Editor/NTSD28NativeRandomBattleBootstrapEditorTests.cs`及`.meta`
- 本Task、Change Record、Ledger、STATE、handoff与总表

禁止修改selection/loading流程、音频播放/catalog、generic `ResetFromSeed`语义、snapshot restore、Config/DAT、
Scene/Prefab、ProjectSettings、Packages、authority或B0/B2 tool schema。

## 不变量

- fresh direct battle恰好消费一次0x004021E0；重复调用helper表示一次新的fresh initialization，不得隐式幂等。
- general RNG tests、manual seed/cursor tests与restore继续使用`ResetFromSeed`，不自动增加BGM call。
- fresh lockstep world与local MatchConfig host必须调用同一helper；diagnostic exporter也必须复用它，禁止各自
  手写advance。
- CRT table-build后不再调用CRT；authority expected CRT state/calls/table hash保持
  `1758127634/3000/A1BA1B90EA55796D`（seed682973786）。
- 当前helper不拥有BGM index；B10若需要实际选择，必须从正式catalog与同一draw语义恢复，不得再次推进world
  stream。

## 验收

- test-first因`ResetForDirectBattle`不存在出现精确CS1061；
- focused覆盖authority vector、repeat fresh reset、generic reset不消费、lockstep host connection和Unity B2
  exporter initial state；
- compile0，native RNG/world/bootstrap/lockstep相关测试与SelfCheck通过；
- 重跑input-common B2 raw，initial RNG必须相等，first difference下移到已知defend re-entry 3/0；
- Console0、Ledger/diff通过；无BGM声音或完整selection runtime声明。

## 回滚

恢复三个fresh-bootstrap caller使用`ResetFromSeed`，移除direct-battle helper与测试；generic RNG、snapshot和
诊断schema不回退。

## 当前证据

- test-first只改测试，Unity compile得到4个预期CS1061，全部指向缺失`ResetForDirectBattle`；production
  尚未改时已覆盖vector×1、repeat×2和lockstep expected×1。
- 已实现共享fresh direct-battle事务：纯`ResetFromSeed`后恰好调用一次
  `SynchronizedNext(0x004021E0, 1)`；local MatchConfig、in-process lockstep与diagnostic exporter统一接入。
- Unity compile0；bootstrap/native RNG/world/exporter 28/28 PASS
  （job `8f7109eca3fb4b2cbae0b0e968182245`），lockstep/in-process 17/17 PASS
  （job `5764a09a225e4836900fbff558bca1a7`）。
- input-common B2 joint重跑valid 3 ticks/6 entities；initial双RNG状态全equal，首差精确下移到completed
  tick2 slot1 `defendReentryCooldown` authority3/Unity0。17:38:34 SelfCheck PASS，预期7条error清除后
  Console0。
- 实际BGM index/音频与完整selection flow仍由B10/用户例外边界处理，不属于本包完成声明。
