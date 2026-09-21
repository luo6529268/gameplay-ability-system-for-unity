# NTSD28-Q08-F4-PLAYER-CLOSE-OWNER-001

Status: VERIFIED_PLAYER_F4_CLOSE_SCOPE. Parent: BATCH-04 / Q08, R03 function-key effect return. Evidence: `artifacts/diagnostics/NTSD28-Q08-F4-PLAYER-CLOSE-OWNER-001/ACCEPTANCE.md`.

Authority: formal Logan playable `native_function_keys.h` routes a fresh battle F4 to `leave_battle`; `main.cpp` sends `WM_CLOSE`, retries pending encoded recording when present, then destroys the window and ends the application. The exact executable and playable-source identities are recorded in `docs/ai/CURRENT-AUTHORITY.md`. Unity currently captures the physical F4 edge but has no production consumer of its close request; see `artifacts/diagnostics/NTSD28-Q08-F4-CLOSE-OWNER-AUDIT-001/REPORT.md`.

Exact production scope: `Assets/NTSD/Scripts/Simulation/Host/SimulationTickDriver.cs` consumes the accepted F4 one-shot in the Player host update before another automatic tick. `Assets/NTSD/Scripts/App/AppManager.cs` owns the ordered runtime shutdown and application quit effect. One existing router Editor test gains exact F4 repeated/outside-battle rejection assertions; the new Player probe supplies close evidence. Editor keeps its existing diagnostic handoff and never quits the Editor. No Scene, Build Settings, resources, input bindings, menu flow, GAS or unrelated battle rule changes.

Verification scope: an accepted physical F4 in a real built Windows Development Player stops ticks, reaches `RuntimeMapCleared`/Stopped with zero active pool borrowers, and exits with process code 0. A nonbattle or rejected/repeated F4 must not request close. The existing Editor physical F4 probe remains valid. Add only the focused Player runtime probe, reusing the existing Q07 Development Player build probe without editing it; do not rerun all character scenarios.

Recording boundary: Unity currently has no corresponding encoded recording save-pending owner. This package must not invent one or claim the formal save-veto is verified. The close effect uses the existing battle shutdown contract; recording persistence is a separate Q08 dependency if/when implemented.

Risk and rollback: a close request during a pending worker tick must join before quit; failed shutdown must leave gameplay stopped and refuse quit. Restore only this Change ID's scoped additions after recording a failure, without resetting other work. Keep the formal source, Scene, GameConfig, old resources and current user changes intact.
