# Unity 战斗框架映射总账（2026-07-18）

## 0. 范围与判定标准

本报告以 `csharp-authority-framework-ledger-20260718.md` 的 172 个唯一 FW ID 为唯一输入总账，逐项映射到 Unity 当前生产源码，并反向扫描 Unity-only 的生产可达战斗分支。没有使用旧对齐文档或历史结论代替源码证据。

状态只使用以下六种：

- `equivalent`：前置条件、分支顺序、字段读写、生命周期副作用和可观察结果均能在当前源码中闭合。
- `Unity-adapter`：因 `MonoBehaviour`、`GameObject`、对象池、渲染或 Unity 事件而采用不同承载方式，但未发现它改变逻辑时序或战斗结果。该状态不是“源码逐行相同”。
- `confirmed-difference`：当前生产源码与权威链存在可直接定位的行为矛盾；包括 Unity 适配越过逻辑边界的情况。
- `missing`：当前 Unity 生产链没有可达的等效实现。
- `authority-unresolved`：权威总账本身已明确把下游行为留给其他分区，不能在本报告中假定等价。
- `scope-excluded`：已定位权威和 Unity 边界，但属于既定排除范围（debug/菜单/宿主重赛/完整 rollback 或普通音频），不计入当前战斗核心 backlog，也不借此宣称等价。

Unity 专属类型、渲染帧和事件本身不算差异；只有它们改变正式 pass 顺序、runtime 真值、对象同 tick 可见性或最终可观察结果时，才判为 `confirmed-difference`。

主要 Unity 证据入口：

- `Assets/NTSD/Scripts/Simulation/NTSDBattleTickSystem.cs`
- `Assets/NTSD/Scripts/Simulation/SimulationTickDriver.cs`
- `Assets/NTSD/Scripts/Simulation/SimulationWorld*.cs`
- `Assets/NTSD/Scripts/Simulation/BattleRuntimeState.cs`
- `Assets/NTSD/Scripts/Simulation/BattleParitySnapshot.cs`
- `Assets/NTSD/Scripts/App/AppManager.cs`
- `Assets/NTSD/Scripts/Animation/LF2Objects/*.cs`
- `Assets/NTSD/Scripts/Animation/Character/*.cs`

## 1. Bootstrap / world 初始化映射

| FW ID | Unity 当前生产映射 | 状态 | 结论与差异证据 |
|---|---|---|---|
| FW-BS-001 | `AppManager.OnSceneLoaded/InitializeBattle`、Unity scene/UI route | Unity-adapter | Unity 以场景生命周期代替 SDL host route；仅宿主入口不同。 |
| FW-BS-002 | `SimulationTickDriver.OnSingletonAwake`、`AppManager.InitializeBattle` | confirmed-difference | Unity 分开创建 world、加载配置和生成角色；没有 `loadedChars<=0` 的同等启动拒绝，RNG 与资源 registry 顺序也不同。 |
| FW-BS-003 | `SimulationTickDriver.ApplyMatchConfig:277-280`、`DeterministicRng.Seed` | confirmed-difference | Unity 使用 `MatchConfig.seed`，且 reset 先固定 seed；没有正式环境变量优先级和系统 tick fallback。 |
| FW-BS-004 | `GameDataManager`、`CharacterAnimtorManager` DAT/资源 registry | confirmed-difference | Unity 资源载入是引擎适配，但未复现“分配后解析失败仍使 HasChar=true”的 registry 残留契约。 |
| FW-BS-005 | `AppManager.InitializeBattle/SetupBattleCharacters` | confirmed-difference | 生产入口使用 scene boundary/spawn point；没有权威 direct battle 的背景失败固定 fallback + 双人 config 链。 |
| FW-BS-006 | 无生产 rematch bootstrap | scope-excluded | 宿主 rematch/rebootstrap 已排除出当前战斗核心；不据此建立 backlog。 |
| FW-BS-007 | `MatchConfig`、`BattleRosterRuntimeState.ApplyMatchConfig` | Unity-adapter | Unity 配置对象承担 slot/team/AI 输入；不是权威便捷构造器，但属于宿主配置适配。 |
| FW-BS-008 | `SimulationTickDriver.ApplyMatchConfig:263-295` | confirmed-difference | reset/config 基本字段存在，但 Unity 随即 `StartInitialStageWave()`，把 `WaveIdx=-1` 提前改为 0；权威正式链无该首波调用。 |
| FW-BS-008-B1 | `BattleRosterRuntimeState.ApplyMatchConfig:203-226` | confirmed-difference | Unity 把启用玩家压缩到连续 `writeIndex`，并直接保存 config team；权威保持原 8-slot index 和独立队伍 fallback。 |
| FW-BS-008-B2 | `AppManager.SetupBattleCharacters:176-225` | confirmed-difference | Unity 用 scene spawn point/zero fallback，不消耗权威出生 RNG；无效 OID/空池的失败边界也不是权威 `HasChar/Spawn` 跳过。 |
| FW-BS-008-B3 | 无生产 rematch entity prime | scope-excluded | 仅属于宿主 rematch重建路径；当前范围不核销该路径的 entity prime。 |
| FW-BS-008-B4 | `Register`、`LF2Entity.RefreshRuntimeSnapshot` | Unity-adapter | Unity 注册时刷新实体 runtime 快照，不需要 legacy/world `CharacterSync` 双镜像。 |
| FW-BS-009 | `LF2Character.Initialize` 与 `AppManager.SetupBattleCharacters` | confirmed-difference | 未找到 difficulty HP bonus、`HitStop=75`、`Vx=Vz=.1`、RespawnCount 和三组输入 cooldown 的完整生产初始化。 |
| FW-BS-010 | `ModuleBind/Initialize`、scene position 写入 | Unity-adapter | frame 0 与位置 prime 由 Unity entity 初始化和 scene 坐标转换承担；承载方式不同。 |
| FW-BS-011 | 无 results rematch config capture | scope-excluded | results host rematch config capture不在当前战斗核心范围。 |

