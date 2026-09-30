<!-- CHANGE-RECORD
id: NTSD28-R06-FORMAL-STORY-QUEUED-REACHABILITY-001
status: FOCUSED_TEST_PASS
change-kind: CODE
code-path: Tools/NTSD28Q07Diagnostics/formal_story_queued_reachability_probe.cpp
authority: root Logan EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and paired playable GameSession28 story mission1 child5 stage row OID8 hp350 join500; alignment R06
evidence: docs/ai/TASKS/NTSD28-R06-FORMAL-STORY-QUEUED-REACHABILITY-001.md
-->

# NTSD28-R06-FORMAL-STORY-QUEUED-REACHABILITY-001

Before edit: previous R06 ordinary natural trace is same-state and complete, while the queued consumer has controlled evidence and stage `Join` producer has a focused Unity birth gate. The formally parsed mission1/child5 phase0 contains OID8 Chiyo `join:500` but no current evidence states its complete `GameSession28` birth tick, slot, queued fields, or natural death/revival. A bounded paired-source diagnostic is needed before claiming an eligible natural Unity queued scenario. Only a new Tools C++ diagnostic is authorized; formal files, Unity production/tests, DAT, Scene, mode and nonbattle files are protected. Validation and rollback are in the Task.

Actual diagnostic code: new `Tools/NTSD28Q07Diagnostics/formal_story_queued_reachability_probe.cpp` creates `GameSession28` mission1/child5/mode1 with one named OID11 player, formal runtime roots and fixed seed; it writes a fresh 301-row CSV from initial world and 300 neutral complete ticks, including selected OID8 slot/HP/action/lives/queued HP/group/owner and revival event. It refuses to overwrite output. No authority or Unity production code changed.

Validation: paired playable/core compilation completed with empty `compile.log`. First run used the wrong extracted-root argument and returned 4 (`unable to open catalog.csv`), with no CSV; corrected run against `resources/runtime` for both roots returned 0 and `rows=301 join_birth=1 natural_lethal=0 queued_continuation=0`. The CSV shows initial tick0 OID8 slot23/HP350/action360/lives0/nextHP500/group5/owner23. Across ticks0–300 the same slot, HP350, phase0 and nextHP500 persist, with zero revival events. Root formal EXE SHA remains `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`. This is a scoped source-birth positive and bounded neutral queued-path negative; no Unity or root EXE gameplay parity is claimed. Raw output and checks: `artifacts/diagnostics/NTSD28-R06-FORMAL-STORY-QUEUED-REACHABILITY-001/ACCEPTANCE.md`. R06/Q07 remain open; no brute-force neutral rerun is authorized by this result.

Repository validation: `Tools/Validate-ChangeLedger.ps1 -RepositoryRoot <workspace>` exited 0 (existing unmatched-record warnings only), `git -c core.safecrlf=false diff --check` exited 0, and Battle/Menu/GameConfig/ProjectBattleModeConfig asset SHA baselines stayed stable. No Unity compile or Play was required by this source-only diagnostic; the earlier Unity stage producer and natural ordinary revival each have separate original-Editor evidence.
