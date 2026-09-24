# NTSD28-USER-SOURCE-TELEPORT-WRITER-001

Status 2026-09-24: `FOCUSED_TEST_PASS / CARRIER_NOT_ACTIVE / PLAY_PENDING`. Final original-Editor class 13/13 PASS with source/physical divergent target anchors. Source-domain candidate ranking is a separate unresolved reader; Q07 remains paused.

Historical pre-edit status: `IN_PROGRESS / SOURCE_FIRST`. Parent D-024. Gameplay source-rule readers and Q07 remain gated.

Authority: formal root EXE SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033` and playable `BattleWorld28::resolve_native_teleport_state` state400/401. The chosen target's native integer X/Z anchors the teleporter; X takes the original 120/60 relative offset, Z takes target integer Z+1, then integer mirror is synchronized. No-target native path snaps X/Z precise to the current integer mirrors. User D-024 independently scales physical X offset to preserve screen fraction.

Unity pre-change: both `LF2Entity.RunNativeTeleportState` and its early-specials compatibility method write only physical X/Z and integer mirrors, using a scaled X offset. They do not update the source-rule X/Z carrier. Target selection currently reads physical X/Z; that separate reader gate is outside this writer package and must remain explicitly open.

Declared script scope: `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs` two teleport branches and `Assets/NTSD/Scripts/Test/Editor/NTSD28C05NativeTeleportProductionEditorTests.cs`. When both source and selected target have initialized source-rule positions, write source X from target source integer ± raw 120/60 and source Z from target source integer+1, using the existing formal midpoint adapter for X. For native no-target, snap initialized source precise to its own integer mirrors. Preserve physical view-scale output, Y, velocity, selection, no-target semantics, DAT/Scene/ProjectSettings/nonbattle files; do not activate any other source-rule reader.

Acceptance: original Editor exact existing state400/401, both facing, factor1/configured view tests extended with divergent source/physical target anchors, plus no-target source precise/int. Check ledger, diff and Scene/content boundaries. Selection-rank parity, full Driver/Play/formal EXE remain separate gates.

Rollback: reviewed patch limited to the two declared scripts and Task/Change docs.
