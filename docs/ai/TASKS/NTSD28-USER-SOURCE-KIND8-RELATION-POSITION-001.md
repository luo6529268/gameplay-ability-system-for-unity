# NTSD28-USER-SOURCE-KIND8-RELATION-POSITION-001

Status 2026-09-24: `FOCUSED_TEST_PASS / CARRIER_NOT_ACTIVE / PLAY_PENDING`. Original Editor final production kind8 class 15/15 PASS, including next physics integer sync. Legacy direct resolver reachability and remaining relation writers remain open; Q07 paused.

Historical pre-edit status: `IN_PROGRESS / SOURCE_FIRST`. Parent D-024. Q07 and source-domain decision readers remain gated.

Authority: formal root `NTSD2.8-Logan.exe` SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`, playable `BattleWorld28::resolve_special_relation_hit` kind8 branch around `battle_world.cpp:5708-5747`, and user D-024. Accepted kind8 sync mode `-1` leaves position, `0/2` copies target precise X, `1/2` copies target precise Y, and all non-`-1` modes copy target precise Z+1. Native integer mirrors are deliberately left to later physics. User-approved physical output remains view-scaled where applicable.

Unity pre-change: active `BattleHitCandidateSequenceRunner` dispatches kind8 to `BattleKind8ControlRelationWriter.TryApply`, which writes only physical precise X/Y/Z and leaves physical integer mirrors. An initialized source-rule X/Z carrier remains stale after the relation snap. Legacy direct `LF2CharacterDatHitResolver` has an older kind8 branch with different physical semantics; its production reachability via direct calls is not established in this package and remains a separate gate.

Declared script scope: `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleKind8ControlRelationWriter.cs` source-only position updates in accepted sync branch; `Assets/NTSD/Scripts/Test/Editor/NTSD28B5Kind8AtomicProductionIntegrationEditorTests.cs` extend existing exact mode matrix. Only copy source precise X for modes other than 1, and source precise Z+1 for all modes other than -1, when both source positions exist. Preserve source integer mirrors until physics, and do not derive from physical position. No DAT/Scene/ProjectSettings/nonbattle changes; no source-domain decision-reader activation.

Acceptance: all sync modes with divergent source/physical anchors and unchanged source integers, source absence gate, existing kind8 action/MP/eligibility tests, adjacent full-tick replay where needed, original Editor compile and focused NUnit. Formal same-input/Play and legacy direct branch remain separate gates.

Rollback: reviewed patch in two declared scripts, preserving existing dirty work.
