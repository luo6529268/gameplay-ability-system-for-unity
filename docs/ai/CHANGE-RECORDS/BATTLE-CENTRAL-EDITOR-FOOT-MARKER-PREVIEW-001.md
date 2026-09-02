# BATTLE-CENTRAL-EDITOR-FOOT-MARKER-PREVIEW-001 — 中央脚底标记 Editor 预览

<!-- CHANGE-RECORD
id: BATTLE-CENTRAL-EDITOR-FOOT-MARKER-PREVIEW-001
status: FOCUSED_TEST_PASS
code-path: Assets/NTSD/Scripts/Animation/Rendering/BattleFootMarkerStyle.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/BattleCentralEditorPreview.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/Editor/BattleCentralEditorPreviewEditor.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/Editor/BattleCentralEditorPreviewEditorTests.cs
authority: USER-REQUEST-FOOTSELF-PLUS-EXISTING-ENTITY-SHADOW-CENTRAL-EDITOR-PREVIEW-2026-09-02
evidence: UNITY_COMPILE_0; FOCUSED_10_10_PASS_JOB_4154DD991BA04939BF964F2FF2713495; COMMON_SHADOW_1000_ONE_SEGMENT_ONE_DRAW; FOOT_1000_ONE_SEGMENT_ONE_DRAW; OFFSCREEN_SHADOW_1_FOOT_1_PASS; GREEN_0; SCENE_DIRTY_UNCHANGED
-->

> 创建日期：2026-09-02
>
> 当前状态：`FOCUSED_TEST_PASS / EDITOR_PREVIEW_READY / USER_VISUAL_REVIEW_PENDING / RUNTIME_NOT_STARTED / PRESENTATION_ONLY`

## 1. 用户要求

用户要求在角色脚下显示 `Assets/NTSD/Sprite/UIPanels/FootSelf.png`，使用中央渲染系统，
先通过 `BattleCentralEditorPreview` 在 Scene View 中查看效果，并可调整尺寸和位置。

用户随后明确：角色表现本身已有 `Shadow`，`FootSelf` 是新增圆圈，不能用 FootSelf 替代
原 Shadow。预览必须同时显示原通用阴影与新增圆圈。

## 2. 当前观察

- FootSelf 为128×48透明PNG，黄色椭圆，TextureImporter是Sprite、Point Filter、无mipmap。
- 资源及meta是用户现有未跟踪内容；本 Change只读取，禁止覆盖、重导入设置或删除。
- 改前Editor Preview只有actor `BattleDynamicMeshBackend`和health
  `BattleHealthBarBatchBackend`，没有通用阴影或脚底标记预览批次。
- Preview由`BattleRenderFeature.BattleEditorPreviewPass`调用`AppendDrawCommands`，可安全
  在同一command buffer中按shadow→marker→actor→health顺序提交。
- `BattleDynamicMeshBackend`已经支持同texture/material连续command合为一个segment，
  不需要新增另一套Mesh实现。
- 正式通用阴影资源接线为`GameConfig.asset/ShadowPrefab`→
  `Assets/NTSD/Prefabs/Common/Shadow.prefab`→根组件`BattleCommonShadowDescriptor`；正式
  presentation以角色脚底逻辑点作为Shadow command position，并使用descriptor构建的
  `BattleCommonVisualBinding.PixelSize/Pivot/RenderState/NormalizedUv`。

## 3. 设计

### 3.1 数据

新增`BattleFootMarkerStyle`：最终像素尺寸、相对角色pivot的像素偏移、顶点tint。
默认值使用FootSelf原始128×48，offset为零，tint为白色。Style归一化只保证尺寸大于0。

每个preview actor增加独立`showCommonShadow`与`showFootMarker`开关；正式runtime“只显示
自己/队友/敌人”的选择逻辑不在本Change实现。

### 3.2 合批

Preview使用独立：

```text
footMarkerFrame
footMarkerBackend : BattleDynamicMeshBackend
PreviewFootMarkerResourceResolver
```

所有actor的marker使用同一个FootSelf texture、同一material、同一binding mode，因此在
4096 quad以内必须形成一个chunk、一个segment、一个submesh；RenderFeature只调用一次
marker DrawMesh。它与actor texture分开，避免把FootSelf强塞进角色sheet/atlas。

原通用阴影同样使用独立`shadowFrame`与`shadowBackend`。所有角色的Shadow使用同一个
正式common binding和中央材质，必须合为单独一个segment/draw；它不读取FootSelf style，
也不与FootSelf混成同一资源层。

### 3.3 顺序与布局

```text
Common shadow batch
→ Foot marker batch
→ Actor segments
→ Health batch
```

marker中心=`actor pivot world + offsetPixels * unitsPerPixel`。尺寸表示最终逻辑像素，
resolver会补偿`BattleDynamicMeshBackend`内部`BattleVisualScale`，使Inspector填128×48时
世界范围正好是128×48逻辑像素，而不是再次乘1.5。

### 3.4 Editor authoring

- 黄色wire bounds显示每个marker范围。
- 灰色wire bounds显示正式通用阴影范围；其尺寸/Pivot来自Shadow prefab，不引入第二套
  手工Shadow尺寸配置。
