<!-- CHANGE-RECORD
id: NTSD28-Q09-P20-LOGAN-GRID-CAPACITY-001
status: FOCUSED_TEST_PASS
change-kind: CODE
code-path: Assets/NTSD/Scripts/Animation/Manager/CharacterAnimtorManager.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q09P20LoganGridCapacityEditorTests.cs
authority: formal Logan playable render_snapshot.cpp SpriteFrameResolver28::resolve 1104-1150 and official hid.dat file(62-126) frame430 pic119
evidence: docs/ai/TASKS/NTSD28-Q09-P20-LOGAN-GRID-CAPACITY-001.md
-->

# NTSD28-Q09-P20-LOGAN-GRID-CAPACITY-001

Created before script edits. Formal playable returns no sprite when a pic is within the first declared sheet range but beyond `rows * columns`; Unity's existing rect builder uses the range without this capacity guard. For official Hidan, pic119 lies inside `62-126` but its local index57 exceeds capacity55, while a second overlapping sheet cannot claim it. This is a source/content first-difference candidate; root-EXE pixels and natural Unity skill presentation have not been captured.

Expected effect: formal Logan PNG publication skips over-capacity cells for any entity; legacy BMP remains on its existing declared-range behavior. First-sheet ownership, existing within-capacity clipping, input, simulation and presentation ordering remain intact. Only the declared manager method/call sites and new focused Editor test may change. No DAT, image, Scene, config Asset or nonbattle edit.

Validation and rollback: see the Task. Record RED/GREEN, compile, original Editor result, Ledger/diff and protected hashes here after execution. Rollback only these hunks/new files after inspecting current dirty work. No test or runtime result claimed at this point.

2026-09-29 test-first RED: added the declared Editor test and unique `.meta` only; production still unchanged. Original Editor PID11944 was idle/non-Play. MCP `refresh_unity` scripts-only did not import the new asset; a subsequent force/all refresh imported this exact script, and Tundra completed with 0 C# errors. The precise one-test job `0780abf10c1246658da86de118cd21fd` failed as intended: declared-grid expected 55 rects, Unity returned 65. [Raw job](../../../artifacts/diagnostics/NTSD28-Q09-P20-LOGAN-GRID-CAPACITY-001/original-editor-red.json). This is a controlled rect-builder difference, not a natural Hidan Play/root-pixel witness. Next change only the declared manager method/call sites and add a legacy guard to this test.

2026-09-29 code written, validation pending: `BuildIndexedSpriteRects` now defaults to first-declared grid capacity. The formal Logan PNG loader uses that guard; non-Logan/BMP loading and catalog reconstruction explicitly retain prior declared-range indexing. The focused test now checks formal length55, legacy length65/image clip and first-sheet overlap ownership. No other manager path, DAT, PNG or Scene was edited. Original Editor GREEN, generated build, Ledger and protected-hash checks remain pending; this checkpoint is `CODE_WRITTEN` only.

2026-09-29 focused result: original Editor PID11944 recompiled production and Editor assemblies with Tundra 0 C# errors. Exact job `a2417ae9e2844655a5f03c789260c292` passed 4/4: formal Hidan capacity, old OID32 image-boundary behavior, first-owner overlap, and catalog reverse-staging control. Ledger validator passed and Git diff check exited 0. The original Battle Scene Editor returned idle/non-Play; Battle/Menu/GameConfig/ProjectBattleModeConfig disk SHA-256 values stayed identical. [Acceptance](../../../artifacts/diagnostics/NTSD28-Q09-P20-LOGAN-GRID-CAPACITY-001/ACCEPTANCE.md). This is `FOCUSED_TEST_PASS`, not a natural Hidan/root visual or full P-20/Q09 exit.
