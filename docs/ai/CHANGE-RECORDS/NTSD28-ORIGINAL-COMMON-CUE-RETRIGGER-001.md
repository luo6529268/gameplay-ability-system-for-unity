<!-- CHANGE-RECORD
id: NTSD28-ORIGINAL-COMMON-CUE-RETRIGGER-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/App/NTSDSoundPlayer.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28OriginalCommonAudioEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28RasenganCommonRegressionEditorTests.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B4State1218EnvironmentCreditEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28OriginalCommonBattleSceneProbeEditor.cs
authority: current user original NTSD.exe reference confirmed and fix common audio; original5EDA captured DirectSound replay 00401A30 and dynamic identity0040BD90
evidence: artifacts/diagnostics/NTSD28-336B44-RASENGAN-COMMON-REGRESSION-20261006/original-common-fix-01/REPORT.md
-->

# NTSD28-ORIGINAL-COMMON-CUE-RETRIGGER-001
Pre-script bounded contract.
Problem: battle events always acquire free pooled voice, repeated shared cue tails overlap. Resource/PCM correct; root336 old backend contains same gap. User clarified this is not acceptable restoration behavior.
Expected: original battle identity retains shared voice, Stop/rewind/Play retriggers; same tick same identity contributions aggregate once; different dynamic cues and built-in vs dynamic identities remain independent. No role/OID/frame/053 special branch. Native DirectSound spatial attenuation/pan used if aggregate mapping is implemented; precise Unity actual matrix verified before claiming.
Paths above only. Existing newdiagnostic expected2 voices must be corrected to expected1 with explicit user-authority correction, historical original336 observations retained. New generic tests first RED with actual clips.
No producer/input/event cadence/DAT change. Resolve any ambiguous builtin cue at actual consumer first; no blanket merger by physicalClip. UI PlaySfx overlap remains outside battle change.
Voice binding owned by existing player, limited existing64 pool, no extra manager/worker/queue. Pool reuse replaces binding; destruction destroys bindings with owner and voice children. Existing ordered teardown stays same.
Validation: generic identity/retrigger/batch/isolation/poolreuse focused RED/GREEN, actual053PCM retained, clip/master/pan boundary; representative other character frame+same common sink; original saved Battle actual skill release, clone hit/common held completion and no dirty/zero residual. No full roster/suite.
Rollback: NTSD28-ORIGINAL-COMMON-AUDIO-CATCH-FIX-20261006 exact before bytes, scoped owned edits only with separate authorization.
Status PLANNED. No production script edited yet.

Pre-change host-adapter declaration 2026-10-06T05:50:05.387058+00:00: actual Unity focused test proves AudioSource.volume clamps sqrt(2) to1; original DirectSound mono center sends original PCM to both channels. Reuse PreparedSoundCue to cache mono-to-stereo duplicated playback PCM during existing battle prewarm (source AudioClip/WAV unchanged), volume/pan use calibrated stereo diagonal mapping. Instance-owned generated clips disposed when cue catalog is replaced and OnDestroy, no World/worker dependency; existing sound player owns lifetime. No new resource file/mixer/manager. Battle cannot create a converted clip after catalog seal; diagnostic unsealed loaded cue may prepare lazily. New test actual doubled-channel data and parent PCM check distinguish source from playback clip. Native matrix output/device remains pending until AudioRenderer/Play evidence. Same id binding common for all roles. No production role cases.

Pre-edit producer identity declaration: hard landing emits raw data016 though it is builtin channel6; use existing SFX_016 alias (same WAV), prewarm both aliases, update exact environment credit cue assertion. Only identity fixes, cadence/damage/state unchanged; additional-before-manifest preserves existing LF2Entity dirty edits.

Before ownership patch: read-only review confirms stale asynchronous load/catalog continuation can create copied PCM after destruction, and partial-copy failures may retain old owned clips until owner shutdown. Add local catalog generation checks after await/before owned creation, destroy guard, and partial-batch cleanup. Fields owned/reset only by existing audio catalog; no new module/thread/shutdown order. Existing production sealed path unaffected.

Pre-create original saved Battle probe: Play-only roster Naruto/Naruto override, controlled P2 authored clone generation272, actual P1 DFJ discrete production inputs then hold and Attack. No Scene serialization/resource edit. Actual production sound publication, same053voice ids/count/channels, actual clone hit→authored catch completion→434 invisible/removed. Other spawned clones parked/AI off for isolation; explicit diagnostic setup, not untouched default scene or original tick-time proof. Existing11-stage shutdown +zeroWorld/slot/pool+cleanScene and private-copy cleanup. One new Editor probe only; no persistent manager/queue.

Current CODE_WRITTEN: production and focused tests imported. Audio first RED6/fixture1, initial GREEN10/10 before ownership guard; catch generic expected RED3/7, full source/projected traces now authored122→262 and434 absent45, first test assertions wrongly expected transient released child and fixture80 instead of native random0..5, corrected with originals retained; last22 run failed5 (two transient assertions/two old fixture80 plus EditMode OnDestroy fixture not called), not claimed PASS. Current narrow corrected run7420cacebc784cf6982d8643dd111c24 pending, original saved Battle probe newly compiled but not yet started. No DAT/Scene/resource changes.

2026-10-06 current focused checkpoint: NTSD28-ORIGINAL-COMMON-CUE-RETRIGGER-001 / FOCUSED_TEST_PASS; NTSD28-ORIGINAL-CATCH-SKILL-COMPLETION-001 / FOCUSED_TEST_PASS. Original Editor selected 32/32 PASS, job7420cacebc784cf6982d8643dd111c24 (8993 is discovery total only); catch11+audio10+environment10+PCM1. Original saved Battle Scene01 running; runtime/output waveform not yet accepted. No DAT/Scene/resource/nonbattle edits or old campaign reopening.

## Current scoped acceptance
2026-10-06 scoped common restoration VERIFIED: NTSD28-ORIGINAL-COMMON-CUE-RETRIGGER-001 / VERIFIED; NTSD28-ORIGINAL-CATCH-SKILL-COMPLETION-001 / VERIFIED. Original Editor32/32 PASS (7420cacebc784cf6982d8643dd111c24), final affected7/7 PASS (2469f52667044d4f979530394ea10d2e); original saved Battle Scene01 PASS87: actual P1 DFJ, enemy owned33 caught77, 434 consumed/invisible87, 05344events one actual voice; no rejected/dropped. Ordered11-stage shutdown objects/slot/borrower0, audio copies/voices0, Scene clean/SHA unchanged. Proposed negative-CaughtSlot guard rejected by prior7 run(twofail), removed; final Cpoint bytes equal Scene/32 version. All evidence retained. Other-role audio sink common; five held-skill candidate DAT families audited, only Naruto actual chain qualifies; not full roster/runtime/GPU/native exact tick or final device waveform parity. PCM stereo payload estimate210MiB, not measured RSS. Protected681/WAV978/backup18/formal336/original5EDA stable. Task NTSD28-336B44-RASENGAN-COMMON-REGRESSION-20261006 / SCOPED_COMMON_FIX_VERIFIED. No DAT/Scene/resource/InputAction/nonbattle edits, file deletion/move, Git discard/commit/push, extra Editor or old goal reopening. Report: artifacts/diagnostics/NTSD28-336B44-RASENGAN-COMMON-REGRESSION-20261006/original-common-fix-01/REPORT.md.
