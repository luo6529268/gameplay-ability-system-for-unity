<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-F02-EVENT-WITNESS-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/BattleHitCandidateSequenceRunner.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07F02Kind10BattlePlayProbeEditor.cs
authority: 336B44 formal root/playable F02 kind10 and frame40-to-41 events; original Unity Battle Scene event witness
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-F02-EVENT-WITNESS-001.md
-->

# NTSD28-336B44-Q07-F02-EVENT-WITNESS-001

Created before edits to any of the three declared scripts. Previous Unity run-03 reached the same tick-tail action/state/position/velocity/HP/durability/relation values for all three slots at initial+45 ticks (2346/2346); run-05 added `Trans.WaitCounter` and vrest but proved neither requested intra-tick event. Formal root directly emits kind10 applied at ticks31–35 and slot2 frame40→41 at tick39. Scope, required no-op/subscription invariants, validation, first-difference policy and forward-only rollback are in the Task. Only this narrow Editor-only instrumentation is authorized; changing gameplay routing is not.

2026-10-03 scoped completion: `LF2Entity.WriteCurrentFrameId` now exposes an `UNITY_EDITOR`-only frame-write observer after the existing frame assignment; `BattleHitCandidateSequenceRunner` exposes an `UNITY_EDITOR`-only kind10 dispatch-completion observer after the existing consumer call. With no subscriber both are no-ops, and non-Editor builds exclude the fields/calls. The existing F02 Editor probe subscribes only around one production `StepOneTick`, records the callback values, and clears both in `finally` and terminal cleanup. No DAT, battle branch, Scene, map, camera, menu, or result logic changed. The probe also preserves the earlier run-05 diagnostic fields and guards unexpected Play exit.

Generated `Assembly-CSharp-Editor.csproj` compiled with 0 errors/296 warnings. Original Editor imported both observer symbols and the probe; unique opt-in run `f02-kind10-scene-20261003-06` entered original Battle Scene Play, completed 45 production ticks, and exited to a clean Menu with no live Driver World and four protected hashes stable. Direct Unity kind10 `applied` tick sequence `[31,32,33,34,34,35,35]` exactly equals formal root; slot2 frame `40→41` occurred in both at ticks `[30,39]`. Initial+45 tick, 3-slot×17-field comparison matched 2346/2346 with no first difference. [Paired event result](../../../artifacts/diagnostics/NTSD28-336B44-Q07-F02-EVENT-WITNESS-001/paired-events-20261003-06.json), [raw Scene result](../../../artifacts/diagnostics/NTSD28-336B44-Q07-F02-NATURAL-SCENE-001/f02-kind10-scene-20261003-06/00055.json). This closes only this direct-event witness. Physical keyboard, GPU Game View and full pool/borrow accounting remain unverified; F02, Q07, Q12 and overall goal stay open.
