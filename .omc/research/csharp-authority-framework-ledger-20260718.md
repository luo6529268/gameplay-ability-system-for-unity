# C# 权威战斗框架总账（2026-07-18）

## 0. 范围、方法与结论口径

- 唯一读取源：`J:\QQFile\NTSD2.4\ntsd_release_C#`。
- 本次没有读取任何 Unity 实现，也没有使用旧对齐文档作为依据；没有修改权威源码。
- 本账只闭合：程序/宿主到战斗 world 的启动、正式 tick 入口和 pass 顺序、固定槽 registry、实体身份/分类、reset/free/reuse 生命周期、战斗结束到重赛/重启边界。
- 输入、AI、帧推进内部、碰撞、命中、cpoint/wpoint/opoint 细节仅追到正式下游入口；这些入口列入接力清单，不在本账中伪装成已完成的细节审计。
- 菜单、结果页光标/表格编辑等 UI 细节排除；只保留它们改变战斗 runtime、触发 rematch/rebootstrap 的边界。
- 行号均取自权威文件的 `rg -n` 结果。源文件含混合换行/乱码注释，PowerShell `Get-Content` 的显示行号可能比 `rg` 少，因此本账以 `rg` 行号为证据行号。

总框架：

```text
Program.Main
  -> LaunchSdlBattleHost
     -> new GameWorld (400 个 Entity 预构造固定槽)
     -> new SimulationTickDriver(BattleTickScheduler)
     -> NtsdRng.Srand(seed)
     -> RuntimeBootstrap.LoadAllChars / LoadStageCampaigns
     -> BattleHostBootstrap.InitializeDirectBattle
        -> load background (失败则 800 / 180 / 350)
        -> DirectBattleBootstrap.InitializeFromConfig
           -> ResetBattleRuntime
           -> roster/config/world 字段
           -> Spawn 首批角色
     -> SdlBattleHost.Run
        -> BattleHostRuntime.StepFrame (30 Hz，最多追 4 tick)
           -> SimulationTickDriver.StepOneTick
              -> frame input
              -> BattleTickScheduler.StepOneTick
                 -> NtsdBattleTickSystem.RunOneTick
                    -> CharacterSync.SyncRuntimeFromLegacy
                    -> GameTick.Run
                    -> CharacterSync.SyncRuntimeFromLegacy
              -> optional checksum
        -> audio consume / result host action / render
```

## 1. 覆盖清单

本账实际读取 42 个权威文件（其中 `HitResolve.cs` 只读取与释放边界直接相关的片段），并定义 172 个唯一框架方法/分支/阶段 ID。核心文件如下：

| 分区 | 权威文件（关键行） | 覆盖内容 |
|---|---|---|
| 进程启动 | `Program.cs:23,48,118,226` | 正式 SDL 路由、world/driver/bootstrap 创建、seed、数据加载、启动失败边界 |
| 战斗 bootstrap | `src/App/DirectBattleBootstrap.cs:34-244` | config、8 roster slots、随机出生、rematch 差异、初始统计 |
| 宿主接线 | `src/Host/BattleHostBootstrap.cs:15,36`; `BattleHostRuntime.cs:41,64,141,193,225,239`; `SdlBattleHost.cs:21,31` | 背景、固定 30 Hz、result host action、重赛/重启、退出 |
| world/registry | `SimulationWorld.cs:22,175`; `SimulationWorld.Registry.cs:14-95`; `SimulationWorld.Passes.cs:13`; `SimulationWorld.QueryAndLinks.cs:12` | 400 固定槽、1000 OID 数据槽、spawn/free/reset、冷却矩阵 |
| 正式 tick | `SimulationTickDriver.cs:25`; `BattleTickScheduler.cs:17`; `NtsdBattleTickSystem.cs:15`; `GameTick.cs:19-2391` | 外层输入/校验、完整 pass 顺序、全部 GameTick helper |
| 分类/身份 | `Entity.cs:7-168`; `EntityCategory.cs:7-33`; `EntityDispatch.cs:12,34`; `NtsdConstants.cs:5-47` | Active/slot/CharData/current identity、DAT obj_type 分类 |
| runtime/snapshot | `NtsdEntityRuntime.cs:8-48,748-879`; `BattleRuntimeState.cs:7-263`; `CharacterSync.cs:12-86`; `ResultsState.cs:5-281` | reset 默认值、runtime 镜像、snapshot/checksum、结果态 |
| 生命周期下游 | `FrameAdvance.cs:13-1019`; `FrameTick.cs:13-457`; `HitResolve.cs:1284-1287` | 所有直接 `Active=false`、opoint/特效生成、命中释放入口 |
| 分派包装 | `FrameRuntimePasses.cs:12-29`; `InteractionRuntimePasses.cs:16-53`; `ObjectPointFactory.cs:11`; `CollisionCandidateCollector.cs:9`; `HitResolver.cs:9,14` | GameTick 到各领域正式入口 |

## 2. Bootstrap / world 初始化总账

### 2.1 进程与资源前置

| ID | 文件/方法 | 调用者 -> 被调者 | 分支、读写与副作用 | 下一边界 |
|---|---|---|---|---|
| FW-BS-001 | `Program.cs:23 Main` | OS -> `LaunchSdlBattleHost` | `args` 为 `sdl`、空、`battle`、`host` 都走 SDL；其他值也走 SDL。没有其他正式战斗路由。 | 进入 FW-BS-002 |
| FW-BS-002 | `Program.cs:118 LaunchSdlBattleHost` | `Main` -> config/world/driver/bootstrap | 创建 `GameWorld`、`BattleTickScheduler`、`SimulationTickDriver`、`RuntimeBootstrap`；调用 `NtsdRng.Srand(ReadRuntimeSeed())`；加载全部 OID、背景数量、stage campaign。`loadedChars<=0` 抛错，战斗不启动。 | 数据 registry 完成后进入 FW-BS-005 |
| FW-BS-003 | `Program.cs:226 ReadRuntimeSeed` | FW-BS-002 -> Win32 `GetTickCount` | 优先 `NTSD_CSHARP_RNG_SEED`，其次 `NTSD_RNG_SEED`；支持十六进制 `0x` 与十进制；否则系统 tick。副作用由 `NtsdRng.Srand` 写全局静态 `Seed/CallCount=0`。 | seed 对随后 bootstrap 的出生 RNG 立即可见 |
| FW-BS-004 | `RuntimeBootstrap.cs:31,54,65,92,112` | FW-BS-002/FW-BS-005 -> DAT loader | `LoadOid` 已存在即成功；解密或分配/解析失败返回 false；成功写 `CharData[oid]`, `ObjType`, `Oid`, `LoadedOidOrder`, `CharCount`。背景按 index 查找，找不到返回 false。stage campaign 只加载到 list。 | `CharData` 在整场 reset 中保留 |
| FW-BS-005 | `BattleHostBootstrap.cs:15 InitializeDirectBattle` | Program/route-out -> background + `DirectBattleBootstrap` | 背景加载失败时强制 `Width=800,ZMin=180,ZMax=350`；构造双人 config 后初始化。 | FW-BS-008 |
| FW-BS-006 | `BattleHostBootstrap.cs:36 InitializeFromBattleConfig` | rematch -> background + `InitializeFromConfig` | 与 FW-BS-005 同背景 fallback，但使用 capture 的 8-slot config，并传 `resultStartRematch`。 | FW-BS-008 |
| FW-BS-007 | `DirectBattleBootstrap.cs:37,49,149,160 Make/Initialize` | 外部便捷入口 -> FW-BS-008 | 默认只激活 slot 0/1；team 1/2；分别带 AI 标记。无额外战斗规则。 | FW-BS-008 |

