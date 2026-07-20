# Audit8 Open-Difference Re-audit

Date: 2026-07-18  
Scope: `FLOW.05`, `FLOW.09`, `IN.CD.02`, `RT.CHECK.01`, `RT.LINKS.01 / ReleaseTick` only.  
Authority: `J:\QQFile\NTSD2.4\ntsd_release_C#`.  
Method: read the current authority call chain and the current Unity production pass, then match storage, ownership, writer, and pass order. No source or documentation was changed by this audit.

## Result

| ID | Current classification | Runtime difference still reachable? | Required follow-up |
|---|---|---:|---|
| `FLOW.05` | Production pass gate is closed | No evidence of a production difference | Fresh self-check/Play Mode evidence should be kept with the gate test |
| `FLOW.09` | Closed; no late second held-logic sync | No | Keep the defer-to-next-tick focused test |
| `IN.CD.02` | Closed; cooldown ownership is human-input-only | No in the production driver | The direct results fixture must bind a controller/input buffer |
| `RT.CHECK.01` | Closed as a trace-tool projection; schema remains an adapter | No battle-runtime difference | Treat projection output as a validator, not as the authority runtime clone |
| `RT.LINKS.01 / ReleaseTick` | Storage, reset, copy, writers, and projection are present | No | Keep the five-path writer/reset matrix |

None of the five items is currently supported as a confirmed, reachable Unity battle-runtime difference. This does not close the separate authority-unresolved IDs in the broader frame/input ledger.

## FLOW.05 — Results-active early return

Authority `src/BattleCore/Simulation/GameTick.cs:45-50` increments the tick header first, then checks `world.Results.IsActive`; in that branch it invokes only the optional post-cooldown observation callback, runs `RunResultsTick`, and returns before cooldown/rest, character input, frame, interaction, stage, late, and tail passes.

Unity `Assets/NTSD/Scripts/Simulation/NTSDBattleTickSystem.cs:21-30` has the same results gate. The results branch calls `PostCooldownHumanInput(tickIndex)`, then `BattleResultsFlow()`, and returns. The ordinary sequence starts only at line 32 (`PostCooldownHumanInput`), line 33 (`RunFrameAdvancePhase`), and subsequent interaction/presentation methods.

The focused regression is `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs:11581-11639` (`CheckAudit7ResultsActiveGate`). It asserts header advancement and human observation, unchanged frame/position/HP/PP/rest/stage/tail carriers, and results-only progression. A direct call to `RunReleaseTick` without a bound shared controller can fail the human-observation assertion; that is a fixture precondition, not a production pass-order difference. The authoritative driver applies the frame input packet before stepping (`Assets/NTSD/Scripts/Simulation/SimulationTickDriver.cs:195-204`).

## FLOW.09 — Late held second synchronization

Authority performs `InteractionRuntimePasses.SyncHeldWeapons(world)` followed by exactly one `RunHeldWeaponStep12(world)` in `GameTick.Run` (`src/BattleCore/Simulation/GameTick.cs:112-115`). Its late per-entity pass (`src/BattleCore/Simulation/GameTick.cs:1513-1593`) advances the holder/frame and handles late cleanup/opoint; it does not invoke held-weapon logic a second time.

Unity's corresponding held logic is executed from `SimulationWorld.HeldObjectProcessAll` (`Assets/NTSD/Scripts/Simulation/SimulationWorld.QueryAndLinks.partial.cs:100-132`) during `NTSDBattleTickSystem.ProcessHeldObjects` (`NTSDBattleTickSystem.cs:103-106`). The only `RunWeaponSyncHeldStep10` call is in the pre-interaction pass (`SimulationWorld.Passes.partial.cs:1049-1060`). `LateEntityUpdateAll` (`SimulationWorld.Passes.partial.cs:740-815`) contains late frame/collision/opoint/tail handling but no second held-logic synchronization. Therefore a holder frame changed in late update is intentionally observed by held logic on the next tick; any same-tick renderer refresh is presentation-only.

`CheckLateHolderFrameChangeDefersHeldPose` (`BattleRuntimeSelfCheck.cs:2968-2972`) is the appropriate focused regression and covers both real and generic held shells.

## IN.CD.02 — `CdDefendLock` cooldown ownership

Authority human polling (`src/BattleCore/Input/InputRuntime.cs:609-622`) rolls input, applies the current held state, then calls `TickInputCooldowns` and input-edge processing. `NtsdEntityInputRuntime.TickCooldowns` (`src/BattleCore/Runtime/NtsdEntityRuntime.cs:619-637`) decrements the seven ordinary cooldowns plus `CdDefendLock`. AI input preparation does not call this human poll path.

