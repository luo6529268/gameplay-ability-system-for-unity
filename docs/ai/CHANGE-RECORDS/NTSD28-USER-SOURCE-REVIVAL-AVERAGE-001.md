<!-- CHANGE-RECORD
id: NTSD28-USER-SOURCE-REVIVAL-AVERAGE-001
status: FOCUSED_TEST_PASS
change-kind: D024_SOURCE_RULE_REVIVAL_AVERAGE_AND_RNG_GATE
code-path: Assets/NTSD/Scripts/Simulation/Passes/Respawn/BattleRespawnModule.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B4RevivalNormalFloorRngProductionEditorTests.cs
authority: formal root EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and playable BattleWorld28 advance_native_revivals; user D-024
evidence: docs/ai/TASKS/NTSD28-USER-SOURCE-REVIVAL-AVERAGE-001.md
-->

# NTSD28-USER-SOURCE-REVIVAL-AVERAGE-001

Created before script edits. Current physical-peer `sumX` controls both respawn placement and RNG; formal rule-space peer integer X instead controls that transaction. This bounded package adds a single source-domain gate and paired precise writes when carrier history is complete, while retaining an explicit physical fallback for incomplete history and all existing physical view scaling. Fill actual validation and limitations after implementation.

2026-09-24 result: physical and source peer integer sums are accumulated in one eligible-peer loop. When revived and all eligible peers have initialized source coordinates, formal source `sumX` gates one `0x90/0x91` RNG pair and both precise output domains; source uses raw offsets and physical uses the approved view scale once. Both integer mirrors remain unchanged until later sync. Incomplete history retains previous physical behavior without inventing source history. Original Editor reimported both declared scripts and focused class job `d9a35ab9ae5d421f8afa8215a72eb128` passed 11/11, 0 failed/skipped. New opposite-gate cases and existing floor, negative integer-average, excluded-peer and fallback tests passed. Full Driver/Play, participant history and formal EXE observation remain pending; code is not full parity proof.

2026-09-24 scoped Play addition: the existing original Battle Scene Play runner in the already-declared Editor test file now invokes the two new source-domain zero-gate cases and configured-view offset case along with seven prior controls. Original Editor fresh `Temp/NTSD28-B4-RevivalNormalFloorRng-Play-v1.result` at local12:13:41 says `state=Passed`, `logicalCases=10`; original Editor returned to non-Play/idle and Battle Scene disk SHA remained `9E7B8A91ADD396D8A3674915BB5AC9A03B8D1A2EC817BA3B12F135D03EBA0AC0`. This was a controlled temporary-World Play probe, not a natural participant Driver or formal EXE witness. Status stays `FOCUSED_TEST_PASS / PLAY_PENDING` for the full declared exit.
