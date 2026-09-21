<!-- CHANGE-RECORD
id: NTSD28-Q08-F4-PLAYER-CLOSE-OWNER-001
status: VERIFIED
change-kind: BATTLE_FUNCTION_KEY_PLAYER_CLOSE_EFFECT
code-path: Assets/NTSD/Scripts/Simulation/Host/SimulationTickDriver.cs
code-path: Assets/NTSD/Scripts/App/AppManager.cs
code-path: Assets/NTSD/Scripts/Test/NTSD28Q08F4PlayerCloseProbe.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28NativeFunctionKeyRouterEditorTests.cs
authority: Formal Logan playable native_function_keys.h F4 leave_battle and main.cpp WM_CLOSE guarded close path
evidence: Focused Windows Player F4 close twice PASS with tick 3 to 3, Stopped, zero borrowers; second process exit 0; Editor physical PASS and router 8 of 8; ACCEPTANCE.md
-->

# NTSD28-Q08-F4-PLAYER-CLOSE-OWNER-001

Before: `SimulationTickDriver` latches a physical F4 leave request. Only Editor diagnostic code consumes it; no Player host performs the formally requested application close. `AppManager.OnApplicationQuit` runs ordered shutdown only when another owner already quits.

Planned change: Player driver update consumes the accepted one-shot before tick advancement and invokes a battle-only `AppManager` close owner. The owner first completes `TryShutdownBattleRuntimeBeforeSceneDestroy`; failure refuses application exit and leaves the runtime stopped/stopping. On success, `Application.Quit(0)` closes the Player. Editor compilation retains the diagnostic handoff without quitting. Existing `UnloadBattle()` remains the separate return-to-menu path.

Expected side effects: accepted Player F4 ends the application after zero-residual shutdown; no new tick begins after the F4 edge. No effect outside a Running battle. No new service, queue, worker or resource owner. The formal recording save-pending veto has no current Unity owner and is explicitly unverified.

Invariants: formal authority, 33 ms cadence, eleven-stage shutdown order, Unity/GAS framework, Menu and nonbattle behavior, current Scene/Build Settings, serialized content root and old resources remain unchanged. The existing Editor physical-key diagnostic remains usable. A shutdown failure must not be masked by exit.

Acceptance: fresh Unity compile with zero error; focused physical F4 and exact repeated/outside-battle F4 rejection coverage; real Windows Player accepted F4 process exit, shutdown report, stopped ticks and zero borrowers; no whole-character suite. Reuse the existing Q07 Windows Development Player build probe without editing it. Validate Change Ledger after code changes. Rollback is this Change ID's four script paths only, after recording the reason; never reset unrelated work.

Actual edits: `SimulationTickDriver.Update` consumes an accepted F4 only in Player and returns before automatic tick; `AppManager.TryCloseBattleApplicationFromNativeFunctionKey` calls the existing ordered shutdown then `Application.Quit(0)` on success; a new Development Player probe records actual F4/tick/quit state; the router test asserts exact repeated and outside-battle F4 rejection. Unity generated only the new probe's `.meta`. No Scene, Build Settings, resource, Menu or GAS file changed.

Validation: fresh Windows Mono Development Player build 0 error, two physical-F4 Player launches PASS, second process exit 0; both tick3→3, Stopped, borrowers0, pool quiesced. Existing Editor physical Play PASS. Router EditMode 8/8 after final test edit, production integration 11/11 before Editor-only test edit. Editor left Play/Scene clean/root14; protected Scene hashes unchanged; Change Ledger 654/19 PASS. Full scoped evidence: `artifacts/diagnostics/NTSD28-Q08-F4-PLAYER-CLOSE-OWNER-001/ACCEPTANCE.md`.

Remaining: Unity has no encoded recording save-pending owner, so formal persistence/veto is not implemented or claimed here. Failure-path shutdown and Q08 result/stage-count also remain open. Status `VERIFIED` applies only to accepted/rejected F4 routing and successful Player close with ordered shutdown.
