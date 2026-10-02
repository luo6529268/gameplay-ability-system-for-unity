<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-D024-GRAB-VIEW-PROJECTION-001
status: RUNTIME_PENDING
change-kind: CODE
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleInteractionWriter.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B6CatchRelationExactFieldsProductionEditorTests.cs
authority: user D-024 ratio decision and root 336B44 C040 source-rule parity
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-D024-GRAB-VIEW-PROJECTION-001.md
-->

# Q07/D-024 抓取画面位置的共用比例投影

Created before production or focused test script modification. Original Battle Scene C040 run-04 gives rule source/root/Unity parity but physical X first-difference +3.471117779 px for both grab participants from tick25 through tick40. Current writer applies raw DAT local offsets to already scaled physical X, while its source-rule carrier correctly follows the formal integer relation formula. Existing configured-view B6 test locks this older mixed-space output.

Expected responsibilities, invariants, affected symbols, acceptance and rollback are in the Task. Only the final physical X publication in the shared grab relation pose may change; source-rule X and battle result must remain intact. Use existing `BattleSpatialProjection` as the single conversion entry. Do not alter DAT, camera, Scene, other combat objects or nonbattle behavior. No script in this package has been modified yet.

2026-10-03 implementation: the preceding sentence is the pre-change snapshot. Actual production edit is `BattleInteractionWriter` shared grab relation pose: after existing formal source integer-anchor/blend writes, project both final `SourceRuleX` values through the World's existing `BattleSpatialProjection` and update physical X integer mirrors. It does not change formal X, relations, frames, HP, DAT or Scene. Focused B6 test changed from historical raw local -8 physical pose to `SourceToViewX(148/140)` and `SourceDeltaToViewX(-8)` for identity and 2048×1152 projection; existing incomplete-source guard remains. This is one generic outlet, with no actor/DAT special case.

The focused test was written first, but Unity Editor's local interface timed out before a Test Runner RED result could be obtained. Generated `dotnet build Assembly-CSharp-Editor.csproj --no-restore --nologo -v:q` after the production edit exited 0 with 291 warnings and 0 errors. Current state is `COMPILE_PASS / UNITY_FOCUSED_PENDING / PLAY_PENDING`, not verification. Editor PID105896 remained responding at process level, but TCP tool requests timed out and `Editor.log` stopped at 05:59:26; no second Editor, forced restart, Scene save or cleanup was attempted. User was asked to inspect possible modal/stall. Next do original Editor focused class, C040 same-input Scene and four protected SHA/shutdown checks once available.

Later correction/validation: Unity had completed domain reload; AssetImportWorker15 took port6400, while the original Editor PID105896 listener moved to port6401. No actual Editor hang was established. Original Editor focused `NTSD28_B6_CatchRelation` test category 25/25 PASS, including both new identity/configured-view cases, saved at `artifacts/diagnostics/NTSD28-336B44-Q07-D024-GRAB-VIEW-PROJECTION-001/original-editor-focused-20261003.json`. Original Battle Scene run-05 same formal input 40 full Driver ticks matched source 640/640 fields and retained tick25 positive; physical X residual max fell from 3.471117779 to 0.528882221 px (attacker0/target0.529/third0), Z max0.232877 px. Play exited to the clean initial Menu; four protected SHA stable. See `ACCEPTANCE.md`. No Game View pixel, physical keyboard or pool borrower evidence, so `RUNTIME_PENDING` is honest despite this scoped focused/Scene pass. No DAT/Scene/nonbattle code changed.
