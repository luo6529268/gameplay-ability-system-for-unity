<!-- CHANGE-RECORD
id: MAPCFG-005
status: CODE_WRITTEN
code-path: Assets/NTSD/Scripts/LevelEditor/BattleMapBoundaryDefinition.cs
code-path: Assets/NTSD/Scripts/LevelEditor/BattleMapCatalog.cs
code-path: Assets/NTSD/Scripts/LevelEditor/BattleMapPresentationDefinition.cs
code-path: Assets/NTSD/Scripts/LevelEditor/BoundaryWallManager.cs
code-path: Assets/NTSD/Scripts/App/BattleBootstrap.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleMapAssetConfigurationEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleMapBoundaryRuntimeSourceEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleMapBoundaryAuthoringEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleMapStartupConfigurationEditorTests.cs
authority: USER-DIRECTED-2026-08-26
evidence: PREIMPLEMENTATION-READONLY-2026-08-26
--> 

# MAPCFG-005：合并边界与表现资产

## 状态

`CODE_WRITTEN / COMPILE_PENDING`（代码、测试和地图资产已写入；本轮编译与运行验证待 Unity Editor 刷新）

## 需求来源与权威

- 需求来源：用户于 2026-08-26 明确确认“可以删除 `Desert01_Presentation`”，并要求将背景图并入
  `Desert01_Boundary`、删除地图资产中的 `boundaryName` 和多边形 `name`。
- 战斗逻辑 authority：本变更不改战斗规则；若触及战斗行为，仍须遵守仓库
  `AGENTS.md` 中 `J:\QQFile\NTSD2.4\ntsd_release` 的 release live path 规则。
- 当前配置主线：`BATTLE-MAP-BOUNDARY-ASSET-001`，前置包为 MAPCFG-001～004。

## 改动前现状

- `BattleMapCatalog.Entry` 同时保存边界定义和表现定义引用；
- `BattleBootstrap` 从 Presentation 定义读取背景 Sprite；
- `Desert01_Boundary.asset` 保存边界几何及 `boundaryName` / 多边形 `name`；
- `Desert01_Presentation.asset` 保存背景 Sprite；
- 共享 `BoundaryData` / `PolygonData` 还被边界运行时、authoring、日志和 JSON 合同使用，不能在本记录中直接删除其兼容字段。

## 实际修改的文件与符号

- `BattleMapBoundaryDefinition`：增加背景 Sprite；将地图资产数据收敛为不带命名字段的 `MapBoundaryData` / `MapPolygonData` 几何结构，并保留到既有运行时结构的适配入口。
- `BattleMapCatalog.Entry` / `TryValidateEntry` / `TryResolve`：移除 Presentation 引用和对应校验。
- `BattleBootstrap.TryPrepareMapConfiguration` / `ClearPreparedMapConfiguration`：从边界定义读取和恢复背景。
- `BoundaryWallManager.TryLoadBoundaryDefinition`：将纯几何资产转换为既有 `BoundaryData`，按稳定序号生成运行时兼容名称；authoring Load 改为数量/顺序契约，不再匹配资产名称。
- `BattleMapAssetConfigurationEditorTests`、`BattleMapBoundaryRuntimeSourceEditorTests`、`BattleMapBoundaryAuthoringEditorTests`、`BattleMapStartupConfigurationEditorTests`：同步单资产/纯几何数据模型并增加无 legacy 名称字段断言。
- `Desert01_Boundary.asset`、`BattleMapCatalog.asset`：写入背景 Sprite、`desert_01` 边界引用和无名称字段的边界几何；未修改 Battle Scene 引用。
- `BattleMapPresentationDefinition.cs` 及 `Desert01_Presentation.asset`（含 `.meta`）：按用户确认删除。

## 不可回退边界

- 不改变战斗 tick、pass 顺序、输入、RNG、checksum、碰撞、网络或性能规则；
- 不修改 `Assets/NTSD/Scripts/Gen/`、`Assets/Plugins/`；
- 不部署默认 `stage.dat`；
- 不把共享运行时 `BoundaryData` / `PolygonData` 的历史兼容字段删除为新破坏性合同。

## 预期副作用

- 每个 Catalog entry 只需一个边界资产引用；
- 背景图与边界几何的 mapId/生命周期由同一资产维护；
- 地图资产不再依赖边界/多边形名称，加载适配层使用序号生成运行时兼容名称；
- Presentation 装饰 Prefab 配置不再进入当前地图配置主线。

