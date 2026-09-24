<!-- CHANGE-RECORD
id: NTSD28-USER-SOURCE-COORDINATE-STATE9996-BIRTH-001
status: FOCUSED_TEST_PASS
change-kind: D024_STATE9996_FIVE_CHILD_RAW_SOURCE_BIRTH
code-path: Assets/NTSD/Scripts/Simulation/Passes/LateLifecycle/BattleLateEntityLifecycleModule.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06State9996DirectSpawnEditorTests.cs
authority: user D-024 ratio decision; paired playable BattleWorld28 state9996 five-child source-integer plus raw random X and Z+1
evidence: docs/ai/TASKS/NTSD28-USER-SOURCE-COORDINATE-STATE9996-BIRTH-001.md
-->

# NTSD28-USER-SOURCE-COORDINATE-STATE9996-BIRTH-001

Pre-script PLANNED record. Physical five-child birth was already ratio-scaled for the retained full-background view, but the source-rule carrier is absent. Formal playable copies parent integer X/Z and adds unscaled random X/+1 before transient birth; this package writes that distinct history to the existing task without changing random calls or physical positions. Source fields remain absent when the parent has no source history; gameplay source-rule consumers remain gated. Exact acceptance, side effects and rollback are in the Task. First-difference/runtime evidence pending.

## Execution evidence 2026-09-24

- BattleLateEntityLifecycleModule.SpawnState9996Children now writes explicit source task precise/int X = parent SourceRuleXInt + existing relativeX and Z = parent SourceRuleZInt + 1 only when parent history is initialized. No new RNG calls, physical/velocity/OID/order/admission/default-Z changes; existing dirty ratio implementation preserved.
- NTSD28Q06State9996DirectSpawnEditorTests.FiveChildBirthUsesIndependentSourceIntegerPosition covers initialized/absent and factor1/configured 2048x1152 view. Parent precise (-100.75,87.5) differs from source integers (-102,85) and battle position. All five children including OID218 assert source precise/int and physical scaled X/Z, exact object count, witness random call sequence and final scalar state, zero active pooled tasks after shutdown.
- Test-first original Editor RED a727a04aebad4f55ac86daf50826e759: 4 executed, absent controls 2 PASS and source-initialized 2 FAIL at expected true/actual false. GREEN aad133506b7a485b8d3e29566f50f935: 6/6 PASS, new four plus State9996ChildAfterParentMotion_PreservesFormalScreenFraction two. Fresh refresh/compile/domain reload; error Console 0 entries.
- Commands: existing Editor bridge refresh_unity/run_tests/get_test_job/read_console; Tools/Validate-ChangeLedger.ps1 PASS 749 records (Temp/NTSD28-USER-SOURCE-COORDINATE-STATE9996-BIRTH-001-ledger.log); git -c core.safecrlf=false diff --check PASS. Scene SHA256 remains 9E7B8A91ADD396D8A3674915BB5AC9A03B8D1A2EC817BA3B12F135D03EBA0AC0; git status Content/Config/Scene/ProjectSettings empty.
- Limits: immediate focused writer synthetic witness, no new formal EXE capture/full SelfCheck/real Play/following-tick matrix. CARRIER_NOT_ACTIVE and source gameplay consumers unchanged; not full battle parity. Rollback only added conditional and focused test, retain prior dirty changes.
