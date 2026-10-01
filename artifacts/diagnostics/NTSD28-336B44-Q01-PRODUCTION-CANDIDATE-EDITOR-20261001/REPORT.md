# Q01 正式目录的 Unity Editor 解析复核（2026-10-01）

状态：`FORMAL_ROOT_EDITOR_CANDIDATE_PASS / STAGED_330_DEFINITION_PARSE_PASS / STAGED_906_IMAGE_PUBLICATION_PASS / SCOPED_BATTLE_SCENE_PLAY_REUSED / GUID_AUDITED / RAW_IDENTITY_MISMATCH_KNOWN_NEWLINES / FULL_VISUAL_AND_NONOBJECT_BOUNDARY_PENDING`。战斗规则权威仍为根目录 SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 的正式 EXE 及其 playable live source；这次检查只涉及内容解析，不裁决战斗规则。

在原项目已打开且非 Play 的 Unity 2022.3.62f3 Editor 中，通过现有 UnityMCP TCP bridge 定向运行 EditMode tests，没有启动第二个 Editor 或新项目。项目 `GameConfig.asset` 的 `BattleContentRuntimeRoot` 为 `Assets/NTSD/Content/LoganRuntime`。被执行的两个现有测试均从正式 `J:\QQFile\NTSD2.8.3.3 zip\NTSD2.8.3.3\NTSD 2.8-Logan\resources\runtime` 读取，不是直接从该生产暂存根读取。

首次请求 2 项时，`NTSD28B11VisualContentCandidateEditorTests` 的 `[SetUp]` 因缺少 `Temp/NTSD28PngAlpha/fixture.dat` 失败。这是测试间的临时夹具依赖：该文件由 `NTSD28B11PngSheetAlphaEditorTests` 的 `[OneTimeSetUp]` 创建。随后单独运行 `DecoderFormat_IsByteBased_OnWorkerAndMainThread`，1/1 PASS；再运行原两项，2/2 PASS，0 failure：

- `FormalCandidate_BuildsAll330DefinitionsWithNativeFrames`：330 个对象定义均构建成功，并保持原生帧编号。
- `FormalCandidate_Captures330DefinitionsAnd906ReferencedImages`：正式目录候选捕获 330 个对象定义与该测试选取的 906 张关联图片，测试还检查 SPARK 输入及候选输入未变。

重跑作业 ID `b42be490f5084c8f967389ee92acb767`，紧凑结果见 [focused-test-result.json](focused-test-result.json)。测试前后 Battle/Menu Scene、GameConfig、ProjectBattleModeConfig 四文件 SHA-256 均相同，见 `protected-before.json`、`protected-after.json`。结束时 Editor 非 Play、idle、未编译/更新资源，活动 Battle Scene `isDirty=false`。活动 Scene rootCount 从测试前 16 到测试后 13；没有测试前根对象清单，不能解释这个内存层差异，也不把磁盘 SHA 相同扩大为内存层完全不变。

随后单独运行现有 `CharacterAssetDeploymentEditorTests.FormalTypeZeroCharacterDatAndImagesAreDeployed`（作业 `9ef3c546147844db86b9fef580daea88`），结果 0/1：在 OID 77 的 `DatSha256` 原始字节相等断言失败。正式 `c/kar/kar.dat` SHA 为 `987696ab...ac66ff`，暂存 SHA 为 `a4500e6b...21f62`；既有逐文件清单已确认该对象的 `newline_normalized_equal=True`，大小只差 1 字节，8 条不同图片路径计数一致。这是已知的原始换行字节差异，不能把这个测试结果写成角色资产完整通过，也不能将其写成战斗解析失败。测试在断言前已经分别执行正式/暂存两端的 `LoganObjectCatalog.Read` 和 `BuildCharacterFrameConfigsFromCatalog`，后者会遍历全部 catalog 条目并在任何解析失败时整体抛错，因此**两端 330 个索引对象定义的解析均已执行成功**；158 名 type-0 角色的后续逐图片/逐帧断言在 OID 77 处被截断。紧凑失败见 [staged-type0-result.json](staged-type0-result.json)。第二轮后四保护文件磁盘 SHA 仍相同，见 `protected-after-type0.json`。

本项当时证明当前 Editor 的现有 Unity 解析链能构建正式目录与生产暂存根的 330 个索引对象定义，以及正式目录候选能捕获选定的 906 张关联图片。下面的本轮补证覆盖暂存发布，但不覆盖真实 Battle Play 和非对象 DAT 图片消费者。

## 暂存候选与发布补证

在同一原 Editor 中继续运行两个现有定向测试：

