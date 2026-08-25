# NTSD Map ID、BoundaryWall 与地图资源配置化计划

> 计划 ID：BATTLE-MAP-BOUNDARY-ASSET-001  
> 版本：1.0  
> 状态：IN_PROGRESS / P1 FOCUSED_TEST_PASS / P2 RUNTIME_PENDING / P3 FOCUSED_TEST_PASS / P4 RUNTIME_PENDING (DEPLOYMENT INPUT PENDING)  
> 最后更新：2026-08-25  
> 这是当前唯一应执行的地图配置方案。  
> 本计划不改变 BoundaryWall 和 BoundaryWallManager 已有的多边形行为，只改变它们的地图数据来源。

## 1. 用户目标

用户要实现的是：

1. 不同地图由稳定的 Map ID 标识；
2. 每张地图有一份可行走区域配置；
3. 可行走区域是任意多边形，不是矩形；
4. 多边形语义沿用现有 BoundaryWall 和 BoundaryWallManager；
5. 每张地图另有一份地图资源配置，例如背景图；
6. Editor 中编辑的边界与实际运行时读取的边界必须是同一份数据；
7. 不改变现有战斗规则、C++ 对齐主线、相机逻辑或本地背景表现。

## 2. 已确认的现有基础

当前 BoundaryWall 和 BoundaryWallManager 已经具备本需求的核心几何行为：

| 能力 | 当前来源 | 现有语义 | 本计划处理方式 |
|---|---|---|---|
| 多个任意多边形 | BoundaryWall 的 polygons | 多边形并集；支持凹多边形和简单多边形检查。 | 直接复用。 |
| 点是否可行走 | BoundaryWallManager.IsPointWalkable | 点位于任意启用区域内即允许。 | 直接复用。 |
| 实体矩形是否可行走 | BoundaryWallManager.IsRectWalkable | 矩形必须完全位于某个允许多边形内。 | 直接复用。 |
| 运行时随机可行走点 | BoundaryWallManager.TryGetRandomWalkablePoint | 从现有区域 bounds 采样，再用现有点包含判断过滤。 | 直接复用。 |
| Stage 外接范围 | BoundaryWallManager.TryGetBattleStageRuntime | 从全部有效 polygon 的外接 bounds 导出旧 Stage 宽度/Z 范围。 | 保持现有 fallback 语义。 |
| Editor 顶点编辑 | BoundaryWall / BoundaryWallEditor | 在 Unity world X/Y 平面编辑。 | 继续作为作者界面。 |
| JSON 导出数据 | BoundaryExportData / BoundaryData / PolygonData | 导出 world X/Y 顶点和多个边界组。 | 新 Asset 尽量复用同一数据结构和 world X/Y 单位。 |

因此，本计划不是新增 polygon collision，也不是重新定义角色如何被 polygon 阻挡。

当前要做的只有一件事：

    现有 Scene BoundaryWall 数据
        改为
    按 Map ID 选择的 Boundary Asset 数据
        再交给
    现有 BoundaryWall / BoundaryWallManager 的同一套判断逻辑

## 3. 不需要做的事情

以下事项不属于当前计划：

- 不需要先审计 C++ Release；
- 不需要把 world X/Y 转换成新的整数 X/Z 坐标体系；
- 不需要创建新的多边形碰撞、点包含、矩形包含或随机采样算法；
- 不需要实现地图 fingerprint、StageFingerprint、lockstep packet 或服务器；
- 不需要改变 C++ 对齐的移动、击退、投掷物、opoint、hit 或 AI 规则；
- 不需要改变 Windows 全覆盖、Android 底部黑区、Bg Transform、Camera 或背景表现组件；
- 不需要将可行走区域压缩成矩形。

未来若有联机身份校验需求，可以把 Map ID 和边界资产 revision 纳入一个独立的联机计划；这不是本计划的前置条件。

## 4. 目标数据结构

### 4.1 BattleMapBoundaryDefinition

一张地图一份逻辑边界 Asset。

建议字段：

