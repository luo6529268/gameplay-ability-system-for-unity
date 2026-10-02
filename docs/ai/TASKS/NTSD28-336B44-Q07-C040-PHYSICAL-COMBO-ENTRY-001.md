# NTSD28-336B44-Q07-C040-PHYSICAL-COMBO-ENTRY-001

Status: `VERIFIED_SCOPED_FORMAL_DISCRETE_INPUT_ENTRY`. Parent: `NTSD28-UNITY-BATTLE-REALIGNMENT-001 / BATCH-04 / Q07 / C040`.

Authority: formal root 336B44 executable and its playable `GameSession28::step`, `input_routing.cpp`, `data/data.txt`, and decoded `c/kaku/kakun.dat`. OID25 Kakuzu frame60 declares `hit_aj:320`; frame322 declares kind3 catch into action336/caughtact130, and action336 CPOINT selects `vaction:132`. These are candidate fields, not proof that ordinary input reaches them.

Question: from action0, can genuine discrete attack-then-jump input enter Kakuzu action320/322 through the formal host and input phase? This is a prerequisite to replacing the controlled initial catcher action in the C040 three-actor witness. A combo may fail because its edge falls outside frame60; report that result without inventing a rule.

Ownership: add only `Tools/NTSD28Q07Diagnostics/c040_kakuzu_physical_combo_probe.cpp`; write unique outputs under `artifacts/diagnostics/NTSD28-336B44-Q07-C040-PHYSICAL-COMBO-ENTRY-001/`; update this Task, matching Change Record, Ledger, STATE, handoff and current master after evidence. No existing formal source, Unity production/test, DAT, image, Scene, Prefab, Menu, or noncombat edits. Existing dirty work is protected.

Method: with formal runtime, mode0, fixed seed, Kakuzu OID25/action0/slot0 and an inert far enemy, sweep a bounded declared attack/jump timing matrix through `session.set_input` and full `session.step`. Record per-tick input phase, current action, comboAJ state, first action60/320/322, RNG and a replayable LFR for a positive candidate. Run independent repeats for any positive and a control; replay the LFR using the formal root executable, compare selected per-tick actor fields. Do not initialize relations, hold or special action. A negative only closes the tested matrix.

Exit: compile without errors; bounded source result and deterministic evidence; formal root check only when a valid LFR exists; scoped Change Ledger and diff checks; exact limits in the report. This package cannot close C040/Q07 or claim Unity Scene behavior. Rollback is a documented forward correction to the new diagnostic; do not delete prior raw output or reset user work.

Result: attack ticks1～2 then jump ticks3～4 or 4～5 reaches Kakuzu action320 at tick4 and kind3 action322 at tick6 from action0; tick2 actually passes frame65 rather than the static frame60 candidate, and both have `hit_aj:320`. Starts5～15 are bounded negatives. Source v2 seed0 repeat hashes equal, root 336B44 replay exit0/PASS and 352/352 selected fields including RNG match. The first seed-mismatch run and command errors remain documented. Far target was not caught, so C040 positive hold/relationship and Unity Scene are still pending. [Report](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C040-PHYSICAL-COMBO-ENTRY-001/REPORT.md).
