<!-- CHANGE-RECORD
id: NTSD28-336B44-Q10-KIM-J1-NATURAL-REACH-001
status: FOCUSED_TEST_PASS
change-kind: CODE
code-path: Tools/NTSD28Q10Diagnostics/kim_j1_natural_reach_probe.cpp
authority: 336B44 formal playable OID507 standing hit_Uj310 and frame310 stereo sound
evidence: docs/ai/TASKS/NTSD28-336B44-Q10-KIM-J1-NATURAL-REACH-001.md
-->

# NTSD28-336B44-Q10-KIM-J1-NATURAL-REACH-001

PLANNED before script edit. The existing staged OID507 DAT has the same formally indexed definition as the current release, but static frame links do not establish natural audio reachability. The sole declared code path is an additive, read-only C++ Tools probe using `GameSession28`, physical discrete input and `last_tick().audio_events`. Expected side effects are create-new local CSV/LFR/report files; no formal, Unity production, DAT, WAV, Scene, map, camera or mode change. Acceptance and limited scope are in the Task. Risk: input-phase or HP/MP gates can make a bounded search negative; report the observed branch rather than treating it as global unreachability. Recovery is a forward diagnostic correction with old results preserved. No file deletion is authorized.

Actual code: added `Tools/NTSD28Q10Diagnostics/kim_j1_natural_reach_probe.cpp`; it creates a mode-0 OID507/OID7 playable session, searches a bounded physical defend/depth-up/jump timing grid, records each completed tick and formal audio event, and writes LFR for a selected case. The initial version's tick-end action310 assertion returned exit10 even though the cue fired. DAT frame310 has zero wait and next311; the corrected v2 observes tick-end action311 and retains `run-01` unchanged as a failed probe. No pre-existing script symbol changed.

Validation: formal EXE SHA `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`; current playable closure G++ compile exit0 for v1 and v2; v2 source runs `run-02`/`run-03` exit0 with byte-identical CSV/LFR; formal root EXE quoted headless replay exit0, passed true/failureCode0 and selected 55-tick action/MP/camera 165/165 equal. Formal cue at tick6 is `c/kim/w/j1.wav` (source1, world X500); formal PCM is 2-channel/48kHz/16-bit, 80,454 sample frames. The first unquoted root launch exited10 without report; corrected run passes. Exact artifact and hashes: `artifacts/diagnostics/NTSD28-336B44-Q10-KIM-J1-NATURAL-REACH-001/REPORT.md`. `Tools/Validate-ChangeLedger.ps1` exit0, `git diff --check` exit0 (line-ending warnings only), and four protected disk SHA checks unchanged. Remaining: original Unity Battle Scene clip/voice/L-R device witness and Q10/Q12 whole-stage acceptance. No DAT/WAV/Scene/production mutation.
