# GameTick / Physics 全量静态对齐审计（2026-07-17）

## 1. 口径与结论

- 唯一权威：`J:\QQFile\NTSD2.4\ntsd_release_C#`。
- Unity 对照范围：`Assets/NTSD/Scripts/Simulation/`、`Assets/NTSD/Scripts/Animation/Character/`、`Assets/NTSD/Scripts/Animation/LF2Objects/`。
- 禁止并且未读取 C++、反编译、伪代码或历史旧实现作为结论依据。
- 本报告是只读静态审计；没有修改生产代码或主进度文档。
- T8 默认 `stage.dat` 部署未计为差异。stage 逻辑本身仍以现有结构化测试夹具做了静态比较。
- F8、Mode2 随机武器、step-wait 是调试/特殊模式，单列为排除项，不计正式对局确认差异。
- 结算菜单 `RunResultsTick` 属于默认战斗范围外；只确认 Unity 正式战斗 tick 未实现该菜单流，不计入本批战斗逻辑差异。

审计覆盖：

- `Simulation/GameTick.cs`：`Run` 正式对局主干、输入/掉落、early state、冷却、preframe、late update、postframe、stage wave 的全部正式分支，覆盖 100%。
- `Frame/Physics.cs`：`Update` 到 `ResetWeaponCountOutsideState12` 的全部水平、深度、摩擦、重力、空中选帧、角色/四类武器/oid999 落地和整数同步分支，覆盖 100%。
- 为防止脱离调用者误判，额外追踪 `Frame/FrameAdvance.cs:13-47`、Unity `LF2Entity`/`LF2Character`/`LF2WeaponBase` 的实际 frame-advance 入口。

本分区共确认 **21 个差异簇**：GameTick/stage 15 个，Physics 6 个。另有 3 个需要对拍或数据契约补齐后才能关闭的风险。此前已知的 `NeedClearInput`、frame advance 前清键和自然武器掉落差异也在本报告中重新实锤，不算“已修复”。

## 2. GameTick 确认差异（15）

