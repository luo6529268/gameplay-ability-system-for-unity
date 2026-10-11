# M03 grouped finite-position scan review
Status: SCOPED_REVIEW_COMPLETE / READY_FOR_BOUNDED_IMPLEMENTATION
Task: NTSD-M03-FINITE-VERTEX-SCAN-REVIEW-20261010
Change: NTSD-M03-FINITE-VERTEX-SCAN-REVIEW-001
Authority: user approval to try new-publication M03 investigation, same original optimization point.
One new Editor source only: Assets/NTSD/Scripts/Test/Editor/M03FiniteVertexScanDiagnostics.cs. No production edits, dispatch or monitoring restore.
Source hypothesis: private WriteQuad writes four positions from left/right/bottom/top/z. Stateless grouped scan checks five stored values at original Upload pre-native guard. Original exception timing and chunk ordering must remain. No state/cache/pre-warm/geometry/rule change. Arbitrarily corrupted buffers are outside invariant; explicitly demonstrate that limitation.
First actual-backend finite/nonfinite/growth and IEEE generated-rectangle equivalence using cold reflection and existing real prepared fixture; retain original finite checks. Then once two immutable actual stride44 CPU vertex arrays (2000 and4096quad), original scan vs grouped scan OFF/ON/ON/OFF, each8warm32sample, one full scan per sample. Same compiled Editor, no internal timers/counters, assertions/reflection/serialization outside measured scope, existing calibrated GC recorder. All scopes valid0events, buffer unchanged. Gate: each ON mean below both OFF means, ON worstP95/P99 <= OFF worst; otherwise stop INCONCLUSIVE. Scan-only evidence cannot claim complete Build gain; proposed production candidate still needs full Build validation. No repeated cost group to find PASS.
Protected scopes and dirty backups in Operation. Original Scene clean/Menu8 restored; validate ledger and scoped diff before closure. Existing M03/H07/Driver/device open gates remain.

Completed evidence: REPORT.md, semantic-result-03.json, original-guard-result.json, chunk-failure-result.json, cost-result.json, independent-analysis.json. Dispatch requires exact saved execution-handoff.txt; not automatic production default.
