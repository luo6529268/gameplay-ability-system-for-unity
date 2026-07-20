# Complete Frame/Input mapping audit (2026-07-18)

Read-only audit. Authority is `J:\\QQFile\\NTSD2.4\\ntsd_release_C#\\src`. Unity
MonoBehaviour/GameObject/Transform, renderer callbacks, pools and CLR shells are
allowed when they preserve authority ordering, runtime fields and observable
results. DAT representation, T8 default stage deployment and fixed-world camera
are excluded.

## Counts

| status | count |
|---|---:|
| equivalent | 39 |
| Unity-adapter | 181 |
| confirmed-difference | 4 |
| missing | 1 |
| authority-unresolved | 12 |
| **total authority IDs** | **237** |

The four differences are listed in the classification table below. The missing
contract is the `ReleaseTick` field listed in that table.
These statuses are static findings, not runtime-pass claims.

## Per-ID classification

Every authority ID is listed exactly once below. A grouped row means every ID
in that row has the same status and evidence.

### Tick, carrier and input

| ID(s) | status | Authority evidence | Unity evidence |
|---|---|---|---|
| `FLOW.01`, `FLOW.02` | equivalent | `BattleCore/Simulation/SimulationTickDriver.cs:25-116`, `StepOneTick`, `CanAdvanceTick` | `Assets/NTSD/Scripts/Simulation/SimulationTickDriver.cs:StepOneTickInternal/CanAdvanceTick`; provider/get, Empty-on-mismatch, apply, scheduler and after/checksum order closes. |
| `FLOW.03`, `FLOW.04` | Unity-adapter | `SimulationTickDriver.cs:93 ApplyFrameInput`; `Entity/CharacterLogic.cs:9 ApplyInput` | `SimulationWorld.ApplyFrameInputSet`, `LF2Entity.RunCharacterInputPhase`, `NTSDInputStateModule.UpdateFromBuffer`; event carrier and CLR shell are host adapters. |
| `FLOW.05` | confirmed-difference | `GameTick.cs:18 Run`; results-active path returns after results tick | `Assets/NTSD/Scripts/Simulation/NTSDBattleTickSystem.cs:17-29 RunReleaseTick` continues ordinary passes while results are active. |
| `FLOW.06`, `FLOW.07`, `FLOW.08`, `FLOW.10` | Unity-adapter | `GameTick.cs:48-134`; `FrameRuntimePasses.cs:12-29` | `NTSDBattleTickSystem.RunReleaseTick`, `SimulationWorld.Passes.partial.cs:RunFrameAdvancePhase/SerialTickAll`; same order, Unity-hosted pass boundary. |
| `FLOW.09` | confirmed-difference | `GameTick.cs:127-134,1513 RunLatePerEntityUpdatePass`; no second held logic sync | `SimulationWorld.Passes.partial.cs:LateEntityUpdateAll` calls `SyncHeldPoseAfterLateHolderFrameChange`, writing held frame/pose runtime fields after holder late frame change. |
| `CARRIER.01`, `CARRIER.02`, `CARRIER.03`, `CARRIER.04`, `CARRIER.05`, `CARRIER.06` | Unity-adapter | `Lockstep/SimulationPlayerInput.cs:5-20`, `SimulationFrameInput.cs:5-13`, `Input/NtsdInputState.cs:16`, `Input/SimInputBuffer.cs:11-29` | `Assets/NTSD/Scripts/Simulation/FrameInput.cs`, `SimulationWorld.FrameInput.partial.cs:ApplyFrameInputSet`, `Input/SimInputBuffer.cs`; flags/tick semantics retained, callback queue is host adaptation. |
| `IN.HUMAN.01`, `IN.CD.01` | Unity-adapter | `Input/InputRuntime.cs:609-629 PollHumanInput/TickInputCooldowns` | `Assets/NTSD/Scripts/Input/NTSDInputStateModule.cs:74 UpdateFromBuffer`; Roll -> held state -> cooldown -> edges is preserved. |
| `IN.CD.02` | confirmed-difference | `Runtime/NtsdEntityRuntime.cs:619 TickCooldowns`; authority AI path does not call input cooldown ticking | `NtsdEntityRuntime.TickDefendLockCooldown` from `SimulationWorld.Passes.partial.cs:VrestTickAll` decrements lock for all active entities, including AI. |
| `IN.EDGE.01`, `IN.EDGE.02R`, `IN.EDGE.02L`, `IN.EDGE.02U`, `IN.EDGE.02D`, `IN.EDGE.02A`, `IN.EDGE.02DEF`, `IN.EDGE.02J`, `IN.HIST.01`, `IN.HIST.02`, `IN.HIST.03`, `IN.JUMP.03` | equivalent | `NtsdEntityRuntime.cs:550-612`; `InputRuntime.cs:2563-2578`; `GameTick.cs:1676` | `Assets/NTSD/Scripts/Simulation/NtsdEntityRuntime.cs:ApplyInputEdges/PushInputHistory` and N30 pass; seven normal cooldowns clear on jump and `CdDefendLock` remains on both sides. |
| `IN.APPLY.00`, `IN.APPLY.01A`, `IN.APPLY.01D`, `IN.APPLY.01J`, `IN.APPLY.02`, `IN.APPLY.03`, `IN.APPLY.04`, `IN.APPLY.05`, `IN.APPLY.06`, `IN.APPLY.07`, `IN.COMBO.01`, `IN.COMBO.02`, `IN.COMBO.03`, `IN.COMBO.04`, `IN.COMBO.05`, `IN.COMBO.06`, `IN.JUMP.01`, `IN.JUMP.02` | Unity-adapter | `InputRuntime.cs:634-934 ApplyCharacterInput/RunCombo*/DoFrameJump` | `Assets/NTSD/Scripts/Input/NTSDInputStateModule.cs` and `Animation/LF2Objects/LF2Entity.cs:TryCharacterDatInputFrameJump`; combo order, early DJA return, costs, negative frames, 999 and PP flip retained through Unity DAT shell. |