### 2.2 整场 reset 与 roster/实体初始化

| ID | 文件/方法 | 前置/early-return | 字段读写、RNG、生成与副作用 | 下一边界 |
|---|---|---|---|---|
| FW-BS-008 | `DirectBattleBootstrap.cs:62 InitializeFromConfig` | config 固定有 8 slots；每个 active slot 若 OID 未加载则跳过实体生成，但 roster 状态仍保持 active | 先 FW-WR-006；写 `GameMode,Difficulty,StageIdx,RandomStage`；stage progression=`series 0,wave -1,round 0`；`StageProgressionValid=campaigns.Count>0`；mode2 时 `AiPhaseGate=1`；mode4 时 `ReserveOwnerValid=true`；`NeedClearInput=true`；清 camera override；填 reserve OID 表。 | roster 后进入出生循环 |
| FW-BS-008-B1 | 同上 83-102 | 每个 slot | `BattleSlotState=active?3:0`，`Team=(slot.Team==0?10+index:slot.Team)`，`Entity=-1`，`Oid=active?oid:-1`，label；active 即增加 `BattleSlotCount`，不依赖 OID 是否存在。 | 同一初始化阶段可见 |
| FW-BS-008-B2 | 同上 104-142 | inactive 或 `!HasChar(oid)` 跳过；`world.Spawn` 失败跳过 | 普通战：`x=width/4 + Rand()% (width/2)`；rematch：team 1 为 x=100，其他 team 为 width-100；两者 `z=Rand()%zRange+zMin`。`Spawn` 取全池首个 inactive 槽。写 roster entity slot、AI、frame/position prime、初始统计。 | 后续 slot 的 spawn 能看到已占用槽 |
| FW-BS-008-B3 | rematch only | `resultStartRematch=true` | entity `Y=-50,YInt=0,Unk344=battleTeam,HolderCopy=slotIndex`；PP 在 game mode 1 为 200，否则 500；`FallDamageDiv=Results.FallDamageDivForTeam(Unk344)`。 | 首 tick 输入清理前可见 |
| FW-BS-008-B4 | 方法尾 | 无 | `CharacterSync.SyncRuntimeFromLegacy(world)`；把 legacy/world 镜像写入 runtime 容器。 | 宿主可开始 tick |
| FW-BS-009 | `DirectBattleBootstrap.cs:224 InitializeBattleStats` | 每个成功出生实体 | `hpBonus=(difficulty*5+10)*10`；`HpMax=min(HpMax+bonus,Hp3)`，`Hp=HpMax`；PP 如上；`RespawnCount=0,HitStop=75,Vx=Vz=.1,Vy=0`；清 current/previous action+direction keys 与 attack/jump/defend cooldown。 | 同 tick 后续角色已 prime |
| FW-BS-010 | `DirectBattleBootstrap.cs:244 PrimeEntity` | 成功出生 | `Frame,WaitCounter,FrameWaitCounter,PrevFrame,PrevFrame2=0`；写 Z/ZInt、XInt/YInt。 | 同上 |
| FW-BS-011 | `DirectBattleBootstrap.cs:190 CaptureBattleConfig` | rematch | 以 `BattleSlotState!=0` 判 slot active，复制 team/OID；若 mapped entity 当前 active，则复制 AI，并以当前 `CharData.Oid` 覆盖 slot OID。inactive/已 free 的映射保留原 roster OID。 | FW-BS-006 |

### 2.3 Reset 保留/清除边界

`SimulationWorld.ResetBattleRuntime`（`SimulationWorld.Passes.cs:13`）是整场 rebuild 的唯一统一 reset：

| ID | 分支 | 精确语义 |
|---|---|---|
| FW-WR-006-A | runtime/global reset | `Runtime.Reset(default bounds)`；清 `ObjectCount,GameTick,InputPhase,FrameMod12,FrameToggle`、AI cache/gates、debug step flags、camera/override、stage spawn runtime、reserve owner、pause/input flags、F8、pending sounds。 |
| FW-WR-006-B | results | 普通重建 `Results.Reset()`；rematch `Results.PrepareForBattleRematch()`：离开 results phase，但保留结果 table/multiplier/committed 数据，只重置 live guard。 |
| FW-WR-006-C | arrays | 清 kill/damage stats、完整 `VRest[400,400]`、`ARest[400]`、roster/labels/reserve runtime；再把 roster OID/entity sentinel 设为 -1，重填 reserve OID 常量。 |
| FW-WR-006-D | pool | 对 400 个 `Objects[i]` 逐个 `Reset()` 并恢复 `Slot=i`；最后 `CharacterSync.SyncRuntimeFromLegacy`。 |
| FW-WR-006-E | 明确保留 | 不清 `CharData[1000]`、`LoadedOidOrder`、`CharCount`、`RuntimeStageCount`、`Bg`、`StageCampaigns`；不重新 `NtsdRng.Srand`。因此 rebootstrap/rematch 延续全局 RNG 序列和已加载数据。 |

## 3. World、固定槽、identity 与分类总账

### 3.1 Registry / pool