| ID | 权威 C# | Unity | 前置条件与差异 | 顺序/字段/RNG 后果 |
|---|---|---|---|---|
| GT-01 | `GameTick.Run:66-72`、`ClearBattleEntryInput:608` | `NTSDBattleTickSystem.RunReleaseTick:17-26`；无 `NeedClearInput` 生产字段/分支 | 进入战斗首 tick 或明确要求清输入 | C# 清 current/previous/runtime input 后整 tick 提前返回；Unity继续输入、frame、碰撞。**确认差异**。 |
| GT-02 | `GameTick.Run:99-101` | `NTSDBattleTickSystem.RunFrameAdvancePhase:28-45`、`SimulationWorld.SerialTickAll:327` | 每个 active slot 开始 frame advance 前 | C# 先清 runtime action/directional keys；Unity未在该边界清键。frame 内读取旧键时会不同。**确认差异**。 |
| GT-03 | `RunNaturalRandomWeaponDrop:636-697`、`SpawnWeaponDrop:793` | `SimulationWorld.RandomWeaponDropTickAll:1099-1191` | weaponCount<4 且 RNG gate=0 | C# 计数所有 current-DAT non-character；按 `LoadedOidOrder` 枚举；122/123 先消耗特殊 RNG 且 game mode 1..4 排除；先找 free slot，再选候选/位置；初始 frame=0，oid122 HP=200。Unity只计 `LF2WeaponBase` type1/2/4/6，按 OID 100..199，漏 game-mode gate，在耗完后续 RNG 后才由 factory 尝试分槽，扫描“飞行帧”作为 action，未显式落实 oid122 HP=200，并以 scene stage snapshot 替代 C# `XMaxOverride/Bg`。**确认差异簇**。 |
| GT-04 | `RunState501Pass:1049-1090` | `SimulationWorld.RunEarlyState501Specials:594-644` | state501，transform target有效；child `KillCount` 关联 | C# child 只接受 `KillCount == source runtime slot`，并同步 CharData/CharId/ObjType/EntityType；Unity额外接受 StableId，并只换 `FrameCache/ObjectId`，依赖稍后 snapshot 派生 type。StableId!=slot 时会误变身无关 child；变身当拍字段契约不同。**确认差异**。 |
| GT-05 | `ApplyPreframeBounds:1301-1398`、`ClampCharactersToStageZ:1961` | `SimulationWorld.StageRender.ApplyPreFrameBoundsAll:69-97`、`ClampCharacterZToStageBoundsAll:50-67`；`LF2Entity.ApplyPreFrameZBounds:2078` 空实现；仅 `LF2Character:1592` override | 任意 weapon/special/other，或 current DAT 与 CLR shell 不同 | C# 全部 active slot 按 current CharData type 做 Z clamp（type3 用 logic-Z，其他 nonchar 为 zmin-1..zmax+1，char为 zmin..zmax）并写 ZInt。Unity只有 CLR `LF2Character` 做 char clamp；所有非角色无 preframe Z；character shell 变成 nonchar 仍被当 char，nonchar shell 变成 char则不 clamp。**确认差异**。 |
| GT-06 | `RegeneratePreCollisionStats:1474-1519`，由 `RunLateEntityUpdate:1539` 按 current CharData character 调用 | 仅 `LF2Character.RunPreCollisionRecoveryPhase:1600` override；基类 `LF2Entity:2175` 空实现 | state transform 造成 current DAT/CLR shell 交叉 | C# 恢复 HP/负 WeaponCount 伤害/PP 的资格由 current DAT type 决定；Unity由 CLR shell 决定。会漏跑或多跑恢复。**确认差异**。 |
| GT-07 | `RunLateEntityUpdate:1558-1583` 的 character death/drop/bounce 与 weapon `Unk31C<0` cleanup | 仅 `LF2Character.RunLateDeathOpointPreCleanupPhase:1346` 和 `LF2WeaponBase.TryRunLatePostOpointCleanupPhase:539` override；基类 `LF2Entity:3481-3483` 为空 | current DAT 与 CLR shell 交叉 | C# 按 current DAT 类型；Unity按 CLR shell。变身后会漏掉死亡放武器/弹地或武器破碎清理，也可能对已变成非角色的 character shell 多跑。**确认差异**。 |
| GT-08 | `RunLateEntityUpdate:1556-1568` | `SimulationWorld.HandleLateFrameTickExit:807-851` | late frame tick 得到 frame 1100..1299 | C# 先识别 frame/100==11或12，广播 `HitStop=1100-frame`，自身 frame=0 并保留实体；Unity在 `:813` 先用 `frameId>=400` 直接 Free，后面的 relay 分支 `:826` 永远不可达。**确认差异**。 |
| GT-09 | `RunState9998Cleanup:825` 只位于 frame advance 后；late tick 后没有第二次 state9998 cleanup | `HandleLateFrameTickExit:819-824` | late frame tick 本拍新进入一个合法 frameId、其 state=9998 | C# 实体保留到下一 tick 的 post-frame-advance cleanup；Unity本 tick late 立即 Free，生命周期少一拍。**确认差异**。 |
| GT-10 | `RunStateSpecialPreCollision:1615-1662`：变更 identity 后直接 `entity.Frame=0`，8000 分支另写 HitStop=140 | `LF2Entity.ApplyStateDataTransform:3670-3690` 调 `ImmediateFrame(0)` | state9995、4000..4999、8000..8999 | Unity `ImmediateFrame` 还写 PN、清 Attacking、同步 transistor，并可能触发表现；C# 这里只写 Frame（外加 identity/Unk31C/可选 HitStop）。**确认差异**。 |
| GT-11 | 正式 C# `GameTick.cs`/调用链无 state9996 分支 | `LF2Character.RunState9996SpecialPreCollision:1280-1336`，由 `RunStateSpecialPreCollision:744-750` 调用 | CLR character，state9996 且 Attacking==1 | Unity额外生成 5 个 oid217/218，消耗多次全局 RNG，并写 attackExempt/速度/位置；该逻辑只引用被禁止作为权威的旧来源，正式 C# 无对应行为。**确认多余逻辑**。 |
| GT-12 | `SpawnTransitionEffectBranch1:1804-1840`、Branch2`:1842-1871` 全程 double | `LF2Entity.SpawnTransitionEffectBranch1:3535-3558`、Branch2`:3560-3582` 将 X/Y/Vx/Vy 强转 float | state13/200 退出或 state18/19 碎片 | RNG 次数/顺序现已相同，但 Unity在写 runtime 前丢失 double 精度，逐 tick trace 和后续位置可能分叉。**确认差异**。 |
| GT-13 | `RunEntityPostframeTail:1897-1959` 只做 Mode2、InitStats、heal/catch、state1700、candidate clear | `SimulationWorld.EntityPostFrameTailAll:854-906` 之后额外 `RunReleaseEntityCleanupTail:908-939` | 任意 current-DAT non-character HP<=0，或 dead character state14 等待超阈值 | 正式 C# 无这个通用尾部销毁 pass；Unity会额外 Free 非角色和部分死亡角色。会改变实体寿命、槽位复用和后续 RNG/碰撞。**确认多余逻辑**。 |
| GT-14 | `SpawnStageImmediateEntrySlot:2079-2148` 先取首个 free slot `20..399`，无槽则不消耗生成 RNG | `SimulationWorld.StageWave.SpawnStageImmediateEntrySlot:436-484` 先算随机 X/Z，factory/registry 动态槽从 `DynamicRuntimeSlotStart=50` 分配 | stage fixture 有 spawn；不涉及默认 stage.dat 部署 | Unity固定槽编号与 C# 不同，且满槽时 RNG 调用边界不同。slot 会进入 holder/owner/vrest/checksum。**确认差异**。 |
| GT-15 | `StageSpawnEntryFactor:2153-2171` 只计 active slots 0..19 | `SimulationWorld.StageWave.StageSpawnEntryFactor:139-165` 用 `GetAllEntities`，未按 `IsActiveForCurrentPass` 排除 `OidMergeDormant/PendingFlushDestroy` | oid7/8 合体或固定槽有 dormant/pending 实体，同时 stage 正比例 spawn | C# 合体只计 active oid51（权重2）；Unity还会计 dormant partner，放大 entryCount/targetTotal。**确认差异**。 |

