<!-- CHANGE-RECORD
id: NTSD-BUTTON-MULTIPOINTER-001
status: RUNTIME_PENDING
change-kind: CODE
code-path: Assets/NTSD/Scripts/UI/NTSDButton.cs
authority: User explicit 2026-10-04 04:49 UTC any-held multi-pointer and abnormal-release request
evidence: docs/ai/FILE-OPERATIONS/NTSD-BUTTON-MULTIPOINTER-001/RECORD.md
-->
# NTSD-BUTTON-MULTIPOINTER-001
User authority: approved any-held pointerId policy, 2026-10-04 04:49 UTC. Before: one boolean, any up/exit clears; no explicit pause/unconditional focus cleanup. Scope ONLY NTSDButton production source. HashSet pointer membership; first/last edge; no reenter press. Clear membership and base state on disable/destroy/focus/pause/noninteractable transition. Preserve left-button filter and inherited Click/Submit. No combat sampling, DAT, Scene, fonts or menu changes.
Dependencies: installed uGUI1.0.0 Selectable uses private single isPointerDown; final member release calls base up, intermediate release does not. Disabled transition observes interactable/CanvasGroup changes. No update loop, worker, queue or persistent service; OnDisable/OnDestroy clear local managed state idempotently as existing renderer teardown proceeds, no new shutdown phase.
Acceptance: actual source isolated behavioral harness with uGUI substitutes, compile against real installed Unity API, final diff and ChangeLedger validation. Unity EventSystem/Play/real multitouch pending because Editor ownership not acquired. Risks: Unity message dispatch, pointer ID reuse cannot distinguish an old callback from a new gesture with same ID without an event-generation token; no invented click semantics. Rollback exact before-0.txt with later-change guard and audited restore. No irreversible action.

CODE_WRITTEN: HashSet membership and first/last edge; base up delayed until final tracked release, final exit also releases base down flag. Disable/destroy/InstantClearState/focus/pause clear set; Disabled visual transition clears held pointers. Click/Submit inherited. Validation pending.

Validation: dotnet run --project artifacts/diagnostics/NTSD-BUTTON-MULTIPOINTER-001/harness/Harness.csproj: 51 assertions PASS. Compiles actual changed NTSDButton source against dependency substitutes based on installed uGUI branches; does NOT prove Unity message dispatch. Covers both two-pointer release orders, exit with other held, unknown/duplicate callbacks, no reenter press, disable/destroy/focus/pause/interactable/clear dedupe and recovery, nonleft filtering, Click/Submit, and reentrant selection disabling.
dotnet build artifacts/diagnostics/NTSD-BUTTON-MULTIPOINTER-001/api-compile/Button.csproj --nologo: PASS, 0 warnings/0 errors, actual UnityEngine.CoreModule and installed UnityEngine.UI.dll. Scoped source API compilation, not a full project or Unity Editor compile.
Review: only NTSDButton production file edited relative to exact pre-task backup. Prior left-button filter/InstantClearState retained and extended. OnPointerClick/OnSubmit remain inherited. No per-frame polling, serialized fields or event signature changes. Runtime Input sampling/combos untouched. Real EventSystem, CanvasGroup propagation, actual focus/pause messages, visuals and physical device multitouch require controlled Play/device acceptance. No Editor ownership acquired or Scene operation requested. Destruction recovery in harness tests the state helper only; a destroyed Unity object itself is not reusable.
Late unknown/up/exit callbacks are harmless after clear. Once a pointerId is reused, an artificially reordered stale callback with the same ID has no generation token; this implementation relies on the EventSystem's per-pointer event ordering. Does not promise to identify indistinguishable recycled-ID events.

Final validation receipt: validator PASSED, exit 0. First in-process redirection ledger.txt is empty because validator writes Console output; full external pwsh capture is ledger-captured.txt. git diff --check on NTSDButton exit0. 2026-10-04T04:59:04.708065+00:00
