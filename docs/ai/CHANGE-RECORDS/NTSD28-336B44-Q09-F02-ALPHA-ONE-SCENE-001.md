<!-- CHANGE-RECORD
id: NTSD28-336B44-Q09-F02-ALPHA-ONE-SCENE-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07F02Kind10BattlePlayProbeEditor.cs
authority: 336B44 formal root and corresponding playable presentation_interpolation.cpp and Unity Q09/F02 same-phase witness
evidence: docs/ai/TASKS/NTSD28-336B44-Q09-F02-ALPHA-ONE-SCENE-001.md
-->

# F02 alpha-one original Scene witness

Created before changing the Editor diagnostic script. The existing F02 probe takes tick39 Game View immediately after a production tick and exports commands after the PNG; its old report does not independently state the built display alpha. The formal raw WARP tick39 image is alpha 1. The change is an opt-in wait for a paused tick39 central build at alpha 1, then a screenshot and the built alpha in the evidence. Only the declared Editor diagnostic path may change. Existing request behavior, production tick and input, formal executable/source, DAT, images, Scene, camera and nonbattle behavior stay untouched.

At creation, expected side effects were one bounded Editor wait on the opt-in request, one new uniquely named evidence run, and an additional serialized diagnostic scalar. No production or file replacement was planned. Preflight, acceptance, failure rules and rollback are in the [Task](../TASKS/NTSD28-336B44-Q09-F02-ALPHA-ONE-SCENE-001.md). This paragraph was recorded before the script edit.

2026-10-04 code and generated compile: only `NTSD28Q07F02Kind10BattlePlayProbeEditor.cs` changed. A false-by-default request flag gates the tick39 alpha-one wait; tick30 and all old requests retain their path. The wait checks the paused tick, materialized central frame and private built-alpha diagnostic, then captures a unique PNG. Central evidence includes generation and built alpha. `dotnet build Assembly-CSharp-Editor.csproj --no-restore -nologo -v:q -clp:ErrorsOnly` exited 0 with 0 errors/297 warnings; scoped `git diff --check` exited 0. This is `COMPILE_PASS`, not original Editor or runtime validation. Existing dirty files were preserved.

2026-10-04 scoped runtime exit: original Editor PID105896/port6401 imported this script in place; no second project/Editor or computer-use. One unique request produced `CAPTURED/DONE` at relative tick39 with CentralOnly simulation/display/frame tick44 and built alpha exactly1, 1920×1080 PNG and five corresponding Entity commands. New initial+45 tick sample array equals prior run-07 46/46, event array 47/47; prior formal-root 2346/2346 selected-field result applies transitively. Relative weapon anchors versus current playable formal-resource raw WARP differ −0.357/−0.887 output pixels. Ordered shutdown `Completed`, five residue counts zero, original Editor back idle/clean Menu, four SHA stable within the run. `Tools/Validate-ChangeLedger.ps1` exited0/PASSED (1213 Records, six current code paths covered). Broader GPU/physical-key/Q09 behavior remains unverified. No production/DAT/scene/nonbattle edit. Status `VERIFIED` for this bounded diagnostic and matched-alpha anchor only. [Report](../../../artifacts/diagnostics/NTSD28-336B44-Q09-F02-ALPHA-ONE-SCENE-001/REPORT.md).
