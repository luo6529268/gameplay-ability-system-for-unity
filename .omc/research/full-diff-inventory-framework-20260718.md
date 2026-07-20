# Framework / Tick / Bootstrap 全量差异盘点（2026-07-18）

## 修复后核销（2026-07-18 16:46，覆盖本报告原始只读状态）

本报告下方“未修复”分类是冻结清单生成时的历史状态。当前 code-only 核销如下：

| ID | 当前状态 | 证据 |
|---|---|---|
| `FW-FLOW-01` | **代码已修 / fresh self-check verified** | `NTSDBattleTickSystem.RunReleaseTick` 已按 authority 先 cooldown 后 human input；`CheckFrameworkCooldownBeforeHumanInputOrder` |
| `FW-RESULT-01` | **代码已修 / fresh self-check verified** | fixed slot、inactive-active、dormant 与 zero-HP results 矩阵由 `CheckBattleResultsSlotAndRelationContracts` 覆盖 |
| `FW-FLOW-02` | **dormant / scope-excluded** | Unity 生产无 `BattleStepMode` writer；authority writer 仅 Host debug/step 控制 |
| `FW-BOOT-01` | **历史误报 / rematch scope-excluded** | authority `Unk344/HolderCopy` 写入位于 `resultStartRematch` 条件内，不是普通 bootstrap 契约 |
| `FW-BOOT-02` | **普通路径 equivalent；rematch scope-excluded** | reset 后 `HpMax/Hp3=500` 使 difficulty bonus clamp 回 500；普通 PP/respawn/input/Cd/速度字段与 Unity 等价，`PP=200` 仅 rematch |
| `FW-RESET-01` / `DEP.RNG.01` | **Unity-adapter / policy closed for current scope** | LCG 算法一致；保留 per-`SimulationWorld` lockstep owner/reset 边界 |

fresh 证据：source `16:44:31.210` < Unity DLL `16:45:52.868` < full self-check `16:46:29.080` **PASS**；`dotnet build` **0 errors / 18 warnings**。4 组 Play Mode 场景仍由用户验证，本核销不构成完整逐帧 certificate。

> 本报告是只读审计结果。本轮没有修改生产代码，也没有把静态等价推断当作运行时验收。唯一权威为 `J:\QQFile\NTSD2.4\ntsd_release_C#`；DAT 文件表示差异不列为问题，T8 默认 `stage.dat` 部署继续暂缓。

## 审计范围与分类

覆盖权威 `GameTick`、`SimulationTickDriver`、`SimulationWorld`、`DirectBattleBootstrap` 的战斗主循环、固定 tick、bootstrap、roster、stage wave/spawn、results、边界和 camera；对应 Unity `NTSDBattleTickSystem`、`SimulationTickDriver`、`SimulationWorld.*`、`AppManager`。

分类含义：

- **confirmed-difference**：权威与 Unity 的可达顺序、字段或结果契约已明确不同，尚未修复。
- **Unity-adapter**：框架实现不同，但当前设计允许；仍需验证没有改变 runtime 真值或可观察结果。
- **equivalent / closed**：当前生产代码与权威调用链一致，已有 focused self-check 或静态证据；不代表 Play Mode 全量通过。
- **authority-unresolved**：需要继续追踪权威调用者或真实场景证据，暂不判定为差异。
- **scope-excluded**：结果菜单、主机 rematch 等不属于当前战斗模拟范围。

## BA8 F1-F7 当前核销