| 字段 | 用途 | 约束 |
|---|---|---|
| MapId | 稳定地图标识，例如 desert_01。 | 非空、唯一、不可由资源路径自动生成。 |
| DisplayName | Editor 显示名。 | 不参与逻辑判断。 |
| Revision | 手动维护的配置版本。 | 仅用于诊断/未来联机，不在本阶段接入协议。 |
| Boundaries | 多个边界组，每组含多个 polygon。 | 数据形状对齐现有 BoundaryData / PolygonData。 |
| VerticesWorld | 每个 polygon 的 world X/Y 顶点。 | 保持现有 BoundaryWallManager JSON 导出的单位和坐标平面。 |

Boundary Asset 必须保持现有语义：

- 多个 Boundary 之间是并集；
- 一个 Boundary 的多个 polygon 也是并集；
- 点包含、矩形完全包含、边缘 epsilon、简单 polygon 检查继续由现有 BoundaryWall 代码决定；
- 不在本阶段新增 HardBlock、Special 或 holes 的新规则。

### 4.2 BattleMapPresentationDefinition

一张地图一份表现资源 Asset。

建议字段：

| 字段 | 用途 | 约束 |
|---|---|---|
| MapId | 与 Boundary Asset 对应的稳定地图标识。 | 必须完全一致。 |
| DisplayName | Editor 显示名。 | 仅表现。 |
| BackgroundSprite | 背景资源。 | 不能推导、修改或覆盖边界。 |
| Optional decoration references | 装饰、音效、特效等资源。 | 仅表现。 |

它不保存可行走区域，不写入 BoundaryWallManager，也不改变战斗位置、碰撞或随机数。

### 4.3 BattleMapCatalog

需要一个很轻量的选择表，把两类 Asset 按 MapId 配对。它可以是独立 Catalog Asset，也可以是现有 GameConfig 中的序列化 MapEntry 列表；实施时选择较少侵入现有项目的方式。

每一项只包含：

    MapId
    BoundaryDefinition
    PresentationDefinition

Catalog 的唯一职责是：

- 根据当前选择的 MapId 找到两份配置；
- 校验没有重复 MapId；
- 校验两个 Asset 的 MapId 一致；
- 在启动前报告缺失资源或配置错误。

它不是第三份地图内容，也不保存 polygon。

## 5. 运行时与 Editor 的职责分离

### 5.1 Editor 作者流程

    选择 BattleMapBoundaryDefinition
       ↓
    在 Scene View 使用现有 BoundaryWall 顶点工具编辑
       ↓
    用户明确点击 Apply to Map Asset
       ↓
    Asset 保存 world X/Y polygon 数据
       ↓
    再次加载该 Asset 时恢复相同 BoundaryWall 显示

要求：

- 不能在 OnValidate、Update 或 Play Mode 中自动把 Scene 覆盖到 Asset；
- 不能在运行时编辑自动保存用户 Scene；
- Bg 图片、Camera、分辨率变化不能改变 polygon；
- Scene View 只是同一份 Asset 的可视编辑器，不是第二个逻辑来源。

### 5.2 Battle 启动流程

    当前 MapId
       ↓
    Catalog 找到 BoundaryDefinition + PresentationDefinition
       ↓
    BoundaryWallManager 加载 BoundaryDefinition
       ↓
    现有 IsPointWalkable / IsRectWalkable / TryGetRandomWalkablePoint
       ↓
    现有 TryGetBattleStageRuntime 继续从 polygon 外接 bounds 产生 legacy Stage fallback

要求：

- 只在 battle bootstrap / 地图加载时设置一次；
- battle 中不切换 MapId；
- battle 中不扫描所有 Scene BoundaryWall 作为另一套真相；
- 不改变现有几何判断结果，只让其读到所选地图的数据。

### 5.3 表现加载流程

    当前 MapId
       ↓
    Catalog 找到 PresentationDefinition
       ↓
    Bg 或现有背景表现组件读取 BackgroundSprite

要求：

- PresentationDefinition 永远不能写 BoundaryDefinition；
- Android/Windows 的显示差异只影响画面；
- 同一 MapId 在不同平台始终加载同一份 polygon 边界数据。

## 6. 分阶段实施

