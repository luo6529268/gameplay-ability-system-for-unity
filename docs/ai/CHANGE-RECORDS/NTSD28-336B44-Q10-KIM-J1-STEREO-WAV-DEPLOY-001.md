<!-- CHANGE-RECORD
id: NTSD28-336B44-Q10-KIM-J1-STEREO-WAV-DEPLOY-001
status: FOCUSED_TEST_PASS
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/SoundPresentationDispatchEditorTests.cs
authority: current 336B44 formal natural Kimimaro j1.wav event and existing Unity battle audio resolver
evidence: docs/ai/TASKS/NTSD28-336B44-Q10-KIM-J1-STEREO-WAV-DEPLOY-001.md
-->

# NTSD28-336B44-Q10-KIM-J1-STEREO-WAV-DEPLOY-001

PLANNED before script edit. The current formal OID507 naturally emits stereo `c/kim/w/j1.wav` at tick6, but the selected LoganRuntime VFS path is absent from Unity. This Change adds that one formal WAV with required Unity metas and extends only two existing focused Editor test methods for path separation, importer metadata, loader, cache and actual battle voice. Existing 020/067/Sakura cases and non-battle Sound route remain protected. No production script, DAT, Scene, Prefab, camera or UI change is planned. Exact source SHA, validation, risk, rollback and unverified exits are specified in the Task. If original Editor compilation or dirty Scene makes tests unsafe, leave runtime validation pending and preserve the Scene. No deletion is authorized.

## 2026-10-02 implementation and current evidence

- Added exact formal `Assets/NTSD/Content/LoganRuntime/vfs/c/kim/w/j1.wav` (321860 bytes), new `w.meta` GUID `718126b204fc48c1a57f1a6feda5a747` and `j1.wav.meta` GUID `ed31f60bd94f4724b957f661b6325288` with `forceToMono: 0`. Formal and deployed WAV SHA-256 match: `86A7F48012EF4A2C1E3E578CCCE37E95F033D1BEEB7C47A8444B651A3D33BD65`. File/folder/metas were all absent before adding them; WAV GUID occurs once in current Assets metas.
- Extended only `SoundPresentationDispatchEditorTests.FormalBattleWav_UsesReleaseFileAndKeepsGenericCueOnOriginalPath` and `FormalBattleWav_DecodesThroughBattlePlayerLoader` with Kimimaro 2-channel/80454-sample case. Existing 020/067/Sakura cases remain. Old generic Kimimaro path remains absent; no production audio branch or old Sound edit.
- `dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:q -clp:ErrorsOnly`: exit0, 0 errors, 253 warnings. `Tools/Validate-ChangeLedger.ps1`: exit0/PASSED and edited test path covered by this ID. `git diff --check`: exit0, line-ending notices only. Formal 020/067 and Sakura WAV SHA unchanged; Battle/Menu Scene and mode asset four protected hashes unchanged. Exact evidence and unresolved items are in `artifacts/diagnostics/NTSD28-336B44-Q10-KIM-J1-STEREO-WAV-DEPLOY-001/REPORT.md`.
- Original Unity Editor import, actual `AudioClip` metadata, loader/pooled-voice focused tests, Battle Scene natural voice and device L/R output remain **unverified**. Live `manage_scene/get_active` returned in-memory Battle Scene `isDirty=true`, while `get_editor_state` was stale and the Editor script DLL remained older than the test edit. The active Scene was not saved/reloaded; no second Editor or computer-use was used. Status is `COMPILE_PASS`, not `FOCUSED_TEST_PASS` or `VERIFIED`. No file was deleted or moved; rollback remains forward correction under the Task's deletion rule.

2026-10-02 focused Editor follow-up, superseding the prior import-pending snapshot: original Editor exact EditMode job `78c4ab13ec30461aa1cdb2133ab5796b` completed with 2/2 PASS, 0 failed. It ran the two named `FormalBattleWav_*` methods after import; the battle-only path, formal WAV clip's 2 channels/80454 sample frames and actual pooled voice were checked while preserving the generic route and earlier cue controls. [Raw job result](../../../artifacts/diagnostics/NTSD28-336B44-Q10-KIM-J1-STEREO-WAV-DEPLOY-001/original-editor-focused-20261002.json). The original Editor is now idle, non-Play and Battle Scene clean; Battle/Menu/mode/Sakura/Kim protected SHA match. Mark `FOCUSED_TEST_PASS / SCENE_PENDING`; natural Battle Scene Kim event→voice, left/right device samples, panning and formal speaker parity remain unverified. No further file deletion or movement.
