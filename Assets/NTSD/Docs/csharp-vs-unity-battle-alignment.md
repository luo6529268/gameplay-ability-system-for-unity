# NTSD C# 工程 vs Unity 工程 — 战斗逻辑差异与对齐清单

> 创建日期：2026-07-12
>
> **唯一权威来源**：`J:\QQFile\NTSD2.4\ntsd_release_C#\src\BattleCore`（NTSD C# 战斗核心工程）
>
> **被对齐工程**：`I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity\Assets\NTSD\Scripts`
>
> 说明：
> - 本文只覆盖**战斗相关逻辑**。可活动范围检测（C# `Bg`/bg.dat）、相机（C# `CameraX`/`UpdateCameraAndBgAnimation`）**不需要对齐**，Unity 保留自己的 BoundaryWall + ProCamera2D。
> - "冗余脚本可删除"的判定必须严格：**只有在 C# 无对应分支、且 Unity 自身也不引用时才可删**；若只是 Unity 换了一种架构实现同一件事（组合/resolver/partial），**不算冗余，不得删除**。
> - **最终表现效果一致原则（重要）**：对于因 Unity 框架/架构限制而**无法做到逻辑层完全对齐**的项，退而求其次的底线是——**运行时最终表现效果必须与 C# 工程完全一致**（位置、帧号、速度、判定结果、伤害数值、时序等对外可观测行为逐帧等价）。即"实现方式可不同，但结果必须等价"。凡标 🔷 的项，验收标准就是这条：不比对代码是否同构，而比对运行结果是否逐帧一致。
> - 标记含义：✅ 已对齐 / ⚠️ 部分对齐或存疑 / ❌ 缺失或明显偏差 / 🔷 架构不同但结果需等价 / 🗑️ 疑似可删（需二次确认）

---

## 0. 权威工程 BattleCore 结构 → Unity 映射总表

| C# BattleCore 文件 | 职责 | Unity 对应 | 映射类型 |
|--------------------|------|-----------|---------|
| `Simulation/GameTick.cs` | 单 tick 总调度（顺序主干） | `Simulation/NTSDBattleTickSystem.cs` + `SimulationWorld.Passes.partial.cs` | 🔷 pass 拆分 |
| `Simulation/NtsdBattleTickSystem.cs` | tick 外层入口 | `Simulation/NTSDBattleTickSystem.cs` | ✅ |
| `Simulation/SimulationWorld.cs` | 世界容器/对象池 | `Simulation/SimulationWorld*.cs` | 🔷 固定槽 vs 动态槽 |
| `Frame/FrameTick.cs` | frame_tick 帧推进 | `Character/FrameTransistor.cs` + `LF2Entity.RunCommonFrameTick` | 🔷 |
| `Frame/FrameAdvance.cs` / `Physics.cs` | 帧推进物理 | `Character/CharacterMechanics.cs` + `PhysicsState` | 🔷 |
| `Interaction/HitResolve.cs` | 命中结算（kind 0~16） | `LF2CharacterHitResolver.cs` + `LF2Weapon.ApplyHitEffects` + `LF2CharacterDatHitResolver` | 🔷 分散到多类 |
| `Interaction/CollisionCollect.cs` | 候选收集 | `Character/BruteForceSceneQuery.cs` | 🔷 |
| `Interaction/CPointRuntime.cs` | 抓取 cpoint | `LF2CharacterCatchResolver.cs` + `PreInteractionTickAll` | 🔷 |
| `Interaction/WeaponRuntime.cs` | 持武器同步/投掷/掉落 | `LF2WeaponHeldStateResolver.cs` + `LF2WeaponReleaseFlowResolver.cs` | 🔷 |
| `Interaction/ObjectPointFactory.cs` (`FrameTick.SpawnFromOpoint`) | opoint 生成 | `Character/LF2ObjectPointFactory.cs` | ✅ 已验证一致 |
| `Input/InputRuntime.cs` | 输入消费 + AI | `Input/CharacterInputModule.cs` + `LF2Entity` shared-DAT 桥 | 🔷 |
| `Entity/Entity.cs` (大字段实体) | 实体真值 | `NTSDEntityRuntime.cs` + `LF2Entity` | 🔷 字段化 |
| `Entity/NtsdCharacter/NtsdWeapon/...` | 实体类别 | `LF2Character/LF2Weapon/LF2SpecialAttack/LF2OtherObject` | 🔷 |

---

## 1. Tick 主循环顺序（`GameTick.Run` vs Unity pass 序列）

C# `GameTick.Run` 是单一函数，顺序完全线性。Unity 拆成 `NTSDBattleTickSystem` 调度多个 `SimulationWorld` pass。两侧顺序必须逐段等价。

