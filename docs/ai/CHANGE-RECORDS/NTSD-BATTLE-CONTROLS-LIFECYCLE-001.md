<!-- CHANGE-RECORD
id: NTSD-BATTLE-CONTROLS-LIFECYCLE-001
status: RUNTIME_PENDING
change-kind: CODE
code-path: Assets/NTSD/Scripts/UI/Battle/BattleControlsView.cs
authority: User authorization for disabled-view input rejection and release-local-state cleanup
evidence: docs/ai/FILE-OPERATIONS/NTSD-BATTLE-CONTROLS-LIFECYCLE-001/RECORD.md
-->
# NTSD-BATTLE-CONTROLS-LIFECYCLE-001
Before: Awake subscribes until Destroy; disabled View can receive live button presses. Failed sink release leaves IsPressed true.
After: OnEnable/OnDisable own idempotent subscriptions; OnDestroy fallback unsubscribes/releases. Press rejected when View disabled. Release clears local state before attempting old binding route. BindPlayer/UnbindPlayer still release before clearing/changing player binding; no delayed release queued to new player.
Scope: BattleControlsView only. Preserve action names, OR merge, tick buffer, external player assignment. No pointer policy, focus policy, scene wiring or joystick changes.
Acceptance: local regression harness for press/release/drag event, disable/re-enable/destroy and sink disappearance/rebind; independent compilation if available; static reference/format check and ledger. No claim of Unity Play. Shutdown uses existing View disable/destroy cleanup only, no new service/queue or shutdown reordering.
Rollback: exact verified before-0.txt, preserve newer edits.

Actual change: OnEnable subscribes via idempotent SetListening; OnDisable/OnDestroy unsubscribe then release. SetActionPressed rejects disabled View presses. False transition clears local IsPressed before checking binding/sink. Existing BindPlayer/UnbindPlayer release-before-clear order unchanged; no deferred release to new player. Only BattleControlsView source modified.
Validation: independent Assembly-CSharp.csproj MSBuild with BuildProjectReferences=false and artifact-only output/obj completed exit0 (compile-second.txt). Existing Library/ScriptAssemblies dependencies copied read-only into isolated build directory after initial missing-metadata compile failure; no Library outputs overwritten. Original Editor not refreshed or controlled. Temporary harness compiles actual View source with dependency stubs: 10/10 checks passed (harness-final.txt): idempotent subscribe, repeated press/release, disabled rejection, reenable, missing sink release, release-old-before-rebind, stale up suppression, new player press, unbind, repeated destroy. This is isolated C# logic verification, not Unity EventSystem/Play validation. Initial net8 harness restore failed due missing8.0.30 packs; net10 harness uses installed packs and succeeds, no downloaded packages. All failed receipts retained.
Validate-ChangeLedger PASSED,1225 records. Scene references remain empty and no external BindPlayer caller added. Multi-pointer/right-button/focus policy unchanged. Runtime remains pending; no persisted Assets tests or restoration of removed HUD tests.
