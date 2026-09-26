# Q07 hit_Fa5 child birth vitals: first difference

Formal paired playable `NativeAi28::step_non_character_hit_fa` sets child `SpawnRequest28.hp = 0`, `mp = 0`, `initial_action = 0`. Its higher runtime slot receives the same-tick frame tail. Indexed `w/e.dat` frame0 has `hit_a:3`, `hit_d:1`; `BattleWorld28::apply_native_type3_frame_hp_drain` keeps HP0 and writes action1. The complete source Session records action1 at tick1.

Original Unity Editor after the structural-epoch repair: exact group1 complete Driver job `d98a731d057f453990db1f51c5d2502d` reached completed tick1, passed zero-postcommit-breach assertion, then failed new formal HP0 assertion with **actual child HP497**. This is consistent with Unity's generic spawn default HP500 and same-tick frame drain of 3; it is stronger than the earlier action0/1 observation. The no-birth group3 exact job `7fe8725d71ec486c8f2c5e097d013d72` passed its eight-row source comparison 1/1. No second Editor or computer-use was used.

The test-only diagnostic is complete; positive parity remains RED. Production repair requires a separate governed Task/Change to carry explicit newborn HP/MP from this direct-spawn semantic through both logic-only and renderer-backed factory paths. It must not infer a special case from object ID or mutate DAT values. BATCH-04/Q07 and D-024 remain open.