| # | C# `GameTick.Run` 步骤 | Unity pass | 状态 |
|---|------------------------|-----------|------|
| 1 | `GameTick++` / `InputPhase` / `FrameMod12` / `FrameToggle` | tick 计数在 `NTSDBattleTickSystem` | ⚠️ 需核对 `InputPhase`/`FrameToggle` 是否都推进 |
| 2 | 清瞬时状态 `PendingSounds.Clear()` 等 | 分散 | ⚠️ |
| 3 | `RunCooldownsTick`（arest-- + attack_exempt 清理） | `VrestTickAll` + `ClearAttackExemptIfCurrentFrameCannotHit` | 🔷 |
| 4 | `postCooldownInput`（人类输入注入） | `PostCooldownInputAll` | ✅ 顺序已对齐（见 AGENTS.md） |
| 5 | `RunOid5152RuntimeMaintenance`（7/8→51 合体） | `Oid5152RuntimeMaintenanceAll` + `TryMergeOid7Or8Into51` / `TrySplitOid51BackToPair` | ✅ 已完成并通过 fresh Unity 运行时验收 |
| 6 | `ApplyCharacterInputPass`（GameTick>1 才应用） | `PostCooldownInputAll` 内 | 🔷 |
| 7 | `RunEarlyStatePasses`（400/401/500/501） | `EarlyFrameAdvanceSpecialsAll` | ✅ 含 BMD-023 修复 |
| 8 | `FrameRuntimePasses.RunFrameLogic`（hit_fa>0 非角色） | `FrameLogicBeforeAdvanceAll` | ✅ |
| 9 | `RunFrameAdvance`（所有 active，清方向键 + 帧推进） | `SerialTickAll`（SimTransit+SimTU） | 🔷 |
| 10 | `RunPostFrameAdvanceStatePasses`（9998 清理 + 复活） | `CleanupState9998Entities` + `RunReleaseEntityCleanupTail` | ⚠️ 复活 pass 见 §8 |
| 11 | `ClampCharactersToStageZ` | (Z 边界，属可活动范围) | 🚫 不对齐 |
| 12 | `RunCPoint` | `PreInteractionTickAll`→`RunCpointCheckStep10` | ✅ |
| 13 | `SyncHeldWeapons` | `RunWeaponSyncHeldStep10` | ✅ |
| 14 | `ValidatePositiveLinks` | (link 校验) | ⚠️ 需确认 Unity 有等价 |
| 15 | `RunHeldWeaponStep12` | `PreInteractionTickAll` 内 | ✅ |
| 16 | `SnapshotPrevFrame2` | `CaptureCollisionFrameSnapshotsAll` | ✅ |
| 17 | `CollectCandidates` | `CollectCollisionCandidatesAll` | ✅ |
| 18 | `ResolveCharacterHits` | `PostInteractionTickAll`（角色候选消费） | 🔷 |
| 19 | `RunNaturalRandomWeaponDrop` | `RandomWeaponDropTickAll` | ✅ |
| 20 | `RunF8WeaponDrop` | **未找到 F8 路径** | 🗑️? 调试功能，见 §7 |
| 21 | `ResolveObjectHits` | `ObjectInteractionTickAll` | 🔷 |
| 22 | `ApplyPreframeBounds`（含相机/bg） | `ApplyPreFrameBoundsAll`（只做逻辑边界） | 🔷 相机部分不对齐 |
| 23 | `ApplyCurrentWavePhaseAdvance` / `StageSpawns` | (stage 波次) | ⚠️ 战斗相关，见 §9 |
| 24 | `ApplyFramePostProcess`（HitCount→Vx 平均） | `FramePostProcessAll` | ✅ |
| 25 | `RunLatePerEntityUpdatePass` | `LateEntityUpdateAll` | ✅ 主对齐点 |
| 26 | `RunMode2RandomWeaponDrop` | `Mode2RandomWeaponDropTailAll` | ✅ |
| 27 | `RunEntityPostframeTail`（heal/catch timer） | `EntityPostFrameTailAll` | ⚠️ 见 §5 heal 差异 |
| 28 | `UpdateBattleResultsFlow` | (结算流程) | 🚫 非战斗运行时范围 |

**关键差异**：
- C# 是**固定 400 槽 `Objects[]` 线性遍历**；Unity 是**动态 runtime slot + SortedDictionary bucket**。这是 🔷 架构差异，结果需等价，遍历顺序必须仍是 slot 升序。
- C# `RunLateEntityUpdate` 单函数内顺序：`RunStateSpecialPreCollision → RegeneratePreCollisionStats → FrameTickRuntime.Tick → 帧组1100/1200 → 死亡掉武器/弹地 → ProcessOpointSpawn → 破武器回收 → RunN30InputTrigger → SpawnStateTransitionEffects → PrevFrame 镜像`。Unity `LateEntityUpdateAll` 已按同序拆分（✅），但 **`RegeneratePreCollisionStats`（HP/PP 自然恢复）** 的位置需核对（见 §5）。

---

## 2. 受击/命中结算（`HitResolve.cs` vs `LF2CharacterHitResolver` + `LF2Weapon`）

C# 把**所有对象**的命中都集中在 `HitResolve.ApplyCandidate`（一个 switch(kind)）。Unity 拆成三条独立路径：
- 角色被击 → `LF2CharacterHitResolver.ResolveHit`
- 武器被击 → `LF2Weapon.Hit` / `ApplyHitEffects`
- 非角色 DAT 实体 → `LF2CharacterDatHitResolver`

这是 🔷 架构差异（合法）。以下逐 kind 核对行为是否等价。

| kind | C# `HitResolve` 分支 | Unity 分支 | 状态 |
|------|---------------------|-----------|------|
| 0/4，以及预处理后的 9→0 → 伤害 | `ApplyDamageCandidate` | `ResolveHit` 普通伤害入口；raw kind9 先由 `BruteForceSceneQuery` 转为 kind0 | ✅ alternate 路径已补齐并运行验证，见下方逐点 |
| 6 | `victim.HitConfirm=3` | `HitConfirmEa=3` return | ✅ |
| 8 | `ApplyKind8`（heal_timer/传送） | `ResolveHit` kind 8 | ✅ |
| 10/11 | `ApplyKind10Or11`（笛子）：kind==11 && weaponCount>=0 return false；WeaponCount=FluteForce 值；Falling 双倍伤害 | `LF2CharacterHitResolver.cs:357-369`（✅）+ `LF2Weapon.cs:481-501`（✅） | ✅ |
| 14 | `ApplyKind14`（方向阻挡） | `ResolveHit` kind 14 + `ApplyKind14DirectionalBlockFrom` | ✅ |
| 15 | `ApplyKind15Movement`（KnockbackVx/Vx/Vz/YInt=-2，按对象类型分 vyStep=3.0/2.3） | `LF2CharacterHitResolver.cs:373-380` 简化实现；武器侧 `LF2Weapon.cs:503-506` `WhirlwindForce` | ⚠️ 形式不同（C# 走 KnockbackVx+真实 Vx/Vz+设 YInt=-2 三段；Unity 走 PS.vx/vz 增量；C# 按对象类型分 3.0/2.3 vyStep，Unity 未区分） |
| 16 | `ApplyKind15Or16` kind=16 路径：Hp-、KillStat++、ComboCountAtk、SFX_065、frame=200、vrest 写入、LinkState 断开 | `LF2CharacterHitResolver.cs:383-390`：`ImmediateFrame(MpDrain=200)` ✅ + MaxMP 缩放伤害 ✅；**缺** KillStat++、ComboCountAtk、SFX_065 音效、vrest 写入、LinkState 断开处理 | ⚠️ |
| 1/3 | `ApplyKind1Grab`/`ApplyKind3Grab` | 走 pre-interaction（`LF2CharacterInteractionResolver`） | 🔷 时序不同，见 §4 |
| 2/7 | `ApplyPickupCandidate` | pre-interaction | 🔷 见 §4 |
| kind 4+WeaponCount>0→0 + dvx 翻转 | `PreprocessCandidate` 154-172 | `BruteForceSceneQuery.cs:602-615` 完整实现（kind 翻转 + dvx 翻转按 PS.dir） | ✅ |
| kind 5 委托攻击 | `PreprocessCandidate`（holder wpoint 替换） | `ResolveHit` kind 5（TrackerParent） | ✅ |
| oid 300 特判 | `ApplyOid300SpecialHit` | `ResolveHit` `ObjectId==300` 分支（`LF2CharacterHitResolver.cs:279`） | ✅ |

