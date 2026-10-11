<!-- CHANGE-RECORD
id: NTSD-H08-RESIDENT-VISUAL-MEMORY-REVIEW-001
status: VERIFIED
code-path: Assets/NTSD/Scripts/Test/Editor/H08ResidentVisualMemoryDiagnostics.cs
authority: User-approved H08 independent memory review before implementation handoff
evidence: artifacts/diagnostics/NTSD-H08-RESIDENT-VISUAL-MEMORY-REVIEW-20261010/REPORT.md
-->
# H08 resident visual memory diagnostic
Task: NTSD-H08-RESIDENT-VISUAL-MEMORY-REVIEW-20261010
Before: source suggests each live body-sheet reference allocates a separate Texture2D, even for the same content path. Actual frequency and memory impact not yet verified. Source textures already Apply(false,true); CPU texture-copy disposal is not a new opportunity.
After planned: one inert Editor-only diagnostic uses current C# catalog and first-declaration ownership and an isolated actual production per-sheet loader, without changing production or global publications. Inventory estimates and Profiler.GetRuntimeMemorySizeLong fixture observations reported separately. No gameplay authority change.
Symbols: H08ResidentVisualMemoryDiagnostics inventory, repeated-sheet actual stage fixture, exact output report and owned cleanup.
Validation: compile; focused actual inventory and bounded duplicate staging; source/Scene/settings protections; ledger/diff. No real battle-wide resident memory/device/optimization acceptance claim.
Rollback: preserve evidence/inert tool; deletion requires separate approval and audit. Exact existing dirty backups held in matching Operation.

Actual: compiled and actual inventory/production duplicate2/2, original shared ownership helper1/1 pass; no production implementation. Report ready and exact3-script handoff prepared.