| ID | 文件/方法 | 分支与写入 | 契约/副作用 |
|---|---|---|---|
| FW-WR-001 | `SimulationWorld.cs:175 SimulationWorld()` | 构造 400 个 `Entity`；每个 `Reset` 后 `Slot=i` | 战斗中复用对象，不重新分配实体 CLR 对象。常量：`MaxObjects=400`, `MaxChars=1000`, `HitCandidateMax=20`（`NtsdConstants.cs:7-9`）。 |
| FW-WR-002 | `SimulationWorld.Registry.cs:14 HasChar`; `:17 GetChar` | OID 越界返回 false/null | `CharData` registry 以 OID 直接索引。 |
| FW-WR-003 | `:25 AllocChar` | 越界 null；首次分配才 append `LoadedOidOrder` 且 `CharCount++` | reset battle 不释放 DAT。解析失败发生在分配后时，该槽仍非 null，后续 `HasChar` 会为 true；这是当前权威行为。 |
| FW-WR-004 | `:39 Spawn` | 从 0 到 399 找首个 `!Active`，调用 `SpawnAt`; 无空槽返回 null | 没有类型分区；battle bootstrap 因从空池开始通常占低槽。 |
| FW-WR-005 | `:50 SpawnAt` | 槽越界 null；不检查槽是否 active；先 `entity.Reset()`，再 `Active=true,Slot,CharId,Team,Unk364,Facing,XYZ/int`；`CharData=GetChar`，可为 null；`EntityType=dat obj_type??0`,`ObjType=character/weapon runtime coarse type`,`Unk31C=WeaponHp`；`ObjectCount++` | 不清该槽 ARest/VRest；调用者若覆盖 active 槽会把 `ObjectCount` 多加 1。当前正式调用方都先找 inactive，但 API 本身没有保护。 |
| FW-WR-006 | `SimulationWorld.Passes.cs:13 ResetBattleRuntime` | 见 2.3 | 整场 pool reset。 |
| FW-WR-007 | `SimulationWorld.Registry.cs:82 FreeEntity` | 槽越界或已 inactive 无动作；否则整对象 `Reset()`、恢复 `Slot`、`ObjectCount--`（不低于 0） | 不清 ARest/VRest，不清 roster mapping，不主动修其他实体 link。 |
| FW-WR-008 | `SimulationWorld.QueryAndLinks.cs:12 ResetCooldowns` | 槽越界 return；清 `ARest[slot]` 和 `VRest[slot,*]`,`VRest[*,slot]` | 仅部分 spawn 路径显式调用。 |
| FW-WR-009 | `SimulationWorld.Registry.cs:95 GetEntity` | 越界 null；合法槽无论 active 与否都返回预构造 Entity | 调用者必须自行判断 `Active`。 |

固定槽约定来自可执行代码而非类型系统：

| 范围 | 使用点 | 语义 |
|---|---|---|
| `0..399` | `Spawn` | 通用首空槽；没有硬保留。 |
| `0..7` | camera preferred scan；8 roster mappings | 初始玩家/队伍常落低槽，但 roster 与 entity slot 是两张表。 |
| `0..9` | `RunN30InputTrigger`、部分 OID 7/8 合体约束 | 特殊玩家槽逻辑。 |
| `0..19` | OID 51/52 maintenance、stage factor | 主角色/固定战斗对象区的隐式语义。 |
| `20..399` | stage immediate spawn | stage 敌人从 20 起找首空槽。 |
| `50..399` | `FindFreeEffectSlot`、opoint、frame-logic 特效/clone | 动态武器、技能、特效区。 |

### 3.2 当前身份与分类

| ID | 文件/方法 | 读/写与分支 | 权威含义 |
|---|---|---|---|
| FW-ID-001 | `Entity.cs:11-147` | 大多数字段是 `NtsdEntityRuntime` 子容器的 property 镜像；`CharData` 是单独引用字段 | runtime 字段不是另一份独立实体；legacy property 和 runtime 实际指向同一存储。 |
| FW-ID-002 | `EntityCategory.cs:17 Get/:22 FromDatObjType` | 分类直接读 `entity.CharData?.ObjType`：null=>Effect；0=>Character；1/2/4/6=>Weapon；3=>Special；其他=>Effect | CLR 类型、`Identity.Category`、`EntityType` 都不决定正式 dispatch。当前 `CharData` 是分类真值。 |
| FW-ID-003 | `NtsdConstants.cs:35 ObjTypeRules` | DAT 0 是 character；所有非 0 被 `IsWeaponDat` 视为 weapon；runtime coarse `ObjType` 仅 0 character / 1 weapon；DAT 0 或 5 使用 character init semantics | 不得把 `Entity.ObjType` 当成完整 DAT obj_type。 |
| FW-ID-004 | `EntityDispatch.cs:12,34` | `CharData==null` return；按 FW-ID-002 dispatch Character/Weapon/Special/Effect 到 category logic | `CharData` 替换对同 tick 后续分派立即生效。 |
| FW-ID-005 | `GameTick.cs:1664 InitRuntimeIdentity` | 写 `CharData,CharId,EntityType,ObjType,Unk31C` | 不写 `Active,Slot,Team,Unk364,OwnerId,HP,Frame,link`；调用点必须决定保留哪些旧状态。 |
| FW-ID-006 | `GameTick.cs:1049 RunState501Pass` | state501 且 `Unk33C>=0` 且 replacement 存在：保存旧 OID 到 `Unk324`，替换 current identity、Frame=0；再把所有 active、`KillCount==selfSlot`、HP>0 的 child 同步到新 identity，child frame 按 Y 取 212/0 | parent/child identity 同 tick 在 frame logic 前变化。 |
| FW-ID-007 | `GameTick.cs:1615 RunStateSpecialPreCollision` | late pass：state9995 character=>OID50；state 4000..4999=>OID=state-4000；state 8000..8999=>OID=state-8000 且 `Unk318=140`；replacement 缺失则不变 | 发生在碰撞/命中之后的 late pass；新 identity 到下一 tick 早期 pass 才完整参与。 |
| FW-ID-008 | `GameTick.cs:1123/1214` | OID7/8 合并到 51 与拆回 | 合并保留主体对象并更换 identity，partner 直接失活；拆分复用记录的 partner slot 并 reset 后重建。 |
| FW-ID-009 | `CharacterSync.cs:35` + `NtsdEntityRuntime.cs:796` | 每 tick 前后把 world/runtime 镜像同步；`Identity.Category=(int)entity.Category` 由 current CharData 派生 | `Identity.Category` 是派生快照字段，不是 dispatch 输入。 |

`Team` 与 `Unk364` 不能合并：spawn 初始两者通常相同，但判队、结果、AI/目标过滤大量读取 `Unk364`；`Team` 主要用于持有/生成继承。`HolderCopy` 默认 99，bootstrap/rematch/opoint 各自写入不同语义。

### 3.3 Entity.Reset 精确边界

`Entity.Reset`（`Entity.cs:164`）调用 `NtsdEntityRuntime.Reset`（`NtsdEntityRuntime.cs:761`），随后强制 runtime coarse `ObjType=Character` 并 `CharData=null`。重要默认值：

- identity：inactive，slot/charId/owner=-1，team/entityType/category/Unk364=0，AI=false。
- transform/frame：位置、整数位置、frame/wait/prev/attacking/hitstop/delay 均 0，facing=0。
- motion：V=0，但 KnockbackVx/Vy/Vz 均为 `0.1`。
- links：Target/Held/ThrowGuard/Release/Stuck/Caught/Catcher/Holder/Picker=-1，`HolderCopy=99`，LinkState=0。
- stats：HP/HPMax/HP3/PP=500，KillCount/SpawnerSlot=-1，其余 0。
- transient：候选数组清零，Mp=0，Mp2/3/4=1000。
- residual：`Unk360=-1,Unk324/328/32C/33C=-1,Unk400/3FC=-1000`，其余状态/确认/计时/阻挡/weapon state 清零。
- input、presentation 也完整 reset；因此规范复用必须从 `Reset` 开始，再写业务身份。