## 2. World、固定槽、identity 与分类映射

| FW ID | Unity 当前生产映射 | 状态 | 结论与差异证据 |
|---|---|---|---|
| FW-WR-001 | `SimulationWorld` buckets、400 runtime-slot bitmap/raw slots、Unity pools | Unity-adapter | Unity 不预构造 400 个同一 CLR Entity，而以对象池实体 + 400 逻辑槽承载；固定槽查询仍存在。 |
| FW-WR-002 | `GameDataManager.GetObjectById`、`ResolveRuntimeCharacterConfig` | Unity-adapter | OID 越界/缺失返回 null 的可观察语义存在，registry 载体不同。 |
| FW-WR-003 | Unity DAT manager load/register | confirmed-difference | 没有权威 `AllocChar` 首次 append/count 以及解析失败后保留非 null 槽的同等状态。 |
| FW-WR-004 | `SimulationWorld.Register/AllocateRuntimeSlot` | Unity-adapter | Unity以用途分区和required slot实现各正式调用点的槽约束；通用API形状不同，但未单独证明生产调用产生不同slot。 |
| FW-WR-005 | `Register:303-350` | confirmed-difference | 注册所有实体都会 `ResetCooldownsForRuntimeSlot`；权威 `SpawnAt` 本身不清 ARest/VRest。 |
| FW-WR-006 | `SimulationWorld.ResetRuntimeState` | confirmed-difference | 有整场 reset，但 RNG、results/rematch、registry 和预构造 pool 保留边界不一致。 |
| FW-WR-006-A | `BattleRuntimeState.Reset`、`ResetRegisteredObjects` | Unity-adapter | 大多数 world/flow/roster/stage/pending 状态被清理；pause 保存在 driver 而非 runtime。 |
| FW-WR-006-B | `BattleResultsRuntimeState.Reset` | scope-excluded | `PrepareForBattleRematch`只服务宿主results rematch，属于当前排除范围。 |
| FW-WR-006-C | `ResetRegisteredObjects`、`ItrRest.Reset`、raw slot reset | Unity-adapter | Unity 以每实体 rest tracker + 400 raw slots代替两张固定矩阵，整场 reset 会清活动实体和 raw 状态。 |
| FW-WR-006-D | `ResetRegisteredObjects` + `LF2ReferencePool/LF2ObjectPool` | Unity-adapter | CLR/GameObject 由池复用而非 400 个永久 Entity；逻辑槽、字段、表现状态按 Unity 生命周期清理。 |
| FW-WR-006-E | `ResetRuntimeState:134-145` | confirmed-difference | Unity 明确 `Rng.Seed(0x4E545344)`，随后 ApplyMatchConfig 再 seed；权威重建/rematch 保留全局 RNG 序列。 |
| FW-WR-007 | `FreeEntityLikeExe`、deferred `Unregister`/flush | Unity-adapter | Unity 先从 pass 查询隐藏并释放逻辑槽，稍后释放 renderer/CLR shell；该机制用于模拟权威立即 inactive/可复用槽。 |
| FW-WR-008 | `ResetCooldownsForRuntimeSlot` | equivalent | 清 occupant ARest 及所有实体与该 slot 的 VRest 双向关系。 |
| FW-WR-009 | `FindEntityByRuntimeSlotIncludingDormant`、raw runtime slots | Unity-adapter | inactive slot 不保证有 CLR entity；raw slot提供逻辑投影，调用者必须选择 active/pending/dormant 查询。 |
| FW-ID-001 | `LF2Entity` 字段 + `NTSDEntityRuntime` 快照 | Unity-adapter | Unity的Health/Frame/Trans与Runtime不是全部同一存储，靠明确刷新边界同步；这是结构适配，未仅凭双存储判行为差异。 |
| FW-ID-002 | `LF2Entity.ResolveCurrentDataObjectType` | equivalent | dispatch 分类读取当前 DAT wrapper/type，不以 CLR subclass 或 runtime category 为真值。 |
| FW-ID-003 | `ResolveReferenceRuntimeObjTypeFromDataType` | equivalent | DAT type 0 映射 character，其他映射粗粒度 weapon；完整 DAT type另行保留。 |
| FW-ID-004 | current-DAT gates + shared character DAT controllers | Unity-adapter | Unity CLR 壳仍参与方法承载，但关键 dispatch 已改为当前 DAT type，并为 transformed character 补 shared controller。 |
| FW-ID-005 | `LF2Entity.TryApplyRuntimeIdentity` | equivalent | 写 ObjectId/current wrapper/type/weapon HP/frame，未重置 team、link、HP、slot 等外围状态。 |
| FW-ID-006 | `EarlyFrameAdvanceSpecialsAll/RunEarlyState501Specials` | equivalent | state501 parent 与 child identity/frame 同一早期 pass 更新。 |
| FW-ID-007 | `LateEntityUpdateAll` -> `RunStateSpecialPreCollision` | equivalent | 9995/4000..4999/8000..8999 identity replacement 位于碰撞后 late pass。 |
| FW-ID-008 | `Oid5152RuntimeMaintenanceAll` + `OidMergeDormant` | Unity-adapter | 合并对象以 dormant CLR shell 模拟直接 inactive，split 复用同 runtime slot；查询和 ObjectCount 排除 dormant。 |
| FW-ID-009 | `RefreshRuntimeSnapshot`、parity projection | Unity-adapter | category/identity 是由当前 DAT 派生的快照，但 Unity 需要显式同步。 |

