# R7-FRAME-01 — FrameTick / recovery SoA conditional certification

> 日期：2026-08-22  
> 状态：`RUNTIME_PENDING / NO-CODE CERTIFICATION`

## Goal

在不改变C++行为合同或既有Unity优化边界的前提下，重新认证exact-character Recovery与FrameTick
data-oriented pass；只有发现新的source-confirmed difference才允许另建Change Record修改脚本。

## Scope

- C++ `regenerate_pre_collision_stats`与`frame_tick`顺序、gate、字段writer；
- Unity `BattleEcsCharacterRecoveryPass`、`BattleEcsCharacterFrameTickPass`；
- exact character/current type0 eligibility、legacy fallback与warmed 0 B；
- current DAT oid51/52 identity同步watch；
- current asset中type0 state2000可达性watch。

## Authority / Evidence

- C++ release `Makefile:11-35`；
- `src/entity/game_tick.cpp:140-173,577-584,687-692`；
- `src/entity/frame_advance.cpp:802-995`；
- `RESEARCH/R7-FRAME-01-frame-tick-recovery-soa-recertification-preflight-20260822.md`；
- focused Unity job `7b5d94953fca4cdb8947aaa2350277ca`。

## Files inspected

- `Assets/NTSD/Scripts/Simulation/Ecs/BattleEcsCharacterRecoveryPass.cs`；
- `Assets/NTSD/Scripts/Simulation/Ecs/BattleEcsCharacterFrameTickPass.cs`；
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs`；
- `Assets/NTSD/Scripts/Simulation/SimulationWorld.Passes.partial.cs`；
- `Assets/NTSD/Scripts/Test/Editor/FrameAdvanceRuntimeSnapshotEditorTests.cs`；
- `Assets/NTSD/Config/data.txt`及current DAT assets。

## Result

- 未发现除既有`D-MOV-005`外的新source-confirmed difference；
- current asset中state2000只出现于OID150/151/217/218 type2与OID124 type4，均不进入exact type0 pass；
- 已定位的exact-character identity writer同时写`ObjectId`与`FrameCache`，当前没有OID51/52 shell/current-DAT
  分离的production writer；该结论仍是`INFERRED safe`，不是runtime trace；
- focused EditMode 22/22 PASS，含Recovery/FrameTick data-oriented-vs-legacy、fallback及warmed 0 B；
- 20:41:14 fresh Unity assembly上的20:42:47 full self-check仍PASS；
- 本包未修改任何脚本，不创建Change Record。

## Unknowns

- mod/mutable DAT若引入type0 state2000，必须重开`D-MOV-005`；
- exact shell/current DAT identity未来若允许分离，Recovery必须改读current DAT OID；
- invalid DAT导致C++ jump flags跨调用残留的可恢复性仍`UNKNOWN`；
- C++ runtime trace与真实Play Mode未取得。

## Verification

- source mapping：完成；
- focused tests：22/22 PASS；
- warmed allocation fixtures：PASS；
- full self-check：20:42:47 PASS；
- Play Mode/C++ trace：PENDING/BLOCKED。

## Stop / reopen conditions

- type0 DAT出现state2000；
- exact eligibility扩展到非type0；
- `ObjectId`与current FrameCache identity出现production分离；
- 需要改Frame/Recovery writer、pass order、RNG、input或DAT时另建Task/Change Record。

## Out of scope

AI sensing/decision、FrameAdvance、broadphase、presentation、pool/capacity、R8 Play Mode、T8与C++ trace获取。

