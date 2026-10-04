<!-- CHANGE-RECORD
id: NTSD-BUTTON-RIPPLE-001
status: RUNTIME_PENDING
change-kind: CODE
code-path: Assets/NTSD/Scripts/UI/NTSDButtonRipple.cs
authority: User authorizes centered cyan ring feedback implementation and actual Unity preview
evidence: docs/ai/FILE-OPERATIONS/NTSD-BUTTON-RIPPLE-001/RECORD.md
-->
# NTSD-BUTTON-RIPPLE-001
User approval: source thread01a0ef91-2a9a-763b-af75-4367dfcd1020 asks implementation and actual Unity preview of centered cyan expanding/fading ring. Independent opt-in NTSDButtonRipple component + Image + existing DOTween. First aggregate press triggers immediately, release ignored; Submit feedback does not invoke button events. Cached tween restarts on rapid taps; no per-tap objects. Serialized color, start/end size, duration, fade curve. Disable/destroy/focus/pause cleanup and subscription symmetry. No battle rules, DAT, input samples or NTSDButton changes. Existing scenes protected (ownership unknown); standalone preview project has separate Library. No plugin/dependency installation.
New code path Assets/NTSD/Scripts/UI/NTSDButtonRipple.cs with meta. Optional new preview prefab/art sprite only after resource decision. Independent diagnostics outside Assets; preserve existing/deleted HUD tests.
Shutdown: local UI effect stops/unsubscribes on OnDisable before object destroy; OnDestroy idempotently kills owned tween, never auto-creates service/spawn. No world/pool/worker dependencies or change to ordered shutdown phases.
Validation: compile against actual Unity/DOTween API; isolated lifecycle/press tests; attempt real Unity rendered preview with exact source and declared sprite. Report Editor/device limitations. Rollback remove only this task's new component/meta/optional new assets under separately recorded operation; restore append docs only with later-change checks. Visual adaptation only, no irreversible behavior boundary.

CODE_WRITTEN: new independent NTSDButtonRipple source/meta; exact field and lifecycle implementation as Task. No existing source edits. Real compile/render verification next.

NTSD-BUTTON-RIPPLE-001 / RUNTIME_PENDING (original battle integration), isolated Unity preview PASS

Deliverables:
Assets/NTSD/Scripts/UI/NTSDButtonRipple.cs (+meta): opt-in component. Serialized ring Image/color/startSize/endSize/duration/fadeCurve. Defaults cyan alpha0.85, image sizes145->230 (110px preview icon), duration0.45s. One cached normalized DOTween, unscaled update, restart on rapid clicks. First aggregate pressed event starts immediately; normal release ignored; Submit animates without invoking click or setting held. No input/NTSDButton changes.
Assets/NTSD/Sprite/UIPanels/BattleHud/ButtonRippleRing.png (+meta): newly authorized256px white transparent soft ring, generated deterministically with Unity Texture2D radial alpha, no custom shader. Sprite/alpha transparency/bilinear/clamp/no mipmap/uncompressed. Image tints it cyan.
Assets/NTSD/Prefabs/NTSDButtonRipplePreview.prefab (+meta): standalone110px Attack button using the unchanged existing icon, ring child (raycastTarget=false), and assigned component. No world/controller binding. No automatic attachment to existing battle buttons. Drop this prefab under a Canvas for user preview; for existing buttons add NTSDButtonRipple and assign a centered child Image using the ring Sprite, keeping existing button/controller bindings.

Validation:
Actual isolated Unity2022.3.62f3 project at C:/Users/Logan/AppData/Local/Temp/NTSDRipplePreview-001, distinct Library. Original editor not controlled. Exact final NTSDButtonRipple and current NTSDButton source compiled in Unity. Real Play Mode with ExecuteEvents PointerDown/Up/Submit and real DOTween/uGUI/Camera rendering; final15 assertions PASS (final-unity-checks.txt, preview4.log). Tests: start hidden/no raycast; immediate down; release continuity; cached rapid replay; two-finger aggregation; natural fade complete; GO disable/re-enable; Submit; focus/pause handler cleanup; button-only disable and interactable=false after release; actual destroy releases tween. Focus/pause called via SendMessage rather than OS focus suspension. No physical touch or original battle-scene Play.
Initial API compile failed: missing facade ref and unsupported Tween.SetDuration; actual implementation corrected to unit-duration tween timeScale; api2 compile0errors/one expected unassigned serialized-field warning. Final exact source compiled by standalone Unity with no CS errors. Initial real preview wrongly assumed captureFramerate controls unscaled time, produced premature fade assertion; raw Capture failure preserved. Later capture uses real-time30Hz sampling; final passes. No errors hidden/deleted.

Actual preview:
NTSDButton-Ripple-Unity-Play.gif is assembled from80 real Unity-rendered Capture3 frames (GIF merges identical frames to62, preserves timing). It is not a redrawn simulation. Render screenshot Unity-Play-frame-040.png. Final follow-up changes only invalid-button animation-tail cleanup, no visual parameters; Capture4 tests final source.
Library upload succeeded: libfile_e7092c799da08191af6ef4e51d3af3b0; file_0000000029c881fd8ae4945bfbb45303; version0; /NTSDButton-Ripple-Unity-Play.gif. Current helper local xattrs failed on Windows (os.setxattr unavailable); Library file is saved, identity persisted separately in library-identity.json. No upload retry.

Scope and remaining work:
Original Battle and Menu scene SHA plus NTSDButton SHA unchanged from prechange.json. Existing serialized button references inspected read-only; original Editor PID138072 running battle logs and no Pipeline connection, no ownership claim or scene save. Prefab all project GUIDs resolved once, remaining Image GUID is installed uGUI. Original scene setup requires user to stop other testing and confirm saved scene/editor available before wiring; alternatively drag the opt-in prefab under a Canvas and enter Play to inspect. No batch changes, shaders, dependencies, DAT, simulation behavior, deleted HUD tests, commits or push. Full original project compile/SelfCheck/device acceptance not claimed.
