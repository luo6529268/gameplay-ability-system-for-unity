<!-- CHANGE-RECORD
id: NTSD28-336B44-Q10-C053-MONO-MATRIX-CALIBRATION-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q10AudioRendererMatrixCalibrationEditor.cs
authority: 336B44 formal playable C053 mono stereo matrices and existing Unity 2022.3.62f3 AudioRenderer calibration
evidence: docs/ai/TASKS/NTSD28-336B44-Q10-C053-MONO-MATRIX-CALIBRATION-001.md
-->

# NTSD28-336B44-Q10-C053-MONO-MATRIX-CALIBRATION-001

PLANNED before script edit. Current Unity behavior: the old Editor-only calibration script has a single historical B1E13 menu run with five hard-coded cases and a protected v3 result path. The actual battle player still sets `panStereo=0`. The Task fixes the sole script path, six current formal target rows, expected temporary Editor/AudioRenderer side effects, protected user Scene boundary, focused acceptance and forward-correction rollback. The intended change adds a separate current-version menu invocation while preserving the historical invocation and its result.

Actual script symbols, compile/capture results, failed attempts, protected Scene hashes and unverified exits will be appended after execution. No production runtime code or content is in this Change.

2026-10-02 CODE_WRITTEN: in the exact declared Editor-only script, added `CurrentC053MenuPath`, `CurrentC053Cases` (two controls and six distinct current-version matrices), `RunCurrentC053` and `RunCases`. The original `Run()` still selects the original five cases and protected `calibration-v3.json`; shared capture/update/cleanup reads `activeCases`. New output uses a UTC timestamped no-overwrite JSON path. No production audio/event/asset/Scene/DAT code changed. `git diff --check` passes for the target script and docs; original Editor compile, Menu Play AudioRenderer capture and Scene hash verification remain pending. A failed sample must remain a separate JSON result, not be overwritten.

2026-10-02 VERIFIED for controlled Unity mono output only: generated Editor project build exit0/0 errors/284 warnings; original Editor assembly fresher than source after MCP refresh. The original Editor opened clean Menu, ran the new menu in Play, and wrote immutable `calibration-20261002-053649-970.json` SHA-256 `A957DFA23992010EE3FE39354EF85F23C46557FE2C4BD8309EDB7ECEC10C903B`: `CAPTURE_COMPLETE`, stereo 48 kHz, eight cases each >=29,696 frames, six current 336B44 target rows max channel error 0.0010538101 (<0.002). `captureStopped=true`; Editor stopped Play and returned to clean Battle. Menu/Battle scene SHA before/after unchanged at `DD6A48A3…B9DC3` / `93448372…7BF60`, historical v3 JSON SHA unchanged. `Tools/Validate-ChangeLedger.ps1` PASSED 1151 Records; focused `git diff --check` exit0. Read-only `execute_code` query failed with MCP/Mono long filename; it was not the calibration route. Voice-level production output, all cues, stereo files, fixed-camera policy and Q10 remain unverified. Full report: [ACCEPTANCE](../../../artifacts/diagnostics/NTSD28-336B44-Q10-C053-MONO-MATRIX-CALIBRATION-001/ACCEPTANCE.md).
