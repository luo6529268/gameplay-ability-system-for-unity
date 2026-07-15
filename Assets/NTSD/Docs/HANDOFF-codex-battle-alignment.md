# 接手文档 — NTSD C# → Unity 战斗逻辑对齐（Codex 无缝接手版）

> 生成：2026-07-13 ｜ 供 Codex 或任何接手者直接开工，无需追溯历史会话。

## 0. 你要做什么

把 **NTSD C# 战斗核心** 里 Unity 尚未对齐的战斗逻辑，逐条补齐 / 修正到 Unity 工程。
T0-T9 主线已实现并通过针对性 self-check，但**整个战斗运行时尚未获得全量逐帧等价证明**。剩余工作以本文 §9 和完整差异清单的「剩余纯战斗逻辑 backlog」为准。

- **正式 gameplay authority**：`J:\QQFile\NTSD2.4\ntsd_release`（C++ release；必要时回查对应 EXE 地址）。
- **C# baseline**：`J:\QQFile\NTSD2.4\ntsd_release_C#\src\BattleCore`，用于 C#→Unity 映射和差异定位；若与 C++/EXE 冲突，以 C++/EXE 为准。本轮 negative `vaction` held-sync 即按该规则处理。
- **被对齐工程**：`I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity\Assets\NTSD\Scripts`
- **完整差异清单（配套读）**：`Assets/NTSD/Docs/csharp-vs-unity-battle-alignment.md`
  - 本文是「行动版」，那份是「全量核实版」。有疑问回查那份的 §0~§9。

## 1. 铁律（不可违反）

1. **权威锁死**：任何正式战斗改动必须能在 C++ release/EXE 找到对应行为；C# baseline 用于快速映射。两者冲突时记录冲突并以 C++/EXE 为准，不得臆造。
2. **表现效果一致优先**：能逐行对齐就对齐；Unity 框架限制无法同构时，**运行时最终表现必须逐帧等价**（位置/帧号/速度/伤害/时序）。
3. **只新增不误删**：本文的 ❌ 项都是「C# 有 Unity 无」，是**新增**任务，**不是删除**。
4. **架构等价严禁删**：见 §5 清单——Unity 用 resolver/组合/hook 换方式实现的，不算冗余。
5. **排除范围不碰**：bg.dat 可活动范围、相机——不对齐，不改。

## 2. 任务清单（按建议顺序，坐标精确到行）

### T0 — 修真 bug：exemptVal 用错变量（已完成，Unity 运行时已验证）
- **C# 权威**：`HitResolve.cs:268` → `int itrArest = itr.Arest < 4 && itr.Vrest == 0 ? 4 : itr.Arest;`
- **Unity 落点**：`LF2Entity.ResolveArestCooldown`；`LF2CharacterHitResolver` 的 AttackExempt 写入已改用 arest/vrest 权威公式。
- **验收**：`CheckArestCooldownRule` 已覆盖 arest/vrest 边界组合并通过 Unity 运行时自检。

### T1 — ApplyAlternateDamage（已完成，Unity 运行时已验证）
- **C# 权威**：`HitResolve.cs:629-827` `ShouldUseAlternateHurt` / `ApplyAlternateDamage` 完整方法。
- **Unity 落点**：共享 `LF2AlternateDamageResolver`，由真实角色与 shared-DAT 两入口复用；runtime/stat/运动尾契约已补齐。
- **验收**：alternate trigger/core/motion/character/shared-DAT/heavy/object-pass 针对性检查均通过。

### T2 — 武器命中 spark（M-9，已完成，Unity 运行时已验证）
- **C# 权威**：`HitResolve.cs:1150` `RecordKind0Hit`（timer：`Fall>60 ? sparkPhase*20 : sparkPhase*20+10`），312/320/**506** 三处调用，**武器命中路径（506）也调**。
- **Unity 落点**：`LF2Entity.RecordKind0Hit` 统一命中记录；`LF2Weapon.ApplyHitEffects` 的 kind 0 路径已接入。
- **验收**：`CheckKind0HitRecords` 已覆盖 owner、timer、随机坐标范围和 10 槽上限并通过 Unity 运行时自检。

