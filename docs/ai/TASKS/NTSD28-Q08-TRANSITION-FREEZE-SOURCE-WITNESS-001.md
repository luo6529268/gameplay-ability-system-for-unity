# NTSD28-Q08-TRANSITION-FREEZE-SOURCE-WITNESS-001

Status: `FOCUSED_TEST_PASS / SOURCE_MODEL_ONLY / UNITY_RED_PENDING`. Parent BATCH-04/Q08. Authority: formal Logan EXE SHA `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`, corresponding playable `GameSession28::step`/`BattleFlow28::step` and `artifacts/diagnostics/NTSD28-Q08-RESULT-TRANSITION-HOST-AUDIT-001/REPORT.md`.

Scope: create a new diagnostic C++ fixture under `artifacts/diagnostics/NTSD28-Q08-RESULT-TRANSITION-HOST-AUDIT-001/native_transition_freeze_fixture.cpp` from the already archived source fixture, then copy it to the isolated `I:/GitHub/Unity_GAS/ntsd-q08-native-validation-20260922/source/ntsd28_playable/tests/game_session_tests.cpp` for `build.ps1 -Target game_session_tests`. The original formal source under J: is read-only. No Unity production or test script, Scene, DAT/image, nonbattle or GAS change.

Capture the formal complete-session mode4 timer349 before, timer350 transition202 same tick, and one following upper tick: combatant action/counter, integer/precise position, HP/MP, synchronized and CRT RNG calls, `last_tick` presence and transition. Repeat the executable twice and compare output hashes. Add ordinary state2 and battle-scene-only distinctions only after a concrete caller fixture supports them; do not call mode4 representative of all modes.

Acceptance: build succeeds with matching official source file hashes for cited live functions, two runs byte-identical, assertions show frozen world and null `last_tick` on transition and next tick; report precise limits. Rollback by restoring only isolated test copy from archived prior fixture under existing approval rules; never reset/clean or touch J: authority.

Observed acceptance: isolated build exit0, both runs exit0 and identical SHA `E194AC7976FCB01FE49905A9420E578C76CAF3FC3F31F52D33DC5CB59C3DBC83`; sampled entity fields and dual RNG stable, `last_tick` null at350 and following tick. This Task's mode4 source witness is complete; Q08 transition integration and Unity RED remain open under the parent audit.
