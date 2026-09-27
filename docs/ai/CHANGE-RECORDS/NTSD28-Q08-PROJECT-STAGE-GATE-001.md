<!-- CHANGE-RECORD
id: NTSD28-Q08-PROJECT-STAGE-GATE-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/App/ProjectBattleModeConfig.cs
code-path: Assets/NTSD/Scripts/Animation/LoganModeComboInput.cs
code-path: Assets/NTSD/Scripts/Simulation/Host/SimulationTickDriver.cs
code-path: Assets/NTSD/Scripts/Simulation/Runtime/BattleRuntimeState.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Passes/BattleEcsCharacterPreFrameBoundsPass.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSDEntityRuntime.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleWorldCoreScalarSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleStateSnapshotRestore.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleStateSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleLockstepChecksumModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleParitySnapshot.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28SourceCharacterStageXEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07ProjectBattleModeConfigEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleEcsCharacterPreFrameBoundsPassEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleWorldCoreScalarSnapshotEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleStateSnapshotRestoreEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleLockstepChecksumEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q08StageGateBattlePlayProbeEditor.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: formal root Logan EXE stage-gate trace and paired playable GameSession28/SimulationTickDriver28/BattleWorld28
evidence: docs/ai/TASKS/NTSD28-Q08-PROJECT-STAGE-GATE-001.md
-->

# NTSD28-Q08-PROJECT-STAGE-GATE-001

Before script edit: formal gate1 high-slot clone boundary is directly observed from the root EXE; Unity complete Driver still uses the gate-independent split. The existing project mode Asset is the approved configuration owner, but it lacks this gate. Exact sources, code/asset scope, expected effects, user exclusions, acceptance and rollback are in the Task Contract. Client-local runtime carrier is chosen to avoid modifying the external Server Kernel package. New mode value must be frozen before tick and included in identity, reset, snapshot/restore and checksum; an unversioned partial carrier is unacceptable. This record is `IN_PROGRESS`; no code or Asset change has yet been made for this ID.
Test-first RED: original Editor PID11944 refreshed and compiled in place, then MCP run_tests job 6fbb7d6526d7430c8bad3cb01fa36f94 failed all 15 selected cases on missing Asset stage-gate field or Client runtime scalar. Raw result: artifacts/diagnostics/NTSD28-Q08-G02-G03-STAGE-MODE-REACHABILITY-20260927/project-stage-gate-red-job.json. Production and Asset were untouched when this result was captured.
Diagnostic addendum before exporter edit: allow only the existing Q08 Naruto clone three-tick schema to override the per-world stage-gate scalar after project-mode publication. This gives a gate0 full-Driver negative control without changing the production Asset, DAT, scene, or any other scenario. The override must be explicit and nonnegative; ordinary captures continue to use the published Asset value. This is a test-only witness, not a production gate selector.

Actual implementation: `ProjectBattleModeConfig.cs` and its sole Resource Asset add selected gate1 to the frozen fingerprint; `LoganModeComboInput` includes it in project semantic identity. `SimulationTickDriver.BeginBattleAllocationSeal` publishes it once to Client-owned `BattleRuntimeState` before tick; the same host clears its publication marker on match reset and ordered World unbind, and marks the restored World after snapshot restore to preserve tick0 replay state. `NTSDEntityRuntime` owns the shared type0 X formula consumed by DataOriented/Legacy physical and source-rule writers. Core/aggregate/checksum versions advance 13→14, 29→30, 33→34; extended parity schema v4→v5 includes the scalar while frozen Authority400 v3 remains unchanged. No Server Kernel, DAT, Scene, or nonbattle path was edited for this ID.

Validation: original Editor RED job `6fbb7d6526d7430c8bad3cb01fa36f94` 15/15 expected missing-field failures; first GREEN `1cb830e5eb6c43d4843bd45677f10314` 26/26; adjacent D-024/D-025/mode job `2920fdbdd64a4869930d6a7eaf4dad1d` 18/18. Tick0 restore re-publication RED `4cb1954aa0764447a6be0b0299361126` expected3/actual1, then marker fix GREEN `26ac636f266a4761894961f8396f4523` 3/3. Original Editor full Driver Naruto clone gate1 3tick source/physical absent/-42/0 PASS; test-only gate0 override absent/-42/-49 PASS. Exact results and SHA are in `artifacts/diagnostics/NTSD28-Q08-G02-G03-STAGE-MODE-REACHABILITY-20260927/PROJECT-STAGE-GATE-PRODUCTION-ACCEPTANCE-20260927.md`.