## 3. Tick 外层入口映射

| FW ID | Unity 当前生产映射 | 状态 | 结论与差异证据 |
|---|---|---|---|
| FW-DRV-001 | `SimulationTickDriver.OnSingletonAwake/Update` | Unity-adapter | Unity 默认暂停到场景/角色装配完成，accumulator不预装 tick；这是 Unity scene生命周期的宿主门控，不改变单 tick规则。 |
| FW-DRV-002 | `Update`、`LateUpdate`、`AppManager.UnloadBattle` | scope-excluded | route-out/rebootstrap属于宿主范围；LateUpdate仅做表现刷新。 |
| FW-DRV-003 | `Update:128-147`、`SimulationConstants.SIM_DT` | Unity-adapter | 固定30Hz一致；每渲染帧追帧和积压截断是 Unity宿主节奏适配，不改变单 tick dt。 |
| FW-DRV-004 | `CanAdvanceTick/StepOneTickInternal` | equivalent | input-ready gate 在修改 tick/world 前返回。 |
| FW-DRV-004-B1 | `StepOneTickInternal:195-203` | equivalent | Before/Get、tick mismatch 空输入、ApplyFrameInput 的顺序一致。 |
| FW-DRV-004-B2 | `SimulationWorld.FrameInput.partial.cs` | Unity-adapter | 以 Unity roster/runtime slot 映射非 AI 玩家，并写离散 frame input；承载输入对象不同。 |
| FW-DRV-004-B3 | `StepOneTickInternal:203-205` | equivalent | tick 后 capture checksum，再 `AfterSimTick`；即使内部 early return，driver 尾仍执行。 |
| FW-DRV-005 | `NTSDBattleTickSystem.RunReleaseTick` | Unity-adapter | Unity 没有单独 scheduler wrapper，driver 直接调用 tick system。 |
| FW-DRV-006 | entity/runtime 显式快照同步 | Unity-adapter | 没有 legacy/world 双向 CharacterSync；Unity 在 pass/实体边界刷新 runtime snapshot。 |
| FW-DRV-007 | `SimulationWorld.PendingSounds`、Unity audio/scene host | scope-excluded | 普通音频消费、PendingHostAction rematch/bootstrap和host countdown均属既定宿主排除范围。 |

## 4. 正式 tick pass 映射

| FW ID | Unity 当前生产映射 | 状态 | 结论与差异证据 |
|---|---|---|---|
| FW-TK-001 | driver `ApplyFrameInputSet` + `RunReleaseTick:21-25` | Unity-adapter | 权威 driver也在 GameTick header前实际 `PollHumanInput`；Unity先入队再poll，随后清 sound/推进header。只有外部poll标记的落点不同。 |
| FW-TK-002 | 无 results-active tick early return | confirmed-difference | Unity results active 后仍进入普通 frame/collision/late pass，与权威下一tick只走results的行为直接矛盾。 |
| FW-TK-003 | `VrestTickAll` | equivalent | ARest/defend lock/AttackExempt cooldown 位于早期 pass。 |
| FW-TK-004 | `PostCooldownHumanInputAll` | Unity-adapter | 方法名虽位于 cooldown前，但它承担权威 driver的实际 human poll；权威 `postCooldownInput` callback在生产宿主只写观察标记。F1 step gate另属排除范围。 |
| FW-TK-005 | `Oid5152RuntimeMaintenanceAll` | equivalent | 位于 cooldown 后、input clear/character input 前。 |
| FW-TK-006 | `RunFrameAdvancePhase:36-40` | equivalent | 权威 driver同样已先poll human input；随后 cooldown、清 NeedClearInput并整 tick return。 |
| FW-TK-007 | `CharacterInputAll:105-127` | equivalent | Unity明确 `tickIndex<=1 return`，从第二 tick开始应用 character input。 |
| FW-TK-008 | `EarlyFrameAdvanceSpecialsAll` | equivalent | 400/401、500、501 位于 frame logic 前。 |
| FW-TK-009 | `FrameLogicBeforeAdvanceAll` | equivalent | 按 current DAT type/frame HitFa 执行非角色 frame logic。 |
| FW-TK-010 | `SerialTickAll` | equivalent | runtime-slot 顺序执行 frame advance，并由实体逻辑维护 key/frame 状态。 |
| FW-TK-011 | `PostFrameAdvanceDeathCleanupAll` | equivalent | state9998 cleanup 后 respawn/free/spawn。 |
| FW-TK-012 | `ClampCharacterZToStageBoundsAll` | equivalent | 第一次 character Z clamp。 |
| FW-TK-013 | `RunFrameAdvancePhase` 内固定调用点 | Unity-adapter | Unity 无观察 callback API；相同时点直接进入后续 cpoint/interaction。 |
| FW-TK-014 | `PreInteractionTickAll` | Unity-adapter | cpoint/抓取前置逻辑由 Unity entity/resolver 承载。 |
| FW-TK-015 | `PreInteractionTickAll`/held sync resolver | Unity-adapter | held pose 同步由 Unity link/entity 组件实现。 |
| FW-TK-016 | `ValidateHeldLinksAll` | equivalent | 正 link 校验位于 pre-interaction 后、第二次 Z clamp 前。 |
| FW-TK-017 | `ClampCharacterZToStageBoundsAll` | equivalent | 第二次 Z clamp。 |
| FW-TK-018 | `HeldObjectProcessAll` | Unity-adapter | held step12/link 处理在第二次 clamp 后、碰撞 snapshot 前。 |
| FW-TK-019 | `CaptureCollisionFrameSnapshotsAll` | equivalent | 冻结 PrevFrame2/collision snapshot 后再收集候选。 |
| FW-TK-020 | `CollectCollisionCandidatesAll` | equivalent | candidate collect 位于两轮 resolve 前。 |
| FW-TK-021 | `PostInteractionTickAll` | Unity-adapter | character hit loop 由 InteractionResolver 分派。 |
| FW-TK-022 | `RandomWeaponDropTickAll` | equivalent | natural drop 位于 character hit 后、object hit 前。 |
| FW-TK-023 | 无 runtime F8 flag/drop | scope-excluded | F8 debug drop不属于当前正式战斗逻辑backlog。 |
| FW-TK-024 | `ObjectInteractionTickAll` | Unity-adapter | object hit loop 紧随 natural drop。 |
| FW-TK-025 | `ApplyPreFrameBoundsAll` | Unity-adapter | entity bounds规则存在；camera/render offset采用用户确认的fixed-world Unity表现适配。CameraX只进入已排除的F8坐标分支。 |
| FW-TK-026 | 无 F1 slow wait gate | scope-excluded | F1 debug/step-wait不属于当前正式战斗逻辑backlog。 |
| FW-TK-027 | `CurrentWaveStageTickAll` -> phase advance | equivalent | bounds 后先尝试当前 wave advance。 |
| FW-TK-028 | `CurrentWaveStageTickAll` -> immediate/deferred/refill | confirmed-difference | 流程顺序接近，但 Unity stage spawn 经 Register 会额外清 slot cooldown。 |
| FW-TK-029 | `RenderDispatchAll` | Unity-adapter | 权威 pre-render callback 映射为 Unity renderer pass；表现对象由 LateUpdate/renderer 接续。 |
| FW-TK-030 | 无 F1 early return | scope-excluded | F1 debug slow early-return已排除。 |
| FW-TK-031 | `FramePostProcessAll` | equivalent | 聚合 knockback 后清累积字段。 |
| FW-TK-032 | `LateEntityUpdateAll` | equivalent | 主要 late 顺序闭合；额外 `SimEntityCollision` 当前仅有基类空实现且无生产 override，不产生可观察状态变化。 |
| FW-TK-033 | `Mode2RandomWeaponDropTailAll` | scope-excluded | Mode2批量drop属于debug范围。 |
| FW-TK-034 | `EntityPostFrameTailAll` + collector初始化 | confirmed-difference | heal/catch/state1700存在；Unity在下一tick收集开始才 `ClearHitCandidateCarriers`，权威在本tick尾清，因此tick后可观察carrier状态不同。 |
| FW-TK-035 | 无 observer callback | Unity-adapter | 观察 callback 被省略，不影响生产规则；results 紧随 tail。 |
| FW-TK-036 | `UpdateBattleResultsFlow` | equivalent | mode1 两队存活聚合和 11 tick summary latch存在。 |
| FW-TK-037 | `Mode2RandomWeaponDropTailAll:SetMode2Request(0)` | scope-excluded | `InitStats/GameMode2`均为debug尾字段，不计入当前战斗核心backlog。 |

