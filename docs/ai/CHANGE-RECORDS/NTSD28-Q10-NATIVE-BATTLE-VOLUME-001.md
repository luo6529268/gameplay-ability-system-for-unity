<!-- CHANGE-RECORD
id: NTSD28-Q10-NATIVE-BATTLE-VOLUME-001
status: RUNTIME_PENDING
change-kind: CODE
code-path: Assets/NTSD/Scripts/Simulation/Host/SimulationTickDriver.cs
code-path: Assets/NTSD/Scripts/App/NTSDSoundPlayer.cs
code-path: Assets/NTSD/Scripts/Test/Editor/SoundPresentationDispatchEditorTests.cs
authority: formal NTSD2.8-Logan playable main.cpp advance_native_volume and audio_backend.cpp active SFX gain
evidence: artifacts/diagnostics/NTSD28-Q10-NATIVE-VOLUME-FIRST-DIFF-20260927/REPORT.md
-->

# NTSD28-Q10-NATIVE-BATTLE-VOLUME-001

Status `RUNTIME_PENDING` after test-first RED, the declared production implementation and scoped original Editor tests. Exact scope, authority, first difference, invariants, tests, residual Q10 work, and rollback are in `docs/ai/TASKS/NTSD28-Q10-NATIVE-BATTLE-VOLUME-001.md`.

The pre-existing dirty `SimulationTickDriver.cs` contains Q08 stage-gate changes around configuration and teardown. This Change must preserve those hunks and touch only the declared host/audio path. `NTSDSoundPlayer.cs` and the declared Editor test are currently tracked clean. No production code or test has been edited under this ID yet.

2026-09-27 test-first checkpoint: added one focused Editor test on the declared test path. Original project Editor PID11944 (idle Menu, non-Play) imported it and job `bd7583127fa1470bbedda374ff2e1c94` executed exactly one named test; it failed at the missing `NativeBattleVolumePercentForDiagnostics` property (`Expected: not null / But was: null`). This is the expected pre-fix runtime RED; no production script was edited before it.

2026-09-27 implementation checkpoint: `NTSDSoundPlayer` now owns the persistent battle-only percentage initialized to 100, computes the formal integer SFX attenuation and gain, stores base volume for each pooled voice, retunes those voices on a percent change and applies gain on new plays. `SimulationTickDriver` now applies the held command after successful LocalFreeRun main-thread ticks, captures it at accepted worker submission and applies it on matching publication, and clears the pending worker command with function-key routing state. No menu settings, DAT, media, Scene, general AudioController, world/checksum, or nonbattle path changed. The existing Q08 stage-gate Driver hunks are preserved. Status `CODE_WRITTEN`; compilation, GREEN, physical Play, per-tick sound publication timing, BGM/WMA, audible parity and broader Q10 exit remain pending.

2026-09-27 original Editor validation: refreshed assemblies and targeted jobs `632f3c4b9dd040d19d335de441a56180` (new volume/host tests plus existing voice saturation 3/3) and `c1209707806a463c8f0738d0729e128b` (physical latch/router 2/2) passed. Three other old sound dispatch/catch-up cases failed because their fixture emitted zero queued events; no touched method owns `TickSoundEmitter.SimTransit`, but the exact current cause is not yet proven. Raw job IDs, failed assertions, scope, Scene hashes, diff/Ledger results and residual Q10 gates are in `artifacts/diagnostics/NTSD28-Q10-NATIVE-BATTLE-VOLUME-001/ACCEPTANCE-PENDING.md`. This is `RUNTIME_PENDING`, not O-03/Q10 completion.

2026-09-27 paused F2 extension: the declared host test now asserts no change while paused without a step, one decrease after `ProcessHostControlCommandsForDiagnostics` accepts F2, and no further change on a Manual tick. Original Editor recompiled; exact job `7c9b3b8363c04878b172546fd369bb3c` passed 1/1. `git diff --check` and Change Ledger validator both exited 0 again after this last script edit. Physical keyboard Play, worker publication and residual Q10 items remain pending.

2026-09-27 pre-edit sound fixture follow-up: the existing three dispatch/catch-up failures all depend on synthetic `TickSoundEmitter.SimTransit`. Its constructor has no native frame and the Q06 guard in `LF2Entity.ExecuteNativePhysicsForWorldPass` exits when `FrameDelay==0 && LinkState>=0 && GetNativeFrameDataById(...)==null`. This predicts zero events without implying a production sound regression. The declared test file alone may give this emitter a positive delay covering its three test ticks; exact assertions and production code remain unchanged. Re-run the three old tests plus new volume tests in the original Editor; retain FAIL if the prediction is wrong.

2026-09-27 fixture correction and verification: set synthetic `TickSoundEmitter.FrameDelay=4` only in the declared Editor test file, keeping it positive through the three sound-publication ticks. Original Editor compiled/domain-reloaded, then job `064bee20b94e4ed1a752f76f7255edc8` passed all three formerly failing dispatch/suppression/catch-up cases; job `070214f87eb14ce28aa6bbeb597adc4e` passed both new native volume cases after the edit. The production Q06 missing-frame guard and all sound assertions remain unchanged. This repairs test reachability, not physical Battle Play, worker, two-tick cue-gain, BGM/WMA or audible parity; status remains `RUNTIME_PENDING`.

2026-09-27 physical Play follow-up: independent test-only Change `NTSD28-Q10-NATIVE-BATTLE-VOLUME-PHYSICAL-PLAY-001` passed the original Editor Menu→formal Battle physical F11/F12/F2 SFX-percent gate (tick2/100→3/99→4/100, paused/release holds), unloaded Battle and exited Play with saved Scene hashes stable. This raises the SFX host path to a bounded physical Play evidence layer. The production package remains `RUNTIME_PENDING` because worker publication, same-Update per-cue gain, BGM/WMA and audible/formal parity have no equivalent exit evidence.

2026-09-27 audio timing correction: paired playable `main.cpp` submits battle events and starts voices after volume adjustment on each successful tick, while later volume changes retune all active voices in `audio_backend.cpp`. Unity batches tick events until LateUpdate. The missing behavior is per-tick voice start/publication and subsequent retune, not permanently distinct per-cue gain. See `artifacts/diagnostics/NTSD28-Q10-AUDIO-PER-TICK-PUBLICATION-AUDIT-20260927/REPORT.md`; previous shorthand is superseded, package status remains `RUNTIME_PENDING`.
