<!-- CHANGE-RECORD
id: NTSD28-Q09-P09-P10-PROJECT-MODE-OVERLAY-GATE-001
status: RUNTIME_PENDING
change-kind: CODE
code-path: Assets/NTSD/Scripts/App/ProjectBattleModeConfig.cs
code-path: Assets/NTSD/Scripts/Simulation/Runtime/BattleRuntimeDataCatalog.cs
code-path: Assets/NTSD/Scripts/Simulation/Presentation/BattlePresentationShadowBuild.cs
code-path: Assets/NTSD/Scripts/Simulation/Presentation/BattleEntityOverlayLayout.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07ProjectBattleModeConfigEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattlePresentationCommandWriterEditorTests.cs
authority: formal root NTSD2.8-Logan EXE and paired playable render_snapshot.cpp selected mode gate 54; user project mode Asset exception
evidence: docs/ai/TASKS/NTSD28-Q09-P09-P10-PROJECT-MODE-OVERLAY-GATE-001.md
-->

# NTSD28-Q09-P09-P10-PROJECT-MODE-OVERLAY-GATE-001

IN_PROGRESS before script edits. Before: project mode Asset and immutable snapshot carry no selected +0x54 gate; runtime catalog cannot publish it to a frozen presentation frame; overlay layout always emits otherwise-eligible lives/nameplate glyphs independent of mode. After intended: asset-owned gate 0 by default, fingerprinted immutable capture, frame copy/reset safe presentation carrier, formal gate predicates applied to lives and all shared nameplate paths. This is battle presentation only and does not import excluded mode DAT or change DAT numerics, gameplay state, ordinary HUD, Scene, camera, or nonbattle functions.

Declared file/symbol ownership and test, validation, rollback, and residual gates are in the Task. `BattlePresentationShadowBuild.cs` already has unrelated Q09 dirty hunks; edit only the named frame fields/capture/overlay call and preserve all other work. Formal mode-0 trace is evidence of selected default, not evidence of other mode production reachability. Actual compile, focused tests, Play, asset/Scene hashes and Ledger outcomes pending.

2026-09-28 CODE_WRITTEN: added `selectedModeReviveLivesGate54` as default-zero serialized project Asset field and immutable `ProjectBattleModeConfig.Snapshot` fingerprint V3; retained the selected snapshot in `BattleRuntimeDataCatalog` at existing prepare/identity boundary; captured its integer into `BattlePresentationFrame` with `CopyFrom`/`Reset`; passed it into `BattleEntityOverlayRuntimeSlot`; suppressed counter for 3/4 and all label paths for 2/4 and high slots at 1/3. Corrected bracket emission when a mode gate suppresses the underlying label. Two existing Editor test files gained three focused cases. No DAT value, excluded mode DAT, Scene, camera, menu, ordinary HUD or battle logic state was changed. Prior dirty Q09 earthquake/shadow hunks in the same presentation file were preserved.

Validation: initial generated `Assembly-CSharp-Editor.csproj` build failed with three test-only `RuntimeEntityHandle` int-to-uint errors, corrected in the same declared test; first log kept at `artifacts/diagnostics/NTSD28-Q09-P09-P10-MODE-BOUND-REACHABILITY-20260928/mode-gate-editor-build.log`. Second generated Editor build exited 0 with 0 errors/191 warnings, log `mode-gate-editor-build-02.log`. Original PID11944 Editor refreshed via existing local MCP `refresh_unity`, returned idle/non-Play after domain reload. Exact new EditMode job `9564e1a0443945159406c0b3cf68dbdb` passed 3/3: Asset snapshot/fingerprint, gate0..4 ordinary/high/composite command output, frozen copy/reset. Adjacent old job `12b918e6f3a54513a9761b5916104b83` passed 3/3: selected mode asset snapshot and platform/viewport nameplate cases. Full suite, SelfCheck, original Battle Play, Legacy and formal root same-view pixels not run for this scoped gate. Keep `RUNTIME_PENDING` and P-09/P-10/Q09 open.

Protected file SHA-256: Battle Scene `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A`, Menu Scene `785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13`, Input Actions `B5148EE08E8902C7DE41661BADC504363C97D9FA4A39DD985A2E828E98941A99`, GameConfig `0527D737A1FA38FC56B51D00DC6E96A421D3C67222546368B147C2D074CB8EA7`; all match pre-task baselines. Declared project mode Asset changed from `0AD22411DC42C899094A69F3D13BE9503E2524F6286B636E02C9DF95ABCD7061` to `12C077BAEFE31C2C26BEABD1FB50F70437F26EEB556BCCA5C2555563BD0858EF`, solely by `selectedModeReviveLivesGate54: 0`.

Final scoped governance: `Tools/Validate-ChangeLedger.ps1 -RepositoryRoot <repo>` exited 0, PASSED with 954 records/26 governed code files in the current shared diff; `git diff --check` exited 0. Full validator and diff outputs are saved beside the two build logs. Original Editor ended idle/non-Play and no test job running. MCP job result JSONs saved without overwrite as `mode-gate-new-tests.json` SHA `0381DA5C715FB7E4C5BA71FB84B7F27EC430F76046CCE303FF850F3B4701C676` and `mode-gate-adjacent-tests.json` SHA `38B15005F7BE13D0764E5050D8AA64D12089DDD7D72AA7C2E3758E54C3122FB4`. Scoped status remains `RUNTIME_PENDING / FOCUSED_TEST_PASS`; aggregate P-09/P-10/Q09 and master goal are open.

Final focused extension: the existing mode0..4 central command case now also tests high-slot special Com composite suppression, which follows a distinct optimized command branch. `Assembly-CSharp-Editor.csproj` build03 exited 0 errors/191 warnings. The original Editor was refreshed again and the single affected exact test job `eb488f85231e4ff1870647d740804be4` passed 1/1; its raw JSON is `mode-gate-special-com-final-test.json`. Earlier 3/3 and adjacent 3/3 remain valid; no full suite or Play was rerun. Validator/diff post-extension recheck follows.

Post-extension recheck: `Tools/Validate-ChangeLedger.ps1` PASSED again (954 records/26 governed code files in the shared diff) and `git diff --check` exited 0; final logs saved with `-final` suffix. Both Scene, Input Actions, GameConfig and declared mode Asset SHA values match those recorded above. No other protected asset or Scene was changed by the final test.
