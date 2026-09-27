# Paired playable Naruto run camera and frame-sound witness

Status: `VERIFIED_SCOPED_SOURCE_DIAGNOSTIC`, 2026-09-27. Parent `NTSD28-UNITY-BATTLE-REALIGNMENT-001 / BATCH-05 / Q10 / O-02`. O-02, Q10 and the master goal remain open.

The formal root `NTSD2.8-Logan.exe` SHA-256 was rechecked as `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`. The new diagnostic source compiled with MinGW g++ 15.1.0, C++17, all 28 paired `ntsd28_core/src` translation units plus `ntsd28_playable/src/game_session.cpp` and `selection_flow.cpp`; compile exit 0. The output executable was written only under this diagnostics directory, never into the formal source tree. Its two `GameSession28` arguments both point to the read-only formal `resources/runtime` root: the first supplies `catalog.csv`, the second supplies complete-VFS `decoded_dat` and `vfs`. Two earlier invocations supplied a child directory incorrectly and failed initialization with exit 5; their `controlled-run.tsv`, `controlled-run-v2.tsv`, `run.log` and `run-v2.log` are preserved. The corrected `controlled-run-v3.tsv` has 30 completed tick rows and exit 0.

The successful compile/run can be reproduced from the Unity repository root with these PowerShell commands (the output path must be changed if the TSV already exists, because the diagnostic refuses overwrite):

```powershell
$sourceRoot = 'J:\QQFile\NTSD2.8.3.3 zip\NTSD2.8.3.3\NTSD 2.8-Logan\source'
$runtimeRoot = 'J:\QQFile\NTSD2.8.3.3 zip\NTSD2.8.3.3\NTSD 2.8-Logan\resources\runtime'
$outputRoot = 'artifacts/diagnostics/NTSD28-Q10-NATIVE-STEREO-CAMERA-TRACE-001'
$coreSources = Get-ChildItem -LiteralPath (Join-Path $sourceRoot 'ntsd28_core/src') -Recurse -Filter '*.cpp' | ForEach-Object FullName
$compileArgs = @('-std=c++17', '-O0', '-g0', '-I' + (Join-Path $sourceRoot 'ntsd28_core/include'), '-I' + (Join-Path $sourceRoot 'ntsd28_playable/include'), 'Tools/NTSD28Q10Diagnostics/native_stereo_camera_trace.cpp') + $coreSources + @((Join-Path $sourceRoot 'ntsd28_playable/src/game_session.cpp'), (Join-Path $sourceRoot 'ntsd28_playable/src/selection_flow.cpp'), '-o', (Join-Path $outputRoot 'native_stereo_camera_trace.exe'))
& g++ @compileArgs
& (Join-Path $outputRoot 'native_stereo_camera_trace.exe') $runtimeRoot $runtimeRoot (Join-Path $outputRoot 'controlled-run-v3.tsv')
```

Controlled source battle: mode0, stage23, seed2833, human Naruto OID2/slot0 at X620/Z650 and human Lee OID7/slot1 at X1200/Z650, both HP500/MP500. Input was logical right held for ticks1–2, released ticks3–4, then held. No actor action, frame, sound event or coordinate was forced. Stage23 formal `decoded_dat/b/Hos/b.dat` declares width1330; `NativeStageCamera28::step` clamps by `max(0, width-1333)`, so camera X was 0 in every observed tick. This original background DAT was read **only** to explain the native camera; it is not a Unity content candidate under the user's exclusion.

| Source tick | Authored frame | Formal event source | Formal event X | Camera X | Native integer matrix L/R | Unity selected cue tick | Unity actor source-rule X | Unity event physical X | Unity actual voice |
|---:|---:|---|---:|---:|---:|---:|---:|---:|---|
| 6 | 9 / `data\\003.wav` | `frame_sound` | 649 | 0 | 53/47 | 8 | 649 | 664 | mono, 2D, pan0 |
| 11 | 11 / `data\\004.wav` | `frame_sound` | 729 | 0 | 41/59 | 13 | 729 | 787 | mono, 2D, pan0 |

The controlled native source actor reaches exactly the same rule-space X as the prior original-Editor selected Unity actor at both cue declarations, with a two-tick offset because the Unity probe began sending D after its Running/tick2 gate. `WorldAudioEventSource28::frame_sound` is enum value 1 in `battle_world.h`; both rows record value 1. The integer L/R projection in the TSV copies the exact branches of `audio_backend.cpp::native_stereo_mix28` with the host's `locked_local_battle_stereo_layout28(0)` parameters; it is a diagnostic projection, not captured speaker output. For these two records the Unity 2D/pan0 voice configuration is independent of position, whereas the formal matrix is unequal and changes with event X. The approved fixed full-background camera and proportional-motion exception remain intact; actor source-rule X must be used for any formal-rule calculation, rather than the queued physical X.

Raw successful TSV SHA-256 `786673550F9C61EF1029699F339E30F0F596BE7B5FE02C8EA1AF48D9347CD3A2`; diagnostic source SHA-256 `D95751C237C172C29B01F9851B0789DBA903D364C23E990077F66369D2034CCA`. The selected Unity Play JSON and its scope are in `../NTSD28-Q10-STEREO-SELECTED-EVENT-PROBE-001/ACCEPTANCE.md`. This source fixture controls Lee's X/Z and uses formal stage23, whereas the Unity scene uses its project map and natural Lee birth; equality of Naruto's two rule-space cue X values does **not** prove identical whole-world/RNG state or formal root EXE device output. The root EXE's existing headless/LFR trace does not expose camera or output matrix. Those gates and other cues remain open.

Only the new diagnostic C++ source, its outputs and governance/evidence documents changed. Unity production, DAT/WAV/images, project map, Scenes, camera Asset, mode Asset and nonbattle behavior were untouched. Both saved Unity Scene SHA-256 values stayed at `785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13` (Menu) and `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A` (Battle).

Final 2026-09-27 governance verification: `Tools/Validate-ChangeLedger.ps1` exited 0 (`Change ledger validation PASSED`, 933 records and 39 governed code files in the current repository diff); its full output is retained in `ledger-validation-20260927.log`. `git diff --check` exited 0. The original Menu and Battle Scene hashes were rechecked and still match the values above. No Unity script was changed by this package, so Unity compilation or Play was not rerun for this source-only witness; the prior selected Unity Play evidence remains the linked scoped result.
