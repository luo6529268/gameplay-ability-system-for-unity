# Task Contract — NTSD28-B2-RNG-WORLD-STATE-001

> 状态：`FOCUSED_TEST_PASS / WORLD_STATE_READY / CONSUMERS_UNMIGRATED`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B2`  
> 建立日期：2026-09-03

## 目标

把已验证的 `NTSD28NativeRandom` 作为第二个、暂未被 gameplay 消费的 `SimulationWorld` 状态域接入，
闭合构造、reset、MatchConfig、lockstep bootstrap、core snapshot、跨 world restore 与 allocation-free
runtime checksum。先写 focused tests 并取得缺失 world/snapshot seam 的 red evidence。

## 允许修改

- `Assets/NTSD/Scripts/Simulation/Core/NTSD28NativeRandom.cs`
- `Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs`
- `Assets/NTSD/Scripts/Simulation/Host/SimulationTickDriver.cs`
- `Assets/NTSD/Scripts/Simulation/Lockstep/Session/InProcessBattleWorldBootstrap.cs`
- `Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleWorldCoreScalarSnapshot.cs`
- `Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleStateSnapshot.cs`
- `Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleStateSnapshotRestore.cs`
- `Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleLockstepChecksumModule.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28NativeRandomWorldStateEditorTests.cs`（新增）及 meta
- 本 Task/Change/Ledger/STATE/handoff/总表

## 设计与不变量

- 保留现有 Server-owned `DeterministicRng world.Rng` 与所有消费者，新增 `world.NativeRandom`；本包不迁移调用点。
- 默认/reset seed 保持当前 world adapter 的 `0x4E545344`；MatchConfig 与 lockstep barrier 使用正式 match seed。
- synchronized table 由 seed 唯一生成且运行中不变。core snapshot 用 allocation-free scalar state：CRT
  state/calls、sync counter/index/calls/lastCallSite 与 table hash，不每 tick 复制 3001 bytes。
- 跨 world restore 先按 snapshot match seed 重建表并校验 hash，再恢复可变标量；hash 不符 fail closed。
- checksum必须覆盖新双流全部 scalar + table hash；现有 legacy RNG 仍覆盖，schema version同步提升。
- 不改 AI、battle/stage/lifecycle consumer、B0 raw exporter/JSON parity schema、Server package、DAT、Scene或资源。

## 验收

1. test-first red只因 `world.NativeRandom`/snapshot scalar seam 尚不存在；
2. world构造/reset、lockstep bootstrap seed与独立 mirror精确相等；MatchConfig源码接线闭合；
3. core scalar capture包含双流且1024次warm capture零分配；
4. 同 world与fresh world restore均恢复双流，128次warm restore零分配；
5. runtime checksum能分别观察CRT和synchronized推进，256次warm capture零分配；
6. 新 focused、core snapshot、state restore、checksum、lockstep相关回归全过；compile/Console/Ledger/diff通过。

## 回滚

移除 `NativeRandom` world owner及四个生命周期接线，恢复snapshot/checksum schema与字段，删除新测试；
保留前一包独立 primitive，不触碰 legacy RNG 或其他工作树改动。

## 实际结果

- test-first red：缺失 `NTSD28NativeRandomScalarState`；随后新 suite 5/5 PASS。
- world新增 `NativeRandom`；constructor/reset=`0x4E545344`，MatchConfig/barrier=match seed。
- scalar snapshot包含CRT state/calls、tableSeed、sync counter/index/calls/lastCallSite/tableHash；
  core schema 2→3、aggregate snapshot 1→2、runtime checksum 4→5。
- 初次related job `bc379...b1fd0` 为38 pass/7 fail，准确暴露default world的match seed 0不能重建
  adapter default table；增加 `tableSeed` provenance后final job `9c40...1509` 50/50 PASS。
- 1024次core capture、128次warm restore、256次checksum均0 managed allocation；fresh world restore通过。
- 完整 `BattleRuntimeSelfCheck` 最终PASS；过程中发现并用三个独立test-only包修复旧验证seam。
- Unity compile 0 error；清理预期negative fixture日志后Console 0 error。consumer/B0 exporter/JSON trace未改。
