# Handoff — BATTLE-MAP-BOUNDARY-ASSET-001

> 日期：2026-08-26
> 状态：P1 FOCUSED_TEST_PASS / P2 RUNTIME_PENDING / P3 FOCUSED_TEST_PASS / P4 RUNTIME_PENDING / MAPCFG-005 IN_PROGRESS (COMPILE PENDING)
> 下一包：MAPCFG-005
> 规范计划：Assets/NTSD/Docs/battle-map-boundary-asset-configuration-plan.md

## 正确的问题定义

用户的可行走区域是任意多边形，且 BoundaryWall 与 BoundaryWallManager 已在 Unity 中提供该行为。

本任务不是：

- 新增 polygon battle physics；
- 用矩形替代 polygon；
- 把 C++ Release 规则重新移植；
- 将地图 fingerprint 接入 lockstep。

本任务是：

    现有 BoundaryWall 多边形数据
       ↓
    按 MapId 保存为 Boundary Asset
       ↓
    战斗启动时按 MapId 加载
       ↓
    现有 BoundaryWallManager API 继续判断

## 架构

1. BattleMapBoundaryDefinition：MapId、多个边界组、多个 world X/Y polygon，以及背景 Sprite；
2. MapId Catalog：按 MapId 选择一份 Boundary Definition，不再配对 Presentation Definition；
3. BoundaryWallManager：加载选中边界数据，但不改变现有几何判断；
4. BoundaryWallEditor：显式 Load From Asset / Apply To Asset，使 Scene 编辑与运行时同源；
5. 地图资产只保存几何序列，运行时适配层为既有 BoundaryWall 接口生成序号名称。

## 已确认的语义必须保留

- polygon 并集；
- Point-in-polygon；
- Rect 必须完全落入允许 polygon；
- 现有 edge epsilon、简单 polygon 检查；
- 当前随机可行走点逻辑；
- 当前 polygon 外接 bounds 到 legacy Stage fallback 的行为；
- Unity world X/Y 坐标。

## 已完成包 MAPCFG-001

### Goal

已新增两类 Map Asset 和 MapId 配对/校验，不接入运行时。

### 允许

- 新增独立 ScriptableObject 数据类型；
- 新增纯 Editor validation 和 focused tests；
- 新增相应 Task、Change Record、Ledger、State、Handoff。

### 禁止

- 不改 BoundaryWall / BoundaryWallManager 几何逻辑；
- 不改 Scene、Bg、Camera、SimulationWorld、battle tick；
- 不做 C++ audit、网络、fingerprint 或新 physics；
- 不创建或保存默认地图资产，除非用户后续明确要求。

### 完成标准

Asset 能精确保留当前 JSON 世界顶点数据；MapId 配对错误 fail closed；背景资源不会改变边界数据；compile 和 focused tests 通过。

## 当前执行留痕

- MAPCFG-001 专用 Task 和 Change Record 已在任何 C# 修改前建立；
- CHANGE-LEDGER 和 STATE 已登记 MAPCFG-001 为 FOCUSED_TEST_PASS；
- 已写入三类 Asset/Catalog 脚本和一个 focused Editor test；
- BoundaryWall、BoundaryWallManager、Scene、Asset 实例、DAT、配置、C++、服务器、Bg、Camera 与 runtime 均未改；
- Unity 正常导入/compile 后，focused EditMode job a0b70302cb314b0cbb0a6b6d3fee0457 为 4/4 PASS；
- Tools/Validate-ChangeLedger.ps1 已实际通过（102 条 Record、135 个当前 governed code diff 已覆盖），scoped diff check 已通过；
- P1 未接入 runtime，下一动作是 P2：让 BoundaryWallManager 显式加载选中 Boundary Asset，同时保持既有几何 API。

## 当前包 MAPCFG-002

### Goal

将已经验证的 `BattleMapBoundaryDefinition` 显式装载为 `BoundaryWallManager` 的运行时数据源，而不是重新实现任意多边形。

### 设计边界

- `BoundaryWall` 只接收 `BoundaryData` 的世界 X/Y 顶点复制；原有点/矩形/边缘/simple-polygon 代码不改；
- `BoundaryWallManager` 在显式 load 时构造 transient carrier walls，并只缓存它们；不得与 Scene fallback 同时参与 query；
- invalid load 不替换已激活 source；explicit clear 后才恢复 Scene fallback；
- 所有对象创建和 Asset 读取只发生在 explicit load / clear，不发生在 tick；
- P2 不接 MapId selector、Bg、Camera、Bootstrap、Scene save、C++ 或网络。