- Actor0提供“Foot Marker Offset（全部 Actor）”二维拖拽手柄。
- Inspector继续使用序列化字段调整Sprite、size、offset、tint和总开关。
- “配置佐助示例”同时填入正式Shadow prefab、FootSelf sprite和默认style。
- Focus Preview把Shadow和marker bounds都纳入取景范围。

## 4. 验收与不变量

- 不修改Scene/Prefab；预览对象仍`ExecuteAlways`且Edit Mode only。
- 不创建SpriteRenderer或per-marker GameObject。
- 不改中央runtime submission、health runtime authoring或BattleRenderFeature材质所有权。
- actor pivot和health stable anchor现有计算不变。
- Shadow或FootSelf asset缺失时只跳过对应层，不阻止actor/health预览。
- Dispose必须释放shadow/foot backend和两个frame publication binding。

## 5. 待执行证据

- Unity compile。
- focused preview/foot batching tests。
- 离屏 validation PNG/result。
- Scene View实际authoring观察。
- Change Ledger validator和scoped diff。

## 6. 实际代码

- 新增`BattleFootMarkerStyle.cs`：128×48默认最终像素尺寸、offset、tint和归一化。
- `BattleCentralEditorPreviewActor`新增独立`showCommonShadow`和`showFootMarker`。
- Preview新增独立common-shadow frame/backend/resolver；binding由正式
  `BattleCommonVisualCatalog.Build(Shadow.prefab)`取得，位置、native pixel size、Pivot、
  RenderState与正式presentation命令一致。
- Preview新增独立foot frame、dynamic mesh backend、resolver和诊断计数；复用现有中央
  backend，不创建新Mesh算法或per-marker GameObject。
- marker resolver用`style.SizePixels / BattleVisualScale`补偿backend内部visual scale，
  确保Inspector尺寸是最终逻辑像素。
- `AppendDrawCommands`固定common shadow→FootSelf marker→actor→health。
- Editor增加灰色Shadow bounds、黄色FootSelf bounds、Actor0全局offset手柄、sample正式
  Shadow/FootSelf配置和focus bounds。
- 新增正式Shadow descriptor/catalog asset测试，以及单角色四层draw、1000 Shadow单
  segment/单chunk/单draw、1000 FootSelf单segment/单chunk/单draw测试；离屏验证报告新增
  shadow与marker count/segment字段。
- 未修改FootSelf PNG/meta、BattleRenderFeature、runtime submission、Scene、Prefab、shader或
  material。

## 7. 验证结果

- UnityMCP对`gameplay-ability-system-for-unity@b1b02287`执行force all refresh与script
  compile；`Library/ScriptAssemblies`于12:16:32/33 fresh生成，Editor.log无`error CS`、
  `Compilation failed`或`Scripts have compiler errors`。另有一次`dotnet build
  Assembly-CSharp-Editor.csproj --no-restore`为0 error（94个既有warning）。
- 最后一个脚本改动后的EditMode job `4154dd991ba04939bf964f2ff2713495`：
  `BattleCentralEditorPreviewEditorTests` 10/10 PASS，failed=0、skipped=0。
- 新增断言确认FootSelf为128×48 Sprite、Point Filter、无mipmap。
- 单角色测试确认正式Shadow与FootSelf是两层，自定义FootSelf 64×20与(3,-4) offset的
  bounds换算精确，draw count为shadow1+marker1+actor1+health1=4。
- 1000 actor测试确认shadow backend resolved=1000、segment=1、chunk=1、quad=1000，
  foot backend同样为1000/1/1/1000；总draw仅shadow1+marker1+actor1=3。
- 菜单`NTSD/Battle Rendering/Validate Edit Mode Central Preview` fresh PASS：
  actor=1、shadow=1/1 segment、foot=1/1 segment、health=1/3 quads、nonClear=1031、
  red=100、yellow=75、
  greenSeparator=0、Scene dirty unchanged。
- 证据：
  `Temp/BATTLE-CENTRAL-EDITOR-PREVIEW-001/editmode-preview.result.json`与
  `editmode-preview.png`。已目视确认原通用Shadow与FootSelf黄色椭圆同时位于角色脚底，
  actor覆盖两个地面层中央。
- 本Change只达到Editor Preview ready；正式runtime own-player选择、snapshot command、
  runtime batching和Play Mode验证明确未开始，等待用户先确认尺寸/位置。
- `git diff --check`通过。Unity Console没有脚本编译错误；one-shot stdio MCP连接关闭时会留下
  `NetworkStream disposed`工具自身error，不属于项目脚本编译或focused test失败。
- `Tools/Validate-ChangeLedger.ps1`已执行但repository-wide exit1，仅有两个任务外error：
  `CLIENT-CONTENT-FRAME-STRUCTURE-ALIGNMENT-001.md`缺`code-path` metadata，以及并行
  `Assets/NTSD/Scripts/Test/WeaponSpawner.cs`脚本diff尚无Record。本Change四个脚本路径均由
  当前Record覆盖，不修改或吸收这两个无关范围。
