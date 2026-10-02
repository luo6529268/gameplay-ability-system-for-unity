# Q10 Sakura formal stereo WAV: bounded deployment and original-Editor test

Date: 2026-10-02. Change: `NTSD28-336B44-Q10-SAKURA-STEREO-WAV-DEPLOY-001`. Status: `FOCUSED_TEST_PASS / SCENE_PENDING`. Parent: `BATCH-05 / Q10`. The only current battle authority is root formal EXE SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` and its playable live source. [Previous natural event witness](../NTSD28-336B44-Q10-SAKURA-STEREO-NATURAL-REACH-001/REPORT.md) established a bounded Sakura event at tick24; this report covers only the selected file's Unity deployment and focused consumption.

## Exact change

- Copied current formal `resources/runtime/vfs/c/saku/w/tra.wav` to `Assets/NTSD/Content/LoganRuntime/vfs/c/saku/w/tra.wav`. Source and deployed SHA-256: `A6D36A499DBEAB218690BC3FDEE5071E5B97165D5DB34427116A3CDE8660EBC7`; deployed length 494014 bytes. Formal WAV: PCM 44.1 kHz, 16-bit, 2 channels, 123466 sample frames.
- Original Unity Editor import generated only the new `w.meta` and `tra.wav.meta`. AudioImporter `forceToMono: 0`; WAV GUID `317975f5495b9ad4ebad2e5708278e7a` occurs in exactly one asset meta.
- Extended two existing tests in `SoundPresentationDispatchEditorTests.cs`. The same test loops still cover prior formal `data/020.wav` and `data/067.wav`. Added the Sakura path to both. The route test checks separate formal battle and original generic Sound roots, imported channel/sample metadata and cache after seal. The decode test uses the existing `NTSDSoundPlayer` and resource loader, then verifies a pooled battle AudioSource voice references the loaded two-channel clip. No cue-specific production branch was added.

## Verification

| Check | Result | Scope |
|---|---|---|
| Formal vs deployed WAV SHA-256 | Equal | Exact selected file only; other WAV files unchanged. |
| Generated Editor C# project build | Exit 0, 0 errors, 253 warnings | `dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:q -clp:ErrorsOnly`; [output](generated-editor-build-v1.txt). |
| Original Unity Editor focused EditMode job `9cb96e87d9a14501a081bc39a4fdf9ee` | 2 passed, 0 failed, 0 skipped; `resultState=Passed` | `FormalBattleWav_UsesReleaseFileAndKeepsGenericCueOnOriginalPath` and `FormalBattleWav_DecodesThroughBattlePlayerLoader`. Job discovery listed 8779 tests; only 2 were executed. |
| Original Editor after tests | Idle, not Play, not compiling; active `NTSD_Battle` Scene `isDirty=false` | Unity MCP `get_editor_state` and `manage_scene/get_active`. |
| Six protected path snapshots | 0 before/after SHA differences | [before](protected-before.json), [after](protected-after.json); one listed GameConfig path was absent in both and is represented by null. Other five existing paths include Battle/Menu Scene, mode asset and previously deployed 020/067 WAVs. |
| New WAV GUID lookup | One meta path | `rg -l -F 317975f5495b9ad4ebad2e5708278e7a Assets -g '*.meta'`. |
| Change Ledger validator | Exit 0, `Change ledger validation PASSED`; current edited test path `COVERED` by this Change ID | [Full log](change-ledger-validation.txt). Existing warnings concern historical records naming files outside the current diff. |
| `git diff --check` | Exit 0 | No whitespace error; Git emitted only line-ending conversion warnings for existing markdown/test paths. |

No DAT, prior resource, Sound-root file, production script, Scene, Prefab, camera or nonbattle audio route was modified. This focused success does **not** establish natural Sakura Scene playback timing, actual stereo left/right output, device sound or formal EXE speaker equivalence. The fixed-background panning decision remains open. Q10 and Q12 stay open; the next independent package should replay the HP100 key sequence in the original Battle Scene and inspect the pending cue, voice and two-channel output without widening this resource deployment.

Deletion audit: no files deleted or moved. If a future task removes these additions, it must record exact paths and obtain the project's required deletion approval.
