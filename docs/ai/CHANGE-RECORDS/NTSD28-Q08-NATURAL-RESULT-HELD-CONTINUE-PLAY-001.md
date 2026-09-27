<!-- CHANGE-RECORD
id: NTSD28-Q08-NATURAL-RESULT-HELD-CONTINUE-PLAY-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q08DirectBattleNaturalKoTwoCyclePlayModeTests.cs
authority: formal GameSession28 held result input and BattleFlow28 timer144 shortcut; original-Editor focused production seam is Play-pending
evidence: artifacts/diagnostics/NTSD28-Q08-NATURAL-RESULT-HELD-CONTINUE-PLAY-001/TASK-CONTRACT.md
-->

# NTSD28-Q08-NATURAL-RESULT-HELD-CONTINUE-PLAY-001

Before script edit: original-Editor focused held-input tests passed, and the original Battle Scene two-cycle natural physical-J KO test passed with both result timers left neutral through 350. The current test also verifies Q07 source-rule birth and natural KO event lifetime in both Worlds. It has not supplied physical held input to a live result at timer144, so the production result-continue seam is still `RUNTIME_PENDING`.

Scope, authority, affected symbols, expected result, preserved assertions, risk, exact acceptance and rollback are in the Task Contract. This extension changes only the second cycle's test input/result expectation. No production, DAT, Scene, mode Asset or nonbattle edit is planned. Validation pending.

Actual test edit: the first World still runs a neutral result to 350. The second World queues physical J on the tick expected to increment native result time 143 to 144, reads the canonical same-tick held Attack/Jump bit, and expects immediate native output350/transition2. Both Worlds retain the natural KO event lifetime70 and Q07 source-birth checks, and keyboard state is released in `finally`. No production or content edit. Original Editor compile and exact Play acceptance are pending.

Scoped result: exact original-Editor UnityTest entered Play and terminal XML `artifacts/diagnostics/NTSD28-Q08-NATURAL-RESULT-HELD-CONTINUE-PLAY-001/UNITY-PLAY-HELD-CONTINUE-PASS-20260927.xml` is 1/1 PASS, SHA `6CE58DF13B4425BAA4EF6ACE4DEA362BD4DF18A7BFC7D96971222983B095D02F`. It proves the first neutral full350 cycle and the second physical-J held 143→144 shortcut with same-tick canonical input, output350/transition2 and direct-Battle same-World logical selection. Existing KO lifetime/Q07 birth assertions remained green. Editor returned idle/non-Play; three protected Scene/Asset hashes were unchanged. No production, DAT, Scene, input actions or nonbattle code changed. `ACCEPTANCE.md` records exact evidence and limitations. The broader production held-input Change remains RUNTIME_PENDING for its other acceptance boundaries; Q08/BATCH-04/total goal remain open.
