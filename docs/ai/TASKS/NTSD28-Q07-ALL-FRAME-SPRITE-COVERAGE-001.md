# NTSD28-Q07-ALL-FRAME-SPRITE-COVERAGE-001

Status: `VERIFIED_STATIC_SCOPE_ONLY`. Parent `NTSD28-UNITY-BATTLE-REALIGNMENT-001` / BATCH-04 Q07, with Q09/R17 pixel follow-up. This is a read-only formal-content and current Unity source audit; no Change Record is needed because no code, asset, DAT, image or Scene is edited.

Scope: enumerate staged decoded DAT containing numeric frame headers and BMP sections; verify same-path formal DAT SHA; parse each source file line and each frame base `pic`; use the formal playable `DatParser::project_sprite_sheet` cumulative `row*col` ranges (not authored `file(first-last)` text); verify all referenced PNGs exist and match the formal VFS bytes; map in-range base pics to source rectangles and actual dimensions. Compare static Unity range/catalog/clipping behavior. Save exact counts and anomalies in a machine-readable artifact.

Exit: complete no-error census, distinct hidden-sentinel/out-of-range/bounds cases, authority and Unity source anchors, and explicit limits: base pic only, not dynamic visual offset, selected runtime path, frame visibility, CPU/GPU image publication, or formal EXE same-state pixels. No old-resource deletion authorization. Evidence: `artifacts/diagnostics/NTSD28-Q07-ALL-FRAME-SPRITE-COVERAGE-001/REPORT-20260925.md` and `frame-sprite-static-audit-20260925.json`.
