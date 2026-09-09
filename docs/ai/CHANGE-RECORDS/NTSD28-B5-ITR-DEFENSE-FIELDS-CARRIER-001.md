# NTSD28-B5-ITR-DEFENSE-FIELDS-CARRIER-001 — ITR defense field carriers

<!-- CHANGE-RECORD
id: NTSD28-B5-ITR-DEFENSE-FIELDS-CARRIER-001
status: VERIFIED
change-kind: TEST_FIRST_DATA_CARRIER
code-path: Assets/NTSD/Scripts/Animation/LF2FrameData.cs
code-path: Assets/NTSD/Scripts/DatParser/Runtime/Utils/Lf2DatConverter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5ItrDefenseFieldsCarrierEditorTests.cs
authority: NTSD 2.8-Logan combat_records.h InteractionRecord28::spark/dbdefend, combat_records.cpp exact parser/copy order, defense_resolution.cpp match_ordinary; EXE B1E13AE1, closure 39DDDA15.
evidence: RED-4 / FOCUSED-4 / B5-269 / HITPLAN-184 / NTSD28-BROAD-745 / SELFCHECK-PASS / SCENE-UNCHANGED / LEDGER-PASS
-->

> 状态：`VERIFIED / CARRIER_READY / BEHAVIOR_UNCONNECTED`

## 原状与边界

Unity `InteractionArea` 有 `recover/bdefend`，但缺 `spark/dbdefend`；converter、CopyFrom 与 HitPlan projection/fingerprint
均无法承载 ordinary-defense 的完整 Authority 输入。当前正式 ITR/weapon-strength 内容未显式声明二者，默认值应保持 0。

本包只补 typed data contract，不接 selection/damage/rest production，不改 content/Scene。回滚为删除两个字段及其
parser/copy/projection/fingerprint 映射和 focused test。

## 验收状态

- `InteractionArea`、converter、CopyFrom、HitPlan raw/projection fingerprint、projection constructor 与 kind5 replacement
  已完整承载 `spark/dbdefend`；缺失 token 保持 default 0。
- red `f48684ea52144bceae017beaa2f59826` 4/4；focused
  `1ab891ed476240ef87b227e975f6610c` 4/4。
- B5 `2ad1951e7f3c4d43ad42a6c5ad07cf90` 269/269；HitPlan
  `d66f5bc32d6e4002aaaca9e5db76477b` 184/184；broad
  `6c5f4a3e449b4be3a01085f4db404465` 745/745。
- Runtime/Editor compile `23:11:31Z / 23:11:32Z`；23:18:31Z SelfCheck PASS；filtered CS0；
  Scene unchanged；Ledger 285/247 PASS。

production/content 未改；下一 atomic ordinary-defense/reduced-hit integration。