## 5. GameTick helper / branch 映射

| FW ID | Unity 当前生产映射 | 状态 | 结论与差异证据 |
|---|---|---|---|
| FW-H-001 | 无 `IsStepWaitGate` 等效逻辑 | scope-excluded | F1/step-wait debug门控已排除；driver pause是独立Unity宿主适配。 |
| FW-H-002 | `FrameLogicBeforeAdvanceAll` 的 DAT/frame/HitFa gates | equivalent | active/current DAT非角色/frame有效/HitFa>0 的 gate存在。 |
| FW-H-003 | 无 `RunResultsTick` | scope-excluded | results菜单输入、设置和PendingHostAction属宿主/UI排除范围。 |
| FW-H-004 | 无4v4 results reserve sync | scope-excluded | results committed reserve属于宿主rematch配置范围。 |
| FW-H-005 | 无 results fall-damage apply | scope-excluded | 该写入只服务下一场results rematch设置，当前不建立backlog。 |
| FW-H-006 | 无 results stage-selection advance | scope-excluded | results菜单stage选择属UI/宿主范围。 |
| FW-H-007 | 无 results controller resolver | scope-excluded | results菜单controller选择属UI范围。 |
| FW-H-008 | `QueueSound`存在；results Pressed不实现 | scope-excluded | results edge和普通音频播放均为排除范围。 |
| FW-H-009 | `UpdateBattleResultsFlow:174-264` | equivalent | 两队曾同时存活、latch winner、11 tick激活 summary 的规则一致。 |
| FW-H-010 | `ClearBattleEntryInputAll` | equivalent | 清 active character 输入及 Unity 输入 mirror。 |
| FW-H-011 | `CharacterInputAll` | Unity-adapter | current-DAT character 分派到 human/AI/combo链；详细输入由另一分区核销。 |
| FW-H-012 | `RandomWeaponDropTickAll` | equivalent | weapon count、`Rand%200`、data.txt加载顺序候选、122/123 gate、选择和四次坐标RNG均闭合。 |
| FW-H-013 | 无 runtime F8 drop | scope-excluded | F8 debug drop已排除。 |
| FW-H-014 | `Mode2RandomWeaponDropTailAll/SpawnMode2RandomWeapons` | scope-excluded | Mode2 debug drop已排除；当前实现与权威LoadedOidOrder契约仍不作为核心等价证据。 |
| FW-H-015 | factory drop spawn + required slot + cooldown reset | Unity-adapter | GameObject/renderer由工厂创建，最终 position/HP/PP/slot/cooldown 契约基本闭合。 |
| FW-H-016 | `PostFrameAdvanceDeathCleanupAll` | equivalent | 固定 state9998 cleanup 后 respawn。 |
| FW-H-017 | `CleanupState9998Entities` + pending destroy flush | Unity-adapter | 逻辑上当场从 active query消失，CLR/GameObject 在 flush 时释放。 |
| FW-H-018 | `PassesRespawnGate`、两条 respawn helper | Unity-adapter | gate、平均位置、RNG、HP/PP/frame/effect 由 Unity实体与工厂承载。 |
| FW-H-019 | `EarlyFrameAdvanceSpecialsAll` | equivalent | 400/401 -> 500 -> 501 固定顺序。 |
| FW-H-020 | LF2Entity early state400/401 helpers | equivalent | 最近敌人/最远队友和位置同步逻辑存在。 |
| FW-H-021 | `RunEarlyState500Specials` | equivalent | `Unk33C/Unk324` gate 后 Frame=0。 |
| FW-H-022 | `RunEarlyState501Specials` | equivalent | parent/child transform 与 frame选择存在。 |
| FW-H-023 | `Oid5152RuntimeMaintenanceAll` | equivalent | slot<20、cooldown、merge/split gate存在。 |
| FW-H-024 | `TryMergeOid7Or8Into51` | Unity-adapter | partner 以 `OidMergeDormant` 模拟残留 inactive，不销毁 CLR壳；逻辑查询和 ObjectCount排除。 |
| FW-H-025 | `TrySplitOid51BackToPair` | Unity-adapter | 复用 dormant partner slot并重建字段；partial failure 保持 dormant。 |
| FW-H-026 | `VrestTickAll/ClearAttackExemptIfCurrentFrameCannotHit` | equivalent | ARest与 AttackExempt/held frame gates闭合；VRest decrement 在 collector。 |
| FW-H-027 | `ApplyPreFrameBoundsAll` + entity bounds helpers | Unity-adapter | entity Z/X/free规则闭合；camera/bg部分由fixed-world Unity表现层接管。 |
| FW-H-028 | `ResetUnityFixedWorldRenderOffsets` | Unity-adapter | 用户确认使用fixed-world camera/renderer适配；权威CameraX唯一战斗读取位于已排除F8 debug drop。 |
| FW-H-029 | `RunPreCollisionRecoveryPhase` | equivalent | late pass中碰撞后执行 regen/stat恢复。 |
| FW-H-030 | `CaptureCollisionFrameSnapshotsAll` | equivalent | active实体冻结 collision frame。 |
| FW-H-031 | `LateEntityUpdateAll` runtime-slot 0..399 loop | equivalent | 低槽生成的高槽对象可在同一 late pass继续被遍历。 |
| FW-H-032 | `LateEntityUpdateAll` + LF2Entity late helpers | equivalent | identity/regen/frame/death/opoint/cleanup顺序闭合；`SimEntityCollision`无生产override，当前是无状态空调用。 |
| FW-H-033 | `LF2Entity.RunStateSpecialPreCollision` | equivalent | state range identity replacement和 8000 hitstop分支存在。 |
| FW-H-034 | `RunLateDeathOpointPreCleanupPhase` death bounce | equivalent | frame186与 Y/Vy/knockback写入存在。 |
| FW-H-035 | `QueueBattleSound/SimulationWorld.QueueSound` | equivalent | broken cue空值 gate并记录 cue、X、当前逻辑 tick。 |
| FW-H-036 | LF2Entity N30 trigger helper | Unity-adapter | history pattern、同队目标和 OID998 effect由 Unity factory承载。 |
| FW-H-037 | `RunLateTailBeforePrevFrame` transition-effect helper | equivalent | current/previous DAT frame state读取和两分支触发条件存在。 |
| FW-H-038 | `SpawnTransitionEffectBranch1` + factory | Unity-adapter | SFX、最多15、RNG和 slot限制通过 Unity task/pool实现。 |
| FW-H-039 | `SpawnTransitionEffectBranch2` + factory | Unity-adapter | count/RNG/slot耗尽行为由 Unity task/pool实现。 |
| FW-H-040 | `FindFirstAvailableFrameLogicSlot/CountAvailableTransitionEffectSlots` | equivalent | dynamic 50..399 首空槽。 |
| FW-H-041 | `ResetCooldownsForRuntimeSlot` | equivalent | 清 ARest和相关 VRest。 |
| FW-H-042 | `EntityPostFrameTailAll`、collector初始化 | confirmed-difference | heal/catch/state1700存在，但 `ClearHitCandidateCarriers`被移到下一tick candidate collect开始；本tick尾状态与权威不同。 |
| FW-H-043 | `ClampCharacterZToStageBoundsAll` | equivalent | current-DAT character Z/ZInt clamp。 |
| FW-H-044 | `FramePostProcessAll` | equivalent | FrameDelay gate、2/(count+1)聚合与 accumulator清理一致。 |
| FW-H-045 | `ValidateHeldLinksAll` | equivalent | 只修 holder正 link，不反修 target。 |
| FW-H-046 | `StageProgressionCurrentPhase` | equivalent | series/wave越界返回 null。 |
| FW-H-047 | `StageProgressionCanAdvanceWave` | equivalent | 末波false；WaveIdx为-1或waveReady为true时允许推进。 |
| FW-H-048 | `StageProgressionAdvanceWave` | equivalent | gate通过后 `WaveIdx++`；额外首波 caller另列 Unity-only。 |
| FW-H-049 | `StageSpawnEntryHp` | equivalent | 配置 HP>0否则500。 |
| FW-H-050 | `SpawnStageImmediateEntrySlot` | confirmed-difference | 初始化/坐标/RNG接近，但 Unity Register无条件清该 slot cooldown，权威 stage `SpawnAt` 不清。 |
| FW-H-051 | `ApplyCurrentWaveImmediateStageSpawns` 内联调用 | equivalent | 以 spawn slot>=0判成功。 |
| FW-H-052 | `StageSpawnEntryFactor` | equivalent | 只数 slot<20 active character，OID51/52加权。 |
| FW-H-053 | `ResetStageSpawnRuntime` | equivalent | wave=-1并清四组 runtime lists。 |
| FW-H-054 | `EnsureCurrentWavePositiveStageRuntime` | equivalent | ratio、40上限、entry/target total规则闭合。 |
| FW-H-055 | `RefillCurrentWavePositiveStageSpawns` | equivalent | 清失活/错 OID跟踪槽后补到 target，spawn失败break。 |
| FW-H-056 | `CurrentWaveStageSpawnsCleared` | equivalent | 按 OID 扫 slot>=20，不区分 phase ownership。 |
| FW-H-057 | `CurrentWaveStageSpawnProducersInitialized` | equivalent | immediate/deferred marker gate一致。 |
| FW-H-058 | `ApplyCurrentWavePhaseAdvance` | equivalent | progression/mode/wave/producer/clear/advance gates与 bound/runtime reset顺序闭合。 |
| FW-H-059 | `ApplyCurrentWaveImmediateStageSpawns` | confirmed-difference | immediate/deferred/refill顺序闭合，但所有实际 spawn继承 Unity Register额外 cooldown reset。 |