### 2.1 kind 0/4/9 伤害主流程逐点核对

C# `ApplyDamageCandidate`（character victim）关键顺序：

1. `itrArest = (itr.Arest < 4 && itr.Vrest == 0) ? 4 : itr.Arest`（`HitResolve.cs:268`） — ✅ **C# 用 Arest 判定 + 取值**
   Unity 已由 `LF2Entity.ResolveArestCooldown` 统一实现同一公式，并供普通角色命中路径复用；`CheckArestCooldownRule` 已在 Unity batchmode 中通过。
2. IronBall victim → dvx/dvy 减半（`PreprocessCandidate`）— Unity 在 `LF2Weapon` 侧，角色路径无此（正确，角色不是 IronBall）
3. alternate 受击路径 — ✅ **已完整落地并通过 Unity 运行时自检**：
   - C# `ShouldUseAlternateHurt`（629-680）→ `ApplyAlternateDamage`（实际逻辑延续到约 line 827）。Unity 以共享 `LF2AlternateDamageResolver` 承载，真实 `LF2Character.Hit` 由 `LF2CharacterHitResolver` 接入，当前 DAT 为角色但 CLR shell 非角色的对象由 `LF2CharacterDatHitResolver.TryResolveHit` 接入；两条入口调用同一 `ShouldUseAlternateHurt` / `ApplyAlternateDamage`，并各自只记录一次 `RecordKind0Hit`。
   - `ShouldUseAlternateHurt` 已覆盖 oid 37/6/52 的 `HitStateCount`/frame 窗口、heavy effect、attacker oid 214/208，以及 `PrevFrame2` state 7 的 HP、`bdefend`、朝向、负 `dvx` 和特殊攻击者判定。
   - 伤害契约为 `FallDamageDiv` 整数换算后 `reducedInjury = injury / 10`；扣 `HP`，`HPBound -= reducedInjury / 3`（整数除法），不累计 `HPLost`。致死与统计副作用使用 holder-copy 的 `KillStat`/`ComboCountAtk`、victim `ComboCountVic`，并以 `Unk344` 索引稳定 3 槽 `KillStats`/`DamageStats`；世界 reset 保持数组 identity 并清零内容。
   - 其余已覆盖 `Fall=80`、hit/attacking 计数、attacker/victim/negative-link holder 的 FrameDelay、attacker-only AttackExempt、vrest clamp、frame 111/112 保留 wait counter、ground/air knockback、state 1002/2000/3000 尾分支。state1002 随机切帧只改 frame/速度，不额外写 `Runtime.WeaponState`；状态判断继续以当前 `Frame.D.state` 为准。
   - heavy weapon 普通伤害的减半发生在 alternate 判断之后，因此 alternate 始终消费原始 itr，不会错误变成 `injury/20`。`ApplyAlternateDamage` 本身也保留 character DAT/type guard，不能被非角色 victim 直接调用。
   - **raw kind9 不直接触发 alternate**：真实角色与 shared-character-DAT 两个 caller 都以 `itr.kind != 9` 为门；raw kind9 必须先由 `BruteForceSceneQuery.ResolveRuntimeItrForPair` 转换为 kind0，才会在非 kind9 普通伤害入口判断 alternate。`LF2SpecialAttack` 也统一在 object interaction pass 使用这条预处理，覆盖 kind4 的 `WeaponCount`/反向 `dvx`（读取逻辑真值 `Dirh()`/`Runtime.Vx`）和 kind9 的 kind0 转换/攻击者 HP 清零。
   - alternate 已写入的 clamp 后 vrest 不会再被角色 DAT、武器或技能对象外层 generic rest 更新覆盖。type3（`Consumable3`/Unity `SpecialAttack`）lead sound 条件已按权威修正；该声音分支属于代码权威对齐，headless 自检无法直接观测音频播放。
   - 针对性自检：`CheckAlternateHurtTriggerMatrix`、`CheckAlternateDamageCoreSideEffects`、`CheckAlternateDamageMotionTailMatrix`、`CheckAlternateDamageCharacterEntry`、`CheckAlternateDamageSharedDatEntry`、`CheckAlternateDamageHeavyWeaponEntries`、`CheckAlternateDamageInteractionVrest`、`CheckSpecialAttackDamagePreprocess`；均包含在 2026-07-14 02:54:22 的 fresh Unity batchmode PASS 中。
4. fall 累积档位（Light/Medium/Heavy/Fall 阈值 → frame 220/222/224/226/180/186）— Unity `HitFall`/`HitFallDown` ✅ 已对齐（注意 5f/7f→0.714 修复已在）
5. `victim.HitStateCount = 45` → Unity `SetHitStateCount(45)` ✅
6. `attacker.FrameDelay=3 / victim.FrameDelay=-3` 普通路径 — Unity 多处 `-3` ✅；alternate 路径独立写 `victim.FrameDelay=-5`，并传播 negative-link holder delay ✅。
7. 攻击方攻击豁免写入 — Unity `attackerLiving?.HitCounters?.SetAttackExempt(exemptVal)` ✅（公式按点 1 修正）

### 2.2 武器被击（`LF2Weapon.ApplyHitEffects` vs `HitResolve.ApplyObjectHurtTail`）

Unity `LF2Weapon.ApplyHitEffects` 已注明"C# baseline: ApplyObjectHurtTail + ApplyStandardDamageKnockbackX"，逐段抄写。核对：
- `FallCounter += fall!=0?fall:20` ✅
- `lightThrow||heavyLike||specialLike → FallCounter=80` ✅
- ApplyStandardDamageKnockbackX 五分支（固定5 / state2000+dvx / FlyingA/B scaled / effect22/23 / 常规）✅
- knockback 帧 180/186 + KnockbackVy ✅
- 攻击者 state 1002 反弹 / state 2000 减速 / state 3000 归 frame 10 — Unity `ApplyAttackerResponse` ✅

**✅ `RecordKind0Hit` 已统一**：`LF2Entity.RecordKind0Hit` 承载 C# timer、owner、随机坐标和 10 槽上限语义，角色与 `LF2Weapon.ApplyHitEffects` 的 kind0 路径均接入；`CheckKind0HitRecords` 已在 Unity batchmode 中通过。

---

## 3. 帧推进（`FrameTick.cs` vs `FrameTransistor` + `RunCommonFrameTick`）

C# `FrameTick.Tick` 是单函数，Unity 拆成 `FrameTransistor.Trans()`（wait/next 推进）+ `LF2Entity.RunCommonFrameTick`（前置门控 + 倒计时）+ hook（`OnFrameTickBeforeWaitAdvance` / `OnFrameTickAfterWaitAdvance`）。

