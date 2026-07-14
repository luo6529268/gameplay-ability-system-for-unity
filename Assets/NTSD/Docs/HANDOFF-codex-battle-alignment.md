# 接手文档 — NTSD C# → Unity 战斗逻辑对齐（Codex 无缝接手版）

> 生成：2026-07-13 ｜ 供 Codex 或任何接手者直接开工，无需追溯历史会话。

## 0. 你要做什么

把 **NTSD C# 战斗核心** 里 Unity 尚未对齐的战斗逻辑，逐条补齐 / 修正到 Unity 工程。
差异点**已全部逐行核实完毕**，本文给出每项的精确坐标 + 修复方向 + 验收标准。

- **唯一权威源**：`J:\QQFile\NTSD2.4\ntsd_release_C#\src\BattleCore`（NTSD C# 战斗核心）
  - 与反汇编 / FLF / C++ `ntsd_release` **无关**，不得引用它们作为依据。
- **被对齐工程**：`I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity\Assets\NTSD\Scripts`
- **完整差异清单（配套读）**：`Assets/NTSD/Docs/csharp-vs-unity-battle-alignment.md`
  - 本文是「行动版」，那份是「全量核实版」。有疑问回查那份的 §0~§9。

## 1. 铁律（不可违反）

1. **权威锁死**：任何改动必须能在 `ntsd_release_C#` 里找到对应代码段。找不到 → 暂停并说明，不得臆造。
2. **表现效果一致优先**：能逐行对齐就对齐；Unity 框架限制无法同构时，**运行时最终表现必须逐帧等价**（位置/帧号/速度/伤害/时序）。
3. **只新增不误删**：本文的 ❌ 项都是「C# 有 Unity 无」，是**新增**任务，**不是删除**。
4. **架构等价严禁删**：见 §5 清单——Unity 用 resolver/组合/hook 换方式实现的，不算冗余。
5. **排除范围不碰**：bg.dat 可活动范围、相机——不对齐，不改。

## 2. 任务清单（按建议顺序，坐标精确到行）

### T0 — 修真 bug：exemptVal 用错变量（已完成，Unity 运行时已验证）
- **C# 权威**：`HitResolve.cs:268` → `int itrArest = itr.Arest < 4 && itr.Vrest == 0 ? 4 : itr.Arest;`
- **Unity 落点**：`LF2Entity.ResolveArestCooldown`；`LF2CharacterHitResolver` 的 AttackExempt 写入已改用 arest/vrest 权威公式。
- **验收**：`CheckArestCooldownRule` 已覆盖 arest/vrest 边界组合并通过 Unity 运行时自检。

### T1 — ApplyAlternateDamage（injury/10 减伤路径，P0，待实施）
- **C# 权威**：`HitResolve.cs:629-680` `ShouldUseAlternateHurt`；`ApplyAlternateDamage` 从约 line 682 延伸到实际结束约 line 827，不能按历史记录截断在 line 789
  - 触发：victim 是角色 + (oid37/6/52 免疫窗口 OR prev2State==7 防御 + Bdefend<=60 + Hp>0 + 面向/dvx/特定 oid)
  - 核心：`reducedInjury = injury/10`（先 `injury = injury*100/FallDamageDiv` if div>0）；`Hp -= reducedInjury`；`HpMax -= reducedInjury/3`；`HitStateCount += Bdefend`；`attacker.FrameDelay=3`；`victim.FrameDelay=-5`；`Fall=80`(if Hp<=0)；KnockbackVx 特殊累积（755-779）
- **Unity 现状**：`LF2CharacterHitResolver.cs:213-214` 只有 `DefendInjuryFactor(0.5) * injury` 乘算，**无 alternate 整除路径，无 FrameDelay=-5**（全工程 grep `FrameDelay = -5` 0 命中）
- **做法**：编译阻塞已解除；先补齐 `KillStat`、`ComboCountAtk`、`DamageStats`、`HpMax` 等 runtime/stat 字段契约，再在 `LF2CharacterHitResolver` 完整落地 `ShouldUseAlternateHurt` + `ApplyAlternateDamage` 到权威方法实际结束，并在 kind 0/4/9 入口（现主流程前）判断调用。不得只搬扣血片段。
- **验收**：防御减伤走 injury/10 而非 *0.5；victim FrameDelay=-5；HpMax 同步减 reducedInjury/3。

### T2 — 武器命中 spark（M-9，已完成，Unity 运行时已验证）
- **C# 权威**：`HitResolve.cs:1150` `RecordKind0Hit`（timer：`Fall>60 ? sparkPhase*20 : sparkPhase*20+10`），312/320/**506** 三处调用，**武器命中路径（506）也调**。
- **Unity 落点**：`LF2Entity.RecordKind0Hit` 统一命中记录；`LF2Weapon.ApplyHitEffects` 的 kind 0 路径已接入。
- **验收**：`CheckKind0HitRecords` 已覆盖 owner、timer、随机坐标范围和 10 槽上限并通过 Unity 运行时自检。

