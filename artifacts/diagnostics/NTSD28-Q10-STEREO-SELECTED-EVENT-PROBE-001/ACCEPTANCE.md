# Selected Naruto battle-stereo event probe

Status: `VERIFIED_SCOPED_UNITY_PLAY`, 2026-09-27. Parent `NTSD28-UNITY-BATTLE-REALIGNMENT-001 / BATCH-05 / Q10 / O-02`; O-02 and Q10 remain open.

The original Editor PID 11944 compiled the modified Editor-only diagnostic script after UnityMCP `refresh_unity` (`mode=force, scope=all, compile=request`): `Assembly-CSharp-Editor.dll` UTC 11:02:09 is newer than the script UTC 10:59:51. UnityMCP `manage_editor play` entered the saved Menu, and `execute_menu_item` ran the new `NTSD/Battle Diagnostics/Q10/Run Selected Naruto Stereo Event Probe`. The probe selected formal `LoganRuntime`, used physical D tap-release-hold with Naruto P1, forwarded every sound batch to the real `NTSDSoundPlayer`, unloaded only its Additive Battle and saved a terminal `PASS` JSON. The real player opened two prepared mono pooled voices; there were no rejected cues.

| Cue | Event/callback tick | Event WorldX | Actor source-rule X | Actor physical X | Voice channels | spatialBlend | panStereo | voice volume |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| `data\\003.wav` | 8/8 | 664 | 649 | 664 | 1 | 0 | 0 | 1 |
| `data\\004.wav` | 13/13 | 787 | 729 | 787 | 1 | 0 | 0 | 1 |

Both records have `sourceRulePositionInitialized=true`, `worldEventPresent=true`, `onMainThread=true`, `preparedClipAssigned=true`, `voiceIsPlaying=true`; pooled count advanced 0→1→2. The data proves that the current selected frame-sound event uses physical X, not source-rule X, and the final voice is 2D centered despite changing event X. It does **not** measure actual speaker channel amplitudes or formal camera X at ticks 8/13; the root EXE's same-state stereo matrix and any Unity output remapping remain pending. Formal battle sound uses rule-space event X and `camera_x` for the matrix. Do not feed the recorded physical X directly into that formula or alter the user's fixed full-background camera to force a match.

Raw result: `play-result.json`, SHA-256 `15E137B9BCCE0800B8A9E8D6BCFB8FBC85C123E906688EDF61C169985A5B70C2`. The earlier cue-play result remains byte-identical at SHA-256 `09A7E8D27F42C027314C339C7AB5CD84BF65A81345AEFA400F95F1EAD37ED3BF`. UnityMCP `manage_editor stop` left the same original Editor idle/non-Play with `NTSD_Menu` active. Saved Menu Scene SHA-256 `785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13`; Battle Scene SHA-256 `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A`; both match their pre-Play values and have no Scene Git diff.

Only the declared existing Editor diagnostic script and governance/evidence documents changed in this package. Production audio, DAT/WAV, Scenes, camera, mode Asset and nonbattle behavior were untouched. Next Q10 production package needs a matched formal camera/event trace and a channel-output acceptance method; this probe alone cannot close O-02.
