<!-- CHANGE-RECORD
id: NTSD28-Q10-NATIVE-STEREO-CAMERA-TRACE-001
status: VERIFIED
change-kind: CODE
code-path: Tools/NTSD28Q10Diagnostics/native_stereo_camera_trace.cpp
authority: formal NTSD2.8-Logan root EXE and paired playable battle camera, frame sound and stereo matrix source
evidence: docs/ai/TASKS/NTSD28-Q10-NATIVE-STEREO-CAMERA-TRACE-001.md
-->

# NTSD28-Q10-NATIVE-STEREO-CAMERA-TRACE-001

Initial `PLANNED` record before script creation. The Task declares the single new diagnostic code path, formal authority and resource roots, controlled battle/input, read-only original-content boundary, output/acceptance, limitations and rollback. No Unity runtime or nonbattle code changes are authorized by this diagnostic. The expected witness is a per-tick source TSV with actual `camera_x` and source audio-event X; a later root EXE/camera/output bridge remains an independent gate.

2026-09-27 `CODE_WRITTEN`: added only the declared C++ diagnostic source. It constructs controlled mode0 Naruto/Lee human combatants, runs a bounded physical-equivalent right tap/release/hold pattern through `GameSession28::set_input`/`step`, records every completed tick's actor X/action, `camera_x`, formal audio event X/path/source, and an explicitly labeled projection of `audio_backend.cpp::native_stereo_mix28` with locked 333/666/-333 parameters. It refuses to overwrite a prior TSV. Compilation/runtime and source output validation remain pending; no production/authority/resource/Scene file changed.

2026-09-27 `VERIFIED` for the scoped source diagnostic: current root EXE SHA rechecked. MinGW g++ 15.1.0 compiled this source plus all 28 paired core translation units and playable `game_session.cpp`/`selection_flow.cpp` (C++17) with exit0. Initial argument attempts incorrectly pointed to `runtime/vfs` then `runtime/decoded_dat` and failed initialization exit5; all raw logs/TSVs retained. Corrected invocation passes `resources/runtime` for both roots, exit0 with 30 rows. Right tap2/release2/hold naturally emitted frame-sound 003 at source tick6, source X649, camera0, 53/47 and 004 at tick11, X729, camera0, 41/59. These source X values match prior Unity actor source-rule X at selected cue ticks8/13. Stage23 formal background width1330 explains camera0 but is **not** a Unity content candidate. Exact conditions, TSV SHA and root-EXE/audio-output limitations are in ACCEPTANCE. No Unity production/DAT/WAV/Scene/nonbattle modification. O-02/Q10 remain open.
