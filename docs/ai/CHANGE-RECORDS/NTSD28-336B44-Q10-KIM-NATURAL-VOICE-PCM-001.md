<!-- CHANGE-RECORD
id: NTSD28-336B44-Q10-KIM-NATURAL-VOICE-PCM-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q10SakuraNaturalStereoPlayModeTests.cs
authority: 336B44 formal stereo mix and already proven Kimimaro natural cue in original Battle Scene
evidence: docs/ai/TASKS/NTSD28-336B44-Q10-KIM-NATURAL-VOICE-PCM-001.md
-->

# Q10 Kimimaro natural battle voice PCM witness

Created before modifying the declared Editor test script. The existing original Scene test reaches Kimimaro's formal `j1.wav` as a playing stereo production voice, but no PCM from that voice has been measured through the project Mixer. The Task defines the exact one-file opt-in extension, formal 3:1 target, original Scene safety checks, validation, remaining limits and forward-correction rollback.

Expected side effect: one new Editor diagnostic menu entry and one independent result directory; the existing three menu variants, production gameplay and saved assets retain their current behavior. The script change must reuse existing `PcmCapture` and cleanup, with no new battle/audio service or generalization. After implementation append the actual diff, generated/original Editor results, protected hashes, failures and remaining risks before raising this status.

2026-10-03 actual diff and verification: in the declared `NTSD28Q10SakuraNaturalStereoPlayModeTests.cs`, added one Kimimaro PCM `MenuItem`/variant, routed that variant through the existing Kimimaro physical-input chain and existing opt-in `PcmCapture`, and assigned a distinct result directory. No production or content file changed under this ID. `dotnet build Assembly-CSharp-Editor.csproj --no-restore --nologo -v:q -clp:ErrorsOnly` passed with 0 errors/270 warnings; original Editor assembly timestamp advanced beyond script modification. Original clean Battle Scene opt-in Play result `play-20261003-033009-390.json` is PASS: formal cue at global tick40/action311, production Sfx voice pan -2/3/volume .75, no other playing source, 14,336 stereo frames/48kHz and source-normalized L/R gain `3.000156898958169:1` against formal 3:1. Capture stopped, Play exited, prior clean Menu restored; Battle/Menu/GameConfig/mode Asset SHA remained at the protected values recorded in [Report](../../../artifacts/diagnostics/NTSD28-336B44-Q10-KIM-NATURAL-VOICE-PCM-001/REPORT.md). Two MCP disposed-client errors appeared during parallel bridge reads; sequential bridge state and runtime test succeeded. Formal EXE speaker, Windows hardware, other cues, async path and dynamic audio-camera policy remain unverified; Q10/Q12 are open. Rollback remains a forward correction of the declared Editor probe variant only, preserving result evidence.
