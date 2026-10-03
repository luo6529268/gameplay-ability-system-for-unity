<!-- CHANGE-RECORD
id: NTSD28-336B44-Q10-NATURAL-VOICE-PCM-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q10SakuraNaturalStereoPlayModeTests.cs
authority: 336B44 playable audio_backend.cpp native_stereo_mix28 and existing natural Sakura source/root and Unity voice evidence
evidence: docs/ai/TASKS/NTSD28-336B44-Q10-NATURAL-VOICE-PCM-001.md
-->

# Q10 natural battle voice PCM witness

Created before script modification. Current production code presents the formal natural Sakura cue with measured voice pan/volume, while only a temporary synthetic source has been measured through Unity `AudioRenderer`. The Task declares the single existing Editor probe path, input/voice invariants, timing and source-contamination risks, protected files, success and failure tiers, and forward-correction rollback. This diagnostic must not change battle rules, audio production, assets or existing probe menus. Actual code diff and validation will be appended after implementation.

2026-10-03 actual diff: only the declared `NTSD28Q10SakuraNaturalStereoPlayModeTests.cs` changed. Added a new opt-in SakuraPcm menu; the existing Sakura/Kim paths pass a null capture and keep their menu/JSON behavior. The capture starts `AudioRenderer` before the natural physical-input chain, measures frames 4–12 after the production cue callback, records the actual Sfx Mixer group and other playing source count, then stops/restores `Time.captureFramerate` and background setting before Play exit. The WAV reference reads its actual two-channel PCM; no synthesized battle cue enters production.

Generated Editor build `dotnet build Assembly-CSharp-Editor.csproj --no-restore --nologo -v:q -clp:ErrorsOnly`: 0 errors/270 warnings. Original Editor opt-in Battle Scene run `play-20261003-030255-256.json` PASS: Sakura natural event global tick58, production clip 2ch/123466 samples, voice pan −0.6666666865/volume .75, Sfx group, zero other active sources. Software output 14,336 stereo frames/48 kHz gave RMS left .1994152094/right .0655320796; source clip reference left .2762528489/right .2723886918; corrected left/right gain 3.000451681 versus formal target 3.0. `AudioRenderer.Stop` observed, Play exited, Battle then Menu clean, four protected SHA unchanged. Old Sakura/Kim menus were not re-run because their common natural chain was already verified and the new capture is opt-in only. Windows hardware/formal EXE speaker PCM, other cues and dynamic audio-camera policy are still separate Q10 gates. Rollback remains confined to this probe addition; no production, DAT/WAV, serialized Scene, project setting or unrelated dirty file was edited. [Report](../../../artifacts/diagnostics/NTSD28-336B44-Q10-NATURAL-VOICE-PCM-001/REPORT.md).
