<!-- CHANGE-RECORD
id: NTSD28-USER-SOURCE-TELEPORT-WRITER-001
status: FOCUSED_TEST_PASS
change-kind: D024_SOURCE_RULE_TELEPORT_WRITER
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C05NativeTeleportProductionEditorTests.cs
authority: formal root EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and playable BattleWorld28 state400/401; user D-024
evidence: docs/ai/TASKS/NTSD28-USER-SOURCE-TELEPORT-WRITER-001.md
-->

# NTSD28-USER-SOURCE-TELEPORT-WRITER-001

Created before script edits. Both Unity teleport outlets write scaled physical X and physical Z+1 but leave the independent source-rule X/Z carrier stale. This package adds source-only writes from the selected target's source integer mirrors and raw offset, plus native no-target integer snap, while leaving target selection and physical behavior unchanged. Runtime and focused evidence will be appended after implementation; gameplay source readers remain inactive.

2026-09-24 result: the active native teleport and early-specials compatibility outlet now write initialized source-rule X from the selected target's source integer X plus signed raw 120/60 and source Z from its source integer Z+1. Native no-target snaps initialized source precise X/Z to their own integer mirrors. If the selected target lacks a source-rule position, the source carrier remains untouched. Existing physical view-scaled X, Y, velocity, target-selection and DAT behavior are unchanged. The existing original-Editor class was extended with divergent physical/source target coordinates and no-target source snap, then ran final job `4609039115e941b29ba3118933466310` 13/13 PASS after the final missing-target guard adjustment; prior pre-guard job `c5307647b8404a2eb2014496fdb94679` was also 13/13. This is `FOCUSED_TEST_PASS / CARRIER_NOT_ACTIVE`: source-domain candidate ranking, Driver/Play/formal EXE, other writers and source-rule gameplay readers remain open.
