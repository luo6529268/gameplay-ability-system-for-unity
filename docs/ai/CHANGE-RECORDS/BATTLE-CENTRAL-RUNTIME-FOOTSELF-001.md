# BATTLE-CENTRAL-RUNTIME-FOOTSELF-001 — Play Mode FootSelf 中央批量渲染

<!-- CHANGE-RECORD
id: BATTLE-CENTRAL-RUNTIME-FOOTSELF-001
status: CODE_WRITTEN
code-path: Assets/NTSD/Scripts/Animation/Rendering/BattleCentralEditorPreview.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/BattleFootMarkerBatchBackend.cs
code-path: Assets/NTSD/Scripts/Simulation/Presentation/BattlePresentationShadowBuild.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/BattlePixelFramePlan.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/BattleCentralRenderSystem.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/BattleRenderFeature.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/Editor/BattleCentralRuntimeFootMarkerEditorTests.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/Editor/BattleCentralLatestFrameMaterializationEditorTests.cs
asset-path: Assets/NTSD/Scene/NTSD_Battle.unity
authority: USER-REQUEST-RUNTIME-FOOTSELF-REUSE-EDITOR-PREVIEW-SIZE-2026-09-02
evidence: TEST_FIRST_6_FOCUSED_CASES_WRITTEN; EXTERNAL_DOTNET_COMPILE_0_AFTER_SHADOW_ANCHOR_AND_STABLE_CHARACTER_SCALE; UNITY_COMPILE_AND_RUNTIME_PENDING_MCP_INSTANCE_DISCONNECTED
-->

> 创建日期：2026-09-02
>
> 当前状态：`CODE_WRITTEN / EXTERNAL_COMPILE_PASS / UNITY_COMPILE_PENDING / FOCUSED_PENDING / PLAY_RUNTIME_PENDING / PRESENTATION_ONLY`

## 1. 用户要求

用户确认正式Play Mode需要新增FootSelf黄色圆圈，并延用
`BattleCentralEditorPreview`中已设置的尺寸。前序Preview Change已经确认原common Shadow与
FootSelf是两个独立层，本Change不得回退该结论。

## 2. 改前状态

- Play Mode已有common Shadow、actor central mesh与runtime HP batch。
- FootSelf只存在于Edit Mode Preview自己的frame/backend，`Application.isPlaying`时预览
  不提交，因此正式运行不可见。
- Preview已经作为runtime HP authoring来源，可沿用同一Resources查找与优先级机制读取
  Foot Marker settings。
- Scene中Preview的Foot Marker style已是用户设置的64×24、offset(0,0)、white tint，但
  `footMarkerSprite`仍为空；Edit Mode靠AssetDatabase fallback显示。必须只补正式Sprite
  引用，否则Player Build没有资源可达性。
- 正式central submission是双buffer并受read lease保护；新增marker backend必须加入同一
  mutation/current-frame契约，不能成为悬空的全局Mesh。

## 3. 计划实现

1. snapshot捕获时，以现有human roster input binding生成`ShowSelfFootMarker`。
2. Entity command透传该标志，不改变command位置、sprite或HP语义。
3. 新增无每帧分配的单Mesh marker backend，从当前captured frame收集Self Entity command。
4. Preview提供runtime Foot Marker authoring settings；CentralRenderSystem在feature注册、
   scene load和OnValidate时刷新。
5. 双buffersubmission同时构建entity、FootSelf和health，RenderFeature按
   FootSelf→entity/Shadow→HP提交。
6. focused tests覆盖选择、尺寸/offset、动画无关、空帧清理、1000合批和lease mutation。
7. Unity compile、focused、offscreen/Edit预览回归和真实Play截图验证后更新本Record。
8. 根据用户2026-09-02 Play截图反馈，修正FootSelf锚点：与common Shadow共用
   `X + RenderOffset - CameraX, ZInt`地面位置，禁止使用会随跳跃变化的`DisplayZ + YInt`。
9. Inspector Width/Height改为79像素标准角色的基准尺寸，按角色资源稳定高度等比缩放；
   offset不缩放，比例不读取当前动画帧，防止动画切帧尺寸抖动。

