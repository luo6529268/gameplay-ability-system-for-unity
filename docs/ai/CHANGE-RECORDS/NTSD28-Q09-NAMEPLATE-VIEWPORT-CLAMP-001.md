<!-- CHANGE-RECORD
id: NTSD28-Q09-NAMEPLATE-VIEWPORT-CLAMP-001
status: FOCUSED_TEST_PASS
change-kind: BATTLE_PRESENTATION_BEHAVIOR
code-path: Assets/NTSD/Scripts/Simulation/Presentation/BattleEntityOverlayLayout.cs
code-path: Assets/NTSD/Scripts/Simulation/Presentation/BattlePresentationShadowBuild.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattlePresentationCommandWriterEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: formal NTSD28 root EXE and paired playable render_snapshot.cpp camera-width nameplate clamp plus user fixed-camera exception
evidence: ORIGINAL_EDITOR_CAMERA_BOUNDARY_COMPONENTS / ORIGINAL_EDITOR_EXACT_COMMAND_RED_X4_02_VS_1_73 / ORIGINAL_EDITOR_MAIN_NEIGHBOR_2_OF_2 / FOCUSED_OVERLAY_SELFCHECK_1_OF_1 / NATURAL_SAME_TICK_PENDING
-->

# NTSD28-Q09-NAMEPLATE-VIEWPORT-CLAMP-001

Created before script edits. The formal nameplate clamp consumes the current camera width. Unity still clamps at legacy 794 despite the selected full-background viewport; at a controlled five-glyph X1000 case this shifts X978 to X749 while the original-Editor camera sees both. The Task contains the authority path, original Editor component evidence, exact affected symbols, risk, acceptance, exclusions and rollback. Existing dirty hunks in the declared paths are protected. Before production edits, the exact original Editor test job `0db923c5709f43c8940ee98ba4a5b29b` gave an executable RED (expected world X4.02, actual1.73; Y both -4.95). Raw job JSON is in the same Q09 viewport audit directory. Because formal right clipping uses `width - textWidth - 1`, the historical no-camera SelfCheck oracle also requires a one-pixel correction; its path was added to Task/Record before that script edit.

Expected side effects: only nameplate horizontal command placement near the actual visible edges; counter placement, label vertical platform offset, entity/World coordinates, presentation body/shadow, user HUD, camera and asset content stay unchanged. No new manager, queue, worker, renderer, resource, pool or shutdown stage is planned.

After implementation append the actual RED/GREEN jobs, code symbols, compile and focused checks, protected hashes, remaining natural Play/EXE limits and any deviations from the declared plan. Do not promote P-10/Q09 aggregate on a controlled command test.

2026-09-27 implementation and validation: `BattleEntityOverlayRuntimeSlot` now carries the visible logical X interval; `BattleEntityOverlayLayout.ResolveLabelOrigin` clamps to that interval with the formal inclusive right-edge pixel. `BattlePresentationShadowBuild.BuildCommands` obtains the interval once from the already captured viewport transform and active orthographic world camera, with the prior 0..794 interval retained only when no valid camera is bound. `BattleRuntimeSelfCheck.CheckBattleEntityOverlayLayoutContracts` has its one historical no-camera right-edge oracle corrected by one pixel. `BattlePresentationCommandWriterEditorTests` contains the controlled X1000, both-edge and counter-independence assertions plus a focused existing-layout SelfCheck caller. Existing dirty hunks in these files were preserved.

Original Editor job `0db923c5709f43c8940ee98ba4a5b29b` was the exact pre-fix RED (expected world X4.02, actual1.73). Post-fix job `e9886dec420446b99ee18f90d17af62a` succeeded with two selected tests completed and zero failures; the later fetched job object retained status/progress but not its summary payload after Editor reload. Post-fix job `c2a0b9afc6a8463d97e559f14da377ba` retained a full result: focused overlay SelfCheck 1/1 passed. Generated Editor C# build exited 0 with zero errors (22 existing assembly-reference warnings). `git -c core.safecrlf=false diff --check` and `Tools/Validate-ChangeLedger.ps1 -RepositoryRoot ...` both passed; the latter covered 925 Records and 33 governed code files in the current dirty diff (historical out-of-diff warnings remain). Original Editor returned idle/non-Play/non-compiling in `NTSD_Battle`. Battle/Menu Scene SHA-256 remained `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A` / `785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13`. The audit report holds the raw job files. No DAT, image, Scene, camera asset, battle logic position or nonbattle code changed for this package. This is a focused command-level result, not natural Play or formal EXE same-view parity; Q09/P-10 and the aggregate goal stay open.