### Movement and AI

| ID(s) | status | Authority evidence | Unity evidence |
|---|---|---|---|
| `MOVE.STAND.01`, `MOVE.STAND.02`, `MOVE.STAND.03`, `MOVE.WALK.01`, `MOVE.WALK.02`, `MOVE.RUN.01`, `MOVE.RUN.02`, `MOVE.RUN.03`, `MOVE.RUN.04`, `MOVE.JUMP.01`, `MOVE.DASH.01`, `MOVE.DASH.02`, `MOVE.HEAVY.01`, `MOVE.LAND215.01`, `MOVE.RECOVER.01`, `MOVE.VTAIL.01`, `MOVE.HASDIR.01` | Unity-adapter | `InputRuntime.cs:935-1515` standing/walk/run/jump/dash/heavy/landing/recover/velocity-tail entries | `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Character.cs` action resolvers and `LF2Entity` shared DAT helpers; runtime facing, velocity and frame writes retain the branch order. |
| `AI.PREP.00`, `AI.PREP.01`, `AI.PREP.02`, `AI.PREP.03`, `AI.PREP.04`, `AI.PREP.05`, `AI.PREP.06`, `AI.PREP.07`, `AI.PREP.08`, `AI.PREP.09`, `AI.PREP.10`, `AI.PREP.11`, `AI.PREP.12`, `AI.PREP.13`, `AI.PREP.14`, `AI.PREP.15`, `AI.PREP.16`, `AI.PREP.17`, `AI.PREP.18`, `AI.PREP.19` | Unity-adapter | `InputRuntime.cs:14-606 PrepareAiInputBasic` labelled branches | `Assets/NTSD/Scripts/Simulation/SimulationWorld.AiInput.partial.cs:PrepareAiInputBasic` and helpers; target/gate/RNG/key-write order is hosted in the Unity world partial. |
| `AI.TARGET.01`, `AI.COORD.01`, `AI.ROLL.01`, `AI.STATE.01`, `AI.DIST.01`, `AI.BETWEEN.01`, `AI.COORD.02`, `AI.S3000.01`, `AI.OID331916.01`, `AI.OID521221.01`, `AI.OID512187.01`, `AI.FIRST.01`, `AI.GUARD.01`, `AI.OID1.01`, `AI.OID1CLOSE.01`, `AI.OID4.01`, `AI.OID5.01`, `AI.SUBOID.01`, `AI.SUB.01`, `AI.PREWRITE.01`, `AI.PRESSURE.01`, `AI.HELD.01`, `AI.HELD.02`, `AI.HELD.03`, `AI.TEAM.01`, `AI.MOVEMODE.01`, `AI.NOTARGET.01`, `AI.SOUND.01` | Unity-adapter | `InputRuntime.cs:1518-606` helper definitions/caller order; fixed OID attempts at `478-494` | `Assets/NTSD/Scripts/Simulation/SimulationWorld.AiInput.partial.cs` corresponding helpers; stable target order, team/history gates, RNG and short-circuit writes are retained. |

