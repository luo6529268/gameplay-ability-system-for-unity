# Q07/D-024 共用地图原点与出生坐标域合同

Status: `STATIC_BOUNDARY_MEASURED / SOURCE_AND_VIEW_ORIGIN_FROZEN / PRODUCTION_AND_PLAY_PENDING`. 本报告只读正式源码、项目地图与当前 Scene；没有写 DAT、地图、Scene、相机或战斗脚本。用户 2026-09-29 选择保留完整背景、按视口比例调整战斗实体实际距离及原 DAT 局部几何，并要求统一换算入口。正式根 EXE SHA-256 本轮复核为 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`。

## 当前地图与相机的实测静态配置

- `NTSD_Battle.unity` 的 `BattleBootstrap.mapId=Sunagakure`、`mapCatalog` 非空，`boundaryManager` 指向本 Scene 的边界组件；`BattleBootstrap.TryPrepareMapConfiguration` 调用 `BoundaryWallManager.TryLoadBoundaryDefinition`。该 Scene 本轮期间已有**写入来源未归属**的 UI Text 删除（SHA-256 `3A089236...ED`），本报告不编辑/回退它；真实 Play 时仍须核对 map 是否成功准备。
- `SunagakureMap.asset` 当前 SHA-256 `F7B5E4A44CAC05480D1CA6F67ABF623531264C1C96725D7FDD23DA50C8E60C08`；多边形世界 X 最小 `-11.997465`、最大 `8.405828`，宽 `20.403293` 世界单位，即 `2040.3293` 项目像素。`BoundaryWallManager.TryGetBattleStageRuntime` 对宽度取整，静态预期 stage 宽约 `2040`，不是 `GameConfig.BattleStageWidthPx=800` 的无边界回退值。运行态仍须读取实际加载多边形确认。
- Scene 的 `ScenesCamera` 序列化启用，正交 size `5.7599998`、中心 X `-1.79`；16:9 下左右约 `-12.03..8.45` 世界单位，宽约 `20.48` 世界单位即 `2048` 像素。`NTSDRenderSpace.GetViewport` 有边界时以当前可走区域的 `xMin` 为战斗像素 X=0，因此静态地图左边界与画面左边缘仅约 3.25 物理像素差。相机固定完整背景例外保持。
- `GameConfig.asset` 的参考视口为 `2048×1152`；统一入口 `BattleSpatialProjection` 的正式视口为 `1333×730`。`rX=2048/1333=1.536384096...`，`rZ=1152/730=1.578082192...`。视口比例不等于 `NTSDRenderSpace.BattleVisualScale=1.5` 的图片放大倍率，不得二次相乘。

## 生产接线必须遵循的坐标域

1. **全战斗共用原点**：以当前地图/展示空间的战斗像素 X=0、地面 Z=0 为 physical 原点，以正式规则坐标 X=0、Z=0 为 source 原点。使用 `BattleSpatialProjection.SourceToViewX/Z(source, 0)` 和逆换算；不得按每个实体的出生位置临时选锚点。若活动地图更换，像素原点由 `NTSDRenderSpace`/边界快照统一确定，不从角色 Transform 推断。
2. **项目地图中随机选出的实际点**：保持物理位置在可走多边形内，由 `ViewToSourceX/Z` 初始化 `SourceRuleX/Z`；不得把同一物理数直接写进 source carrier。随机抽样范围与 RNG 顺序另需正式/项目地图例外对照，不能仅因倒算成功宣称同 seed 原版出生相同。
3. **正式 DAT/规则提供的 source 点**：通过 `SourceToViewX/Z` 得出物理中心，源值原样进入 source carrier；`StageSpawnTaskConfigurator`、直接 spawn、reserve/queued join、OPoint 与复活须按输入来源归类，不能统一把所有 `spawnX` 解释为 source 或 physical。现有 OPoint 相对位移已乘统一 World 倍率，应避免再次乘。
4. **碰撞局部几何**：正式 `CollisionGeometry28::project` 在源整数位置上组合 `centerx/centery`、ITR/BDY x/y/w/h 与朝向，然后按严格 `<` 判断 XY。Unity 同一对中心在 physical 域时，双方 X 局部边界与 zwidth/z-offset 必须走同一投影；RoleAware、brute/fallback、粗筛、精确和即时 AABB 均调用共享投影。原 DAT 整数绝不改写。特殊 `itr.y=int.MinValue` 的源整数溢出/整高语义单独保留，不能盲做浮点乘法。
5. **边界双域**：项目可走多边形和 10 秒非角色出区计时继续使用 physical 地面像素；正式规则中读 stage width/Z 范围及边缘阈值的 source 消费者需用投影逆变换的 source 边界。按当前静态地图 X 宽度，source 等价值约 `2040.3293/rX=1328.0073`；仍须按运行态 `BoundaryWallManager` 实测和各消费者整数规则决定取整，不得直接改 `BaseStageWidthPx`，因为它也被物理出生和地图约束读取。

同源示例：正式 Han 在源 X500、候选前 X535，Lee 源近 X520、远 X580。共用原点下 Unity 对应初始 X 约 `768.192/798.920/891.103`，Han 候选前约 `821.966`。正式 ITR `[536,561)`、近 BDY `[502,545)`、远 BDY `[562,605)` 投影后，近重叠 `9*rX=13.8275`、远间隙 `1*rX=1.5364`。此前 Unity 原位 Lee X520/580 是未映射间距，不能作为 GREEN。整数位置/局部边界舍入必须在生产查询和真实 Play 里证明近抓远不抓；这里的连续值演算不是最终碰撞结果。

待实现前的精确出口：先建立**出生坐标来源分类与 source/physical stage bounds** 的 Task/Change；再建立**所有共享碰撞矩形及 Z/即时查询**的 Task/Change。第一包至少覆盖 `AppManager.InitializeBattleParticipants`/`SyncParticipantBirthPosition`、`SimulationStageWaveModule` 两条 stage spawn 路线与 `StageSpawnTaskConfigurator`，核对 `LF2Entity.ApplyInitialRuntimePosition` 与 OPoint/复活相对位移是否已单次投影。第二包至少覆盖 `BruteForceSceneQuery.LocalRectWorldRect`、`ItrWorldRectExeRaw`、所有 zHalf/深度比较与 RoleAware 缓存和 AABB 消费者。两包必须同一套 `BattleSpatialProjection` API，不能各写比率常量；项目地图与相机不改。

验收最窄矩阵：原Editor原项目脚本编译；ratio1 不变；映射后 Han/Lee 近/远、左/右镜像、非角色、负数与整数边界、Z 半径/offset、特殊 raw ITR、RoleAware/brute/fallback 同候选、地图出区 10 秒控制；随后原 Battle Scene 真实输入 Play 与正式根 EXE/对应 playable source 同条件核验。当前只冻结静态原点与输入域；Y 高度、完整 source stage 边界消费者、运行态地图值和所有 Play 结果未验，Q07/BATCH-04、D-024及总目标开放。

## 2026-09-29 出生调用方补充分类

- `AppManager.SetupBattleCharacters` 在 `SimulationTickDriver.ApplyMatchConfig` 建好且配置好 World 比例、`BattleBootstrap.TryPrepareMapConfiguration` 刷新项目地图边界后，按 `BaseStageWidthPx/ZMin/ZMax` 抽样物理出生点。`NTSD28-Q07-D024-MAP-PHYSICAL-PARTICIPANT-BIRTH-001` 已仅在正式菜单路径把它逆映射进 source carrier；原 `SyncParticipantBirthPosition` 的多处 raw 探针调用保持不变。原Editor编译和本类EditMode 4/4已过，真实菜单Play待。
- `BattleStageCampaignLoader.LoadFromFile` 读取 `MatchConfig.stageCampaignFilePath` 或缺省 `Assets/StreamingAssets/NTSD/data/stage.dat`；该缺省文件在当前项目不存在，符合用户暂缓默认 stage.dat 部署。显式文件经解密/parser 把 `<phase>` 条目 `x` 作为整数 `BattleStageSpawnData.X`；`BattleStageCampaignValueAdapter` 原样传到 `SimulationStageWaveModule`。正式版 stage.dat 的条目应视为源规则作者坐标，**但任意项目自备的显式路径来源未核，不能无条件宣称它也在源域**；没有显式文件时生产波次禁用。后续实现先核输入文件身份与域，不得借此私自补默认 DAT。
- `SimulationStageWaveModule.SpawnStageImmediateEntrySlot` 在 `spawn.X` 加0..299或选源边界外 -150..-450 / `stageBound+150..450`，并依据 `stageBound-794` 决定朝向。`TrySpawnResultsReserveEntry` 从 stage宽度产生角色 `-100/width+100`、其他对象 `50/width-50`。这两条当前把同一个 `spawnX/Z` 同时送给 `StageSpawnTaskConfigurator.task.pos/sourceRuleX/Z`、`initialRuntimeX/Z`、直接构造、最终 `entity.SetPos`。应先在 source 域完成规则分支/RNG/朝向，再唯一前映射到 physical 并显式保留原 source 整数；不能简单修改其中一次赋值，否则 factory/direct 与尾部再次赋值会重叠或反向覆盖。
- `LF2Entity.ApplyInitialRuntimePosition` 按 `task.useSourceRulePosition` 和 `useInitialRuntimeIntPosition` 独立接收 source 与 physical，并不自动做倍率；它可以承载一对分域坐标。OPoint 相对位移生成器目前已经按 `World.FixedViewRunDistanceScale` 处理物理偏移，且另存 source 偏移；Stage 配置器与相对 OPoint 不是同一输入域，不得叠乘。
- `BattleRandomWeaponDropModule` 普通/模式2两条路径也从当前 **项目地图物理** stage 宽/Z 范围选实际点，却令 `sourceRuleX/Z=physicalX/Z`。它与菜单出生同属逆映射类，仍待单独生产/聚焦包。`BattleEcsCharacterPreFrameBoundsPass`、Legacy stage render pass、random weapon 以及 stage wave 都读取同一 `BaseStageWidthPx/ZMin/ZMax`，当前这些值由项目地图提供给 physical 边界；后续应在统一投影入口或独立双域边界快照中明确每个读者的域，不能全局把原字段除以倍率。

这份分类仅定位剩余接线，不将 stage、武器或碰撞标成已修复；Q07与总目标继续开放。

## 2026-09-29 随机武器出生补证

上段“随机武器仍待”是本报告写成时的状态，已由 [`NTSD28-Q07-D024-RANDOM-WEAPON-PHYSICAL-BIRTH-PROJECTION-001`](../../../docs/ai/CHANGE-RECORDS/NTSD28-Q07-D024-RANDOM-WEAPON-PHYSICAL-BIRTH-PROJECTION-001.md) 覆盖：普通和模式2地图物理出生点均经 World 的同一 `BattleSpatialProjection` 逆投影到 source；模式2 source +1Z 的实际增量由 `SourceDeltaToViewZ(1)` 给出。原 Editor 单类 GREEN job `69e32163d83b464baf20cb3ab7168986` 5/5，生成 Editor 编译0错误，保护文件哈希不变。该结果只关闭此出生换算的聚焦检查；自然 Play、显式 stage 输入域与共享碰撞几何仍待，Q07不关闭。