## 4. Tick 外层入口总账

| ID | 文件/方法 | 分支/调用 | 可见性与副作用 |
|---|---|---|---|
| FW-DRV-001 | `BattleHostRuntime.cs:64 Start` | clock 未运行则启动；accumulator 预装一个 30Hz tick | 第一宿主帧可立即推进。 |
| FW-DRV-002 | `BattleHostRuntime.cs:141 StepFrame` | route-out request 先直接 rebootstrap 并不 render；否则按 FW-DRV-003 运行 0..N tick | 每个完成 tick 才允许 render。 |
| FW-DRV-003 | `BattleHostRuntime.cs:166 ComputeTicksToRun` | throttle off=>1；否则 delta clamp 0..250ms；`FixedTickMs=1000/30`；最多 4 tick | 固定步长，不把 host delta 传入规则。 |
| FW-DRV-004 | `SimulationTickDriver.cs:25 StepOneTick` | `tickIndex=world.GameTick+1`；lockstep/input-ready gate 不满足则整个方法 return | return 时不调用 provider before/after，不变更 world。 |
| FW-DRV-004-B1 | 同上 42-47 | provider `BeforeSimTick/GetFrameInput`；返回 TickIndex 不匹配则替换为空 frame；`ApplyFrameInput` | 输入在 GameTick 递增 GameTick 前写入 entity。 |
| FW-DRV-004-B2 | `SimulationTickDriver.cs:93 ApplyFrameInput` | player slot 越界、mapped entity null/inactive/AI 均跳过；其余调用 `InputRuntime.PollHumanInput` | 只面向 roster player slot，不遍历所有 entity。 |
| FW-DRV-004-B3 | 同上 49-73 | scheduler 完成后可生成 checksum，再 `AfterSimTick(tickIndex)` | 即便 GameTick 因 results/NeedClearInput/F1 early-return，scheduler 已返回，checksum/provider-after 仍执行。 |
| FW-DRV-005 | `BattleTickScheduler.cs:17 StepOneTick` | 纯转发所有观察 callback | 不改变顺序。 |
| FW-DRV-006 | `NtsdBattleTickSystem.cs:15 RunOneTick` | tick 前后各 `CharacterSync.SyncRuntimeFromLegacy(world)`，中间 `GameTick.Run` | 后同步结果可被 checksum/host/render 读取。 |
| FW-DRV-007 | `BattleHostRuntime.cs:193 RunSimulationTick` | driver 后立刻 audio consume；再处理 `PendingHostAction`；最后 host countdown/hostTicks | result rematch/rebootstrap 会在本 tick 后、下一 render 前整体重建 world。 |

## 5. GameTick.Run 完整正式 pass 顺序

下表是不可重排的可观察顺序。`next-visible` 表示本阶段写入第一次能被哪个下游阶段读取。

| ID | 权威位置 | 条件/调用 | 主要写入/副作用 | next-visible |
|---|---|---|---|---|
| FW-TK-001 | `GameTick.cs:31-39` | 每次已获准 tick | `GameTick++`,`InputPhase` 翻转，`FrameMod12`,`FrameToggle`；清 early-return/pause、`PendingSounds`、external-input flag | 本 tick 所有后续 pass |
| FW-TK-002 | `:41-46` | `Results.IsActive` | 调 `postCooldownInput`、`RunResultsTick` 后 return | 跳过全部普通战斗 pass；host 仍消费 sound/action |
| FW-TK-003 | `:48` | 普通战斗 | `RunCooldownsTick` | OID/input/碰撞均见更新后的 ARest/AttackExempt |
| FW-TK-004 | `:49-57` | step mode 2 转成 mode1 且 gate=1；非 wait gate 才调 `postCooldownInput` | step gate/header input hook | OID maintenance 以后 |
| FW-TK-005 | `:58` | always | `RunOid5152RuntimeMaintenance` | input/early/frame logic 可见合并/拆分结果 |
| FW-TK-006 | `:60-66` | `NeedClearInput` | 清 flag，`ClearBattleEntryInput`，return | 本 tick 不再推进；driver after/checksum 仍执行 |
| FW-TK-007 | `:68-70` | `GameTick>1` | `ApplyCharacterInputPass`; 再 `afterInputPass` | early states/frame logic |
| FW-TK-008 | `:72` | always | `RunEarlyStatePasses`（400/401、500、501） | frame logic/advance |
| FW-TK-009 | `:73-81` | 每个 `Active && CharData!=null && DAT非character && frame.HitFa>0` | `FrameRuntimePasses.RunFrameLogic`；之后 callback | 后续全实体 frame advance |
| FW-TK-010 | `:83-93` | 每个 active | 先清 action keys、direction keys，再 `RunFrameAdvance`；之后 callback | post-frame state 与交互 |
| FW-TK-011 | `:95` | always | state9998 free + respawn/free/spawn | 第一次 Z clamp/cpoint |
| FW-TK-012 | `:96` | characters | clamp Z，写 Z/ZInt | beforeCollect callback/cpoint |
| FW-TK-013 | `:97` | callback | observer only by contract | cpoint |
| FW-TK-014 | `:99` | always | `CPointRuntime.Run` | held sync/links/collision |
| FW-TK-015 | `:100` | always | held weapon sync | link validation |
| FW-TK-016 | `:101` | positive links | invalid target/reciprocal holder 时清 holder link/target/held slot | second Z clamp |
| FW-TK-017 | `:102` | characters | second Z clamp | held step/collision |
| FW-TK-018 | `:103` | always | held weapon step12 | snapshot |
| FW-TK-019 | `:105` | 每个 active | `PrevFrame2=Frame` | candidate collector / hit resolve 冻结读取 |
| FW-TK-020 | `:106-107` | always | collect candidates；callback | resolve loops |
| FW-TK-021 | `:108` | always | resolve character hits (HitResolve loop1) | natural drop 统计 active pool |
| FW-TK-022 | `:109` | always | natural random weapon drop | F8/object hits；新对象 active |
| FW-TK-023 | `:110` | `F8Pressed` | 消费 flag并随机生成武器 | object hits |
| FW-TK-024 | `:111` | always | resolve object hits (loop2) | bounds/stage/late |
| FW-TK-025 | `:113` | all active+CharData | preframe Z/X bounds、越界 free、camera/bg anim | stage progression/render hook |
| FW-TK-026 | `:114-116` | wait gate 且 `BattleStepFlag449048==0` | `f1SlowEarlyReturn=true`,`BattlePauseOverlay=1` | stage passes 和 pre-render hook仍执行 |
| FW-TK-027 | `:117` | always, helper 内另 gate | advance current stage wave | immediate spawn |
| FW-TK-028 | `:118` | always, helper 内另 gate | current wave immediate/deferred/refill spawns | pre-render callback |
| FW-TK-029 | `:119-120` | callback | `prePostprocessRender`; 随后 overlay=0 | early return gate |
| FW-TK-030 | `:121-125` | FW-TK-026 true | `BattleStepEarlyReturned=1` 后 return | 跳过 postprocess/late/tail/results；driver after/checksum仍执行 |
| FW-TK-031 | `:127-128` | normal | 聚合 knockback velocity，清 hit accumulators；callback | late per-entity |
| FW-TK-032 | `:129-130` | normal | 对 0..399 顺序执行 late entity update；callback | mode2 drop |
| FW-TK-033 | `:131` | `GameMode2==1` | 为所有候选武器生成 drop | postframe tail；新对象可被 tail 处理 |
| FW-TK-034 | `:132` | normal | debug kill-all-weapons、stats init、heal/catch timers、state1700 heal latch、清 hit carriers | result update |
| FW-TK-035 | `:133` | callback | observer hook | results flow |
| FW-TK-036 | `:134` | normal | game mode1 判两队存活并推进 battle end phase | 方法尾 |
| FW-TK-037 | `:135-136` | normal | `InitStats=0`,`GameMode2=0` | tick 后 sync/checksum/host |

