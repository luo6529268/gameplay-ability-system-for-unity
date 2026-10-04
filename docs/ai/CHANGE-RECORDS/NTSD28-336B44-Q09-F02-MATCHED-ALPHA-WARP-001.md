<!-- CHANGE-RECORD
id: NTSD28-336B44-Q09-F02-MATCHED-ALPHA-WARP-001
status: VERIFIED
change-kind: CODE
code-path: Tools/NTSD28Q07Diagnostics/f02_pickup_throw_entry_probe.cpp
authority: 336B44 formal root and matching playable GameSession snapshot, presentation interpolation, D3D11 offscreen path
evidence: docs/ai/TASKS/NTSD28-336B44-Q09-F02-MATCHED-ALPHA-WARP-001.md
-->

# F02 matched-alpha playable WARP witness

Created before editing the declared C++ diagnostic. Current tool's `--offscreen-tick39` renders the current snapshot without interpolation; original Unity Battle Scene `CentralOnly` uses a render-time interpolated command. Saved geometry shows the apparent weapon X difference can be explained by a `-30` source-pixel display delta, but no corresponding WARP image or direct alpha value exists. Only the diagnostic tool is in scope; production gameplay, formal release, Unity scripts/Scene, DAT, images, audio and nonbattle files are excluded.

Expected change: one independent opt-in captures tick38 and tick39 snapshots, calls the existing playable interpolation at alpha0.26, asserts adjacent identity and slot2 delta, writes geometry/PNG to a new path. Existing flags stay unchanged. Acceptance is a clean build, opt-in exit0 and correct geometry/PNG, and old no-flag output regression byte-identical to the prior formal BG1/Z400 baseline. The existing COM lifetime pattern is retained. Task and rollback: [Task](../TASKS/NTSD28-336B44-Q09-F02-MATCHED-ALPHA-WARP-001.md). Status is `IN_PROGRESS`; no script has yet been edited under this ID.

2026-10-03 code written: only `Tools/NTSD28Q07Diagnostics/f02_pickup_throw_entry_probe.cpp` changed. Added independent `--offscreen-interp-tick39`, retained tick38 raw snapshot, used current playable `sample_render_presentation28` and `interpolate_render_snapshot28` at alpha0.26, asserted slot2 `delta.x=-30` and interpolated left530, then emitted new geometry/summary/PNG in the caller's unique output directory. The old raw WARP branch is untouched, and renderer destruction remains before COM uninitialization. Build, run, old-command regression, PNG validation and live Unity matched-alpha comparison are still pending. No formal or Unity production code, DAT, assets, Scene or nonbattle behavior changed by this code edit. Status `CODE_WRITTEN`.

2026-10-03 scoped verification: `build-20261003-01` g++ against current playable closure including production `presentation_interpolation.cpp` exit0/no diagnostics; new alpha0.26 run exit0, valid 1333×730 WARP PNG SHA `F89473EA85EE411465027BEBA5B4640FC7A23D0854AEFA079466B02E143B4A86`, five formal sprites, slot2 center584→554. Old no-flag and old raw-WARP flag each exited0 in new unique directories. Each of three runs' summary/source/opponent/relation/LFR five files matches previous BG1/Z400 baseline byte-for-byte; old raw geometry and PNG also byte-identical. Existing original Unity tick39 saved command gives relative weapon-vs-Naruto/Tayuya output-pixel residual −0.36/−0.89 after native-to-Unity ratio. Actual Unity `displayAlpha` was not independently logged, and no fresh Editor Play or root EXE GPU Present ran; those and wider Q09/Q12 stay open. Only the declared diagnostic C++ path changed. Status `VERIFIED` for this bounded tool/geometry witness. [Report](../../../artifacts/diagnostics/NTSD28-336B44-Q09-F02-MATCHED-ALPHA-WARP-001/REPORT.md).