### T3 — frame 110/114 → CdDefendLock=3（M-14，已完成，Unity 运行时已验证）
- **C# 权威**：`FrameTick.cs:208-209` → `if (frame==110 || frame==114) CdDefendLock=3;`
- **Unity 落点**：`LF2Entity.RunCommonFrameTick` 尾部写 `CdDefendLock=3`；runtime 字段、Reset 和 cooldown 衰减已承载。
- **验收**：`CheckFrameTickDefendLockTail` 已覆盖 110/114、早退、普通帧和 3→0 衰减并通过 Unity 运行时自检。

### T4 — oid 7/8 → 51 合体拆分（M-1，中）
- **C# 权威**：`GameTick.cs:1093` `RunOid5152RuntimeMaintenance` → `1123` `TryMergeOid7Or8Into51` + `1214` `SplitOid51BackToPair`
- **Unity 现状**：grep `MergeOid/TryMerge/SplitOid/Oid5152` 全 0 命中，**完全缺失**。
- **做法**：在 Unity tick 早期 pass（对应 GameTick step5，`RunEarlyStatePasses` 附近）新增等价 pass。
- **验收**：oid7/8 满足条件时合体为 51；51 满足条件时拆回一对，位置/帧/血量与 C# 一致。

### T5 — 复活 pass（M-2，大）
- **C# 权威**：`GameTick.cs:839-934` `RunRespawnPass`（tick step10）
  - 门控：state==14 + Hp<=0 + (KillCount>=0 OR Unk364==5 OR slot>=20) + HitStop∈(0,5)
  - 分支A（RespawnCount<=0）：Hp2Orig<2→FreeEntity；否则 Hp2Overlay-1、队友 X/Z 平均+随机、Pp=500、HpMax=Hp3、Hp=HpMax、HitStop=20、Frame=212、YInt=-300
  - 分支B（RespawnCount>0）：Pp=0、HpMax=RespawnCount、Hp3=HpMax、Hp=HpMax、RespawnCount=0、Unk364=1、oid∈[0x1E,0x24]→Unk318=0x8C、Frame=0xDB、FrameDelay=0xA、生成 oid998 复活特效
- **Unity 现状**：仅 `RespawnCount` 字段（`LF2Entity.cs:301`）+ `HitStun=30` 触发碎片（`LF2Character.cs:2205`），**主逻辑未移植**。
- **做法**：新增 Unity 复活 pass，落点对应 GameTick step10（`CleanupState9998Entities` 之后）。
- **验收**：角色死亡后按两分支正确复活/释放，位置/血量/帧/oid998 特效与 C# 一致。

### T6 — kind 15/16 副作用补齐（M-15/M-16，中，需验运行结果）
- **C# 权威**：`HitResolve.cs:1628` `ApplyKind15Or16` + `1737` `ApplyKind15Movement`
  - kind16 完整：Hp-、KillStat++、ComboCountAtk、`RecordSound("SFX_065")`、Frame=200、vrest 写入、LinkState==2 断开
  - kind15 位移：`KnockbackVx = Vx + (±1)`、真实 Vx=KnockbackVx、`KnockbackVz = Vz + (±0.5)`、`YInt=-2`；按对象类型分 vyStep（角色3.0 / 飞行道具3.0 / IronBall2.3）
- **Unity 现状**：kind16 `LF2CharacterHitResolver.cs:383-390` 只 `ImmediateFrame(200)`+MaxMP 缩放伤害，**缺** KillStat/ComboCount/SFX_065/vrest/LinkState 断开；kind15 `:373-380` 走 `PS.vx/vz` 增量，**未设 YInt=-2、未按类型分 vyStep**。
- **做法**：补齐 kind16 的副作用；kind15 改为 C# 的 KnockbackVx/Vx/Vz + YInt=-2 三段写法，按对象类型分 vyStep。
- **验收**：kind15/16 命中后运行表现（击退量、冻结帧、连击计数、音效）逐帧与 C# 一致。

### T7 — combo 连招 wrapper（大）
- **C# 权威**：`InputRuntime.cs:740` `RunComboWrappers`（9 组：Dra/Dla/Dld/Dlu/Drd/Dru/Djd/Dja/Daa/Dab + DjaGuard，含 oid6 Sasuke DjaGuard 特判），入口 `InputRuntime.cs:647`。
- **Unity 现状**：grep `RunComboWrappers/ComboDra/.../DjaGuard/combo_` 全 0 命中，**完全缺失**。
- **做法**：在 `LF2CharacterActionResolver` 玩家输入消费的最前段（对应 C# combo wrapper 位置）移植 9 组连招判定 + oid6 特判。
- **验收**：方向键+攻击/跳组合触发对应连招帧，与 C# 触发条件/目标帧一致。

### T8 — stage 波次刷敌（M-13，大）
- **C# 权威**：`GameTick.cs:2317` `ApplyCurrentWavePhaseAdvance` + `2350` `ApplyCurrentWaveImmediateStageSpawns` + `2226` `RefillCurrentWavePositiveStageSpawns`（配套 `StageProgression` + `StageSpawnRuntime*` 一整套，见 `SimulationWorld.cs:68-80`），tick step23。
- **Unity 现状**：grep `StageSpawn/StageProgression/WaveIdx` 全 0 命中，**完全缺失**。
- **做法**：新增波次运行时状态 + 立即刷敌 + 正数补充刷敌三段，落点对应 GameTick step23。
- **验收**：进入波次时按 stage 数据刷敌，清完波次推进，数量/位置/HP 与 C# 一致。

