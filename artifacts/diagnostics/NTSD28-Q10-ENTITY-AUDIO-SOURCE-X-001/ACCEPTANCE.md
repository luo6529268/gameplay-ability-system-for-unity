# Q10/O-02 entity battle audio source X — scoped acceptance

2026-09-28, original Unity project and existing Editor PID 11944. This package changes the `WorldX` of declared entity-bound battle sound events to the initialized source-rule X, preserving each caller's former physical fallback if that carrier is unavailable. It does not change cue selection, tick, voice dispatch, DAT, camera, stereo output, or nonbattle audio.

Formal paired playable frame events 003/004 used X649/729. Before the edit, the original Battle's selected Naruto physical-D run observed source-rule X649/729 but queued physical X664/787. After the edit, the original Editor's Menu→Battle Play probe with physical D produced:

| cue | tick/callback | queued WorldX | actor source-rule X | actor physical X | prepared voice |
|---|---:|---:|---:|---:|---|
| `data\\003.wav` | 8/8 | 649 | 649 | 664 | yes, playing |
| `data\\004.wav` | 13/13 | 729 | 729 | 787 | yes, playing |

The probe reported `PASS`, Battle unload `true`, no new unprepared-cue rejection, and distinct accepted ticks. The relevant event fields are in [play-result.json](play-result.json). The prior selected Play and formal paired reference remain in `NTSD28-Q10-STEREO-SELECTED-EVENT-PROBE-001` and `NTSD28-Q10-NATIVE-STEREO-CAMERA-TRACE-001`.

Test-first original-Editor EditMode job `c15c76f63da344d7858c999a7759e832` failed 1/1 on the absent resolver as intended ([RED job](red-test-job.json)); after implementation job `97c55400aecd442fa229741e5f9d46cf` passed 1/1 ([GREEN job](green-test-job.json)). `dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:q -clp:ErrorsOnly` passed with 0 errors and 199 existing warnings after the final probe edit. The original Editor imported the code and ran the focused test and natural Play.

After the probe, the Editor is idle, non-Play, Menu active and clean. Menu Scene SHA `785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13`; Battle Scene `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A`; Input Actions `B5148EE08E8902C7DE41661BADC504363C97D9FA4A39DD985A2E828E98941A99`; GameConfig `0527D737A1FA38FC56B51D00DC6E96A421D3C67222546368B147C2D074CB8EA7`; ProjectBattleModeConfig `B57CFEF32CC3ECE37AC98A4A1EF04FEB2FEE4EB3CC08466A4BFD32C1EDD85B82`. All five match the pre-Play hashes.

The two selected frame cues prove the natural shared entity sound path. Hit, special-attack, transition and KO producers use the same resolver but were not separately run in this package. The original Unity voice for both selected cues remains mono clip, 2D `spatialBlend=0`, `panStereo=0`; the formal speaker matrix and remaining Q10/O-02 outlets remain open. Q07/D-024 collision-domain choice is independent and still pending.
