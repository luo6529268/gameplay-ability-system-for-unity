# D-025 非角色可行走区域延时清除合同

状态：`USER_RULE_CONFIRMED / FOCUSED_TEST_PASS / RUNTIME_PENDING`，2026-09-24。此合同承接 D-024，正式 EXE 的非角色即时 X 边界规则在用户明确例外范围内不再作为 Unity 清除时机；DAT 不修改。正式版其他战斗规则仍按当前 release live path 裁决。下方“已核对的现状”是实施前快照；当前代码/验证见 Task 与 Change Record。

## 已核对的现状

- 当前 `NTSD_Battle.unity` 的 `BattleBootstrap` 选择 `BattleMapCatalog` 的 `Sunagakure`，地图边界来自 `Assets/NTSD/Map/SunagakureMap.asset`，经 `BattleBootstrap.TryPrepareMapConfiguration` 加载到 `BoundaryWallManager`。`BoundaryWallManager.IsPointWalkable` 以世界 X/Y 地面点检测多边形并集；无有效边界时返回 true。`NTSDRenderSpace.GroundPixelToWorld` 提供战斗 X/Z 到世界地面 X/Y 的映射。
- 生产 `PreFrameBounds` 通过 DataOriented pass 或 Legacy path 调用 `LF2Entity.ApplyPreFrameXBounds`。该方法目前对 type3 和其他非角色对象做即时 X 清除，OID122/123 另有位置夹取；目前只对普通接地物体检查 YInt。原项目 Editor 已在 width800/factor1 下证明普通 OID150 X50 和受保护 OID122 X50/X790 的原版期望首差，详 `NTSD28-USER-D024-NONCHAR-X-BOUNDARY-WITNESS-001`。这些即时清除与用户新规则冲突，不能直接把旧阈值乘比例。
- `SimulationTickDriver` 默认可把核心 tick 放在 dedicated worker。主线程在提交 worker 前准备 stage snapshot；在 worker 中直接查询 `BoundaryWallManager`、`Transform` 或相机不安全。可行走几何必须在地图准备/host 阶段冻结成不引用 Unity 对象的纯数据，并在当前地图退出时失效。PreFrame 读取实体战斗 runtime X/Z，不读取 Renderer/Transform。
- `NTSDEntityRuntime` 按实体复用；`TryCopyCanonicalStateTo`、`Reset`、`BattleWorldEntityRuntimeSnapshotBuffer` 和 `BattleLockstepChecksumModule` 是新增连续离区计时的保存/恢复/校验路径。若仅在 MonoBehaviour 用 `Time.time` 或在 world 临时字典计时，会让快进、回放和池复用得到不同清除 tick。

## 实施顺序与出口

1. 定义纯数据多边形快照。地图加载成功后在主线程把每个启用可行走 polygon 的世界顶点转成战斗地面像素 X/Z，保持与 `BoundaryWallManager.IsPointWalkable` 相同的并集、边界含入及凹多边形语义。没有有效多边形时显式表示 `Unavailable`，不得触发清除。避免每 tick 分配或 Unity API 查询。只读查询可由 worker 使用。验证正式 Sunagakure 内/边/外点及无边界回退。
2. 在非角色 PreFrame 统一执行离区计时：区域内清零；首次离区记录当前逻辑 tick；连续离区到至少 304 个 33 ms 逻辑步后使用现有 `FreeEntityLikeExe` 生命周期入口。tick 计数在同 tick 重复调用时幂等；F5 仍按逻辑 tick；Y 高度不改变地面离区判定。角色不进入该规则；type3、武器、其他非角色均覆盖。保留与清除无关的独立状态/位置规则，但旧即时离场分支必须被 D-025 替代。
3. 新计时字段必须在出生/池复用重置，按当前版本合同进入实体 snapshot/restore 与 checksum。新的地图快照只在战斗配置/地图边界建立后发布；worker Join 后可释放，旧 World/地图不得泄漏到下一战。记录 schema 变更与旧快照不兼容性，不借机改其他 payload。
4. 先跑定向 RED/GREEN：正常 303/304 tick 边界、离区后重入清零、二次离区重计、不同非角色类别、角色不清除、无边界不误删、F5 同 tick、池复用、snapshot/restore/checksum、DataOriented/Legacy。随后在原项目既有 Editor 上做编译、定向 self-check、真实 Battle Scene Play；不启动第二项目，不保存用户 Scene。DAT、图片、相机、非战斗功能及有序关闭主序列不改。

回滚：按独立 Task/Change 的准确脚本清单撤回 D-025 新载体、边界快照和接线，保留用户/其他任务修改与原始资源。旧 D-024 静态/RED 证据保留为决策前历史，不因新规则删除。
