<!-- CHANGE-RECORD
id: NTSD28-Q08-NATURAL-KO-FEED-LIFETIME-PLAY-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q08DirectBattleNaturalKoTwoCyclePlayModeTests.cs
authority: formal battle_world.cpp knockout-tail lifetime and project-owned mode Asset; BATCH-04 Q08 natural Play gap
evidence: artifacts/diagnostics/NTSD28-Q08-NATURAL-KO-FEED-LIFETIME-PLAY-001/TASK-CONTRACT.md
-->

# NTSD28-Q08-NATURAL-KO-FEED-LIFETIME-PLAY-001

Before edit: existing direct Battle Scene UnityTest uses physical J to cause two real Naruto KOs, reaches two result transitions and checks Q07 source-rule birth in both Worlds. It checks the KO event at creation, but not its configured 70-tick lifetime. The separate Q08 formal/Unity complete-tick gate covers a negative-environment event, not this natural input route.

Plan, exact path, authority, invariant, acceptance, rollback and protected work are in the Task Contract. This test-only extension will assert both lifetime boundaries in each World without adding a fixture or changing battle behavior. Do not claim Q08 or BATCH-04 completion from a passing test. Validation is pending.

After script edit: the existing helper now asserts the selected project's KO feed is present with lifetime70, captures the actual natural event time for the credited physical-J KO, and checks that exact event is present at time+70 and absent at time+71 in both World cycles. It preserves the original Q07 birth, Q08 physical-input, two-KO, result and World assertions. No production behavior or resource was edited. Original Editor compile and exact Play test remain pending.

Scoped result: the first exact Editor Play run failed before the added assertions because the first P1 J edge was absent from canonical tick input; its raw XML is archived in the Task folder, SHA `C366F6DFA2967ABB9D9571AD20AC726BEFFC6C2822379C912ACC9334AAD6908D`. No code was changed in response. A single same-test rerun entered Play and passed 1/1, including both natural event expiry boundaries in both Worlds. Its terminal XML SHA is `2B6B5C6F2A61BF2D21E991805CDDEC46210DC7516DE03E3C01B6DEA8A5E23475`. Original Editor became idle/non-Play, stale MCP job was cleared after XML archival, three protected Scene/Asset hashes stayed unchanged. `artifacts/diagnostics/NTSD28-Q08-NATURAL-KO-FEED-LIFETIME-PLAY-001/ACCEPTANCE.md` records exact limits. This Change is VERIFIED only for that natural Play lifetime route; Q08/BATCH-04 and total alignment remain open. Ledger validator exited 0 after the edit with 913 Records, and `git diff --check` exited 0 with line-ending notices.
