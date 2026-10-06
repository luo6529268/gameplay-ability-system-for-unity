<!-- CHANGE-RECORD
id: NTSD28-336B44-PROJECTED-CHARACTER-BOUNDARY-SYNC-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSDEntityRuntime.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Passes/BattleEcsCharacterPreFrameBoundsPass.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28SourceCharacterStageXEditorTests.cs
authority: user P2 repeated knockback sync defect and unified ratio conversion; existing Unity-owned map boundary, approved spatial projection, fresh original Battle run03 tick448 first-difference
evidence: artifacts/diagnostics/NTSD28-336B44-SAGE-P2-REGRESSION-20261006/scene-4eeb5e1d154542429c9a7eb5edce3bef.json
-->

# NTSD28-336B44-PROJECTED-CHARACTER-BOUNDARY-SYNC-001

Pre-change: original saved Battle run03 physical P1 Sage completed at191tick and visible OID99 snapshot; third P2 uppercut hit was real, then source/view first diverged at448tick by1.854463615904 project pixels. NTSDEntityRuntime.ClampSourceRuleCharacterX independently clamps raw source to the same numeric stage width as projected view; LF2Entity compatibility and ECS exact PreFrame use two separate clamp calls. Old characterization test explicitly preserves raw travel after projected wall clamp, which contradicts this newly observed position/collision consistency requirement.

Bounded design: introduce one NTSDEntityRuntime projected-character X boundary method. Preserve existing slot/team/hitstop/selected-mode X clamp decision and project map numeric boundary. When an initialized projected character view position is actually clamped, derive source X from the SAME BattleSpatialProjection.ViewToSourceX and refresh source integer. Do not inverse-convert on unclamped motion (avoid precision churn), do not initialize absent source carriers, and retain existing identity/raw-helper behavior. Both exact and compatibility paths call this same method. No P2/Naruto/frame-specific condition; no Transform/render truth; both coordinate carriers are battle runtime state.

Paths/symbols: NTSDEntityRuntime new ClampProjectedCharacterStageX; LF2Entity.ApplyPreFrameXBounds; BattleEcsCharacterPreFrameBoundsPass.TryApplyExactCharacter; existing NTSD28SourceCharacterStageXEditorTests two-domain characterization correction plus repeated projected edge cases. Existing independent raw clamp helper remains available for identity/historical tests. No new module, queue, manager, cache or shutdown stage.

Expected effects: only source carrier at an actual projected character X wall clamp changes; numeric Unity boundary, view displacement, damage, input, DAT, Scene, cadence, noncharacter TTL, mode rules and ordinary unbounded arithmetic stay unchanged. Current project map/ratio is an approved Unity exception, not a new formal background rule.

Validation: narrow left/right boundary RED then GREEN in exact/legacy paths, existing modified two-domain case, one identity missing-carrier guard; original saved Battle same Sage+three uppercuts run04, source/view/collision consistency and11-stage zero-residual shutdown. Existing geometry and natural191 tests reused. Build/errorCS0, Change Ledger, diff/protected hashes.

Rollback: exact saved own hunks only after separate authorized operation; before bytes/hash/status for four clean scripts in boundary-before-manifest.json under existing Operation. No reset/restore or unrelated dirt.

## Execution update

Original Editor RED job `1e9eb32d25944e46bdedf439535b8dd0` executed exactly8 cases:4 failed at the expected right-boundary source-coordinate assertion,4 passed (left0 boundary and no-clamp/missing-source guards already held). Discovered8955 is not the executed count. Full response retained in `boundary-red-job-final.json`.

Implemented the declared three production symbols through one `ClampProjectedCharacterStageX` entry. Actual projected X clamp refreshes source X and source integer using the same world's projection; unclamped source bytes and absent carrier remain untouched. Identity path still calls the existing raw helper. Existing selected-mode/slot/team/hitstop gate and noncharacter branches are unchanged. Existing stage test updated and four repeated-wall profile/direction cases plus one bit-preservation case added. Status CODE_WRITTEN; compile, same8 GREEN and original Scene run04 remain pending.

Correction after compile/GREEN: original Editor scripts refresh completed, Console errorCS0. Same exact8-case job `7f65f872bd58421d92b36b338bc88175` passed8/8 with no skips. Independent read-only review found no proved issue in the four owned paths and projection definition. Record advances FOCUSED_TEST_PASS. Original saved clean Battle run04 started through MCP only after idle/nonPlay/noTests/noCompile checks; actual Scene result remains pending. No full test suite executed.

Original Battle run04 completed PASS: `scene-e2c9120d493a42568ec2db3a526f6d98.json`, runId `20e1591eb2da469d8d4abd873de615ec`,555 complete Driver ticks, P1 physical Sage191tick/visible OID99; three actual P2 uppercuts including right wall, natural recovery without P2 resets, finalHP200. Maximum source/view error1.5916157281026244e-12 versus pre-fix t448 first difference1.85446361590402. Actual debug body geometry checks followed collision frame/runtime throughout. Existing11-stage shutdown complete, objects/slots/borrowers0, Scene/config unchanged and original Scene clean after owned exit. VERIFIED only for this user-approved projected character boundary repair and declared tests; no full-world/native map/Host-cadence/GPU-image claim.