Unity's `NTSDBattleTickSystem.TickCooldowns` (`NTSDBattleTickSystem.cs:86-89`) delegates to `SimulationWorld.VrestTickAll`, which only ticks interaction rest and attack-exempt cleanup (`SimulationWorld.Passes.partial.cs:936-944`). Human cooldowns are decremented in `NTSDInputStateModule.UpdateFromBuffer` (`Assets/NTSD/Scripts/Input/NTSDInputStateModule.cs:81-95`, including `DecrementCooldowns` at lines 162-172) through `LF2Character.RunHumanInputPollPhase` (`LF2Character.cs:754-760`). AI `CharacterInputAll` (`SimulationWorld.Passes.partial.cs:81-103`) calls character input/AI preparation but does not invoke the human input module, so it does not decrement `CdDefendLock`.

The production ownership is therefore aligned: human poll decrements once; AI does not. A results-active focused test must bind the human roster entity to a controller with an input buffer (`TryGetSharedInputControllerForSimulation`, `LF2Entity.cs:2387-2397`) or apply a `FrameInputSet` before calling the tick directly. Calling `RunReleaseTick` on an unbound fixture is not a valid proof of production ownership.

## RT.CHECK.01 — Parity snapshot projection

Authority's canonical snapshot is a direct runtime copy: `src/BattleCore/Entity/CharacterSync.cs:796-877` copies identity/category/owner, transform, links (including `ReleaseTick`), stats, input, presentation, and residual fields; the checksum includes those domains (`CharacterSync.cs:173-317`).

Unity's trace projection now reads the corresponding runtime fields:

- identity/category/owner/relation: `Assets/NTSD/Scripts/Simulation/BattleParitySnapshot.cs:385-397`;
- links and `releaseTick`: lines 435-450;
- stats/input/presentation: lines 460-517;
- block flags and `Unk318/31C/324/33C` aliases: lines 519-542;
- inactive raw-slot ARest/VRest preservation: `BattleParitySnapshot.cs` `ProjectARestDomain`/`ProjectVRestDomain` and `BattleRuntimeSelfCheck.cs:402-410`.

The focused projection assertions (`BattleRuntimeSelfCheck.cs:375-400`) set block, Unk, owner, relation, and release values and require them in JSON. They pass the intended validator contract. The remaining difference is intentional schema adaptation: Unity emits canonical trace JSON with default/reset-slot projections and aliases, while authority's `CharacterSync` snapshot is an internal runtime object. This is a static validation-tool boundary, not a battle-runtime semantic discrepancy. Do not use equal JSON shape as a requirement for runtime alignment.

## RT.LINKS.01 — `ReleaseTick` storage and writers

Authority stores and resets `ReleaseTick` in `NtsdEntityRuntime` (`src/BattleCore/Runtime/NtsdEntityRuntime.cs:166,184,203`) and writes the current game tick only in formal throw and consume release paths (`src/BattleCore/Interaction/WeaponRuntime.cs:287-303`). Damaged-drop and ordinary `DropWeapon` paths preserve the existing value, matching the authority contract.

Unity has the runtime field and reset (`Assets/NTSD/Scripts/Simulation/NTSDEntityRuntime.cs:34,342`). `LF2WeaponReleaseFlowResolver.ReleaseHeldWeaponRuntime` stamps the current world tick when requested and clears links (`Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponReleaseFlowResolver.cs:23-29`); consume uses the separate writer at lines 31-36. The held resolver requests stamping for formal throw/kind3 paths (`LF2WeaponHeldStateResolver.cs:94-95,404-409`) and leaves damaged-drop/character-drop preservation paths unstamped (`LF2WeaponHeldStateResolver.cs:99-129`). The trace projection serializes the field (`BattleParitySnapshot.cs:447`).

`CheckAudit7WeaponReleaseTickContracts` and `RunAudit7WeaponReleaseTickCase` (`BattleRuntimeSelfCheck.cs:2974-3099`) cover formal throw, kind3, consume, damaged drop, character drop, and pooled reset. This closes the previously missing storage/writer/hash contract.

## Recommended evidence refresh

1. Run `dotnet build Assembly-CSharp.csproj --no-restore /m:1` and require zero errors.
2. Run a fresh Unity `BattleRuntimeSelfCheck` after forcing compilation; preserve the source/DLL/result timestamps.
3. For `FLOW.05`/`IN.CD.02`, use a roster-bound human `SelfCheckController` and apply a `FrameInputSet` before the results-active tick. A direct unbound `RunReleaseTick` fixture is insufficient.
4. Keep `CheckLateHolderFrameChangeDefersHeldPose`, `CheckAudit7WeaponReleaseTickContracts`, and the parity projection assertions in the fresh result.

