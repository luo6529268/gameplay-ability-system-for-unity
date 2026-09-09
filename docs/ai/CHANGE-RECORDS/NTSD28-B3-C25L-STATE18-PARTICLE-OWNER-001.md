# NTSD28-B3-C25L-STATE18-PARTICLE-OWNER-001 — C25l state18 particle owner

<!-- CHANGE-RECORD
id: NTSD28-B3-C25L-STATE18-PARTICLE-OWNER-001
status: RUNTIME_PENDING
change-kind: TEST_FIRST_BEHAVIOR_ALIGNMENT
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Simulation/Passes/LateLifecycle/BattleLateEntityLifecycleModule.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C25LState18ParticleOwnerEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleEcsLateTailNoOpEditorTests.cs
authority: NTSD 2.8-Logan simulation_tick_driver.cpp C25k-l-m and battle_world.cpp materialize_state18_broken_weapon_particles; EXE B1E13AE1, closure 39DDDA15.
evidence: TEST-FIRST-CS0103-X4 / COMPILE0 / FOCUSED4 / RELATED63 / SELFCHECK-FLUSH-REGRESSION-CAUGHT-AND-FIXED / FINAL-FOCUSED18 / FINAL-NTSD28-BROAD447 / SELFCHECK-PASS-2026-09-05T03:44:32Z / SCENE-UNCHANGED / CONSOLE0 / LEDGER-PASS / RESOURCE-SPAWN-PLAY-PENDING
-->

> 状态：`RUNTIME_PENDING / PROGRAMMATIC_ORDER_VERIFIED / RESOURCE_SPAWN_PENDING`

## 改前事实

- state18/19与state13/200混在`SpawnLateTransitionEffects`，位于cleanup之后和Prev commit之前。
- sustained branch已有bound4选择与每粒四次随机，离开态已有7数量，但无global-delay seam且结构位置错误。
- virtual `RunLateTailBeforePrevFrame`有多个覆写，因此本包不移动整个tail或C25m。

## 计划

拆出C25l方法与纯decision kernel，在OPoint后立即调用并flush；mixed tail只保留state13/200 branch。先用缺失kernel/placement红灯，再实现。

## 实际证据

- `RunNativeC25State18BrokenWeaponParticles`已从mixed tail拆出，位于frame-zero OPoint后、cleanup和现有C25m commit前；state13/200 branch与virtual tail不移动。
- `BattleNativeState18ParticleKernel.ResolvePreRollCount`闭合非适用0、离开态7、持续态需roll(-1)和positive global-delay抑制；当前production formal caller保持delay-clear，动态producer留B8。
- 无OID999资源时持续态只消费selection roll，离开态不消费selection roll；资源/slot不足时不消费粒子tuple。
- test-first fixture先修正了自身`ulong`类型错误，随后留下4个纯`CS0103` missing-kernel红灯。
- 首轮focused job `189359ba1e5e45beb951305f01b90956` 4/4；related `fce6cf1bf07342f7a1abe3089c23972b` 63/63；首轮broad `33623704898748d39f3f53245f932702` 447/447。
- 首次SelfCheck真实捕获无任务也额外flush的回归；实现改为仅实际queued粒子时即时flush，没有修改旧SelfCheck断言。修复后job `9ae112a5e4c34d8689a3f156d91932ce` 18/18，SelfCheck于`2026-09-05T03:44:32Z` PASS，最终broad `cdd9d9b76081444eb0bf39813da3f44b` 447/447。
- clear后Console error 0；Scene SHA/length/mtime仍`0D74E174...D77 / 203477 / 2026-09-04T13:12:45.1526434Z`；diff-check无whitespace error；Ledger 206 Records / 188 governed code files PASS。
- 未做正式OID999资源存在时的7/1粒子、最低slot、四次tuple和同tick newborn Play验收，因此状态保持`RUNTIME_PENDING`。
