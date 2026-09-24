<!-- CHANGE-RECORD
id: NTSD28-USER-SOURCE-COORDINATE-WEAPON-FRAGMENTS-001
status: FOCUSED_TEST_PASS
change-kind: D024_BUILTIN_AND_DAT_WEAPON_FRAGMENT_SOURCE_BIRTH
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleNativeWeaponPieceWriter.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06NativeWeaponPieceEditorTests.cs
authority: user D-024 ratio decision and paired playable BattleWorld28 materialize_weapon_piece_fragments raw integer positions
evidence: docs/ai/TASKS/NTSD28-USER-SOURCE-COORDINATE-WEAPON-FRAGMENTS-001.md
-->

# NTSD28-USER-SOURCE-COORDINATE-WEAPON-FRAGMENTS-001

Pre-script PLANNED record. Unity physical fragment birth already scales raw random X/Z offsets for the fixed full-background camera, but a new independent source-rule child history is not passed to the task. Formal playable uses source integer X/Z plus the same raw random offsets for both built-in and DAT-table fragments before `spawn_at`. This bounded change propagates source precise/int values without altering physical battle position, random stream, DAT, Scene, camera or other entity types; uninitialized source remains absent and gameplay readers stay gated. Task gives exact acceptance, side effects and rollback. Focused first-difference evidence pending.

## Execution evidence 2026-09-24

- BattleNativeWeaponPieceWriter.Materialize/Spawn: private Spawn receives parent plus already-drawn raw dx/dz (built-in dz=0). Only initialized parents publish source precise/int X/Z = parent source integers + raw offsets. No random/physical/rounding/Y/velocity/slot/action/team/admission/recycling change. Existing dirty ratio edits preserved.
- NTSD28Q06NativeWeaponPieceEditorTests.FragmentBirthPreservesIndependentSourceIntegerHistory: eight initialized/absent by factor1/configured by formal witness row0/row93 cases. Independent parent source precise/int, every accepted child source precise/int and physical ratio/ToEven rounding, formal call sequence/final RNG state, object count, parent source and battle coordinates unchanged.
- Original Editor RED 0b6f55ec08564b2a81d7e20ddff752e4: four absent PASS, four initialized FAIL expected child initialized true/actual false. GREEN 8be060d749f6417f8080cadbc67ae041: 12/12 PASS (new eight plus FragmentBirthRelativeXZUsesViewRatio four). Existing bridge refresh_unity/run_tests/get_test_job/read_console; fresh compilation/domain reload, error Console 0 entries.
- git -c core.safecrlf=false diff --check PASS; Scene SHA256 unchanged 9E7B8A91ADD396D8A3674915BB5AC9A03B8D1A2EC817BA3B12F135D03EBA0AC0; Content/Config/Scene/ProjectSettings status empty.
- Tools/Validate-ChangeLedger.ps1 FAILED, log Temp/NTSD28-USER-SOURCE-COORDINATE-WEAPON-FRAGMENTS-001-ledger.log: unrelated BATTLE-CENTRAL-EDITOR-FOOT-MARKER-PREVIEW-001 and BATTLE-CENTRAL-EDITOR-PREVIEW-001 missing from STATE. Observed STATE 808447 bytes including 808046 NUL; Handoff 696883 bytes including 696534 NUL before this worker attempted this package's state/handoff updates. Neither file edited in this package; root must recover them and rerun validator before delivery.
- Limits: CARRIER_NOT_ACTIVE / GLOBAL_LEDGER_BLOCKED_EXTERNAL. No full SelfCheck, Play or new formal capture; no gameplay source reader activated. Focused pass does not close overall alignment.

2026-09-24 correction to the historical blocker above: root's byte-prepend operation caused the two governing documents' NUL corruption. The corrupt bytes were backed up; intact 03:18 Git object snapshots restored their bodies; subsequent entries were reconstructed from Task/Change Records. `Tools/Validate-ChangeLedger.ps1` now returns exit 0 (`Temp/NTSD28-D024-RECOVERED-DOCS-ledger.log`). Current status is `FOCUSED_TEST_PASS / CARRIER_NOT_ACTIVE / LEDGER_VALIDATED_AFTER_DOC_RECOVERY`; full SelfCheck/Play/formal EXE remains pending for this package.
