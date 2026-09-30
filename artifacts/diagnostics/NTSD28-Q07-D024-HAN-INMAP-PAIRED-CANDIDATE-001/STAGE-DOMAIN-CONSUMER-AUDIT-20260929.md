# Q07/D-024 项目舞台 Z 双域首轮消费者审计

状态：`READ_ONLY_INVENTORY / PRODUCTION_DECISION_PENDING`。本文件只审当前源码和已完成的原 Battle Play，不修改项目背景、原版背景或模式 DAT、正式 EXE、Unity 生产代码、相机、场景及 GameConfig。

**已测量冲突。** 原 Unity Battle Scene 的项目可走外包范围给出物理 Z237..760；World 当前深度倍率 `1152/730≈1.57808219`，零锚点逆映射成源 Z≈150.18..481.60。正式背景 ID23 在正式发行 runtime 中给出源舞台 Z542..712。正式 X500/Z400 的输入第1 tick 被钳到542，Unity X500/Z400 映射为物理 Z631.23并在项目多边形内。两端的合法源区间不交。因此“双方同绝对源Z且各自在地图内”的出口对这组背景/项目地图不可达；本项目使用自己的背景和可走区域，本来就应把舞台边界作为已声明的内容例外处理。相对碰撞近1/远0是有效的限定控制，但不能代替舞台域验收。

**2026-09-29 背景范围纠正：** 上述“不交”仅指正式 **ID23/Hos** 对项目地图。正式 ID1/San 的源 Z375..575 与项目合法源区间在 Z375..481.60 相交；其 Z400 近/远根 EXE LFR 已实跑，双方首次 action146 候选1/0及源X/Z同值。见[San限定验收](../NTSD28-Q07-D024-SAN-SAME-Z-ROOT-CANDIDATE-001/ACCEPTANCE.md)。此补证让共同绝对源Z的**碰撞候选**出口可验，不表示项目边界 source/physical 混用已经修好，也不授权用正式背景DAT配置Unity。

**可复算的首差预测，尚未当作新 Play 结果：** 对源 Z600，统一投影的实际点是 `600×1152/730≈946.8493`，越项目物理上限760；目前 `NTSDEntityRuntime.ClampStageZ` 的 source 分支拿项目物理数字237..760比较，因600在此数字区间而不发生钳制。若这对边界被明确标为项目物理域，其对应的源上界应为 `760×730/1152≈481.5972`，源钳制的实际增量应落到物理760。近侧源 Z100同理：当前以物理下界237钳源会把实际点推到约374.0055，正确投影边界是源≈150.1823、实际237。这两组数值是下一定向 RED 的预期，不能把静态演算写成已运行生产断言。

