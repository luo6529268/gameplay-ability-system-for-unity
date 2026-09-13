<!-- CHANGE-RECORD
id: NTSD28-B11-PNG-SHEET-ALPHA-CONTRACT-001
status: VERIFIED
change-kind: SOURCE_SPECIFIC_PNG_BATTLE_SHEET_ALPHA
code-path: Assets/NTSD/Scripts/Animation/Runtime/BMPLoader.cs
code-path: Assets/NTSD/Scripts/Animation/Manager/CharacterAnimtorManager.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B11PngSheetAlphaEditorTests.cs
authority: User D-023 and active goal; formal B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033; playable d3d11_renderer load_texture/render sprite branch and render_snapshot SpriteFrameResolver28.
evidence: VERIFIED_SOURCE_PNG_SHEET_AND_GPU_SAMPLES_ONLY / RED_6_FAIL / UNITY_21_OF_21 / GPU_10_SAMPLES_D3D11_URP_GAMMA / CS_0 / SCENE_CLEAN / NO_RESOURCE_MIGRATION
-->

# PNG战斗sheet source alpha接入

状态IN_PROGRESS。仅三个声明脚本及测试.meta；文档/证据限本Task/Record、Ledger/STATE/handoff/authority/总表和artifacts/diagnostics/NTSD28-B11-PNG-SHEET-ALPHA-CONTRACT-001，测试夹具Temp/NTSD28PngAlpha。

原状：BMPLoader.BmpData无格式信息；ProcessAndCreateSpritesAsync统一去黑并强制其余alpha255，再按绿色占比整行/列清alpha。原生WIC32bppRGBA/UNORM保留straightRGBA，以DAT的w+1/h+1取矩形跳过分隔，不做颜色猜测。

实现：BmpData新增只读IsPng属性（internal set），仅据实际PNG signature在worker/manual和mainthread成功解码时赋值。新增明确source预热入口LoadCharacterSpritesFromSourceAsync，旧LoadCharacterSpritesAsync原签名委托null source保持原样。source按调用参数传入唯一ProcessAndCreateSpritesAsync（该方法只操作显式staging参数和静态helper，改internal static，不增owner）。新增PrepareBattleSheetPixels集中原Color→Color32及后处理；仅source.IsLoganRuntime且decoded.IsPng保留所有RGBA并跳过启发式grid清理；其余路径复用原ProcessSheetPixelsFast/ClearDetectedGridSeparatorAlpha。主纹理上传与catalog/atlas均消费同一个processedSheet。UI LoadBMPAsSpriteAsync、spark、Editor caller、RuntimeSpriteProcessor原方法、shader均不修改。

不改变Unity/GAS框架、snapshot12/20/23、shutdown阶段、缓存/全局publication所有权。static化不是Mono边界重构；新的source预热入口仍待完整catalog/cache事务调用，不自行把旧global配置切成native。测试直接调用同一个生产per-sheet staging方法，不构造manager/触发OnDestroy；本包不发布全局缓存。

验收：RED证明native像素策略/format缺失；GREEN覆盖0/128/238/241/255 alpha、黑色半透明、绿色可见边缘、legacy PNG/BMP旧去黑/绿色清理、mainthread/worker signature。实际生产async staging→atlas source pixels→SpriteCatalog→现有BattleCentralTransparent材质GPU读回，含正式nar.png与隔离fixture；报告当前Gamma/URP，仅证实测试设备和路径，不冒称全场/atlas-array/其他色彩空间全部一致。必要时把后继GPU差异精确记录，不改ProjectSettings。

无新增长期资源owner：测试新Texture/Sprite/Material/Mesh/RenderTexture/CommandBuffer均finally释放；生产仍由原prewarm staging及shutdown管理。回滚为声明脚本精确diff，不回退Q02-B/C或用户工作；删除按现有批准规则。

## 验证

写前Task/Record已登记；尚无RED/GREEN。详细权威指纹/原始测试/Scene/保护记录写入本包artifact。前置Q02-C Unity14/14不替代本包证据。全局source/cache/正式资源迁移留后继，整场Play/最终视觉不在本包关闭。

写后登记：BMPLoader.BmpData.IsPng、LoadPngDataManual、TryLoadWithUnityData；manager的LoadCharacterSpritesAsync/LoadCharacterSpritesFromSourceAsync、PrepareBattleSheetPixels、ProcessAndCreateSpritesAsync已按声明改写。Unity RED job94e86ba6fc2c48a1b391230a6aaa00c0共6项全部因缺少格式/source入口失败；red-result.json保留。GREEN与GPU/保护尚待执行。颜色转换移入原有worker处理闭包，不改变旧像素算法或主线程上传。

## 最终证据（覆盖前文进行中状态）

状态VERIFIED_SOURCE_PNG_SHEET_AND_GPU_SAMPLES_ONLY；job5226692062864130b652cd8be8089e29实际21/21（本包6、旧PNG13、Overlap2）PASS。隔离PNG与正式nar生产sheet staging→atlas raw→SpriteCatalog→现有战斗材质GPU各5样点，当前Direct3D11/URP/Gamma符合source-alpha。CS0/Scene dirtyfalse，3056/3059原样，其余3文件仅累计已声明变更、零缺失；原processor/shader hash保持。Ledger466/19PASS。实际命令/原始结果/职责与边界见artifacts/diagnostics/NTSD28-B11-PNG-SHEET-ALPHA-CONTRACT-001/Q02-D-REPORT.md。未验全局prewarm/publication、全场Play、atlas-array/pageGPU和其他色彩空间；后继NTSD28-B11-CATALOG-PUBLICATION-CONTRACT-001已READY_CONTRACT，Q03/Q07/Q09条件保持。