## 6. GameTick helper / branch ledger

### 6.1 入口、输入、结果边界

| ID | 方法（行） | branches / early-return / 读写 / 副作用 |
|---|---|---|
| FW-H-001 | `IsStepWaitGate:157` | `BattleStepMode==1 && BattleStepGate44905C!=1`。 |
| FW-H-002 | `EntityUsesFrameLogicPass:161` | inactive/null/character=>false；frame null=>false；只接受 `HitFa>0`。 |
| FW-H-003 | `RunResultsTick:174` | 只读取 slot0 active，否则 slot1 active 作为 controller；按键仅取 pressed edge。详细菜单编辑排除。战斗相关分支：可写 committed reserve/fall damage/difficulty/stage selection，并设 `PendingHostAction=Rematch/BootstrapDirect`；尾部 `Results.Timer++` 且清 `InitStats/GameMode2`；可 enqueue UI sound。 |
| FW-H-004 | `Sync4V4ReserveOwner:450` | `ReserveOwnerValid=(GameMode==4)`；复制 results committed total/hp 到 reserve arrays。 |
| FW-H-005 | `ApplyResultsFallDamage:463` | 对 active DAT character 写 `FallDamageDiv`。 |
| FW-H-006 | `AdvanceResultsStageSelection:474` | stage 100->99；99->0；否则++，达到 runtime count 时转 100 且 RandomStage=1。 |
| FW-H-007 | `ResolveResultsController:502` | slot0 mapped active 优先，slot1 次之，否则 null。 |
| FW-H-008 | `Pressed:517`; `QueueSound:519` | edge=`prev0/current1`；sound event 含 cue/worldX/current tick。 |
| FW-H-009 | `UpdateBattleResultsFlow:529` | 非 mode1 或 results active return；只遍历 8 roster mappings，按 current `Unk364`（0 时 fallback roster team）聚合至最多两队。必须曾有两队同时 alive (`HadBoth`) 才允许结束。两边均 alive 时清 end phase；一边/双方 0 时 latch winner，phase 每 tick+1；到 11 激活 summary (`Phase=200`)。 |
| FW-H-010 | `ClearBattleEntryInput:608` | 只对 active DAT character 清 current/prev keys，再整个 input runtime reset。 |
| FW-H-011 | `ApplyCharacterInputPass:624` | 只对 active DAT character 调 `CharacterLogic.ApplyInput`。具体输入/AI 分区接力。 |

### 6.2 随机武器与 post-frame state

| ID | 方法（行） | branches / 读写 / RNG / 生命周期 |
|---|---|---|
| FW-H-012 | `RunNaturalRandomWeaponDrop:636` | 把所有 active DAT非character 都计为 weapon；>=4 return；否则先消耗 `Rand()%200`，仅 0 继续。候选为 loaded OID 100..199；122/123 额外 RNG/filter；选 OID 和 X/Z 共 5 次主要 RNG；空 effect slot/无候选 return。 |
| FW-H-013 | `RunF8WeaponDrop:699` | flag false return；true 时先消费 flag。扫描 100..199；122 会 RNG filter；选 OID+位置 RNG；slot>=50；生成一个。 |
| FW-H-014 | `RunMode2RandomWeaponDrop:747` | 仅 `GameMode2==1`；为 loaded 100..199 每个候选各生成一个，OID122类分支 RNG；每个位置 4 RNG；slot 用尽 break。 |
| FW-H-015 | `SpawnWeaponDrop:793` | char missing return；指定 slot `Reset->Active`，FW-ID-005，写 X/Z，Y=-500，速度0，OID122 HP=200；`ObjectCount++`，清该槽 cooldown matrix。 |
| FW-H-016 | `RunPostFrameAdvanceStatePasses:819` | 固定先 9998 cleanup，再 respawn。 |
| FW-H-017 | `RunState9998Cleanup:825` | active+frame state9998 => `FreeEntity`；同循环后续 slot 可立即复用。 |
| FW-H-018 | `RunRespawnPass:839` | 仅 active、frame state14、HP<=0、特定 kill/team/slot gate、HitStop 1..4。`RespawnCount<=0` 且 Hp2<2 => free；否则减 overlay，按同 `Unk364` character 平均位置并各用 RNG 偏移，重置 HP/PP/frame212/Y=-300。`RespawnCount>0` 分支恢复 overlay/HP、frame219，并可在 >=50 spawn OID998 effect；生成清 cooldown。 |

### 6.3 early states、合体/拆分、bounds/stat