### T3 — frame 110/114 → CdDefendLock=3（M-14，已完成，Unity 运行时已验证）
- **C# 权威**：`FrameTick.cs:208-209` → `if (frame==110 || frame==114) CdDefendLock=3;`
- **Unity 落点**：`LF2Entity.RunCommonFrameTick` 尾部写 `CdDefendLock=3`；runtime 字段、Reset 和 cooldown 衰减已承载。
- **验收**：`CheckFrameTickDefendLockTail` 已覆盖 110/114、早退、普通帧和 3→0 衰减并通过 Unity 运行时自检。

### T4 — oid 7/8 → 51 合体拆分（已完成，Unity 运行时已验证）
- **C# 权威**：`GameTick.cs:1093` `RunOid5152RuntimeMaintenance` → `1123` `TryMergeOid7Or8Into51` + `1214` `SplitOid51BackToPair`
- **Unity 落点**：`Oid5152RuntimeMaintenanceAll`、merge/split helper 与 runtime 身份维护链已落地。
- **验收**：合体、同 tick cooldown、拆分、失败恢复与 Dja release 针对性检查均通过。

### T5 — 复活 pass（已完成，Unity 运行时已验证）
- **C# 权威**：`GameTick.cs:839-934` `RunRespawnPass`（tick step10）
  - 门控：state==14 + Hp<=0 + (KillCount>=0 OR Unk364==5 OR slot>=20) + HitStop∈(0,5)
  - 分支A（RespawnCount<=0）：Hp2Orig<2→FreeEntity；否则 Hp2Overlay-1、队友 X/Z 平均+随机、Pp=500、HpMax=Hp3、Hp=HpMax、HitStop=20、Frame=212、YInt=-300
  - 分支B（RespawnCount>0）：Pp=0、HpMax=RespawnCount、Hp3=HpMax、Hp=HpMax、RespawnCount=0、Unk364=1、oid∈[0x1E,0x24]→Unk318=0x8C、Frame=0xDB、FrameDelay=0xA、生成 oid998 复活特效
- **Unity 落点**：`PostFrameAdvanceDeathCleanupAll` 已实现两分支、free gate、队友平均落点、血量/PP/帧字段与 oid998 特效。
- **验收**：无 stored-count、free gate、stored-count + effect 三项检查均通过。

### T6 — kind 15/16 副作用补齐（已完成，Unity 运行时已验证）
- **C# 权威**：`HitResolve.cs:1628` `ApplyKind15Or16` + `1737` `ApplyKind15Movement`
  - kind16 完整：Hp-、KillStat++、ComboCountAtk、`RecordSound("SFX_065")`、Frame=200、vrest 写入、LinkState==2 断开
  - kind15 位移：`KnockbackVx = Vx + (±1)`、真实 Vx=KnockbackVx、`KnockbackVz = Vz + (±0.5)`、`YInt=-2`；按对象类型分 vyStep（角色3.0 / 飞行道具3.0 / IronBall2.3）
- **Unity 落点**：真实角色与 shared-DAT resolver 已补 kind15 authority 位移、kind16 统计/vrest/link/SFX 副作用。
- **验收**：`CheckKind15CharacterWhirlwind`、`CheckKind16CharacterSideEffects` 均通过。

### T7 — combo 连招 wrapper（大）
- **C# 权威**：`InputRuntime.cs:740` `RunComboWrappers`（9 组：Dra/Dla/Dld/Dlu/Drd/Dru/Djd/Dja/Daa/Dab + DjaGuard，含 oid6 Sasuke DjaGuard 特判），入口 `InputRuntime.cs:647`。
- **Unity 现状**：已由 `NTSDInputStateModule` 承载 9 组 wrapper 与 oid6 DjaGuard，真实消费路径为 `LF2Character.RunPostCooldownInputPhase -> UpdateLocalInputStateFromControllerBuffer -> ComboUpdate -> NTSDInputStateModule.ApplyFrameInput`。
- **本轮新增验证**：`BattleRuntimeSelfCheck` 已补 `CheckComboWrappersCharacterFrameJumps` 与 `CheckOid6DjaGuardComboHold`，覆盖 9 组 frame jump、左右向切换、cooldown 清空，以及 oid6 guard hold/release。
- **验收现状**：已通过当前打开 Unity 的 request 自检机制，`Temp/NTSD_BattleRuntimeSelfCheck.result` fresh 返回 `PASS`。**T7 已完成。**

