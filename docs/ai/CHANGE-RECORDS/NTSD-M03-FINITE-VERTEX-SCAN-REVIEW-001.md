<!-- CHANGE-RECORD
id: NTSD-M03-FINITE-VERTEX-SCAN-REVIEW-001
status: VERIFIED
code-path: Assets/NTSD/Scripts/Test/Editor/M03FiniteVertexScanDiagnostics.cs
authority: User-approved new-publication M03 review
evidence: artifacts/diagnostics/NTSD-M03-FINITE-VERTEX-SCAN-REVIEW-20261010/REPORT.md
-->
# M03 grouped finite-position scan review
Status: IN_PROGRESS
Task: NTSD-M03-FINITE-VERTEX-SCAN-REVIEW-20261010
Change: NTSD-M03-FINITE-VERTEX-SCAN-REVIEW-001
Authority: user approval to try new-publication M03 investigation, same original optimization point.
One new Editor source only: Assets/NTSD/Scripts/Test/Editor/M03FiniteVertexScanDiagnostics.cs. No production edits, dispatch or monitoring restore.
Source hypothesis: private WriteQuad writes four positions from left/right/bottom/top/z. Stateless grouped scan checks five stored values at original Upload pre-native guard. Original exception timing and chunk ordering must remain. No state/cache/pre-warm/geometry/rule change. Arbitrarily corrupted buffers are outside invariant; explicitly demonstrate that limitation.
First actual-backend finite/nonfinite/growth and IEEE generated-rectangle equivalence using cold reflection and existing real prepared fixture; retain original finite checks. Then once two immutable actual stride44 CPU vertex arrays (2000 and4096quad), original scan vs grouped scan OFF/ON/ON/OFF, each8warm32sample, one full scan per sample. Same compiled Editor, no internal timers/counters, assertions/reflection/serialization outside measured scope, existing calibrated GC recorder. All scopes valid0events, buffer unchanged. Gate: each ON mean below both OFF means, ON worstP95/P99 <= OFF worst; otherwise stop INCONCLUSIVE. Scan-only evidence cannot claim complete Build gain; proposed production candidate still needs full Build validation. No repeated cost group to find PASS.
Protected scopes and dirty backups in Operation. Original Scene clean/Menu8 restored; validate ledger and scoped diff before closure. Existing M03/H07/Driver/device open gates remain.

Symbols: M03FiniteVertexScanDiagnostics, Original/Grouped local mirrors, real-buffer reflection, focused equivalence and finite cost test. New Editor-only tool is inert outside explicit NUnit invocation. Native meshes/textures owned by existing fixture and backend in using/finally. No production caller or runtime owner added. No irreversible changes.

Implementation written: one Editor-only source with actual private CPU vertex capture, uploaded-position comparison, generated IEEE rectangle corpus and arbitrary-corruption negative control. One finite scan ABBA cost method requires semantic PASS and refuses existing output. Original production untouched. Import/compile requested; results pending.

Focused attempt01 after import had no evidence/output (NOT_EXECUTED, not PASS); explicit compile/reload then attempt02 executed and failed after first invalid buffer. Fixture reused a backend after failed Build without required Clear; original existing tests explicitly Clear after rejected invalid input. Correct only test cleanup with backend.Clear() after each invalid case, preserving failure/raw/source backup. Production unchanged; cost not started.

Final diagnostic VERIFIED only: fresh semantic1/1 (21 actual builds/36876quads/27 nonfinite/14641 IEEE rectangles), original guards3/3 and original chunk failure2/2; unique scan group8windows/256 calibrated scopes0events, both sizes LOCAL_SCAN_GAIN_SIGNAL_ONLY. FullBuild and production admission pending. No production changes; exact handoff ready. REPORT and independent-analysis contain raw evidence/limitations. Prior empty test request and fixture Clear failure retained.

Final closure: Validate-ChangeLedger PASSED1412 records/exit0, historical warnings retained; scoped git diff --check exit0 (line-ending notices only), new diagnostic no trailing whitespace. OriginalEditor Menu clean8. Change VERIFIED only diagnostic; exact handoff prepared, dispatch receipt authoritative.
