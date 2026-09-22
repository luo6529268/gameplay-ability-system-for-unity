<!-- CHANGE-RECORD
id: NTSD28-Q08-BATTLE-ONLY-REMATCH-UNITY-RED-001
status: CODE_WRITTEN
change-kind: TEST_ONLY
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q08BattleOnlyRematchRedPlayModeTests.cs
authority: formal Logan GameSession28 first battle-only result rematch; current direct Battle Scene Play probe
evidence: target RED in isolated real Battle Scene Play; UNITY-BATTLE-ONLY-REMATCH-RED.xml and BATTLE-ONLY-REMATCH-UNITY-RED.md
-->

# NTSD28-Q08-BATTLE-ONLY-REMATCH-UNITY-RED-001

Created before the test script edit. Task: `docs/ai/TASKS/NTSD28-Q08-BATTLE-ONLY-REMATCH-UNITY-RED-001.md`. Before: the direct Battle Scene result command 2 remains pending in real Play; there is no target assertion for the first rematch. After: one exact new Editor Play test will assert recreated World, roster and resumed battle under the real Battle Scene/formal content bootstrap. No production behavior change. Expected failure is at the host boundary; compile/bootstrap/device failures are not valid RED evidence. Validate only the targeted test in the isolated Unity copy, then ledger and diff checks. Do not alter existing diagnostic evidence. Rollback only the new test path and isolated copy, subject to repository deletion authorization.

Actual: new test file only, identical in original and isolated project. Isolated Unity entered real Battle Scene Play and completed formal content bootstrap; XML total1/passed0/failed1 at line 51 exactly on old World identity after command2. The planned roster/tick assertions have not yet run. This expected RED is not `FOCUSED_TEST_PASS` or runtime acceptance. Scene files remain identical to each other and to the prior SHA. Detailed result and XML hash: `artifacts/diagnostics/NTSD28-Q08-RESULT-TRANSITION-HOST-AUDIT-001/BATTLE-ONLY-REMATCH-UNITY-RED.md`. No production edits. Follow-up production Task/Change required; existing pending-command diagnostic test must be superseded when behavior changes.