### T8 — stage 波次刷敌（M-13，大）
- **C# 权威**：`GameTick.cs:2317` `ApplyCurrentWavePhaseAdvance` + `2350` `ApplyCurrentWaveImmediateStageSpawns` + `2226` `RefillCurrentWavePositiveStageSpawns`（配套 `StageProgression` + `StageSpawnRuntime*` 一整套，见 `SimulationWorld.cs:68-80`），tick step23。
- **Unity 落点**：`BattleRuntimeState` 已补齐 `StageProgression` / `StageSpawnRuntime*`；`SimulationWorld.StageWave.partial.cs` 已实现立即刷敌、正 ratio 并发槽/总量补充、清场推进和 phase bound 写回；`NTSDBattleTickSystem` 在 `PreFrameBounds` 后、`RenderDispatch` 前执行该 pass，匹配权威 step23 顺序。spawn 契约已补 `Unk344=2`、DAT type 0/5 的 character-init `RelationTeam=2/HitStun=20`、其他类型 `RelationTeam=0/HitStun=0`、dynamic slot 50+ 和 action 0 保留。
- **生产接线**：`AppManager.InitializeBattle -> SimulationTickDriver.ApplyMatchConfig -> BattleStageCampaignLoader -> ConfigureStageCampaigns(-1) -> StartInitialStageWave()` 已接通；默认读取 `Application.streamingAssetsPath/NTSD/data/stage.dat`，也可由 `MatchConfig.stageCampaignFilePath` 显式覆盖。仓库当前未纳入二进制 `stage.dat`，缺失时会明确 warning 并保持 `StageProgressionValid=false`。
- **本轮新增验证**：`CheckStageWaveBootstrapAndSpawnContract` 覆盖 stage 文本解析、pre-wave -1→0、bound、type0/type5/非角色身份契约和 action 0；`CheckStageWaveImmediateSpawnAndAdvance` 覆盖真实 direct spawn、dynamic slot 50+、20-49 非 stage 槽隔离、清场推进；`CheckStageWavePositiveSpawnRefill` 覆盖并发槽补位与总量上限。
- **验收现状**：fresh Unity batch self-check 返回 `PASS`。**T8 逻辑与生产接线代码已完成并通过运行时验证；默认 `stage.dat` 部署由用户明确暂缓，不进入当前推进。**

### T9 — AI 输入生成器（已完成，Unity 运行时已验证）
- **C# 权威**：`InputRuntime.cs:16` `PrepareAiInputBasic` + 14 辅助函数：
  `AiBetweenX / AiPostCacheCoordinateAllowsSpecial / AiPreUpdateTarget3000SideEffect / AiUpdateOid33_19_16PredictedDuaDecision / AiUpdateOid52_1_2_21PreLabel591Decision / AiUpdateLabel591Oid51_2_18_7Decision / AiUpdateFirstDecision / AiUpdateTeammateGuardDecision / AiUpdateOid1ComboDecision / AiUpdateCloseOid1Decision / AiUpdateOid4ComboDecision / AiUpdateOid5ComboDecision / AiProcessSubOidGroup / AiSpecialOidForSubGate / AiProcessHelper`（行号见差异清单 §6.2）
- **Unity 落点**：`SimulationWorld.AiInput.partial.cs` 已完整承载主入口及直接/间接 helper 闭包，包含 runtime-slot target/cache、coordinate、team/history/held gate、C8/D3/D4/7A/7B 扫描、oid 决策组、move-mode/no-target 和三个 `AiProcessSub*` 尾部分支。
- **输入/生产接线**：human 输入保持在 oid51/52 maintenance 前；AI 在 maintenance 后、early specials 前生成 keys/edges/history 并立即进入同实体 Combo/action。主 roster 按 `isHuman` 设置 `AiControlled`/`RelationTeam`，opoint/stage character 默认启用 AI；current DAT type0 的 shared-DAT shell 同样覆盖。
- **验收**：fresh dotnet build 为 0 errors / 18 warnings；fresh Unity batch 日志明确返回“战斗运行时自检通过/自检完成”。自检覆盖 target/cache、coordinate、同 seed 确定性和 human 隔离，并回归 T0-T8。**T9 已完成。**

## 3. 已确认对齐（不要重复处理）

tick 主循环主干（含 `InputPhase`/`FrameMod12`/`FrameToggle` 统一推进）、全局 `ValidatePositiveLinks`、kind 0/4/9 主流程、kind 6/8/10/11/14、oid300、kind5 委托、M-5 死亡弹地、M-7 kind4+WeaponCount 翻转、HP/PP 自然恢复、heal/catch timer、state14 复活与 respawn pass、frame mp turn-around、frame202 HitStun=20、opoint 生成、cpoint 正值主流程、state 400/401/500/501、N30 触发、状态转换特效。

