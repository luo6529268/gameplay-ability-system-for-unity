# 战斗输入、帧推进与交互全量差异盘点（2026-07-18）

## 修复后核销（2026-07-18 16:46，覆盖本报告原始只读状态）

原 `authority-unresolved` 已全部定性，且本报告关联的 confirmed code differences 已进入 fresh full self-check：

- `UNRES.04`：N30 `triggerCode==100` 同队存活角色 `Unk3FC/Unk400` 广播已修，`CheckAudit11N30Code100Broadcast` 通过。
- `DATA-01A-D`：默认 running speed、600-frame cache、合法缺帧/非法帧语义和 cpoint action alias 已修；authored-frame consumer gates 已补齐，相关矩阵通过。
- `FW-RESULT-01`：fixed roster slot、relation identity、dormant/inactive 与 alive bucket 已修，focused results 矩阵通过。
- `RT.LINKS.01/LP-05`：formal release、consume 和 force-clear 的 link/index/ReleaseTick 写入边界已按 authority 收口；typed/generic release 矩阵通过。
- `LP-03`：formal throw 的 Unity-only `Zz=1` 已移除，release 后 `Zz=0` 断言通过。
- `FW-FLOW-01`：cooldown-before-human-input 顺序已修并通过 focused check。

fresh 证据为 source `16:44:31.210` < Unity DLL `16:45:52.868` < result `16:46:29.080` **PASS**；`dotnet build` **0 errors / 18 warnings**。`DEP.RNG.01` 继续作为 per-world lockstep adapter；`RT.CHECK.01` 仍是 trace/validator adapter 而非战斗 runtime 差异。4 组 Play Mode 由用户验证，本段不声明完整逐帧 certificate。

本报告是只读审计结果，目标是先完成差异盘点，再进入修复阶段。审计没有修改 Unity 生产代码，也没有把 DAT 文件表示差异、T8 默认 `stage.dat` 部署或固定世界相机适配列为战斗逻辑差异。

唯一权威是 `J:\\QQFile\\NTSD2.4\\ntsd_release_C#\\src`。Unity 的 `MonoBehaviour`、`GameObject`、`Transform`、对象池、输入事件和渲染回调只视为宿主适配；只有当它们改变 runtime 真值、tick 顺序、输入边沿、对象生命周期或命中结果时，才计为战斗差异。

## 1. 审计边界与总账

| 分区 | 权威 ID | 当前复核范围 | 证据来源 |
|---|---:|---|---|
| 主循环、bootstrap、world、实体壳 | 172 | 全部已有 ID 重新核对；F1-F7 生产修复已在源码中存在 | `.omc/research/csharp-authority-framework-ledger-20260718.md`、`unity-framework-mapping-ledger-20260718.md` |
| 输入、组合键、帧推进、physics、runtime snapshot | 237 | 全部 ID，逐项按旧完整映射和最新 Unity 代码复核 | `.omc/research/csharp-authority-frame-input-ledger-20260718.md`、`unity-frame-input-mapping-complete-20260718.md` |
| collision、hit、cpoint、抓取、持有、投掷、opoint、stage | 105 | 全部 ID，旧 I1/I2 已按最新代码重新核对 | `.omc/research/csharp-authority-interaction-ledger-20260718.md`、`verify-unity-interaction-mapping-20260718.md` |

旧映射中的 `4 confirmed + 1 missing` 不能直接作为当前状态：`FLOW.05`、`FLOW.09`、`IN.CD.02` 和 `ReleaseTick` 已在最新源码中补齐或收口；`RT.CHECK.01` 仍存在工具投影差异。静态映射覆盖完整 ID 集合，不等于完成任意对局的逐帧证明。

## 2. 当前确认差异

### DIFF-RT-CHECK-01：Parity snapshot 遗失三个权威暂态字段