### 已完成的 pre-code 证据

- 只读确认 Manager 的所有现有查询统一消费 `_boundaries`；
- 只读确认 JSON export 与 P1 Asset 共享 `BoundaryData -> PolygonData -> Vector2Data` 的 world X/Y 形状；
- MAPCFG-002 Task、Change Record、Ledger、STATE 已在代码前建立；
- 已写 `BoundaryWall` world-data bridge、Manager explicit load/clear source switch 与 focused Editor test；没有修改 Scene / Asset 实例；
- 首次 test-only NUnit overload compile failure 已留档；修复后 Unity compile 与 focused job `850175a9e86141f680f03e2bcb26f7b5` 3/3 PASS；
- existing `BattleRuntimeSelfCheck` result file 于15:53:47为 PASS；scoped diff/static call-site review 已通过，最终 ledger/diff 尚待；
- P2 未创建 default Asset、未接 MapId/Bootstrap，也未跑 Play Mode；真实 production map integration 仍由 P4 验收。

### P2 收口

- final ledger validator 已通过（103 条 Record、138 个 governed diff 已覆盖），scoped diff check 已通过；
- `TryLoadBoundaryDefinition` / `ClearLoadedBoundaryDefinition` 尚未被 App / Simulation 调用，因此不可能悄然改变当前 Battle Scene；
- P2 可作为 `RUNTIME_PENDING` 结束：逻辑与回归测试完成，生产 MapId integration 仍归 P4。

## 当前包 MAPCFG-003

### Goal

在不改变多边形语义的前提下，提供 `BattleMapBoundaryDefinition` 与 Scene `BoundaryWall` 的明确双向作者操作，让 Scene View 编辑与将来 runtime Asset 数据同源。

### 固定边界

- 用户必须显式点击 Load / Apply；没有自动同步、自动保存、自动创建/删除/重排 walls；
- MAPCFG-003 历史验证曾使用 stable hierarchy order + group name preflight；该名称匹配已由 MAPCFG-005
  supersede，新地图资产的 Load 现在只按 stable hierarchy order + boundary count 对齐；
- Apply 只 capture enabled Scene fallback walls 的 world X/Y 深复制；
- active P2 runtime carrier source 未 clear 时，P3 必须拒绝作者操作；
- P3 不接 MapId bootstrap/Bg/Camera/battle/network。

### 当前留痕

- MAPCFG-003 Task、Change Record、Ledger、STATE 与父计划已先行建立；
- 已写 deep-copy/capture/explicit Load-Apply/runtime-source guard/Undo-dirty 与 focused tests；未写 Scene、Asset 实例或运行时 MapId 接线；
- Unity compile、P3 focused job `5e4b965f9e7b4452a5c6e236117b673a` 3/3、static no-save audit/scoped diff 已通过；
- cross-phase job `63182377db004cb084fc830402bbb878` 10/10 与P3后existing self-check 16:26:35 PASS；
- final ledger validator（104 Record / 139 governed diff covered）与scoped diff PASS；
- P3 只完成可测试Editor bridge；用户真实Inspector click 与P4 production integration继续独立。

## 已完成包 MAPCFG-004（合并前历史记录）

### Goal

在角色创建和解除暂停前，将用户配置的 Catalog+MapId 显式 resolve 为 P2 boundary source 和同一 world Bg sprite；无配置则保持当前 Scene fallback，半配置/无效配置 fail-close。

### 观察到的部署 blocker

- `Assets/NTSD` 当前没有实际 Boundary/Presentation/Catalog Asset；
- 未知正式 MapId、要从哪个当前 Scene wall 生成边界、以及要绑定哪个 Bg Renderer；
- 因此 P4 先实现 optional configuration/code fixture，绝不私自创建/保存默认资源或写 Scene；真实 Play acceptance 必须待用户配置。

### 当前留痕

