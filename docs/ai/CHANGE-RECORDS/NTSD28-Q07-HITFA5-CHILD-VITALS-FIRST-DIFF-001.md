<!-- CHANGE-RECORD
id: NTSD28-Q07-HITFA5-CHILD-VITALS-FIRST-DIFF-001
status: VERIFIED
change-kind: Q07_HITFA5_CHILD_VITALS_FIRST_DIFFERENCE_DIAGNOSTIC
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07HitFa5FullDriverEditorTests.cs
authority: formal root NTSD2.8-Logan.exe paired playable NativeAi28 hit_Fa5 and BattleWorld28 type3 frame HP drain, indexed w/e.dat frame0
evidence: NTSD28-Q07-HITFA5-FULL-SESSION-SOURCE-001 tick1 action1; NTSD28-Q07-UNIFIED-AI-STRUCTURAL-EPOCH-001 Unity tick1 action0
-->

# NTSD28-Q07-HITFA5-CHILD-VITALS-FIRST-DIFF-001

Pre-change: existing full-Driver test checks child action before recording child HP. Formal hit_Fa5 source creates HP0 and type3 frame0 drains to zero and selects `hit_d:1`; Unity factory appears to assign default HP500. Actual Unity child HP in this complete Driver is unknown.

Planned minimal test-only edit: assert actual child HP at completed tick1 before child-action comparison. Preserve all other existing checks. No production or resource modification. Acceptance, risk, protected files and rollback in Task Contract. Validation pending.

2026-09-26 result: original Editor MCP refresh compiled this one new assertion. Complete-Driver group1 positive at completed tick1 reached the child and failed formal HP0 with actual HP497 (`d98a731d057f453990db1f51c5d2502d`), after the existing zero-postcommit-breach check. `w/e.dat` frame0 `hit_a:3` means Unity's observed 497 is consistent with default HP500 followed by one frame drain. Group3 no-birth control still passed eight source rows, 1/1 (`7fe8725d71ec486c8f2c5e097d013d72`). Formal source sets `SpawnRequest28.hp=0` before publishing the child. This proves a birth-vitals first difference; the positive test is intentionally RED until a separate production repair, not a parity success. No source DAT or scene changed.
