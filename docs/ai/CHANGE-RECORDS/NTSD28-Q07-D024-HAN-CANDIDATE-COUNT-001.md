<!-- CHANGE-RECORD
id: NTSD28-Q07-D024-HAN-CANDIDATE-COUNT-001
status: VERIFIED
change-kind: CODE
code-path: Tools/NTSD28Q09Diagnostics/han_natural_earthquake_lfr.cpp
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q09HanEarthquakeBattlePlayProbeEditor.cs
authority: formal root NTSD2.8-Logan EXE and paired playable collision-candidate phase with current Han Lee DAT; original Unity Battle Scene diagnostics
evidence: artifacts/diagnostics/NTSD28-Q07-D024-HAN-CANDIDATE-COUNT-001/ACCEPTANCE.md
-->

# NTSD28-Q07-D024-HAN-CANDIDATE-COUNT-001

`PLANNED` before either diagnostic edit. Current post-tick Han/Lee rows prove a source-rule versus physical-X geometry gap, but no direct candidate-phase count. The Task fixes two existing opt-in diagnostic paths, one native X520/jump3 case and one original Battle Scene Play, protected scope, output and rollback. No production collision policy, DAT, Scene, mode, Map or nonbattle behavior is authorized by this Record.

Final `VERIFIED` is diagnostic-only. Actual edits: the C++ LFR source now appends completed collision action, Han selected/Lee-directed candidate counts and native appended total, with optional `x520-jump3` selection to avoid rerunning four cases; the Editor Battle probe appends `Frame.Prev2` and `Runtime.HitCandidateCount` to each tick. Native `-O2` compile and selected run exit0; original Unity generated Editor build 0 errors and the original Editor recompiled before the one Play. At collision-action146 native paired Session has one Han→Lee candidate and reciprocal catch in completed tick10; Unity has zero selected candidates at relative tick9 and tick10 and no catch through 50 ticks. Input-phase offset and native pre-candidate coordinates remain limits. The Play report retains its original skill-chain FAIL, while diagnostic first-difference scope is verified. Ordered shutdown, borrower0, Editor non-Play and two Scene SHA values held. See `ACCEPTANCE.md` for exact rows, hashes and protected scope. Rollback would be only the reviewed two diagnostic diffs, subject to repository deletion rules; do not change production policy from this record alone.

Final governance: `Tools/Validate-ChangeLedger.ps1` exit0, 943 Records/14 governed code files in the current diff (`ledger-validation.log`); `git -c core.safecrlf=false diff --check` exit0. Fresh Menu/Battle Scene hashes remained the protected values in the acceptance report.
