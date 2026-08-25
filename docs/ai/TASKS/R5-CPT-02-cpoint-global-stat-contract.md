# R5-CPT-02 — CPoint global kill/damage stat contract

> 建立日期：2026-08-22  
> 状态：RUNTIME_PENDING — 最小 writer、focused matrix、Unity compile与full self-check均已通过；C++ trace / Play Mode待验。  
> 对应差异：D-CPT-002  
> Change ID：R5-CPT-002

## Goal

在 Unity current-frame held CPoint injury 的既有唯一 writer 内，恢复 C++ release
weapon.cpp 的 global kill/damage statistic side effect，同时保持所有已对齐的 CPoint phase、
injury 与 presentation 边界不变。

## Authority / Evidence

- C++ release build participation：J:/QQFile/NTSD2.4/ntsd_release/Makefile:20-21；
- C++ ordering：src/entity/game_tick.cpp:659-664 的 CPoint pass 后 weapon-sync；
- C++ writer：src/entity/weapon.cpp:42-75；
- C++ global array definition：src/entity/entity_collision.cpp:57-61；
- Unity mapping：BattleRuntimeState.cs:661-683、LF2Entity.cs:402-406、
  BattleCpointWriter.cs:147-202；
- phase precondition：R5-CPT-004 已使 SyncHeldCpoint 成为唯一 held injury owner。

## Scope

允许文件：

1. Assets/NTSD/Scripts/Simulation/Ecs/BattleCpointWriter.cs
2. Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs

允许符号：

- BattleCpointWriter.ApplyHeldInjury；
- existing CheckSharedDatCpointStep10StatsAndInputOrder 及同一区域的 focused test helper。

## Required behavior

1. positive injury 的 actualInjury 继续沿用既有 fall-damage integer scaling；
2. lethal 条件与现有 holder kill condition 相同；
3. valid victim.Unk344 仅为 1 或 2 时：
   - lethal branch 在 C++ 对应位置将 world.KillStats 增加 1；
   - positive injury 在 C++ 对应 combo 之后将 world.DamageStats 增加 actualInjury；
4. world stat 写入不得以 holder 存在为条件；
5. negative injury、already-attacking gate、invalid index 均不得写 global stats；
6. 不得修改既有 HP、HPBound、ComboCount、AttackingCounter、FrameDelay、holder score、CPoint raw-frame、
   held position 或 pass order 的逻辑。

## 已写实现

- lethal branch 在既有 active-holder kill score 后，按 valid index 1/2 guard 写 world.KillStats；
- positive injury 的 existing holder combo 后，按同一 valid index guard 写 world.DamageStats；
- world 写入独立于 holder query 结果，不新增 collection、allocation或 array capacity；
- existing shared-DAT lethal assertion现要求 world stats 增量；
- 新 focused matrix 覆盖 nonlethal、lethal no-holder、invalid 0/3、negative injury与 already-attacking。

## 实际验证

- UnityMCP scripts refresh 后 filtered C# compiler error：0；
- full BattleRuntimeSelfCheck：PASS，结果文件时间为 2026-08-22 09:44:35；
- focused matrix在 full self-check 内通过；
- ledger validator 与本包 scoped diff 在最终文档更新后已通过；
- C++ release trace / real Play Mode 仍未取得，最高状态维持 RUNTIME_PENDING。

## Verification

| 层级 | 验收 |
|---|---|
| S0 source | reread C++ ordered branch、all Unity ApplyHeldInjury callers、array/index mapping。 |
| S1 focused | lethal holder、lethal no-holder、nonlethal、index 0/3、negative、already-attacking matrix；锁定每个 global slot sentinel及既有 vitals/combos。 |
| S2 governance | Record、ledger、STATE、full diff、main plan、handoff 更新；validator/scoped diff PASS。 |
| S3 Unity | scripts refresh后的 C# error 0；full BattleRuntimeSelfCheck PASS。 |
| S4 honesty | 仅可提升至 RUNTIME_PENDING；C++ trace / real Play Mode待验。 |

## Stop conditions

- source 表明 stat writer 还需要改其他 production module、array capacity、pass order 或 type-specific hit path；
- focused fixture显示 R5-CPT-004 owner仍不唯一；
- 修复需要碰 D-CPT-003、held/link/opoint/input/collision/render/DAT/scene；
- Unity compile/self-check 失败且无法在列明两文件内最小修复；
- 需要修改、运行、构建或向 C++ authority 写入。

## Out of scope

C++ trace instrumentation、Unity trace/comparator、R2/R3/R4/R6+、server/lockstep、ECS redesign、
performance、Android、T8 default stage asset、physical input binding与完整 Play Mode认证。
