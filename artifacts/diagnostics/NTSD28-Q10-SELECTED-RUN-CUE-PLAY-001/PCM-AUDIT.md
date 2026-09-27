# Selected Naruto running WAV container versus decoded samples (2026-09-27)

Read-only comparison of the two cues actually played in `play-result.json`. Inputs were the existing Unity `Assets/NTSD/Sound/data/{003,004}.wav` and formal root `resources/runtime/vfs/data/{003,004}.wav`. Python standard-library `wave` read each RIFF/WAV's complete PCM frames; SHA-256 was calculated over exactly `readframes(getnframes())`. No source audio was edited.

| Cue | Container bytes Unity/formal | Channels | Sample width | Sample rate | Frames | PCM bytes | Decoded PCM SHA-256 Unity/formal |
|---|---:|---:|---:|---:|---:|---:|---|
| `003.wav` | 9880 / 9870 | 1 | 2 | 22050 Hz | 4857 | 9714 | both `9DF1C977C5D312C03368D6FD8F0D7A860CA91E77177AC3E897691F1513E2DC68` |
| `004.wav` | 11238 / 11228 | 1 | 2 | 22050 Hz | 5536 | 11072 | both `4D55DEB8F1136E02AD4BD9CB0412EB041E3EED989979BD01C972FF41C8FEC274` |

Both pairs have identical channel/sample-width/rate/frame counts and byte-identical decoded PCM payloads. Their whole-file SHA-256 values differ and the Unity files are each 10 bytes longer, so the containers are not byte-identical; this audit does not locate every differing non-PCM byte. For these two selected sounds, copying the formal WAV solely to change PCM samples is unnecessary. This does not prove Unity's playback gain, pitch, spatialization, scheduling, mixer output or physical-device waveform matches the formal XAudio2 backend; O-02/O-03/Q10 remain open. It also says nothing about the other WAV files in the Q10 catalog audit.
