<!-- CHANGE-RECORD
id: NTSD-BUTTON-RIPPLE-001
status: IN_PROGRESS
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
