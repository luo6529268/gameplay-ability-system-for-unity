# NTSD28-USER-CAMERA-VIEWPORT-SCALE-001

Status: `ROLLED_BACK` after the user's explicit clarification that the original camera size must be retained. Parent: `NTSD28-UNITY-BATTLE-REALIGNMENT-001`.

Authority and decision: formal NTSD 2.8-Logan playable `RenderCamera28` and `NativeStageCamera28` use 1333×730 logical pixels. The user reviewed source/Unity 48-pixel run comparison (3.60% versus 2.34% at 16:9) and on 2026-09-23 requested a proportional camera correction. This supersedes D-020's fixed *size* exception only for battle presentation; fixed center and other exceptions remain until separately reviewed.

Correction: the user clarified that the requested ratio must change character run distance while retaining the original camera size. The preceding interpretation and its temporary script edit were withdrawn by inverse patch. Do not use this Task to reintroduce a camera-size change; any movement proposal needs a separate rule/visual-coordinate contract and an explicit account of the existing formal-source equal-tick position evidence.

Before: `BattleBackgroundPlatformPresentation.ResolveWorldCameraFrame` fits the entire 2048×1152 Training background, yielding 2048 logical pixels across a 16:9 view. Entity logic already moves the same X per tick as the source-model samples.

Declared edit: `Assets/NTSD/Scripts/App/BattleBackgroundPlatformPresentation.cs` and its focused tests `Assets/NTSD/Scripts/Test/Editor/BattleBackgroundPlatformPresentationEditorTests.cs`. Cap the background-derived view to the formal 1333×730 logical-pixel extent, retain output aspect, map bounds and mobile bottom-gap behavior. Do not edit Scene, DAT, movement, input, UI, simulation camera state or background sprite. Since the formal 1333:730 aspect differs from 16:9, fit inside both limits without stretching; quantify the resulting horizontal ratio.

Risk and acceptance: a fixed-center cropped view can hide a runner near stage ends. This batch must report that limit explicitly and must not claim full camera behavior parity. Compile in the original Editor, run the exact background-presentation tests, check real Battle Scene camera size and Scene SHA, and check all battle logic positions unchanged. Long-run follow, if required, needs a separate authority-backed presentation/coordinate task. Roll back through a reviewed inverse patch to these two files only; preserve unrelated dirty work.
