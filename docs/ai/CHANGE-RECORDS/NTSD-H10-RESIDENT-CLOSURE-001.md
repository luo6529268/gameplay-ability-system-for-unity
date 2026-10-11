<!-- CHANGE-RECORD
id: NTSD-H10-RESIDENT-CLOSURE-001
status: IN_PROGRESS
code-path: Assets/NTSD/Scripts/Test/Editor/H10ResidentClosureDiagnostics.cs
authority: Direct user request to finish H10 before moving on, 2026-10-10
evidence: docs/ai/TASKS/NTSD-H10-RESIDENT-CLOSURE-20261010.md
-->
# NTSD-H10-RESIDENT-CLOSURE-001
Requirement: finish current-stage H10 assessment then implement evidence-backed remaining optimization.
Initial implementation Editor-only diagnostic; current production untouched. Inspect current native catalog and existing production collection/resolution through isolated objects; source ownership must be proven before release changes.
No singleton auto-creation/replacement except explicitly saved-restored diagnostic GameConfig context if necessary. No existing live cache deletion, no full library load, no forced GC. Cold allocations allowed.
Tests: actual C# current catalog, resolved formal/legacy paths, shared IDs, conservative selected closure, sound coverage and source-derived PCM payload clearly labelled estimates; any native object tests exact owner cleanup.
Risk: under-approximating dynamic dependencies causes silent sounds; global cache contains borrowed resources. No production filtering until proven.
Rollback: new diagnostic and exact doc delta only, with audited preimages; no overwrite user work.

## Diagnostic implementation and actual validation
Added CurrentProductionCueInventoryAndVisualClosureCoverage and UncachedOwnedSourceCanRetireAfterExactProductionConversion. Job 8b662ac5add6460fa203666cfb0b323b actual 2/2; first discovery total0 retained as NOT_PASS. Real loader empty-key behavior and actual PCM source retirement proved in isolated owner; no production loading changes. 87 protections stable/Menu clean8. See artifacts/diagnostics/NTSD-H10-RESIDENT-CLOSURE-20261010/REPORT.md. Whole H10 continuation remains IN_PROGRESS.