1. `NTSD28Q07StagedCandidateIdentityEditorTests.StagedCandidateMatchesFormalRuntime`，作业 `a32b0629f05c4597bbe4e02dde38ba79`，0/1 FAIL。正式、暂存两个 330 对象/906 选定图片的候选均已捕获；测试在第一条 `Catalog.DefinitionFingerprint` 相等断言处停止。`LoganObjectCatalog.Entry` 对每份 DAT 原始字节计算 SHA，`DefinitionFingerprint` 明确包含这些 SHA；20 份对象 DAT 的已知换行字节差异足以造成该指纹不同。后续 fusion、内容身份和 visual 指纹断言未执行，不能宣布一致。`LoganContentIdentity.SemanticFingerprint` 是从原始组件指纹与解码合同计算，名称不表示对换行归一后的语义等价。结果见 [staged-candidate-identity-result.json](staged-candidate-identity-result.json)。
2. `NTSD28Q07StagedPublicationEditorTests.StagedFormalCandidatePublishesAllOwnersAndRecyclesResources`，作业 `e609510cfff64d98a0a036b4d3298361`，1/1 PASS，约 131 秒。它直接从生产暂存根捕获 330 对象和 906 选定图片，并通过隔离的生产发布路径检查 330 个对象配置与数据、适用 head/small 图片绑定、正式 SPARK PNG 的选定切片、发布 key，以及所跟踪 sprite/资源在 teardown 后全部回收。这是 **EditMode 隔离发布证据**，不代表真实 Battle Scene 的自然出生、可见画面、所有 1013 对象 VFS 图片均被发布，或非对象 `resource.dat`/`system.dat` 图片消费。结果见 [staged-publication-result.json](staged-publication-result.json)。

发布前后 Battle/Menu Scene、GameConfig、ProjectBattleModeConfig 四保护文件 SHA-256 一致，见 `protected-before-publication.json`、`protected-after-publication.json`；结束时原 Editor 非 Play、idle、Battle Scene `isDirty=false`。活动 Scene rootCount 仍为 13，本轮发布前后的精确内存根对象身份未单独封存。没有改脚本、DAT、图片或 Scene。Q01 的原始字节身份与正式版不同这一事实必须保留；在用户禁止改 DAT 数据的约束下，不通过偷偷改换行或放宽正式版身份声明来伪造相等。后续只按当前需要检查真实场景 Play、非对象资源消费者及其用户例外，Q01、Q07、Q09 与总目标仍开放。旧测试名中的 `B11` 只作历史命名，不定义 336B44 战斗规则。

## 既有真实 Battle Scene 与 GUID 入口复核

本项复用当前 336B44 的既有原 Scene 证据，不重跑相同案例：[C052 疾风物理键 Scene 报告](../NTSD28-336B44-Q07-C052-HAYATE-PHYSICAL-SCENE-001/REPORT.md) 的 v3 原件记录生产 `contentRoot=Assets/NTSD/Content/LoganRuntime`、36 tick、退出 Play、Scene clean；其独立比较为选定 684/684 字段一致。另 [C051 护甲 Scene 报告](../NTSD28-336B44-Q07-C051-ARMOR-SCENE-001/ACCEPTANCE.md) 的 OID78/97 与 OID78/2 两例各12 tick也从同一生产根读取并通过，均正常退出、四保护 SHA 稳。这些已证明**选定对象内容进入原 Battle Scene 生产 Driver 的局部 Play 链**；它们没有拍摄/比较角色 PNG 像素，不能把对象加载等同 Game View 视觉验收。

对生产暂存根的只读 [GUID 审计](staged-guid-audit.json)得：1371 个数据文件（338 DAT、1031 PNG、1 CSV、1 TXT）的相邻 `.meta` 均存在；含文件夹及根目录的 1700 个 `.meta` 各有一个合法 GUID，暂存根内无重复，扫描 `Assets` 下 7023 个 `.meta` 也没有该组 GUID 与其它资产冲突。扫描 `Assets/NTSD` 下 808 个 Unity 序列化 Scene/Prefab/Asset 等文件，仅发现一个暂存内容 GUID 直接引用：`NTSD_Battle.unity` 第833行的 `BattleCentralEditorPreview` 引用 `vfs/c/sasu/sasu.png` sheet。该 GameObject 序列化为 inactive，属于编辑器预览，不是当前生产角色加载真相；但是将来如获授权清理该 PNG，必须先处理这条引用，不能只看 DAT 引用图。

当前生产 `GameConfig.asset` 通过 `BattleContentRuntimeRoot` 字符串选 `LoganRuntime`，`BattleContentSource` 将 DAT/图片解析为受根目录约束的磁盘路径；`LoganVisualContentCandidate` 读取文件字节/路径，隔离发布测试已证实际加载。因而逐对象/逐图片 GUID 不是这条生产 DAT/PNG 读取链的解析前置条件，但它仍是 Unity 序列化引用与未来删除清单的保护条件。以上检查只覆盖所列 808 个 `Assets/NTSD` 序列化文件，不宣称项目所有可能的脚本构造引用均已穷尽。

据此，Q01 的生产根目录、330 定义、906 选定图片隔离发布、选定 Battle Scene Play 以及已检查 GUID 关系已有分层证据；**正式/暂存原始 DAT 指纹仍不同**，1013 对象 VFS 图片的全部自然 Game View 消费、非对象图片按用户例外的最终裁决和更广战斗场景仍归 Q07/Q09/Q12，不能把 Q01 的局部证据合并为全目标完成。
