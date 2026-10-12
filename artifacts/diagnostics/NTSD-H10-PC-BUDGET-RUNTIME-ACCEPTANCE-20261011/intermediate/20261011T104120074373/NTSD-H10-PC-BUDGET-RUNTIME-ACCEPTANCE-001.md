<!-- CHANGE-RECORD
id: NTSD-H10-PC-BUDGET-RUNTIME-ACCEPTANCE-001
status: CODE_WRITTEN
code-path: Assets/NTSD/Scripts/Test/Editor/H10ResidentClosureDiagnostics.cs
authority: Human approved specific PC engineering targets and bounded same-H10 runtime diagnosis in discussion thread 01a11fe2-bfaa-72f3-a9ed-c8260f63b193
evidence: docs/ai/TASKS/NTSD-H10-PC-BUDGET-RUNTIME-ACCEPTANCE-20261011.md
-->
# NTSD-H10-PC-BUDGET-RUNTIME-ACCEPTANCE-001

Pre-change record, 2026-10-11. Sole code path and new symbols: H10ResidentClosureDiagnostics.PcBudgetRuntimeQualification, PcBudgetRuntimeThreeSessionsPlay, RestorePcBudgetRuntimeScene, nested PcBudget fixture/evidence helpers. Existing methods and production behavior preserved. Previous evidence includes two-session full catalog object ledger and smaller lifecycle/static coverage; it does not cover this combined live Play/dispatch/three-cycle task.

Task contract declares approval, exact read/write domains, first-difference stop, minimum tests, state owner/capacity/stage1/stage5/drain/deferredDestroy lifecycle and rollback. Operation OP-NTSD-H10-PC-BUDGET-RUNTIME-ACCEPTANCE-20261011 contains ten actual preimages/SHA. No runtime state/cache/threshold added to production. Diagnostic state is bounded to one test fixture; no Q06/body or rule changes.

Risks and side effects: original Editor scripts import, a single isolated Play session and strictly owned audio fixture lifecycle; scene setup restore and exact cache/voice/config/manager preservation are mandatory. No irreversible production change; unknown ownership or unsafe Editor blocks execution instead of cleanup. One bounded group only after compile and qualification; no same-version full rerun. Process bytes observed in Editor are diagnostic, not game-process/8GB certification. AudioClip budget may reuse old measured envelope only after current input fingerprint correspondence.

Validation planned, not yet run: compile; PcBudgetRuntimeQualification; PcBudgetRuntimeThreeSessionsPlay; source/resource/Git preservation; Tools/Validate-ChangeLedger.ps1. Code not yet changed at this record creation. Player plan only read, no build. Rollback only precise task additions from audited actual preimages, preserving later user edits and old evidence.

Actual first implementation appended PcBudget* diagnostic members and separate outputs only. Existing method bodies retained. Static review before compile: protect construction failures, capture owned references before first public shutdown, preserve first hot failure samples. No Unity run yet.
