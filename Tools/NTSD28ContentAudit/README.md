# NTSD 2.8-Logan 内容接入审计工具

所属 Task/Change：`NTSD28-B11-CONTENT-ENTRY-INVENTORY-001`（Q01 / BATCH-01）。
所有输入只读；本工具不导入、替换或删除资源，不启动 Unity，不提供战斗运行时对齐证书。

## 数据来源

- `AuthorityContentCapture.cpp` 链接当前权威源中的 DatParser、CombatRecordDecoder、ObjectSpawnPlanner 和 CollisionGeometry；构建源清单与 SHA-256 进入 manifest。这是源码诊断模型，不是正式 EXE 的运行输出。
- `UnityContentCapture.csproj` 链接当前生产 ParserV2 / Decryptor / Converter / 数据契约源文件。Unity stubs 仅提供外围类型和日志收集，不替代解析或转换规则；不加载 Library 中旧 DLL。
- `Audit-Content.py` 对同一份新版 DAT 的两端转换结果对账，并按 registry 的 section / ID 建立旧新版对应关系。跨版本内容的原始 AST 差异单列，不能当作规则缺陷。

## 执行

在仓库根目录运行；需要 Python、.NET 10 SDK、PowerShell 7 和构建脚本指定的 MinGW-w64。具体已使用版本与命令结果见本 Change Record。

```powershell
$auditRepo = $PWD.Path
$auditAuthority = 'J:\QQFile\NTSD2.8.3.3 zip\NTSD2.8.3.3\NTSD 2.8-Logan'
$auditOutput = Join-Path $auditRepo 'artifacts/diagnostics/NTSD28-B11-CONTENT-ENTRY-INVENTORY-001'
pwsh -File Tools/NTSD28ContentAudit/Build-AuthorityContentCapture.ps1 -Mode All
dotnet build Tools/NTSD28ContentAudit/UnityContentCapture.csproj
dotnet run --project Tools/NTSD28ContentAudit/UnityContentCapture.csproj --no-build -- --self-test
dotnet run --project Tools/NTSD28ContentAudit/UnityContentCapture.csproj --no-build -- --input-root "$auditAuthority/resources/runtime/decoded_dat" --input-mode plaintext --output "$auditOutput/unity/authority-through-unity.jsonl"
dotnet run --project Tools/NTSD28ContentAudit/UnityContentCapture.csproj --no-build -- --input-root "$auditRepo/Assets/NTSD/Config" --input-mode unity --output "$auditOutput/unity/current-unity.jsonl"
python Tools/NTSD28ContentAudit/Test-ContentAudit.py
python Tools/NTSD28ContentAudit/Audit-Content.py --repository "$auditRepo" --authority "$auditAuthority" --native-capture "$auditOutput/native/authority-content.jsonl" --unity-native-capture "$auditOutput/unity/authority-through-unity.jsonl" --unity-current-capture "$auditOutput/unity/current-unity.jsonl" --output "$auditOutput/report"
```

重复导出到单独的输出路径并比较 SHA-256；不要用自检 PASS 代替实际全量输出与输入身份检查。大型 JSONL 是可重建的诊断工件，不是正式游戏资源。

## 解释边界

- `normalized` 是实际转换 DTO 的字段投影，包括该 DTO 的真实默认值；原始 `fields` 保留显式字段、顺序、重复和未知字段。缺失字段不能填零伪造一致。
- 例外是native BDY投影：它保留DatDocument的typed scalar字段并附CollisionGeometry诊断，未序列化最终box；下游只标PARSED_BODY_FIELD_REVIEW。ITR标量/数组亦先标表示审查，再追实际consumer。详细裁决见Q01报告。
- 每个失败仍输出记录；整帧转换失败时再单独转换 subblock，用于定位。单块成功不代表原整帧已能载入。
- 顶层/frame 字段及部分专门 block 仍是原始 AST，不能据此声明整个 CharacterData 或 production loader 语义一致。
- `data/resource.dat`、INKHUD 等可由不同的正式专用 parser 消费。generic DatParser 的拒绝要按领域解释，不能直接指认正式 release 错误。
- registry 和声明的 OPoint/图片引用只是保守静态清单，不能证明所有运行时可达对象、动态资源路径或像素效果。图片检查只读头部尺寸/格式与文件哈希。
- GUID 反查不能证明动态引用不存在。所有旧资源均保持 `deleteAuthorized=false`；非角色资源、UI、背景、音频、Foot Marker 和用户例外须按已有合同另行处理。
- menu_face单列非战斗图片引用；Unity只从实际AST读layer，不保留时明确不可用。capture路径覆盖还必须通过source/header/input hash与mode身份gate，路径相同不能证明旧capture仍有效。
- 工具输出 hash 证明本次数据一致性，不代替 Unity 编译、SelfCheck、真实 Play 和正式 EXE 同场景对照。
