<!-- CHANGE-RECORD
id: NTSD-H07-INPUT-FACING-COST-DIAG-001
status: ABANDONED
code-path: Assets/NTSD/Scripts/Test/Editor/H07InputFacingCostDiagnosticsEditorTests.cs
authority: Direct user continuation of H07 input same-cause cross-module diagnosis
evidence: artifacts/diagnostics/NTSD-H07-AI-DECISION-WORK-REVIEW-20261010/input-facing-cost-diag/REPORT.md
-->
# Input facing native setter cost diagnostic
Current: LF2Entity.SwitchDir on main thread calls LF2Sprite.SwitchLR which writes SpriteRenderer.flipX even under central legacy suppression. Managed-only update already exists; restoring legacy visibility does not explicitly resync flip.
Change: one new Explicit Editor diagnostic only; production stays original. Local control existing managed-only, not an implemented candidate. No speculative direction cache.
Scope/validation/side effects/rollback: Operation NTSD-H07-INPUT-FACING-COST-DIAG-20261010; fixed finite ABBA, narrow semantics,0B, original Scene untouched, no fullDriver/12ms gain claim. Test pending.

2026-10-10: Exact new Explicit Editor test written (CODE_WRITTEN evidence); production unchanged. Five semantic witnesses and finite8windows, controlled preview Scene teardown. Await compilation/test. Existing Editor before-state idle/nonPlay NTSD_Menu verified in editor-before.json.

Initial job total0 NOT_EXECUTED due Explicit filter; temporarily remove Explicit for exact dispatch, restore afterwards. Preserve test-result.json. No measurement occurred.

Final: ABANDONED diagnostic candidate for this fixture; no measurement executed. Actual prefab/backend chain lacks demonstrated body setter frequency. Initial runner succeeded total0 is NOT_EXECUTED, no PASS. Explicit original bytes restored, no second test dispatch, production unchanged. REPORT.md contains exact evidence/limits.

Import/discovery correction: total0 did not establish Explicit as sole cause; script-only refresh had not imported new meta. Final all-scope import with Explicit restored, Console0; no rerun. ABANDONED remains, validator PASSED1407, diff-check0, 2972protected0drift.
