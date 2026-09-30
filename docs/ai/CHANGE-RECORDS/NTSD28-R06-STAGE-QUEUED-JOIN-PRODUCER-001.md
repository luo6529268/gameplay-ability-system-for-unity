<!-- CHANGE-RECORD
id: NTSD28-R06-STAGE-QUEUED-JOIN-PRODUCER-001
status: FOCUSED_TEST_PASS
change-kind: CODE
code-path: Assets/NTSD/Scripts/Simulation/Stage/SimulationStageWaveModule.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06OrdinaryStageDisplayBirthEditorTests.cs
authority: formal root Logan EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and paired playable GameSession28 spawn_native_story_instance stage row join_100 to entity revive_next_hp_314, BattleWorld28 queued revival; alignment R06
evidence: docs/ai/TASKS/NTSD28-R06-STAGE-QUEUED-JOIN-PRODUCER-001.md
-->

# NTSD28-R06-STAGE-QUEUED-JOIN-PRODUCER-001

Before edit: formal stage rows have nonzero `join`, and the paired playable complete stage spawn explicitly writes `row.join_100` to `revive_next_hp_314`; Unity parses `BattleStageSpawnValue.Join` but no current stage birth exit writes it to `LF2Entity.RespawnCount`. The consumer's controlled B4 queued tests do not establish the stage producer. Exact affected symbols are `SimulationStageWaveModule.SpawnStageImmediateEntrySlot`, `NTSD28Q06OrdinaryStageDisplayBirthEditorTests` and its existing fixture helper. Expected side effect is only queued HP initialization for actual stage-spawned entities; default-zero stage rows retain zero. No DAT/Scene/config/nonbattle changes. Test-first RED, focused GREEN, compile, shutdown, four SHA and audit required; see Task for rollback and authority limits.

First code edit: `NTSD28Q06OrdinaryStageDisplayBirthEditorTests.ActualStageEntryPublishesJoinAsQueuedRevivalHpAndClearsItOnReuse` now invokes the actual stage entry, checks a nonzero Join=80 and then a freed/reused Join=0 entry. No production writer has changed yet. Original Editor compile and RED execution pending.

Original Editor RED evidence: Tundra rebuilt with no C# error, focused EditMode job `1b6c4e11d5f14f74a6567dbde008cbae` failed exactly at actual stage entry Join=80, entity `RespawnCount` expected 80 but was 0. Production edit now adds `entity.RespawnCount = spawn.Join` in `SimulationStageWaveModule.SpawnStageImmediateEntrySlot` after the shared stage vital contract and before runtime snapshot refresh. This covers both factory and direct fallback through the one common exit, leaving ordinary OPoint birth unchanged. Post-edit compile/GREEN/adjacent test/audit pending.

Superseding focused result: original Editor Tundra runtime/test compile succeeded with no C# error. First post-edit job `6a8eab1111964d0498745ae8343e3c56` failed to initialize before running tests; after fresh discovery, same-instance job `163f37b80e894db090d33c5793e63d80` passed the actual stage entry and freed/reused Join0 check `1/1`. Adjacent stage vital/display and reuse job `250bb1df49b34f13be079566d498e10a` passed `4/4`. The fixture's successful shutdown requires zero World objects/claimed slots/reference borrowers. Four protected Scene/config SHA-256 values match the prior baseline. Only the two declared C# files changed in this package; no DAT, Scene, mode/map data or nonbattle code. This is a conditional producer field gate, not a full queued revival runtime/Play/EXE result. Evidence: `artifacts/diagnostics/NTSD28-R06-STAGE-QUEUED-JOIN-PRODUCER-001/ACCEPTANCE.md`. Rollback remains only the two declared hunks, subject to repository approval rules.

Final audit: `pwsh -NoProfile -File Tools/Validate-ChangeLedger.ps1 -RepositoryRoot (Get-Location).Path` exit0 (990 records, 22 governed files in the shared dirty diff); `git -c core.safecrlf=false diff --check` exit0. Validator transcript is retained in the named artifact directory.