## 4. 确认可不移植

- **M-6 F8 强制掉武器**（`RunF8WeaponDrop`）：调试功能，Unity 不需实现（非冗余）。
- `RunMode2RandomWeaponDrop`、`InitStats`/mode2 postframe 分支：属于 C# baseline 的 F7-F9/debug 控制路径，不作为正式战斗对齐项。

## 5. 架构等价（🔷 严禁当冗余删除）

| Unity 机制 | 对应 C# | 说明 |
|-----------|---------|------|
| `LF2Character*Resolver` / `LF2Weapon*Resolver` | `NtsdCharacter`/`HitResolve`/`CPointRuntime`/`WeaponRuntime` 各段 | 组合模式拆分 |
| `LF2Entity` shared-DAT 输入桥（~900 行） | `InputRuntime.ApplyCharacterInput` 角色分发 | 服务 transform 后 wrong-shell 角色 |
| `NTSDEntityRuntime` 字段分桶 | `Entity` 大字段对象 | 运行时化，字段一一对应 |
| `FrameTransistor` hook | `FrameTick.Tick` 内联步骤 | 拆 hook 供覆写 |
| `SimulationWorld` 动态槽 | `Objects[400]` 固定槽 | 遍历顺序须保持 slot 升序 |
| `DirectWriteFramePreserveWaitCounter` | `SetFrameImmediate`（不清 attacking） | BMD-023，区别于会清 attacking 的 ImmediateFrame |

## 6. 排除范围（不对齐、不改）

菜单/选人/加载、HUD/结算、bg.dat 的 Z 可活动范围、相机、背景/纯渲染、音频播放系统、网络、回放/回滚基础设施、`src/Host/*`。注意：PreFrame 中改变实体存亡或 X 坐标的逻辑边界仍在战斗范围内。

## 7. 工作流（每个任务照做）

1. **溯源**：打开 C# 权威行号，读懂完整逻辑（含分支/常量/字段读取顺序）。
2. **索要原型**：向 Codex 要 unified diff patch（`sandbox=read-only`，严禁真实改码），作为逻辑参考。
3. **重写**：以原型为参考，写成符合 Unity 架构的生产级代码（用现有 resolver/hook/runtime 字段）。
4. **改码**：用 executor-high（多文件）或 executor（单文件）落地。
5. **Review**：改完立即用 Codex review 或 `code-reviewer-low`。
6. **验收**：按每项的「验收」标准，优先跑 `BattleRuntimeSelfCheck`；无法运行时说明原因，不谎报。
7. **更新清单**：完成一项，去 `csharp-vs-unity-battle-alignment.md` §10 勾选对应行。

## 8. 关键文件速查

| 用途 | 路径 |
|------|------|
| 全量差异清单 | `Assets/NTSD/Docs/csharp-vs-unity-battle-alignment.md` |
| C# tick 主干 | `ntsd_release_C#/src/BattleCore/Simulation/GameTick.cs` |
| C# 命中结算 | `ntsd_release_C#/src/BattleCore/Interaction/HitResolve.cs` |
| C# 帧推进 | `ntsd_release_C#/src/BattleCore/Frame/FrameTick.cs` |
| C# 输入+AI | `ntsd_release_C#/src/BattleCore/Input/InputRuntime.cs` |
| Unity 角色命中 | `Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterHitResolver.cs` |
| Unity 武器 | `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Weapon.cs` |
| Unity 帧推进钩子 | `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs` / `LF2Character.cs` |
| Unity pass 调度 | `Assets/NTSD/Scripts/Simulation/SimulationWorld.Passes.partial.cs` |
| Unity 候选收集 | `Assets/NTSD/Scripts/Animation/Character/BruteForceSceneQuery.cs` |

## 9. 优先级建议

T0-T9 主线、P1 BOUNDS-X 以及本轮 OPOINT-VIS、STEP10、TRANSFORM-SHELL、FRAME-ADV/FRAME-TICK 均已通过针对性 Unity 运行时自检。当前没有已确认但未实现的正式战斗代码差异，但这不是完整对局逐帧等价证明。最终 architect 已基于最新 diff、build 和 Unity runtime 证据完成 fresh 复核，结论 PASS、无 blocker。字段级 authority、Unity 现状和验收矩阵见完整差异清单 §10。

