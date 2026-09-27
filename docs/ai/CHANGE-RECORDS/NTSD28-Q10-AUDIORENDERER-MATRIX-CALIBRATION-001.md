<!-- CHANGE-RECORD
id: NTSD28-Q10-AUDIORENDERER-MATRIX-CALIBRATION-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q10AudioRendererMatrixCalibrationEditor.cs
authority: formal NTSD2.8-Logan root EXE and paired playable mono battle stereo matrix; Unity 2022.3 AudioRenderer and AudioSource pan contract
evidence: docs/ai/TASKS/NTSD28-Q10-AUDIORENDERER-MATRIX-CALIBRATION-001.md
-->

# NTSD28-Q10-AUDIORENDERER-MATRIX-CALIBRATION-001

PLANNED before script creation. The Task contains the precise authority, original Unity discrepancy, single Editor-only code path, temporary Play side effects, no-overwrite output, cleanup and rollback. This calibration will not alter production event coordinates, sound mixing or the saved Scene. The user-approved fixed full-background camera and proportional movement remain intact.

Expected acceptance is one focused original-Editor capture comparing full-left and center controls with Unity candidate pan/gain settings for the formal 53/47 and 41/59 mono matrix rows. Compilation, actual capture, source cleanup and Scene SHA are pending. The result cannot close O-02/Q10 or claim formal EXE speaker output.

2026-09-27 CODE_WRITTEN: added the declared Editor-only script. It generates a short mono 440 Hz PCM source in Play, captures five cases (hard-left, Unity center, native 53/47, 41/59 and 50/50 candidates), normalizes per-channel RMS to hard-left, and saves a no-overwrite JSON. Every exit route stops the voice and any capture it owns, unsubscribes callbacks and destroys its temporary GameObject/clip. The script does not modify production event coordinates or the saved Scene. Original-Editor compile and Play remain pending.

2026-09-27 first focused Play CAPTURE_FAILED: original Editor compiled the new script with zero C# errors and created its meta. It captured only 1024 hard-left frames (RMS 0.08826394) and zero frames in four later cases; JSON SHA B596146ACB8D015A6652C240B88638F13C8073585C6EA81FEDCCD150D10F06E3. This cannot validate pan law. CaptureStopped=true; original Editor exited Play, saved Menu/Battle Scene SHA unchanged at 785F828C...81E13 / 2EE465D8...B77A. The preceding Task now declares a same-script diagnostic timing adjustment with temporary captureFramerate and player-loop update, restored on all exits; v2 output must be separate and must preserve this failure.

2026-09-27 v2 focused Play CAPTURE_COMPLETE with 194 Editor/game/audio updates, 6.464 DSP seconds, and 25,600–26,624 selected sample frames per case; JSON SHA D1FA49A1F41B1D576D9909D5360F7565B44B8DF7E800575A75A5ED8B73047425. The hard-left control measured 1/0; Unity center measured 0.70717/0.70717 against 0.70711; native-center candidate measured 0.50004/0.50004 against 0.5. The trigonometric candidate failed exact 53/47 (measured 0.51960/0.48136) and 41/59 (0.44678/0.56274). This is useful RED evidence: panStereo uses a square-root constant-power amplitude law in the tested Unity output. CaptureStopped=true, original Editor exited Play, both Scene hashes unchanged. The Task now authorizes only a v3 diagnostic inverse-formula correction; no production code has been changed.

2026-09-27 VERIFIED, scoped Editor diagnostic only: v3 original-Editor Play completed 190 audio/game updates and 6.336 DSP seconds, with 26,624–28,672 sample frames per case. The hard-left reference measured 1/0. Inverting the measured Unity mono panning law produced 53/47 -> 0.52992970/0.46993765 and 41/59 -> 0.40998980/0.58998531; maximum absolute channel error 0.00007028. Native-center 50/50 -> 0.49996591/0.49996591. JSON SHA-256 024D2AD54FB348B63C2D754D591CB88FEE03E52762E94735EC271988DCC6CDDD. CaptureStopped=true; probe restored captureFramerate/runInBackground and removed the temporary voice. Original Editor exited Play to idle Menu with no compilation/update in progress; Menu/Battle Scene SHA-256 remained 785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13 / 2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A. The v1 failure and v2 RED remain archived. This validates Unity's controlled mono main-output mapping only; no production battle sound, formal root EXE device output, other cue type, O-02, Q10 or BATCH-05 is closed. See the package ACCEPTANCE.md for method and limits.
