<!-- CHANGE-RECORD
id: NTSD28-Q08-BATTLE-ONLY-SECOND-CYCLE-SOURCE-WITNESS-001
status: FOCUSED_TEST_PASS
change-kind: GOVERNANCE_ONLY
code-path: NONE
authority: formal Logan playable GameSession28 battle-only first and second result host path
evidence: exact-hash isolated source build exit0; second-cycle fixture double-run exit0 with identical output SHA; one rematch then selection confirmed
-->

# NTSD28-Q08-BATTLE-ONLY-SECOND-CYCLE-SOURCE-WITNESS-001

Created before diagnostic C++ edit. Task: `docs/ai/TASKS/NTSD28-Q08-BATTLE-ONLY-SECOND-CYCLE-SOURCE-WITNESS-001.md`. The governed Unity code paths are `NONE`; exact diagnostic paths are specified by Task. Before: first-cycle source fixture passed, but no fixture reaches the second result after the first recreation. Planned: extend a separate copy of that fixture, build in the isolated native source tree, double-run and record measured flags/flow/scene/world identity. Expected side effects are diagnostic source and build outputs only. No formal J: or Unity production files change. Validation, limits and rollback follow the Task.

Actual: created `artifacts/diagnostics/NTSD28-Q08-RESULT-TRANSITION-HOST-AUDIT-001/native_battle_only_second_cycle_fixture.cpp` and patched its already isolated test copy, leaving the previous first-cycle fixture untouched. Formal and isolated `game_session.cpp` both SHA `9CCB6E12...65E4189`; new fixture SHA `FB168AFD...FC4872`, executable SHA `C8C291D...90A3EBE3`. Focused build exit0, two source-model runs exit0 and byte-identical SHA `52297BA8...57306CA1`. After first direct rematch, `battle_scene_only_loop=0`; second timer350 emits transition2/no combat, next call enters selection/transition1 with same World and dead victim HP0. Exact log, assertions and limitations in `artifacts/diagnostics/NTSD28-Q08-RESULT-TRANSITION-HOST-AUDIT-001/SECOND-CYCLE-SOURCE-WITNESS.md`. Status `FOCUSED_TEST_PASS / SOURCE_MODEL_ONLY`; formal EXE-visible frontend and Unity rematch remain pending. No J: or Unity script/Scene/content edit, commit, push or computer-use.