| C# `FrameTick.Tick` 步骤 | Unity | 状态 |
|--------------------------|-------|------|
| `ThrowFrameGuard==Frame` early return | `RunCommonFrameTick` 门控 | ⚠️ 需确认 |
| `FrameDelay!=0 && !Consumable3` return | ✅ | ✅ |
| `AttackExempt--` | ✅ | ✅ |
| `LinkState<0` return | ✅ | ✅ |
| cpoint kind==2 return | ✅ | ✅ |
| Consumable3 + hitA>0 → HP-=hitA, HP<=0 跳 hitD | `LF2Entity.RunCommonFrameTick` type3 分支 | ✅ |
| HitStop/Fall/HitStateCount/HitConfirm 倒计时 | `RunCommonFrameTick` | ✅ |
| frame!=waitCounter → 音效+attacking=0 | `FrameTransistor.Trans` frame 变化清 attacking | ✅ |
| `attacking++` | `Trans.AttackingCounter++` | ✅ |
| state 0 + YInt<0 → frame 212 + SuppressJumpInit | `OnFrameTickBeforeWaitAdvance` | ✅ BMD-023 相关 |
| IronBall state 2000 静止 return | `LF2Weapon.ApplyObjectSpecificFrameTickBeforeWaitAdvance` | ✅ |
| state 14 HP<=0 → HitStop=30 | `RunCommonFrameTick` | ✅ |
| state 2000 facing=vx | ✅ | ✅ |
| `attacking>wait` → next 换帧 | `Trans` attacking>wait | ✅ |
| next=999 → 212/0（空中角色） | `ResolveFrameTickNext999Target` | ✅ |
| next<0 翻面 | `Trans` switchDir | ✅ |
| 上一帧 state14→非13 的 HitStop=15 逻辑 | `OnFrameTickAfterWaitAdvance` | ✅ 含 oid/5==3 skip + difficulty 分支 |
| frame 212 + JumpInitPending → 跳跃初速 | `OnFrameTickAfterWaitAdvance` | ✅ |
| frame mp<0 PP 扣费 + hitD turn | `OnFrameTickAfterWaitAdvance` | ✅ |
| frame 110/114 → CdDefendLock=3 | `RunCommonFrameTick` 尾 | ✅，`CheckFrameTickDefendLockTail` 运行通过 |
| frame 202 → HitStop=20 | ✅ | ✅ |

**结论**：帧推进主干及上述 state14、frame mp、110/114、202 尾部特判均已核实对齐（🔷 hook 拆分合法）。

**逐点核实结果（§3 全部）：**
- §3-1 state14 入口 HitStun=30 + AttackingCounter=0（KillCount>=0 OR Unk364==5 OR slot>=20）— Unity `LF2Character.cs:2205-2211` ✅ **完整对齐**
- §3-2 state14→非13 复活 HitStun=15 分支（aiControlled 检查 + Difficulty!=2 + oid/5==3 + GameMode==1/4 + Oid!=38）— Unity `LF2Character.cs:2134-2163` `ApplyCommonCaughtExitHitStop` ✅ **完整对齐**
- §3-3 frame mp turn-around（C# `HitResolve.cs:178-203`）— Unity `LF2Entity.cs:3284-3321` `ApplyCommonFrameTickPpDisplayPostAdvance` ✅ **完整对齐**（含 PP 扣费、frame.hitD turn、Dual KeyLeft/Right + Facing + YInt==0 条件）
- §3-4 frame==202 → HitStun=20 — Unity `LF2Entity.cs:3634-3635` ✅
- §3-5 frame==110 || frame==114 → `CdDefendLock=3` — Unity `LF2Entity.RunCommonFrameTick` 尾部已实现，runtime Reset/cooldown 衰减已承载；`CheckFrameTickDefendLockTail` 已运行通过 ✅

---

## 4. 交互（pre/post-interaction, cpoint 抓取, opoint）

### 4.1 命中候选消费时序差异（重要）

C# 在 `HitResolve.ApplyCandidate` 里**同一个 switch** 同时处理攻击(0/4/9)、抓取(1/3)、拾取(2/7)。Unity 分成两个阶段：
- `PostInteractionTickAll` → 角色候选消费（攻击 + pre-interaction 混合，`LF2CharacterInteractionResolver.TryConsumeUnifiedStep7CandidateSequence`）
- `ObjectInteractionTickAll` → 武器/技能候选消费

🔷 这是合法架构差异，但 **候选序列消费顺序必须与 C# 一致**（按 step6 收集顺序）。Unity 已用 `TryGetCollisionCandidateSequence` 保序 ✅。

### 4.2 抓取 cpoint

| C# | Unity | 状态 |
|----|-------|------|
| `ApplyKind1Grab`/`ApplyKind3Grab`（命中即建立） | `HandlePreInteractionKind`（pre-interaction 建立） | 🔷 时序不同 |
| `AlignGrabPair`（对位公式 centerx/wact/lerp） | `ApplyImmediateCatchPairState`（同公式） | ✅ 公式一致 |
| `CPointRuntime.Run`（step10 维护） | `RunCpointCheckStep10` + `RunCpointMismatchTailStep10` | ✅ |
| cpoint kind==2 受击 fronthurtact/backhurtact | `ApplyCaughtVictimHurtFrame` / `TryCaughtA` | ✅ |
| throwvx/vy/vz 投掷 + throwinjury | `LF2CharacterCatchResolver`（自检覆盖） | ✅ 有 BattleRuntimeSelfCheck |

### 4.3 opoint 生成 — ✅ 已在 `skill_release_flow_comparison.md` 验证一致

`FrameTick.ProcessOpointSpawn` / `SpawnFromOpoint` vs `LF2ObjectPointFactory.ProcessOpointSpawn` / `ProcessOneLateOpoint`：条件（kind>0 && oid>0 && attacking==0 && (角色→FrameDelay==0)）、facing 展开（>10 → count/facing）、多发 AttackExempt+VRest 扩散、state 3003 linked slot vrest — 均已对齐。

---

## 5. HP/PP 自然恢复 + heal/catch timer

**✅ HP/PP 自然恢复语义对齐**（逐字段核实）：
- C# `RegeneratePreCollisionStats`（`GameTick.cs:1474-1519`） vs Unity `LF2Character.cs:2534-2584`：
  - HP `Hp < HpMax`（HP < HPBound）每12tick+1 ✅
  - `hpForRate = Hp; >500 → 500; oid 51/52 /=2; PP += (500-hpForRate)/100+1` ✅
  - `WeaponCount<0` 每12tick 扣血（injury=900/FallDamageDiv）✅，HP -= injury、HPBound -= injury/3、ComboCountVic += 9 ✅