| ID | 方法（行） | branches / 读写 / 生命周期 |
|---|---|---|
| FW-H-019 | `RunEarlyStatePasses:955` | 固定 400/401 -> 500 -> 501。 |
| FW-H-020 | `RunState400401Pass:962` | 每隔一个 FrameToggle 执行；state400 找最近敌方 active living character，state401 找最远同队；无目标时 Y/V=0；有目标时把 entity 放到目标 Z+1、按 facing X 偏移 120/60，Y/V=0。 |
| FW-H-021 | `RunState500Pass:1032` | state500 且 `Unk33C==-1 || Unk324>=0` => Frame=0。 |
| FW-H-022 | `RunState501Pass:1049` | 见 FW-ID-006。 |
| FW-H-023 | `RunOid5152RuntimeMaintenance:1093` | 只扫 slot<20；每 tick `Unk338>0`--；OID7/8 尝试 merge；OID51、`Unk328==1` 且 frame 不在9..260、cooldown结束则 split。 |
| FW-H-024 | `TryMergeOid7Or8Into51:1123` | self/partner HP、state2、team、cooldown、距离、slot 位置、多模式阈值均需通过；合并 HP/HPMax，主体 frame290/位置/残留字段，identity=>51,PP500；清 partner cooldown后 `partner.Active=false,CharData=null,ObjectCount--`，不整对象 Reset。 |
| FW-H-025 | `SplitOid51BackToPair:1214` | self/partner CharData 或 partner slot 无效即 return（可能已经把 self identity 改回且设置 cooldown）；成功时 partner 整对象 reset 重建，HP 对半，位置/队伍复制、双方 frame112/PP0、`ObjectCount++`、清 partner cooldown。 |
| FW-H-026 | `RunCooldownsTick:1265` | active 槽 `ARest>0`--；`AttackExempt<=0` 或 CharData null 跳过；无 itr 清 exempt；state1001 held 分支仅在 holder active+CharData 且 holder frame无有效 attacking wpoint 时清。VRest 的逐对 decrement 在 candidate collector，下游接力。 |
| FW-H-027 | `ApplyPreframeBounds:1301` | DAT type3 用 visual Z offset clamp；character clamp Z 到 stage；其余允许±1。type3 X 超 stage±300 free；character slot>=20 clamp [-100,width+100]，slot<20 按 team/override clamp；OID122/123 weapon 特例 [10,width-10]；其他非character在地面且 X越界 free。最后更新 camera/bg。 |
| FW-H-028 | `UpdateCameraAndBgAnimation:1400` | width>794 时优先 active living character slot<8，以 state14/current facing 算 target；没有则所有 living character；仍无则 fallback 800。camera 以平滑整数速度追 target，受 max/override clamp；每个 bg layer `Cc>0` 时 anim counter 循环。width<=794 清 camera。 |
| FW-H-029 | `RegeneratePreCollisionStats:1474` | active DAT character；非 wait gate 时每12 tick HP+1；WeaponCount<0 每12 tick按 FallDamageDiv 自伤并增 ComboCountVic；每3 tick且 PP/kill/hitstop gate通过按 HP（OID51/52再/2）回 PP。虽方法名含 PreCollision，实际由 late pass 在本 tick 碰撞/命中之后调用。 |
| FW-H-030 | `SnapshotPrevFrame2:1521` | active entity `PrevFrame2=Frame`。 |

### 6.4 late entity、opoint、transition effects、tail

| ID | 方法（行） | branches / 读写 / RNG / 生命周期 |
|---|---|---|
| FW-H-031 | `RunLatePerEntityUpdatePass:1533` | 0..399 固定顺序调用 FW-H-032；较低槽在此阶段生成的 >=50 新对象，若其 slot 尚未遍历到，会在同一 late pass 被继续处理。 |
| FW-H-032 | `RunLateEntityUpdate:1539` | inactive/null return；依次 state-special identity、regen、`FrameTickRuntime.Tick`；frame 1100/1200 relay 分支把 child hitstop relay并 self frame0 return；frame<0或>=400先 Frame=0 再 free；死亡 character 强制 drop weapon/死亡弹起；随后 opoint spawn；weapon `Unk31C<0` enqueue broken sound+free；N30 trigger；transition effects；若仍 active `PrevFrame=Frame`。每次可能释放/替换后都重查 Active/CharData。 |
| FW-H-033 | `RunStateSpecialPreCollision:1615` | 见 FW-ID-007。 |
| FW-H-034 | `ApplyDeathBounceFrame:1673` | Frame186，Vy/KnockbackVy=-3，Y/YInt=-1。 |
| FW-H-035 | `QueueBrokenWeaponSound:1682` | broken cue 空则 return；否则 enqueue current X/tick。 |
| FW-H-036 | `RunN30InputTrigger:1696` | 只 slot<10、living character；识别 history `[9,0,9,0]=>100`,`[9,9,9,9]=>102`,`[9,5,9,5]=>104`；先清 history tail，再要求 effect slot/OID998。spawn effect 后遍历同队 living character：100 为每人两次 RNG 写目标坐标；102/104 开关 history gate。 |
| FW-H-037 | `SpawnStateTransitionEffects:1773` | 从 current CharData 读取 PrevFrame/Frame state；离开 state13/frame200 时分支1；prev state18/19，离开时 count7，否则 1/4 RNG 生成1。 |
| FW-H-038 | `SpawnTransitionEffectBranch1:1804` | OID999 missing return；先 SFX_066；最多15个 >=50 effect，每个 reset/identity/随机位置速度/frame，`ObjectCount++` 并清 cooldown；slot用尽 break。 |
| FW-H-039 | `SpawnTransitionEffectBranch2:1842` | 类似分支1，生成 count 个，位置/速度/frame RNG；slot用尽 break。 |
| FW-H-040 | `FindFreeEffectSlot:1873` | 50..399 首 inactive，否则 -1。 |
| FW-H-041 | `ResetCooldownsSlot:1884` | 与 world.ResetCooldowns 相同。 |
| FW-H-042 | `RunEntityPostframeTail:1897` | active entity：`GameMode2==2` 且 DAT非character => `Unk31C=-1`（本 tick 已过 broken-weapon free，通常下一 tick late free）；`InitStats==1` 强制 HP/PP=500；推进 HealTimer/CatchTimer；current state1700 latch HealTimer=1100；最后 `ClearHitCandidateCarriers`（不清候选数组本体）。 |
| FW-H-043 | `ClampCharactersToStageZ:1961` | active DAT character Z clamp并写 ZInt。 |
| FW-H-044 | `ApplyFramePostProcess:1980` | active 且 FrameDelay==0；HitCount>0 时按 `2/(count+1)` 把累积 knockback 写 V，随后清 HitCount/Knockback；否则至少清 KnockbackVx；总清 Y/Z knockback。 |
| FW-H-045 | `ValidatePositiveLinks:2009` | 只看 `LinkState>0` holder；target 越界或 target inactive/`target.HolderIdx!=holderSlot` 时清 holder LinkState/TargetIdx/HeldWeaponSlot；不修 target 反向字段。 |

### 6.5 Stage progression / spawn