## 6. 生命周期与同 tick 可见性映射

| FW ID | Unity 当前生产映射 | 状态 | 结论与差异证据 |
|---|---|---|---|
| FW-LC-001 | pool create -> `Register` | Unity-adapter | Unity通用注册会清cooldown，但各生产spawn需按调用点判定；明确冲突的stage路径已单列FW-LC-004/FW-H-050。 |
| FW-LC-002 | opoint/drop/effect factory + required slot | Unity-adapter | 创建时重置 entity/runtime/rest并立即注册；slot和同 pass可见性由抑制字段显式控制。 |
| FW-LC-003 | `ProcessOpointSpawnAlignedToCpp` + queued-task flush | Unity-adapter | late pass生成 dynamic slot，可在后续 runtime slot继续 frame tick；GameObject发布由工厂承担。 |
| FW-LC-004 | `SpawnStageImmediateEntrySlot` | confirmed-difference | 本 tick进入后续 late的时点接近，但 cooldown被额外清理，且首波在 bootstrap被提前启动。 |
| FW-LC-005 | `FreeEntityLikeExe` + `Unregister`/flush | Unity-adapter | active query立即隐藏、slot可提前释放；对象池释放延后到安全 mutation boundary。 |
| FW-LC-006 | pending destroy paths | Unity-adapter | state9998/bounds/invalid/broken/hit free均用 pending标志模拟权威立即 inactive。 |
| FW-LC-007 | `OidMergeDormant` | Unity-adapter | partner runtime残留并从 active query/ObjectCount排除，split前复用并重建。 |
| FW-LC-008 | hit_Fa source `PendingFlushDestroy` | Unity-adapter | 源对象字段/identity保留到 flush，逻辑上当场 inactive；raw/dormant查询用于 parity投影。 |
| FW-LC-009 | `ResetRuntimeState` | confirmed-difference | entity/pool/rest清理存在，但 Unity整场 reset会 reseed RNG，权威保留全局序列。 |

