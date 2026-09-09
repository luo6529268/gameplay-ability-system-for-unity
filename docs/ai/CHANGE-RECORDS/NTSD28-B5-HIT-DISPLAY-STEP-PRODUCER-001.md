# NTSD28-B5-HIT-DISPLAY-STEP-PRODUCER-001 — hit display-step producer

<!-- CHANGE-RECORD
id: NTSD28-B5-HIT-DISPLAY-STEP-PRODUCER-001
status: VERIFIED
change-kind: TEST_FIRST_DISPLAY_STEP_PRODUCER
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5HitDisplayStepProducerEditorTests.cs
authority: NTSD 2.8-Logan apply_native_hit_resource_transfer positive-injury pre-local-gate writes; EXE B1E13AE1, closure 39DDDA15.
evidence: HIT-RESOURCE-PREREQUISITE-AUDIT-VERIFIED / FOUR-CARRIERS-READY / TEST-FIRST-COMPILE-RED-CS0117-X2 / COMPILE0 / FOCUSED10 / RELATED255 / NTSD28-BROAD616 / SELFCHECK-PASS-2026-09-05T12:00:34Z / SCENE-UNCHANGED
-->

> 状态：`VERIFIED / TYPE0_5_DISPLAY_STEPS_ALIGNED / TYPE6_SKIP_VERIFIED`

focused test已先写入，fresh compile得到2个预期`CS0117`。

- pure helper仅在positive injury写score/damage/current-HP=`/10`、effective-max-HP=`/20`；
  zero/negative保留旧值。
- type0 standard、type1/2/4 weapon、type3/5 special/other production seam已接；type6跳过。
- fresh compile 0 error；focused `f81627c6d6eb4580806ff3635440fc69` 10/10；
  related `be27e146083948e692371f69dd1121ba` 255/255；精确NTSD28 broad
  `b8cff2f87b7d49749331c52ab4199326` 616/616。
- BattleRuntimeSelfCheck `2026-09-05T12:00:34Z` PASS；Scene SHA
  `0D74E174D37AF673CB717C699D13F3D115C65EDD332E373B6673757D5E323D77`、203477 bytes、
  mtime unchanged。MP resource transaction仍后置。
