# Task Contract — NTSD28-B5-DEFINITION-ATTACKING-CARRIER-001

> 状态：`VERIFIED / DATA_CARRIER_READY / PRODUCTION_CONSUMPTION_UNCONNECTED`
> 依赖：`NTSD28-B5-TYPE1-ARMOR-ATOMIC-PRODUCTION-AUDIT-001 / VERIFIED`

## 目标

为Authority `<stats>.attacking`建立Unity typed definition carrier，并接入正式DAT converter；为armor matcher、
weapon durability与hit-resource共同提供同一原始definition倍率。

## 允许路径

- `Assets/NTSD/Scripts/Animation/LF2CharacterData.cs`
- `Assets/NTSD/Scripts/DatParser/Runtime/Utils/Lf2DatConverter.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B5DefinitionAttackingCarrierEditorTests.cs` 与 `.meta`
- 本Task/Change、Ledger、STATE、handoff、总表与armor manifest

## 不变量

- 只读取`stats` block的`attacking`；scalar last-win，缺失/重复Apply重置为0。
- 不把wpoint/itr的同名字段误映射到definition carrier。
- 保留signed int parser语义；本包不消费active-mode、不改变伤害或资源行为。
- 不修改content/Scene。

## 验收

test-first覆盖stats parse、last-win、same-name isolation、stale clear和正式loader既有调用链；随后compile、focused、
B5、NTSD28 broad、SelfCheck、Console、Scene与Ledger。

## 完成证据

red `fff2515ecd114ff6825ba64fb4a69613` 4/4预期失败；focused
`7760ffbf9670428a8a89496a388d55cc` 4/4、B5 `d4760ca7319c44dc9d6625c9832311ec`
328/328、NTSD28 broad `1c285dc929874fe9b974723f8a735a08` 804/804均通过；SelfCheck PASS，
Console仅7条已知负路径日志，Scene基线与Ledger均通过。