### T9 — AI 输入生成器（最大块，~600 行）
- **C# 权威**：`InputRuntime.cs:16` `PrepareAiInputBasic` + 14 辅助函数：
  `AiBetweenX / AiPostCacheCoordinateAllowsSpecial / AiPreUpdateTarget3000SideEffect / AiUpdateOid33_19_16PredictedDuaDecision / AiUpdateOid52_1_2_21PreLabel591Decision / AiUpdateLabel591Oid51_2_18_7Decision / AiUpdateFirstDecision / AiUpdateTeammateGuardDecision / AiUpdateOid1ComboDecision / AiUpdateCloseOid1Decision / AiUpdateOid4ComboDecision / AiUpdateOid5ComboDecision / AiProcessSubOidGroup / AiSpecialOidForSubGate / AiProcessHelper`（行号见差异清单 §6.2）
- **Unity 现状**：grep `AiUpdate/AiBetweenX/AiPostCache/AiPreUpdate/AiProcessSub/AiSpecialOid/AiProcessHelper/ThreatScan` 全 0 文件命中，**完全缺失**。
- **做法**：整块移植。先移主入口 `PrepareAiInputBasic`（目标选择 + C8 威胁扫描 145-250 + 守卫），再逐个移 oid 专属决策。
- **验收**：AI 角色在相同战场状态下做出与 C# 一致的移动/攻击/守卫/连招决策。**建议拆多个子任务分步验证**，别一次性写完。

## 3. 已确认对齐（不要重复处理）

tick 主循环主干、kind 0/4/9 主流程（除 T0 bug）、kind 6/8/10/11/14、oid300、kind5 委托、**M-7 kind4+WeaponCount 翻转**（`BruteForceSceneQuery.cs:602-615`）、**§5 HP/PP 自然恢复**（`LF2Character.cs:2534-2584`）、heal/catch timer、**帧推进主干 + state14 复活 HitStop**（`LF2Character.cs:2134-2163/2205-2211`）、**frame mp turn-around**（`LF2Entity.cs:3284-3321`）、frame202 HitStun=20、opoint 生成、cpoint 抓取、state 400/401/500/501、N30 触发、状态转换特效。

## 4. 确认可不移植

- **M-6 F8 强制掉武器**（`RunF8WeaponDrop`）：调试功能，Unity 不需实现（非冗余）。

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

bg.dat 可活动范围 / Z 边界钳制、相机（ProCamera2D vs CameraX）、bg 层动画、结算界面、`src/Host/*`、`src/Data/*`（Unity 用自己的 DatParser）。

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

先 **T0**（1 行 bug）→ **T3**（2 行）→ **T2**（小）→ **T1**（P0 减伤）→ **T4/T5**（合体/复活）→ **T6**（kind15/16）→ **T7/T8**（combo/波次）→ **T9**（AI，最后，拆多步）。

## 10. 实施进度（2026-07-14）

> 全量清单 §10 的 `[x]` 仅表示“已核实定性”，不表示已经实现；实际完成状态以本表为准。

| 任务 | 状态 | 关键落点 | 针对性自检 |
|------|------|----------|------------|
| T0 | **已完成 / Unity 运行时已验证** | `LF2Entity.ResolveArestCooldown`；`LF2CharacterHitResolver` 的 AttackExempt 写入改用 arest/vrest 公式 | `CheckArestCooldownRule` 已覆盖 arest/vrest 边界组合并通过 |
| T2（M-9） | **已完成 / Unity 运行时已验证** | `LF2Entity.RecordKind0Hit` 统一命中记录；`LF2Weapon.ApplyHitEffects` 的 kind 0 路径接入 | `CheckKind0HitRecords` 已覆盖 owner、timer、随机坐标范围和 10 槽上限并通过 |
| T3（M-14） | **已完成 / Unity 运行时已验证** | `LF2Entity.RunCommonFrameTick` 尾部写 `CdDefendLock=3`；runtime 字段、Reset 和 cooldown 衰减已承载 | `CheckFrameTickDefendLockTail` 已覆盖 110/114、早退、普通帧和 3→0 衰减并通过 |
| T1（M-8） | **待实施（编译阻塞已解除）** | 必须完整覆盖权威 `HitResolve.cs` 中 `ApplyAlternateDamage` 到实际结束（约 line 827），不是历史记录的 789；先补齐 `KillStat`、`ComboCountAtk`、`DamageStats`、`HpMax` 等 runtime/stat 字段契约 | 不得只搬扣血片段；完整落地后再补充并运行针对性自检 |

最新验证（2026-07-14）：全量 Roslyn RSP 为 **0 errors / 24 warnings**；`Temp/NTSD_BattleRuntimeSelfCheck.result` 当前明确为 **PASS**。因此 T0/T2/T3 已完成 Unity 运行时验收；T1 的编译阻塞已解除，但仍未实施。
