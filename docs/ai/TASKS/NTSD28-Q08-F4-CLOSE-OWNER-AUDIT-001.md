# NTSD28-Q08-F4-CLOSE-OWNER-AUDIT-001

Status: `CONFIRMED_PRODUCTION_EFFECT_GAP / READ_ONLY`. Parent BATCH-04/Q08, R03 function-key effect return. Authority and exact Unity caller/consumer search are recorded in `artifacts/diagnostics/NTSD28-Q08-F4-CLOSE-OWNER-AUDIT-001/REPORT.md`.

The formal playable F4 path issues guarded whole-application `WM_CLOSE`; Unity routes and latches F4 but no production owner consumes the close request. Existing Editor physical test validates only the handoff and consumes it diagnostically. No script or resource changed in this audit.

Next executable package: design the exact battle-host close owner and recording/shutdown boundary, then create a separate pre-change Task/Change with named code paths, focused accepted/rejected physical-key tests and real Player close/ordered-shutdown witness. Do not silently substitute return-to-Menu semantics or quit the Unity Editor during test. Q07 Scene closure and Q08 mode-4 result-stage count remain separate dependencies.