| 项目 | 证据 |
|---|---|
| 权威调用链 | `BattleCore/Runtime/NtsdEntityRuntime.cs:124-125,328` 定义 `SuppressJumpInit`、`JumpInitPending`、`AbortRemainingHitPairs`；`BattleCore/Entity/CharacterSync.cs:123-124,295,413-414` 将它们纳入 snapshot 比较和 hash。`HitResolve.cs:45-50,1315` 在同一命中循环中设置/消费 abort 标记；`FrameTick.cs:70-71,117-130,162` 维护跳跃暂态。 |
| Unity 调用链 | `Assets/NTSD/Scripts/Simulation/BattleParitySnapshot.cs:429-432,520` 将 `jumpInitPending`、`suppressJumpInit`、`abortRemainingHitPairs` 固定写为 `false`。`NTSDEntityRuntime` 没有同名持久字段；`LF2Entity.cs:5468-5527` 在 frame advance 使用局部变量，`LF2Character*InteractionResolver.cs` 使用局部 `abortAfterSuccessfulHit`。 |
| 预期 | 快照/哈希应能反映权威 runtime 中这三个字段在 tick 内的值。 |
| 实际 | Unity 普通生产路径使用局部变量完成同 tick 控制流，但 parity snapshot 无法观测或比较这些暂态；在 snapshot 断点或跨实现 checksum 对拍时会出现字段缺失/恒 false。 |
| 分类 | **confirmed-difference（parity trace 工具；不等同于已证明的普通命中结果差异）** |
| 复现 | 在 frame 212 jump-init 或 oid300 redirect 的命中循环中插入 snapshot 断点；C# snapshot 的对应字段可为 true，Unity snapshot 始终为 false。当前 self-check 未覆盖中间断点。 |
| 后续 | 必须先记录到修复清单；修复时补齐 Unity runtime 暂态字段或明确将它们从双方 trace 契约中一致排除，然后重新做 checksum/snapshot 对拍。 |

## 3. 已确认、但当前源码已收口的差异（不得重复计入开放项）

这些项目曾是审计中的正式差异，本轮仅记录“已写入源码，仍需按验收层级复核”，不将旧 PASS 自动升级为完整对齐。

| ID/簇 | 权威链 | 当前 Unity 代码 | 当前判断 |
|---|---|---|---|
| `FLOW.05` results-only tick | `BattleCore/Simulation/GameTick.cs:44-49` results active 时只执行 results flow 后 return | `Assets/NTSD/Scripts/Simulation/NTSDBattleTickSystem.cs:25-30` 同样 return；普通 frame/collision pass 不再进入 | 生产差异已收口；需 fresh self-check/Play 证据 |
| `FLOW.09` late held 二次同步 | `GameTick.cs:127-134,1513` late pass 没有第二次 held step12 | `SimulationWorld.Passes.partial.cs:740-824` late pass 只执行对象自身 late tick、opoint、tail、PrevFrame；未发现第二次 held step12 | 生产差异已收口；需 fresh 场景复核 |
| `IN.CD.02` AI defend lock | `Runtime/NtsdEntityRuntime.cs:619` 输入 cooldown 由 human poll 路径驱动，AI 不独立递减输入 Cd | `SimulationWorld.Passes.partial.cs:936-973` `VrestTickAll` 只递减 ARest，并按 authority 规则清 `AttackExempt`，没有 `TickDefendLockCooldown` | 生产差异已收口；需 fresh AI/input 证据 |
| `RT.LINKS.01 / ReleaseTick` | `Interaction/WeaponRuntime.cs:293,303` 投掷/正式释放写当前 tick；runtime reset 为 -1 | `NTSDEntityRuntime.cs:34,342` 有字段；`LF2WeaponReleaseFlowResolver.cs:23-34` 写入；self-check `BattleRuntimeSelfCheck.cs:2975-3099` 覆盖 reset/throw/drop 分支 | 缺失字段已补齐；需 fresh snapshot/Play 复核 |
| `INT-HIT-005 / I1` IronBall gate | `Interaction/HitResolve.cs:237,365,491,840,866,916,994,1260,1727,1811,1914` 按 DAT `WeaponType.IronBall` 分支 | 当前 `BruteForceSceneQuery.ItrAllowed` 不再使用旧 type6 gate；对应类型通过 `LF2ObjectType`/当前 DAT type 解析 | 旧 gate 差异已收口；需 IronBall 专项运行证据 |
| `INT-OP-002 / I2` late opoint 坐标 | `Frame/FrameTick.cs:late opoint` 使用整数逻辑坐标与 `Z+1` 语义 | `LF2ObjectPointFactory.cs:229-250` 使用 `Runtime.XInt/YInt` 和 `Runtime.Z + 1.0`，并设置 direct runtime/int position | 旧坐标差异已收口；需真实 late opoint 运行证据 |
| F1-F7 framework 根因 | `GameTick.cs`/`SimulationWorld` stage、roster、spawn、results、tail、rest 契约 | 当前 `SimulationTickDriver.ApplyMatchConfig` 不提前首波；固定 0..7 roster；`AppManager.SetupBattleCharacters` 消费 stage bounds/RNG；stage spawn raw rest 在注册后恢复；tail 清 carrier；camera_x 保持 0 | 源码已收口；尚未由本报告宣称全量行为通过 |

