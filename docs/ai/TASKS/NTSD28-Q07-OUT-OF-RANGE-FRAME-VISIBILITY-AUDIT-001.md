# NTSD28-Q07-OUT-OF-RANGE-FRAME-VISIBILITY-AUDIT-001

Status: VERIFIED_STATIC_VISIBILITY_SCOPE_ONLY. Parent: BATCH-04 / Q07 and R17.

Authority: root NTSD2.8-Logan.exe SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`, paired playable `dat_parser.cpp`, `render_snapshot.cpp`, and formal `resources/runtime/decoded_dat`. The prior 330-DAT frame census identified 84 non-999 base pics outside native cumulative sheet capacity; this task verifies whether current production visual offsets could turn those into selected sprites.

Scope: read-only recomputation of cumulative `row*col` capacity, formal/staged DAT bytes, all 84 recorded `(DAT, frame, pic)` rows, the shipped picture-offset producers, and Unity's selected-pic guard. Report a visibility disposition without modifying DAT values, PNGs, scripts, scenes, the original resources, or other project behavior. Preserve the six separate in-range source rectangles that exceed PNG image bounds as an independent Q07/R17 gate.

Acceptance: exact row and DAT counts, formal/staged SHA agreement, maximum capacity over all indexed DATs, nonnegative production offset proof, and an explicit statement of what static source evidence cannot establish about natural frame reachability or root-EXE pixels. Evidence: `artifacts/diagnostics/NTSD28-Q07-OUT-OF-RANGE-FRAME-VISIBILITY-AUDIT-001/REPORT-20260926.md`.
