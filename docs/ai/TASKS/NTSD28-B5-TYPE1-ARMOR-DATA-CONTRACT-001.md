# Task Contract — NTSD28-B5-TYPE1-ARMOR-DATA-CONTRACT-001

> 状态：`VERIFIED / DATA_CONTRACT_READY / PRODUCTION_SELECTION_UNCONNECTED`
> 依赖：`NTSD28-B5-ORDINARY-DEFENSE-REDUCED-HIT-EXIT-AUDIT-001 / VERIFIED`

## 目标

建立与 Authority `ArmorRecord28` 一一对应的 Unity typed armor definition contract：14个标量、frame ranges、
state/kind/id/effect重复列表、sound1/sound2、深拷贝与稳定fingerprint，并接入正式DAT→character-data加载链。

## 允许路径

- `Assets/NTSD/Scripts/Animation/LF2ArmorData.cs` 与 `.meta`
- `Assets/NTSD/Scripts/Animation/LF2CharacterData.cs`
- `Assets/NTSD/Scripts/DatParser/Runtime/Parsing/Lf2DatParserV2.cs`
- `Assets/NTSD/Scripts/DatParser/Runtime/Utils/Lf2DatConverter.cs`
- `Assets/NTSD/Scripts/Animation/Manager/CharacterAnimtorManager.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B5Type1ArmorDataContractEditorTests.cs` 与 `.meta`
- 本 Task/Change、Ledger、STATE、handoff、总表与 armor manifest

## 不变量

- scalar last-win；`type`存在时优先于`ptype`，否则`ptype` fallback；缺失默认0。
- `frame`每个声明只读取前两个有效整数；state/kind/id/effect每个声明读取首整数并保持声明顺序。
- sound1/2缺失为null，存在则保留最后原始文本；不自行规范路径。
- repeated Apply必须清除旧armor list，不保留stale定义；DeepCopy不能共享可变列表。
- fingerprint覆盖全部行为字段、列表顺序/range边界和sound null/内容，且不分配。
- 不实现match/activation/runtime initialization，不写入`Assets/NTSD/Config`或资源/Scene。

## 验收

test-first覆盖full record、ptype fallback/type precedence、last-win、malformed/default、多block顺序、stale clear、
deep-copy independence、fingerprint差异与warm zero-allocation；随后compile、focused、B5、NTSD28 broad、SelfCheck、
Console、Scene与Ledger。

## 验收结果

- test-first red：`8f8b645f2e824f91bf1288a627dbad54`，6/6按预期失败（typed apply入口尚不存在）。
- 初次实现聚焦：`ca01b7bfdcf345c083873185daa4aad6`，4/7通过；暴露parser未保留armor frame第二整数及测试checksum异或抵消。
- 修正后focused：`7a9b60b9066042de9398d89d054d023f`，7/7通过。
- B5：`ae81f98ddb4545dab2353a0493ef8deb`，279/279通过。
- NTSD28 broad：`719a11c099ea49b1be86120731d4b4f4`，755/755通过。
- SelfCheck：2026-09-06 00:19:49Z `PASS`；Console仅7条既有rest-binding故意失败日志，无C#编译错误。
- Scene保持SHA-256 `D4266C6D0A802975B50E481E1D01662DC79AEF168C6E2DE14AE382396FDA583B`、
  205625 bytes、mtime UTC `2026-09-05T15:44:52.7794120Z`；未保存、回退或覆盖。
- 本包没有部署正式armor content，也没有接selection、activation或runtime armor HP/recovery。
