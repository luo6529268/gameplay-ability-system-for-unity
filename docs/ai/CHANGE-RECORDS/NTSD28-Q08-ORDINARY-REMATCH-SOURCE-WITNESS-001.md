<!-- CHANGE-RECORD
id: NTSD28-Q08-ORDINARY-REMATCH-SOURCE-WITNESS-001
status: FOCUSED_TEST_PASS
change-kind: GOVERNANCE_ONLY
code-path: NONE
authority: formal Logan playable GameSession28 ordinary result transition and battle-scene-only rematch live caller; Q08 transition host audit
evidence: matching formal source file hashes, isolated build exit0, ordinary and battle-only branch double-run identical SHA 63A82770; Unity host integration pending
-->

# NTSD28-Q08-ORDINARY-REMATCH-SOURCE-WITNESS-001

Created before diagnostic C++ fixture edit. Task: `docs/ai/TASKS/NTSD28-Q08-ORDINARY-REMATCH-SOURCE-WITNESS-001.md`. The Ledger validator's governed script scope does not include `artifacts/diagnostics`, so metadata uses `GOVERNANCE_ONLY`/`code-path: NONE`; the artifact will nevertheless contain C++ diagnostic code and its exact path/results must be recorded here. No governed Unity/project script or J: authority edit is planned.

Before: mode4 command202 349→350→next freeze has a double-run source witness and a Unity world-clock RED. Ordinary transition2/selection and battle-scene-only direct rematch differ in the formal `GameSession28::step` caller but have no fresh complete-session branch witness in this campaign. Planned fixture and isolated build are constrained by the Task. Expected side effects are diagnostic build output only. Record actual file hash, two runs, measured flow/scene/world reset and limits immediately after editing. Rollback and validation are in the Task.

Actual diagnostic code written: `artifacts/diagnostics/NTSD28-Q08-RESULT-TRANSITION-HOST-AUDIT-001/native_ordinary_rematch_fixture.cpp` retains prior six-group/timing/lethal/mode4 cases, generalizes the sampled freeze projection to a session argument, and adds complete ordinary mode0 and battle-only mode0 cases. Each first checks 349→350 transition2 with no combat tick and unchanged sampled battle projection; the next call asserts ordinary upper selection/state1/same world versus battle-only recreated combatant with positive HP and active battle scene. Isolated build and double-run pending; no Unity project/authority edit.

Validation: cited formal-vs-isolated source files hash-match; focused native build exit0. Two complete test runs exited0 and have identical output SHA `63A82770A51AB12C08C142C327D4E21DA6B2AA7BC37E61AE12D59DDF39AC8921`. Ordinary branch emitted `350:result+transition2+noCombat;next:selection+transition1+sameWorld`; battle-only emitted `350:result+transition2+noCombat;next:recreatedBattle+victimHp500`. Artifact fixture SHA `3CE1551E...50F87E64`, executable SHA `E33C41A5...DE3C66D`; full hashes and limits in the Q08 transition audit. The prior pending sentence records the intermediate code-written state; current status `FOCUSED_TEST_PASS / SOURCE_MODEL_ONLY`. This does not prove Unity host integration, formal EXE frontend pixels, or mode4 equivalence. No J: authority, Unity script, Scene, content or nonbattle edit.