| 优先级 | 当前推进 |
|---|---|
| P0 | ✅ negative `vaction` 已完成并通过 fresh Unity batch：action signed + victim raw、throw raw frame/prev2、held-sync raw→flip/abs 与原始 signed vaction cpoint 坐标 |
| P1 | ✅ OPOINT-VIS、DAT transform shell、Step10 duration/mismatch throw/dir、non-character current-DAT cpoint、injury/stat 与 `HPLost` 已通过 2026-07-15 fresh runtime matrix |
| P2 | ✅ per-class `frame_advance` / current-DAT `frame_tick` 已通过 fresh runtime matrix；release 没有 oid9 专属 drain，不新增猜测分支 |
| P3 | collision snapshot/candidate 权威审计未发现生产差异；保留回归矩阵与未来 snapshot 消费期间 slot reuse 风险 |

T8 默认 `stage.dat` 部署由用户明确暂缓，不进入当前推进。

P0 验收覆盖 `CheckCpointNegativeActionMatrix`、`CheckCpointHeldSyncVactionMatrix`、`CheckCpointThrowRawAndTransformMatrix`，包含 real `LF2Character` 与 shared-DAT shell；最新独立 Unity batch 日志明确返回“战斗运行时自检通过/自检完成”。

本批已验收项：

- OPOINT-VIS：`CheckQueuedObjectPointPassBoundaries` 与 late-mutation 矩阵已验证 pre-advance、natural drop、逐实体 late 发布边界、real factory queue、父回收与高/low slot 可见性；过程修复 pending-destroy active-filter。
- STEP10：duration/mismatch tail、non-character/shared-DAT cpoint、input priority、KillStats/DamageStats 与 `HPLost` 矩阵已通过。
- TRANSFORM-SHELL / FRAME-ADV / FRAME-TICK：已验证 character/weapon `PS.BindRuntime`、逐 slot Transit/TU、SpecialAttack 单次 physics/frame_tick/type3 drain、`PpDisplay`、state14、negative next、state4000/8000 WFC/hit-stop 顺序、type1/2/4/6/oid999 current-DAT landing，以及 cross-SimOrder pending destroy 只注销一次。
- COLLISION-SNAPSHOT：当前 authority 审计未发现生产差异，不是修复项；对象引用 cache 与 C++ slot cache 的潜在分歧只在未来同 slot 即时复用 producer 出现时升级。

## 10. 实施进度（2026-07-14）

> 全量清单 §10 的 `[x]` 仅表示“已核实定性”，不表示已经实现；实际完成状态以本表为准。

