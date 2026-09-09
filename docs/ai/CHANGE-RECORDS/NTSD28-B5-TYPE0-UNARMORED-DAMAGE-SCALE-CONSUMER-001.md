# NTSD28-B5-TYPE0-UNARMORED-DAMAGE-SCALE-CONSUMER-001 — type0 unarmored damage scale consumer

<!-- CHANGE-RECORD
id: NTSD28-B5-TYPE0-UNARMORED-DAMAGE-SCALE-CONSUMER-001
status: VERIFIED
change-kind: TEST_FIRST_PRODUCTION_DAMAGE_CONSUMER
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5Type0UnarmoredDamageScaleEditorTests.cs
authority: NTSD 2.8-Logan damage_after_native_target_defend_scale -> effective_injury_after_native_weakness unarmored order and standard type0 mutation; EXE B1E13AE1, closure 39DDDA15.
evidence: DAMAGE-SCALE-EFFECT-AUDIT-VERIFIED / TEST-FIRST-COMPILE-RED-CS0117-X4 / FIRST-FOCUSED-XOR-SENTINEL-CANCELLED-CORRECTED / COMPILE0 / FOCUSED9 / RELATED284 / NTSD28-BROAD664 / SELFCHECK-PASS-2026-09-05T14:14:56Z / CONSOLE0 / SCENE-UNCHANGED / LEDGER-PASS
-->

> 状态：`VERIFIED / TYPE0_UNARMORED_SCALE_WEAK_ALIGNED`

只接type0 unarmored effective injury；raw display、weapon/type3 legacy scale、armor、effect、resource、
producer/content保持不变。

focused test先写入，fresh compile取得4个预期`CS0117`；无范围外编译错误。

- exact low-32 multiply/signed divide及`+340 -> weak/2`顺序闭合。
- type0 HP/HPBound/combo/damage stat/KO消费effective injury，四display step继续消费raw injury。
- fresh compile 0 error；focused `c5839f1d809f4f0c9d2a86e265eda417` 9/9；related
  `48bbea13d4eb402aab55479c1d328ffd` 284/284；精确NTSD28 broad
  `d33507bb8e6b4b12a9226d8c60e688e2` 664/664；14:14:56Z SelfCheck PASS。
- Console 0、Scene unchanged、Ledger PASS；producer/nonchar/armor/effect仍后置。
