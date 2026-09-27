# NTSD28-Q10-NATIVE-BATTLE-VOLUME-001

Status: `RUNTIME_PENDING / SCOPED_EDITOR_TESTS_PASS`. Parent `NTSD28-UNITY-BATTLE-REALIGNMENT-001 / BATCH-05 / Q10 / O-03`. This is an independent Q10 effect package; Q07 remains the earliest open group, and Q10 cannot close with this package alone.

Authority: formal root EXE SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033` and its paired playable `source/ntsd28_playable/src/main.cpp:2401-2417,2525-2550,2572-2599`, `audio_backend.cpp:100-118,460-473,689-695`. Detailed read-only first difference: `artifacts/diagnostics/NTSD28-Q10-NATIVE-VOLUME-FIRST-DIFF-20260927/REPORT.md`.

Pre-edit Unity: F11/F12 physical latch and router produced a continuous host command, but `SimulationTickDriver` only stored/exposed it. `NTSDSoundPlayer` played battle SFX with per-cue volume and had no formal 0..100 battle gain or retuning of active pooled voices. Menu `GameLocalSettings` controls are separate. Battle BGM has no identified production voice consumer; its WMA decode/playback and exact gain remain Q10 work outside this script package.

Declared self-authored script paths and symbols:

- `Assets/NTSD/Scripts/Simulation/Host/SimulationTickDriver.cs`: bind successful LocalFreeRun tick completion and successful worker publication to a single volume-command application. Capture the submitted command for worker publication, and do not apply on submission, failed tick, Manual/Lockstep, paused no-step, or an aborted lifecycle.
- `Assets/NTSD/Scripts/App/NTSDSoundPlayer.cs`: keep battle-only percent 0..100 at 100 initially, reproduce formal SFX hundredth-dB mapping and gain, retune pooled active voices when it changes, and apply the same multiplier to newly played battle SFX. Do not modify menu volume or general AudioController.
- `Assets/NTSD/Scripts/Test/Editor/SoundPresentationDispatchEditorTests.cs`: test initial/boundary/held-direction behavior, formal SFX gain samples, already-playing pooled voice retune, and ordinary cue playback at changed volume. Preserve existing allocation and event/checksum tests.

2026-09-27 focused fixture follow-up, before further script edit: the three existing sound-event tests in this same declared file currently construct `TickSoundEmitter` with no DAT frame, `FrameDelay=0` and a non-held link. The Q06 missing-physics-frame guard therefore prevents its overridden `SimTransit` from running; a successful driver tick legitimately has zero fixture events. Update only that synthetic emitter's delay precondition so the sound publication/checksum tests exercise a permitted native-physics pass, then rerun the three named tests and the new volume tests. This fixture correction must not weaken the production guard, alter test expectations, or be counted as battle Play/audio parity.

Expected effect: F11/F12 held changes battle SFX percent once for each successfully completed LocalFreeRun logic tick (including accepted paused F2); F12 wins simultaneous holds. Current active voices are retuned and newly presented voices use the current gain. The source event queue, checksum, world, scene, DAT, and nonbattle settings do not change.

Known residual (corrected after source re-audit): Unity currently batches multiple tick sound events until LateUpdate. Formal playable submits and starts each tick's battle cue after that tick's volume adjustment, then retunes already active voices on later volume changes. Thus the unresolved contract is per-tick voice start/publication plus later active-voice retune, not immutable gain frozen into each event. The follow-on Q10 audio publication/timing package must test that sequence. BGM/WMA and audible device/formal EXE parity also remain. Do not state that O-03 or Q10 is closed from the SFX result.

Validation: establish focused RED before production edit if the existing test runner can execute; then run generated-project compile, original Editor compile/focused tests, targeted actual battle physical F11/F12 and F2 Play when possible, Scene hashes, `git diff --check`, `Tools/Validate-ChangeLedger.ps1`. If the original Editor is unavailable, report the exact lower evidence level and leave runtime pending. Only rerun Q06/Q07 tests if a shared owner changes.

Rollback: review and reverse only this Change ID's exact hunks after confirming the current dirty worktree. Do not restore/reset/clean any pre-existing work. No deletion or content migration is authorized by this Task.