| ID | 权威证据 | Unity证据 | 当前结论 | 证据等级 |
|---|---|---|---|---|
| F1 初始 wave | `BattleCore/Simulation/GameTick.cs:132-133,2317+` 只在正式 tick 的 stage pass 推进；`DirectBattleBootstrap.InitializeFromConfig` 保持 `WaveIdx=-1`。 | `SimulationTickDriver.ApplyMatchConfig` (`Assets/NTSD/Scripts/Simulation/SimulationTickDriver.cs:263-287`) 不调用 `StartInitialStageWave`；`CurrentWaveStageTickAll` (`SimulationWorld.StageWave.partial.cs:15-18`) 在 tick pass 推进。 | **closed in source**；需 bootstrap Play Mode 复核首 tick 可见边界。 | 静态 + focused self-check |
| F2 8-slot roster/team | `DirectBattleBootstrap.cs:80-109` 始终遍历 8 个 slot；`BattleTeamFromSlotTeam` 将 independent team 映射到 `10 + slotIndex`。 | `BattleRosterRuntimeState.ApplyMatchConfig` 保留 8 槽并按原始 index 解析 team/input；`SetupBattleCharacters` 按原始列表 index 绑定。 | **closed in source**；配置列表长度/空洞仍需真实场景覆盖。 | 静态 + focused self-check |
| F3 初始出生 X/Z 与 RNG | `DirectBattleBootstrap.cs:108-137` 使用 Bg width/z bounds 和 `NtsdRng.Rand()`。 | `AppManager.SetupBattleCharacters` (`Assets/NTSD/Scripts/App/AppManager.cs:203-219`) 使用 runtime stage bounds 和 `DeterministicRng.NextRaw()`。 | **closed in source**；Unity stage snapshot 必须与权威 Bg 语义一致。 | 静态 |
| F4 初始 prime | `DirectBattleBootstrap.cs:224-244` 设置 HP/PP、`HitStop=75`、`Vx=Vz=.1`、`Vy=0`、输入和 Cd。 | `AppManager.cs:224-235` 设置 HP/PP 初值、速度和 `HitStun=75`，并同步整数位置。 | **mostly closed**；identity/难度统计契约见 FW-04/FW-05。 | 静态 + focused self-check |
| F5 stage spawn rest | 权威 `SpawnStageImmediateEntrySlot` (`GameTick.cs:2070+`) 不经通用注册清除 rest。 | Unity stage spawn 使用 `ReleaseSpawnSemantic.StageSpawnAt`，注册时跳过 cooldown reset，并在 `StageWave.partial.cs:492` 恢复 raw rest。 | **closed in source**；需 stage fixture/Play Mode 复核复用槽。 | 静态 + focused self-check |
| F6 results early return | `GameTick.cs:42-50` results active 只执行 results input/flow 后返回。 | `NTSDBattleTickSystem.RunReleaseTick` (`Assets/NTSD/Scripts/Simulation/NTSDBattleTickSystem.cs:24-30`) results active 只做 human poll + `BattleResultsFlow` 后返回。 | **closed in source**；results 输入菜单本身不在本轮范围。 | 静态 + focused self-check |
| F7 carrier tail | `GameTick.cs:1897+` 在当前 tick post-frame tail 清 hit candidate carrier。 | `SimulationWorld.EntityPostFrameTailAll` (`SimulationWorld.Passes.partial.cs:859-895`) 当前 tick 清 carrier，并重置 transient MP。 | **closed in source**；需与对象销毁/复用 trace 一起回归。 | 静态 + focused self-check |

## 新发现的开放 Framework 差异

### FW-FLOW-01：普通 tick 的 cooldown 与 human input 顺序反转

- 权威：`GameTick.Run` (`J:\QQFile\NTSD2.4\ntsd_release_C#\src\BattleCore\Simulation\GameTick.cs:53-67`) 先 `RunCooldownsTick`，处理 step gate，再调用 `postCooldownInput`，随后才进入 oid maintenance/clear-input。
- Unity：`NTSDBattleTickSystem.RunReleaseTick` (`Assets/NTSD/Scripts/Simulation/NTSDBattleTickSystem.cs:32-43`) 先 `PostCooldownHumanInput`，`RunFrameAdvancePhase` 内第 1 步才调用 `VrestTickAll` (`:86-90`)。
- 触发条件：普通非-results tick，尤其输入边沿与 `ARest`/`AttackExempt` 同时到期的帧。
- 预期：输入读取发生在本 tick cooldown 递减之后。
- 实际：Unity 输入读取发生在 cooldown 递减之前；`Vrest` 值和攻击豁免清理可能被输入/角色逻辑提前观察。
- 分类：**confirmed-difference / 未修复 / 未运行时验证**。
- 依赖：需要 focused self-check 覆盖 `ARest`、`AttackExempt`、输入边沿同 tick 组合。

### FW-FLOW-02：BattleStep gate 处理缺失

- 权威：`GameTick.cs:56-67` 每 tick 将 `BattleStepGate44905C` 清零；`BattleStepMode==2` 时置 gate=1、mode=1；`postCooldownInput` 在 `IsStepWaitGate` 时不执行。
- Unity：`BattleFlowRuntimeState` 只有 `BattleStepMode/BattleStepGate` 字段；`AdvanceBattleFlowTick` (`SimulationWorld.Registry.partial.cs:272-281`) 只更新 tick/input 相位；`NTSDBattleTickSystem.RunReleaseTick` 无对应 mode=2 转换或 step-wait 抑制。
- 触发条件：设置 `BattleStepMode=1/2` 的单步/慢速战斗路径。
- 预期：step gate 控制 cooldown/input 与尾部早退时序。
- 实际：Unity 仍无条件执行 `PostCooldownHumanInput`，也没有权威 `BattleStepGate` 转换。
- 分类：**confirmed-difference / 未修复 / 未运行时验证**。
- 依赖：需确认单步模式是否属于当前生产战斗入口；若可达则为正式 backlog。

