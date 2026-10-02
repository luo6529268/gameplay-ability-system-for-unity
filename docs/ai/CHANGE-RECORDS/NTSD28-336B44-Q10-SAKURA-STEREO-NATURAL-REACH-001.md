<!-- CHANGE-RECORD
id: NTSD28-336B44-Q10-SAKURA-STEREO-NATURAL-REACH-001
status: VERIFIED
change-kind: CODE
code-path: Tools/NTSD28Q10Diagnostics/sakura_stereo_natural_reach_probe.cpp
authority: 336B44 formal Sakura frame and playable input/audio/LFR live path
evidence: docs/ai/TASKS/NTSD28-336B44-Q10-SAKURA-STEREO-NATURAL-REACH-001.md
-->

# NTSD28-336B44-Q10-SAKURA-STEREO-NATURAL-REACH-001

PLANNED before script edit. Current Unity production has no selected stereo `c/saku/w/tra.wav` in either battle sound lookup root, but no natural event has yet proved that this is player-observable. The declared change is a read-only, standalone C++ diagnostic in the exact Tools path above. Its only side effects are create-new local CSV/LFR and later formal-root playback report/trace files; formal inputs and the Unity project remain unchanged. It will test a bounded action0 physical-button sequence, record the playable event, and compare the same LFR action/MP/tick at the official EXE boundary. A successful source event does not itself prove Unity silence or output-channel fidelity.

Actual symbols, timing cases, compile/runtime results, failed cases, unverified exits and validator result will be appended after implementation. Task defines acceptance and rollback. No production repair is authorized by this record.

2026-10-02 actual script: added only `Tools/NTSD28Q10Diagnostics/sakura_stereo_natural_reach_probe.cpp`, with bounded 36-case defend/up/attack timing search, official Session audio-event observation and LFR recording; no production script changed. `run_case` uses Sakura action0/HP100 and a distant Lee, selects the first exact 240→165→172→new Jump→340 plus `c/saku/w/tra.wav` positive, and writes create-new CSV/LFR. HP500 first exposed the formal `state:1150145` redirect to145; subsequent HP100 positive is explicitly reported. The scope, resource roots and failure logs are in [REPORT](../../../artifacts/diagnostics/NTSD28-336B44-Q10-SAKURA-STEREO-NATURAL-REACH-001/REPORT.md).

Validation: v1 wrong compile include and v2 missing Windows entry linkage retained; v3/v4/v5 compile exit0/stderr0. Source runs 05/06 both exit0 with tick24 stereo event and identical grid/CSV/LFR SHA. Current formal root SHA remained 336B44…7BD3; official root LFR run06 explicit process exit0, `passed=true/failureCode=0`, twice identical trace SHA. Source/root selected action/MP/camera comparison 165/165 and no difference. Report's `nativeParityClaim=false` and extra host tick56 retained; root trace has no audio event column. Actual Unity clip/voice/speaker output and Q10 aggregate remain unverified. No deletions, resource copy or Scene modification.

Final audit: `Tools/Validate-ChangeLedger.ps1` exit0/PASSED, 1152 Records/29 governed code paths; sole new probe is `COVERED` by this Change ID. [Full validator output](../../../artifacts/diagnostics/NTSD28-336B44-Q10-SAKURA-STEREO-NATURAL-REACH-001/change-ledger-validation.txt) retains unrelated historical warnings. `git diff --check` exit0; line-ending warnings are informational. This task's source runs and official root reports are scoped diagnostic evidence, not a Unity Q10 completion certificate.
