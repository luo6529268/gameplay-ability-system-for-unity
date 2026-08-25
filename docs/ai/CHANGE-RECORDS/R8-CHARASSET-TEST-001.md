# R8-CHARASSET-TEST-001 — type:0 character DAT/BMP deployment contract

<!-- CHANGE-RECORD
id: R8-CHARASSET-TEST-001
status: VERIFIED
code-path: Assets/NTSD/Scripts/Test/Editor/CharacterAssetDeploymentEditorTests.cs
authority: USER-APPROVED-R8-CHARASSET-001 / UNITY-RUNTIME-DAT-LOAD-CONTRACT
evidence: UNITY-COMPILE0 / EDITMODE-1-OF-1-PASS / NORMAL-PLAY-CONSOLE0
-->

> 创建日期：2026-08-24  
> 最后更新：2026-08-24  
> 类型：test / resource deployment verification

## 1. 目标与范围

- 目标：为已获用户批准的 `type:0` 角色 DAT/BMP 恢复建立可重复的 EditMode 资源契约测试。
- 范围：读取 `Assets/NTSD/Config/data.txt` 的全部 `type:0` 条目；用运行时相同密码调用 `Lf2DatDecryptor`；用 `Lf2DatParserV2` 解析；确认每个 `<bmp_begin>` 中 `head`、`small` 与 `file(...)` 所引用的 BMP 都存在。
- 不在范围：不改 DAT 的 gameplay 字段、不改对象池/渲染/输入/pass 顺序、不改 C++ 工程、不执行 R08 的合体对象行为认证。

## 2. 依据与原状

- 用户于 2026-08-24 明确批准从 `J:\QQFile\NTSD 2.4.1` 恢复缺失 `type:0` DAT/BMP，并要求随后测试。
- Unity 实际加载链使用 `Lf2DatDecryptor.DecryptFile(..., "odBearBecauseHeIsVeryGoodSiuHungIsAGo")` 和 `Lf2DatParserV2`；因此测试必须复用这两个生产读取组件，而不能只检验目录名。
- 已完成的部署静态校验：42 个 type0 映射、0 缺失 DAT、0 解析/BMP 块异常、227 条 BMP 引用均存在。

## 3. 计划改动

| 文件 | 符号 | 改前 | 改后 | 风险 |
|---|---|---|---|---|
| `Assets/NTSD/Scripts/Test/Editor/CharacterAssetDeploymentEditorTests.cs` | 新 `CharacterAssetDeploymentEditorTests` | 无部署资源的全量可重复契约 | 读取、解密、解析并验证 42 个 type0 角色及其 BMP 路径 | 仅 EditMode 测试；不写资源、不创建运行时实体 |

## 4. 实际改动

- 新增 `CharacterAssetDeploymentEditorTests` 及稳定 `.meta`，使用运行时 `Lf2DatDecryptor` 与 `Lf2DatParserV2`，并且不使用测试专用解密或路径逻辑。
- 测试覆盖当前 `data.txt` 的 42 个 `type:0` 条目、每份 DAT 的 frame parse 以及所有声明 BMP 的 `File.Exists`；使用独立 `CharacterAssetDeployment` NUnit category。由于当前 MCP 对定向 Test Runner 筛选返回 0 tests，额外提供调用同一测试方法的 `Tools/NTSD/Tests/Verify Type0 Character Asset Deployment` Editor 菜单，不复制验证逻辑。
- 代码没有写入或修改 DAT、BMP、`data.txt`、场景或任意 gameplay/runtime 脚本。

## 5. 验收、回滚与未验证项

- 实际验收：Unity全资源导入后Console compiler error=0；定向EditMode job
  `34a8a483ff314b82b65e9df5f4aaaf0e`为`1/1 passed`，实际执行目标方法
  `NTSD.Test.CharacterAssetDeploymentEditorTests.TypeZeroCharacterCatalogDecryptsParsesAndResolvesDeclaredBitmaps`；
  随后`NTSD_Battle` Play 20秒，Console新增error/warning=0。
- MCP在新增`.meta`之前曾把定向筛选误报为0 tests；该次不计入验收。补齐稳定`.meta`并全资源导入后，精确筛选已命中并通过。
- 未验证项：本测试不宣称 C++ release gameplay 已对齐；也不单独证明每个角色在战斗场景中的完整技能表现。
- 回滚：仅删除本 Change 新增的 Editor 测试及 `.meta`，并将本 Record 标记 `ROLLED_BACK`；资源恢复本身由 `R8-CHARASSET-001` 记录和其 Temp 备份独立管理。