- 字段映射：`HpMax`↔`HPBound`、`Pp`↔`PP`，通过 `Runtime.HpMax` / `Health.HPBound` / `Runtime.Pp` / `Health.PP` 字段映射。
- 调用入口：Unity `RunPreCollisionRecoveryPhase` 虚函数（`LF2Entity.cs:972` + `LF2Character.cs:2619-2622`），由 `SimulationWorld.Passes.partial.cs:264` 调用。✅

**heal/catch timer（C# `RunEntityPostframeTail`）**：Unity `EntityPostFrameTailAll` 覆盖 HealTimer/CatchTimer/state1700 ✅（之前已确认）。

---

## 6. 输入 + AI

### 6.1 玩家输入消费（`InputRuntime.ApplyCharacterInput` vs `CharacterInputModule` + `LF2CharacterActionResolver`）

C# `ApplyCharacterInput` 单函数：combo wrapper → hitA/hitD/hitJ frame jump → frame110 facing → state 301/19 lane → LinkState2 heavy → frame215 landing → frame182/188 recovery → state 0/1/2/4/5 分发 → ApplyFrameVelocityTail。

Unity 有两套：
- `LF2Character` → `LF2CharacterActionResolver`（完整角色输入）
- `LF2Entity` shared-DAT 桥（`RunSharedCharacterDatStandingActionInputPhase` 等，用于"当前 DAT 是角色但 CLR 实例不是 LF2Character"的 transform 后对象）

🔷 合法架构分层。**注意**：shared-DAT 桥自称"最小实现"，只覆盖 standing/walking/running/dash/jump 基础，**不覆盖 combo/catching/held-weapon 全动作**。这不是冗余 —— 它服务 transform（state 501/4000/8000）后仍挂在 wrong shell 的角色。

关键值对齐（已修复）：
- walk 斜向 `Vx *= 5.0/7.0` = 0.7142857142857143 ✅（两侧都是）
- heavy run 斜向 `Vx *= 5f/6f` / `0.8333...` ✅

**❌ combo wrapper（DJA 等 9 组方向+攻击/跳连招）缺失**：C# `InputRuntime.cs:740` `RunComboWrappers` 实现 ComboDra/Dla/Dld/Dlu/Drd/Dru/Djd/Dja/Daa/Dab + DjaGuard 等 9 组方向连招 + oid6（Sasuke）DjaGuard 特判。Unity grep `RunComboWrappers / ComboDra / ComboDla / ComboDja / DjaGuard / combo_` **全工程 0 命中**。**修复方向**：移植 `RunComboWrappers` 全部 9 组 + oid6 特判。

### 6.2 AI（`InputRuntime.PrepareAiInputBasic`）

**❌ AI 输入生成器完全缺失**：
- C# `InputRuntime.cs:16` `PrepareAiInputBasic`（~600 行巨型函数，oid 专属 combo 决策、C8 威胁扫描、7A/7B 守卫、队友守卫、held weapon 决策、历史闸门、oid1/4/5/33/52 多种 oid 专属 combo）。
- 实际包含 14 个辅助函数（已 grep 确认）：
  - `AiBetweenX`、`AiPostCacheCoordinateAllowsSpecial`、`AiPreUpdateTarget3000SideEffect`
  - `AiUpdateOid33_19_16PredictedDuaDecision`、`AiUpdateOid52_1_2_21PreLabel591Decision`
  - `AiUpdateLabel591Oid51_2_18_7Decision`、`AiUpdateFirstDecision`、`AiUpdateTeammateGuardDecision`
  - `AiUpdateOid1ComboDecision`、`AiUpdateCloseOid1Decision`、`AiUpdateOid4ComboDecision`、`AiUpdateOid5ComboDecision`
  - `AiProcessSubOidGroup`、`AiSpecialOidForSubGate`、`AiProcessHelper`
- Unity grep `AiUpdate / AiBetweenX / AiPostCache / AiPreUpdate / AiProcessSub / AiSpecialOid / AiProcessHelper / ThreatScan` **全工程 0 文件命中**。
- **修复方向**：从零移植 `PrepareAiInputBasic` 全部逻辑 + 14 个辅助函数。**这是 P1/P2 后最大的工作量块**。

---

## 7. C# 有、Unity 未确认/缺失的战斗逻辑（重点排查项）

