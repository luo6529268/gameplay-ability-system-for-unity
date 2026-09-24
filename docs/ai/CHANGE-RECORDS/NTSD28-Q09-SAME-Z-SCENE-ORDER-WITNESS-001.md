<!-- CHANGE-RECORD
id: NTSD28-Q09-SAME-Z-SCENE-ORDER-WITNESS-001
status: VERIFIED
change-kind: TEST_ONLY_BATTLE_SCENE_PRESENTATION_WITNESS
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q09SameZSceneOrderPlayProbeEditor.cs
authority: Formal playable render_snapshot.cpp equal-Z entity command painter order and user Q09 alignment objective
evidence: docs/ai/TASKS/NTSD28-Q09-SAME-Z-SCENE-ORDER-WITNESS-001.md
-->

# 原 Battle Scene 同 Z 顺序见证

Pre-change: production painter-order comparators/radix are written and focused/self-check pass, but R07B Play probe waits for an old DAT `hit_Fa` prerequisite and yields no P-04 witness. Add a single Editor-only request probe that uses the production World and safely scoped transient entities, observes the materialized order, and reports exact cleanup. Risks: touching a live World, worker in-flight, or stale publication; mitigate by requiring CentralOnly, pausing at a safe boundary, checking handles/slot generations, and restoring pause and entities in `finally`. Acceptance and rollback are in the Task Contract.

Editor-only request probe script written; compile and Play not yet run. It requires the original Battle Scene, CentralOnly, tick>=5, pauses at a worker-safe boundary, registers two same-Z logic-only entities, captures materialized order/ranks, then unregisters and restores pause. It reports GPU submission as not witnessed.

First original Scene request result: FAIL at the probe's 45 s startup deadline before World/tick>=5, with Editor.log still logging BattleTestBootstrap formal PNG loading. This is an unobserved startup gate, not P-04 behavior. Probe startup deadline increased to bounded 180 s and failure diagnostics now capture driver/World/tick; no production path changed. First result retained in Temp until next request consumes it; archive before rerun.

Final scoped validation: original Editor imported the script and generated .meta; Assembly-CSharp-Editor.dll updated after the script, editor returned idle/is_compiling=false and no error CS appeared. First startup request timed out during content load; archived first-startup-timeout.json. After bounded 180 s startup wait, original NTSD_Battle Play result scene-order-pass.json was PASS at CentralOnly tick5: slots 50/51 generation1, same Z240, ranks 1/0, sorting orders 5/1, baseline objects 4->4 and claimed slots 2->2, pause restored. Editor independently observed exited Play; Battle Scene disk SHA unchanged. Root reviewed final diff; independent reviewer found no P0-P2 for this narrow witness. The probe explicitly calls MaterializePresentationOrder and has zero drawable commands, so this is not natural LateUpdate, GPU/pixel, Legacy or formal EXE parity. Test-only code and Unity-generated .meta are the actual changed files; production behavior unchanged. Full evidence: artifacts/diagnostics/NTSD28-Q09-SAME-Z-SCENE-ORDER-WITNESS-001/ACCEPTANCE.md. Status VERIFIED applies only to the scoped scene materialized-order witness; parent P-04/Q09 remain open.

Post-run Scene hash correction: immediately after the PASS and Play exit, Battle Scene was still 9409F2...3B3A39. At 20:14:05 the disk Scene changed to 7D7286...0E567; current diff has two Camera m_Enabled 1->0. This was after the result timestamp 20:12:16; writer/intent unconfirmed. Probe contains no Scene save and does not claim current Scene unchanged. Preserve this parallel change; subsequent pixel witness must check camera state.
