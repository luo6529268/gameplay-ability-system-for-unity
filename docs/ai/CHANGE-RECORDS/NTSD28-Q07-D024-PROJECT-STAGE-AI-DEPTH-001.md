<!-- CHANGE-RECORD
id: NTSD28-Q07-D024-PROJECT-STAGE-AI-DEPTH-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28SourceStageDepthEditorTests.cs
authority: formal root EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 playable native_ai.cpp and user D-024 proportional project-map exception
evidence: docs/ai/TASKS/NTSD28-Q07-D024-PROJECT-STAGE-AI-DEPTH-001.md
-->

# NTSD28-Q07-D024-PROJECT-STAGE-AI-DEPTH-001

Status: `VERIFIED / SCOPED_AI_STAGE_DEPTH_SUPPLY`.

Before: project Stage.ZMin/ZMax are physical237/760, source-history AI target uses `SourceRuleZInt`, and both legacy `SimulationWorld.StageZMin/Max` and SoA `CaptureAiDecisionWorldState` return raw physical numbers. Formal native AI compares integer source position to integer stage bounds with strict `<` and +10/+30/-30. The prior scoped Stage-Z/PreFrame production repair intentionally did not change AI.

Plan, narrow validation, risk and inverse-diff rollback are in the [Task](../TASKS/NTSD28-Q07-D024-PROJECT-STAGE-AI-DEPTH-001.md). Initial test must prove RED before production edit; source-complete and source-absent AI branches must be distinguished. No lifecycle, worker, snapshot schema, map or DAT change is planned.

Actual edits: a test-first assertion in `NTSD28SourceStageDepthEditorTests` distinguishes source-complete AI (expected 151/482 after project map inverse projection) from no-source AI (237/760), and checks legacy properties plus SoA decision world snapshot. `SimulationWorld.ResolveAiStageDepthBounds` is now the one production supplier: for source-history AI it uses the existing `TryGetStageRuleDepthBounds`/`BattleSpatialProjection` and `Math.Ceiling` to preserve native strict integer `<` thresholds; otherwise it returns the unchanged physical values. No decision branch, AI RNG/order, DAT, map, Scene, GameConfig or worker/snapshot/checksum schema changed.

Validation: generated Editor C# build 0 errors; original Editor Tundra success/0 C# errors. Test-first original Editor job `e2e7c9803fd7408cbba127c674060b51` RED expected151/actual237. After production fix the same test `bb2fab63a16345da84b66edbb1b496fe` passed1/1, and stage-depth plus adjacent AI kernel groups `2623a4ec7e1d40e8953c18b72d4c6407` passed30/30. [Evidence](../../../artifacts/diagnostics/NTSD28-Q07-D024-PROJECT-STAGE-AI-DEPTH-001/ACCEPTANCE-20260929.md). No full Battle Scene AI near/far decision Play was run, so this is not runtime-verified AI parity. Five protected SHA remained at the values in the preceding stage-clamp acceptance, targeted `git diff --check` exited0, and `Tools/Validate-ChangeLedger.ps1` passed with 1025 records/65 governed dirty code files. Rollback is the inverse diff of the exact two script hunks, preserving prior dirty work.

2026-09-29 superseding scoped Play evidence: the separate [AI stage-edge diagnostic](../../../artifacts/diagnostics/NTSD28-Q07-D024-AI-STAGE-EDGE-PLAY-001/ACCEPTANCE-20260929.md) ran two original Editor Menu→Battle full-Driver controlled decisions at source Z160/161 on the project map. Both reached AI target slot0 and selected down/up respectively with physical237/760 and source151/482 bounds. Ordered exit and zero borrowers twice. This closes **AI stage-boundary supply plus its near-edge Play exit only**; it does not prove all AI decisions, far-edge behavior, wave/state405 or Q07. Earlier “No full Battle Scene AI near/far decision Play” sentence is historical before this appended evidence.
