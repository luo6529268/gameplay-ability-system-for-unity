# NTSD28-B5-DEFINITION-ATTACKING-CARRIER-001

<!-- CHANGE-RECORD
id: NTSD28-B5-DEFINITION-ATTACKING-CARRIER-001
status: VERIFIED
change-kind: TEST_FIRST_DATA_CARRIER
code-path: Assets/NTSD/Scripts/Animation/LF2CharacterData.cs
code-path: Assets/NTSD/Scripts/DatParser/Runtime/Utils/Lf2DatConverter.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5DefinitionAttackingCarrierEditorTests.cs
authority: NTSD 2.8-Logan native_attacking_injury28 and ObjectDefinition28 stats.attacking; EXE B1E13AE1, closure 39DDDA15.
evidence: red fff2515ecd114ff6825ba64fb4a69613 4/4 expected failures; focused 7760ffbf9670428a8a89496a388d55cc 4/4 pass; B5 d4760ca7319c44dc9d6625c9832311ec 328/328 pass; NTSD28 broad 1c285dc929874fe9b974723f8a735a08 804/804 pass; BattleRuntimeSelfCheck PASS 2026-09-06T01:28:12Z; Console only 7 intentional rest-binding negative-path errors; scene baseline D4266C6D...583B unchanged; ledger PASS.
-->

> 状态：`VERIFIED / DATA_CARRIER_READY / PRODUCTION_CONSUMPTION_UNCONNECTED`

Unity已有exact integer helper与active-mode field，但LF2CharacterData/正式converter缺`stats.attacking` carrier。
本包只补数据，不接行为或content。回滚删除field/test/meta及converter case/reset。

## 实际修改

- `LF2CharacterData.definition_attacking`：保存Authority definition `<stats>.attacking` signed scalar，缺失默认0。
- `Lf2DatConverter.ApplyNativeInputDefinitionData(...)`：每次Apply先清0，只从`stats` block映射，重复scalar last-win。
- `NTSD28B5DefinitionAttackingCarrierEditorTests`：覆盖signed parse、last-win、重复Apply stale clear与`wpoint.attacking`隔离。

## 验证结果

- test-first red：`fff2515ecd114ff6825ba64fb4a69613`，4/4按预期失败（字段尚不存在）。
- focused：`7760ffbf9670428a8a89496a388d55cc`，4/4通过。
- B5：`d4760ca7319c44dc9d6625c9832311ec`，328/328通过。
- NTSD28 broad：`1c285dc929874fe9b974723f8a735a08`，804/804通过。
- `BattleRuntimeSelfCheck`：`PASS`，结果时间`2026-09-06T01:28:12Z`。
- Console：仅7条self-check故意触发的rest-binding负路径错误，无编译错误。
- `NTSD_Battle.unity`：SHA-256仍为`D4266C6D0A802975B50E481E1D01662DC79AEF168C6E2DE14AE382396FDA583B`，长度205625、mtime未变。

## 剩余边界

本包不读取该字段来改变命中结果；下一包先把runtime armor HP与HP/MP消费累计纳入HitPlan capture/compare，
再由独立原子生产包接selection→match→activation→reduced/fallback→break。