| 阶段 | Work Package | 目标 | 当前状态 |
|---|---|---|---|
| P1 | MAPCFG-001 | 建立两类地图 Asset 和 Map ID 配对表 | FOCUSED_TEST_PASS / RUNTIME_NOT_CONNECTED |
| P2 | MAPCFG-002 | 让现有 BoundaryWallManager 从选中 Boundary Asset 读取数据 | RUNTIME_PENDING / P3 READY / P4 INTEGRATION PENDING |
| P3 | MAPCFG-003 | Editor 中 Asset 与 BoundaryWall 的显式双向作者工具 | FOCUSED_TEST_PASS / P4 READY / MANUAL-INSPECTOR PENDING |
| P4 | MAPCFG-004 | Bootstrap 的 Map ID 选择、背景资源加载和定向验收 | RUNTIME_PENDING / DEPLOYMENT INPUT PENDING |

### P1 — MAPCFG-001：地图 Asset 和 Map ID 配对

**Goal**

新增 BattleMapBoundaryDefinition、BattleMapPresentationDefinition 和最小配对表。

**解决方案**

- Boundary Asset 的结构对齐当前 JSON 导出结构，不发明新的 polygon 语义；
- Presentation Asset 只存地图资源；
- 配对表通过 MapId 校验一对一关系；
- 初期只做纯数据和 EditMode validation，不接入 runtime。

**边界**

- 不修改 BoundaryWall 几何算法；
- 不修改 Scene、Bg、Camera、GameConfig 行为；
- 不修改 SimulationWorld、tick、C++、网络或 lockstep。

**验证**

- 重复 MapId、缺失配对、MapId 不一致明确报错；
- Asset 反复保存/重载后 world X/Y 顶点顺序不变；
- 改背景 Sprite 不会改 boundary 数据；
- Unity compile 与 focused EditMode tests 通过。

**停止条件**

需要改变现有 BoundaryWall polygon 语义，或 Asset 数据无法表达现有 JSON 导出的世界顶点。

### P2 — MAPCFG-002：BoundaryWallManager 数据源切换

**Goal**

在不更改现有几何算法的前提下，让选中 MapId 对应的 Boundary Asset 成为 BoundaryWallManager 的数据源。

**解决方案**

- 在地图加载边界提供显式 Load / Apply API；
- 保持现有 BoundaryWall 的 ContainsPoint、IsRectAllowed、TryGetWorldVertices 和 Manager public API 语义；
- Asset 到 BoundaryWall 的恢复使用原有 world X/Y 顶点，不做新的坐标换算；
- 现有 Scene BoundaryWall 仅作为 Editor 预览/legacy fallback，不与选中 Asset 同时竞争。

**边界**

- 不重写 point-in-polygon、rect-in-polygon、边缘 epsilon、随机采样或外接 bounds 算法；
- 不增加每 tick 的 Asset 读取、Scene 查找或 GC 分配；
- 不把 polygon 改写为矩形。

**验证**

- 对同一份原始边界，Asset 加载前后 IsPointWalkable、IsRectWalkable、TryGetRandomWalkablePoint 和 TryGetBattleStageRuntime 结果一致；
- 多个凹 polygon、边缘点、矩形跨边界、禁用区域等现有语义有 focused tests；
- battle 进行时更改 Bg 或 Camera 不改变已加载边界；
- Unity compile、focused tests、BattleRuntimeSelfCheck 和定向 Battle Scene 验证。

**停止条件**

Asset 加载导致现有 API 语义变化，或需要修改移动/碰撞/hit 顺序才能工作。

### P3 — MAPCFG-003：Editor 作者工具

**Goal**

保证 Scene View 的顶点编辑与实际 Asset 数据一一对应，消除“编辑看到的边界”和“运行时加载的边界”不同源的问题。

**解决方案**

- 复用 BoundaryWallEditor 的可视编辑能力；
- 增加明确的 Load From Map Asset 和 Apply To Map Asset 操作；
- 当前 MapId 和绑定 Asset 在 Inspector 可见；
- 加载 Asset 后 Scene View 显示它的 polygon，运行时也加载同一份数据。

**边界**