## 4. 历史 unresolved 清单（已由 BATTLE-AUDIT11 代码核验收口）

本节保留原始盘点内容以便追溯。根据 `.omc/research/final-verify-unres-02-05-code-parity-20260718.md` 及三份 `verify-authority-unresolved-*` 报告，以下项目已经完成代码层定性；因此本节不再代表当前开放的 `authority-unresolved` 数量。当前 code-only scope 下该数量为 **0**。

### 4.1 已定性为 equivalent / Unity-adapter

`UNRES.01`、`UNRES.02`、`UNRES.03`、`UNRES.05`、`DEP.INT.01`-`DEP.INT.04`、`DEP.WORLD.01`。

### 4.2 已定性为 confirmed code difference，部分修复待 fresh self-check

`UNRES.04`、`DATA-01A`、`DATA-01B`、`DATA-01C`、`DATA-01D`；关联确认项为 `FW-RESULT-01`。`DEP.RNG.01` 已改列为 Unity-adapter/policy-open（算法等价，owner/reset 边界待策略决定）。`UNRES.04` 与 DATA-01A-D 的首轮生产修复已落地，但最新 fresh Unity full self-check 仍被 `CheckStateTransformLandingMatrix` 的 transformed landing fixture 断言阻塞，实际为 `frame=60/runtimeFrame=60/durability=15/state=1004/vy=0/vx=8.4`；`FW-RESULT-01` 仍未修复。`DEP.DATA.01` 按最终报告拆分为 DATA-01A-D，原始聚合 ID 仅保留作历史索引。

### 4.3 非正式 runtime 差异的代码结论

`DATA-01E` 为当前 consumer 已屏蔽的 Unity-adapter/masked；`DATA-01F` 为 schema-only omission；`DATA-01G` 为 closed in source。它们不计入 confirmed runtime difference。

以下是原始盘点时的 unresolved 表，保留为历史记录：

## 4.4 原始盘点表（历史）

以下项目不是“已经确认的差异”，而是权威 ledger 明确要求继续追踪的 `authority-unresolved`。在完成调用链、字段默认值、重置时机和运行证据前，不得标成 equivalent，也不得擅自修复。

