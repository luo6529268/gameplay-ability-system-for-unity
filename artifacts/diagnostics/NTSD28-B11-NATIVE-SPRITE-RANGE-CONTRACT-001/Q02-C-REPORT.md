# Q02-C 原生图片范围接入验证

状态：VERIFIED_SOURCE_RANGE_ADMISSION_ONLY。只涉及新版内容入口，不代表正式资源已切换或 B11 整域完成。

## 权威与差异

正式 EXE SHA-256 为 B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033。playable 闭包中的 `ntsd28_core/src/data/dat_parser.cpp::project_sprite_sheet` 保留声明范围，但实际图片编号从 0 按 `max(row*col,0)` 累加；`render_snapshot.cpp::SpriteFrameResolver28` 按有效范围定位 sheet 与 source rect。Unity 原入口直接使用声明范围，且丢弃反向声明。

`authority-identity.json` 新鲜核对正式 EXE、405 DAT、Q01 原生捕获与其 40 个源码/头文件身份。Q01 原生捕获是 source-model 诊断证据，不是正式 EXE 整场 trace；Q01 原 Unity capture 在本次 parser 修改后只保留历史含义。

## 实现边界

- `Lf2DatParserV2.ParseLoganContent` 接受原生反向/有符号尾端声明；共享 ParseCore，旧 Parse 默认语义保持。
- `CharacterAnimtorManager.BuildCharacterDataFromSource` 配对选择 parser 与 source；共用原有 frame/definition/movement/weapon 转换职责。旧私有实例 BuildCharacterDataFromDat 名字/签名保留。
- `BuildSpriteFilesForSource` 在唯一内容入口投影有效 startFrame/endFrame；AST 原始声明、所有数据类型字段形状保持。旧源路径/声明仍按原规则消费；新源路径使用已有 BattleContentSource。
- 四个仅写入参数对象的 Extract/Apply helper 改为 static 以复用同一转换主体，无新增 manager、World、worker、pool、publication 或生命周期 owner。整数溢出明确拒绝，不定义 C++ 溢出行为。
- SpriteCatalog/矩形计算和全局生产 caller 未改。原生 source 入口为后续 catalog/cache 事务准备；不在旧全局缓存中偷偷混入新资源。

## 已运行与待运行

1. 真实 Unity RED job `e8c86fc4a9874e578c4e4cf8ed19dc00`：10 项全部因新 API 缺失失败，见 red-result.json。
2. 首次实现后 job `265f06ad86c041e6844e0893c99cdfe3`：14 项执行完成，唯一失败是 NUnit 展开反射异常后不再返回 TargetInvocationException。实际 OverflowException 符合实现合同；测试改为直接验证该异常。原结果保留 green-result.json（文件名不构成通过证书）。
3. 该轮 405 DAT / 773 sheet 的原始声明、有效范围、尺寸、行列及路径与冻结 native capture 一致；四份正式 DAT（jugo、jugoCS2、shis、tra）经新配对入口转换成功；真实 SpriteCatalog 的 sheet/rect 与原生夹具预期一致。
4. 最终 job `4257ecda97e44b7abcc978d728373e12`：14/14 PASS（新增12、旧Overlap2）；final-result.json 与 verification-summary.json保留结果。CS error 0，NTSD_Battle isDirty=false/rootCount14，见compile-errors.json/scene-result.json。Ledger 465 records / 18 governed code files PASS。不运行整场 Play 或完整 SelfCheck；本包没有部署内容，不把 admission 证据扩大成战斗表现证书。

## 保护与后继

workspace-protection.json：3059 个启动时既有文件，3056 个原样，3 个声明内变化（本包 parser/manager，加前包 BMPLoader），零缺失；未触及 Config/Sprite/Scene/Prefab/ProjectSettings/第三方/非战斗脚本，schema 12/20/23 保持。

Q02 仍在进行。下一 P-21 `NTSD28-B11-PNG-SHEET-ALPHA-CONTRACT-001` 解决 raw PNG 后 sheet 处理丢半透明；随后完成 catalog.csv/registry_index、source/cache/publication 事务。Q03 的 6 文件 9 帧 converter 拒绝等字段合同独立待处理；Q07 才迁移正式资源。R17 仅 raw 与 range 接入子条件回访，最终视听/GPU/实际内容可用条件保持。

实际命令：通过既有 `Temp/Goal13_bridge.py` 调用 refresh_unity、run_tests（EditMode，NTSD.Test.NTSD28B11NativeSpriteRangeEditorTests 与 NTSD.Test.BattleSpriteOverlappingRangeEditorTests）、get_test_job、read_console(error CS)、manage_scene(get_active)。原生指纹与3059文件保护使用Python hashlib只读复核；审计使用 `Tools/Validate-ChangeLedger.ps1 -RepositoryRoot $PWD.Path`。最终结果只关闭本包内容接入范围。
