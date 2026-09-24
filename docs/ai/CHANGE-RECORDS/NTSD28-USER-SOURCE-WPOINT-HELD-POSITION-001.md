<!-- CHANGE-RECORD
id: NTSD28-USER-SOURCE-WPOINT-HELD-POSITION-001
status: FOCUSED_TEST_PASS
change-kind: D024_SOURCE_RULE_WPOINT_HELD_POSITION_WRITERS
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleHeldObjectWriter.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponHeldStateResolver.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06HeldNativeFrameBindingEditorTests.cs
authority: formal root EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and playable BattleWorld28 settle_held_refill_objects; user D-024
evidence: docs/ai/TASKS/NTSD28-USER-SOURCE-WPOINT-HELD-POSITION-001.md
-->

# NTSD28-USER-SOURCE-WPOINT-HELD-POSITION-001

Created before script edits. Two active canonical held-object writers position generic and weapon-component children only in physical coordinates. This package adds native integer-anchored source-rule X/Z writes for both without changing DAT-local offsets or physical placement. Direct character-link resolver reachability, cover2 physical mismatch and full runtime acceptance remain separate. Record actual validation after implementation.

2026-09-24 result: the two declared canonical writers now write independent held source-rule precise/integer X/Z from holder source integer position, raw WPoint/frame-center offsets and formal cover rules when both carriers are initialized. Physical pose and DAT values remain unchanged. Existing row1 generic and row21 weapon-component two-view tests were extended to assert source held X359/Z251 at factor1 and configured full view; physical held X359/384 remains. Original Editor focused jobs `03d997d44bde4781a33e908974259a2a` 4/4, adjacent `a417e536bad94c9abffb84b2054b3e10` 2/2 full tick, and `ee4247a0c9ec47cc80c2e42ffefb33ea` 1/1 incomplete-carrier all passed. Direct character-link path static callers stop at a module with no current production `ProcessTransit` call found; do not equate that to runtime deadness. Cover2 physical branch official-content reachability and Play/EXE remain open.