| ID | 权威边界 | Unity 对应点 | 需要的证据 |
|---|---|---|---|
| `UNRES.01` | `InputRuntime` 中 `Unk*` 字段的正式语义 | `SimulationWorld.AiInput.partial.cs` 保留字段 | 同一 seed、多 tick AI trace |
| `UNRES.02` | label-only helper 的分支归属 | Unity AI/input helper | 逐分支调用顺序对照 |
| `UNRES.03` | `mpDelta/value==550` 等数据分支 | `LF2Entity` frame/input helpers | 同一 DAT frame fixture |
| `UNRES.04` | unknown `Unk360/Unk3FC/Unk400` 生命周期 | `NTSDEntityRuntime` 与 AI late trigger | reset、clone、pool reuse trace |
| `UNRES.05` | authority 仅能由行为推断的 label/状态关系 | Unity state/transition resolver | 受控输入序列与字段快照 |
| `DEP.INT.01` | `Entity.ResetHitCandidates`、完整 reset 字段 | `LF2Entity.Reset`、pool lifecycle | 逐字段 reset/复用对拍 |
| `DEP.INT.02` | `FrameRuntime.SetFrameImmediate` wait/next/prev 写入 | `DirectWriteFrame*` helpers | frame transition snapshot |
| `DEP.INT.03` | `FrameTickRuntime.Tick` 与 late lifecycle | `SimulationWorld.LateEntityUpdateAll` | late pass 逐实体顺序 trace |
| `DEP.INT.04` | hit-record capacity/replacement helper | `LF2Entity.RecordKind0Hit`/presentation | 多命中同 tick 容量 fixture |
| `DEP.WORLD.01` | world slot、VRest/ARest、RNG owner | `SimulationWorld` registry/rest/Rng | 同 seed full tick compare |
| `DEP.RNG.01` | RNG 算法/seed 归属 | `SimulationWorld.Rng`、character RNG | 每个调用点序列和结果对拍 |
| `DEP.DATA.01` | DAT parser/population 与 runtime consumer | `CharacterAnimtorManager`、DatParser | 结构化 fixture；不要求 raw DAT 文件相同 |
| `RT.CHECK.01` 之外的 snapshot 字段 | authority snapshot 的字段是否全部可观测 | `BattleParitySnapshot.ProjectEntityRuntime` | 将本节 DIFF-RT-CHECK-01 与 runtime trace 一起收口 |

## 5. 237 个 Frame/Input ID 的完整分类索引

以下索引与 `.omc/research/unity-frame-input-mapping-complete-20260718.md` 一一对应，避免把“适配”误记为“差异”。

| 分类 | ID 集合（完整分组见原 ledger） | 数量 | 本轮判断 |
|---|---|---:|---|
| equivalent | `FLOW.01/02`; `IN.EDGE.01`, `IN.EDGE.02R/L/U/D/A/DEF/J`, `IN.HIST.01/02/03`, `IN.JUMP.03`; `DATA.CONST.01/02`; `RT.RESET.01`, `RT.COPY.01`, `RT.CLONE.01`, `RT.COPYENTITY.01`, `RT.APPLY.01`, `RT.IDENTITY.01`, `RT.TRANSFORM.01`, `RT.MOTION.01`, `RT.FRAME.01`, `RT.TRANSIENT.01/02/03`, `RT.STATS.01`, `RT.RESIDUAL.01/02/03/04`, `RT.INPUT.01-06` | 39 | 逻辑字段/顺序可直接对应；仍需 fresh runtime 证据 |
| Unity-adapter | `FLOW.03/04/06/07/08/10`; `CARRIER.01-06`; `IN.HUMAN.01`, `IN.CD.01`; `IN.APPLY.*`; `IN.COMBO.*`; `IN.JUMP.01/02`; 全部 `MOVE.*`; 全部 `AI.*`; 全部 `FT.*`, `FA.*`, `FL.*`, `PH.*`; `DATA.FRAME.*`, `DATA.OP.01`, `DATA.CP.01`, `DATA.CHAR.01`; `RT.SYNC.01`, `RT.PRESENT.01`, `RT.ENTITY.01`; `WRAP.*` | 181 | Unity 宿主实现不同，但当前静态调用链保留权威分支；不能用 adapter 标签替代运行时验收 |
| confirmed-difference | `UNRES.04`、`DATA-01A/B/C/D`、关联 `FW-RESULT-01`（旧 `FLOW.05/FLOW.09/IN.CD.02` 已收口） | 6 个当前代码确认项（另有 `RT.CHECK.01` parity trace 工具差异） | 首轮修复已落地但 fresh full self-check 仍被 transformed landing fixture 阻塞；`FW-RESULT-01` 待修复。`DEP.RNG.01` 归入 Unity-adapter/policy-open；`RT.CHECK.01` 仍按工具差异单独记录 |
| missing | `RT.LINKS.01 / ReleaseTick`（当前字段已补齐） | 1（历史映射）/0（当前源码） | 需 fresh snapshot 证明已真正接入 |
| authority-unresolved | 无（原 `UNRES.02`-`UNRES.05` 已由 BATTLE-AUDIT11 定性；原始 12 项保留于 §4.4） | 0（code-only scope） | 不再有未定性代码项；Play Mode 与非脚本表现另按用户范围处理 |

