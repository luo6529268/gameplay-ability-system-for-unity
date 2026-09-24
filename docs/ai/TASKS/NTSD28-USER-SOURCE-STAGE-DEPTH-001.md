# NTSD28-USER-SOURCE-STAGE-DEPTH-001

Status 2026-09-24: `FOCUSED_TEST_PASS / CARRIER_NOT_ACTIVE / PLAY_PENDING`. Original Editor new source-depth 6/6 and adjacent StageZ 9/9 PASS, including zero-allocation hot path. Source stage X, other writers and source readers remain open; Q07 paused.

Historical pre-edit status: `IN_PROGRESS / SOURCE_FIRST`. Parent D-024. Source gameplay readers and Q07 remain gated.

Authority: formal root EXE SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033` and playable `BattleWorld28::clamp_type0_stage_depth` in the simulation tick sequence. Despite its name, it visits all active entities: type0 depth is clamped to [zMin,zMax], other types to [zMin-1,zMax+1], followed by integer mirror truncation. User D-024 allows physical motion to be scaled in the fixed full-background view, so source depth must clamp independently.

Unity pre-change: default `BattleEcsCharacterStageZPass.ExecuteDataOriented`, legacy `SimulationStageRenderModule.ClampCharacterZToStageBoundsAll`, and later `LF2Entity.ApplyPreFrameZBounds` clamp only physical Z/ZInt. An initialized source-rule Z can pass stage edges or inherit a physical clamp artifact, creating a non-perceptual rule-coordinate difference.

Declared script scope: `Assets/NTSD/Scripts/Simulation/Core/NTSDEntityRuntime.cs` shared source-only clamp helper; `Assets/NTSD/Scripts/Simulation/Ecs/Passes/BattleEcsCharacterStageZPass.cs`, `Assets/NTSD/Scripts/Simulation/Stage/SimulationStageRenderModule.cs`, `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs` three reachable depth outlets; focused `Assets/NTSD/Scripts/Test/Editor/NTSD28SourceStageDepthEditorTests.cs`. No change to physical bounds, stage data, DAT, camera/Scene/ProjectSettings/nonbattle code or source-rule gameplay readers.

Acceptance: initialized vs absent source carrier, type0 and non-type0, near/far edge and interior, configured vs factor1 physical view, default/Legacy/ShadowCompare early StageZ and later PreFrame fallback, integer truncation and no allocation on existing hot path. Run exact original-Editor focused tests plus relevant existing stage tests, ledger/diff/Scene guards. Full Driver/Play/formal EXE separate.

Rollback: reviewed patch in declared scripts only; preserve unrelated working-tree edits.