| ID | 方法（行） | branches / 读写 / RNG / 生命周期 |
|---|---|---|
| FW-H-046 | `StageProgressionCurrentPhase:2036` | 按 `StageSeriesIdx` 找 campaign；WaveIdx 越界/null。 |
| FW-H-047 | `StageProgressionCanAdvanceWave:2053` | campaign 不存在/已末波 false；否则 `WaveIdx==-1 || waveReady`。 |
| FW-H-048 | `StageProgressionAdvanceWave:2068` | FW-H-047 false return；否则 `WaveIdx++`。 |
| FW-H-049 | `StageSpawnEntryHp:2076` | spawn HP>0 用配置，否则500。 |
| FW-H-050 | `SpawnStageImmediateEntrySlot:2079` | spawn id<0、无 >=20 空槽、CharData missing 均 -1；通过 `SpawnAt`（不清 cooldown），再写 init semantics/team/holder、随机 X/Z、HP/PP/frame/facing；OID122 HP=200。`SpawnAt` 已 `ObjectCount++`。 |
| FW-H-051 | `TrySpawnStageImmediateEntry:2150` | FW-H-050>=0。 |
| FW-H-052 | `StageSpawnEntryFactor:2153` | 只数 slot<20 active character；OID51额外+1，OID52额外+2。 |
| FW-H-053 | `ResetStageSpawnRuntime:2173` | wave=-1，清四组 runtime lists。 |
| FW-H-054 | `EnsureCurrentWavePositiveStageRuntime:2182` | wave/counts 已匹配则 return；否则重建每个 spawn 最多40槽的数组；ratio<=0跳过；entryCount=`factor*ratio` clamp0..40；targetTotal=`times*ratio*factor` floor且>=0。 |
| FW-H-055 | `RefillCurrentWavePositiveStageSpawns:2226` | runtime wave/count mismatch return；先把失活/错误OID的跟踪槽设-1；再在未达 targetTotal 时补 spawn，失败 break。 |
| FW-H-056 | `CurrentWaveStageSpawnsCleared:2274` | 对本 phase 每个 id 扫 slot>=20；任意 active同OID=>false；未按该 phase 实例 ownership 区分。 |
| FW-H-057 | `CurrentWaveStageSpawnProducersInitialized:2295` | phase 有 ratio<=0 immediate 但 `WaveApplied!=WaveIdx` false；有 ratio>0 positive 但 deferred marker不等 false。 |
| FW-H-058 | `ApplyCurrentWavePhaseAdvance:2317` | progression invalid、mode非1/2、`WaveIdx<0`、phase null、producer未初始化、未清场、不可 advance 均 return；成功 WaveIdx++，应用 next bound，清 marker/runtime。 |
| FW-H-059 | `ApplyCurrentWaveImmediateStageSpawns:2350` | invalid/mode/WaveIdx<0 return；phase null reset runtime；未应用 immediate 时生成 ratio<=0 项；构建 positive runtime；未应用 deferred 时按 entryCount 首批 spawn；最后 refill。 |

## 7. 生命周期与同 tick 可见性矩阵

| ID | 路径 | reset? | cooldown matrix? | CharData 清理? | `ObjectCount` | 同 tick 后续可见性 |
|---|---|---:|---:|---:|---:|---|
| FW-LC-001 | `Spawn/SpawnAt` | 是 | 否 | 重写，可为 null | +1 | 立刻 active；若在低槽遍历前生成可被同 pass处理 |
| FW-LC-002 | weapon/effect/N30/transition custom spawn | 是 | 是 | 重写且要求 loaded | +1 | 立刻 active |
| FW-LC-003 | `FrameTick.SpawnFromOpoint:333` | 是 | 是 | 重写 | caller 在成功后+1 | late pass 中生成 >=50；可能同一 late pass继续 Tick |
| FW-LC-004 | stage spawn `SpawnAt` | 是 | **否** | 重写 | +1 | 发生在 hit/bounds 后、render hook 前；本 tick不参与已结束的碰撞，但正常路径会继续进入 postprocess/late；F1 slow early-return 时 late 被截断 |
| FW-LC-005 | `FreeEntity` | 是 | 否 | 是 | -1 | 本 tick后续遍历看到 inactive；槽可立即复用 |
| FW-LC-006 | state9998/bounds/invalid-frame/broken weapon/hit OIDC9 | 通过 `FreeEntity` | 否 | 是 | -1 | 依调用时点决定是否已参与本 tick早期 pass |
| FW-LC-007 | OID51 merge partner | 否 | 是（先清） | 是 | -1 | 其余 runtime 残留，但 inactive；split 前会 reset |
| FW-LC-008 | `FrameAdvance` hitFa cases 5/8/11/6/9/13 | 否 | 否（源对象） | **否** | -1 | 直接 inactive，旧字段/CharData残留；槽下次规范 spawn reset |
| FW-LC-009 | whole battle reset | 是，全部400槽 | 全矩阵清 | 是 | 归0 | loaded DAT/Bg/campaign/RNG保留 |

`ObjectCount` 在当前权威 `src` 中只写不读：它是镜像计数，不是 spawn gate 或结果判定依据。真实活性以每槽 `Active` 为准。

### FrameAdvance 直接失活分支（必须单独对齐）

- `FrameAdvance.cs:247 RunFrameLogicCase5`：生成 OID219 后 source `Active=false`，不清 CharData/cooldown。
- `:315 RunFrameLogicCase8`：OID225 数据缺失或生成结束后 source 直接失活。
- `:519 RunFrameLogicCase11`：批量生成 OID211 后 source 直接失活。
- `:667 RunFrameLogicCase6Or9`：批量生成 OID220/221/222 后 source 直接失活。
- `:774 RunFrameLogicCase13`：无槽/OID228缺失/成功生成后 source 直接失活。
- 所有上述分支都会把 `ObjectCount` 最多减到0，但不会 `Entity.Reset`；这是正式权威行为，不得擅自统一成全 reset 后再声称等价。

## 8. Results / 战斗结束与宿主边界

| ID | 入口 | 条件与状态变化 | 结束/重启边界 |
|---|---|---|---|
| FW-END-001 | `UpdateBattleResultsFlow:529` | 仅 mode1；两队必须曾同时 alive；清场后 latch winner，连续 11 个正常 tick 后 `Results.ActivateSummary` 写 `Phase=200` | 激活发生在普通 tick 最尾部；该 tick 已完整执行战斗 pass。 |
| FW-END-002 | 下一 tick `GameTick.Run:41` | tick header 仍递增/清 pending sound；发现 results active 后只 `RunResultsTick` 并 return | 角色不再 frame/collision/late；driver checksum/host仍执行。 |
| FW-END-003 | `RunResultsTick` result settings | UI 细节排除；与战斗相关仅 `PendingHostAction`、difficulty/stage/reserve/fall damage | host action 在同 tick `Driver.StepOneTick` 返回后处理。 |
| FW-END-004 | `BattleHostRuntime.cs:225 RematchBattle` | 清 route/countdown/pressed/audio/overlay；capture 当前 roster config；`InitializeFromBattleConfig(...,true)` | 整场 reset+重新 spawn；保留 results table/multiplier 与 RNG continuation；触发 `WorldReloaded`。 |
| FW-END-005 | `BattleHostRuntime.cs:239 BootstrapDirectBattle` | 清宿主状态；用环境/host config 重新 load bg并初始化 | 普通 reset results；仍不 reseed RNG；触发 `WorldReloaded`。 |
| FW-END-006 | `BattleHostRuntime.cs:93 F4` | step mode=0、AI gate=0、exit countdown=350；mode2/3 设 route-out request | 下一 `StepFrame` 直接 BootstrapDirectBattle，不运行 tick，不 render。countdown 本身没有 close 分支。 |
| FW-END-007 | `SdlBattleHost.cs:56-86` | `ShouldClose` 分支存在，但当前 `BattleHostRuntime.RunSimulationTick` 从未构造 `ShouldClose=true` | 正式关闭来自 SDL_QUIT；Escape key当前被忽略。菜单/UI退出排除。 |

