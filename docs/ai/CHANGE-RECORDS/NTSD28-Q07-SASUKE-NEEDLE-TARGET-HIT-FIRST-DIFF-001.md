<!-- CHANGE-RECORD
id: NTSD28-Q07-SASUKE-NEEDLE-TARGET-HIT-FIRST-DIFF-001
status: VERIFIED
change-kind: DIAGNOSTIC_NATIVE_UNITY_FIRST_DIFFERENCE_ONLY
code-path: Tools/NTSD28Q07Diagnostics/sasuke_needle_lfr_probe.cpp
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditor.cs
authority: formal NTSD2.8-Logan root EXE and paired playable GameSession28/HitCandidateBuilder28/BattleWorld28 with formal OID440 chi.dat
evidence: X550/X1200 paired source-rootEXE-originalEditor 26-tick PASS; first difference at tick16 attacker MP; ACCEPTANCE-20260926.md
-->

# NTSD28-Q07-SASUKE-NEEDLE-TARGET-HIT-FIRST-DIFF-001

2026-09-26 final diagnostic result: `VERIFIED_SCOPED_DIAGNOSTIC / FIRST_DIFFERENCE_FOUND`. Both native diagnostic and original Unity Editor compile; root formal EXE X550/X1200 reports PASS; both strict Unity raw scenarios PASS. X1200 2100/2100 common field comparisons agree; X550 1428 comparisons contain only 11 attacker-MP differences at ticks16–26. Formal hit reduces target HP500→360 in the same tick as Unity. Native resource-transfer production branch has no equivalent call in Unity's shared standard-character damage path; the existing pure helper uses the wrong legacy MP bank and has no production caller. See `artifacts/diagnostics/NTSD28-Q07-SASUKE-NEEDLE-TARGET-HIT-FIRST-DIFF-001/ACCEPTANCE-20260926.md`. Production repair is a new independent Task/Change. No DAT, image, Scene, ProjectSettings or nonbattle edits.

2026-09-26 code-written update: `Tools/NTSD28Q07Diagnostics/sasuke_needle_lfr_probe.cpp::wmain` now accepts only optional X550/X1200 and preserves default X1200 plus the old seven-column CSV when invoked with historical three arguments; the explicit-target form appends victim HP/action. `Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditor.cs::ValidateScenario` now recognizes a separate target-hit schema and permits exactly X550/X1200 with all other original Sasuke fields fixed. The existing Sasuke schema retains its strict X1200 admission. No production script/Scene/DAT was written. C++ compilation, original Editor compile and both runtime cases are pending at this point. Rollback remains confined to these exact diagnostic additions after provenance review.

Pre-change state: the C++ source LFR Sasuke harness fixes target X1200; Unity's existing Sasuke scenario validation also fixes X1200. Formal chi action2 has a complete kind0 ITR, and the original Scene has naturally produced four OID440/action2 children. Neither side has an accepted target-hit witness for that natural child chain. Existing C++ tool and Unity raw capture script are current clean/dirty files respectively; the existing unrelated Unity script diff is protected.

Declared edit: only the two `code-path` scripts above. The C++ harness gains a narrowly validated optional target X without changing its historical default or replacing prior outputs. The Unity exporter gains a separate strict target-hit schema/fixture admission for X550/X1200, leaving old frozen schemas and runtime capture behavior unchanged. New evidence goes in the same-ID diagnostic directory. Expected side effects are diagnostic files and Editor compile/domain reload only. No production rule, DAT, image, Scene, camera, Config, Menu or framework change.

Invariants: same formal EXE/content identity, seed 0x28A55A5A, Stage23, two human combatants, Sasuke action110/position(500,0,350)/team1, Naruto action0/Z350/team2, 26 ticks, defend/right/attack at completed ticks2/4/6. Source-rule geometry and D-024 physical screen travel remain distinct; do not modify DAT hitboxes or scale ITR/BDY dimensions. X1200 remains the no-hit control; X550 is a predeclared overlap hypothesis, not a guaranteed hit. Capture/report no-hit gates and first differences honestly. No existing diagnostics are overwritten.

Acceptance and rollback: compile C++ and original Unity Editor, run only the two focused source/release/Unity cases, compare recorded per-tick source/Unity/release fields and input/RNG, retain failures, verify protected Scene/config hashes and no NUL regression in three restored docs. Run Ledger validator and diff whitespace check. If production first difference appears, this diagnostic Record cannot authorize a production fix; create a separate Task/Change. Rollback is limited to exact new script hunks and outputs under repository deletion/overwrite rules, preserving all pre-existing dirty work.
