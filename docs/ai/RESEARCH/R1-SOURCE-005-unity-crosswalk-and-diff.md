# R1-SOURCE-005 — Unity CPoint / held / link / opoint / 生命周期 crosswalk

> 状态：COMPLETED（静态 source 审计；runtime / joint fixture 待后续阶段）。  
> Unity source 只描述当前实现；C++ Release source 才定义规则。  
> 本文件不修改 gameplay，不把静态映射写成运行时验收。

## 1. Unity 当前主链

当前 NTSDBattleTickSystem 的相关顺序为：

1. FrameAdvance、DeathCleanup、第一次 StageBounds；
2. PreInteraction：CPoint kind 1 全体、CPoint kind 2 全体、current CPoint sync / held validation；
3. HeldLinkValidation：positive link validation；
4. 第二次 StageBounds；
5. HeldProcess：一轮 negative-link held process；
6. candidate snapshot / collect / character consume / object consume；
7. render handoff 后 LateEntityUpdate：opoint、cleanup、tail、queued flush。

对应坐标：

- NTSDBattleTickSystem.cs:278-385；
- SimulationWorld.Passes.partial.cs:2226-2357、1358-1626；
- SimulationQueryAndLinkModule.cs:39-189。

这与 C++ 的「第一轮 held → candidate / collision → CPoint/weapon sync → positive link → 第二轮 held」
并不相同。D-SCHED-001～004 是实际 source 顺序风险；不得通过移动渲染或回退 ECS/中央表现
掩盖它们。

## 2. CPoint crosswalk

| Unity 入口 | 当前逻辑 | 对应 C++ 合同 | 静态结论 |
|---|---|---|---|
| LF2Entity.RunCpointCheckStep10 → BattleCpointWriter.RunKind1 | GetCollisionFrameData 优先读取 Frame.Prev2D；验证 caught / catcher relation，处理 decrease/action/throw/dircontrol。 | cpoint.cpp:23-190 | 主数据来源已映射。换帧 helper、全局 stats 见 D-CPT-001/002。 |
| LF2Entity.RunCpointMismatchTailStep10 → BattleCpointWriter.RunKind2Validation | 使用 current frame 的 kind 2；关系失效写 frame 212、vy=-3、y clamp。 | cpoint.cpp:192-217 | 分支结构已映射。 |
| LF2Entity.RunWeaponSyncHeldStep10 → BattleCpointWriter.SyncHeldCpoint | 使用 current frame state Catching 的 kind 1 CPoint，同步 hurt、position、cover/facing。 | weapon.cpp:22-107 | 数据公式和 current-frame boundary 已映射。 |
| LF2Character.RunWeaponSyncHeldStep10 → LF2CharacterWeaponLinkResolver.RunWeaponSyncHeldStep10 | 调用 GetHeldEntity 以校验 character-held reference。 | weapon.cpp:109-132 | 当前是额外 Unity reference consistency path；其实际字段差异由 link 项审计。 |

### 2.1 CPoint raw-frame 观察

- Unity GetCollisionFrameData：LF2Entity.cs:4479-4506；
- Unity CPoint raw-preserving helper：LF2Entity.cs:5046-5080；
- Unity immediate helper：LF2Entity.cs:4297-4300、5911-5916；
- RunKind1 在失效 relation、action 和 duration expiry 中调用 DirectWriteFrameImmediateWaitReset；
- C++ 同类位置为 raw Entity.frame write，未写 Entity.wait_counter。

因此 D-CPT-001 的范围是 frame/wait side effect，不是“所有 CPoint 坐标公式错误”。

### 2.2 CPoint 统计写入

BattleCpointWriter.ApplyHeldInjury（212-254）写入 HP、HPBound、combo、holder KillStat、
AttackingCounter、FrameDelay；在该 writer 内没有 World.KillStats / World.DamageStats 写入。
C++ weapon.cpp:50-75 在同一 injury 分支对 g_kill_stats / g_damage_stats 有额外写入。
这形成 D-CPT-002；不应由普通 hit writer 的统计行为推断为 CPoint 已对齐。

## 3. held / link crosswalk

| Unity 入口 | 当前逻辑 | C++ 对照 | 静态结论 |
|---|---|---|---|
| SimulationQueryAndLinkModule.HeldObjectProcessAll | 升序 RuntimeSlotTable；只处理 link_state<0；invalid holder relation 时清 LinkState 和 HolderStableId；有效时调用 BattleHeldObjectWriter.RunStep12。 | game_tick.cpp 两轮 link_state<0 扫描 | Unity 仅有一轮；invalid cleanup 范围较宽。 |
| BattleHeldObjectWriter.RunStep12 | 对 weapon 调用 weapon.Act；对非 weapon 同步 wpoint、state10/12 drop、dvx throw、kind3 drop。 | game_tick.cpp held body | 主公式已映射，type2 FrameDelay 写入见 D-HOLD-001。 |
| LF2WeaponHeldStateResolver.Act | holder state 17 时处理 drink；同步 held pose；type 1/4/6 与 type 2 throw，ThrowHeldWeapon 写 SpawnerEntityIndex。 | game_tick.cpp:1468-1639 / 1903-2018 | type2 spawner assignment 超出 C++ branch，见 D-HOLD-002。 |
| BattleEcsPositiveLinkValidationPass.ExecuteDataOriented | bitset 按升序正 link slot 读 target，并在无效时清 LinkState、TargetSlotIndex、HeldWeaponStableId。 | game_tick.cpp:1829-1846 | 无效 cleanup 超出 C++，见 D-LINK-001。 |
| LF2CharacterWeaponLinkResolver.AttachOpointHeldObject | parent link=1、target=child、held slot=child；child link=-1、holder=parent。 | collision.cpp:1343-1352 | kind 2 core link mapping 已闭合。 |