| 任务 | 状态 | 关键落点 | 针对性自检 |
|------|------|----------|------------|
| T0 | **已完成 / Unity 运行时已验证** | `LF2Entity.ResolveArestCooldown`；`LF2CharacterHitResolver` 的 AttackExempt 写入改用 arest/vrest 公式 | `CheckArestCooldownRule` 已覆盖 arest/vrest 边界组合并通过 |
| T2（M-9） | **已完成 / Unity 运行时已验证** | `LF2Entity.RecordKind0Hit` 统一命中记录；`LF2Weapon.ApplyHitEffects` 的 kind 0 路径接入 | `CheckKind0HitRecords` 已覆盖 owner、timer、随机坐标范围和 10 槽上限并通过 |
| T3（M-14） | **已完成 / Unity 运行时已验证** | `LF2Entity.RunCommonFrameTick` 尾部写 `CdDefendLock=3`；runtime 字段、Reset 和 cooldown 衰减已承载 | `CheckFrameTickDefendLockTail` 已覆盖 110/114、早退、普通帧和 3→0 衰减并通过 |
| T1（M-8） | **已完成 / Unity 运行时已验证** | 共享 `LF2AlternateDamageResolver`；真实 `LF2Character.Hit` 与 `LF2CharacterDatHitResolver.TryResolveHit` 两入口；`NTSDEntityRuntime.Unk344`；稳定 3 槽 `KillStats`/`DamageStats` 与保 identity reset；`HPBound` 整数扣减且 `HPLost` 不变；heavy 顺序、character guard、clamp 后 vrest、SpecialAttack object-pass kind4/9 预处理、state1002 不写 `WeaponState` | `CheckAlternateHurtTriggerMatrix`、`CheckAlternateDamageCoreSideEffects`、`CheckAlternateDamageMotionTailMatrix`、`CheckAlternateDamageCharacterEntry`、`CheckAlternateDamageSharedDatEntry`、`CheckAlternateDamageHeavyWeaponEntries`、`CheckAlternateDamageInteractionVrest`、`CheckSpecialAttackDamagePreprocess` 均通过 |
| T4（M-1） | **已完成 / Unity 运行时已验证** | `Oid5152RuntimeMaintenanceAll`、`TryMergeOid7Or8Into51`、`TrySplitOid51BackToPair` 已落地到 `SimulationWorld.Passes.partial.cs` 与运行时身份维护链 | `CheckOid5152MergeSuccessAndDormantIsolation`、`CheckOid5152MergeCooldownOneTriggersSameTick`、`CheckOid5152SplitSuccessAndOddTruncate`、`CheckOid5152SplitFailurePartialRecovery`、`CheckOid5152DjaReleaseTriggersSameTickSplit` 均通过 |
| T5（M-2） | **已完成 / Unity 运行时已验证** | `SimulationWorld.PostFrameAdvanceDeathCleanupAll` 已补齐 respawn 两分支、队友平均落点、PP/HP/HpMax/Frame212/Y=-300、oid998 特效生成；`LF2Entity` / `LF2LivingObject` / `LF2Character` 已补 no-renderer 销毁注销链；`LF2ReferencePool` 已补惰性初始化，允许 self-check 直接 new 的角色安全释放 | `CheckRespawnPassWithoutStoredCount`、`CheckRespawnPassFreeEntityGate`、`CheckRespawnPassWithStoredCountAndEffectSpawn` 均通过 |
| T6（M-15/M-16） | **已完成 / Unity 运行时已验证** | 真实 `LF2CharacterHitResolver` 与 shared-DAT `LF2CharacterDatHitResolver` 均已对齐 kind15 authority 位移与 kind16 完整结算；角色 victim 不再走旧的 MaxMP 缩放或 `PS.vx/vz` 增量路径 | `CheckKind15CharacterWhirlwind`、`CheckKind16CharacterSideEffects` 均通过 |
| T7（§6.1 / combo） | **已完成 / Unity 运行时已验证** | `NTSDInputStateModule` 已承载 9 组 combo wrapper 与 oid6 DjaGuard；角色真实输入路径经 `RunPostCooldownInputPhase` 消费并落到 `ApplyFrameInput` | `CheckComboWrappersCharacterFrameJumps`、`CheckOid6DjaGuardComboHold` 已覆盖 9 组 frame jump、左右向切换、cooldown 清空，以及 oid6 guard hold/release 并通过 |
| T8（M-13 / stage） | **逻辑与接线已完成 / Unity 运行时已验证；默认资产部署暂缓** | `BattleStageCampaignLoader`、`ApplyMatchConfig` 生产接线；stage progression/runtime；立即刷敌、positive refill、清场推进、phase bound、精确身份字段与 dynamic slot 50+ | 三项 stage self-check 均通过；默认 `stage.dat` 部署由用户明确暂缓 |
| T9（AI） | **已完成 / Unity 运行时已验证** | `SimulationWorld.AiInput.partial.cs` 完整 AI 闭包；输入 pass 分段；runtime 字段与 roster/opoint bootstrap | `CheckAiTargetCacheCoordinateAndDeterminism`、`CheckAiHumanInputIsolation` 通过，并回归 T0-T8 |

最新已完成验证（2026-07-15 12:36:59）：fresh `dotnet build Assembly-CSharp.csproj /v:minimal /m:1` 为 **0 errors / 18 warnings**；UnityMCP 执行最新 `BattleRuntimeSelfCheck` 后日志明确包含“战斗运行时自检通过/自检完成”。本批回归既有检查，并通过 OPOINT-VIS、STEP10、TRANSFORM-SHELL、FRAME-ADV、FRAME-TICK 新增矩阵。这是针对性断言证据，不是完整对局逐帧等价证明。COLLISION-SNAPSHOT 权威审计无生产差异，仅保留未来 pass 内同 slot 即时复用风险。最终 architect 已对最新 diff、build 与 runtime 证据完成 fresh 复核，结论 PASS、无 blocker；T8 默认 `stage.dat` 部署继续暂缓。