## 7. Results / 战斗结束与宿主边界映射

| FW ID | Unity 当前生产映射 | 状态 | 结论与差异证据 |
|---|---|---|---|
| FW-END-001 | `UpdateBattleResultsFlow` | equivalent | mode1、HadBoth、winner latch和11 tick summary激活一致。 |
| FW-END-002 | 无 results-only tick | confirmed-difference | summary激活后的下一 tick仍运行全部普通战斗 pass。 |
| FW-END-003 | `BattleResultsRuntimeState.PendingHostAction` 字段仅存在 | scope-excluded | results菜单设置和host action属宿主/UI排除范围。 |
| FW-END-004 | 无 `RematchBattle` | scope-excluded | 宿主rematch/rebootstrap已排除。 |
| FW-END-005 | `AppManager` scene unload/load | scope-excluded | scene reload属于Unity宿主路径，不以它核销权威results bootstrap。 |
| FW-END-006 | 无 F4/route-out战斗宿主链 | scope-excluded | F4 debug与route-out/countdown属排除范围。 |
| FW-END-007 | Unity scene/app退出生命周期 | Unity-adapter | SDL close细节属于宿主适配；未发现它定义额外战斗规则。 |

## 8. Snapshot / runtime 镜像边界映射

| FW ID | Unity 当前生产映射 | 状态 | 结论与差异证据 |
|---|---|---|---|
| FW-SN-001 | `CaptureParityFrameSnapshot` | scope-excluded | 完整snapshot/apply/rollback不在当前实现范围。 |
| FW-SN-002 | `BattleParitySnapshot` 分域投影 | scope-excluded | 当前仅作审计观察工具，不以结构差异建立核心backlog。 |
| FW-SN-003 | 无 snapshot apply | scope-excluded | Apply/rollback明确排除。 |
| FW-SN-004 | `BattleParitySnapshot` hashes | scope-excluded | Unity审计hash域可扩展；它不宣称等同权威runtime checksum。 |

## 9. 权威分区接力项