- MAPCFG-004 Task、Change Record、Ledger、STATE 与父计划已先行建立；
- 已写 `BattleBootstrap` optional prepare/clear、`AppManager`/`BattleTestBootstrap` 的 startup gate与 P4 focused in-memory tests；未写 Scene、Asset实例、背景资源、Camera、Transform、PPU、battle rule、C++或网络。
- 空配置不改变当前 fallback，也不触发 P4 新增 Stage refresh；半配置或无效配置在 mutation 前 fail-close；只在成功装载实际 MapId 后 refresh Stage snapshot。
- 保留既有 central-seal/F7 baseline，不拥有或重排其旧 diff。
- Unity import/compile完成；P4 focused job `51942ac652474e6c9ba42427a93ba44a` 4/4 PASS，P1–P4 job `50c3e1586f5145e18b6d990662b920b0` 14/14 PASS，existing self-check于17:33:25 PASS（均为 MAPCFG-005 之前的程序集证据）。
- `Tools/Validate-ChangeLedger.ps1` 已通过（105 Records / 141 governed diff covered）；P4 scoped diff也通过（仅既有 LF→CRLF 提示，无 whitespace error）。
- 真实 Map Asset/Scene/Play验收仍缺用户的MapId与四个Inspector引用配置，当前不可宣称真实地图已部署。

### 后续验收

同一几何的 source wall / Asset source 必须在 point、rect、deterministic random sample 和 Stage bounds 上一致；随后运行 Unity compile、focused tests、relevant self-check、validator 与 scoped diff。

## 当前包 MAPCFG-005

### Goal

按用户确认收敛地图资产：删除独立 `BattleMapPresentationDefinition` 与 `Desert01_Presentation.asset`，将背景 Sprite 放入 `Desert01_Boundary.asset`，并让地图资产不再保存 `boundaryName` 或多边形 `name`。

### 固定范围

- Catalog Entry 只保留 `mapId` 与 `BattleMapBoundaryDefinition`；
- `BattleBootstrap` 从 Boundary Definition 读取背景，并继续沿用现有 prepare/clear 时序；
- 地图几何使用不带命名字段的 `MapBoundaryData` / `MapPolygonData`；
- `BoundaryWallManager` 加载时生成稳定序号名，仅供既有运行时/JSON/编辑器兼容层使用；
- 共享 `BoundaryData` / `PolygonData` 字段、既有 JSON 导出、战斗 tick、RNG、checksum、Camera、服务器和 C++ authority 不在本包删除或改变；
- 不部署默认 `stage.dat`，不把当前代码级证据扩大为完整 Play Mode 验收。

### 当前留痕

- `MAPCFG-005` Task 和 Change Record 已在代码修改前建立，并已登记到 Ledger、STATE 与本 handoff；
- 已修改 `BattleMapBoundaryDefinition`、`BattleMapCatalog`、`BattleBootstrap`、`BoundaryWallManager` 及四个 MAPCFG focused Editor test；
- 已将 `Desert01_Boundary.asset` 改为单资产背景/几何结构，Catalog 指向 `desert_01` 边界资产；
- 已删除 Presentation 脚本及 Desert01 Presentation 资源；
- 本轮静态契约检查、`git diff --check` 和 Ledger 覆盖校验已通过；Unity 修复前实际编译日志发现 `BattleMapBoundaryDefinition.cs` 的 4 个 `CS0122`，已通过构造函数 deep-copy 修复；修复后复用 Unity Roslyn 参数的交叉编译退出码为 `0`，仅有工程既有 warnings。当前 Unity Editor 的正式程序集仍未刷新，本轮 focused tests/新程序集 self-check 尚待，不能宣称 `COMPILE_PASS` 或 `VERIFIED`。

### 下一步

1. 让当前 Unity Editor 刷新修复后的脚本并取得新的正式程序集与 0 C# error；
2. 运行 MAPCFG-001～005 的 focused 回归，重点验证纯几何资产、序号名适配、Catalog resolve 和背景恢复；
3. 运行当前 Editor 的 `BattleRuntimeSelfCheck` 与 `Tools/Validate-ChangeLedger.ps1`；
4. 若代码级证据通过，再确认 `BattleBootstrap` 的真实 Scene 引用和 Play Mode 验收是否具备条件。

## 旧计划状态

BATTLE-MAP-ASSET-ARCHITECTURE-001 在未写任何代码前被发现范围错误：它把已有的 BoundaryWall polygon 行为误分类为需要新 C++ physics audit 的功能。该旧计划现在只保留为历史记录，不得执行其中的 M0 至 M7。
