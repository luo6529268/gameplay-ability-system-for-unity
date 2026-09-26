<!-- CHANGE-RECORD
id: NTSD28-Q09-PLATFORM-SHADOW-OFFSET-PUBLICATION-001
status: RUNTIME_PENDING
change-kind: BATTLE_PRESENTATION_BEHAVIOR
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Simulation/Presentation/BattlePresentationShadowBuild.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattlePresentationCommandWriterEditorTests.cs
authority: paired playable render_snapshot.cpp shadow screen_top/depth_order and battle_world.cpp platform offset producer
evidence: TEST_FIRST_COMPILE_RED_CS1061_CS1739 / ORIGINAL_EDITOR_COMPILE_0_ERROR / FOCUSED_LEGACY_CENTRAL_2_OF_2_PASS / ADJACENT_CLASS_6_OF_7_COMLABEL_REFLECTION_RED / SCENE_SHA_STABLE / LEDGER_842_1_PASS / DIFF_CHECK_PASS / PLAY_EXE_PENDING
-->

# NTSD28-Q09-PLATFORM-SHADOW-OFFSET-PUBLICATION-001

Before script modification, source producer and Unity runtime carrier are present, but Unity Legacy `UpdateShadow` uses `GetRenderZInt()` alone and central snapshot/command has no offset. Test-first scope is one shadow-only command displacement plus snapshot copy preservation and unaffected Z/order/foot anchor; production scope is exactly the three Task paths. Existing LF2Entity dirty boundary hunk near stage X must remain intact. No new manager, worker, pool, scene object or shutdown phase is introduced. Task contains authority, scope, acceptance, risk and rollback.

After the edits, record actual symbols, RED/GREEN focused results, compile/Play evidence, Scene/hash/governance checks, and remaining root EXE/visual limits. Do not infer nameplate or other Q09 item completion from shadow movement.

Actual production edit: `LF2Entity.UpdateShadow` adds existing runtime `RenderShadowOffset10C` only to legacy shadow center Y. `BattlePresentationEntitySnapshot` adds an optional offset value, captures it from `NTSDEntityRuntime`, and carries it through `WithResolvedSprite` and `WithPresentationBaseOrder`; `BuildCommands` uses it only for Shadow position while preserving Z/order and the original stableGroundPosition for foot-marker and health anchors. No new shutdown owner. Test file adds focused central command/copy and real Legacy SpriteRenderer checks.

Test-first Editor compile RED: two CS1061 missing property and one CS1739 missing constructor parameter. After production edit Editor compile0. Initial focused run failed because the diagnostic expected only one frame command while existing overlays emitted seven; the corrected baseline comparison passed 1/1 (`a098c0cbd4a940a4b936cb5f12daa22b`). Final two focused EditMode tests passed 2/2 (`f920d5a5ffd04565b54c23b561535668`). An adjacent class run completed 6/7 and failed in an unchanged com-label reflection fixture before command building; its six-parameter lookup does not match the production seven-parameter optional-bool constructor. Original Editor idle/non-Play and Menu/Battle Scene SHA stable. Report: `artifacts/diagnostics/NTSD28-Q09-PLATFORM-SHADOW-OFFSET-PUBLICATION-001/ACCEPTANCE-20260926.md`. Battle Play camera pixels, root EXE same-scene GPU and full Q09 remain pending; no SelfCheck or whole-suite claim.

Governance after the later direct Legacy test edit: `Tools/Validate-ChangeLedger.ps1` exited 0 with 842 Records and one then-current governed code file in diff (the production hunks had been committed separately in the shared worktree). `git diff --check` exited 0, and all three live progress documents were NUL-free. The two untracked B11 authority-content JSONL files observed in status were not touched by this package.
