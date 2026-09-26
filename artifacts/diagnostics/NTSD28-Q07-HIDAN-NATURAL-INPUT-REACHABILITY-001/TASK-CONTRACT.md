# NTSD28-Q07-HIDAN-NATURAL-INPUT-REACHABILITY-001

Status: `PLANNED`, BATCH-04/Q07. Controlled Hidan action236 source/EXE/Unity driver selected fields align, but entering action236 from action0 through real input routing is unverified.

Final scoped status: `VERIFIED`; source and root formal EXE natural input path documented in `ACCEPTANCE.md`. Unity natural input and Battle Play remain separate gates.

Authority: formal indexed Hidan OID24 `hid.dat`: multiple authored actions have `hit_aj:235`, action235 leads to action254 while action249 leads to236; paired playable `GameSession28::step`, `input_routing.cpp` uses history attack=5 then jump=0 for `hit_aj`. The exact runtime route cannot be assumed from DAT alone.

Declared script before edit: new `Tools/NTSD28Q07Diagnostics/hidan_catch_input_reachability_probe.cpp` only. Initialize actor/target OID24 from action0 in full paired source GameSession, use a bounded attack-then-jump input schedule search with separated target X1200, record first action235/249/236 and sampled input phases to new JSONL evidence. A follow-up overlapping target may be attempted only after an action236 route is observed. No production, DAT, Unity script, Scene, config, sprite, audio or nonbattle changes. Preserve all existing outputs and concurrent dirty work.

Acceptance: paired source build and exact candidate results, with failed schedules retained. Do not claim natural catch or Unity parity unless the action and relation are actually observed; if not reached, report the observed path and keep this gate open. Ledger/diff/protected-hash validation. Rollback only the new diagnostic and evidence.
