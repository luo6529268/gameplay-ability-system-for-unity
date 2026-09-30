# Q10 battle WAV route and cache ownership audit

Status: `READ_ONLY_ROUTE_AUDIT / NO_AUDIO_COPY / NO_CODE_CHANGE` (2026-09-29). This is an implementation dependency for the existing 978-path formal battle-WAV candidate, not proof of audible parity or approval to replace unrelated audio.

Scope correction later on the same date: the 978-path v1 candidate included `data/085.wav`, whose currently identified formal owner is character selection. The [v2 battle-only candidate](../NTSD28-Q10-FRAME-SOUND-ENTRY-READINESS-001/FORMAL-BATTLE-WAV-DEPLOYMENT-CANDIDATE-V2-20260929.md) contains 977 rows and is the current exact review list. All references to 978 below describe the preserved v1 audit, not a deployment authorization.

Fresh identity checks: the formal root EXE is SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`; the exact 978-path candidate CSV is SHA-256 `A335C40971C9885AF53856EEBB1EAE265662DB2344E7E968941BEB51B566FA25`. The PowerShell 13/13 cue-table-candidate membership check returned no unmatched ID. The three changed handoff documents pass `git -c core.safecrlf=false diff --check`; no Unity compilation, import, Play, or audio decode was run in this audit.

## Current Unity route

- `AppManager.InitializeBattleAsync` calls `NTSDSoundPlayer.PrepareBattleCuesAsync(CharacterAnimtorManager.Instance)` before the battle allocation seal (`Assets/NTSD/Scripts/App/AppManager.cs:189-211`). The call does not pass a content source.
- `PrepareBattleCuesAsync` collects manager sound IDs plus thirteen built-in SFX IDs and calls the same `GetOrPrepareCue` used by public `PlaySfx`, private battle `PresentSound`, and the diagnostic wrapper (`NTSDSoundPlayer.cs:143-175,192-204,239-272`).
- `GetOrPrepareCue` caches by sound ID only. A changed `AudioController` clears the cache, but a changed configured Logan root does not. Its source path remains `Application.dataPath / soundRootFolder / normalizedRelativeFolder`, where `soundRootFolder` defaults to `NTSD/Sound` (`NTSDSoundPlayer.cs:17,271-319`). `PrepareBattleCuesAsync` resets the sealed flag but does not otherwise clear already prepared entries (`:143-155`). Consequently, merely staging the candidate WAVs under `Assets/NTSD/Content/LoganRuntime/vfs` cannot make this player read them.
- `CharacterAnimtorManager.ConfiguredContentRoot` already reads `GameConfig.BattleContentRuntimeRoot` (`CharacterAnimtorManager.cs:43-45`; `GameConfig.cs:15`). `BattleContentSource.ForLoganRuntime(root)` supplies a VFS root and a root-constrained image resolver (`BattleContentSource.cs:28-51,73-117`), but there is no audio resolver in that source.
- The current `NormalizeRelativeFolder` replaces backslashes and colons in `AudioItem.streamingFolder`; it does not enforce the root-constrained relative-key contract used by `BattleContentSource` (`NTSDSoundPlayer.cs:670-698`). A new formal VFS sound route therefore needs explicit relative-path validation, not direct reuse of this normalization as a security boundary.

## Built-in channel cues are not file paths

The same player prewarms thirteen literal `SFX_###` IDs (`NTSDSoundPlayer.cs:21-36`). In the serialized direct Battle Scene the `AudioController.AudioList` is empty (`Assets/NTSD/Scene/NTSD_Battle.unity:1812`), so each ID uses the fallback `streamingFolder = soundId`. Its current `IsSingleFilePath` test recognizes only an audio extension; all thirteen IDs are therefore treated as *directories* named `SFX_###`, not WAV paths (`NTSDSoundPlayer.cs:271-319,627-635,657-681`). Merely changing the root to Logan VFS would still leave this class of battle cue unresolved.

The formal playable backend instead resolves a built-in event's `native_channel` by ordered index into `data/sound.dat`, then resolves the returned relative WAV path (`source/ntsd28_playable/src/audio_backend.cpp:325-375,390-403,530-549`). Current formal `decoded_dat/data/sound.dat` has 18 ordered entries. A current-tree PowerShell parse counted 13 Unity IDs, 18 table entries and 978 candidate paths; 13/13 IDs had a unique same-numbered basename in both table and candidate, with no unmatched ID:

| Unity cue | Formal table index | Formal path |
|---|---:|---|
| `SFX_001` | 0 | `data/001.wav` |
| `SFX_002` | 1 | `data/002.wav` |
| `SFX_004` | 5 | `data/004.wav` |
| `SFX_006` | 2 | `data/006.wav` |
| `SFX_010` | 3 | `data/010.wav` |
| `SFX_011` | 4 | `data/011.wav` |
| `SFX_017` | 7 | `data/017.wav` |
| `SFX_032` | 11 | `data/032.wav` |
| `SFX_033` | 12 | `data/033.wav` |
| `SFX_039` | 13 | `data/039.wav` |
| `SFX_065` | 14 | `data/065.wav` |
| `SFX_066` | 15 | `data/066.wav` |
| `SFX_068` | 16 | `data/068.wav` |

This table is a current-content filename correlation, not by itself proof that every Unity producer uses the same formal channel. Two selected formal channel branches are directly identifiable: `battle_world.cpp::append_native_kind0_post_audio` emits channel 14 for reaction action 200 and 16 for 203, while Unity's corresponding hit plan queues `SFX_065` and `SFX_068` (`battle_world.cpp:748-770`; `BattleEcsHitExecutionPlan.cs:4238-4277`). Full producer-channel parity and audible playback remain Q10 exits. The production route must resolve built-in events from the formal *ordered sound table* under the validated VFS root, while protecting public nonbattle playback and content-root changes; a suffix-only hard-coded table would not preserve table-order semantics if content changed.

## Bounded implementation implication

The planned Q10 package needs one coherent battle-only source identity: resolve formal file-path cues within the configured Logan VFS, map built-in channel cues through the ordered formal sound table, key or invalidate prepared cues when the battle source changes, and preserve the existing empty-root and public nonbattle `PlaySfx` behavior. It must show that one selected natural file-path cue and one selected natural built-in cue use the formal staged path and open actual voices after original-Editor import. The candidate's 978/978 source hashes and unoccupied destination paths are a copy precondition only; they do not establish runtime routing, decode, Player packaging, or overall Q10 completion. No script, WAV, DAT, Scene, asset, or nonbattle behavior was changed by this audit.