| FW ID | Unity 当前生产映射 | 状态 | 结论与差异证据 |
|---|---|---|---|
| FW-X-001 | `NTSDInputStateModule`、character input/AI/combo | authority-unresolved | 权威总账明确把输入字段与 RNG细节交给 Input 分区，不能在框架报告中宣称等价。 |
| FW-X-002 | entity frame logic/advance/physics | authority-unresolved | 详细分支交给 Frame/Physics 分区。 |
| FW-X-003 | cpoint/catch resolvers | authority-unresolved | 抓取、投掷、identity swap和 link字段需 Interaction 分区核销。 |
| FW-X-004 | held weapon/link resolvers | authority-unresolved | holder/weapon双向字段和 drop边界需 Interaction 分区核销。 |
| FW-X-005 | `BruteForceSceneQuery` candidate collection | authority-unresolved | pair顺序、VRest decrement、itr索引需 Interaction 分区核销。 |
| FW-X-006 | character/object hit resolvers | authority-unresolved | kind/effect/stat/sound/free/abort需 Interaction 分区核销。 |
| FW-X-007 | frame tick/opoint factory | authority-unresolved | frame wait/next/mp/sound与全部 opoint分支需 Frame/Interaction 分区核销。 |
| FW-X-008 | `BattleStageCampaignLoader`/DAT parser | authority-unresolved | runtime consumer已映射，但 stage parser和 `WaveIdx=-1`数据契约仍需独立确认。 |

## 10. Unity-only 生产可达分支反向清单

| Unity 分支 | 生产可达证据 | 对权威行为的影响 | 分类 |
|---|---|---|---|
| driver pause gate | `SimulationTickDriver.Update:120-126`、`StepOneTick:304-311`；`AppManager.InitializeBattle/UnloadBattle`切换 | 场景装配和用户暂停时阻止宿主调用tick，不改变单tick规则 | Unity-adapter |
| Unity accumulator/backlog策略 | `Update:128-147` | 固定30Hz下的Unity宿主追帧/过载策略 | Unity-adapter |
| bootstrap首波推进 | `ApplyMatchConfig:285-292` -> `StartInitialStageWave:69-83` | `WaveIdx -1 -> 0`，在首个正式 tick前改变 stage状态 | confirmed-difference |
| roster压缩 | `BattleRosterRuntimeState.ApplyMatchConfig:203-226` | active config被压到连续 slot，改变 roster index/entity mapping | confirmed-difference |
| scene spawn point | `AppManager.SetupBattleCharacters:180-225` | 不消费权威出生 RNG，初始 X/Z不同 | confirmed-difference |
| register无条件清 cooldown | `SimulationWorld.Register:346-348` | stage/general spawn也清 ARest/VRest | confirmed-difference |
| pending destroy提前释放 slot | `ReleasePendingDestroySlots:552-583` | 为模拟同 tick复用而在 GameObject释放前释放逻辑槽 | Unity-adapter；需由生命周期测试持续约束 |
| `OidMergeDormant` | `Passes:244/333`、Registry active query gate | 用 dormant CLR壳模拟 partner直接 inactive | Unity-adapter |
| 多组 `Suppress*UntilTick` | `NTSDEntityRuntime:106-111`、各 pass gate | Unity factory可跳过特定新生对象 pass；权威按生成时点自然决定可见性 | Unity-adapter，若赋值错误会成为差异 |
| late `SimEntityCollision` | `SimulationWorld.Passes.partial.cs:779` | 当前无生产override，调用为空；未来出现override时必须重新核销 | Unity-adapter（当前无状态） |
| fixed-world camera reset | `StageRender:120-137` | 用户确认的固定世界表现适配；CameraX战斗读取仅见排除的F8分支 | Unity-adapter |
| scene walkability query | `StageRender.IsGroundPointWalkable` -> `BoundaryWallManager` | Unity碰撞/移动可被 scene polygon阻止；权威框架只见 stage bounds | confirmed-difference，具体调用影响交由 Physics分区 |
| stage direct character fallback | `TrySpawnStageCharacterDirect:610-652` | 无完整 Unity工厂时直接 `new LF2Character`并注册 | Unity-adapter；字段/reset必须与正常factory一致 |
| GameObject/CLR池释放 | `FreeEntityLikeExe`、`LF2ReferencePool/LF2ObjectPool` | 表现和CLR生命周期延后，逻辑active状态先隐藏 | Unity-adapter |
| `UnbindWorld/RecreateWorld` | `SimulationTickDriver:314-332`；测试/scene unload可调用 | 可丢弃整个 world和tick index；属于scene host/完整重建排除范围 | scope-excluded |
| parity snapshot扩展域 | `BattleParitySnapshot` | checksum比较域与权威不同，且只可capture不可apply；仅作审计工具 | scope-excluded |
| renderer `LateUpdate` | `SimulationTickDriver.LateUpdate:157-167` | Unity表现按渲染帧刷新；只要不写回runtime属于适配 | Unity-adapter |
| immediate Unity audio个例 | `LF2OtherObject.ReleaseFlow.partial.cs:65-70` | 绕过 `PendingSounds`直接播放，但普通音频播放不在当前战斗核心范围 | scope-excluded |

## 11. In-scope 确认差异的 5 个根因簇

首轮暂态曾统计25个 `confirmed-difference`。源码复核后，`FW-WR-004`、`FW-ID-001`、`FW-LC-001`降为 `Unity-adapter`，`FW-WR-006-B`改为 `scope-excluded`：API形状、双存储、通用注册或宿主rematch本身都不足以证明生产可观察结果不同。最终有21个in-scope确认差异，全部归入以下5簇。

