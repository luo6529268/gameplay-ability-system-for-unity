# NTSD28-336B44-Q07-C012 special-hit latch tail

Status: `UNITY_FULL_TICK_PASS / RUNTIME_PENDING`. Parent: BATCH-04/Q07. The Unity two-tick hit chain now passes; formal-root same-state and natural Battle Scene evidence are still open, so this is not a Q07 certificate.

Authority: selected formal root EXE SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`. Its declared playable `simulation_tick_driver.cpp::SimulationTickDriver28::step` clears `special_hit_latch_0eb` for each still-active entity after that entity's hit consumers and resource/frame tail. Current `tests/battle_world_tests.cpp::test_special_hit_latch_clears_after_native_entity_tail` checks that an OID815/type3 attacker suppresses a character candidate before the tail, clears at the active tail, and permits a new candidate on the next tick. This source test is not a formal-root EXE same-state trace.

Unity first difference: `SpecialHitLatch0EB` already had dedicated hit writers, consumers, copy/snapshot/checksum handling, and full-reset semantics. Production `SimulationWorld.EntityPostFrameTailAll` did not clear it, and skipped healthless noncharacter objects before any maintenance. The edit clears the latch once at the shared active-entity tail before the health-specific branch. Existing hit producers, pre-tail `ClearHitCandidateCarriers`, data-oriented character maintenance, DAT, resources and scenes were not changed.

| Evidence | Result |
| --- | --- |
| Original Editor exact RED `39687e8b17e64464b4203f1c1bbcc55e` | Two selected tail cases failed 2/2 before production change: latch was true after tail where both cases expected false. |
| Original Editor exact GREEN `783c720d94d54c80a3e531b06a6d21c5` | 4/4 passed, 0 failed/skipped: new legacy and data-oriented character plus healthless type3 tail cases, old legacy/data-oriented maintenance equality, and old pre-tail latch-preservation control. Editor compiled and reloaded before test. |
| Original Editor complete hit tick `bcfc0fb61f17468bbe8ba6c106304322` | 1/1 passed, 0 failed/skipped. Synthetic type3 `LF2SpecialAttack` ordinary ITR and type0 character BDY collect exactly one candidate. First complete tick with latch set leaves HP 500 and clears latch at tail; second complete tick damages the same target. |
| Corrected healthless type3 tail `b90ee5f82a6a491bb06ddca77e88b7fa` | 2/2 passed, 0 failed/skipped for Legacy and DataOriented. The test object explicitly has object enum type3 and null `Health`, closing the previous fixture precondition gap. |

The first two complete-tick fixture jobs (`577ca5628240445fb49d72c6b32e04dc`, `239e202ee5cb45ce9ac9a0ad34da70d8`) failed at second-tick HP because the test initially used `LF2OtherObject`, a type5 shell without `IBattleHitCandidateConsumer`, despite assigning ObjType 3. The second job confirmed geometry and action/position stability. Replacing only the fixture shell with the production type3 `LF2SpecialAttack` made the same two-tick test pass; no hit production code was changed.

The earlier 4/4 GREEN job's purported healthless object also used `LF2OtherObject`, which in fact has non-null `Health` and object enum type5. That job remains valid for the character tail, pre-tail control, and maintenance parity, but not for the healthless/type3 wording. The corrected 2/2 job above supplies that evidence.

Remaining: same-state formal-root EXE/Unity trace and original Battle Scene integration if the chosen type3 content is naturally reachable. The synthetic frame data is a focused production-path fixture, not evidence that a selected DAT or formal release input reproduces this exact state. No Play or full-suite result is claimed for C012.

Governance after the complete-tick test: Change Ledger validator passed (1035 records, 82 governed code files in the existing dirty diff); scoped `git diff --check` passed. Battle Scene, Menu Scene, GameConfig and ProjectBattleModeConfig SHA-256 remain `3A089236...C235ED`, `DD6A48A3...723B9DC3`, `0527D737...7CB8EA7` and `B57CFEF3...85B82`, matching the pre-test baseline. The existing Unity Editor remained available on port 6401; no second project was launched. These checks do not replace the formal release comparison.
