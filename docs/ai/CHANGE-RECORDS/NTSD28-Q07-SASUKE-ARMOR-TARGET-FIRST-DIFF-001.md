<!-- CHANGE-RECORD
id: NTSD28-Q07-SASUKE-ARMOR-TARGET-FIRST-DIFF-001
status: VERIFIED
change-kind: FORMAL_ARMOR_TARGET_FIRST_DIFFERENCE_DIAGNOSTIC
code-path: Tools/NTSD28Q07Diagnostics/sasuke_armor_target_lfr_probe.cpp
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditor.cs
authority: root formal NTSD2.8-Logan.exe plus paired playable/core selected-armor resource-transfer branch and indexed OID87 sus.dat
evidence: X550 formal events 3/3/35/35 at tick16; source/rootEXE/originalEditor PASS; first MP difference tick16; ACCEPTANCE-20260926.md
-->

# NTSD28-Q07-SASUKE-ARMOR-TARGET-FIRST-DIFF-001

2026-09-26 final diagnostic: `VERIFIED_SCOPED_DIAGNOSTIC / FIRST_DIFFERENCE_FOUND`. New C++ harness and strict Unity schema compiled; both X550/X1200 paired source/root EXE/original Editor runs PASS. Formal tick16 four OID440 hits yield HP damage 3/3/35/35; Unity matches target HP424/action186 and child lifecycle, but after the two reduced hits misses 52 current MP on each combatant. X1200 maps 2100/2100 fields, X550 has 34 differences over 1428 comparisons, first at tick16 MP; independent target horizontal position divergence starts tick20. See `artifacts/diagnostics/NTSD28-Q07-SASUKE-ARMOR-TARGET-FIRST-DIFF-001/ACCEPTANCE-20260926.md`. A separate production Task/Change is needed. No production/DAT/Scene/nonbattle edit in this diagnostic.

Pre-change state: prior Sasuke OID11 vs Naruto OID7 X550/X1200 exact-state comparison now matches after shared unarmored resource fix. Formal `battle_world.cpp` selected-armor branch has a separate resource-transfer call; current Unity `BattleDamageWriter.ApplyAlternateDamage` has no visible call, but this is only a static candidate. OID87 `c/sasu/sus.dat` type-1 armor is byte-identical between formal and Unity staged content. The current source harness and Unity strict schema only admit OID7; no OID87 scenario has run.

Declared diagnostic edit: add only one bounded C++ source LFR harness under Tools and one separate strict OID87/current-MP300 scenario branch to the existing Editor raw exporter. Preserve the existing OID7 and other schema guards and all earlier evidence. Expected side effects are native compile output, original Editor compile/reload and focused diagnostic files. No production/Scene/Prefab/DAT/PNG/ProjectSettings/nonbattle changes.

Acceptance and rollback: source and root formal EXE provenance, hit/armor gate, 26-tick source-formal-Unity traces, first difference or no-difference result, focused checks and protected hashes. If no selected armor is observed, do not code a speculative fix. Roll back only this diagnostic's exact additions after review; never clean unrelated dirty work or overwrite a prior result.
