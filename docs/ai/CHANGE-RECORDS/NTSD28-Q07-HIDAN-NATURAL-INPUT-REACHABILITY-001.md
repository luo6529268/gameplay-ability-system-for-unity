<!-- CHANGE-RECORD
id: NTSD28-Q07-HIDAN-NATURAL-INPUT-REACHABILITY-001
status: VERIFIED
change-kind: Q07_HIDAN_NATURAL_INPUT_REACHABILITY_SOURCE_DIAGNOSTIC
code-path: Tools/NTSD28Q07Diagnostics/hidan_catch_input_reachability_probe.cpp
authority: formal paired playable GameSession28 input_routing and indexed Hidan OID24 hid.dat hit_aj235 and action249 next236
evidence: controlled action236 source/EXE/Unity capture aligned but natural input route unknown
-->

# NTSD28-Q07-HIDAN-NATURAL-INPUT-REACHABILITY-001

Pre-change: Hidan action236 controlled full-driver comparisons do not establish that normal player input reaches the action. DAT alone shows several `hit_aj:235` entries and action249→236, while playable input routing gives attack then jump history for `hit_aj`; intervening state selection remains unmeasured.

Declared path: only new source diagnostic `Tools/NTSD28Q07Diagnostics/hidan_catch_input_reachability_probe.cpp` plus new JSONL and documentation. Bounded complete `GameSession28::step` schedules from action0, not direct action injection, no Unity production/test, formal source/DAT, Scene, config, image/audio or nonbattle edits. Acceptance and rollback in Task Contract. Status PLANNED before script.

Actual script: only the declared new source probe was added. It runs seven attack ticks1–2 then jump-start ticks3/5/7/9/11/13/15 cases from initial action0, target X1200, 40 complete GameSession ticks each, records action235/249/236/relation and per-tick input phase/action/MP/HP to new JSONL, refuses output overwrite. Build and execution pending; no natural reachability conclusion yet.

Final scoped status `VERIFIED`: paired source full closure compiled with g++ C++17 exit0 (28 core sources plus five playable sources). X1200 first bounded run found jump-start3/5 reach action249→236; later starts7–15 do not. Exact paired input history established attack=5 followed by jump=0, but observed route is action249→236, not action235; the static `hit_aj:235` token did not predict this selected action. After the observed route, the same diagnostic script was extended to accept X580/X1200 and record source-authored LFR for jump-start3, with a new binary/output path that retained the first outputs. At X580, attack ticks1–2 and jump ticks3–4 from action0 give action249 tick4, action236 tick10, reciprocal catch tick11, action239 tick12, held injury tick13: attacker PP155→177, target PP300→322 and HP500→470. X1200 does not establish relation or damage. The 40-tick natural-input LFRs replayed through the root formal EXE SHA-256 `B1E13...19033` with no action override, both reports `passed:true`/failure0; independent source CSV/EXE trace comparison has 240/240 equal selected values each for actor action/PP, target PP/HP and both catch slots. Root LFR reports state `nativeParityClaim:false`, so only the independently compared fields establish scoped parity. No Unity natural-input capture/Play, full state or pixels claimed. See same-ID ACCEPTANCE.md.

Final repository gates after the Hidan diagnostic chain: Change Ledger validator PASS, 877 records/11 governed code paths; `git diff --check` exit0; protected Scene/config SHA-256 values unchanged. These gates do not certify Battle Play.
