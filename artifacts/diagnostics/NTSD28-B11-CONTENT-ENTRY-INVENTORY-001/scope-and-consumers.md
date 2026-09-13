# NTSD28-B11-CONTENT-ENTRY-INVENTORY-001：资源入口、引用与非战斗边界审计

状态：`IN_PROGRESS / READ_ONLY_CONSUMER_AUDIT`

本文件只记录 Q01 对 Unity 现有资源入口、实际 caller、静态引用和 Q02 加载根/PNG 接入风险的审计结果。本文不修改生产脚本、DAT、图片、Scene、Prefab、Importer 或 ProjectSettings；不把静态覆盖扩大为完整运行时引用闭合。

权威目标是用户已确认的 D-023：DAT 与角色相关图片采用当前 `NTSD 2.8-Logan` 正式 runtime 内容。正式输入身份仍是：

- EXE：`J:\QQFile\NTSD2.8.3.3 zip\NTSD2.8.3.3\NTSD 2.8-Logan\NTSD2.8-Logan.exe`
- 正式解码数据索引：`resources\runtime\decoded_dat\data\data.txt`
- 正式 DAT：`resources\runtime\decoded_dat\<object file path>`
- 正式图片根：`resources\runtime\vfs\`
- Unity 迁移前入口：`Assets/NTSD/Config/data.txt`、`Assets/NTSD/Config/Character/*.dat`、`Assets/NTSD/Sprite/Character/**/*.bmp`

## 1. 结论

Q02 不能只把 `data.txt` 或角色图片复制到新目录。至少需要沿现有加载框架闭合三个入口：

1. **索引 root 与 singleton reload**：`GameDataManager` 在 singleton 初始化时无参读取旧 `Assets/NTSD/Config/data.txt`，并在 `LoadDataFile()` 发现 `objectLookup.Count > 0` 后拒绝再次载入。仅让 `CharacterAnimtorManager.ParseCharacterFrameConfigs()` 传入 Logan `data.txt` 不足以切换数据源。
2. **DAT root 与 VFS root 分离**：Logan 的 `data.txt` 对象路径相对 `decoded_dat`，DAT 内 `file(...)`、`head`、`small` 图片 key 相对 `vfs` 根。当前 Unity 把 file sheet 以 DAT 所在目录拼接，把 head/small 以固定 `AnimationConfig` 目录拼接，都会产生错误路径。
3. **后台 PNG 解码**：`CharacterAnimtorManager` 的生产 sheet、head、small 都在线程池调用 `BMPLoader.LoadBmpData()`。`BMPLoader` 在非主线程明确跳过 Unity `Texture2D.LoadImage()`，只走手动 BMP 解析；Logan 的 PNG 因此会在后台路径返回失败。主线程 `LoadBMP()` 能尝试 `LoadImage()` 不能修复生产线程池路径。

最小实施方向是扩展现有 `GameDataManager`、`CharacterAnimtorManager`、`BMPLoader`、`Lf2DatParserV2`/`Lf2DatConverter` 和 `BattleSpriteCatalog` 接缝，保持现有 prewarm、事务式 Sprite catalog、atlas publication 和 `CharacterUIResourceManager`，不新造第二套角色加载系统。

## 2. Logan 与 Unity 当前路径证据

| 内容 | Logan 正式实例 | Unity 当前实例 | 结论 |
|---|---|---|---|
| 对象索引 | `resources/runtime/decoded_dat/data/data.txt`，例如 `id: 2 type: 0 file: c\\nar\\nar.dat` | `Assets/NTSD/Config/data.txt`，例如 `id: 2 type: 0 file: Assets/NTSD/Config/Character/naruto.dat` | 路径模型和对象集合均变化，不能 basename 猜配。 |
| Naruto DAT | `decoded_dat/c/nar/nar.dat` | `Assets/NTSD/Config/Character/naruto.dat` | Logan 文件名、目录和内容均不是当前 Unity 文件的同一路径。 |
| Naruto frame sheet | DAT 中 `file(0-199): c\\nar\\nar.png w:79 h:79 row:10 col:20`，实际文件 `vfs/c/nar/nar.png` | DAT 中使用 `Assets/NTSD/Sprite/Character/MingRen/naruto_*.bmp` | Logan `file` key 是 VFS-root key；当前 resolver 若以 DAT 目录为 base 会重复拼接 `c/nar`。 |
| Naruto head | DAT 中 `head: sprite\\face\\naruto_f.png`，实际 `vfs/sprite/face/naruto_f.png` | 当前 DAT 通常写 `Assets/NTSD/Sprite/Character/MingRen/naruto_f.bmp` | Logan head 是 VFS-root key，且扩展名从 BMP 变为 PNG。 |
| Naruto small | DAT 中 `small: sprite\\small\\naruto_s.png`，实际 `vfs/sprite/small/naruto_s.png` | 当前 DAT 通常写 `Assets/NTSD/Sprite/Character/MingRen/naruto_s.bmp` | 与 head 相同；另有 `smallb` 字段，当前 converter/character data 不持有该字段。 |
| Logan parser 输入 | 正式 DAT 已是 decoded plain text；正式 `data.txt` 为多行 object entry | Unity 入口默认读取现有 Config；当前角色 DAT 多数为加密文件 | `Lf2DatDecryptor` 能处理明文或加密，但入口 root 和后续图片消费仍阻断。 |

Logan `nar.dat` 的 `<bmp_begin>`、`file(...)`、`head`、`small` 语法与当前 `Lf2DatParserV2` 的 token 形状基本兼容：`Lf2DatParserV2.Parse()` 约 `58-64` 建立 source path；`149-202` 解析 `file(x-y)`；`286-314` 把 `name/head/small` 写入 `Lf2BmpSection`。这只能说明语法入口已有候选兼容性，不等于字段、资源路径、PNG 解码或最终 runtime 可用。

## 3. 实际加载入口与脚本缺口

### 3.1 `GameDataManager`

文件：`Assets/NTSD/Scripts/Animation/GameDataManager.cs`

| 符号/行 | 当前行为 | Q02 影响 |
|---|---|---|
| `InitializeSingleton()` `31-35` | singleton 建立后立即调用无参 `LoadDataFile()`。 | 在角色 prewarm 之前，旧 Config 可能已经成为唯一缓存。 |
| `LoadDataFile(string filePath = ...)` `40-49` | 默认路径是 `Assets/NTSD/Config/data.txt`；`objectLookup?.Count > 0` 时直接 return。 | 即使 caller 传 Logan `data.txt`，已加载旧索引时也不会切换。需要显式、受控的 reload/source selection seam，不能在 caller 外部反射清字段。 |
| `LoadDataFile()` `53-78` | 读取 UTF-8 文本，`ParseDataFile()`，按 id/type 建 lookup。 | Logan `data.txt` 的 object 行可由当前 regex 识别，但正式对象路径必须交给新的 root resolver。 |
| `ResolveObjectFilePath()` `165-171` | `Assets` 开头原样返回，否则以 `dataFileDirectory` 拼接。 | 对 Logan `c/nar/nar.dat` 若 `dataFileDirectory` 为 `decoded_dat/data`，会错误得到 `decoded_dat/data/c/nar/nar.dat`；正式 source root 应是 `decoded_dat`。 |
| `ParseObjects()` `198-216` | 单行 regex `id/type/file`，无 object path authority、source root 或 duplicate policy。 | 语法上能读 Logan 当前 object 行；完整 duplicate/hidden/classification 以 B11 exporter/catalog 为准，不能把这个 parser 的 count 当正式 catalog 证书。 |

另有 `CharacterFramePreviewWindow.LoadDataFile()` `1034-1045` 直接固定 `Assets/NTSD/Config/data.txt` 并调用同一个 manager。该入口是 Editor-only，不能随 Q02 战斗 root 修改自动改变；需要单独提供 editor preview source 选择或明确继续使用迁移前基线。

### 3.2 `CharacterAnimtorManager`

文件：`Assets/NTSD/Scripts/Animation/Manager/CharacterAnimtorManager.cs`

| 符号/行 | 当前行为 | Q02 影响 |
|---|---|---|
| `TotalCharacterFrameConfigPath` `31` | 固定 `Assets/NTSD/Config/AnimationConfig`。 | 不是 Logan data/DAT/VFS root；`ParseCharacterFrameConfigs()` 只能从当前工程相对目录推导旧 `data.txt`。 |
| `ParseCharacterFrameConfigs()` `444-539` | `446-447` 由 `AnimationConfig/../data.txt` 得到旧 `Assets/NTSD/Config/data.txt`；`458` 调 manager `LoadDataFile()`；`486-488` 从 object file 得 DAT，再改扩展为 `.dat`；`498` 解密；`506-516` parser/converter。 | 需要把 index path、decoded DAT root 和 VFS root 作为同一 source contract 传入，而不是继续从 `AnimationConfig` 推导。`dataManager` 旧缓存锁会使 `458` 失效。 |
| `BuildCharacterDataFromDat()` `558-645` | `588-616` 将 `fileDef.Path` 以 DAT directory 拼成 absolute path；`580-581` 保存 head/small 原始字符串。 | Logan frame path 是 VFS-root key，不能相对 DAT directory；head/small 后续还会走另一个错误 base。建议保留 logical key/normalized path，统一交给 source-root resolver。 |
| `ResolveSpritePath()` `851-864` | `Assets/` 原样转 project root；其他路径相对传入 DAT directory。 | 只能处理当前 Unity DAT 写的绝对 Assets 路径；不能处理 Logan `c/nar/nar.png`、`sprite/face/*.png` 的 VFS-root 语义。 |
| `LoadCharacterSpritesAsync()` `870-1067` | 角色配置成功后收集 sheet，后台并发调用 `ProcessAndCreateSpritesAsync()`，成功后构建/publish `BattleSpriteCatalog` 与 atlas。 | 这是最小战斗接入的正确 publication seam；只需让它收到可读 PNG pixels 和稳定 logical source path，不要另建 catalog。失败时现有代码会保留旧 catalog，符合 fail-closed 方向。 |
| `BuildSparkPublicationAsync()` `1074-1144` | 固定 `Application.dataPath/NTSD/Sprite/UIPanels/SPARK.bmp`。 | SPARK 是共用表现资源，不属于 D-023 角色 sheet 自动替换范围；PNG loader 改动必须保留 BMP fallback，避免误伤 spark。 |
| `LoadAllCharacterUISpritesAsync()` `1294-1345` | 从 DAT head/small 取 key，`1321-1331` 调 `ResolveSpritePath()`，再经 `LoadBMPAsSpriteAsync()` 写 `CharacterUIResourceManager`。 | Logan head/small 会同时受 VFS root 和 PNG 后台 decode 阻断；这条路径还服务菜单选择，不能只修战斗 sheet。 |
| `GetDatFileDirectory()` `1351-1355` | 无论角色 DAT 实际位置，固定返回 `TotalCharacterFrameConfigPath`。 | 这是 head/small 解析的直接错误点；应由当前已解析的 DAT source directory 或 VFS root contract 提供路径，不能继续固定 `AnimationConfig`。 |
| `LoadBMPAsSpriteAsync()` `1364-1406` | 文件名/注释/`BMPLoader.LoadBmpData()` 均按 BMP 设计；在线程池读取并创建 Sprite。 | PNG head/small 在当前生产路径不可用；方法名也会误导后续维护。实现时应复用现有 `BMPLoader` API 或将其能力泛化，保持 Sprite creation/ownership 逻辑。 |
| `ProcessAndCreateSpritesAsync()` `1408-1548` | `1425-1429` 在线程池 `LoadBmpData()`；`1455-1463` 处理透明/网格；`1486-1529` 生成 atlas source、Texture2D 和 Sprite。 | PNG 接入的唯一战斗 sheet seam。需要保留 row/col/rect/pivot/ownership 逻辑；不能把 source extension 当 frame contract。 |
| `BuildBattleSpriteCatalog()` `1550-1640` | catalog key 是 character/visual id + effective pic，entry 同时保存 `SourceSheetPath`、Texture、Rect、pivot。 | source path 应改为 normalized logical VFS key，避免绝对盘符造成重复 source identity；catalog contract 本身可复用。 |

### 3.3 `BMPLoader`

文件：`Assets/NTSD/Scripts/Animation/Runtime/BMPLoader.cs`

| 符号/行 | 当前行为 | Q02 影响 |
|---|---|---|
| `LoadBMP()` `23-55` | 读 bytes，先 `Texture2D.LoadImage()`，失败后手动 BMP。 | 主线程/直接调用对 PNG 有候选支持，但不是角色生产路径的证据。 |
| `LoadBmpData()` `57-75` | 线程 ID 非 1 时直接 `LoadBmpDataManual()`；主线程才尝试 `Texture2D.LoadImage()`。 | `CharacterAnimtorManager` 的 sheet/head/small 都在线程池调用，Logan PNG 会进入 `LoadBmpDataManual()`；`156-177` 只接受 `BM`/未压缩 BMP。此处是确认阻断。 |
| `LoadBMPManual()` `151-252`、`LoadBmpDataManual()` `255-326` | 支持 4/8/24/32-bit BMP，后者还支持 RLE8。 | 保留 BMP 以兼容 SPARK、非角色资源和迁移期间 fallback；扩展 PNG 时必须避免把非角色 BMP 路径改坏。 |

最小实现约束是继续复用 `BMPLoader` 作为唯一像素入口，增加可在线程池安全执行的 PNG byte decoder 或等价现有实现，并让 `LoadBmpData()` 按签名/扩展名选择 PNG/BMP。不要在 `CharacterAnimtorManager` 内另造 PNG 专用 loader。

### 3.4 解析器与 converter

| 文件/符号 | 已观察事实 | 边界 |
|---|---|---|
| `Lf2DatDecryptor.DecryptFile()` | 读取 bytes，能识别 UTF-8 明文 `<bmp_begin>`/`<frame>`/`<stage>`，否则按旧密钥从偏移 123 解密。 | Logan decoded DAT 可直接读取；这不证明 encrypted release input、所有正式 source field 或 path semantics 已闭合。 |
| `Lf2DatParserV2.Parse()` `58-64` | 保留 `SourcePath`/`FileName`，解析 block/frame。 | 可承载 Logan DAT 的 source path，但 caller 必须给正确 path。 |
| `Lf2DatParserV2` `149-202` | `file(x-y)` 只限制 range/数值属性，不限制 `.bmp` 扩展名。 | parser 语法不阻止 `.png`；真正阻断来自 resolver 与 pixel loader。 |
| `Lf2DatConverter` `ConvertToFrameData()`、`ApplyNativeInputDefinitionData()`、`ApplyNativeArmorDefinitionData()` | 当前 converter 已有 native input/armor 等迁移逻辑，但 CPoint alias/27-scalar 等另属 Q03/schema 范围。 | Q02 不应把资源根修复扩大为字段语义重写；只记录 formal projection 首差，字段 contract 仍按 Q03。 |

## 4. 真实 caller 与非战斗边界

### 4.1 允许作为最小战斗接入路径的 caller

1. `LoadingPrewarmController.PreWarmOnceAsync()` `97-146`：现有统一 prewarm 生命周期。
2. `LoadingPrewarmController.CreateCharacterConfigTask()` `206-227`：调用 `ParseCharacterFrameConfigs()`。
3. `LoadingPrewarmController.CreateCharacterSpriteTask()` `230-249`：调用 `LoadCharacterSpritesAsync()`。
4. `AppManager.SetupBattleCharacters()` `207-260`：从 `CharacterAnimtorManager.GetCharacterConfig(slot.characterId)` 取战斗配置，属于战斗实际消费入口。
5. `BattleCentralRenderSystem` `480-527` 及 `BattleSpriteCatalog`：读取已发布 immutable catalog；不应新增资源直读。
6. `NTSDSoundPlayer.PrepareBattleCuesAsync()` `100-142`：会接收 `CharacterAnimtorManager`，但角色 DAT/PNG 迁移不应自动改变非角色 sound root；音频另按 O/H 处理。

最小路径应保持：

```text
source selection
  -> GameDataManager index
  -> CharacterAnimtorManager DAT parse/converter
  -> logical VFS key resolution
  -> BMPLoader pixel decode (PNG/BMP)
  -> RuntimeSpriteProcessor / rect+pivot
  -> BattleSpriteCatalog + central atlas publication
  -> AppManager.SetupBattleCharacters / central renderer
```

### 4.2 不可自动修改的非战斗 consumer

| Consumer | 位置/符号 | 资源耦合 | 处理边界 |
|---|---|---|---|
| 菜单角色列表 | `Assets/NTSD/Scripts/UI/SelectRoleItem.cs:343-355` | 直接取 manager 已加载角色 ID；角色集合切换会改变可选列表。 | 不在 Q02 自动重写选择规则；只保证 head/name cache 的新内容可用，选择 UI 继续用户例外。新增 Logan selectable/conditional policy 需单独确认。 |
| 菜单角色头像 | `SelectRoleItem.UpdateCharacterDisplay()` `532-572` | `GetCharacterName()` + `CharacterUIResourceManager.GetHeadSprite()`。 | 不硬编码 Logan basename；等待 head PNG 经同一 manager 发布。缺失 head 时现有逻辑保持 null/Unknown 行为，具体 UX 不在本包。 |
| UI head/small cache | `CharacterUIResourceManager` `42-152` | DAT parse 后动态 `SetCharacterUISprites()`；当前 `GetSmallSprite()` 没有发现生产 caller。 | 保留 manager；清理旧图片前确认动态 cache 已停止使用并清理 runtime Sprite ownership。不要把没有静态 caller 当作可删除证据。 |
| Loading screen | `LoadingPrewarmController.FormatLoadingResourcePath()` `158-174` | 只按 `/Sprite/Character/` 截断显示路径。 | 新 vfs/外部 root 会只显示 basename；这是诊断/界面文本，不应驱动 root resolver。可在后续 UX 包单独改。 |
| Editor 角色/帧预览 | `CharacterFramePreviewWindow` `81`、`272-275`、`1018-1045`、`1249-1354`、`1489-1495` | 读 singleton manager、固定旧 data path、缓存动态 sprites；可加载 OPoint object 预览。 | 不自动把 Editor preview 切到 Logan root。若要支持 Logan，需要独立 source selector/preview contract；否则保留旧基线并在删除旧资源前停用/迁移该入口。 |
| Scene editor preview | `Assets/NTSD/Scene/NTSD_Battle.unity:3026-3077` 的 inactive `BattleCentralEditorPreview` | actor `sourceSheet` 静态引用 `Zuozhu/sasuke_0.bmp` GUID `6d174fff55a50784d9bbf85531fb7d86`；scene 还引用 `GameConfig.asset`。 | 这是已发现的真实静态图片引用。删除旧角色图片前必须单独移除/重绑定该 preview actor；不能靠动态 manager migration 解决。该 scene object inactive，但 Unity asset reference 仍会断。 |
| Foot marker | `GameConfig.cs:28-34`、`GameConfig.asset:22-30`；`BattleCentralEditorPreview` `407-472`、`1153-1191` | 当前用户已配置 `FootMarkerSprite` 和 6 帧 PNG，GUID 来自 `Assets/NTSD/Sprite/UIPanels/BattleHud/Foot/frame_01..06.png`；scene 通过 `GameConfig.asset` GUID `513170c32731baa449d4d0e9abf8c827` 引用。 | 明确不是角色资源。清理 `Assets/NTSD/Sprite/Character` 时不得删除、重绑或移动 Foot marker、GameConfig 或其 GUID。 |
| Editor asset deployment test | `Assets/NTSD/Scripts/Test/Editor/CharacterAssetDeploymentEditorTests.cs:18-74、105-121` | 断言 type-0 DAT 的路径必须以 `Assets/NTSD/Sprite/Character/` 开头且扩展名是 `.bmp`。 | Q02 切到 Logan `vfs`/PNG 后该测试必然成为旧合同，不能当 runtime 失败证据；必须单独修订测试/诊断合同后再运行。当前只读审计不改它。 |
| Formal content patcher | `Assets/NTSD/Scripts/Test/Editor/FormalContentClosureResourcePatcher.cs:562-564` | `Character(file)` helper 固定生成 `Assets/NTSD/Config/Character/<file>`。 | 这是 Editor/fixture 写入工具，不是生产战斗 loader；不能随 Q02 全局替换，否则会改变已冻结的 Unity content fixture。需要单独建立 Logan source/fixture 版本。 |
| Unity raw capture | `Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditor.cs:49-53、1347、1404-1406` | `ProductionDataIndex` 固定旧 `Assets/NTSD/Config/data.txt`，`ProductionConfigRoot` 固定旧 Config root；capture 还依赖这些路径。 | Q02 资源根切换后，旧 capture 只能继续作为迁移前 baseline；必须另建 Logan-content capture contract，不能默默改变现有 artifact 含义。 |
| Sprite grid/preview tests | `BattleSpriteGridSeparatorEditorTests.cs:9-12`、`BattleCentralEditorPreviewEditorTests.cs:599-602`、`BattleCentralEditorPreviewEditor.cs:9-12` | 直接固定 `MingRen/naruto_0.bmp` 或 `Zuozhu/sasuke_0.bmp`。 | 这些是 Editor fixture/preview consumer，不得随战斗接入自动改成 Logan；在资源删除前应迁移 fixture 或保留专用测试输入。 |
| Test scene profile | `Assets/NTSD/Scene/NTSD_Test.unity:856、904` | `profileKey: MingRen` 等测试场景配置仍指旧角色 profile。 | 不是生产战斗入口；删除旧角色后需要独立测试夹具迁移，不能通过 Q02 直接修改场景。 |

### 4.3 静态 GUID 与 dynamic path 覆盖结果

本次只读扫描 `Assets/NTSD/Sprite/Character/**/*.meta`，发现 245 个角色图片/目录 meta；在排除角色目录自身 meta 后，只有 1 个生产/Scene YAML 静态图片 GUID 引用：

```text
Assets/NTSD/Scene/NTSD_Battle.unity:3053
sourceSheet GUID 6d174fff55a50784d9bbf85531fb7d86
-> Assets/NTSD/Sprite/Character/Zuozhu/sasuke_0.bmp.meta
```

这个结果**不是完整引用闭合**。以下动态引用不会出现在 GUID 扫描中：

- `Assets/NTSD/Config/data.txt` 中的 object file path；
- `Assets/NTSD/Config/Character/*.dat` / `chars/*.dat` 中的 `file/head/small` 字符串；
- `CharacterAnimtorManager` 根据 `fileInfo.filePath`、`ResolveSpritePath()`、`GetDatFileDirectory()` 生成的绝对路径；
- Editor preview/test 中的字符串常量；
- 已发布 `BattleSpriteCatalog`、atlas source 和运行时 Texture/Sprite ownership；
- `Resources`、外部 runtime root 或其他未在 `Assets` YAML 中表达的动态路径。

因此“静态只发现 1 个 scene 引用”不能提升为“旧角色资源均可删除”。删除候选必须至少同时满足：新 DAT normalized projection 已存在、所有动态 file/head/small key 已被新 VFS catalog 覆盖、战斗 prewarm 成功发布新 catalog、菜单 head/name 结果可消费、Editor/test fixture 已迁移或明确保留旧输入、scene sourceSheet 引用已处理。

## 5. Q02 最小接入建议与禁止自动修改项

### 5.1 建议的最小脚本范围

| 优先级 | 现有脚本/符号 | 建议 |
|---|---|---|
| P0 | `GameDataManager.LoadDataFile()`、`InitializeSingleton()` | 增加现有 manager 内的显式 source selection/reload contract，避免旧 singleton cache 阻断 Logan index；保留旧默认用于非战斗/Editor 未迁移场景，或在 battle prewarm 前明确注入 Logan source。不要依靠 reflection 清私有字段。 |
| P0 | `CharacterAnimtorManager.ParseCharacterFrameConfigs()`、`BuildCharacterDataFromDat()`、`ResolveSpritePath()`、`GetDatFileDirectory()` | 将 `decoded_dat root`、`vfs root`、index path 分离；DAT object file 解析相对 decoded root；frame/head/small 解析相对 vfs root；保留原始 logical key 与 normalized key，避免绝对盘符进入 catalog identity。 |
| P0 | `BMPLoader.LoadBmpData()` 及其现有 helper | 保留 BMP 手动解析，补充线程池可用的 PNG byte decode；确保 PNG/BMP 均返回同一 `BmpData` pixel contract，主线程 Unity API 与后台路径结果一致。 |
| P1 | `CharacterAnimtorManager.LoadCharacterSpritesAsync()`、`ProcessAndCreateSpritesAsync()` | 只换 source pixels/path resolver；保留 row/col、inclusive range、grid separator、pivot、atlas publication、failure rollback。 |
| P1 | `LoadAllCharacterUISpritesAsync()`、`LoadBMPAsSpriteAsync()` | 复用统一 PNG/BMP pixel entry 和 VFS key resolver；保留 `CharacterUIResourceManager` cache 及 null/failure semantics。 |
| P1 | `BattleSpriteCatalog` / `BattleAtlasResources` | 不新造 catalog；只确认 `SourceSheetPath`、`BattleAtlasLayoutPlanner.NormalizePath()` 与 VFS logical path 一致，并让新 source set 通过已有 duplicate/conflict/fail-closed 检查。 |
| P2 | `LoadingPrewarmController`、`AppManager.SetupBattleCharacters` | 继续使用现有 task/battle entry；只在 source selection contract 已闭合后接线。不要把菜单/Editor/test 目录的旧 root 直接全局替换。 |

### 5.2 当前不能自动改的内容

- `SelectRoleItem` 的可选角色排序、随机角色、菜单表现和 mode/roster 规则。
- `CharacterFramePreviewWindow` 的 Editor-only data source 与 OPoint 预览行为。
- `NTSD_Battle.unity` 的 inactive `BattleCentralEditorPreview` sourceSheet，除非单独取得 Scene 修改范围；删除旧角色图片前必须处理该引用。
- `NTSD_Test.unity`、Editor tests 和 self-check 中用于回归的旧 BMP fixture；这些是测试/预览输入，不等于生产资源消费。
- `GameConfig.asset`、Foot marker PNG、Foot marker animation、common `SPARK.bmp`、weapon/effect/background/UI 资源。D-023 当前只确认 DAT/角色相关图片，不能把整个 `Assets/NTSD/Config` 或 `Assets/NTSD/Sprite` 目录整体切换/删除。
- `NTSDSoundPlayer` 的音频 root/catalog；角色 DAT 迁移不自动代表 sound.dat/WAV 迁移。

## 6. 验收前置与静态覆盖边界

Q02 在脚本修改前至少应形成以下可复核结果：

1. source identity：正式 `data.txt`、object DAT、VFS PNG 的路径和 hash 已登记；对象 ID 只能按 catalog/source path 映射，禁止 basename 猜配。
2. parser projection：至少对 one-character、multi-file、head/small、PNG frame sheet、type 3 object、重复/缺失 path 形成 normalized projection；CPoint/OPoint schema 另按 Q03，不在此处隐含解决。
3. path contract：明确 index root、decoded DAT root、VFS root、logical key normalization、case/separator 规则；`file(...)` 和 head/small 必须走同一 source catalog，不再使用 DAT directory 猜 VFS。
4. pixel contract：PNG 与 BMP 的 width/height/pixel order/transparency/grid separator/pivot 结果可比较；后台和主线程 decode 不得产生不同结果。
5. caller matrix：战斗 prewarm、`AppManager.SetupBattleCharacters`、central catalog、菜单 head/name、Editor preview、test fixture 分列状态；不能以战斗 catalog 成功覆盖菜单/Editor/test。
6. deletion gate：只有新 content catalog、动态引用、静态 GUID、Scene preview、Editor/test fixture 和 runtime ownership 均有证据，才能形成精确旧资源删除集合。没有引用闭合的旧 BMP 只能标 `REVIEW_REQUIRED`。

本文件未运行 Unity、Editor、资源导入、测试或战斗 Play；也未宣称 Q01/Q02 完成或所有引用已闭合。

## 7. 本轮只读依据

- Unity：`GameDataManager.cs`、`GameDataConfig.cs`、`CharacterAnimtorManager.cs`、`BMPLoader.cs`、`Lf2DatDecryptor.cs`、`Lf2DatParserV2.cs`、`Lf2DatConverter.cs`、`LoadingPrewarmController.cs`、`CharacterUIResourceManager.cs`、`SelectRoleItem.cs`、`AppManager.cs`、`BattleSpriteCatalog.cs`、`BattleAtlasResources.cs`、`BattleCentralRenderSystem.cs`、`BattleCentralEditorPreview.cs`、`GameConfig.cs`、`BattleFootMarkerBatchBackend.cs`。
- Unity static consumers：`Assets/NTSD/Scene/NTSD_Battle.unity`、`NTSD_Test.unity`、`CharacterAssetDeploymentEditorTests.cs`、`FormalContentClosureResourcePatcher.cs`、`NTSD28UnityRawCaptureEditor.cs`、`BattleSpriteGridSeparatorEditorTests.cs`、`BattleCentralEditorPreviewEditorTests.cs`、`BattleCentralEditorPreviewEditor.cs`。
- Logan：`resources/runtime/decoded_dat/data/data.txt`、`resources/runtime/decoded_dat/c/nar/nar.dat`、`resources/runtime/vfs/c/nar/nar.png`、`resources/runtime/vfs/sprite/face/naruto_f.png`、`resources/runtime/vfs/sprite/small/naruto_s.png`、`resources/runtime/catalog.csv`。
- Commands were read-only PowerShell `Get-Content`/`rg`/`Get-ChildItem`/`git status`/GUID reference scans. No Unity, test, build, import, delete or resource mutation was run.
