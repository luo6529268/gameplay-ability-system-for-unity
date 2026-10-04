<!-- CHANGE-RECORD
id: NTSD-BUTTON-RIPPLE-POOL-001
status: RUNTIME_PENDING
change-kind: CODE
code-path: Assets/NTSD/Scripts/UI/NTSDButtonRipple.cs
authority: User2026-10-04 05:38 UTC independent overlapping rings request
evidence: docs/ai/FILE-OPERATIONS/NTSD-BUTTON-RIPPLE-POOL-001/RECORD.md
-->
# NTSD-BUTTON-RIPPLE-POOL-001
Authority: user2026-10-04 05:38 UTC explicitly requires independent rings for repeated effective presses; later presses must not reset active rings. Scope NTSDButtonRipple.cs plus existing opt-in preview prefab, preserve sprite/style parameters and original scenes/button input.
Design: prewarm configurable pool of Image+independent cached Tween slots. On first aggregate press acquire inactive slot, grow when all busy without overwriting active effects; recycle on completion; retain peak capacity until component destruction. No per-tap Instantiate/Destroy after warmup, no max that discards valid ordinary taps. Repeated simultaneous-pointer Down remains governed by NTSDButton aggregation. Release never spawns. Config snapshot per emission. No new dependency/shader/simulation change.
Lifecycle: OnDisable unsubscribes and pauses/hides/returns all slots; focus/pause/invalid button stops all. OnDestroy kills every tween and destroys only internally owned extra Images; supplied template Image is not destroyed. UI-local pool has no World/worker/OPoint dependencies and uses existing disable-before-destroy shutdown integration. No singleton or service recreation. Idempotent cleanup.
Acceptance: actual isolated Unity compile and Play with exact source. At least3 live distinct radii/progress values; earlier ring progresses rather than restarts; growth beyond prewarm; all complete/reuse; disable/re-enable and no duplicate subscription; focus/pause; normal single click, multi-pointer held aggregation, destruction cleanup. Actual rendered GIF upload to Library. Original Scene ownership still unconfirmed, do not control original Editor. Rollback exact source/prefab before-0/1 backups with new audited operation and later-change checks; no irreversible boundary.

CODE_WRITTEN: pooled Image/RingSlot state with independent cached Tween, free-slot scan then growth; completion recycles, all lifecycle stops return slots and destroy kills owned tweens/images. Initial size4 configurable1..16, no active overwrite limit. Parameters unchanged.

NTSD-BUTTON-RIPPLE-POOL-001 / RUNTIME_PENDING (original Scene/device); isolated real Unity compile and Play PASS
User approval2026-10-04 05:38 UTC supersedes previous restart-one-ring visual policy.
Changed production files: Assets/NTSD/Scripts/UI/NTSDButtonRipple.cs and Assets/NTSD/Prefabs/NTSDButtonRipplePreview.prefab. Prefab delta only initialPoolSize:4; GUIDs, icon/ring sprite, size/color/duration/fade curve unchanged.
Every effective aggregate press now acquires a free Image+independent Tween/progress/config slot. Four Images prewarmed by default (Inspector1..16); first use creates that slot's cached Tween. No free slot -> grow rather than interrupt active ring. Completion hides and marks free; future taps reuse image/tween. Peak-demand capacity retained until destroy; growth allocations occur only when concurrency exceeds capacity, no per-click destruction. No click latch/hold change, no per-finger duplicate emission, no battle/runtime changes.
Lifecycle: unsubscribe and stop/recycle all on component disable; focus/pause and invalid Button stop all; destruction kills every Tween and destroys only owned extra Images, not supplied ring template. No world/pool manager dependency. Submit remains visual-only. Template Image raycastTargetfalse; extra Images use its sprite/material/maskability and also raycastTargetfalse.

Validation: separate Unity2022.3.62f3 project C:/Users/Logan/AppData/Local/Temp/NTSDRipplePoolPreview-001, separate Library, original Editor untouched. Exact final production source bytes compared with compiled preview copy. Unity compile no CS errors; real Play Mode, real uGUI/DOTween, ExecuteEvents input, Camera render. unity-checks.txt:27 assertions PASS. Three concurrent rings with strictly different sizes and earlier progress not reset; five concurrent rings grow pool4->5; natural completion/free reuse; disable/reenable one event subscription; focus and pause cleanup/recovery; second finger/duplicate Down no extra emission; button-only disable/noninteractable tail cleanup; normal single click/release; Submit; all five Tweens inactive after actual destroy.
Focus/pause are invoked through SendMessage, not real OS events. No physical touchscreen or original Battle Scene integration tested; original scene/Button hashes unchanged. No dependency installation, Shader, DAT, deleted HUD test, commit or push.

Preview: NTSDButton-Multiple-Rings-Unity-Play.gif assembled from100 genuine Unity-rendered frames (identical frames merged by GIF encoder to81). Unity-three-rings.png shows3 simultaneous distinct rings. Original captures in isolated project's Capture folder; test driver/entry snapshots archived as .cs.txt. No re-drawn animation.
Library create succeeded: libfile_870fcb6d1f148191a5193c4abf5538fd, file_00000000c8d081fd8c762cd1443af398, version0, /NTSDButton-Multiple-Rings-Unity-Play.gif. Local metadata helper fails because Windows Python lacks os.setxattr; upload itself succeeded and identity preserved in library-identity.json. No upload retry.

Integration: existing opt-in prefab now carries pool size4; drag under Canvas for local preview, or attach component with ring Image to selected existing button when Scene ownership is clear. Original Battle/Menu Scene still not edited. Existing previous single-ring report is historical; this record supersedes only its restart policy.
