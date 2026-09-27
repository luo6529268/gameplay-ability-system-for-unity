# Q10/O-02 Unity mono output calibration: scoped acceptance

Status: `VERIFIED` for this Editor-only calibration; `O-02`, `Q10`, `BATCH-05` and the master battle-alignment goal remain open.

Authority input: formal root `NTSD2.8-Logan.exe` SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`, with paired playable controlled Naruto run events `003`/`004` at rule X `649`/`729`, camera X `0`, and native mono output matrix gains `53/47` and `41/59`. That is a paired source diagnostic; it is not a captured root-EXE speaker waveform. Existing Unity selected Play had the same rule-source X values but queued physical X `664`/`787` and played centered 2D mono voices. This package changed only the diagnostic script and its Unity-generated `.meta`.

The original Editor (Unity 2022.3.62f3) compiled the new script with zero C# errors. In saved Menu Play, the test alone played a temporary 440 Hz mono PCM voice, captured stereo main output with `AudioRenderer`, and normalized channel RMS to its full-left reference. It temporarily used `Time.captureFramerate=30` and `Application.runInBackground=true` to drive capture frames, restored both on completion, stopped `AudioRenderer`, and destroyed the temporary source and clip. No production audio, event coordinate, DAT, WAV, Scene, camera, mode asset or nonbattle flow was modified.

| Attempt | Result | Evidence |
| --- | --- | --- |
| v1 | `CAPTURE_FAILED`: 1024 frames only in the first case and zero in four later cases; no pan inference | `calibration.json`, SHA-256 `B596146ACB8D015A6652C240B88638F13C8073585C6EA81FEDCCD150D10F06E3` |
| v2 | `CAPTURE_COMPLETE`: 194 game/audio updates, 6.464 DSP seconds, 25,600–26,624 selected sample frames. Initial trigonometric inverse was RED: target `0.53/0.47` measured `0.51960/0.48136`; target `0.41/0.59` measured `0.44678/0.56274` | `calibration-v2.json`, SHA-256 `D1FA49A1F41B1D576D9909D5360F7565B44B8DF7E800575A75A5ED8B73047425` |
| v3 | `CAPTURE_COMPLETE`: 190 game/audio updates, 6.336 DSP seconds, 26,624–28,672 selected sample frames. Corrected inverse met both native target rows within `0.00008` per channel | `calibration-v3.json`, SHA-256 `024D2AD54FB348B63C2D754D591CB88FEE03E52762E94735EC271988DCC6CDDD` |

Observed mono-channel law in this Unity output is approximately `left = volume * sqrt((1 - pan) / 2)`, `right = volume * sqrt((1 + pan) / 2)`. For desired normalized native gains `L,R`, the verified diagnostic inverse is `power = L² + R²`, `pan = (R² - L²) / power`, `volume = sqrt(power)`; zero power needs a separate zero-volume branch if used in production.

| v3 case | Applied pan / volume | Measured left / right | Target left / right | Maximum absolute channel error |
| --- | --- | --- | --- | --- |
| Hard-left control | `-1 / 1` | `1 / 0` | `1 / 0` | `0` |
| Unity center control | `0 / 1` | `0.70698041 / 0.70698041` | `0.70710677 / 0.70710677` | `0.00012637` |
| Native 53/47 | `-0.11956955 / 0.70837843` | `0.52992970 / 0.46993765` | `0.53 / 0.47` | `0.00007028` |
| Native 41/59 | `0.34870204 / 0.71847057` | `0.40998980 / 0.58998531` | `0.41 / 0.59` | `0.00001467` |
| Native 50/50 | `0 / 0.70710677` | `0.49996591 / 0.49996591` | `0.5 / 0.5` | `0.00003410` |

The original Editor exited Play and reported idle, Menu active, no compile or asset update. The saved Menu Scene SHA-256 stayed `785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13`; Battle Scene stayed `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A`. The v3 result reports `captureStopped=true`. Scene hashes and the terminal Editor state support no persisted Scene change; they do not establish a formal-device listening comparison.

Next Q10 work must first declare its own production Task/Change contract. The user has been asked whether left/right localization under the fixed full-background exception should track **visible position as a scene fraction** or the **formal rule-world X and audio-only virtual camera**. Production mapping must wait for that choice while preserving D-024 physical-distance behavior; it must then apply the measured mono output inverse at the battle-only voice outlet and handle zero volume and non-mono clips. A selected natural Play should verify the actual queued event, output voice parameters, audible-channel capture and lifecycle. Other cues, BGM/WMA, formal root-EXE device output and Q10 overall remain separate gates.
