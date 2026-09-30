<!-- CHANGE-RECORD
id: NTSD28-Q09-P02-LEGACY-SHADOW-MOTION-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q09KarinState9997BattlePlayProbeEditor.cs
authority: formal Logan render_snapshot.cpp and presentation_interpolation.cpp; original-Battle Legacy-born shadow pixel witness
evidence: artifacts/diagnostics/NTSD28-Q09-P02-VISIBLE-CONSUMERS-001/LEGACY-SHADOW-MOTION-BATTLE-PLAY-20260929.md
-->

# NTSD28-Q09-P02-LEGACY-SHADOW-MOTION-001

Pre-script state: the original Battle LegacyOnly boot now creates a visible ordinary-character shadow, but the existing display-motion Play probe switched a CentralOnly-born actor to Legacy and had no shadow to sample. No original-Battle adjacent-frame Legacy-born shadow motion has been recorded. The task above freezes the only script path, request/field scope, validation, side effects, protection and rollback before editing.

Planned diagnostic changes: add one unique request selector and default-false `sampleLegacyShadowMotion`; on the already paused natural Battle World, observe ordinary slot 0 through 30/60/120 display policies with an adjacent published motion pair. Save actual body/shadow positions, alpha, logic tick/checksum and source/view coordinates; restore all probe-controlled state in `Finish` even after an exception. Existing request paths, earlier reports and production behavior are protected.

Pending: script change, generated and original Editor compile, one original-Battle Play report, asset hashes, ordered exit, Ledger validator and diff check. No runtime result is claimed by this pre-edit record.

Post-edit code scope: only `NTSD28Q09KarinState9997BattlePlayProbeEditor.cs` gained a unique opt-in motion request selector, request/report fields, `BeginShadowMotion*`/`ObserveShadowMotion` and `Finish` restoration. It reuses the existing Legacy-before-scene-load temporary GameConfig, natural ordinary slot 0, paused complete-Driver Karin entry and ordered exit. Each 30/60/120 case moves only the probe actor's source/view X by a controlled +20 source pixels, publishes an adjacent frame and samples the existing `PresentLatestFrame` Legacy materializer immediately and after 50 ms. Logic tick/checksum and source/view state are asserted; actor coordinates and render-FPS policy are restored on success or failure. No production, DAT, Scene, resource, saved config or nonbattle file was edited for this package. Generated/original Editor compile and Play remain pending.

2026-09-29 verification: generated Editor build exited 0/0 errors (240 warnings); original Editor Tundra compiled the changed probe and `Assembly-CSharp-Editor.dll` timestamp followed the source. One unique original-Battle Play report `q09-legacy-shadow-motion-20260929-01.json` (SHA-256 `C93F4ADD5617CA9F46D713240CF069971D01005EB0DA79165719D0C6CD107AF6`) returned `LEGACY_SHADOW_MOTION_SAMPLED`, no error. Independent JSON parsing found adjacent 8→9/9→10/10→11, 30 FPS alpha 1→1 with body/shadow delta 0, and 60/120 alpha 0→1 with both X deltas +0.307277 units; checksum unchanged per pair. Driver remained paused and current tick fixed at 8. Owned child/fixture released, objects/slots/borrowers 4/2/2→4/2/2; Editor returned idle/non-Play, Battle Scene clean, four protected asset hashes and old request hash stable. Fresh idle inspection showed one saved CentralOnly GameConfig Asset and no clone. This closes only the Legacy-born ordinary shadow display-motion witness, not natural combat motion or formal EXE same-view parity; P-02/Q09/BATCH-05 remain open, Q07 remains earliest open. Full evidence is the acceptance artifact above.
