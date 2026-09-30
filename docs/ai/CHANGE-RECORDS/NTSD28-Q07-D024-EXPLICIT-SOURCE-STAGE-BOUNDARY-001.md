<!-- CHANGE-RECORD
id: NTSD28-Q07-D024-EXPLICIT-SOURCE-STAGE-BOUNDARY-001
status: FOCUSED_TEST_PASS
change-kind: CODE
code-path: Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28SourceStageDepthEditorTests.cs
authority: formal root EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 playable source stage limits and user D-024 full-view proportional battle display
evidence: docs/ai/TASKS/NTSD28-Q07-D024-EXPLICIT-SOURCE-STAGE-BOUNDARY-001.md
-->

# NTSD28-Q07-D024-EXPLICIT-SOURCE-STAGE-BOUNDARY-001

Status: `FOCUSED_TEST_PASS / NO_MAP_BOUNDARY_SUPPLIER_ONLY`.

Before: no-map `GameConfig` snapshot uses source-domain `ZMin/ZMax` 180/350. `SimulationWorld.TryGetStageRuleDepthBounds` returns those numbers for both source and view outputs even when `BattleSpatialProjection.DepthScale>1`. Missing-source fallback in Stage-Z/PreFrame consequently sees too-short physical bounds. Project-map physical snapshot has a separate, already verified branch.

Planned change: source output remains exact; view output uses the existing `world.SpatialProjection.SourceToViewZ` at the common anchor for the explicit-source branch. Add focused RED/GREEN assertions for source/view boundary outputs and missing-source physical clamp; retain project-map and ratio1 controls. Only two script files named above are owned. Expected side effect is a wider no-map physical fallback at configured full-view ratio; no source-rule value, random draw, pass order or map value changes.

Actual edits: two focused assertions in `NTSD28SourceStageDepthEditorTests` cover source/view outputs, missing-source physical fallback and ratio1 control. The original Editor exact two-test RED job `4ac090566cea46d38d9be776dfd5c346` executed 2 tests: the configured 1152/730 case failed expected 284.05479452054794 versus actual 180.0, while identity control passed. `SimulationWorld.TryGetStageRuleDepthBounds` now projects only the explicit-source branch's view limits through `SpatialProjection.SourceToViewZ`; source limits and the physical-origin project-map branch are untouched.

Generated `Assembly-CSharp-Editor.csproj` build exited0 with 0 errors and 22 existing assembly-reference warnings. Original Editor exact GREEN job `7c5f59ecdd644351b6d59e2965814cc5` passed19/19; an earlier class-regex run initialized 0 cases before timeout and is excluded from acceptance. The Battle Scene was loaded/clean with14 roots, Editor non-Play after testing. All five protected Battle/Menu Scene, map and configuration asset SHA matched recorded baselines. Targeted script/document `git diff --check` exited0; `Tools/Validate-ChangeLedger.ps1` exited0 (`PASSED`, 1027 records/66 governed dirty files). [Scoped evidence](../../../artifacts/diagnostics/NTSD28-Q07-D024-EXPLICIT-SOURCE-STAGE-BOUNDARY-001/ACCEPTANCE-20260929.md). Rollback by inverse patch of these exact hunks without restoring or cleaning other work. Stage-wave and mode-4 results reserve are separate conditional producers; Q07/BATCH-04 stays open.