## 验收标准

- Unity 脚本编译为 0 error；
- MAPCFG-005 定向 EditMode/配置测试通过；
- 当前已启动 Unity Editor 中 `BattleRuntimeSelfCheck` 实际通过；
- `Tools/Validate-ChangeLedger.ps1` 通过；
- 真实 `BattleMapCatalog` + `Desert01_Boundary` 场景的完整 Play Mode 验收若受现有资源或编辑器控制能力限制，必须保留为 `RUNTIME_PENDING`，不得误标 `VERIFIED`。

## 回滚

通过版本控制审查后仅回退 MAPCFG-005 的目标文件；不使用 `reset`、`restore`、`clean` 或删除其他用户文件。

## 实施后记录

### 2026-08-26 当前证据

- 静态契约检查通过：Presentation 脚本/资源不存在；`BattleMapCatalog.asset` 为
  `desert_01 → Desert01_Boundary`；`Desert01_Boundary.asset` 已包含背景 Sprite 且不含
  `boundaryName` / 多边形 `name`；代码和地图资产中无 Presentation runtime 引用。
- `git diff --check` 通过；仅有仓库既有的 LF→CRLF 提示，没有新增 whitespace error。
- `Tools/Validate-ChangeLedger.ps1` 通过：106 条 Record，当前 9 个 governed code 文件均由
  MAPCFG-005 或其历史记录覆盖。输出中的其他 Record 警告来自既有 dirty worktree 的未同时变更
  文件，不是 MAPCFG-005 覆盖错误。
- 当前 Unity Editor 的实际编译日志在修复前发现 `BattleMapBoundaryDefinition.cs` 的 4 个
  `CS0122`（外层类直接写入嵌套几何类私有序列化字段）；该错误已通过构造函数接收 deep-copy
  结果的最小改动修复。
- 修复后复用 Unity 自己生成的 `Assembly-CSharp.rsp`、宏和源码集合运行 Roslyn 交叉编译，命令
  `D:\Unity\HubEditor\2022.3.62f3\Editor\Data\NetCoreRuntime\dotnet.exe exec ...\csc.dll`
  输出到 `Temp/MAPCFG-005-compile/Assembly-CSharp-after-fix.dll`，退出码为 `0`；仅保留工程
  既有 nullable/unused-field warnings。该结果是修复后的源代码交叉编译证据，不替代 Unity
  Editor 正式程序集证据。
- 当前 Unity Editor PID `37088` 与 `Library/EditorInstance.json` 一致，但
  `Library/ScriptAssemblies/Assembly-CSharp.dll` / `Assembly-CSharp-Editor.dll` 仍为
  2026-08-25 17:24:38/17:24:39，尚未得到修复后的 Unity 正式程序集；因此不能把
  `Temp/NTSD_BattleRuntimeSelfCheck.result = PASS`（22:52:28，旧程序集）计为 MAPCFG-005
  self-check。
- 临时 Unity 工程交叉编译命令
  `dotnet build .\MAPCFG-005-runtime-compile.proj --no-restore -p:Configuration=Debug -p:BuildProjectReferences=false -m:1`
  未进入项目源代码编译，因生成工程依赖的 `Temp\bin\Debug` firstpass/package DLL 不存在而
  以 CS0006 结束；该结果不能证明代码通过或失败。临时 wrapper 已移除。
- 另一次 Editor rsp 交叉编译尝试额外同时引用了旧正式 `Assembly-CSharp` 与临时修复程序集，
  因此产生 `CS0433` 重复类型；该验证命令的引用组合无效，未作为 MAPCFG-005 源码或测试结论，
  也未覆盖 Unity 正式程序集。

### 仍待验证

- 当前 Unity Editor 刷新脚本后取得新的 `Assembly-CSharp*.dll` 和 0 C# error；
- MAPCFG-001～005 focused Editor tests，至少覆盖当前四个地图测试类；
- 使用新程序集重新运行 `BattleRuntimeSelfCheck`；
- 真实 `NTSD_Battle` Scene 的 Catalog/MapId/BoundaryWallManager/Bg Renderer Inspector 接线与
  Play Mode 行为。若不具备这些引用或人工验证条件，状态保持 `RUNTIME_PENDING`。
