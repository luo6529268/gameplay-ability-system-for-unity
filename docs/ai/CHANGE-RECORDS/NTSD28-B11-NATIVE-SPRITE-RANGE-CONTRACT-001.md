<!-- CHANGE-RECORD
id: NTSD28-B11-NATIVE-SPRITE-RANGE-CONTRACT-001
status: VERIFIED
change-kind: SOURCE_SPECIFIC_NATIVE_SPRITE_RANGE_ADMISSION
code-path: Assets/NTSD/Scripts/DatParser/Runtime/Parsing/Lf2DatParserV2.cs
code-path: Assets/NTSD/Scripts/Animation/Manager/CharacterAnimtorManager.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B11NativeSpriteRangeEditorTests.cs
authority: User active goal/D-023; native dat_parser project_sprite_sheet 192-220 and SpriteFrameResolver28 resolve 1104-1147; formal EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033; playable build closure verified.
evidence: VERIFIED_SOURCE_RANGE_ADMISSION_ONLY / RED_10_FAIL / UNITY_14_OF_14 / NATIVE_405_DAT_773_SHEETS_MATCH / CS_0 / SCENE_CLEAN / NO_SCHEMA_OR_CONTENT_MIGRATION
-->

# NTSD28-B11-NATIVE-SPRITE-RANGE-CONTRACT-001

准确实现、文件与验收见同ID Task。原状：Parser拒绝reversed，CharacterData将declared直接给sprite/catalog消费者。改后新增明确Logan内容入口，保留AST声明、在内容入口统一累计effective，沿原有SpriteFileInfo/矩形/catalog代码消费；旧入口继续原语义，资源暂未切换。

副作用限解析/转换时新配置对象和日志，无新World/worker/pool/renderer/cache owner，不改变tick/关闭/Mono架构/版本。四个私有Extract/Apply无instance字段读写，静态化只是复用同一builder；旧BuildCharacterDataFromDat实例反射签名保留。source配置builder不自动发布，不让新source混入旧全局lookup。

风险：完整frame转换仍有Q03已记录拒绝；range源入口成功不代表全新内容可用。P-21 PNG alpha、catalog/source/cache publication与Q07资源迁移仍后继。D-022 schema12/20/23与字段形状不变；Q01旧Unity capture仍是冻结历史证据，parser源hash改变后不得把旧capture当当前source证明，当前native source/input身份另验。

回滚为本包三脚本的精确diff及新增test.meta，保留既有Q01/Q02用户工作，不做破坏性Git；任何删除按既有批准规则处理。

## 验证记录

Task/Record在脚本前建立。真实Unity RED job e8c86fc4a9874e578c4e4cf8ed19dc00：10项全部因新API缺失失败，证据red-result.json。现已修改声明的两个production脚本并增加测试；新增ParseLoganContent/ParseCore、BuildCharacterDataFromSource/BuildCharacterDataCore/BuildSpriteFilesForSource，四个原stateless helper为static；旧入口保留。GREEN/最终编译/场景检查待执行。authority-identity.json已确认正式EXE、405 DAT、40 source/header和冻结native capture一致。

最终：`VERIFIED_SOURCE_RANGE_ADMISSION_ONLY`，准确范围并非全B11。Unity最终14/14（job4257ecda97e44b7abcc978d728373e12）；405DAT/773sheet native capture投影一致；4正式DAT配对builder与真实SpriteCatalog矩形通过；CS0、Scene dirtyfalse、3056既有文件原样/3声明变化/0缺失、Ledger465/18PASS。第一次实现后的唯一失败为NUnit展开反射异常的测试期望，改成直接OverflowException断言后全绿。详细命令、原始结果、改前后职责和未验项见 `artifacts/diagnostics/NTSD28-B11-NATIVE-SPRITE-RANGE-CONTRACT-001/Q02-C-REPORT.md`。P-21 alpha、catalog/cache/publication、Q03 Converter、Q07迁移与最终Play/GPU仍后继；没有新增生命周期owner。