| 编号 | C# 逻辑 | 位置 | Unity 状态 | 判定 |
|------|---------|------|-----------|------|
| M-1 | **oid 7/8 → 51 合体 / 51 拆分** (`RunOid5152RuntimeMaintenance` `GameTick.cs:1093`, `TryMergeOid7Or8Into51` :1123, `SplitOid51BackToPair` :1214) | GameTick early | ✅ `NTSDBattleTickSystem` / `SimulationWorld.Passes` / `NTSDEntityRuntime` / `BattleRuntimeSelfCheck` 已落地 | **✅ 已完成 / Unity 运行时已验证（T4）** |
| M-2 | **复活 pass**（`RunRespawnPass` `GameTick.cs:839-934`：state14+HP<=0 + HitStop 窗口 + 两分支[Hp2Overlay/RespawnCount] + 队友位置平均 + Pp=500/HpMax=Hp3 + Frame=212/YInt=-300 + 生成 oid998 复活特效） | GameTick step10 | ✅ `SimulationWorld.Passes` / `BattleRuntimeSelfCheck` 主逻辑与样例已落地；已补 no-renderer 销毁注销链与 reference-pool 惰性初始化 | **✅ 已完成 / Unity 运行时已验证（T5）** |
| M-3 | **N30 输入触发**（`RunN30InputTrigger`：input history 9/0/9/0→触发码 100/102/104 生成 998 + history gate 广播） | LateEntityUpdate | ✅ `RunLateCharacterDatInputTrigger`（LF2Entity） | ✅ 已移植 |
| M-4 | **状态转换特效**（`SpawnStateTransitionEffects`：state13/frame200 退出 + state18/19 燃烧特效） | LateEntityUpdate | ✅ `SpawnLateTransitionEffects` | ✅ |
| M-5 | **死亡弹地帧**（`ApplyDeathBounceFrame`：frame186 + Vy=-3） | LateEntityUpdate | ⚠️ `RunLateDeathOpointPreCleanupPhase` 需确认 | ⚠️ 未逐行确认 |
| M-6 | **F8 强制掉武器**（`RunF8WeaponDrop`） | GameTick | ❌ grep `F8/force drop` 0 命中 | 🗑️ **确认是调试功能，可不移植** |
| M-7 | **kind 4 + WeaponCount>0 → kind 0 + dvx 翻转**（`PreprocessCandidate` 154-172） | HitResolve | ✅ `BruteForceSceneQuery.cs:602-615` 完整实现 | ✅ 已对齐 |
| M-8 | **ShouldUseAlternateHurt / ApplyAlternateDamage**（injury/10 减伤 + KnockbackVx 特殊累积 + FrameDelay=-5） | HitResolve 629-约827 | ✅ 共享 `LF2AlternateDamageResolver`；`LF2Character.Hit` 与 shared-character-DAT resolver 两入口均接入；runtime/stat/运动尾契约均有自检 | **✅ 已完成 / Unity 运行时已验证（T1）** |
| M-9 | **RecordKind0Hit**（命中记录锚点 + spark，武器命中也调用） | HitResolve 1150 | ✅ `LF2Entity.RecordKind0Hit` 统一角色/武器 kind0 记录 | **✅ 已完成 / Unity 运行时已验证（T2）** |
| M-10 | **oid300 特殊命中**（bdy.x>1000→帧号） | HitResolve | ✅ `ResolveHit` ObjectId==300（`LF2CharacterHitResolver.cs:279`） | ✅ |
| M-11 | **state 400/401 传送**（最近敌/最远友） | GameTick early | ✅ `RunEarlyTeleportSpecialsPhase` | ✅ |
| M-12 | **state 500/501 变身 transform** | GameTick early | ✅ `RunEarlyState500/501Specials`（BMD-023） | ✅ |
| M-13 | **stage 波次生成**（`ApplyCurrentWavePhaseAdvance` `GameTick.cs:2317` + `ApplyCurrentWaveImmediateStageSpawns` :2350 + `RefillCurrentWavePositiveStageSpawns` :2226，StageProgression/StageSpawnRuntime 一整套） | GameTick step 23 | ❌ grep `StageSpawn/StageProgression/WaveIdx` 全 0 命中 | **❌ 缺失（波次刷敌，属战斗逻辑，需新增）** |
| M-14 | **frame 110/114 → CdDefendLock=3**（`FrameTick.cs:208-209`） | FrameTick 尾 | ✅ `LF2Entity.RunCommonFrameTick` 尾部 + runtime Reset/cooldown | **✅ 已完成 / Unity 运行时已验证（T3）** |
| M-15 | **kind 16 完整结算**（`ApplyKind15Or16` kind=16：KillStat++/ComboCountAtk/SFX_065/vrest/LinkState 断开） | HitResolve 1640-1704 | ✅ 真实 `LF2CharacterHitResolver` 与 shared-DAT `LF2CharacterDatHitResolver` 均已补齐 FallDamageDiv 缩放、KillStat/ComboCount、frame200、vrest、2/-2 持有断开与 SFX_065 | **✅ 已完成 / Unity 运行时已验证（T6）** |
| M-16 | **kind 15 完整位移**（`ApplyKind15Movement`：KnockbackVx+真实 Vx/Vz+YInt=-2，按对象类型分 vyStep 3.0/2.3） | HitResolve 1737 | ✅ 真实 `LF2CharacterHitResolver` 与 shared-DAT `LF2CharacterDatHitResolver` 均已改为 authority 的 KnockbackVx/Vz + YInt/Vy 语义；武器/铁球侧原 `WhirlwindForce` 保持 3.0/2.3 分支 | **✅ 已完成 / Unity 运行时已验证（T6）** |

> **判定原则提醒**：当前仍标 ❌/⚠️ 的 M-1/M-2/M-13/M-15/M-16 都**不能直接删对应 Unity 脚本**；它们是"C# 有 Unity 缺/结果仍需验证"。M-7/M-8/M-9/M-10/M-11/M-12/M-14 已确认对齐或完成并运行验证。只有 M-6（F8 调试）确认是调试功能后可不移植。

---

## 8. 判定为"架构不同但等价"的项（🔷 — 不得当冗余删除）

以下 Unity 代码看似"多出来"，实为 Unity 框架下实现 C# 同一逻辑的必要产物，**严禁因为 C# 没有同名文件就删除**：

| Unity 脚本/机制 | 对应 C# 逻辑 | 说明 |
|-----------------|-------------|------|
| `LF2Character*Resolver.cs`（Hit/Catch/DamageState/Action/Interaction/State/WeaponLink） | `NtsdCharacter` + `HitResolve`/`CPointRuntime`/`InputRuntime` 各段 | 组合模式拆分，逻辑等价 |
| `LF2AlternateDamageResolver` + `LF2CharacterDatHitResolver` | `HitResolve.ShouldUseAlternateHurt` / `ApplyAlternateDamage` | alternate 真值集中一次实现，由真实 `LF2Character.Hit` 与 shared-character-DAT 两入口复用 |
| `LF2Weapon*Resolver.cs`（Interaction/HeldState/ReleaseFlow/FrameLogic） | `WeaponRuntime` 各段 | 同上 |
| `LF2Entity` shared-DAT 输入桥（~900 行） | `InputRuntime.ApplyCharacterInput` 中"当前 DAT 是角色"的分发 | 服务 transform 后 wrong-shell 角色，C# 因为是纯数据 Entity 不需要 shell 概念 |
| `NTSDEntityRuntime` 字段分桶 | `Entity` 大字段对象 | Unity 运行时化，字段一一对应 |
| `FrameTransistor` hook（OnFrameTickBeforeWaitAdvance 等） | `FrameTick.Tick` 内联步骤 | 拆成 hook 供子类覆写 |
| `SimulationWorld` 动态 runtime slot | `Objects[400]` 固定槽 | Unity 用对象池，遍历顺序需保持 slot 升序 |
| `RefreshRuntimeSnapshot` 调用 | `CharacterSync.SyncRuntimeFromLegacy` | Unity 每 pass 后刷快照 |
| `DirectWriteFramePreserveWaitCounter` | `SetFrameImmediate`（不清 attacking） | BMD-023：区别于 `ImmediateFrame`（会清 attacking） |

---

## 9. 不需要对齐的部分（明确排除）

| 项 | C# 位置 | 原因 |
|----|---------|------|
| 可活动范围 / Z 边界钳制 | `ApplyPreframeBounds` Z 段、`ClampCharactersToStageZ`、`Bg.ZBoundary*` | 用户明确：bg.dat 可活动范围不对齐，Unity 用 BoundaryWall |
| 相机 | `UpdateCameraAndBgAnimation`、`CameraX`/`CameraVel` | 用户明确：相机不对齐，Unity 用 ProCamera2D |
| bg 层动画 | `layer.AnimCounter` | 背景表现 |
| 结算界面 | `RunResultsTick`、`UpdateBattleResultsFlow` | 非战斗运行时（菜单/结算） |
| SDL/Host/音频桥 | `src/Host/*` | C# EXE 适配层 |
| 数据加载 | `src/Data/*` | Unity 用自己的 DatParser |

