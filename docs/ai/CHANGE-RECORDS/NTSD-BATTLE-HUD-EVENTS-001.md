<!-- CHANGE-RECORD
id: NTSD-BATTLE-HUD-EVENTS-001
status: RUNTIME_PENDING
change-kind: CODE
code-path: Assets/NTSD/Scripts/UI/Battle/BattleHudView.cs
code-path: Assets/NTSD/Scripts/UI/Battle/BattleUiContracts.cs
code-path: Assets/NTSD/Scripts/Simulation/Host/SimulationTickDriver.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSDEntityRuntime.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs
code-path: Assets/NTSD/Scripts/Simulation/Runtime/SimulationRegistryModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Input/SimulationFrameInputModule.cs
code-path: Assets/NTSD/Scripts/App/AppManager.cs
code-path: Assets/NTSD/Scripts/Test/BattleTestBootstrap.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleHudViewEditorTests.cs
code-path: Assets/NTSD/Scripts/Simulation/Presentation/BattleHudChangeTracker.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleHudEventEditorTests.cs
authority: User explicitly requests data-driven HUD with existing MMEventManager and no Update polling
evidence: docs/ai/TASKS/NTSD-BATTLE-HUD-EVENTS-001.md
-->
# NTSD-BATTLE-HUD-EVENTS-001
Before: prior HUD polls cached values in LateUpdate; host scans roster and rereads vitals/name every presentation. Replaced by dirty-data producer and immutable MM event consumer. Battle authority remains current 336B44; presentation-only request does not authorize changes to vital values or tick order.
Design: per-world bounded (8 participant) tracker with runtime identity/generation and changed fields; HP/HPBound/HP3 setter retains existing VitalStore call, adds non-Unity notification; MP/MPMax become value-preserving properties with backing fields. Full initial state only after ready/roster binding, including same defaults. Existing control resolver reports binding transitions; unregister invalidates by generation. Host consumes pending data only at safe synchronous/main-thread publication boundaries. Main thread resolves name/head on binding change. MMEventManager subscribers receive immutable value payload, no runtime references. HUD OnEnable subscribes then reads last published event once, OnDisable unsubscribes/clears.
Ownership/shutdown: world owns tracker; no worker, queue or service created. Stage 1 closes ticking/input via existing host. Stage 5 after Join stops tracker and detaches runtime observers, clears publication and dispatches clear; later unregister is idempotent. No shutdown reorder. World reset detaches old observers. Input bind path never publishes Unity events from worker.
Acceptance: zero compile errors, focused initial/same-value/MP/HP/generation/pool/enable-disable/exit-reenter/thread tests, current Editor only if idle. No Scene/Prefab/asset writes. Rollback: verify baseline/last hashes and restore exact per-file backups under a recorded restore operation; no destructive Git.
Risks: MP field-to-property compatibility checked for ref/out/reflection callsites; serialized runtime copies remain explicit value reads/writes. No future unexecuted test request may remain armed.

CODE_WRITTEN: tracker and runtime property hooks implemented; production/App and direct Battle bootstrap ready hooks, input binding transition checks, unregister generation invalidation, main-thread MM event dispatch and HUD subscribe/sync/unsubscribe implemented. Runtime MP serialization names retained via FormerlySerializedAs. Compilation and focused tests pending.

First actual Unity Editor test run: 11 passed / 1 failed (EditMode preview fixture did not invoke Mono OnEnable). Fixed fixture invokes enable/disable explicitly; actual lifecycle covered separately by real Play. Round2 adds two-cycle Battle scene test and preserves request callback across domain reload via SessionState; backup runner-before-domain-persistence.txt. No production failure inferred from fixture assertion. Actual first Editor compiled new runtime/test sources successfully.

Final actual validation: original Unity Editor round2 completed 12 passing focused tests, 1 failed real-scene Play test. Failure: Unity Test Runner EditModeLauncher.OpenNewScene threw InvalidOperationException (cannot be used during play mode); no play-evidence or screenshot produced, so real HUD/shutdown/reentry acceptance remains unverified. No claim of real dedicated-worker validation; focused test checks worker records without UI dispatch and main-thread event delivery. Editor returned to NTSD_Menu. round2/editor-tests.txt closes request; no pending rearm.
Generated-project compile: compile-runtime-first.txt 0 errors/77 warnings; compile-tests-first.txt 0 errors/351 warnings; compile-round2.txt 0 errors/328 warnings. Actual Editor compiled changed sources and executed focused tests. Protected scene/image/font before-after hashes all unchanged (protected-after-editor.json).
Names: current indexed character catalog display name resolved at binding change using runtime ObjectId; scene existing characterNameText -> TMP PlayerName remains intact. No images, fonts, scene/prefab or DAT changed.
`nFinal format check: scoped git diff --check exits 0. Validate-ChangeLedger.ps1 completed; full receipt ledger-final.txt (existing repository historical warnings retained). Actual Editor Assembly-CSharp.dll timestamp 2026-10-03 21:45:38 UTC, Assembly-CSharp-Editor.dll 21:52:06 UTC.

Validation correction: Validate-ChangeLedger FAILED with one unrelated record error: NTSD28-336B44-Q09-P08-ROOT-UNITY-SAME-STATE-001 declares non-governed .cs.meta code-path. No HUD record error reported. Unrelated record preserved; whole-repository delivery gate remains blocked. HUD status RUNTIME_PENDING, not VERIFIED. Full receipt ledger-final.txt.

Follow-up review: artifacts/diagnostics/NTSD-BATTLE-HUD-EVENTS-001/followup-review.txt. Editor independently changed Menu -> Battle; source/ownership unknown, no Editor writes or new test launch. Static review completed; no new runtime PASS. All 12 script hashes unchanged since handoff.

User-authorized cleanup supersession: NTSD-BATTLE-HUD-VIEW-TEST-REMOVAL-001 removes BattleHudViewEditorTests.cs/meta and embedded runner; four prior passing tests now historical evidence, other event/play tests retained. Runtime status unchanged.