### Frame, advance and physics

| ID(s) | status | Authority evidence | Unity evidence |
|---|---|---|---|
| `FT.TICK.00`, `FT.TICK.01`, `FT.TICK.02`, `FT.TICK.03`, `FT.TICK.04`, `FT.TICK.05`, `FT.TICK.06`, `FT.TICK.07`, `FT.TICK.08`, `FT.NEXT.01`, `FT.NEXT.02`, `FT.NEXT.03`, `FT.NEXT.04`, `FT.NEXT.05`, `FT.TAIL.01`, `FT.SOUND.01`, `FT.OP.00`, `FT.OP.01`, `FT.OP.02`, `FT.OP.03`, `FT.SPAWN.00`, `FT.SPAWN.01`, `FT.SPAWN.02`, `FT.SPAWN.03`, `FT.SPAWN.04`, `FT.SPAWN.05` | Unity-adapter | `Frame/FrameTick.cs` Tick/next/tail/sound/opoint/spawn entries | `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs:RunCommonFrameTick`, character/weapon/special/other `SimFrameTick`, opoint factory and `SimulationWorld.LateEntityUpdateAll`; object pool and CLR shells are adapters. |
| `FA.ADV.00`, `FA.VEL.01`, `FA.VEL.02` | Unity-adapter | `Frame/FrameAdvance.cs` advance and velocity entries | `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs:RunCurrentDatFrameLogicBeforeAdvance`, per-slot `SimTransit/SimTU`; interleaving preserves authority order. |
| `FL.ROOT.00`, `FL.TARGET.01`, `FL.CASE10`, `FL.CASE1`, `FL.CASE5`, `FL.CASE8`, `FL.CASE2_4_12_14.A`, `FL.CASE2_4_12_14.B`, `FL.NOTARGET.CATCH`, `FL.CASE11`, `FL.CASE6_9`, `FL.CASE13`, `FL.CASE3`, `FL.CASE7`, `FL.NOTARGET.DRIFT`, `FL.Z.01`, `FL.Z.02` | Unity-adapter | `Frame/FrameAdvance.cs` case and `ApplyFrameVelocity` branches | `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs:RunCurrentDatFrameLogicBeforeAdvance/RunHitFa8FrameLogic/RunHitFa6Or9FrameLogic/RunHitFa13FrameLogic`; same gates, RNG occurrence order and velocity writes. |
| `PH.ROOT.01`, `PH.X.01`, `PH.Z.01`, `PH.TYPE3.01`, `PH.FRIC.01`, `PH.FRIC.02`, `PH.BOOM.01`, `PH.Y.01`, `PH.GRAV.01`, `PH.AIR.01`, `PH.GROUND.00`, `PH.GROUND.CHAR13`, `PH.GROUND.SHURIKEN`, `PH.GROUND.FLY`, `PH.GROUND.BALL`, `PH.GROUND.999`, `PH.LAND.GENERIC`, `PH.SYNC.01`, `PH.WCOUNT.01`, `PH.SOUND.01` | Unity-adapter | `Frame/Physics.cs` X/Z/type3/friction/boom/Y/gravity/air/ground/landing/sync/weapon/sound entries | `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs` shared physics and character mechanics; weapon/special/other `SimTU`; Transform/render projection is downstream only. |

### Data, runtime and wrappers

