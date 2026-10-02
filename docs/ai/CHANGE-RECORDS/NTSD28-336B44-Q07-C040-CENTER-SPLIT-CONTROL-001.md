<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C040-CENTER-SPLIT-CONTROL-001
status: VERIFIED
change-kind: CODE
code-path: Tools/NTSD28Q07Diagnostics/c040_held_pose_source_control.cpp
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B6CatchSettlementVactionPreflightProductionEditorTests.cs
authority: 336B44 playable catch settlement controlled center and CPOINT source split
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C040-CENTER-SPLIT-CONTROL-001.md
-->

# NTSD28-336B44-Q07-C040-CENTER-SPLIT-CONTROL-001

PLANNED before diagnostic or test script edits. The previous formal Bee132/vaction130 pair has different CPOINT coordinates but identical frame centers, leaving the current-frame center branch unisolated. Current formal Bee137/vaction130 has a different center Y while retaining valid kind-2 CPOINTs. Extend the existing source-pass probe to cover both current actions under positive and zero hold, and add a corresponding center-divergent test of the unchanged Unity production writer. Expected side effects are only new diagnostic output and Editor test coverage; no battle behavior should change. Exact paths, acceptance, preservation, risks and forward-correction rollback are in the Task. Do not claim natural reachability or root EXE equivalence from a controlled source-pass run.

Actual script changes before verification: the existing source diagnostic now iterates current Bee action132 and action137, each with hold5 and hold0, carrying the selected action through initial DAT frame lookup, retained-action expectation and CSV output. The existing Unity focused test class adds two isolated synthetic center-divergent cases: current frame132 centerY71 versus vaction130 centerY79, with expected projected Y206 under positive hold and Y214 under zero hold. It retains the prior two CPOINT cases unchanged. No production script, DAT, Scene, resource or non-battle path changed. The formal compile/run, generated Editor build, original Editor named tests and governance checks are pending.

Final scoped evidence: current playable-closure C++ diagnostic compile exit0; separate `run-01` and `run-02` each exit0 with four exact formal pass rows and identical CSV SHA `B4F5412D0F1E85C91DB7F88D47C17BAEDB50F9153D5131AA83B885F44F1CF6BA`. The original Bee132 rows and header are byte-identical to the previous certificate. Bee137 hold5 retained action137/current centerY71 and settled at Y-7; hold0 selected action130/centerY79 and settled at Y1. Generated Editor C# build exit0, 0 errors/253 warnings. Original Editor named center cases passed 2/2, old held-pose plus adjacent cases 10/10; raw jobs and full limits are in `artifacts/diagnostics/NTSD28-336B44-Q07-C040-CENTER-SPLIT-CONTROL-001/REPORT.md`. Original Editor is idle/non-Play, Battle Scene clean and disk SHA unchanged; Menu and mode asset SHA unchanged. Only the declared diagnostic and test scripts changed. Root formal EXE natural LFR, original Battle Scene full Driver and Game View remain unverified; C040/Q07 and the overall goal are open. Governance validation is appended after it runs.

Governance: `Tools/Validate-ChangeLedger.ps1` exit0/PASSED with 1162 records and 8 governed code files in the existing worktree diff. The first `git diff --check` exited 2 because the master Markdown had a new blank line at EOF from the status insert; this formatting-only issue was removed without changing its content. The second `git diff --check` exited 0 with only Git line-ending notices. Both outputs are retained in this package's diagnostic directory. No full test suite was run for the two-case controlled branch.