## 3. Physics 确认差异（6）

| ID | 权威 C# | Unity | 前置条件与差异 | 可观察后果 |
|---|---|---|---|---|
| PH-01 | `Physics.UpdateHorizontal/UpdateDepth:33-60` 只按 block flags 决定 X/Z 位移 | `CharacterMechanics.Step:156-232` 在位移后额外调用 scene `IsMovementWalkable`，失败就回滚 X/Z；native/shared character 均注入该 callback | 场景存在 BoundaryWall 且新点不在 polygon | C# 不做该 point-polygon 回滚，边界由既定 pass 处理；Unity运动轨迹被额外改变。**确认多余逻辑**。 |
| PH-02 | `UpdateVertical:124`、`ApplyGroundResolve:220` 对 `-0.0001/0.0001` 使用 double 且 `(0,0.0001]` 不 clamp；IronBall 只看 newY>0.0001 | `CharacterMechanics.Step:201-216`、`WeaponDynamics:268-272` 用 float epsilon/`Y>0` 即 clamp，统一 `crossedGround` 还要求 oldVy>0 | 极小 Y 边界、外部/opoint 生成 below-ground Y、IronBall oldVy<=0 | 空中重力、Y 真值和 IronBall landing 分支不同。**确认差异簇**。特别地，grounded state2000 stationary 的正确 C# 结果是保持 frame，不是 frame20：newY=0 在 `Physics.cs:230` 已 return。 |
| PH-03 | `Physics.cs:44,46` 用 double `0.2`；`:242,397,415` 用 `0.3333333333333333` | `NTSDGlobal.WeaponExtraVxFactor` 是 `0.2f`，`LF2Entity:4755/4757`、`LF2Weapon:170/172` 乘 float；character landing `LF2Entity:4960/4988/5012` 用 `1f/3f` | oid120/101/type4 横移，或角色落地衰减 Vx | double runtime 被 float 常量污染，第一拍即可产生位级不同，随后 X/碰撞分叉。**确认差异**。 |
| PH-04 | state12/18 landing `Physics.cs:387-389` 直接 `Hp -= damage; HpMax -= damage`；state13 高速落地也不 clamp HP | `LF2Entity.ApplySharedCharacterDatLandingWeaponCountDamage:5026-5042` 和 state13 `:4993-4999` 额外把 HP/HPBound clamp 到0 | 落地伤害超过当前 HP 或 HPBound | C# 允许负值留到后续死亡/结果链；Unity提前归零，checksum/分支不同。**确认差异**。 |
| PH-05 | oid999 分支位于 `ApplyGroundResolve:367`，受开头 `newY<=0.0001 return` 约束 | `LF2Entity.ApplyCurrentDatNonCharacterLanding:4905-4912` 条件为 `Runtime.Y > -0.0001`，即 grounded Y=0 每拍也触发 | oid999 在地面、Vy=0 | C#不切 frame101；Unity强制 frame101 并清 Vx/Vy/Attacking。**确认差异**。 |
| PH-06 | `Physics.ApplyGroundResolve:255-356` 只更新 frame、速度、Unk31C、Facing、Attacking；不会写 `Residual.WeaponState` | `LF2Entity.ApplyCurrentDatNonCharacterLanding:4800-4900` 每个 light/heavy/throw/drink landing 分支额外写 `Runtime.WeaponState=1000/1003/1004/2000/2004` | 任意武器落地/反弹 | C# 的 WeaponState 只在 `FrameAdvance.FrameLogic:66-80` 走 1002→2000→3000 内部链；Unity将它改成 frame-state，下一拍 boomerang/减速/持有判定可能不同。**确认差异**。 |

