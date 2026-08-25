# R5-CPT-02 — CPoint injury global-stat source preflight

> 日期：2026-08-22  
> 状态：PLANNED — source / Unity mapping 已闭合；尚未改脚本。  
> 对应差异：D-CPT-002  
> Change ID：R5-CPT-002  
> C++ authority：J:/QQFile/NTSD2.4/ntsd_release 的 src/entity/weapon.cpp:42-75、
> src/entity/entity_collision.cpp:57-61，由 Makefile:20-21 纳入 release build；
> 调用顺序见 src/entity/game_tick.cpp:659-664。

## 结论

在 R5-CPT-004 已将 held CPoint injury 的唯一 owner 收敛到 current-frame weapon-sync 后，
Unity BattleCpointWriter.ApplyHeldInjury 仍缺 C++ 同一 injury branch 的
world.KillStats / world.DamageStats 写入。这是可独立实施的静态差异。

本包只补这两个 world-stat side effect；不改变 injury、HP、HPBound、combo、frame delay、held position、
CPoint pass 顺序或任何其它战斗规则。

## C++ release contract

| 顺序 | C++ source | 已确认行为 |
|---|---|---|
| 1 | weapon.cpp:50-53 | 只有 injury_src 不为零且 attacker_special.attacking 为零才进入 held injury。正 injury 再按 fall_damage_div 计算整数 injury。 |
| 2 | weapon.cpp:54-59 | lethal 条件为 victim hp 大于零、injury 不小于 hp、kill_count 等于 -1。active holder 才获得 entity-local kill_stat 加一；随后独立地，vic.unk_344 为 1 或 2 时 g_kill_stats 加一。 |
| 3 | weapon.cpp:60-67 | 写 HP、hp_max、attacking、attacker/victim frame delay、victim combo、active holder combo。 |
| 4 | weapon.cpp:68-69 | 在 entity-local combo 后，独立地，vic.unk_344 为 1 或 2 时 g_damage_stats 加 injury。 |
| 5 | weapon.cpp:70-73 | negative injury 只治疗 HP/hp_max 并写 attacking；不写任一 global stat。 |
| 6 | entity_collision.cpp:57-61 | global arrays 长度为 4，source contract 的有效语义索引严格是 1/2。 |

关键边界：

- global stats 的写入不依赖 holder 存在；holder 不活跃或不存在时，entity-local holder score 跳过，
  但有效 unk_344 的 global stat 仍写；
- 非 lethal positive injury 不写 KillStats，但仍写 DamageStats；
- unk_344 为 0、3 或其他无效值不得写 Unity stat arrays；
- AttackingCounter 不为零时，整个 injury branch 不写 health、combo 或 global stats。

## Unity mapping / current gap

| C++ | Unity | 现状 |
|---|---|---|
| g_kill_stats 的 1/2 | SimulationWorld.KillStats 的 1/2 | BattleRuntimeState 固定分配 3-slot array；当前 ApplyHeldInjury 未写。 |
| g_damage_stats 的 1/2 | SimulationWorld.DamageStats 的 1/2 | 同上；当前 ApplyHeldInjury 未写。 |
| vic.unk_344 | LF2Entity.Unk344 | Unity 注释已定义 1..2 为 global stat slot。 |
| C++ weapon-sync owner | BattleCpointWriter.SyncHeldCpoint 到 ApplyHeldInjury | R5-CPT-004 后为唯一 current-frame held injury owner。 |

## 允许实施范围

- Assets/NTSD/Scripts/Simulation/Ecs/BattleCpointWriter.cs
  - 仅在 ApplyHeldInjury 的既有 positive-injury branch 中，按 C++ 相对顺序写入两个 stat array；
- Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
  - 更新 existing shared-DAT lethal assertion；
  - 新增或扩展 focused matrix，覆盖 nonlethal、lethal/no-holder、invalid index、negative injury、
    already-attacking no-op。

## 明确排除

- D-CPT-003 reciprocal mismatch control flow；
- CPoint relation/decrease/action/throw/dircontrol、kind2 validation、held/link、opoint、input、collision、
  render、DAT/scene、pool/capacity/performance；
- BattleDamageWriter 和 normal/special hit 的既有 stat writer；
- C++ source、build、executable、resource、configuration、runtime trace；
- pass order、field schema 或 stat array capacity 的长期架构变更。

## 验收合同

1. source reread：重新确认 C++ ordered branch、Unity call graph 与 writer scope；
2. focused self-check：
   - lethal holder：holder kill 加一，world kill 加一，holder/victim combo 加 injury，
     world damage 加 injury；
   - lethal no-holder：world kill/damage 仍写；
   - nonlethal valid slot：only world damage 加 injury；
   - unk_344 为 0/3：所有 global slots sentinel 保持；
   - negative injury / existing attacking：global slots sentinel 保持；
3. Unity scripts refresh 后 filtered C# compiler error 为 0；
4. full BattleRuntimeSelfCheck PASS；
5. Validate-ChangeLedger.ps1 与 scoped git diff --check PASS。

## 证据等级与未关闭项

- C++ source / build participation / Unity mapping：VERIFIED；
- C++ release runtime trace：BLOCKED，遵循 R1-WP02 boundary；
- real Unity Play Mode：PENDING；
- 本包完成后的最高状态：RUNTIME_PENDING，不得写为 C++ runtime 完整对齐。