## 4. 禁止范围

- 不实现FootFriend/FootEnemy。
- 不改变C++ battle rule、30Hz tick、input时点或roster ownership。
- 不把Editor Preview MonoBehaviour变成逐帧运行逻辑；它只提供序列化authoring值。
- Scene只允许把Preview的空`footMarkerSprite`绑定到FootSelf；不改其他Scene字段、Prefab
  或用户PNG/meta。

## 5. 验证记录

### 5.1 实际代码

- snapshot捕获对`LF2Character`调用现有
  `SimulationWorld.IsBoundActiveHumanRosterInputEntity`，得到presentation-only
  `ShowSelfFootMarker`；AI/武器/特效不携带该标志。
- Entity command新增稳定Foot anchor与Self marker标志。用户Play截图证明最初复用了health
  ground的`DisplayZ+Y`会随跳跃抬升；修正目标是直接复用Shadow的
  `ScreenPixelToWorld(X+RenderOffset-CameraX, ZInt)`。
- Snapshot将同一份资源稳定高度明确暴露为`StableCharacterHeightPixels`；既有
  `StableHealthAnchorHeightPixels`保持同值兼容入口，FootSelf不再通过血条语义字段取比例。
- 新增`BattleFootMarkerBatchBackend`：无每帧容器分配、每marker一quad、单Mesh/submesh，
  基准尺寸=`Preview style pixels * UnitsPerPixel`，不再乘`BattleVisualScale`；实际宽高再乘
  稳定角色高度相对79像素的比例，offset保持Inspector最终像素。
- central submission双buffer新增marker backend与mutation/current-frame/lease校验；capacity、
  publish、reset、legacy/failure clear均同步覆盖。
- RenderFeature按FootSelf batch→原central segments（含Shadow/actor）→HP batch提交，复用同一
  fallback central material和FootSelf texture，不创建Material实例。
- Preview新增runtime Foot Marker authoring读取；scene中的既有64×24/offset/tint不变，只把
  空Sprite引用绑定到FootSelf GUID，保证Player Build资源可达。
- 新增6个focused tests：稳定资源高度比例、authoring精确复用、Shadow地面anchor/64×24换算、
  2×角色比例且offset不缩放、non-self/旧帧清空、1000 marker单Mesh/submesh；同步更新
  latest-frame reflection ctor夹具。

### 5.2 当前验证

- `dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:minimal`：在修正
  `BattleRenderFeature.BattleRenderPass.Dispose()` 遗留的 `healthMaterial` 旧字段名后重新执行，
  exit code 0、0 error、81个既有warning。
- 测试源码先写；首次完整外部编译暴露测试专用Vector3 comparer不可见的2个CS0103，已只改
  为逐分量tolerance断言，随后外部编译0 error。
- Unity用户刷新时曾报告`BattleRenderFeature.cs(340,17)`的CS0103；根因是字段重命名为
  `fallbackMaterial`后，`Dispose()`仍写旧名。此次只把该行改成`fallbackMaterial = null`，
  全文件已无`healthMaterial`引用，外部完整Editor工程编译再次为0 error。
- 用户Play截图确认FootSelf会随跳跃身体抬升且大角色圆圈偏小。实现修正后重新执行
  `dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:minimal`：exit code 0、
  0 error、81 warnings；`git diff --check`对本轮目标文件无空白错误。
- Unity MCP HTTP server启动时发现`gameplay-ability-system-for-unity@b1b02287`，但随后Editor
  plugin未保持6401连接，`instances`为空且refresh返回503；因此本轮Unity compile、focused
  tests和Play截图仍须在Editor重新连接/刷新后复验，不能由外部compile替代。
- UnityMCP已发出force all/script compile请求，但当前Editor的6401 bridge持续timeout；
  `Library/ScriptAssemblies/Assembly-CSharp.dll`尚未fresh重建。观察与用户刚才正在Play Mode
  的上下文一致，且stop请求也因同一bridge timeout未被确认。
- 因此不得把外部compile扩大为Unity compile/focused/Play PASS；等待Editor退出当前Play并
  恢复bridge后继续，不需要改代码或另开第二个Unity。
