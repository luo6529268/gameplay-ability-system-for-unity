<!-- CHANGE-RECORD
id: NTSD28-Q07-NARUTO-NATURAL-PICKUP-SOURCE-001
status: VERIFIED
change-kind: Q07_NARUTO_NATURAL_PICKUP_SOURCE_DIAGNOSTIC
code-path: Tools/NTSD28Q07Diagnostics/naruto_natural_pickup_source_probe.cpp
authority: root formal NTSD2.8-Logan.exe B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and paired playable GameSession28 full step, indexed OID2/OID120 DAT
evidence: Unity controlled complete-Driver natural pickup action115 and held-air action30 pic97 is scoped; no formal source same-route witness
-->

# NTSD28-Q07-NARUTO-NATURAL-PICKUP-SOURCE-001

Pre-change Unity state: the existing opt-in Editor probe's `naturalPickup` branch sends explicit canonical input packets through full `SimulationTickDriver.StepOneTick`, producing OID120 held relation and Naruto action30/pic97 at tick20. It uses temporary runtime slots50/51 and a synthetic human roster slot2; it is not a physical-key or formal-EXE same-world trace. No production code change is justified by this evidence alone.

Planned change and effects: add one diagnostic-only C++ probe under the declared path, loading formal decoded Naruto/weapon definitions, preplacing a ground OID120 as a test fixture, then running complete paired playable session ticks for near and far weapon positions. It records action/pic, holder/weapon relation, phase, position and lifecycle; the probe neither changes formal rules nor persists any Unity state. Expected side effects are only new diagnostic binary/CSV/report files. Exact acceptance, limitations and rollback are in the Task Contract.

Invariants: no direct pickup consumer call, no forced attack/held action, no DAT value edit, no original EXE replacement, no Scene/nonbattle change, no old-result overwrite, and no inference that a source-injected weapon is reproduced by the root LFR playback. Failure or differing action is recorded as a diagnostic first difference, not repaired by a special case.

Code written: `Tools/NTSD28Q07Diagnostics/naruto_natural_pickup_source_probe.cpp` now loads the formal weapon definition, stages near/far ground OID120 fixtures in a paired playable session, drives ordinary input through complete `GameSession28::step`, and writes per-tick CSV plus a bounded summary. No Unity runtime, formal source, DAT, Scene, or existing diagnostic output was edited by this code addition.

Validation: MinGW g++ 15.1.0 C++17 compiled all 28 current core translation units plus playable `game_session.cpp`/`selection_flow.cpp` and this probe with `-municode`, final `compile-v4.log` exit 0. `run-03` exit 0: X190 natural pickup tick2 and ordinary jump/air attack reached action30/pic97 tick16; X800 far control never picked through tick70. Six saved fields on native ticks2–16 versus Unity ticks6–20 with offset +4 matched 90/90; no sampled first difference. Full evidence and initial-state/input differences are in `artifacts/diagnostics/NTSD28-Q07-NARUTO-NATURAL-PICKUP-SOURCE-001/ACCEPTANCE-20260926.md`. The first two compile invocations and first two source invocations failed on command-line include/Unicode/path arguments and remain recorded; the final run supersedes their setup failures. `Tools/Validate-ChangeLedger.ps1` exit 0, 879 records/14 governed code files; `git diff --check` exit 0 (line-ending warnings only); three recovered progress docs each NUL=0. Root EXE same-world and GPU evidence remain unavailable from this fixture.
