<!-- CHANGE-RECORD
id: NTSD28-Q08-SECOND-BATTLE-ONLY-RESULT-UNITY-RED-001
status: CODE_WRITTEN
change-kind: TEST_ONLY
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q08SecondBattleOnlyResultSelectionRedPlayModeTests.cs
authority: formal Logan GameSession28 second battle-only result upper-selection boundary
evidence: temporary-host Play target RED; real Battle Scene twice OOM during formal image loading; SECOND-CYCLE-RED-AND-MEMORY.md
-->

# NTSD28-Q08-SECOND-BATTLE-ONLY-RESULT-UNITY-RED-001

Created before test script edit. Task: `docs/ai/TASKS/NTSD28-Q08-SECOND-BATTLE-ONLY-RESULT-UNITY-RED-001.md`. Before: first direct rematch now recreates battle, but its explicit one-rematch latch only prevents another direct recreation; absent Menu Scene, ordinary AppManager route cannot consume the second result. Expected after: one new real Battle Scene Play test records the first difference at second transition2→1 while preserving World identity/frozen combat. This is test-only, no runtime behavior change. Validate targeted isolated Unity test and ledger/diff; do not claim UI/pixels or natural KO. Rollback only the declared file and isolated copy under repository rules.

Actual: new test file contains a formal-content real Battle Scene case and a temporary-host logic proxy. Two real-Scene attempts terminated with OOM at `broken.png` during bootstrap, no XML and no combat verdict. The `-nographics` temporary host test compiled and ran, with same World/frozen tick checks passing and target transition1 assertion failing on observed2 (XML total1/passed0/failed1). Evidence and limits: `artifacts/diagnostics/NTSD28-Q08-RESULT-TRANSITION-HOST-AUDIT-001/SECOND-CYCLE-RED-AND-MEMORY.md`. Production unchanged; real-Scene acceptance remains pending.