### 3.1 Unity-only relation safety adapter

RuntimeSlotTable generation、BattleRelationLinkStore bitset、PendingFlushDestroy 与 object-pool release
不是 C++ 400-slot Entity 字段。它们可作为防止 stale reference、降低扫描开销和支持已批准扩展
容量的 Unity adapter；唯一验收标准是不能改变 C++ 的 slot-order、newborn visibility、relation
field 和最终可观察结果。

当前不能把 generation 存在本身写成差异，也不能以 C++ MAX_OBJECTS=400 倒退
MobileExtended / DesktopExtended。

## 4. opoint crosswalk

| Unity 入口 | 当前逻辑 | C++ 对照 | 静态结论 |
|---|---|---|---|
| SimulationWorld.LateEntityUpdateAll | 升序 RuntimeSlotCapacity scan；在每个实体 late state/death 后处理 current-frame opoint；每实体 tail 后 FlushTasks；final pending destroy flush。 | game_tick.cpp:577-647、late scan 687-691 | immediate / cursor-driven 的设计意图已映射；需 lifecycle joint fixture。 |
| BattleStructuralWriter.ProcessLateOpointSegment | 将当前 tick / structural ordinal 传给 object-point materializer。 | frame_advance.cpp:102-172 | structural command / generation 为 adapter，不应改变 current entity immediate boundary。 |
| BattleLogicObjectPointRuntime.ProcessOpointSpawnCoreForStructuralWriter | gate、facing>10、slot 50 起 lowest-free、direct position、kind2 link、multi spread/vrest。 | frame_advance.cpp:102-172；collision.cpp:1271-1371 | 主要字段/顺序已映射。 |
| LF2ObjectPointFactory.ProcessOpointSpawnCoreForStructuralWriter | 非 logic-only 的同一 late opoint route；CreateObjectImmediate 使用 structural writer。 | 同上 | CentralOnly/URP 渲染不参与 simulation truth。 |
| BattleLogicEntityFactory.PostInitLiving / LF2Character.InitializeFromOpoint / LF2WeaponBase.InitializeFrame | 写 relation、owner、team、frame、position、velocity、AI identity。 | collision.cpp:1285-1369 | child current frame 与 core relation 多数对应；Prev2 初值见 D-OP-001。 |

### 4.1 已确认而非缺陷的边界

- DynamicRuntimeSlotStart=50（SimulationWorld.Registry.partial.cs:42）与 C++ opoint 的最低 slot 50 一致；
- Unity Authority400 只用于 C++ fixture；生产 MobileExtended/DesktopExtended 允许更大容量，属于用户已批准需求；
- logic-only 与 Unity renderer materializer 都从 structural writer 的 CurrentEntityImmediate 边界调用；渲染命令/Texture2DArray/URP 仍由 SOURCE-006 单独审计。
- 普通 current-frame opoint 在 LateEntityUpdate 中发生，而 candidate / character/object consume 与 RenderDispatch 均已在前面完成；它的正常 collision 与中央 render handoff 最早是下一逻辑 tick。此结论不覆盖 frame-logic 的 hit_Fa 专项 spawn 或 special-object queued task，它们属于 SOURCE-003/004 与本包的联合夹具。

## 5. 生命周期 crosswalk

| C++ source | Unity source | 当前分类 |
|---|---|---|
| free_entity 立即 active=false，下一次 spawn 前 reset | PendingFlushDestroy 先从 active pass 隐藏；ReleasePendingDestroySlots / FlushPendingEntityDestroy 释放 RuntimeSlot、generation、pool owner | adapter，待 joint lifecycle fixture |
| Entity.reset 归零 relation / frame / prev2 / hold fields | NTSDEntityRuntime.Reset、LF2Entity.ResetReusableRuntimeComponents、各 subtype Reset | 字段覆盖需要以 release fixture 核对，不可只看 Reset 名称 |
| late cursor 越过的低 slot newborn 下 tick 才访问，高 slot newborn 可同轮访问 | LateEntityUpdateAll 以 runtime slot 升序查当前 occupant；object spawn 是 CurrentEntityImmediate | 静态路径可对应，需 lower/higher slot newborn fixture |

## 6. 当前明确不应改动的 Unity 边界

- 不移除或回退 BattleCentralRenderSystem、CentralOnly、Texture2DArray、dynamic Mesh、URP；
- 不恢复 Legacy SpriteRenderer production renderer；
- 不降低 MobileExtended 的 1,000 active 目标，也不为 C++ fixture 给 DesktopExtended 增加 production cap；
- 不移除 FrameInputSet、30 Hz tick、RuntimeSlotTable generation、SoA/ECS writer、pool 或 worker；
- 不实施本文件列出的 D 项；它们必须先经 R1-SOURCE-007 的依赖和验收矩阵分批处理。

## 7. 已知未闭合点

1. kind 2 opoint 的 TrackerFlag / TrackerParent 被 Unity kind5 hit 路径读取；C++ live source 对应辅助字段/consumer 未闭合，不能把 Unity 注释当作证据。
2. PendingFlushDestroy 与 C++ active=false 的 low-slot reuse、same-tick newborn 和 render-visible 最早时点需要 joint fixture。
3. D-CPT-001 的 Unity FrameWaitCounter 与 FrameTransistor wait 两条字段链都要放入 fixture，不能只看当前 frame。
4. D-HOLD-002 的 SpawnerEntityIndex 是否影响后续 type2 AI/hit，需要以 C++ consumer source 和 fixture闭合。
5. Unity FrameLogicBeforeAdvanceAll 和 LF2SpecialAttack object consume 会 flush queued object-point tasks；C++ 也存在 hit_Fa 专项生成，但不能把该独立 source family误判为标准 late DAT opoint 的行为差异。
