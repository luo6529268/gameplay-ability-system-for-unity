# NTSD28-B2-NATIVE-TYPE0-BUILTIN-DATA-SEAMS-001 — native built-in data seams

<!-- CHANGE-RECORD
id: NTSD28-B2-NATIVE-TYPE0-BUILTIN-DATA-SEAMS-001
status: FOCUSED_TEST_PASS
change-kind: CODE_AND_TEST
code-path: Assets/NTSD/Scripts/Animation/LF2CharacterData.cs
code-path: Assets/NTSD/Scripts/DatParser/Runtime/Models/Lf2BmpSection.cs
code-path: Assets/NTSD/Scripts/DatParser/Runtime/Parsing/Lf2DatParserV2.cs
code-path: Assets/NTSD/Scripts/DatParser/Runtime/Utils/Lf2DatConverter.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28NativeType0BuiltinDataSeamsEditorTests.cs
authority: NTSD 2.8-Logan input_routing.cpp movement_action/linked_stats_action and their live type-0/common built-in callers; input_routing_tests.cpp custom sequence and linked selector cases in main closure.
evidence: TASK-CONTRACT-CREATED / SOURCE-CHAIN-CLOSED / UNITY-MISSING-4-SEQUENCES-AND-9-STATS / TEST-FIRST-RED-37-EXPECTED-CS-ERRORS / CURRENT-COMPILE-SEGMENT-OUTSIDE-NEW-TEST-0 / DATA-SEAM-CODE-WRITTEN / UNITY-COMPILE-0 / FOCUSED-6-6 / PARSER-CARRIER-20-20 / B2-BROAD-247-247 / SELFCHECK-PASS-151800 / EXPECTED-NEGATIVE-LOGS-7-THEN-CONSOLE-0 / LEDGER-134-89 / PRODUCTION-BUILTINS-UNCONNECTED / CONFIG-DAT-SCENE-UNCHANGED / JOINT-TRACE-PENDING
-->

> 状态：`FOCUSED_TEST_PASS / DATA_SEAMS_READY / CONSUMERS_UNCONNECTED`

## 改前事实

- V2 tokenizer保留sequence token，但parser只把`*_frame:`后的count记为普通property，动作列表被丢弃。
- `LF2CharacterData`只有固定walking/running rate/speed及`recmp/caughtact/use_ai`，没有4组动作sequence
  或9个linked action selector。
- 当前legacy movement/weapon选择使用固定action或Unity对象查询；不能定义2.8 native built-ins。

## 预期改后职责

- `Lf2BmpSection`以有序model保存4种命名sequence。
- `Lf2DatParserV2`按count读取inline/multiline动作，严格停在声明边界。
- `Lf2DatConverter`将sequence及9个stats字段复制到每个独立`LF2CharacterData`实例。
- runtime consumer保持不变，后续ground/air built-ins包才读取这些seam。

## 验证记录

- Task Contract已建立。
- 新增6项focused contract后，按最后一次`Requested through public api`编译段切片得到37条唯一
  预期C#错误；全部仅在新测试中引用缺失sequence model/carrier与stats carrier，新测试外错误0。
  Editor.log历史上的上一包31条红错不计入本次证据。
- data seam代码已写：BMP model保存有序sequence；V2 parser按count读取inline/multiline动作并停在边界；
  character data新增独立4 List与9个absent→0 stats；converter每次clear/replace。`git diff --check`
  通过；Unity compile/focused尚待。
- final validation：最后编译段0 error、Tundra success；new job
  `a91ed9d7aa3c4d7c9ce37196cf04c478` 6/6；parser/carrier job
  `34187adb2e704624aa6578b57747ebb7` 20/20；B2 broad job
  `c78df034ae4c4e87baeb7b1f5f246652` 247/247。
- request-file full SelfCheck于15:18:00写出`PASS`；Console仅7条既有负向rest-binding夹具错误，
  清除后fresh error0。`git diff --check`与Change Ledger通过（134 records / 89 governed code files）。
- Config/DAT、Scene/Prefab、ProjectSettings、runtime input consumer、RNG和authority均未由本包修改；
  existing Scene dirty state保持。built-ins与joint trace仍未完成。
