<!-- CHANGE-RECORD
id: NTSD28-Q07-D024-SHARED-ANCHOR-CALLSITE-001
status: FOCUSED_TEST_PASS
change-kind: CODE
code-path: Assets/NTSD/Scripts/Simulation/Core/BattleSpatialProjection.cs
code-path: Assets/NTSD/Scripts/App/AppManager.cs
code-path: Assets/NTSD/Scripts/Animation/Character/BruteForceSceneQuery.cs
code-path: Assets/NTSD/Scripts/Simulation/Passes/RandomWeapon/BattleRandomWeaponDropModule.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28D024UnifiedSpatialProjectionEditorTests.cs
authority: user D-024 unified ratio entrance and formal B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033
evidence: docs/ai/TASKS/NTSD28-Q07-D024-SHARED-ANCHOR-CALLSITE-001.md
-->

# NTSD28-Q07-D024-SHARED-ANCHOR-CALLSITE-001

Status: `FOCUSED_TEST_PASS / CURRENT_ZERO_ANCHOR_EQUIVALENCE`. Script ownership, acceptance and rollback are in the Task.

Before: `BattleSpatialProjection` holds a single World-owned X/Z scale but makes each source↔physical position caller pass a shared anchor. Three production owners repeat literal zero for the project map origin. This is numerically correct now, yet leaves a later approved anchor change scattered across consumers. Explicit-anchor tests and diagnostic fixture calls must remain valid.

Plan: add immutable instance anchor X/Z initialized to zero by the existing factory and identity path, expose no-anchor source↔view X/Z methods that delegate to the existing explicit-anchor methods, then replace only the listed production literal-zero calls. Add a focused zero equivalence and nonzero configured-instance round trip. No mapping policy change, no background DAT consumption or map/config edit. Expected gameplay side effect: none; risk: an accidental overload or origin discrepancy, guarded by exact tests and scan. Actual changed symbols, compile/test result, remaining uncertainty and SHA protection will be appended immediately after edit.

Actual edit (2026-09-29): `BattleSpatialProjection` now owns immutable `SharedAnchorX/Z`, zero in `Identity` and existing two-argument viewport factory. A four-argument factory can carry one explicitly selected shared anchor without changing current World setup. Four one-argument source↔view methods delegate to the original two-argument methods using the instance anchor. `AppManager.SyncParticipantPhysicalBirthPosition`, the shared `BruteForceSceneQuery` X/Z collision geometry/recovery calls, and both `BattleRandomWeaponDropModule` birth paths now use the no-anchor methods; the existing explicit-anchor API remains. Existing projection test class adds current-zero equivalence and a nonzero-instance round trip. No stage/domain policy, scalar ratio, Scene/map/DAT or nonbattle change.

Validation so far: generated `dotnet build Assembly-CSharp-Editor.csproj --no-restore -nologo -clp:ErrorsOnly` exited0, 0 errors and 245 warnings; static production scan found no remaining explicit `SourceToViewX/Z(..., 0.0)` or `ViewToSourceX/Z(..., 0.0)`; scoped `git diff --check` exit0. Original Editor import, focused test, validator and protected SHA still pending. No runtime parity claim yet.

Final narrow verification: original Editor PID11944/port6401 refreshed its script assemblies (the bridge connection dropped during domain reload, then recovered), with final assembly writes later than all five owned scripts and 0 observed C# compilation errors. Projection class EditMode job `f3f208c182994b059cb2074593634b14` passed 6/6. The three affected production caller classes (App physical participant birth, B0F8 random weapon birth, B5 shared collision) in job `5f62319306e64cac91a098a4c2fd2dc8` passed 39/39. Editor ended non-Play/idle. The static explicit-zero production conversion scan was empty; `Tools/Validate-ChangeLedger.ps1` reported PASSED (1021 records, 52 governed code files in current dirty diff) and scoped `git diff --check` exited0. Five protected Battle/Menu Scene, SunagakureMap, GameConfig and ProjectBattleModeConfig SHA-256 values match the pre-change baselines. Prior pending statements mark interim states. Current result is limited to equivalent current zero-anchor routing; nonzero project map anchor activation, stage-domain split, same-state formal/Unity Play and all Q07 exits remain unverified. Rollback remains scoped to this package's five files, preserving concurrent changes.
