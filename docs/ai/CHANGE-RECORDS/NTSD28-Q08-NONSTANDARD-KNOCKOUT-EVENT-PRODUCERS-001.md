<!-- CHANGE-RECORD
id: NTSD28-Q08-NONSTANDARD-KNOCKOUT-EVENT-PRODUCERS-001
status: CODE_WRITTEN
change-kind: CODE
code-path: Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleNegativeEnvironmentRecoveryWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleCpointWriter.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B4State1218EnvironmentCreditEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5NegativeEnvironmentRecoveryProductionEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B6HeldInjuryAccountingCoverProductionEditorTests.cs
authority: formal Logan playable battle_world.cpp record_native_knockout at calls 1898, 2262, 6133
evidence: artifacts/diagnostics/NTSD28-Q08-KNOCKOUT-PRODUCER-COVERAGE-001/REPORT.md
-->

# NTSD28-Q08-NONSTANDARD-KNOCKOUT-EVENT-PRODUCERS-001

Created before script modification. Status `PLANNED`; no compile, test or runtime evidence for this package yet. The [Task Contract](../../../artifacts/diagnostics/NTSD28-Q08-NONSTANDARD-KNOCKOUT-EVENT-PRODUCERS-001/TASK-CONTRACT.md) fixes authority, exact paths, expected effects, nonbattle boundary, acceptance and rollback.

Before: three existing nonstandard lethal producer paths increment the resolved `KnockoutCount358` but do not append the formal event. The current shared standard-hit event writer drops an unresolved source, unlike the native helper. After: the shared event writer accepts exact slots and preserves formal missing-source defaults; each existing count writer publishes exactly one event immediately before HP subtraction. The earlier Q08 buffer/snapshot/checksum ownership is reused with no schema change. No KO count, damage, score, renderer, sound, Scene or nonbattle behavior change is intended.

Risks: source and credit differ for environment paths; a negative impact source is the formal invalid slot, not the victim; held injury resolves only the catcher direct owner; duplicate append or wrong owner depth would change Q09 feed and lockstep checksum. Narrow fixtures and same-seed trace must distinguish these. Rollback is only this Record's reviewed deltas; do not restore/reset unrelated dirty files. Original Editor WORDS test request is consumed with no result, so a competing test request is not queued until its terminal state is known.

2026-09-22 implementation: `SimulationWorld` now has a slot-based append path, validates the credited runtime, follows up to four source owners, reads source type from the registered entity or raw runtime, and preserves formal `-1/1000` defaults when source is absent. Existing standard-hit publication reuses this path, so an unresolved source no longer drops a valid-credit event. The environment floor-contact, negative-environment recovery, and held CPoint writers append beside their existing single KO count increment before HP subtraction; no damage/score/count formula or pass schedule was changed. Existing focused fixture files now assert event fields, missing-source defaults and zero records on representative rejected gates. Script-diff whitespace check passed.

Validation: `Tools/Validate-ChangeLedger.ps1` returned 0 (existing unrelated declared-but-not-current-diff warnings remain). A Temp-only absolute targets file added the earlier Q08 new test to the original project's generated Editor csproj input; `dotnet msbuild Assembly-CSharp-Editor.csproj -t:Build -clp:ErrorsOnly -nologo` returned 0 with runtime and Editor DLLs under `Temp/diagnostics/NTSD28-Q08-NONSTANDARD-KNOCKOUT-EVENT-PRODUCERS-001/Build`. `-getItem:Compile` showed the three edited existing Editor fixture files and earlier Q08 test as evaluated compile inputs. This is offline compilation only: it did not refresh the live Unity Editor or run NUnit. Original Editor compile, focused NUnit, SelfCheck, same-seed full-tick trace, Play and shutdown remain pending; status stays `CODE_WRITTEN`.
