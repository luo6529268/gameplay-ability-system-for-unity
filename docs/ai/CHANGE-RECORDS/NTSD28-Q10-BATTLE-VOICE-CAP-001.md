<!-- CHANGE-RECORD
id: NTSD28-Q10-BATTLE-VOICE-CAP-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/App/NTSDSoundPlayer.cs
code-path: Assets/NTSD/Scripts/Test/Editor/SoundPresentationDispatchEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q10SelectedRunCuePlayProbeEditor.cs
authority: formal paired playable audio_backend.cpp XAudio2Backend28::submit 64-active oldest eviction and user nonbattle-preservation boundary
evidence: docs/ai/TASKS/NTSD28-Q10-BATTLE-VOICE-CAP-001.md
-->

# NTSD28-Q10-BATTLE-VOICE-CAP-001

Pre-script record. Formal playable accepts the 49th through 64th simultaneously active SFX and evicts the oldest only on accepting the 65th; Unity's current 48 desktop/24 mobile shared pool instead drops new cues once full. This is a source-confirmed conditional first difference, not yet a natural-gameplay observation. The Task defines a battle-only 64-slot policy while preserving direct generic `PlaySfx`'s configured cap and old drop behavior, bounded exact paths, test-first saturation/ordinary-cue tests, original Editor Play, protected assets and rollback. No script edit or runtime result is claimed here.

Test-first code written: `SoundPresentationDispatchEditorTests.BattleVoicePool_AcceptsSixtyFiveAndEvictsOldest_WithoutChangingGenericCap` prepares two in-memory clips, submits 65 battle events, asserts 65 accepted starts/64 slots/oldest evicted and then checks direct generic `PlaySfx` still drops when its original logical cap is full. Original Editor RED execution and production changes remain pending.

Original Editor test-first RED job `22d4b25d19484b02827b005ddc6f21a8` failed 1/1 at the intended first assertion: 65 accepted battle event starts expected, only 48 observed. The raw structured result is `artifacts/diagnostics/NTSD28-Q10-BATTLE-VOICE-CAP-001/red-job.json`. Generated Editor project compiled the new test with 0 errors/199 warnings. This is test-only evidence before production edits; GREEN, natural Play and nonbattle preservation still pending.

Production code written only in declared `NTSDSoundPlayer.cs`: `PresentSound(s)` carries an explicit battle-event flag through loaded/async cue dispatch; direct generic `PlaySfx` passes false. Battle prewarm expands the existing voice array to 64 without discarding the original 48 GameObjects or their state; generic calls still search their configured first 48/24 logical slots. Every accepted play records a sequence. When battle slots are all busy, the new event reuses the smallest sequence (oldest accepted) and increments the existing old-voice drop metric; generic full-pool behavior remains reject-new. Cue lookup/guard, volume, pitch, spatial settings and queue ordering are unchanged. Generated compile, GREEN, adjacent allocation test and natural Play pending.

The declared selected-run Editor probe now has a separate opt-in Voice Cap menu variant with unique `NTSD28-Q10-BATTLE-VOICE-CAP-001/play-result.json`. It reuses the natural Menu→Battle/physical-D two-cue sequence and checks that the production prewarm provided 64 battle voice slots before cue playback. Earlier fixed diagnostic result paths remain untouched. Original Editor compilation/Play pending.

Scoped verification: generated Editor build after final probe edit passed (0 errors/199 warnings). Original Editor focused GREEN `d310a556269e4287ad7c6cf8a627968e` passed 1/1, and adjacent no-allocation/unknown-rejection `83c6831563ba4c8f91224d8051c6a274` passed 1/1. The original saved Menu→Battle natural physical-D Play reported `PASS`: prewarmed pool64, 003 tick8 WorldX649 and 004 tick13 WorldX729, both prepared voices playing, Battle unloaded. The Editor returned idle/non-Play, Menu clean; Menu/Battle/Input/GameConfig/project-mode Asset SHA-256 before/after matched. `git diff --check` exited 0. Raw results and limits are in `artifacts/diagnostics/NTSD28-Q10-BATTLE-VOICE-CAP-001/ACCEPTANCE.md`. This verifies the conditional saturation policy and a normal two-cue regression, not natural 65-voice reachability, mobile device output, stereo matrix, BGM or Q10 aggregate. Ledger validator pending below.

Final governance: `Tools/Validate-ChangeLedger.ps1` exited 0 (`Change ledger validation PASSED`, 970 Records/37 governed code files in the existing dirty diff; unrelated historical declaration warnings remain). No DAT, Scene, Asset, Input or nonbattle file was edited in this package. `git diff --check` passed.
