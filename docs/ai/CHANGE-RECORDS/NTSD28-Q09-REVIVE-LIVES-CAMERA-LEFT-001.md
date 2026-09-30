<!-- CHANGE-RECORD
id: NTSD28-Q09-REVIVE-LIVES-CAMERA-LEFT-001
status: FOCUSED_TEST_PASS
change-kind: BATTLE_PRESENTATION_BEHAVIOR
code-path: Assets/NTSD/Scripts/Simulation/Presentation/BattleEntityOverlayLayout.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattlePresentationCommandWriterEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: formal NTSD28 root EXE and paired playable render_snapshot.cpp revive-lives camera-left condition
evidence: PRECHANGE_SOURCE_FIRST_DIFFERENCE / GENERATED_EDITOR_BUILD_0_ERRORS / ORIGINAL_EDITOR_EXACT_1_OF_1_PASS / ADJACENT_2_OF_2_PASS / INHERITED_SELF_CHECK_FAIL_0_OF_1_THEN_RERUN_2_OF_2_PASS / NATURAL_GPU_PENDING
-->

# NTSD28-Q09-REVIVE-LIVES-CAMERA-LEFT-001

Created before script edits. Exact authority, existing Unity condition, scope, fixed-view coordinate mapping, acceptance, rollback and open exits are in the matching Task Contract. The pre-change first difference is a static condition mismatch: formal snapshot requires `camera.x <= entity.position.x + 14`; the shared Unity counter eligibility does not check a camera/view left edge. No runtime first-difference is claimed yet.

After editing, append exact symbols, tests, compile result, protected-state check and remaining natural/formal exits. Do not report P-09 or Q09 complete from the focused result.

Actual edit: `BattleEntityOverlayLayout.TryBuild` adds the view-left condition to the existing counter eligibility expression. It widens subtraction to avoid integer overflow and leaves labels independent. The existing `BattlePresentationCommandWriterEditorTests` gains `ReviveLivesCounter_UsesVisibleCameraLeftMarginWithoutHidingNameplate`: cameraX20, visibleLeft100, X105 hides the counter while keeping the label; X106 emits `x2` with first glyph X73 and keeps the label; mode gate3 still hides only the counter. No content, camera, Scene or gameplay coordinate changed.

Generated `dotnet build Assembly-CSharp-Editor.csproj --no-restore -nologo -v:q` exited 0 with 0 errors/230 warnings. Original Unity Editor 2022.3.62f3 PID11944 accepted `refresh_unity` and recompiled both runtime/Editor assemblies after source write. Exact EditMode job `b5f6f6c913e54a9c872156090ce1f018` passed 1/1; adjacent mode-gate and viewport-edge job `ce07cd0291aa4afa9c6d92ff1b78945d` passed 2/2. Battle/Menu/GameConfig/project-mode assets remain Git-clean with SHA-256 `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A`, `785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13`, `0527D737A1FA38FC56B51D00DC6E96A421D3C67222546368B147C2D074CB8EA7`, `B57CFEF32CC3ECE37AC98A4A1EF04FEB2FEE4EB3CC08466A4BFD32C1EDD85B82`. Real Battle GPU and formal same-viewport pixels were not run for this subgate; P-09/Q09 stay open. Ledger/diff audit is recorded separately after running it.

Additional pre-edit audit: inherited `OverlayLayoutSelfCheck_UsesFormalInclusiveRightEdge` job `d5ee9f7cc7a74d3f99916a89a6d8f9fb` failed 0/1 because its old X=-100 lives=2 assertion expects a negative counter glyph before the clamped Com label. Formal snapshot suppresses that counter at the camera-left gate; the label remains at X0. Only the old expectation in `CheckBattleEntityOverlayLayoutContracts` is added to this Change's declared code paths. Preserve this failure as test chronology; rerun after the correction.

Final focused correction: only that old self-check condition now expects three label glyphs, first at X0, with the offscreen counter absent; its separate right-edge assertion remains unchanged. Generated Editor build again exited 0 with 230 warnings. Original Editor refreshed/reloaded the updated runtime assembly and exact rerun job `5679e297af6848889349620bc9a3cc62` passed 2/2 (inherited self-check plus new boundary test). The prior adjacent mode/viewport job remained 2/2 without a production edit after it. Original Editor state after the job: Menu Scene, idle, not Play, not compiling, no test running. Final validation and Scene/Asset protection checks follow in the acceptance report; natural GPU and formal same-viewport remain open.

Final audit: `Tools/Validate-ChangeLedger.ps1 -RepositoryRoot <workspace>` exit 0, 971 records and five governed changed scripts all covered; `git -c core.safecrlf=false diff --check` exit 0. Battle/Menu/GameConfig/project-mode files are Git-clean and their final hashes match the values above. No Play/scene pixel acceptance is implied.