---

## 10. 对齐优先级清单（已全部逐行核实，✅=已核实定性）

### P0 — 已修复并完成 Unity 运行时验证
- [x] **§2.1-1 / T0** `exemptVal` 公式 — **已修复并通过 Unity 运行时自检**：`LF2Entity.ResolveArestCooldown` 与 `LF2CharacterHitResolver` 已按 arest/vrest 权威公式处理
- [x] **§2.1-3 / M-8 / T1** ApplyAlternateDamage — **已完成并通过 Unity 运行时自检**：共享 `LF2AlternateDamageResolver` 覆盖约 line 827 的完整权威契约；真实 `LF2Character.Hit` 与 shared-character-DAT resolver 两入口、`Unk344`/统计数组/`HPBound`、heavy/rest/preprocess/state tail 均有针对性检查

### P1 — 已补齐并完成 fresh Unity 运行时验证
- [x] **M-1 / T4** oid 7/8→51 合体拆分 — **已完成并通过 fresh Unity 运行时自检**
- [x] **M-2 / T5** 复活 pass（`RunRespawnPass` 完整逻辑）— **已完成并通过 fresh Unity 运行时自检**

### P1 — 已确认缺失战斗逻辑（需新增）
- [x] **M-13** stage 波次生成（`ApplyCurrentWaveXxx` 整套）— **确认缺失**
- [x] **§6.1 / combo** `RunComboWrappers` 9 组方向连招 + oid6 DjaGuard — **确认缺失**
- [x] **§6.2 AI** `PrepareAiInputBasic` + 14 个辅助函数 — **确认完全缺失（最大工作量块）**

### P1 — 已确认对齐（无需动作）
- [x] **M-7** kind4+WeaponCount>0→0 dvx 翻转 — ✅ `BruteForceSceneQuery.cs:602-615`
- [x] **M-9 / T2** 武器命中 spark（`RecordKind0Hit`）— **已完成并通过 Unity 运行时自检**（角色与武器 kind0 路径统一记录）
- [x] **§5** HP/PP 自然恢复 + HpMax/HPBound — ✅ 逐字段对齐
- [x] **kind 10/11 笛子** ✅、**kind 14 方向阻挡** ✅、**oid300** ✅、**kind 5 委托** ✅

### P2 — 帧推进尾部特判（已核实）
- [x] **§3-1/§3-2** state14 复活 HitStop（oid/5==3 + difficulty 分支）— ✅ 完整对齐（`LF2Character.cs:2134-2163 / 2205-2211`）
- [x] **§3-3** frame mp turn-around — ✅ 完整对齐（`LF2Entity.cs:3284-3321`）
- [x] **§3-4** frame 202 HitStun=20 — ✅（`LF2Entity.cs:3634`）
- [x] **M-14 / T3** frame 110/114 CdDefendLock=3 — **已完成并通过 Unity 运行时自检**

### P2 — 已补齐并完成 Unity 运行时验证
- [x] **M-15 / M-16 / T6** kind 15/16 完整位移与副作用 — **已完成并通过 Unity 运行时自检**

### P3 — 确认可不移植
- [x] **M-6** F8 强制掉武器 — ✅ 确认是调试功能，Unity 不需实现（非冗余，是未移植的调试项）

### 未逐行确认（下一轮核实）
- [ ] **M-5** 死亡弹地帧（`ApplyDeathBounceFrame`）在 `RunLateDeathOpointPreCleanupPhase` 的落点
- [ ] §1 tick 表中标 ⚠️ 的次要项：InputPhase/FrameToggle 推进、ValidatePositiveLinks 等价、复活 pass 落点（已随 M-2 定性）
- [ ] **negative vaction 残余风险**：cpoint 的负 `vaction` 符号帧/翻面语义尚未做专项逐行对齐与 Unity 运行时验证；本轮 T1 PASS 不关闭该项。

---

## 附：核对方法

1. 本文所有 ⚠️/❓ 项都需**打开对应 C# 源码段 + Unity 源码段逐行比对**后才能定性。
2. 定性为"Unity 用别的方式实现了" → 标 🔷 并记录对应关系，**不删**。
3. 定性为"C# 有 Unity 真没有，且是正式战斗逻辑" → 标 ❌ 进 P1 待补。
4. 定性为"C# 是调试/表现/菜单，非战斗运行时" → 标 🚫 排除。
5. 每完成一项核对，更新对应行状态并在 §10 勾选。

---

## 附二：核实总账（更新至 2026-07-14）

**❌ 已确认缺失（C# 有 Unity 无，必须新增，共 3 项）：**

| 项 | 内容 | 工作量 |
|----|------|--------|
| M-13 | stage 波次刷敌（ApplyCurrentWave 整套）| 大 |
| combo | RunComboWrappers 9 组连招 + oid6 DjaGuard | 大 |
| AI | PrepareAiInputBasic + 14 辅助函数 | **极大（~600 行）** |

**✅ 已修复真 bug（共 1 项）：**

| 项 | 内容 |
|----|------|
| §2.1-1 / T0 | `exemptVal` 已改用权威 arest/vrest 公式，并通过 Unity 运行时自检 |

**✅ 原缺失项已完成并通过 Unity 运行时自检（共 6 项）：**

| 项 | 内容 |
|----|------|
| M-1 / T4 | oid 7/8→51 合体拆分 |
| M-2 / T5 | 复活 pass（含 free-entity gate、队友平均落点、stored-count 分支与 oid998 特效） |
| M-8 / T1 | 共享 ApplyAlternateDamage 完整契约、真实角色/shared-DAT 两入口及 object-pass 预处理 |
| M-9 / T2 | 角色/武器统一 `RecordKind0Hit` |
| M-14 / T3 | frame 110/114 写 `CdDefendLock=3` 及 cooldown 生命周期 |
| M-15 / M-16 / T6 | kind15 authority 位移 + kind16 完整结算、副作用与持有断开 |

**⚠️ 部分对齐（副作用/形式差异，需补齐或验运行结果，共 1 项）：**

| 项 | 内容 |
|----|------|
| M-5 | 死亡弹地帧落点未逐行确认 |

