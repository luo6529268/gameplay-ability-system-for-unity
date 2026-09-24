# NTSD28-USER-SOURCE-TELEPORT-TARGET-001

Status: `FOCUSED_TEST_PASS / PLAY_PENDING`. D-024 non-perceptual audit; Q07 remains paused.

Authority: formal root EXE SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`; playable `BattleWorld28::resolve_native_teleport_state` in `source/ntsd28_core/src/simulation/battle_world.cpp`. State 400/401 selects eligible live characters by source integer X/Z Manhattan distance, strict nearest `<10000` or farthest `>-1`, in slot order. User D-024 scales actual physical displacement while preserving the full-background camera.

Unity pre-change: `LF2Entity.RunNativeTeleportState` and legacy early teleport rank candidates using view-scaled physical X/Z. Existing dual-carrier destination writer therefore can teleport to the wrong target when source and physical rankings disagree.

Declared script scope: `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs`; `Assets/NTSD/Scripts/Test/Editor/NTSD28C05NativeTeleportProductionEditorTests.cs`. Add focused state400/state401 divergent-ranking witnesses before production change. In both selection paths, use source integer X/Z if source and every eligible candidate have initialized source-rule positions; otherwise preserve the existing physical ranking without mixing domains. Retain eligibility, slot order, strict tie/threshold, approved physical offset and independent source destination. No DAT, Scene, resources, camera, ProjectSettings or nonbattle edits.

Acceptance: RED before and GREEN after for divergent nearest/farthest; existing C05 class passes in original-project Editor. Full Driver, natural Battle Play and formal EXE parity remain separate gates.

Rollback: review and reverse only declared script hunks, retaining other work.

Result: Original Editor RED job `7cadeedda7124d7c94a64cf8b79942e7` failed both divergent state400/state401 cases, expected selected target collision Y -21 but got -42. Both selection paths now maintain independent physical/source rankings and use source only if every eligible participant has source history. Original Editor GREEN job `551d9463866649fbb1dde703f00f11bf` passed C05 15/15. Full Driver, natural Battle Play and formal EXE parity remain unverified.