| ID(s) | status | Authority evidence | Unity evidence |
|---|---|---|---|
| `DATA.FRAME.01`, `DATA.FRAME.02`, `DATA.OP.01`, `DATA.CP.01`, `DATA.CHAR.01` | Unity-adapter | `Data/DatModels.cs:45-185` frame/opoint/cpoint/character defaults and bounds | `Assets/NTSD/Scripts/DatParser` models/loaders and `LF2FrameData`; representation differs, default/index/field semantics are retained. |
| `DATA.CONST.01`, `DATA.CONST.02` | equivalent | `Common/NtsdConstants.cs:7-60` | `Assets/NTSD/Scripts/Define` and simulation constants use the same object/candidate/Dvx/gravity/type constants. |
| `RT.RESET.01`, `RT.COPY.01`, `RT.CLONE.01`, `RT.COPYENTITY.01`, `RT.APPLY.01`, `RT.IDENTITY.01`, `RT.TRANSFORM.01`, `RT.MOTION.01`, `RT.FRAME.01`, `RT.TRANSIENT.01`, `RT.TRANSIENT.02`, `RT.TRANSIENT.03`, `RT.STATS.01`, `RT.RESIDUAL.01`, `RT.RESIDUAL.02`, `RT.RESIDUAL.03`, `RT.RESIDUAL.04`, `RT.INPUT.01`, `RT.INPUT.02`, `RT.INPUT.03`, `RT.INPUT.04`, `RT.INPUT.05`, `RT.INPUT.06` | equivalent | `Runtime/NtsdEntityRuntime.cs:226-879` reset/copy/clone/input/transient/stats/residual contracts | `Assets/NTSD/Scripts/Simulation/NtsdEntityRuntime.cs` matching reset/copy/edge/cooldown and field methods; direct runtime storage and reset order match. |
| `RT.SYNC.01`, `RT.PRESENT.01`, `RT.ENTITY.01` | Unity-adapter | `Entity/CharacterSync.cs:12-317` and presentation bridge entries | `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs`, character sync and presentation arrays; CLR/renderer bridge is Unity-native. |
| `RT.CHECK.01` | confirmed-difference | `Entity/CharacterSync.cs:89-317` snapshot/hash covers all non-transient fields | `Assets/NTSD/Scripts/Simulation/BattleParitySnapshot.cs:364 ProjectEntityRuntime` hard-codes or aliases category/default slot, releaseTick, block bounds and renamed weapon/transform fields. |
| `RT.LINKS.01` | missing | `Runtime/NtsdEntityRuntime.cs:160-214`; weapon release writes current tick and hash includes it | Unity `NtsdEntityRuntime` has no `ReleaseTick` storage/write-back; snapshot remains `-1`. |
| `WRAP.AI.01`, `WRAP.AI.02`, `WRAP.INPUT.01`, `WRAP.FRAME.01`, `WRAP.FRAME.02`, `WRAP.FRAME.03`, `WRAP.FRAME.04`, `WRAP.PHYS.01`, `WRAP.DISPATCH.01`, `WRAP.CATEGORY.01` | Unity-adapter | Authority wrapper entries in `Input/AiInputRuntime.cs`, `InputRuntime.cs`, `Frame/*Runtime.cs`, `PhysicsRuntime.cs`, `Entity/*` | `Assets/NTSD/Scripts/Simulation` and `Animation/LF2Objects` forwarding methods invoke the same semantic implementations. |

### Explicitly unresolved

| ID(s) | status | Evidence |
|---|---|---|
| `UNRES.01`, `UNRES.02`, `UNRES.03`, `UNRES.04`, `UNRES.05` | authority-unresolved | Authority ledger section 11.1: unknown `Unk*` names, label-only helpers and source `mpDelta/value==550` branches; Unity preserves behavior without inventing semantics. |
| `DEP.INT.01`, `DEP.INT.02`, `DEP.INT.03`, `DEP.INT.04`, `DEP.WORLD.01`, `DEP.RNG.01`, `DEP.DATA.01` | authority-unresolved | Authority ledger section 11.2 delegates interaction/world/free-slot/RNG/DAT-loader ownership to adjacent ledgers; this report records the dependency and makes no silent parity claim. |

## Zero-omission check

The authority ledger's first-column extraction yields 237 IDs. The rows above
account for 39 equivalent + 181 Unity-adapter + 4 confirmed-difference + 1
missing + 12 authority-unresolved = 237. No authority ID is added, dropped or
silently merged into a prefix-only placeholder.

This completes static classification only. Compiler, BattleRuntimeSelfCheck and
Play Mode evidence remain separate acceptance gates.