### FW-BOOT-01：初始玩家 identity 字段契约不完整

- 权威：`DirectBattleBootstrap.cs:138-140` 对每个 active player 写 `entity.Unk344=battleTeam`、`entity.HolderCopy=si`。
- Unity：`AppManager.SetupBattleCharacters` (`Assets/NTSD/Scripts/App/AppManager.cs:224-235`) 只写 `Team`、`RelationTeam`、`AiControlled`、位置/速度和 `HitStun`；`Unk344` 保持初始化默认值，`HolderCopySlot` 保持 `LF2Character.Initialize` 的默认 `99`。
- 触发条件：初始玩家参与击杀/伤害统计、holder-copy、落地伤害或依赖 `Unk344` 的技能/AI 分支。
- 预期：`Unk344` 与 battle team 一致，`HolderCopy` 指向原始 roster slot。
- 实际：Unity 初始实体可能为 `Unk344=0`、`HolderCopySlot=99`。
- 分类：**confirmed-difference / 未修复 / 未运行时验证**。
- 依赖：补齐字段前需确认 `Unk344` 在 Unity runtime 的所有读写方和 reset 语义。

### FW-BOOT-02：初始统计/难度初始化未完整迁移

- 权威：`DirectBattleBootstrap.InitializeBattleStats` (`DirectBattleBootstrap.cs:224-244`) 按 difficulty 计算 HP bonus 并 clamp 到 `Hp3`，设置 PP/PPBound、respawn、HitStop、速度、全部按键边沿和 `CdAttack/CdJump/CdDefend`。
- Unity：`AppManager.SetupBattleCharacters` (`AppManager.cs:224`) 直接调用 `lf2.Initialize(NTSDGlobal.Default.Health.HpFull, ... )`，之后只写 Team/RelationTeam、位置/速度和 HitStun (`:225-235`)；没有对应 difficulty HP bonus/cap、respawn 与完整 cooldown/edge 字段显式写入。
- 触发条件：非默认 difficulty、DAT `Hp3`/HP 不等于全局默认值、重用对象或初始输入边沿非零时。
- 预期：与权威 `InitializeBattleStats` 的字段集合、顺序和数值一致。
- 实际：Unity 依赖 `Initialize`/pool reset 的隐式默认值，尚未证明所有字段契约等价。
- 分类：**confirmed-difference（字段契约缺失；部分数值可能偶合等价）/ 未修复 / 未运行时验证**。
- 依赖：需要按 difficulty、DAT HP3、pooled/rebootstrap 三组 fixture 验证。

### FW-RESET-01：world reset 的 RNG 生命周期不同

- 权威：`SimulationWorld.ResetBattleRuntime` (`BattleCore/Simulation/SimulationWorld.Passes.cs:13-70`) 清 runtime/tick/实体/slot/rest，但未调用 `NtsdRng.Srand`；`NtsdRng` 状态由进程级静态 RNG 延续。
- Unity：`SimulationWorld.ResetRuntimeState` (`Assets/NTSD/Scripts/Simulation/SimulationWorld.Registry.partial.cs:138-151`) 每次 reset 调 `Rng.Seed(0x4E545344u)`；随后 `SimulationTickDriver.ApplyMatchConfig` (`SimulationTickDriver.cs:268-281`) 再按 `config.seed` 重置。
- 触发条件：同一进程连续重开/重赛、reset 后发生随机掉落或 stage spawn。
- 预期：若权威未显式重播 seed，后续 RNG 序列延续静态状态；若上层 bootstrap 另行 Srand，则应在同一入口明确对齐。
- 实际：Unity 每次 world reset 都重置 RNG，且 config seed 可能引入权威不存在的重播边界。
- 分类：**confirmed-difference / 未修复 / 未运行时验证**；上层是否应显式播种仍需 authority-unresolved 核销。

## Stage / boundary / camera 映射

