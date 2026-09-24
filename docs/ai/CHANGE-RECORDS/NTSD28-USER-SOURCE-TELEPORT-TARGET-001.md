<!-- CHANGE-RECORD
id: NTSD28-USER-SOURCE-TELEPORT-TARGET-001
status: FOCUSED_TEST_PASS
change-kind: D024_SOURCE_RULE_TELEPORT_TARGET_RANKING
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C05NativeTeleportProductionEditorTests.cs
authority: formal root EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and playable BattleWorld28 resolve_native_teleport_state; user D-024
evidence: docs/ai/TASKS/NTSD28-USER-SOURCE-TELEPORT-TARGET-001.md
-->

# NTSD28-USER-SOURCE-TELEPORT-TARGET-001

Created before script edits. Current physical-distance candidate ranking can diverge from the formal source-integer ranking after approved view scaling. This change owns both teleport selection paths and focused C05 tests; results and limits are recorded in the Task.

2026-09-24 result: two divergent state400/state401 tests first failed in original Editor job `7cadeedda7124d7c94a64cf8b79942e7` with expected selected target collision Y -21 but actual -42 in both cases. `LF2Entity.RunNativeTeleportState` and early compatibility path now track source and physical ranking independently; source ranking is selected only when the source and every eligible candidate have initialized source positions. Incomplete source history retains physical fallback. Eligibility, strict comparison, candidate iteration, teleport offsets and dual destination positions remain. Original Editor job `551d9463866649fbb1dde703f00f11bf` passed all 15 C05 cases with no failures. Natural Battle Play, full Driver and formal EXE comparison remain pending. No DAT, Scene, resource or nonbattle changes.
