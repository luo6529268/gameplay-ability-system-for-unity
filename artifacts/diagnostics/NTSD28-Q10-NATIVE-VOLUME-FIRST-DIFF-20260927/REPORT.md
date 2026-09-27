# Q10/O-03 F11/F12 volume production first difference (2026-09-27)

Status: `STATIC_FIRST_DIFFERENCE_CONFIRMED / IMPLEMENTATION_PENDING / RUNTIME_PENDING`. This is a read-only source and Unity production-path audit, not an audible or Play result. Owner: `BATCH-05 / Q10 / O-03`; it does not reopen Q07.

## Formal playable path

The formal release identity remains the root `NTSD2.8-Logan.exe` SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`. The corresponding playable build includes `src/main.cpp` and `src/audio_backend.cpp` through the declared playable build closure. In `main.cpp:2401-2417`, `advance_native_volume` samples held F11/F12 once per successful battle logic step, subtracts or adds one percentage point, and gives F12 priority if both are held. The regular step calls it after `session.step` and before battle audio submission (`main.cpp:2525-2550`); a paused F2 single step does the same (`main.cpp:2572-2599`). No successful step means no volume advance. Recording-overlay handling affects the displayed volume notice, not the audio gain update.

`audio_backend.cpp:100-118,460-473,689-695` clamps to 0..100 and immediately retunes active SFX and BGM voices. SFX attenuation is `0 -> -10000`, otherwise integer `((volume - 100) * 0xED8) / 100` hundredths of a dB, with C++ division truncating toward zero. BGM uses its separate `0 -> -10000`, otherwise `34 * volume - 3900` mapping. Gain converts hundredth-dB with `pow(10, db / 2000)`; zero is explicitly silent. Newly submitted battle sounds also use the updated state.

## Current Unity path and first difference

`Assets/NTSD/Scripts/Simulation/Input/NTSD28NativeFunctionKeyPhysicalLatch.cs` captures physical holds, and `NTSD28NativeFunctionKeyRouter.cs:193-201` maps them to `VolumeDown`/`VolumeUp` with F12 priority. `Assets/NTSD/Scripts/Simulation/Host/SimulationTickDriver.cs:2083-2092` stores the continuous host command, and `:1094-1095` exposes a diagnostics getter. A production search of the driver found no consumer of `_nativeFunctionKeyContinuousHostCommand` beyond assignment and diagnostics. Thus physical routing evidence does not establish an audio effect: pressing F11/F12 currently leaves battle audio gain unchanged.

`NTSDSoundPlayer.cs:288-367` sends battle cues to pooled `AudioSource`s and sets each voice volume from its `AudioItem` base/random value, without a native battle-volume multiplier or active-voice retune. `SimulationTickDriver.cs:709-735` dispatches already-published sounds to the battle sink. `GameLocalSettings.MasterVolume/SfxVolume/MusicVolume` are separate menu/user settings and are outside this battle-rule fix. There is no identified production BGM voice owner in the inspected battle sound path; `data/bgm.dat` content entry alone does not provide playback.

## Bounded next package

Before editing scripts, create a dedicated Q10 Task/Change with the exact driver, battle sound sink, and focused test paths. Test the 100→99→0→100 boundary, held-key one-change-per-successful-tick behavior, both-key F12 precedence, paused F2, at most two LocalFreeRun ticks per Update, worker publication timing if enabled, and retuning of an already-playing pooled voice. Keep Manual/Lockstep, menu settings, Battle Scene, DAT values, source files, and nonbattle audio unchanged. The BGM consumer and formal WMA resource/playback boundary need their own declared owner; an SFX-only fix cannot close O-03 or Q10. Do not repeat Q06 frame-sound producer tests or Q07 representative content matrices unless a shared writer changes.

No project script, DAT, media, Scene, project setting, or formal-source file was changed for this audit. No Unity compile, Play, speaker recording, or formal-EXE live audio comparison was run.
