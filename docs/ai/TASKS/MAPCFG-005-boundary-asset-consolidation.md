# MAPCFG-005：合并边界与表现资产

> 状态：`CODE_WRITTEN / COMPILE_PENDING / RUNTIME_PENDING`（静态与账本校验已通过，Unity 重新编译和运行时验证待执行）  
> 日期：2026-08-26  
> 主线：`BATTLE-MAP-BOUNDARY-ASSET-001`

## 用户确认

用户确认可以删除 `Desert01_Presentation`，并要求：

- 将背景图配置增加到 `Desert01_Boundary`；
- 删除地图资产中的 `boundaryName` 和多边形 `name` 字段。

## 目标

收敛当前地图配置为单一的边界资产：由 `BattleMapCatalog` 通过 `mapId` 解析一个
`BattleMapBoundaryDefinition`，由该资产同时提供边界几何和背景图。

## 允许范围

- `Assets/NTSD/Scripts/LevelEditor/BattleMapBoundaryDefinition.cs`
- `Assets/NTSD/Scripts/LevelEditor/BattleMapCatalog.cs`
- `Assets/NTSD/Scripts/LevelEditor/BattleMapPresentationDefinition.cs`（删除）
- `Assets/NTSD/Scripts/App/BattleBootstrap.cs`
- 与 MAPCFG-005 直接相关的 Editor/EditMode 测试、场景配置和地图资产
- `Assets/NTSD/Docs/battle-map-boundary-asset-configuration-plan.md`
- `docs/ai/STATE.md`、当前 handoff、Ledger 和本 Task/Change Record

## 明确禁止

- 不修改 C++ release authority、战斗 tick、输入、RNG、checksum、碰撞或网络逻辑；
- 不修改 `Assets/NTSD/Scripts/Gen/`、`Assets/Plugins/`；
- 不扩展为新的地图功能包，不部署默认 `stage.dat`；
- 不删除共享 `BoundaryData` / `PolygonData` 的兼容字段或改变既有 JSON/编辑器合同；
- 不把编译或 self-check 通过误报为真实配置下的完整运行时验收。

## 设计边界

地图资产使用不带命名字段的边界组/多边形数据；加载到既有 `BoundaryWall` 运行时前，
由适配层生成仅供运行时和兼容工具使用的稳定序号名称。名称不再参与地图资产解析、
边界匹配或地图配置有效性的判定。

## 前置状态

- `BattleMapPresentationDefinition`、`Desert01_Presentation.asset` 和 Catalog 的表现引用
  仍存在；这是本变更要清理的现状。
- `BattleMapBoundaryDefinition` 当前使用带 `boundaryName` / `PolygonData.name` 的共享数据模型；
  需要收敛资产数据契约，但保留共享运行时兼容层。
- 当前 Unity Editor 已由用户启动；不得启动第二个会写同一 `Library` 的 Unity 实例。

## 预期副作用

- Catalog Entry 只保留 `mapId` 与边界资产引用；
- `BattleBootstrap` 从边界资产读取背景 Sprite；
- 旧的 Presentation 资源不再被运行时解析；
- 边界载入、场景 authoring 和历史兼容 JSON 仍可使用运行时生成的序号名称。

## 完成标准

1. 工程脚本编译 0 error；
2. MAPCFG-005 相关 EditMode/配置回归测试通过；
3. 当前 Unity Editor 的 `BattleRuntimeSelfCheck` 实际通过；
4. `Tools/Validate-ChangeLedger.ps1` 通过；
5. 资产与文档状态区分“已写入”“已编译”“已自检”和“真实场景运行时待验证”。

## 回滚方式

只回退 MAPCFG-005 触及的脚本、资产、测试和文档变更；不使用破坏性 Git 操作，
不影响用户工作树中的其他未提交内容。
