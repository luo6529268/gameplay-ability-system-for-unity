# NTSD28-336B44-Q07-C040-BEE-NATURAL-ENTRY-001

Status: `VERIFIED_SCOPED_FORMAL_DISCRETE_INPUT_ENTRY`. Parent: `NTSD28-UNITY-BATTLE-REALIGNMENT-001 / BATCH-04 / Q07 / C040`.

Authority: formal 336B44 EXE and matching playable `GameSession28::step`/`input_routing.cpp`, `data/data.txt` OID75, and decoded `c/bee/bee.dat` frame63 `hit_a:66`, frame69 `hit_a:70`, frame70～73 action chain. The mixed-initial C040 triad already proves Bee action70 gives armor hit at tick5 and positive hold at Kakuzu's tick7 grab; action70 from ordinary input remains unproved.

Question: with Bee OID75/action0 and discrete attack presses only, what bounded input schedules reach action66/70/73 in full formal GameSession? Record the exact input phase and earliest action ticks. A DAT next chain alone is not a natural-input proof.

Ownership: add only `Tools/NTSD28Q07Diagnostics/c040_bee_natural_attack_probe.cpp` and new artifacts under `artifacts/diagnostics/NTSD28-336B44-Q07-C040-BEE-NATURAL-ENTRY-001/`; then update Task, Change Record, Ledger, STATE, handoff and master with results. No formal source, Unity production/test, DAT, images, Scene, Prefab, Menu or noncombat edits. Preserve all dirty work; no computer-use or second Unity project.

Method: mode0/seed0, Bee action0/slot0 and distant inert opponent, initial attack ticks1～2 plus two bounded two-tick re-press windows. Run complete GameSession ticks, record phase, actor action, edge/combo, exact first frame63/69/66/70/73 and RNG. Repeat positive schedules, emit/replay LFR through formal root and compare selected fields. A negative only closes the declared schedule matrix; no action override, relation, hold, damage or DAT mutation.

Exit: zero-error compile; bounded source and formal-root evidence or accurate negative; deterministic repeat; scoped Ledger and diff checks. Natural Bee action73 alone does not establish simultaneous Kakuzu grab, Unity Scene, C040, Q07 or overall closure. Rollback is a documented forward correction of the new diagnostic and records, not deletion of prior files.

Result: 121 complete-tick schedules, 8 reach action70/73 from Bee action0. Earliest second attack7/third16 reaches action65/63/66/69/70/73 at ticks2/8/9/14/16/19. Two full-matrix runs produce four equal file hashes, root formal EXE representative LFR exit0/PASS and 480/480 selected fields agree. Far opponent prevents contact; near three-actor timing and original Unity Scene remain open. [Report](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C040-BEE-NATURAL-ENTRY-001/REPORT.md).