## 6. 用户报告的 Naruto/武器场景复核

这些场景必须使用真实 `NTSD_Battle`、实际输入边沿和生产 DAT；self-check fixture 不能替代 Play Mode。现有文档中记录过以下结果，但本报告不把历史日志自动视为本轮 fresh 证据。

| 场景 | 权威链 | 关键预期 | 现有记录 | 当前审计结论 |
|---|---|---|---|---|
| Naruto 防下跳六分身 | `InputRuntime.RunCombo` -> frame271/272/273 -> `FrameTick` opoint -> `ObjectPointFactory` 递归 oid205/204 -> 6 x oid33/action307 | 六个 clone、关系字段、renderer 可见，后续 307/219 生命周期不提前销毁 | 主文档 §4.3 记录 `L -> L+S -> L+S+K` 与 6 visible renderer | 已有定向证据；需在最终修复批次后复跑 |
| Naruto 防前跳螺旋丸 | combo frame240 -> oid434/action396/397 -> held `wpoint`/step12 -> attack 257/258/259 | weapon 层级、整数挂点、holder 移动同 tick 跟手，攻击键驱动 held DAT 而非普通武器 | 主文档记录 Rasengan Play PASS | 已有定向证据；需 fresh 复核渲染排序与攻击链 |
| Naruto 奔跑防跳 | running frame102 -> kind3/cpoint 295-299 -> 275-279 -> 86-88 | 命中后继续下一招，caught/catcher link 持续到后续 cpoint | 主文档记录链路 Play PASS | 已有定向证据；需 fresh 复核 link 与命中对象 |
| 投掷武器 | `WeaponRuntime` release -> `HitResolve` standard/alternate -> `ARest/VRest/AttackExempt` | 首击只结算一次，weapon state/FrameDelay/AttackExempt 按 authority 结束 | 主文档记录 HP 单次下降、35 tick 冷却后无二次命中 | 已有定向证据；需 fresh 复核不同 weapon/target 组合 |

## 7. 结论与修复前置

1. 本轮已把现有三份 authority ledger 覆盖的 514 个 ID 重新归档，明确区分了“当前确认差异”“已修复待验收”“Unity 适配”和“权威未决”。
2. 当前能直接从源码确认的开放项是 `DIFF-RT-CHECK-01` parity projection；旧 BA8 的前三个生产差异、`ReleaseTick`、IronBall gate 和 late opoint 坐标已有对应实现，不能继续按旧状态重复计数。
3. 原 12 个 `authority-unresolved` 是本报告生成时的历史原始计数；BATTLE-AUDIT11 已完成代码证据闭合，当前 code-only scope 下为 0。已定性的 confirmed code differences 仍须修复，不能据此宣布“完整战斗逻辑已对齐”。
4. 下一阶段应只按本报告和两份主对齐文档中的记录逐项修复；每项修复后必须取得 0 编译错误、fresh self-check，以及必要的真实场景 Play Mode 证据。

## 8. Unity-only dormant stubs（不计为当前战斗差异）

`Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterWeaponLinkResolver.cs:442-455` 保留了 `ClearConsumedHeldWeaponReference`、`ClearReleasedHeldWeaponReference` 和 `ClearHolderLinkRuntimeOnly` 三个 TODO 空实现。反向扫描确认它们目前没有生产调用者；权威 C# 也没有同名 API，正式释放/持有清理由 `WeaponRuntime` 的主流程完成。`RunWeaponSyncHeldStep10` 仅调用 `GetHeldEntity()`，而正式调用点的 `LF2Character.RunWeaponSyncHeldStep10` 先执行基类 cpoint，再由 resolver 做关系读取。

因此这些 stub 记录为 **dormant Unity-only adapter / authority-unresolved dependency**，不是当前可达的战斗行为差异；若未来出现调用者，必须先回到权威 `WeaponRuntime` 调用链确认语义后再实现，不能根据 TODO 名称猜测。
