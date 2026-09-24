# NTSD28-USER-SOURCE-COORDINATE-PHYSICS-001

Status 2026-09-24: `FOCUSED_TEST_PASS / CARRIER_NOT_ACTIVE / PLAY_PENDING`. Original Editor exact jobs: source physics 7/7, weapon identity/type-3 extra 12/12, adjacent fixed-view 11/11. The last class still has known-defect characterization tests. Source flags and other writers remain incomplete; Q07 stays paused.

Historical pre-edit status: `IN_PROGRESS / SOURCE_FIRST`. Parent: D-024 all-entity motion ratio. This is a bounded source-rule coordinate writer package; rule readers remain inactive and Q07 remains paused.

Authority: formal root `NTSD2.8-Logan.exe` SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033` and playable-build `PhysicsIntegrator28::step` in `source/ntsd28_core/src/simulation/physics_integrator.cpp`. Its X/Z integration uses raw velocity and source collision flags, adds independent identity X and type-3 hit_j Z extras, applies friction only after displacement, then truncates the precise coordinates to integer mirrors. User D-024 requires physical output scaled to the fixed full-background view without changing DAT values.

Pre-change Unity: `CharacterMechanics.StepBattleLogic`, `WeaponDynamics`, and `StepNonCharacterBattleLogic` integrate only scaled physical X/Z. `LF2Entity.RunSharedNonCharacterDatFrameAdvance` and `LF2Weapon.WeaponFlightPhysics` add physical identity/type-3 offsets. An initialized independent source-rule X/Z carrier is left at birth or at its preceding frame-motion result. Later OPoint and other rule consumers can therefore read stale native coordinates after a physics tick.

Declared script scope: `Assets/NTSD/Scripts/Animation/Character/CharacterMechanics.cs`, `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs`, `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Weapon.cs`, focused `Assets/NTSD/Scripts/Test/Editor/NTSD28SourceCoordinatePhysicsEditorTests.cs`, and the existing identity/type-3 extra matrix `Assets/NTSD/Scripts/Test/Editor/NTSD28B4IdentityXExtrasEditorTests.cs`. Add source-only unscaled motion using source flags and post-move integer truncation, including the existing independent identity X and type-3 Z paths. Do not derive native coordinates from scaled physical coordinates, alter velocity/friction, activate source-rule readers, edit DAT/Scene/ProjectSettings/nonbattle code, or change camera behavior.

Acceptance: focused test of positive/negative/fractional X/Z, directional blockers, source flag consumption, pre-friction displacement, configured/factor-one physical views, identity/type-3 extras, and absent source carrier. Compile and run only the exact focused Editor class and adjacent existing ratio cases; validate ledger and diff, and verify DAT/Scene boundaries. Full Battle Scene/EXE same-input and source-reader activation are separate gates.

Rollback: reviewed patch limited to the declared scripts and test. Preserve all unrelated working-tree edits and generated/user files.
