# Task Contract — NTSD28-B2-INPUT-PHASE-CADENCE-001

> 状态：`FOCUSED_TEST_PASS / PHASE_CADENCE_READY / AI_PROXY_PENDING`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B2`  
> 建立日期：2026-09-03

## 目标

对齐 NTSD 2.8 human/replay input 的 1tu/2tu pending→current 采样：每 tick 都令 previous=current，
2tu 仅 phase0 更新 current、phase1 保留 current；1tu 每 tick强制phase0。AI producer仍每 tick处理，本包不改AI。

## 已确认映射

- 权威 `advance_input_update_phase_4a0b90`：default 2tu从0初值产生1/0；1tu恒0。
- Unity `AdvanceBattleFlowTick` 已在输入前把 `Runtime.Flow.InputPhase` 产生为1/0；可复用该标量，
  不另建重复phase counter。
- Unity `NTSDInputStateModule` 当前只有current/previous，buffer事件每tick直接改current；需新增private pending七键。
- FrameInputSet在tick执行前入buffer，随后BattleFlow推进phase，再进入human poll，顺序满足pending采样接线。

## 允许修改

- `Assets/NTSD/Scripts/App/MatchConfig.cs`
- `Assets/NTSD/Scripts/Input/NTSDInputStateModule.cs`
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Character.cs`
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs`
- `Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs`
- `Assets/NTSD/Scripts/Simulation/Host/SimulationTickDriver.cs`
- `Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleWorldCoreScalarSnapshot.cs`
- `Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleStateSnapshot.cs`
- `Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleStateSnapshotRestore.cs`
- `Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleLockstepChecksumModule.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28InputPhaseCadenceEditorTests.cs`（新增）及meta
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`（仅更新与新phase合同冲突的AI mutation夹具）
- 本 Task/Change/Ledger/STATE/handoff/总表

## 不变量

- default保持2tu；`MatchConfig.oneTuInput=false`不改变现有serialized match。
- pending是输入采样私有状态，不加入0x21-byte proxy；current/previous/cooldown/combo仍由既有runtime writer发布。
- cooldown与edge处理每tick执行；phase1 current不变，因此不产生虚假edge。
- oneTu模式位为Client world自有状态，不修改Server-owned scalar package；纳入snapshot/restore/checksum。
- 不改AI producer/RNG、proxy字段、input remap、recording、Scene/Input Actions、DAT/资源或B3+ pass。

## 验收

1. test-first red来自缺失pending phase overload/oneTu seam；
2. 2tu press在phase1只进pending、phase0才进current；release同理，previous每tick滚动；
3. 1tu phase恒0且每tick采样；reset回default 2tu；MatchConfig接线存在；
4. snapshot/restore/checksum覆盖oneTu和phase，warm allocation不回归；
5. focused及input/snapshot/checksum/lockstep回归、full SelfCheck、compile/Console/Ledger/diff通过。

已捕获的 full SelfCheck first difference：AI same-team mutation夹具手工设置phase1却要求human jump立即
采样。该夹具目标是AI summary invalidation，故应显式启用oneTu，而不能削弱production 2tu gate。

## 回滚

移除pending七键与phase overload、oneTu world/config/snapshot/checksum接线和新测试；恢复schema版本，不触碰
双RNG或其他包改动。

## 实际结果

- test-first：14个预期 `CS1739/CS1061/CS0117`，仅指向phase overload/oneTu seams。
- `NTSDInputStateModule`新增private pending七键；previous每tick滚动，只有`oneTu || phase0`把pending
  采入current。原三参数API保持每次采样兼容。
- default 2tu继续产生1/0；`MatchConfig.oneTuInput`显式开启1tu恒0。模式位纳入core snapshot/
  restore/runtime checksum；core schema4、aggregate3、checksum6。
- focused 5/5；final related job `73c995c5553842f881ab8b172736737f` 86/86 PASS。
- full SelfCheck first differences校正：AI summary synthetic mutation显式oneTu；默认2tu held-left在tick2
  首次edge=5、tick3=4、tick6=1且history只出现一次。最终SelfCheck PASS，compile/Console0。
- AI producer、0x21-byte proxy、recording freeze、remap和RNG consumer均未处理。