| 生产入口 | 当前值的来源/读者 | 可确认的域与风险 |
|---|---|---|
| `BoundaryWallManager.TryGetBattleStageRuntime` → `SimulationStageRenderModule.ResolveUnityStageRuntime` → `BattleStageRuntimeState.SetSceneSnapshot` | 项目多边形世界坐标转地面像素，写 `Stage.ZMin/ZMax` | **物理项目地图域**。不能整体把 `Stage.ZMin/ZMax` 改为源值，否则菜单随机落点、可走边界与快照含义一并变化。 |
| `SimulationStageRenderModule.ClampCharacterZToStageBoundsAll`、`BattleEcsCharacterStageZPass`、`BattleEcsCharacterPreFrameBoundsPass`、Legacy `RunLegacyPreFrameBoundsAll` → `NTSDEntityRuntime.ClampStageZ` | 从同一 `Stage.ZMin/ZMax` 取边界；common clamp 在有 source carrier 时同时写 source 与 physical 修正 | 现有 D-024 `STAGE-Z-VIEW-CLAMP-001` 已将**源钳制增量**按倍率投影到物理位置，但输入边界本身仍是项目物理数字；不能只改 `ClampStageZ`，否则 fast/legacy 和其它读者仍混域。 |
| `SimulationAiDecisionModule.Z`、`SimulationWorld.CaptureAiDecisionWorldState` → `AiDecisionKernel` / legacy AI | AI目标 Z 在 `UseSourceRulePosition` 下读源载体；快照 `StageZMin/Max` 原样读项目 `Stage.ZMin/ZMax`，规则还加/减源10或30 | **源位置对物理舞台边界数值**，在配置比例时会改变边缘按键决策；AI快照、SoA/legacy 两路线须一同核对，不能单改一个 if。 |
| `AppManager.InitializeBattleParticipants`、`BattleRandomWeaponDropModule` | 从 `Stage.ZMin/ZMax` 的物理范围抽出生点，后者及前者已有 `BattleSpatialProjection.ViewToSourceZ` 逆算载体 | **项目物理抽样**应保留原 RNG 与多边形策略；这两条已接统一投影，不能把 stage 快照全局改成源值。 |
| `SimulationStageWaveModule` 普通波次/结果预备队 | 用同一 stage Z 数字抽样，随后 factory/direct/`SetPos`及 `SetSourceRulePosition` 都使用抽到的同一值 | **未闭的混域出生路径**；显式 stage 规则输入域、实际出生点及最终写入顺序另判，默认 `stage.dat` 仍按用户决定暂缓部署。 |
| `BattleLateEntityLifecycleModule.AdvanceNativePostDisplayResources` → `BattleNativePostDisplayResourceWriter` state405 | 以 stage Z 数字求中心并调用 `runtime.SetPosition` | **中心复位路径待核**：该调用是否同步 source carrier、与正式 state405 在项目地图例外下如何映射，需精确用例；不能让舞台双域补丁遗漏它。 |
| `BattleSimulationWorkerBoundary`、`BattleStateSnapshotRestore`、`BattleParitySnapshot` | 复制/恢复/校验 `Stage.ZMin/ZMax` | 若引入双域边界，工作线程请求、快照恢复及 checksum 要保留一致性与可重放性；不能只加运行时临时计算不声明序列化合同。 |
| `BruteForceSceneQuery` 四处 `SpatialAabbXZ preferredRoot` | 从 `Stage.ZMin/ZMax` 给即时、formal、RoleAware 和 shadow 空间索引提供首选根盒 | 这些树的参与实体/框在**物理碰撞域**，应继续读项目物理数字；全局替换成源边界可能改变候选和 fast/fallback 行为。 |

**显式夹具与 worker 的额外约束。** `SimulationWorld.SetExplicitStageRuntimeSnapshotForTesting` → `SimulationStageRenderModule.SetExplicitStageRuntimeSnapshotForTesting` 目前直接注入整数并禁止随后从 Scene 刷新；现有 `NTSD28SourceStageDepthEditorTests` 等会在放大视口下仍以180..350作为正式**源规则**边界，验证 native clamp 与 physical 增量。此类数值不能未经标记就当成项目地图物理坐标逆投影。原 Battle Scene 的自动快照来源则是 `BoundaryWallManager` 的项目物理多边形；`GameConfig` 无地图回退还需单独确认作者单位。Worker 提交前 `SimulationTickDriver` 捕获同一 Stage 快照，`BattleSimulationStageSnapshot` 目前只传宽/Z近远/透视，没有边界来源标签。下一设计必须区分**Scene项目地图物理快照**与**显式正式源夹具**，并考虑 worker、snapshot/restore 与 replay；不能只在渲染模块藏一个主线程布尔开关然后宣称跨模式已闭。

项目 stage 深度跨度 `760−237=523` 物理像素，正式背景源跨度 `712−542=170`；若试图同时对齐两侧边缘，所需深度倍率为 `523/170≈3.0765`，与用户确认的整画面比例 `1152/730≈1.5781` 不同。故不应为这张自有地图另造实体运动倍率或更改统一投影来强行重合两套边缘；项目地图例外应通过已声明的边界域处理。

正式 playable `BattleWorld28::clamp_type0_stage_depth` (`source/ntsd28_core/src/simulation/battle_world.cpp` 8084附近) 对统一正式源位置 `precise_z` 钳制，type0无边距、其他类型±1，并同步整数镜像；`native_ai.cpp` 1313、2203附近以同一正式 `stage_bounds.z_near/far` 与实体源位置比较。Unity 的项目地图边界是用户保留的内容例外；这个正式公式用于确认**规则与单位**，不授权把原版背景 DAT 复制进 Unity。

下一可实施出口应先冻结一份**项目物理 stage 快照 + 统一投影得到的源规则边界**的双域合同，并对上表每个读者标注读取哪一个域。优先用现有原 Battle Scene 做一个靠近项目物理边界的角色/非角色出界、AI 边缘和出生点定向用例，明确取整、边距、相位及快照/worker 数据；保持 D-025 非角色离开项目可走区域10秒逻辑时间清除。只有合同和 RED 证据齐备后才建独立生产 Task/Change；Q07/BATCH-04/总目标继续开放。上述是下一步设计建议，尚非已验证的生产修复。