- 不自动保存 Scene；
- 不自动覆盖 Asset；
- 不根据背景图片自动缩放或平移边界；
- 不修改背景表现、Camera 或战斗逻辑。

**验证**

- Asset → BoundaryWall → Asset 往返不丢顶点、顺序或 world 坐标；
- Scene View 编辑后只有用户点击 Apply 才写 Asset；
- 重新打开/加载后 runtime 边界与 Scene View 一致；
- 不保存用户已有 dirty Scene。

**停止条件**

现有 BoundaryWallEditor 不能按 Asset 数据完整恢复，或需要改变其几何合同。

### P4 — MAPCFG-004：Map ID 启动选择与地图资源

**Goal**

在战斗开始前选择 MapId，加载对应 boundary 与 presentation，并验证不同地图切换。

**解决方案**

- 在明确的 battle bootstrap 入口读取当前 MapId；
- 先校验 Catalog，再加载 BoundaryDefinition；
- 然后把 PresentationDefinition 交给当前 Bg/背景表现接口；
- 无效 MapId、缺少 boundary 或 MapId 不匹配时在开始战斗前 fail closed。

**边界**

- 不增加 fingerprint 或网络协议；
- 不在战斗中切图；
- 不改变 Windows/Android 背景表现的既有独立逻辑；
- 不在本阶段改背景资源本身。

**验证**

- Map A 和 Map B 各自加载正确的 polygon 和背景资源；
- 同一 MapId 在 Windows/Android 得到相同边界结果；
- 背景资源替换不会改变 IsPointWalkable / IsRectWalkable；
- 无效 MapId 不启动 battle；
- Unity compile、focused tests、真实 Battle Scene 运行时验收。

**停止条件**

需要为选择地图而重构战斗 tick、Stage 规则或联机协议。

## 7. 修改与进度留痕

本计划之后每个脚本阶段必须：

1. 先创建对应 Task Contract；
2. 先创建唯一 Change Record；
3. 在 CHANGE-LEDGER、STATE、本计划和 Handoff 中登记；
4. 再修改脚本；
5. 修改后立即登记实际文件、符号、验证、风险和回滚；
6. 阶段关闭前运行 Tools/Validate-ChangeLedger.ps1。

状态只能依据真实证据推进；编译通过不等于 Battle Scene 已验证。

## 8. 当前进度

