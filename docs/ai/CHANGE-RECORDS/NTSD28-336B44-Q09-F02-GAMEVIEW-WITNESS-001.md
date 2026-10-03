<!-- CHANGE-RECORD
id: NTSD28-336B44-Q09-F02-GAMEVIEW-WITNESS-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07F02Kind10BattlePlayProbeEditor.cs
authority: 336B44 formal F02 trace and user fixed-background proportional view exception
evidence: docs/ai/TASKS/NTSD28-336B44-Q09-F02-GAMEVIEW-WITNESS-001.md
-->

# Q09/F02 original Battle Game View witness

Created before editing the declared Editor diagnostic script. The current F02 original Scene trace proves selected rule fields, event ordering and shutdown, but reports that `viewX/viewZ` are simulation projections rather than rendered pixels. The Task defines a single opt-in two-tick screenshot witness, preconditions, no-simulation-drift comparison, protected assets and rollback. Expected side effects are one new unique request/result directory and two PNGs. Existing F02 modes and production behavior must remain unchanged. Actual code diff, compile, Play results, failures and visual limits will be appended before status promotion.

2026-10-03 actual script diff: the declared F02 Editor probe now accepts an optional `captureGameView` request, pauses production tick stepping while `ScreenCapture` writes PNGs after relative ticks30/39, validates PNG signature/dimensions, records SHA and size, and then resumes the original 45-tick flow. With the option false, the old request sequence and result fields keep their previous behavior. Generated `dotnet build Assembly-CSharp-Editor.csproj --no-restore --nologo -v:q -clp:ErrorsOnly` passed with 0 errors/271 warnings. Original Editor import, Play screenshots, old paired-state comparison and shutdown remain unverified at this point. No production, content or Scene file was edited.

2026-10-03 verification: original Editor imported with no compile error and completed `f02-kind10-view-20261003-01` in the original Battle Scene. The two distinct 1920×1080 PNGs passed format/size/SHA checks. `before`, `after`, all 46 samples and all 47 event rows are exactly equal to the already paired run-07; this bounds the screenshot wait's simulation impact. `00058.json` reports `CAPTURED / DONE`, empty error, orderly shutdown, zero World/slot/pool borrower/active renderer/Sprite, quiesced pool, detached World, clean saved Menu after exit, and stable protected hashes. Subsequent live Editor state was idle and not playing. The later PNG visibly contains the weapon near Tayuya, while both PNGs reveal a large opaque black block overlapping Naruto/joystick. The latter is a Q09 observation requiring separate source diagnosis; no full visual parity or formal EXE pixel parity is claimed. See [visual report](../../../artifacts/diagnostics/NTSD28-336B44-Q09-F02-GAMEVIEW-WITNESS-001/REPORT.md). Status `VERIFIED` is limited to obtaining original Game View evidence without simulation drift; F02/Q09/Q12 overall remain open. Rollback, if needed, is a reviewed forward correction to the opt-in diagnostic script; no content/Scene/production code change occurred.