**✅ 已确认对齐或已完成并验证（主要项）：**
tick 主循环主干、kind 0/4/9 主流程（含 raw kind9→kind0 预处理与 alternate）、kind 6/8/10/11/14 命中、oid300、kind5 委托、kind4+WeaponCount 翻转（M-7）、HP/PP 自然恢复（§5）、heal/catch timer、帧推进主干 + state14 复活 HitStop（§3-1~§3-5）、frame mp turn-around、opoint 生成、cpoint 抓取、state 400/401/500/501、N30 触发、状态转换特效。

**🔷 架构不同但等价（严禁删，见 §8）：** resolver / shared-DAT 桥 / 字段化 runtime / hook 拆分 / 动态槽 / DirectWriteFramePreserveWaitCounter 等。

**🚫 不需对齐（见 §9）：** bg 可活动范围、相机、结算、Host、数据加载。**🗑️ 确认可不移植：** M-6 F8 调试掉武器。

**⚠️ 残余专项验证风险：** negative `vaction` 的符号帧/翻面语义仍未专项逐行对齐和 Unity 运行时验证，不计入上述 3 项已知部分差异。

---

### 一句话总结

**战斗逻辑差异点已完成本轮核实。** 当前净结果：**P0 未修复项 0 + 3 项缺失逻辑（AI、combo、stage 波次）+ 6 项已完成并通过 Unity 运行时自检 + 1 项部分对齐**；另保留 negative `vaction` 专项验证风险。按剩余工作量排序，最大的三块仍是 **AI 输入生成器、combo 连招、stage 波次刷敌**。

## 实施进度（2026-07-14）

> §10 的 `[x]` 仅表示“已核实定性”，不表示已经实现；实际完成状态以本表为准。

| 任务 | 状态 | 关键落点 | 针对性自检 |
|------|------|----------|------------|
| T0 | **已完成 / Unity 运行时已验证** | `LF2Entity.ResolveArestCooldown`；`LF2CharacterHitResolver` 的 AttackExempt 写入改用 arest/vrest 公式 | `CheckArestCooldownRule` 已覆盖 arest/vrest 边界组合并通过 |
| T2（M-9） | **已完成 / Unity 运行时已验证** | `LF2Entity.RecordKind0Hit` 统一命中记录；`LF2Weapon.ApplyHitEffects` 的 kind 0 路径接入 | `CheckKind0HitRecords` 已覆盖 owner、timer、随机坐标范围和 10 槽上限并通过 |
| T3（M-14） | **已完成 / Unity 运行时已验证** | `LF2Entity.RunCommonFrameTick` 尾部写 `CdDefendLock=3`；runtime 字段、Reset 和 cooldown 衰减已承载 | `CheckFrameTickDefendLockTail` 已覆盖 110/114、早退、普通帧和 3→0 衰减并通过 |
| T1（M-8） | **已完成 / Unity 运行时已验证** | 共享 `LF2AlternateDamageResolver`；真实 `LF2Character.Hit` 与 `LF2CharacterDatHitResolver.TryResolveHit` 两入口；`NTSDEntityRuntime.Unk344`；稳定 3 槽 `KillStats`/`DamageStats` 与保 identity reset；`HPBound` 整数扣减且 `HPLost` 不变；heavy 顺序、character guard、clamp 后 vrest、SpecialAttack object-pass kind4/9 预处理、state1002 不写 `WeaponState`。type3 lead sound 已按代码权威对齐，headless 未直接观测音频 | `CheckAlternateHurtTriggerMatrix`、`CheckAlternateDamageCoreSideEffects`、`CheckAlternateDamageMotionTailMatrix`、`CheckAlternateDamageCharacterEntry`、`CheckAlternateDamageSharedDatEntry`、`CheckAlternateDamageHeavyWeaponEntries`、`CheckAlternateDamageInteractionVrest`、`CheckSpecialAttackDamagePreprocess` 均通过 |
| T4（M-1） | **已完成 / Unity 运行时已验证** | `Oid5152RuntimeMaintenanceAll`、`TryMergeOid7Or8Into51`、`TrySplitOid51BackToPair` 已落地到 `SimulationWorld.Passes.partial.cs` 与运行时身份维护链 | `CheckOid5152MergeSuccessAndDormantIsolation`、`CheckOid5152MergeCooldownOneTriggersSameTick`、`CheckOid5152SplitSuccessAndOddTruncate`、`CheckOid5152SplitFailurePartialRecovery`、`CheckOid5152DjaReleaseTriggersSameTickSplit` 均通过 |
| T5（M-2） | **已完成 / Unity 运行时已验证** | `SimulationWorld.PostFrameAdvanceDeathCleanupAll` 已补齐 respawn 两分支、队友平均落点、PP/HP/HpMax/Frame212/Y=-300、oid998 特效生成；`LF2Entity` / `LF2LivingObject` / `LF2Character` 已补 no-renderer 销毁注销链；`LF2ReferencePool` 已补惰性初始化，允许 self-check 直接 new 的角色安全释放 | `CheckRespawnPassWithoutStoredCount`、`CheckRespawnPassFreeEntityGate`、`CheckRespawnPassWithStoredCountAndEffectSpawn` 均通过 |
| T6（M-15/M-16） | **已完成 / Unity 运行时已验证** | 真实 `LF2CharacterHitResolver` 与 shared-DAT `LF2CharacterDatHitResolver` 均已对齐 kind15 authority 位移与 kind16 完整结算；角色 victim 不再走旧的 MaxMP 缩放或 `PS.vx/vz` 增量路径 | `CheckKind15CharacterWhirlwind`、`CheckKind16CharacterSideEffects` 均通过 |

最新验证（2026-07-14）：fresh `dotnet build Assembly-CSharp.csproj /v:minimal /m:1` 为 **0 errors / 42 warnings**；随后 fresh Unity batchmode `BattleRuntimeSelfCheck` 完整通过，`ntsd_selfcheck_unity.log` 结尾明确记录 `[BattleRuntimeSelfCheck] 战斗运行时自检通过。`。本轮新增通过的针对性断言为 `CheckKind15CharacterWhirlwind`、`CheckKind16CharacterSideEffects` 与 `CheckLateDeathBounceFrame`；其中非主线 M5 仅补齐 `EnterLateDeathLaunchFrame()` 的 `Runtime.YInt = -1` 写回，并已验证 frame 186 强制切换、`Y/YInt=-1`、`Vy/KnockbackVy=-3` 以及 grounded frame 212 重弹行为。因此 M-15/M-16/T6 已升级为“已完成 / Unity 运行时已验证”，非主线 M5 也已补齐并通过运行时验证；T0/T1/T2/T3/T4/T5 的既有 Unity PASS 证据继续有效；type3 lead sound 仍只声明代码权威对齐，headless 未直接观测音频播放。