| 根因簇 | 覆盖 FW IDs | 修复边界与影响文件 | 所需聚焦 self-check |
|---|---|---|---|
| C1 生产 bootstrap/registry/roster/initial prime 未按权威闭合 | `FW-BS-002`, `FW-BS-003`, `FW-BS-004`, `FW-BS-005`, `FW-BS-008`, `FW-BS-008-B1`, `FW-BS-008-B2`, `FW-BS-009`, `FW-WR-003` | 从 `AppManager.InitializeBattle/SetupBattleCharacters`、`SimulationTickDriver.ApplyMatchConfig`、`BattleRosterRuntimeState.ApplyMatchConfig` 到 DAT manager一次整体修；必须保持Unity scene/pool承载，但补齐slot不压缩、有效OID gate、权威出生RNG、初始stats和WaveIdx=-1正式入口。影响 `App/AppManager.cs`、`Simulation/SimulationTickDriver.cs`、`Simulation/BattleRuntimeState.cs`、`Animation/GameDataManager.cs`及角色生成入口。 | 用内存stage/roster夹具覆盖8-slot中间空洞、独立team、无效OID、固定seed、两个有效角色；逐项断言roster index/entity slot、X/Z、RNG call count、HP bonus、HitStop、速度、frame/int position，以及首tick前WaveIdx。不得依赖默认stage.dat。 |
| C2 整场 reset 与 RNG continuation 边界错误 | `FW-WR-006`, `FW-WR-006-E`, `FW-LC-009` | `SimulationWorld.ResetRuntimeState`不能自行固定reseed；初次seed、普通reset和未来显式`ResetWorld(seed)`要分开。影响 `SimulationWorld.Registry.partial.cs`、`SimulationTickDriver.ApplyMatchConfig`、`DeterministicRng.cs`。 | 预置RNG state/callcount并注册实体，执行runtime reset；断言实体/rest/world被清而RNG下一值延续。另测显式新战配置只在权威允许的初始seed边界设置一次。 |
| C3 stage spawn错误继承Register通用cooldown reset | `FW-WR-005`, `FW-TK-028`, `FW-H-050`, `FW-H-059`, `FW-LC-004` | 把逻辑槽注册与ARest/VRest reset解耦，按spawn semantic决定；stage `SpawnAt`必须保留旧slot rest，weapon/effect/respawn等权威明确路径仍清。影响 `SimulationWorld.Register`、`SpawnStageImmediateEntrySlot`、object factory/task spawn semantic和rest tracker。 | 在即将复用的stage slot预置ARest、入向/出向VRest，触发immediate与positive/refill stage spawn，断言全部保留；并对natural weapon、opoint/effect、respawn各做反例，断言应清路径仍清。 |
| C4 results active后仍运行普通战斗pass | `FW-TK-002`, `FW-END-002` | 在tick header/输入观察边界后增加results-only核心早退；不要求实现results菜单或host rematch，但必须阻止frame、碰撞、stage、late和tail。影响 `NTSDBattleTickSystem.RunReleaseTick`、`BattleResultsRuntimeState`。 | 构造Results active实体，记录frame/position/HP/rest/stage wave，运行一tick；断言tick header按权威推进而所有普通战斗字段不变，且不会产生碰撞、opoint或stage spawn。 |
| C5 hit candidate carrier清理晚一个tick | `FW-TK-034`, `FW-H-042` | 把 `ClearHitCandidateCarriers` 从下一tick `CollectCollisionCandidates`初始化补回本tick entity postframe tail；collector可保留防御性reset，但不能成为唯一清理点。影响 `SimulationWorld.Passes.partial.cs`、`BruteForceSceneQuery.cs`、各实体carrier字段。 | 让character/weapon/special在interaction写入HitConfirm2，执行完整tick；在tick checksum/AfterSimTick前断言carrier已清零，并验证同tick动作读取仍能看到命中、下一tick输入不能看到残留。 |

## 12. 特殊边界结论

1. 纯 C# 可运行不是本次判定要求。Unity 可以保留 `MonoBehaviour`、`GameObject`、`Transform`、renderer和对象池，但这些对象不能成为战斗真值。
2. 400固定槽在 Unity 中可以由 bitmap/raw slots + pooled CLR shell承载；是否等价取决于 slot选择、reset、cooldown和同 tick可见性，而不是 CLR对象是否永久存在。
3. `PendingFlushDestroy` 与 `OidMergeDormant` 原则上是合法 Unity adapter；只有 active query、ObjectCount、slot复用、snapshot和后续重建全部保持权威边界时才成立。
4. `Transform` 和渲染帧目前没有被当作主要位置真值，但 scene spawn point和 walkability已经进入逻辑输入，因此不能一概归为表现层。
5. CameraX 在权威中唯一非表现读取位于 F8 debug drop；该分支当前排除。因此 fixed-world camera可作为Unity adapter，但若未来把CameraX接入正式实体坐标，必须重新判定。
6. RNG state与 `Match.Seed`是两个边界。Unity整场 reset/rebootstrap重新 seed，而权威全局 RNG延续；不能用相同初始 seed掩盖重建后的序列差异。
7. results summary判胜已经存在，但 results-active 后停止普通战斗、结果输入、rematch/bootstrap和host action没有闭合，因此不能称战斗结束框架已对齐。
8. parity snapshot是审计工具，不是权威 snapshot等价实现。当前缺 Apply/rollback，hash域也不同，只能用于观察，不能反证 runtime已等价。

## 13. 计数、证据强度与完成边界

- 权威输入总账唯一 FW ID：172。
- 本报告逐项出现 FW ID：172；唯一172，遗漏0，额外0。
- 状态统计：`equivalent` 64、`Unity-adapter` 51、`confirmed-difference` 21、`missing` 0、`authority-unresolved` 8、`scope-excluded` 28；合计172。
- 证据强度：当前生产源码静态调用链审计；未把现有 self-check 通过情况当作本报告结论。
- 本报告不是编译、Play Mode或定向场景验收，也不声明整个战斗系统已对齐。
- `authority-unresolved` 的 8 项必须由对应 Input/Frame/Interaction/Stage parser总账接力。
- `scope-excluded` 不进入当前backlog；范围恢复时必须重新从权威调用链核销。
- `confirmed-difference` 与 `missing` 项均需要后续实现、编译、自检和定向运行时证据后才能改变状态。
