# NTSD28-B5-TYPE0-HITPLAN-DAMAGE-SCALE-CORRECTION-001 — type0 hit-plan damage scale correction

<!-- CHANGE-RECORD
id: NTSD28-B5-TYPE0-HITPLAN-DAMAGE-SCALE-CORRECTION-001
status: VERIFIED
change-kind: TEST_FIRST_DIAGNOSTIC_DATAORIENTED_PARITY_FIX
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleHitExecutionPlanEditorTests.cs
authority: NTSD 2.8-Logan unarmored target +340 then attacker weakness; EXE B1E13AE1, closure 39DDDA15.
evidence: TYPE0-PRODUCTION-SCALE-VERIFIED / HITPLAN-RAW-INJURY-FIRST-DIFFERENCE-READ / TEST-FIRST-RED-bcafb1b27f60453eaa2a778fa4e34f96-WRITER-DIFF-0x78000000000000 / COMPILE-0 / FOCUSED-916ef6b283c24c3189fe081da7cc1e71-1OF1 / HITPLAN-b8aed234cd3f454b9908d5ae341e6224-179OF179 / EXACT96-BROAD-eaceb244f4d74ae287c54d468ebb48b6-675OF675 / SELFCHECK-PASS-20260905T153517Z / CONSOLE0 / SCENE-CONCURRENT-USER-EDITOR-CHANGE-PRESERVED
-->

> 状态：`VERIFIED / HITPLAN_PRODUCTION_SCALE_PARITY`

只修type0 hit-plan projection，不改production、raw display/status、armor/content/audio/spark。

## 实际改动与验证

- `ProjectStandardCharacterDamageWriterEffect`现在与production相同，先调用
  `ResolveNativeUnarmoredHpInjury(raw,+340,weak)`，再投影HP/HPBound/combo/stat/lethal。
- raw ITR仍由既有display/status路径消费，没有双重缩放。
- red `bcafb1b27f60453eaa2a778fa4e34f96` 1/1，writer diff `0x78000000000000`。
- compile0；focused `916ef6b283c24c3189fe081da7cc1e71` 1/1；hit-plan
  `b8aed234cd3f454b9908d5ae341e6224` 179/179；exact96 broad
  `eaceb244f4d74ae287c54d468ebb48b6` 675/675；15:35:17Z SelfCheck PASS；Console0。
- Scene在验证期间出现并发用户/Editor修改：SHA变为
  `54D2691C121EBDC2F797B82CD74A6A10448D46FF6E14DD4603D0EBFBC441198F`、205620 bytes、
  mtime `2026-09-05T15:26:46.1274111Z`；diff含新增UI Image、两Camera disabled及序列化runtime字段。
  本包没有通过apply_patch或命令写Scene，故不回退、不纳入本Change，原样保留用户工作。
