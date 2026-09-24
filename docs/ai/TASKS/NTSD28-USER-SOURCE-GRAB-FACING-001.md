# NTSD28-USER-SOURCE-GRAB-FACING-001

Status: `FOCUSED_TEST_PASS / PLAY_PENDING`. D-024 non-perceptual audit; Q07 paused.

Authority: formal root EXE SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`; playable `BattleWorld28` kind1/kind3 paired relation uses `attacker->position.x > target->position.x` before action selection and facing writes. D-024 source-rule carrier represents those formal integer positions while physical X is scaled for the full-background view.

Unity pre-change: `BattleInteractionWriter.TryApplyGrab` kind1 and `TryApplyKind3Grab` compare physical integer X. Divergent source/physical order can reverse both facing and signed action selection, even though paired source position output was previously corrected.

Declared script scope: `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleInteractionWriter.cs`; `Assets/NTSD/Scripts/Test/Editor/NTSD28B6CatchRelationExactFieldsProductionEditorTests.cs`. Add divergent-order RED test for kind1 and kind3, then compare source integer X when both histories initialized, physical fallback otherwise. Retain frame lookup, signed action resolution, local pose/dual-position writes, pool/no-allocation behavior. No DAT/Scene/resources/camera/ProjectSettings/nonbattle edits.

Acceptance: original-project Editor divergent RED/GREEN and existing B6 class focused pass. Full Driver, natural Battle Play and formal EXE parity pending.

Rollback: reverse only declared script hunks after review.

Result: original Editor RED `934eca9cacc549fcaa8ebeeae43eb792` failed both kind1/kind3 reversed-order cases; GREEN `2ac7226b11f54bff9641210917568569` passed B6 25/25. Both grab branches now use source integer X only when both carriers are initialized. Full Driver/natural Play/formal EXE parity pending.
