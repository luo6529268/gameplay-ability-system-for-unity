<!-- CHANGE-RECORD
id: NTSD28-Q07-D024-HAN-MAPPED-BATTLE-PLAY-001
status: RUNTIME_PENDING
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q09HanEarthquakeBattlePlayProbeEditor.cs
authority: user D-024 unified spatial conversion; formal EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and playable candidate path
evidence: docs/ai/TASKS/NTSD28-Q07-D024-HAN-MAPPED-BATTLE-PLAY-001.md
-->

# NTSD28-Q07-D024-HAN-MAPPED-BATTLE-PLAY-001

Status: `RUNTIME_PENDING / SCOPED_CANDIDATE_MATCH / PROJECT_MAP_Z_BOUNDARY_OPEN`.

Original state: the existing original Battle Scene Han/Lee Play probe accepts source X520/580 but writes them directly as physical X and source X, so its start spacing is not mapped for the user-approved full-view ratio. The probe already observes the real full Driver first action146 in-collector and post-tick candidate branches, camera/map stability, ordered shutdown and Battle Scene disk hash.

Planned change and exact boundary: add one opt-in source-mapped start mode to the existing probe request. Only its start construction, report fields and consumed-request serialization change. Use the already-owned `world.SpatialProjection` for both X and Z and retain formal source carrier values. Do not add a new ratio, modify DAT, production, Scene, camera, map, formal source or nonbattle code. Preserve all old request behavior and existing diagnostics. Expected side effect is new no-overwrite near/far evidence only. Risk: mapped Z may meet a different project-map stage boundary; record actual stage bounds and fail honestly rather than alter map or stage rules.

Acceptance and rollback: see Task. Validation commands, actual edits, results and unverified items will be appended after script edit. Rollback is a narrow inverse of the optional probe branch only; historical artifacts are retained.

Actual script change (2026-09-29): added optional `sourceMappedPositions` to request and consumed-request schema; `CreateCharacter` now uses `world.SpatialProjection.SourceToViewX/Z` only when that flag is true, while `AppManager.SyncParticipantBirthPosition` retains the original source X/Z. The new report records scales, current stage bounds, and exact initial source/physical pairs, with a mapped-start assertion. Legacy requests default to false and retain their old positions and control flow. No production script, DAT, Scene, camera, map or nonbattle file was edited for this package. Compile, original Editor Play and protected hashes remain to be verified.

Follow-up script edit: added two dedicated mapped near/far request paths and a static path array, retaining all old request files. The mapped paths require the exact paired source X520/X580 and opt-in flag. The only edited script remains the declared Editor probe; no per-frame path-array allocation.

Validation: `dotnet build Assembly-CSharp-Editor.csproj --no-restore -nologo -clp:ErrorsOnly` exited 0 twice, 214 warnings/0 errors. Original Editor PID11944 recompiled via existing MCP bridge/Tundra with 0 errors; two unique original Battle Scene Play requests ran full Driver. Completed first action146 `HitCandidateCount` was near1/far0, matching the formal paired candidate count, and each ordered shutdown completed with borrower count0. Source starts and actual mapped positions match the World projection. Both actual Z≈1025.753 exceed the live project stage Z maximum760; `ClampStageZ` still compares source650 to the physical stage numbers, so an in-map natural acceptance remains open. This package does not prove an in-collector first-return branch; the callback's post-collection cache limit is preserved. Original Editor returned idle/nonPlay; five protected Scene/map/config hashes remained unchanged. Scoped `git diff --check` exited0. Exact machine reports/hashes and remaining boundary issue are in [acceptance](../../../artifacts/diagnostics/NTSD28-Q07-D024-HAN-CANDIDATE-BRANCH-001/MAPPED-NEAR-FAR-ACCEPTANCE-20260929.md). Ledger validator will be recorded separately after the current documentation updates.

Final audit for this package: `Tools/Validate-ChangeLedger.ps1 -RepositoryRoot $PWD.Path` exited0 with `Change ledger validation PASSED` (1019 Records, 51 governed code files in the shared dirty diff); scoped `git diff --check` exited0 after the documentation update. The validator's historical unmatched-path warnings did not fail validation. No full test suite or valid in-map same-source formal/Unity Play was run.
