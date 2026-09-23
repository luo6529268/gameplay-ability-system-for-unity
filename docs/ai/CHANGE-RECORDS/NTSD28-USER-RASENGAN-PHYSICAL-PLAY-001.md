<!-- CHANGE-RECORD
id: NTSD28-USER-RASENGAN-PHYSICAL-PLAY-001
status: FOCUSED_TEST_PASS
change-kind: USER_REPORTED_RASENGAN_PHYSICAL_PLAY_DIAGNOSTIC
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UserRasenganPhysicalPlayProbeEditor.cs
authority: formal NTSD 2.8-Logan paired playable source and user physical Naruto skill report
evidence: artifacts/diagnostics/NTSD28-USER-NARUTO-RUN-ATTACK-WINDOW-001/FULL-WINDOW-COMPARISON.md
-->

# NTSD28-USER-RASENGAN-PHYSICAL-PLAY-001

Status `FOCUSED_TEST_PASS / DIAGNOSTIC_ONLY / USER_SYMPTOM_OPEN`; Task `docs/ai/TASKS/NTSD28-USER-RASENGAN-PHYSICAL-PLAY-001.md`.

Before: source-model and original-Editor canonical `FrameInputSet` 32-tick attack-window traces match Naruto action/counter/MP; the physical AttackAction callback, human-roster capture, render-visible action and wall-time ordering during the actual Battle Scene have not been measured. Earlier Play bridge snapshots were stale even while `Editor.log` showed BattleTestBootstrap complete; readiness must use the current run's bootstrap marker before invoking the probe.

Intended after: add one Editor-only Play diagnostic menu that observes current Naruto OID2 and its production `CharacterInputModule`, establishes an authored action-241 Play-only initial state, queues physical J at the first visible action-253 observation, and records the next tick input and action transition. It must report failures as failures, save the raw timing evidence, and release the synthetic key. No production behavior, DAT token, Scene, camera, general input binding or nonbattle function is modified.

Expected side effects: new Editor test script/meta and Temp report only. Acceptance: original Editor compiles; targeted Play report contains tick-indexed input/action evidence; Scene SHA remains constant; no DAT/Scene diff; Ledger validator passes. A successful probe is not a formal EXE visual certificate. Rollback is a reviewed inverse of this exact new script and its metadata/registrations.

Initial implementation is the declared Editor-only Play probe. The original Editor compiled it with zero reported Console errors. In the saved Battle Scene's Play clone, the current run's bootstrap marker preceded menu invocation; Naruto OID2 began authored action241 at current tick567. The first action253 was observed at tick591; physical J queued 3.424 ms later; tick592 captured canonical Jump held/pressed32, runtime KeyJump1 and action301. Initial report is frozen as `artifacts/diagnostics/NTSD28-USER-NARUTO-RUN-ATTACK-WINDOW-001/rasengan-physical-first253-play.json`. This confirms the first visible 253 input path in Play, not the user's perceived late input or formal EXE visual boundary.

Amendment before further script edit: add a second menu that queues physical J only after the second observed action-253 tick and writes a distinct report, preserving the first-menu path. Correct the diagnostic row's `counter` to `Runtime.AttackingCounter` rather than `Trans.WaitCounter`; the latter tracked frame binding and was misleading. Make the two-tick hold relative to actual press time. The late case will distinguish the visible-window boundary; no production code, DAT, input binding, Scene or camera edit is allowed.

Final scoped result: the original Editor compiled the amended probe. In a second Play run, Naruto first reached action253 at tick1290 with `AttackingCounter=0`; the probe queued physical J after the second action253 observation at tick1291, then tick1292 carried canonical J held/pressed32, runtime KeyJump1 and action301. Raw report: `artifacts/diagnostics/NTSD28-USER-NARUTO-RUN-ATTACK-WINDOW-001/rasengan-physical-second253-play.json`; the earlier first253 result is preserved separately. Both physical cases passed. The first report's `counter` field was the old frame-binding diagnostic and must not be used as AttackingCounter evidence. Editor Play was stopped; `Assets/NTSD/Scene/NTSD_Battle.unity` SHA-256 remained `9E7B8A91ADD396D8A3674915BB5AC9A03B8D1A2EC817BA3B12F135D03EBA0AC0`; DAT and Scene show no Git diff. Formal DAT Naruto action253 has `hit_a:300`, whereas action254 lacks `hit_a`; the paired playable presentation interpolation changes position, not action/pic. This narrows the open question to formal EXE visible timing versus the Unity view and the user's actual press cue; it does not prove the user symptom fixed or authorize a wider window. No production code or DAT was changed in this package.