## 4. 已核对等价的主分支

以下分支在当前静态代码中未发现新的字段/顺序差异（不代表已有 focused test 自动证明全部输入空间）：

- GameTick tick-head `GameTick/InputPhase/FrameMod12/FrameToggle` 的推进。
- state400/401 目标选择、距离严格比较、无目标清速度及位置偏移主逻辑。
- state500 reset gate 主判断。
- oid7/8→51 合体与51拆分的主要 gate、HP/HPBound、位置、计时器和正式 reset 字段；dormant 适配自身能模拟 Active=false，但 stage factor 未排除 dormant，已列 GT-15。
- cooldown `ARest` decrement 与当前 frame itr/holder-wpoint 对 AttackExempt 的清除。
- frame postprocess 的 hitCount>0 速度平均和 knockback 清零（正常生产写入只有0/1）。
- held positive-link validation 的 target slot/反向 holder 检查。
- character state12/18/13 与普通落地主分支的帧号和阈值；差异仅为 PH-02/03/04。
- light/heavy/throw/drink 的主要落地阈值、反弹速度、Vx系数、声音和帧号；差异仅为 PH-02/06。
- type3 visual-Z 的加法和 logic-Z 概念；preframe Z 分派缺失另列 GT-05。
- `SyncIntegers` 使用 `(int)double` 截断，当前 Unity `Runtime.SyncIntegerPosition` 等价。
- `ResetWeaponCountOutsideState12` 主条件等价。
- stage wave 的 phase advance、ratio/times 计算、40上限、bound 写入、refill/producer gate 主体；槽位/RNG与 dormant factor 例外已列 GT-14/15。

## 5. 待运行时/数据契约证实的风险（3）

| ID | 风险 | 需要的证据 |
|---|---|---|
| R-GP-01 | C# 有独立 `FrameWaitCounter`，`FrameRuntime.SetFrameImmediate` 会清0；Unity `NTSDEntityRuntime` 没有独立字段。新 `BattleParitySnapshot.cs:403,410` 暂时把 `frameWaitCounter` 和 `waitCounter` 都投影成同一个 `runtime.WaitCounter`，因此 comparator 无法发现这类差异。 | 补独立 runtime 字段、所有写入/重置方和 schema 后再对拍。当前不能签发 full certificate。 |
| R-GP-02 | `CharacterMechanics.Step` 的 friction 受 `mass>0` gate，C# `Physics.ApplyGroundFriction` 不看 mass。当前 `NTSDSpec` 已部署条目均为正数/默认1，尚未找到 production mass=0 角色。 | 全量资产/spec 审计或构造 future mass=0 fixture；若可达则升级为确认差异。 |
| R-GP-03 | Unity 用 `OidMergeDormant` 模拟 C# `Active=false + CharData=null`。多数 query 已正确排除，但 `GetAllEntities` 使用点不是统一 active 过滤，GT-15 已实锤一个消费者；仍可能有其他消费者读取 dormant 的 ObjectId/current DAT。 | 全 repo `GetAllEntities` consumer audit 与 merge→多 tick trace。 |

## 6. 明确排除项

- `RunF8WeaponDrop`：调试功能，不计正式战斗差异。
- `RunMode2RandomWeaponDrop` / `InitStats`：特殊调试模式，不计正式战斗差异。
- step-wait/overlay early-return：调试单步，不计正式战斗差异。
- `RunResultsTick` 的结算菜单交互：默认战斗场景范围外；若未来把“battle-end flow”纳入逐帧证书，需要单独实现/对拍。
- camera_x 表现链：按用户明确要求，Unity 固定世界相机，不恢复角色驱动 camera；不把该表现适配计为战斗逻辑差异。背景 layer `AnimCounter` 也未作为本批战斗状态证书字段。
- 默认 `stage.dat` 未部署：继续暂缓，不计差异；GT-14/15 是独立于默认资产部署的 stage runtime 逻辑问题。

## 7. 建议修复顺序

1. 先修会让首个正式 tick 立即分叉的 GT-01/02、PH-03/06。
2. 再修生命周期/槽位：GT-08/09/13/14/15、PH-05。
3. 中央化 current-DAT 分派：GT-04/05/06/07/10，避免继续按 CLR shell 漏分支。
4. 清除正式 C# 不存在的 GT-11，并修 GT-12 double 链。
5. 修自然掉落 GT-03 和 BoundaryWall 额外逻辑 PH-01。
6. 补 `FrameWaitCounter` 契约后，运行同 seed 双端逐 tick trace；以首个 domain/slot/field 差异继续收敛。

在上述确认差异和风险关闭前，不能声明 GameTick/Physics 或全战斗逐帧等价。
