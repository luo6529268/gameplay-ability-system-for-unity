<!-- CHANGE-RECORD
id: NTSD28-USER-SOURCE-COORDINATE-JSON-CHECKSUM-001
status: FOCUSED_TEST_PASS
change-kind: D024_SOURCE_RULE_JSON_CHECKSUM_PROJECTION
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleParitySnapshot.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleLockstepChecksumEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: user D-024 source-rule history requirement; separate occupied raw runtime snapshot and in-process lockstep checksum projection
evidence: docs/ai/TASKS/NTSD28-USER-SOURCE-COORDINATE-JSON-CHECKSUM-001.md
-->

# NTSD28-USER-SOURCE-COORDINATE-JSON-CHECKSUM-001

Status: `FOCUSED_TEST_PASS / DIRECT_EXTENDED_SELFCHECK_PASS / CARRIER_NOT_ACTIVE`. Before script edits, the Task recorded exact scope, v3→v4 Extended/Lockstep schema effect, frozen parity-v3 boundary, active/empty/occupied raw acceptance and rollback. In `BattleParitySnapshot.cs`, both Extended and Lockstep schema IDs are now v4; `ProjectExtendedRuntimeSlots` opts in to the nine-field source-rule projection for active entity and empty raw payload and separately publishes `occupiedRawSourceRule` for occupied slots. The shared `ProjectEntityRuntime` defaults this projection off, so frozen `BattleParityFrameSnapshot` stays trace-v3 with unchanged JSON/hash under source-only mutation. Only the directly dependent extended schema string assertion changed in `BattleRuntimeSelfCheck.cs`; no gameplay writer/reader changed.

Original Editor test-first RED job `4c6f7871d4b84459a6043600ba43afe5` failed all six payload/profile cases because source X changes were invisible. Production edit followed; job `939cc5d5fc4e46ceb370d10484d22221` ran 17 tests: three Authority cases failed only at a new test's final substring check, which accidentally matched pre-existing `hitResourceRules`; all nine-field sensitivity, reversal and frozen JSON/hash assertions before that point passed, while Extended cases and direct SelfCheck had no failure. The test was narrowed to exact `"sourceRule":` without production change. Focused rerun job `b38ca4b99cd74a37bc2a3c67fe9b97a5` passed 4/4 (three Authority cases plus direct `CheckExtendedChecksumContracts`). The previously passing Extended cases were not rerun after that test-only string change. This is direct focused evidence, not a full SelfCheck or gameplay parity claim.

`Tools/Validate-ChangeLedger.ps1` passed with 744 Records; `git diff --check` passed; Battle Scene SHA remained `9E7B8A91ADD396D8A3674915BB5AC9A03B8D1A2EC817BA3B12F135D03EBA0AC0`; scoped Content/Config/Scene/ProjectSettings status was clear. Birth/motion/collision/stage/attachment writers, OID219/fusion readers, full Driver/Play and formal EXE remain open. Rollback is limited to these three files and v4 schema IDs, preserving all other work.
