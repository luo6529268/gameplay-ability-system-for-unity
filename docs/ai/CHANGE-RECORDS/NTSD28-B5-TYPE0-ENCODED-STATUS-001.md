# NTSD28-B5-TYPE0-ENCODED-STATUS-001 — type0 confirmed encoded status producer

<!-- CHANGE-RECORD
id: NTSD28-B5-TYPE0-ENCODED-STATUS-001
status: VERIFIED
change-kind: TEST_FIRST_ORDERED_RNG_PRODUCER
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5Type0EncodedStatusEditorTests.cs
authority: NTSD 2.8-Logan apply_confirmed_input_statuses / apply_encoded_status in confirmed standard-hit type0 live path; EXE B1E13AE1, closure 39DDDA15.
evidence: 13-CALLSITE-ORDER-READ / ENCODED-CHANCE-PAYLOAD-READ / POISON-SPECIAL-CODEC-READ / CONFUS-7-CALL-PERMUTATION-READ / TEST-FIRST-COMPILE-RED-CS0117-X3 / COMPILE0 / FOCUSED4 / RELATED216 / NTSD28-BROAD567 / SELFCHECK-PASS-2026-09-05T10:42:11Z / SCENE-UNCHANGED / CONSOLE0 / LEDGER-PASS
-->

> 状态：`VERIFIED / TYPE0_ENCODED_PRODUCER_ALIGNED / ARM_AND_OTHER_TYPES_DEFERRED`

只实现type0 confirmed encoded status producer；join/mimic immediate side effects、unarmored arm及其他target type后置。

## 当前实施与证据

- test-first导入得到3个预期`CS0117`缺producer method。
- pure producer已实现13个固定callsite顺序、poison codec、encoded chance/payload及confus 7-call collision-free remap；0值不消费。
- `ApplyStandardCharacterDamage`在vital写后、reaction前调用同一producer；未接arm/join-mimic/其他types。
- fresh compile error0；focused `d3124a84908244c5886e6fedfbacd94d` 4/4；RNG/hit related `f41c5ff0340f43ba8b201cebcac2da05` 216/216。
- 精确NTSD28 broad `bc7e64a8d59449308c71331f844079e1` 567/567；SelfCheck
  `2026-09-05T10:42:11Z` PASS；Scene/Console/Ledger通过。
- type0 encoded producer闭合；arm/join-mimic/other types仍后置。