| ID | 权威 | Unity | 分类与结论 |
|---|---|---|---|
| FW-STAGE-01 | `GameTick.ApplyPreframeBounds` (`GameTick.cs:1301-1397`) 使用 `world.Bg.Width/ZBoundaryMin/ZBoundaryMax`；角色、type3、普通对象有不同 X/Z/free 规则。 | `SimulationWorld.StageRender.partial.cs:70-110` 从 `GameConfig`/`BoundaryWallManager` scene snapshot 取 bounds；实体边界由 `LF2Entity.ApplyPreFrame*` 分派。 | **Unity-adapter**。允许 scene-native 输入，但必须证明 snapshot 数值与权威 Bg 语义一致；若 `BoundaryWallManager.IsPointWalkable` 参与移动，则该 polygon 是额外可达规则，当前仍是 confirmed difference（旧记录）。 |
| FW-STAGE-02 | `ApplyCurrentWavePhaseAdvance` 与 `ApplyCurrentWaveImmediateStageSpawns` 在同一 tick、preframe bounds 后按固定顺序执行（`GameTick.cs:128-133,2317-2400`）。 | `CurrentWaveStageTickAll` (`StageWave.partial.cs:15-18`) 同样先 advance 后 immediate；stage spawn 在汇合点恢复 rest (`:492`)。 | **equivalent / closed in source**；仍需 real fixture/Play Mode。 |
| FW-CAM-01 | `UpdateCameraAndBgAnimation` (`GameTick.cs:1397-1473`) 每 tick 推进 CameraX/CameraVel，并更新 Bg layer AnimCounter。 | `ResetUnityFixedWorldRenderOffsets` (`StageRender.partial.cs:112-137`) 每 tick 强制 camera/render offset=0。 | **Unity-adapter / user-approved**：用户明确要求 fixed-world camera，不作为待修复战斗差异；不得把 camera offset 写回 runtime X/Z。背景动画若参与战斗规则需另行确认。 |
| FW-RESULT-01 | `UpdateBattleResultsFlow` 以 8 个 `BattleSlotEntity` 和 `Unk364` 统计 active/alive，results active 后进入 `RunResultsTick`。 | `SimulationWorld.UpdateBattleResultsFlow` (`StageRender.partial.cs:174+`) 以 roster runtime slot、`RelationTeam` fallback 和 `IsActiveForCurrentPass` 统计；results active 的普通战斗 pass 已早退。 | **confirmed code difference / 未修复**：正常初始 roster 可等价；slot inactive、dormant entity、relation identity 变化时选择规则与 authority 不同。results 菜单/host rematch 属于 scope-excluded。 |

## 其它 Framework 适配项（当前未判定为正式差异）

| ID | 观察 | 分类 |
|---|---|---|
| FW-ADAPT-01 | Unity 用 `_buckets` + runtime slot 遍历，权威用固定 `Objects[]` index 遍历；注册/复用时需要保持 slot 分配、stable id 和副作用顺序。 | Unity-adapter；若动态注册在同 tick 改变顺序，应升级为 confirmed difference。 |
| FW-ADAPT-02 | Unity 使用 pooled `LF2Entity`/`GameObject` factory，权威 `Spawn`/`SpawnAt` 直接填充固定实体槽。 | Unity-adapter；必须完整重置 runtime/rest/identity/presentation，stage spawn 当前已有专用 contract。 |
| FW-ADAPT-03 | Unity `SimulationTickDriver.Update/LateUpdate` 用 30Hz accumulator 驱动逻辑、LateUpdate 刷表现；权威 driver 由调用者显式 `StepOneTick`。 | Unity-adapter；逻辑 tick 内禁止使用 Unity deltaTime，当前代码满足该边界。 |
| FW-ADAPT-04 | Unity `ApplyMatchConfig` 从 scene/config 取得 background/stage campaign；权威 `RuntimeBootstrap` 读取 DAT/bg/stage.dat。 | Unity-adapter；raw DAT/manifest 差异不处理，T8 默认资产部署暂缓。 |
| FW-ADAPT-05 | Unity `RenderDispatchAll` 调用 renderer late tick；权威 `prePostprocessRender` 是 host/test callback，核心逻辑不依赖渲染。 | Unity-adapter；表现层不得写回 runtime 真值。 |

## 汇总

- BA8 F1-F7：**7 项生产路径已闭合（source/focused self-check），尚缺对应 bootstrap/stage Play Mode 证据**。
- 本轮新增正式开放 Framework 差异：**FW-FLOW-01、FW-FLOW-02、FW-BOOT-01、FW-BOOT-02、FW-RESET-01，共 5 项**。其中 FW-BOOT-02 的部分默认值可能偶合等价，仍按字段契约缺失记录，不直接推断所有对局均不同。
- 既有 `BoundaryWallManager` walkability 与权威 Bg bounds 的差异继续保留为 stage/physics 交接项；fixed-world camera 是用户批准的 Unity 适配。
- 本报告没有修改生产代码，也没有宣称完整战斗逻辑对齐。后续应先把本报告项并入两份主文档，冻结总差异清单，再按文档顺序修复并重新做编译、self-check 和定向 Play Mode 验收。
