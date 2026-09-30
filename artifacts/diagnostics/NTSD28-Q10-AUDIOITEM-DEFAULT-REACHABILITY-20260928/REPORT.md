# Q10/O-02 default battle AudioItem reachability audit

Date: 2026-09-28. Scope: read-only source and serialized-asset audit of the saved Menu-first, Battle-second project entry. No Unity Editor Play, device capture, DAT edit, scene edit, or production-code edit was performed for this audit.

## Authority and observed project path

- The formal root `NTSD2.8-Logan.exe` SHA-256 was freshly checked as `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`.
- The paired playable build includes `src/audio_backend.cpp` (`source/ntsd28_playable/scripts/build.ps1`, lines 95, 131, 664). `XAudio2Backend28::submit` lines 503-607 submits each resolved event wave with battle volume and optional stereo matrix. Its event loop has no per-cue wall-clock throttle or randomized volume/pitch branch. This is a source-path observation, not a new root-EXE speaker measurement.
- `ProjectSettings/EditorBuildSettings.asset` registers `NTSD_Menu` first and `NTSD_Battle` second. A search of serialized `.unity`, `.prefab`, and `.asset` files under `Assets` for the `AudioController` script GUID `9a98fb328cdb667479d395af046ebc5e` found only `Assets/NTSD/Scene/NTSD_Battle.unity`. That instance has `AudioList: []` at line 1812; Menu has no serialized `AudioController`.
- Searches of project scripts found no assignment to `AudioController.AudioList` and no `AddComponent<AudioController>` path. `NTSDSoundPlayer.GetOrPrepareCue` lines 271-317 chooses `FindAudioItem(...) ?? CreateFallbackAudioItem(...)`. The fallback at lines 655-668 sets `minTimeBetweenCall=0`, `randomVolume=0`, `randomPitch=0`, and `volume=1`. `PlayPreparedCue` lines 339-378 still contains `Time.time` and random calls, but with these fallback values they cannot suppress an otherwise loaded cue or vary its volume/pitch in the saved default scene.

## Disposition

For the saved default Menu-to-Battle scene configuration, the old `Time.time` throttle and random volume/pitch are **static-inactive configuration branches**, not a proven current battle playback first difference. Do not change shared audio code solely to remove them. This does not prove a dynamically assigned `AudioList`, another scene/configuration, or a future configured cue cannot reach them; such a path must be witnessed before opening a production change. The already observed 2D/pan-zero stereo mismatch, battle cue coverage, BGM/stop-all, natural 65-voice reachability, and root-device output remain separate open Q10/O-02 exits.

Validation: read-only source/asset searches and root EXE SHA check only. No new compile, automated test, Editor runtime, or audible test was claimed. Existing Q10 Play results remain the evidence for selected 003/004 voice playback, not for this dynamic-configuration exclusion.
