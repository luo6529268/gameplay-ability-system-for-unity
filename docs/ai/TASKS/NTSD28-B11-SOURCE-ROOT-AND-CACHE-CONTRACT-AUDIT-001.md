# Q02-A：资源根、缓存与共用 caller 合同审计

状态：`DELIVERED / READ_ONLY_CONTRACT_AUDIT`。总目标 `NTSD28-UNITY-BATTLE-REALIGNMENT-001`；批次 `BATCH-02`；Q owner `Q02`；回访 `R17`。实际出口：artifacts/diagnostics/NTSD28-B11-CONTENT-SOURCE-PATH-CONTRACT-001/Q02-A-AUDIT.md；后继纯路径Change同目录记录，PNG Task已准备。

前置证据：Q01 同ID内容审计的 `Q01-REPORT.md`、`scope-and-consumers.md`、`report/object-mapping.csv`、`capture-identity-verification.json`、`same-input-sprite-layout-differences.csv`。只继承已关闭子集，不重做 B1/B2。

本子包先只读追踪以下现有代码，形成最小生产包 Task/Change 后再修改脚本。读取当前 authority/Git/适用规则，保护当前文件 hash；不修改或导入 Assets，不启动场景，不调用会创建 singleton 的测试，不改旧数据根、不 hot reload。

## 精确读取接缝

- `Assets/NTSD/Scripts/Animation/GameDataManager.cs`：InitializeSingleton、LoadDataFile、ParseObjects、ResolveObjectFilePath、所有 singleton/cached registry caller。
- `Assets/NTSD/Scripts/Animation/Manager/CharacterAnimtorManager.cs`：ParseCharacterFrameConfigs、BuildCharacterDataFromDat、ResolveSpritePath、GetDatFileDirectory、LoadAllCharacterUISpritesAsync、LoadCharacterSpritesAsync、BuildBattleSpriteCatalog；完整 source/cache/publication 关系。
- Q01已确认的 `LoadingPrewarmController`、`AppManager.SetupBattleCharacters` caller，按实际声明路径追踪，不按目录猜所有权。
- `Assets/NTSD/Scripts/Animation/Runtime/BMPLoader.cs`：只确认现有API和后台调用线程；PNG实现属于后继Q02-B。
- ParserV2 的 file declaration 到实际 sprite catalog pic index 消费；31文件/55字段差异只是AST证据，须确认是否已在后续重建纠正。
- 菜单 `SelectRoleItem` / `CharacterUIResourceManager`、Editor preview、raw capture、内容patcher与测试场景：只读列出资源数据/缓存共享边界及原行为保持策略。
- 正式 `GameSession28`、runtime registry/VFS resolver实际路径语义与构建参与性；原权威目录始终只读。

## 必须冻结的决定

1. index path、decoded DAT root、VFS root与source identity传递位置；D-023只决定内容来源，不授权全局改变共享cache默认行为。
2. singleton首次初始化/加载顺序；cache何时允许建立、资源何时发布、失败时保留何种状态；不在Running/Stopping期间临时切source，不新增shutdown管理器。
3. 旧默认caller、Menu/Editor/Test保持策略；source切换只作用于声明的战斗入口。如无法不改变非战斗行为，该局部按总合同暂停并报告，其他独立任务继续。
4. Q07前保持旧正式内容不变；正式新目录仅作为隔离测试来源。资源部署物理路径和GUID操作须在迁移前单独冻结，不凭此审计复制或删除资源。
5. root/key/sheet/head/small/错误路径/缓存先载旧源/BMP兼容及真实生产caller测试清单。必要适配复用原加载和publication框架。

## 出口

产出精确code-path/symbol清单、source/cache状态顺序、非战斗保护证明与验证方案，再建立一个最小实现Change Record。Q02-A审计完成不等于Q02交付；PNG、有效pic范围与真实加载focused证据仍须完成。触发R17时登记实际变动；未发生资源/schema变化不伪造fingerprint或版本推进。

审计已实际执行：确认catalog.csv/registry_index才是对象目录权威；菜单预热、additive战斗、固定cachekey、UI头像字典共用且存在混源风险。31/55 sprite AST差异追至consumer后，25文件条件性范围仍不同，6文件只是容量偶然裁剪一致；PNG未发布，不冒充实际catalog观察。纯BattleContentSource已另建Task/Change实现，最终24项源链接/真实Unity focused通过；它不代表此审计涉及的cache与publication已修改。
