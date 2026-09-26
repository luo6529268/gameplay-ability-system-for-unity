<!-- CHANGE-RECORD
id: NTSD28-Q09-BPOINT-BLEED-SCENE-PIXEL-001
status: VERIFIED
change-kind: BATTLE_PRESENTATION_TEST
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q09BPointBleedScenePixelProbeEditor.cs
authority: formal NTSD28 root EXE and paired playable render_snapshot.cpp bpoint plus d3d11_renderer.cpp solid draw
evidence: ORIGINAL_EDITOR_COMPILE_0_ERRORS / PLAY_01_FIXTURE_HP_FAIL_RETAINED / PLAY_02_COLOR_PREDICATE_FALSE_NEGATIVE_RETAINED / PLAY_03_CONTROLLED_CENTRAL_PIXEL_PASS / INDEPENDENT_PNG_3_RED_PIXELS / CAMERA_CLEANUP_AND_FOUR_SHA_STABLE / LEGACY_EXE_PENDING
-->

# NTSD28-Q09-BPOINT-BLEED-SCENE-PIXEL-001

Created before script edit. Current central writer/resolver/mesh tests are green, but no original Battle Scene pixel verifies that the new mark is visible and ordered over the body. Only the new opt-in Editor probe and its `.meta` are in scope. It will use the current formal content without changing DAT values, production rules, cameras or Scenes. Expected effects are temporary Play-only fixture state and diagnostic PNG/JSON artifacts; no new runtime manager or shutdown owner. Acceptance, cleanup, risks and rollback are in the Task.

Actual script symbols, compile result, Play result, Scene hashes and governance checks must be appended after editing. Do not mark P-08/Q09 complete from this probe.

Actual script: one new opt-in Editor probe and `.meta`. `Prepare` loads formal Ita into a temporary production World slot; `ObserveHigh` verifies HP500 emits no mark; `ObserveLow` verifies current HP166 emits one 1x3 mark after body; `Capture` preserves original camera fields and writes high/low PNG; `ComparePixels` attributes red A/B changes inside the projected mark; `CleanupAndFinish` unregisters fixture, verifies counts and Scene SHA, restores pause, exits Play. First Play `-01` failed because probe wrote HP3 instead of HP, second `-02` falsely reported no pixel due an overstrict green-channel predicate; both raw failures retained and only the probe was corrected. Final `-03` `PASS_CONTROLLED_CENTRAL_PIXEL`, with independent PNG read confirming 3 red pixels at x1040/bottom-y603..605. Original Editor final compile 0 errors, non-Play idle; camera restored; World4→4, slots2→2, borrowers2→2, four protected hashes stable. Full report: `artifacts/diagnostics/NTSD28-Q09-BPOINT-BLEED-SCENE-PIXEL-001/ACCEPTANCE.md`. Legacy/natural/formal EXE remain open; no production/DAT/Scene/nonbattle code changed.

Final governance: `Tools/Validate-ChangeLedger.ps1` exit0, 861 Records and 21 governed code files in the current dirty worktree diff; `git diff --check` exit0. The three restored recovery documents were rechecked bytewise with no NUL; the authorized v3 replacement was already installed and was not repeated over newer addenda. Legacy ownership read-only audit is linked from the alignment table and does not change this scoped result.