| 日期 | 内容 | 状态 | 说明 |
|---|---|---|---|
| 2026-08-25 | 方案 v1.0 | PLANNED / NO CODE | 明确复用现有 BoundaryWall/BoundaryWallManager 多边形语义；移除对 C++ Release 审计、矩形化、fingerprint 和新 polygon physics 的错误前置。 |
| 2026-08-25 | MAPCFG-001 pre-code | IN_PROGRESS | 已建立专用 Task、Change Record、Ledger、STATE、Handoff；尚未写 C#、测试、Scene 或 Asset。 |
| 2026-08-25 | MAPCFG-001 code | CODE_WRITTEN | 已新增 Boundary Asset、Presentation Asset、Catalog 与内存 focused test；未改 BoundaryWall 几何、Scene、runtime、C++、网络或背景表现；compile/test 待。 |
| 2026-08-25 | MAPCFG-001 verification | FOCUSED_TEST_PASS | Unity 正常导入与 compile 后，focused EditMode job a0b70302cb314b0cbb0a6b6d3fee0457 为 4/4 PASS；`Tools/Validate-ChangeLedger.ps1`（102 条 Record / 135 个 governed diff covered）与 scoped diff check 通过；未接入 runtime，P2 继续。 |
| 2026-08-25 | MAPCFG-002 pre-code | IN_PROGRESS | 已只读核验现有 `BoundaryData -> BoundaryWall -> BoundaryWallManager` 查询链，确立 transient carrier + explicit load/clear 的最小方案；Task、Change Record、Ledger、STATE、Handoff 已先行建立，尚未写 P2 C#、Scene 或 Asset 实例。 |
| 2026-08-25 | MAPCFG-002 code | CODE_WRITTEN | 新增 `BoundaryWall` world-data copy bridge、Manager transient carrier explicit load/clear/refresh guard 与三项 focused Editor tests；未改既有几何、Scene、Asset 实例、Bootstrap、Camera、Bg、C++ 或 battle rule。compile/test 尚待。 |
| 2026-08-25 | MAPCFG-002 focused verification | FOCUSED_TEST_PASS | 首次 test-only NUnit collection overload CS1503 已记录并最小修正；Unity recompile 后 job `850175a9e86141f680f03e2bcb26f7b5` 为3/3 PASS，覆盖 query parity、failed load、clear fallback。existing self-check / final governance 待。 |
| 2026-08-25 | MAPCFG-002 self-check | RUNTIME_PENDING | 现有 `BattleRuntimeSelfCheck` 菜单运行后 result 文件15:53:47为`PASS`；final ledger validator（103 Record / 138 governed diff covered）和scoped diff均PASS；不等同P4的MapId/Bootstrap/真实地图 Asset/Player验收。 |
| 2026-08-25 | MAPCFG-003 pre-code | IN_PROGRESS | 已确认采用显式 Editor Load/Apply、world X/Y deep copy、stable hierarchy + name preflight、Undo/dirty but no save、active runtime carrier source fail-close；Task、Change Record、Ledger、STATE、Handoff 均已先建，尚未写 P3 C# / Scene / Asset 实例。 |
| 2026-08-25 | MAPCFG-003 code | CODE_WRITTEN | 已写 Asset deep-copy replace、wall capture、Manager explicit Load/Apply/runtime guard/Inspector confirmation UI、focused tests；未调用保存 API，未改 Scene/Asset实例、P4 bootstrap或battle规则。验证待。 |
| 2026-08-25 | MAPCFG-003 focused verification | FOCUSED_TEST_PASS | Unity recompile后job `5e4b965f9e7b4452a5c6e236117b673a` 为3/3 PASS；static no-save audit/scoped diff通过；P1/P2 shared bridge regression与final governance待。 |
| 2026-08-25 | MAPCFG-003 regression | FOCUSED_TEST_PASS | P1/P2/P3 combined job `63182377db004cb084fc830402bbb878` 为10/10 PASS；P3后existing self-check result于16:26:35为PASS；final ledger validator（104/139）与scoped diff PASS；P3仍不替代P4真实MapId/Bootstrap/Player集成。 |
| 2026-08-25 | MAPCFG-004 pre-code | IN_PROGRESS / DEPLOYMENT INPUT PENDING | 当前无生产 Map Asset/Catalog/MapId；将只实现 optional Catalog+MapId+Bg startup gate及in-memory focused tests，legacy empty config保持fallback，半配置/无效配置fail-close；不创建/保存资源或Scene。 |
| 2026-08-25 | MAPCFG-004 code | CODE_WRITTEN / COMPILE PENDING / DEPLOYMENT INPUT PENDING | 已写 optional Bootstrap prepare/clear、App/Test startup gate和四项内存focused tests。空配置零mutation且不新增Stage refresh；只有实际MapId成功装载才在角色创建前refresh Stage snapshot。未创建真实Asset/MapId/Scene/Bg资源，也未改Camera/Transform/PPU、背景平台表现、battle rule、C++或网络；compile/test/self-check/governance待。 |
| 2026-08-25 | MAPCFG-004 code verification | FOCUSED_TEST_PASS / RUNTIME_PENDING / DEPLOYMENT INPUT PENDING | Unity import/compile完成；P4 job `51942ac652474e6c9ba42427a93ba44a` 4/4、P1–P4 job `50c3e1586f5145e18b6d990662b920b0` 14/14 PASS；existing self-check于17:33:25 PASS。真实Asset/MapId/Scene/Play及final governance仍待；不创建/猜测用户地图配置。 |
| 2026-08-25 | MAPCFG-004 governance | RUNTIME_PENDING / DEPLOYMENT INPUT PENDING | Ledger validator通过（105 Records / 141 governed code diff covered），P4 scoped diff通过（仅既有LF→CRLF提示、无whitespace error）。真实部署/Play仍等用户明确配置MapId和资产引用。 |