## 9. Snapshot / runtime 镜像边界

| ID | 方法 | 精确边界 |
|---|---|---|
| FW-SN-001 | `SimulationWorld.CreateSnapshot/ApplySnapshot:186,191` | 转发 `CharacterSync`。 |
| FW-SN-002 | `CharacterSync.CreateSnapshot:43` | 复制 `BattleRuntimeState` 与 400 个 `NtsdEntityRuntime`；runtime clone 明确不复制 transient scratch。 |
| FW-SN-003 | `CharacterSync.ApplySnapshot:52` | Apply world runtime，再 Apply 每个 entity runtime；`NtsdEntityRuntime.ApplyTo` 不写 `Entity.CharData` 引用。 |
| FW-SN-004 | `CharacterSync.CreateChecksum:76` | 先 sync；FNV hash world runtime 与所有 400 entity runtime，包括 inactive 槽；不 hash CharData 内容/引用、VRest/ARest、Results、stage spawn lists、global RNG seed/callcount。 |

因此 snapshot 只恢复 runtime 数值镜像，不足以单独恢复 current `CharData` identity、冷却矩阵、results、stage producer tracking 或 RNG。调用者若把它作为完整 world rollback，需要另一个权威契约；当前框架中没有该闭合调用。

## 10. 未解析调用与分区接力点

这些不是“未知是否存在”的调用，而是已定位正式入口、尚需对应专项 ledger 闭合的范围：

| ID | 正式入口 | 本账已确认 | 接力必须闭合 |
|---|---|---|---|
| FW-X-001 | `InputRuntime.PollHumanInput`; `CharacterLogic.ApplyInput` | driver 和 GameTick 调用时点、entity gate | key edge/history/combo/AI消费、字段全读写、RNG |
| FW-X-002 | `FrameRuntimePasses -> EntityDispatch -> CharacterLogic/FrameAdvance` | 分类依据、调用顺序、直接失活/生成分支 | 每个 category frame logic/advance 的完整分支、Physics、frame transistor |
| FW-X-003 | `CPointRuntime.Run` | 位于第二次 Z clamp之前、碰撞收集之前 | cpoint 两 pass、identity swap、抓取/投掷/link字段 |
| FW-X-004 | `WeaponPointRuntime.SyncHeldWeapons/RunHeldWeaponStep12/ForceDrop` | 顺序和死亡 drop 调用点 | holder/weapon 双向字段、释放同 tick 边界 |
| FW-X-005 | `CollisionCollect.CollectCandidates` | `PrevFrame2` freeze 后调用，loop resolve前 | pair顺序、VRest decrement、candidate arrays/itr index |
| FW-X-006 | `HitResolve.ResolveLoop1/Loop2` | character loop -> random/F8 drop -> object loop | 全 kind/effect、stats/sound/free、AbortRemainingHitPairs |
| FW-X-007 | `FrameTick.Tick/ProcessOpointSpawn` | late per-entity 时点、spawn pool/reset/cooldown/同pass再处理 | frame wait/next/mp/sound、opoint kind/facing/multi-spawn全部分支 |
| FW-X-008 | stage DAT model/parser | runtime consumer helper已闭合 | campaign id/wave initial contract、ratio/times/bound数据语义 |

## 11. 权威链内部待确认点（不能默认为实现意图）

以下均由当前唯一权威源码直接得出，应在更高层验收中保留为“待确认/需证据”，不能用经验补写：

1. **首波不可达**：Program、reset、DirectBattleBootstrap 都把 `StageProgression.WaveIdx=-1`。唯一 `WaveIdx++` 在 `StageProgressionAdvanceWave`，唯一调用方 `ApplyCurrentWavePhaseAdvance` 却在 `WaveIdx<0` 时先 return。全仓没有其他写入，因此现有正式链不会从 -1 进入 wave 0，stage spawn helpers 在正式 bootstrap 后不可达。
2. **`Paused` 不门控 tick**：字段仅 reset/snapshot/hash，没有 driver/GameTick 读取；设置 `Paused=true` 也不会阻止推进。
3. **RNG 与 runtime seed 分离**：Program 只写全局 `NtsdRng.Seed`；`BattleRuntimeState.Match.Seed` 没有从它赋值。checksum hash `Match.Seed`，却不 hash真实全局 RNG seed/callcount。rematch/rebootstrap 不 reseed。
4. **snapshot 非完整 world restore**：不恢复 `CharData`、VRest/ARest、Results、global RNG、stage runtime lists；仅靠当前 `ApplySnapshot` 无法保证后续分派/RNG完全复现。
5. **cooldown 清理不统一**：`SpawnAt/FreeEntity/stage spawn` 不清 ARest/VRest；opoint/custom effect spawn会清。必须逐路径保持，不能抽象成“所有入池/出池统一清理”而不改变行为。
6. **直接失活保留残留字段**：FrameAdvance 多个 hitFa 分支只 `Active=false`，合体 partner 只额外清 `CharData`。后续系统通常以 Active gate跳过，但 snapshot/checksum仍 hash inactive槽全部 runtime字段。
7. **无效 OID 的 active roster**：bootstrap 先把 slot state设3并计入 `BattleSlotCount`，再因 OID未加载跳过 entity；结果聚合跳过 null mapped entity。该状态不会自动修复。
8. **`ObjectCount` 可与 Active 数不一致**：`SpawnAt`不保护覆盖 active；各直接失活路径手工减计数；该计数当前不被规则读取。验收应分别比较计数和每槽 Active，不要互相推导。

## 12. 覆盖计数与完成边界

- 已读取权威文件：42（41 个完整/结构性读取，另有 `HitResolve.cs` 的释放片段）。
- 唯一 ID 定义：172，机械检查无重复。分布为 `FW-BS` 15、`FW-WR` 14、`FW-ID` 9、`FW-DRV` 10、`FW-TK` 37、`FW-H` 59、`FW-LC` 9、`FW-END` 7、`FW-SN` 4、`FW-X` 8；`-B`/`-A..E` 是同方法内部可执行分支。
- GameTick 正式普通战斗阶段：37 个顺序点，其中 3 个早退边界（results、NeedClearInput、F1 slow return）。
- 生命周期入口：9 类；明确直接 `Active=false` 而不全 reset 的 FrameAdvance 方法 5 个（含多个内部失败/成功分支），另有 OID51 merge 1 类。
- 未解析调用：8 个专项接力簇，均已给正式入口和调用时点；没有未定位的框架级动态调用。
- 本账可证明“权威框架、顺序、registry、identity、lifecycle、战斗结束边界已建模”；不能据此单独证明输入/帧/碰撞/命中每条内部规则已经完整建模，更不能据此宣称目标工程已对齐。
