<!-- CHANGE-RECORD
id: NTSD28-Q08-ORDINARY-REAL-MENU-RESULT-WITNESS-001
status: VERIFIED
change-kind: Q08_ORDINARY_REAL_MENU_RESULT_DIAGNOSTIC
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07MenuSceneCallbackPlayProbeEditor.cs
authority: formal NTSD2.8-Logan.exe B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and paired playable GameSession28 precombat transition plus parent ordinary result host
evidence: docs/ai/TASKS/NTSD28-Q08-ORDINARY-REAL-MENU-RESULT-WITNESS-001.md
-->

# NTSD28-Q08-ORDINARY-REAL-MENU-RESULT-WITNESS-001

Before script edit: the existing Editor-only Q07 Menu probe drives real Menu component callbacks, verifies formal publication and BattleRunning, then calls `AppManager.UnloadBattle` itself. It does not witness production `SimulationTickDriver.TryDispatchOrdinaryResultTransition` through `AppManager.TryReturnToCharacterSelectionFromBattleResult` on the real saved Scenes. The old request JSON is retained from a prior empty-root run and is protected user work. The parent Q08 route's temporary-scene evidence cannot certify the real Menu scene binding.

Intended after: an independent opt-in request shares that exact callback path but injects only canonical ordinary result phase3/timer350/transition2 after Battle is running, then observes production dispatch, ordered unload, existing character selection view, old driver retirement and zero pool borrowers. The Q07 path and its results remain unchanged. This Change owns only the named Editor diagnostic script; no battle production, DAT, picture, Scene, GameConfig, Menu, GAS or nonbattle behavior change. Task contains acceptance, limits and rollback. Current state `IN_PROGRESS`; compile/Play pending.

Code written: the Editor probe now selects a separate `Temp/NTSD28_Q08_OrdinaryRealMenuResult.request.json` only while requested or awaiting Scene restore, writes its result in this Change's diagnostic directory, and preserves the original Q07 request/result path. After the existing formal-content Menu callback chain reaches BattleRunning, the opt-in sets the canonical result fields and waits for the production host to unload Battle and activate the existing character selection. It records result injection, selection visibility, old-driver destruction and pool borrowers. The pre-existing Q07 branch remains the default. Original Editor compile and Play are pending.

Verified 2026-09-25: the original Editor ran unique request `q08-ordinary-real-menu-result-20260925-1` through real Menu callbacks and additive Battle Scene. It reported PASS: formal publication matched, BattleRunning World2, canonical result phase3/timer350/transition2 injected, production host returned to existing character selection, Battle unloaded, old Driver destroyed and pool borrowers0. Editor exited Play and restored saved Battle; Battle/Menu/GameConfig SHA-256 values remained unchanged. Recent Editor log had no C# compiler error; the one Console error was an MCP helper disconnect/disposed-object message, unrelated to battle behavior. Evidence: `artifacts/diagnostics/NTSD28-Q08-ORDINARY-REAL-MENU-RESULT-WITNESS-001/ACCEPTANCE-20260925.md` and raw JSON. Natural KO, formal EXE GUI and Player remain pending; this Change's diagnostic-only scope is verified. No production, DAT, image, Scene, Menu or nonbattle file was changed by this Change.

Post-edit checks: Change Ledger validator exited 0 with 817 Records and 18 governed code files covered in the current worktree diff; `git diff --check` exited 0. The focused real-Scene Play is the relevant runtime check. Broad EditMode/self-check suites were not rerun for this diagnostic-only extension.
