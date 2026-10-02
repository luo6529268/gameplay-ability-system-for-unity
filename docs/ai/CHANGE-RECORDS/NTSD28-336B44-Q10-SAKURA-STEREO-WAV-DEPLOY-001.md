<!-- CHANGE-RECORD
id: NTSD28-336B44-Q10-SAKURA-STEREO-WAV-DEPLOY-001
status: FOCUSED_TEST_PASS
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/SoundPresentationDispatchEditorTests.cs
authority: 336B44 formal Sakura natural stereo cue and existing Unity formal battle audio lookup
evidence: docs/ai/TASKS/NTSD28-336B44-Q10-SAKURA-STEREO-WAV-DEPLOY-001.md
-->

# NTSD28-336B44-Q10-SAKURA-STEREO-WAV-DEPLOY-001

PLANNED before script edit. Formal current low-HP Sakura input emits `c/saku/w/tra.wav`; Unity selected formal battle WAV path is missing. Existing `NTSDSoundPlayer.GetOrPrepareCue` already separates deployed formal battle files from generic Sound, and `NTSDResourceLoader.LoadSingleAudioClipAsync` loads WAV through Unity. This Change only extends one existing Editor test script to exercise the stereo cue's actual path, channel/frame count and voice; resource insertion is exactly one new WAV with required new folder/WAV metas. Expected side effects are a Unity import and independent diagnostic outputs, not changes to Battle Scene or runtime code.

Task specifies source, protected files, focus verification, unverified Scene/device exits and forward-correction rollback. Actual edited symbols, copied SHA, compiler/Editor tests, failed attempts, protection checks and validator result will be appended after execution. No production camera/stereo policy is authorized here.

## 2026-10-02 implementation and bounded evidence

- Added only `Assets/NTSD/Content/LoganRuntime/vfs/c/saku/w/tra.wav` plus Unity-generated `w.meta` and `tra.wav.meta`; deployed and current formal source SHA-256 are both `A6D36A499DBEAB218690BC3FDEE5071E5B97165D5DB34427116A3CDE8660EBC7`. New WAV meta has `forceToMono: 0`; its GUID `317975f5495b9ad4ebad2e5708278e7a` occurs in one asset meta.
- Edited only `SoundPresentationDispatchEditorTests.FormalBattleWav_UsesReleaseFileAndKeepsGenericCueOnOriginalPath` and `FormalBattleWav_DecodesThroughBattlePlayerLoader`: existing `data/020.wav` and `data/067.wav` cases remain, and the Sakura case checks formal battle path, original generic Sound route, two channels, 123466 samples, sealed cache, loader and real pooled battle voice. No production script, DAT, old Sound, Scene, Prefab or camera edit.
- `dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:q -clp:ErrorsOnly`: exit 0, 0 errors, 253 warnings; output retained in the [report directory](../../../artifacts/diagnostics/NTSD28-336B44-Q10-SAKURA-STEREO-WAV-DEPLOY-001/).
- Original Unity Editor focused EditMode job `9cb96e87d9a14501a081bc39a4fdf9ee`: result 2/2 Passed, 0 failed, 0 skipped. The 8779 value in job progress is discovered test inventory, not executed test count. After the job, the original Editor was idle, non-Play, compiling=false, Battle Scene `isDirty=false`.
- Before/after six listed protected paths have zero SHA differences; the two previously deployed formal WAV SHA are stable. The GameConfig path in that list is absent in both snapshots, represented as null rather than claiming an existing asset was hashed.
- `Tools/Validate-ChangeLedger.ps1`: exit 0, PASSED, edited test path `COVERED` by this ID; historical declaration warnings retained in the report log. `git diff --check`: exit 0 with line-ending notices only. Natural Sakura battle event through Unity Scene, exact voice timing, stereo left/right sample output, formal root device sound and fixed-background panning policy are **not yet verified**. This is a focused test exit only; Q10/Q12 stay open.