Remaining: a targeted original Battle Scene Play job `20b6cc8dd0cf4094ae7b46c1bbb1c458` entered and exited Play, but MCP TestRunner job stayed `running` with 0 completed across the domain transition. After verifying the Editor was idle and out of Play, the documented `clear_stuck` operation marked this orphaned job `failed` with `Job cleared manually (stuck or orphaned)`; raw final job JSON is archived. This is no test PASS or demonstrated gameplay failure. The root EXE LFR recording/replay RNG mismatch still prevents full-state parity. Status is `RUNTIME_PENDING`, not a Q08 or BATCH-04 closure. Rollback is confined to declared paths and requires user approval under the repository rule; other dirty work is preserved.

Governance/format checks: `pwsh -NoProfile -File Tools/Validate-ChangeLedger.ps1 -RepositoryRoot I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity` exited 0 (`Change ledger validation PASSED`, 30 governed code files in the shared diff); the first Windows PowerShell invocation without an explicit repository root failed while evaluating its default `$PSScriptRoot` argument, so it is not counted as a validator failure of this change. `git diff --check` on this Change's declared code/Asset paths exited 0; Git emitted only working-copy LF→CRLF notices. Original Battle/Menu Scene hashes stayed at the pre-change values in the acceptance report.

Play witness addendum before new script: add one Editor-only request-file probe `NTSD28Q08StageGateBattlePlayProbeEditor.cs` and its Unity `.meta`, in the existing original Editor and clean saved `NTSD_Battle` scene. It must observe the published project gate1 after the real runtime reaches Running/tick>=2, write a unique result, request Play exit, and leave the Scene unchanged. This replaces the orphaned cross-domain TestRunner route; it does not exercise combat input, DAT values, menu, or nonbattle code. Result-file PASS only proves observation; external Editor idle/non-Play and Scene hash must be checked afterward before calling exit verified.

Final scoped Play result: the new probe compiled in the original Editor and wrote `PASS_CAPTURED` at real `NTSD_Battle` tick2, `observedGate=1`, `projectGate=1`, two active slots and formal catalog ready. It requested exit; an independent MCP Editor-state read then observed idle/non-Play and the original Battle scene still active. Battle/Menu Scene SHA-256 remained `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A` / `785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13`. Result artifact is `artifacts/diagnostics/NTSD28-Q08-G02-G03-STAGE-MODE-REACHABILITY-20260927/project-stage-gate-original-battle-play.json` SHA-256 `3EE62E2F1C1D50359032DA6B3BD5808FC91B9966FACFC74A50153332434BB080`. This closes only the selected project stage-gate publication/type0 boundary branch as `VERIFIED_SCOPED`; no full-random-state LFR parity, other mode selection, Q08 result branches, Q07 or BATCH-04 completion is claimed. The orphaned TestRunner job remains archived as failed infrastructure evidence, superseded for this Play gate by the independent probe.

Final focused self-check/validator: original Editor `BattleLockstepChecksumEditorTests.SourceRuleJsonProjectionRetainsExtendedSelfCheckContract` invoked `BattleRuntimeSelfCheck.CheckExtendedChecksumContracts` and passed 1/1 (job `fc6e057da3dd4b71bb939839280f6aaf`; raw JSON in the same diagnostic directory). After adding the Play probe, the original Editor rebuilt the Editor assembly and completed the real Play above. Final `pwsh -NoProfile -File Tools/Validate-ChangeLedger.ps1 -RepositoryRoot I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity` passed with 31 governed code files in the shared diff; scoped `git diff --check` exited 0. The test-only probe `.meta` was generated by this Editor import and no scene resource was saved.
