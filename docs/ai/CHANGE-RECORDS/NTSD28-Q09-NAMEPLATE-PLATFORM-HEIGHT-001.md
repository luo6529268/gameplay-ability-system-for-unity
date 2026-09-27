<!-- CHANGE-RECORD
id: NTSD28-Q09-NAMEPLATE-PLATFORM-HEIGHT-001
status: FOCUSED_TEST_PASS
change-kind: BATTLE_PRESENTATION_BEHAVIOR
code-path: Assets/NTSD/Scripts/Simulation/Presentation/BattleEntityOverlayLayout.cs
code-path: Assets/NTSD/Scripts/Simulation/Presentation/BattlePresentationShadowBuild.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattlePresentationCommandWriterEditorTests.cs
authority: formal NTSD28 root EXE and paired playable render_snapshot.cpp nameplate screen_top plus platform operation30 writer
evidence: FORMAL_SOURCE_Z_PLUS_RENDER_SHADOW_OFFSET_PLUS_3 / ORIGINAL_BATTLE_SCENE_PLATFORM_OFFSET_NEGATIVE_FIVE / ORIGINAL_EDITOR_RED_1_OF_1 / GREEN_2_OF_2 / POST_ASSERTION_1_OF_1 / NATURAL_GPU_PENDING
-->

# NTSD28-Q09-NAMEPLATE-PLATFORM-HEIGHT-001

Created before the three declared script edits. Formal nameplate Y follows `Z + render_shadow_offset_10c + 3`; Unity currently captures offset for the platform shadow but omits it from the overlay slot, producing Z+3. The Task names current behavior, source/scene reachability, exact paths, exclusions, acceptance and rollback. No new manager, resource, pool, shutdown stage or schema is planned.

After edits append exact test-first result, symbols, compilation, governed validation, protected hashes and remaining scene/EXE visual gates. A focused command check is not a full P-10/Q09 exit.

Actual test-first RED: original Editor PID11944 refreshed/compiled the declared test, exact job `7a6d0e94264a4d4ab72ab78e91995520` failed at platform offset -5, expected world Y -4.90 / actual -4.95. Production edits then added optional `RenderShadowOffset10C` to `BattleEntityOverlayRuntimeSlot`, used it only in `ResolveLabelOrigin`, and passed `BattlePresentationEntitySnapshot.RenderShadowOffset10C` from `BattlePresentationShadowBuild.BuildCommands`. The test's reference helper passes the same value. Original Editor force-all refresh compiled; exact new and adjacent optimized-writer job `1d126bcdaac9409fa7f0e809f1794060` passed 2/2. A direct counter/label independence assertion was then added; after a second original Editor refresh exact job `7a34ca9a95424ab48e8e58fc4df39c16` passed 1/1. Generated Editor C# build exited 0 errors; console and governance final checks below. No Scene, DAT, asset, gameplay World or nonbattle change. Natural composed Game view and root EXE same-view evidence remain pending; P-10/Q09 open. Evidence: `artifacts/diagnostics/NTSD28-Q09-NAMEPLATE-PLATFORM-HEIGHT-001/ACCEPTANCE.md`.

Final checks: Change Ledger validator PASS, 924 records/33 governed code files; `git -c core.safecrlf=false diff --check` exit0. Battle/Menu Scene Git status clean and disk SHA-256 respectively `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A` and `785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13`, matching the previous protected values. Original Editor final state idle/non-Play/non-compiling/no test. One pair of parallel read-only MCP calls closed during framed receive; sequential retry of the same Editor succeeded. Console error filter showed MCP client-exit/disposed-object bridge entries only, no C# compiler errors; no test was rerun because this was observation transport noise.
