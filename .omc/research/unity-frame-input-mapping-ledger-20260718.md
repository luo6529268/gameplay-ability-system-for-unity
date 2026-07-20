# Unity 输入、帧推进、物理与 Runtime 映射审计（2026-07-18）

## 0. 范围、标准与结论边界

唯一权威是 `J:\QQFile\NTSD2.4\ntsd_release_C#`。正向核销输入为
`.omc/research/csharp-authority-frame-input-ledger-20260718.md`。Unity 证据来自当前生产源码，未使用旧对齐文档定义行为。

Unity 的 `MonoBehaviour`、`GameObject`、对象池、`Transform`、渲染回调和 CLR 壳类型允许保留；只有它们改变权威 C# 的 pass 顺序、RNG 消耗、runtime 真值或可观察结果时才判差异。

本报告是静态源码映射，不是编译、self-check 或 Play Mode 验收。已闭合 237 个权威 ID 的集合覆盖和 5 个差异/缺失 ID，但剩余 220 个 ID 尚未机械拆分为逐行 `equivalent` 与 `Unity-adapter` 两个最终计数。因此本报告不能支持“整个分区已完全对齐”的结论。

### 勘误说明（2026-07-18）

先前映射沿用了 authority ledger 对 `DoFrameJump` 的误读，将 `IN.JUMP.03` 错判为 `confirmed-difference`。权威 `InputRuntime.cs:926-934` 实际只清七个普通 Cd，不清 `CdDefendLock`；Unity 同样清七个普通 Cd并保留 lock，因此该 ID 已改为 `equivalent`。`IN.CD.02` 的 AI `CdDefendLock` 全实体递减差异不受影响。

状态词：`equivalent`、`Unity-adapter`、`confirmed-difference`、`missing`、`authority-unresolved`。

## 1. 执行摘要

| 类别 | 数量 | 说明 |
|---|---:|---|
| 权威唯一 ID | 237 | 按权威 ledger 表格首列机械提取；含 `FL.CASE2_4_12_14.A/B` 与 `FL.CASE6_9`。 |
| 已确认差异 ID | 4 | `FLOW.05`、`FLOW.09`、`IN.CD.02`、`RT.CHECK.01`。 |
| 缺失 ID | 1 | `RT.LINKS.01` 中的 `ReleaseTick` 存储/写回缺失。 |
| 权威未解析/跨分区 | 12 | `UNRES.*` 5 项和 `DEP.*` 7 项；不擅自判等价。 |
| 其余已定位映射 | 220 | 已定位 Unity 类型/方法；本轮未完成逐 ID 的 equivalent/adapter 机械拆分。 |
| Unity-only 根因 | 5 | 1 个 difference、2 个 unreachable、2 个 adapter/contract-only；见第 6 节。 |

最重要的正式可达结论：

1. Results active 后 Unity 仍运行普通战斗 pass；权威只运行 results tick。
2. `CdDefendLock` 的递减拥有者不同；成功输入跳帧时双方都只清七个普通 Cd并保留 lock。
3. Unity 在 holder 的 late FrameTick 改帧后额外同步 held 逻辑帧和位置，权威本 tick 没有第二次 held sync。
4. Authority `ReleaseTick` 在武器释放时写当前 tick 并进入 hash；Unity 无存储，snapshot 固定写 -1。
5. Unity parity snapshot 对多个已有 runtime 字段硬编码或错映射，不能作为“字段一致”的证据。

## 2. Tick / Input 已确认映射

| 权威 ID | Unity 证据 | 状态 | 结论 |
|---|---|---|---|
| `FLOW.01`、`FLOW.02` | `SimulationTickDriver.StepOneTickInternal/CanAdvanceTick` | equivalent | provider before/get、tick mismatch Empty、Apply、tick、checksum、after 的外层顺序闭合。 |
| `FLOW.03` | `SimulationWorld.ApplyFrameInputSet`、`PostCooldownHumanInputAll` | Unity-adapter | 完整 held snapshot 先入同 tick event buffer，再由实体 poll。正式 provider 每 tick 每 slot 唯一时与权威等效；重复 slot 的公开 contract 不同，见第6节。 |
| `FLOW.04` | `LF2Entity/LF2Character.RunCharacterInputPhase` | Unity-adapter | current-DAT dispatch + CLR 壳共享入口；不能因 CLR 类型存在本身判差异。 |
| `FLOW.05` | `NTSDBattleTickSystem.RunReleaseTick:17-29` | confirmed-difference | 缺少 `Results.IsActive` early return。Unity 会继续 cooldown、input、frame、interaction、late、stage/tail；权威只 `RunResultsTick` 后返回。Results 可由 `BattleResultsFlowAll` 正式激活。 |
| `FLOW.06`-`FLOW.08`、`FLOW.10` | `NTSDBattleTickSystem.RunFrameAdvancePhase`、`SimulationWorld.Passes` | equivalent / Unity-adapter | tick>1 input、early state、frame logic、serial advance、cpoint/held/candidate/resolve 的主顺序已定位；承载方式存在 Unity 适配。 |
| `FLOW.09` | `SimulationWorld.LateEntityUpdateAll:740-817` | confirmed-difference | 主 late 顺序存在，但 `SimFrameTick` 改 holder frame 后调用 `SyncHeldPoseAfterLateHolderFrameChange`。下游 `LF2HeldObjectRuntime.SyncHeldFrameAndPosition` 改 held Frame、Facing、FrameDelay、X/Y/Z/Zz，不只是刷新 Renderer；权威 late pass 无第二次 held sync。 |
| `IN.HUMAN.01`、`IN.CD.01` | `NTSDInputStateModule.UpdateFromBuffer`、shared shell poll | equivalent / Unity-adapter | Roll -> apply complete held state -> decrement -> edges 的结果闭合；Unity event queue 是宿主输入适配。 |
| `IN.CD.02` | `NTSDEntityRuntime.TickInputCooldowns:301-310`、`TickDefendLockCooldown:318-322`、`SimulationWorld.VrestTickAll:962-970` | confirmed-difference | 普通 7 Cd 在 human poll 递减；DefendLock 被拆到全 active entity 的 Vrest pass。权威 AI `PrepareBasic` 不调用 input TickCooldowns，Unity AI 的 lock 会递减。Human 消费前的有效相位通常一致，但 AI 结果不一致。 |
| `IN.EDGE.*`、`IN.HIST.*` | `NTSDEntityRuntime.ApplyInputEdges/PushInputHistory`、late N30 | equivalent | 交叉 Cd 语义、history code、tail/gate 和 N30 序列闭合。 |
| `IN.APPLY.*`、`IN.COMBO.*`、`IN.JUMP.01/.02` | `NTSDInputStateModule`、`LF2Entity.TryCharacterDatInputFrameJump` | equivalent / Unity-adapter | combo 顺序、DJA 早退不写回、费用、负 frame、999 与 PP-mode flip 已定位。 |
| `IN.JUMP.03` | `TryCharacterDatInputFrameJump -> SetSharedCharacterDatInputFrameDirect`、`NTSDInputStateModule.ClearActionAndDirectionCooldowns:409-413` | equivalent | 权威与 Unity 的成功 DoFrameJump 都只清 7 个普通 Cd，均保留 `CdDefendLock`。 |
| `MOVE.*`（17 ID） | `LF2CharacterActionResolver`、`LF2Entity` shared character-DAT action helpers | equivalent / Unity-adapter | standing/walk/run/jump/dash/heavy/crouch/recover/vtail 已定位；transformed DAT 通过 shared shell 适配。未用 CLR 类型差异直接判错。 |

## 3. AI 与 RNG

`AI.*` 48 个 ID 均已定位到 `SimulationWorld.AiInput.partial.cs`。目标扫描、缓存、边界、helper 调用顺序、`Label591/435`、target-team line-cover 异常都按权威行为保留；未发现可闭环的 AI 分支缺失。

RNG 计数必须区分“含调用的源码行”和“实际调用 occurrence”：

| 范围 | 权威 | Unity | 结论 |
|---|---:|---:|---|
| InputRuntime 全部 | 72 行 / 73 次 | 分散到 AI 与 action resolver | 权威 line 2007 同行有两次 RNG。 |
| AI 专属 | 69 行 / 70 次 | 65 行 / 70 次（不含 helper 声明） | 70 对 70；此前 69 对 65 只是按行计数，不能证明缺失。 |
| FrameAdvance frame logic | 11 行 / 11 次 | `LF2Entity.RunHitFa8/6Or9/13FrameLogic` 共 11 次 | 顺序为 hitFa8 的 4 次、6/9 的条件 1+2 次、13 的 3 次，按各可达分支对应。 |

Unity `Rand(modulus)` 使用 `Math.Max(1, modulus)`；当前 AI 正式路径中零 modulus 的唯一直接情形由 `Rand3<=0 || ...` 短路，其他分母在前置钳制后为正。本轮未把 helper 的防御性 clamp 判为正式差异。

## 4. Frame / Physics / Dispatch 集合核销

| ID 集合 | 数量 | Unity 映射 | 当前状态 |
|---|---:|---|---|
| `FT.*` | 26 | `LF2Entity.RunCommonFrameTick`、各 CLR 壳 `SimFrameTick`、opoint factory、late helpers | equivalent / Unity-adapter |
| `FA.*` | 3 | character `SimTransit`、non-character `SimTU`、shared frame velocity | equivalent / Unity-adapter |
| `FL.*` | 17 | `RunCurrentDatFrameLogicBeforeAdvance`、`RunHitFa*FrameLogic`、weapon resolver | equivalent / Unity-adapter |
| `PH.*` | 20 | `CharacterMechanics`、weapon/special/other frame-advance physics | equivalent / Unity-adapter |
| `WRAP.*` | 10 | Unity pass/entity forwarding methods | Unity-adapter |
| `DATA.*` | 7 | Unity DAT frame/opoint/cpoint models与常量 | equivalent / Unity-adapter |

`SerialTickAll` 对每个 runtime slot 固定调用 `SimTransit` 再 `SimTU`，但当前生产实现不是重复推进：character 壳在 `SimTransit` 推进而 base `SimTU` 为空；weapon/special/other 壳的 base `SimTransit` 为空而 `SimTU` 推进；transformed current-DAT 再走 shared route。该结构判为 `Unity-adapter`。

## 5. 137 字段映射与已确认例外

权威 ledger 的 137-field 逻辑集合按 Identity、Transform、Motion、Frame、Links、Transient、Stats、Residual、Input、Presentation 十组核销。Unity 不是一份 137 字段的同形类：部分在 `NTSDEntityRuntime`，部分在 `LF2Entity.Frame/Health`、hit record arrays、rest tracker 或 world candidate cache。结构拆分本身不是差异。

| 权威字段/组 | Unity 存储 | 状态 | 证据/例外 |
|---|---|---|---|
| Identity/Transform/Motion 主字段 | `NTSDEntityRuntime` + registry active/current-DAT category | Unity-adapter | Facing 由 `Dir` 承载；Active/category 由 world/current DAT 派生。 |
| Frame 主字段 | runtime + `LF2FrameInfo` + `FrameTransistor` | Unity-adapter | `SuppressJumpInit/JumpInitPending` 在 common FrameTick 内以局部变量承载，tick 末权威也清 false。 |
| Links 主字段 | runtime slot/link fields | Unity-adapter | slot/stable-id 名称混用必须按读写方解释，不能机械按名字判定。 |
| `ReleaseTick` | 无 | missing | 权威 weapon release 写 currentTick并进入 runtime hash；Unity snapshot line447固定 -1。 |
| `CdDefendLock` | `NTSDEntityRuntime.CdDefendLock` | confirmed-difference | 存储和成功 DoFrameJump 保留契约等价；差异仅在递减拥有者，见 `IN.CD.02`。 |
| Transient candidate arrays/Mp | runtime Mp + Unity candidate cache | Unity-adapter | scratch 载体不同；不得把 snapshot 固定零当作帧内 candidate 行为证据。 |
| `AbortRemainingHitPairs` | resolver 局部返回/loop break | Unity-adapter | 权威字段在消费后清 false；Unity用控制流承载。 |
| `Unk318` | `RenderPicOffset` | equivalent storage | Unity生产可写0x8C；snapshot却硬编码0。 |
| `Unk31C` | `WeaponFlightCounter` | equivalent storage | weapon HP/flight durability生产读写存在；snapshot却硬编码0。 |
| `Unk324/Unk33C` | `TransformOriginalObjectId/TransformTargetObjectId` | equivalent storage | transform 链存在；snapshot却硬编码-1。 |
| 四 `Block*` | `XBoundPositive/Negative`、`ZBoundPositive/Negative` | equivalent storage | hit写、physics读并清；snapshot却硬编码0。 |
| Presentation hit records | `LF2Entity` 10-slot arrays | Unity-adapter | 逻辑记录与 SparkRenderer 读取存在，存储不在 runtime 类。 |

`RT.CHECK.01` 判为 `confirmed-difference`：`BattleParitySnapshot.ProjectEntityRuntime` 至少存在以下错误投影：

- 空槽 `identity.category` 固定 3，权威 Reset/default category 为 0；空槽 commitments 因而可恒定不同。
- `releaseTick=-1`，无对应存储。
- `blockBackZ/blockFwdZ/blockLeft/blockRight=0`，忽略实际 bounds 字段。
- `unk318=0`、`unk31C=0`、`unk324=-1`、`unk33C=-1`，忽略已有 renamed storage。
- `grabbedTimer` 错映射到生产会写正负 link 的 Unity `GrabbedBy`；权威 `GrabbedTimer` 无生产写者。
- `ownerId` 取 `OwnerSlotIndex`，而 Unity `LF2Entity.OwnerId` 代理 `OwnerStableId`。
- `unk364` 在 RelationTeam 为0时回退 Team，会改变权威合法0值。

## 6. Unity-only 反向审计

| Unity-only 分支 | 生产证据 | 分类 | 结论 |
|---|---|---|---|
| Results active 仍跑普通 pass | `NTSDBattleTickSystem.RunReleaseTick` | difference | 正式可达，归 R1。 |
| late holder frame-change held sync | `LateEntityUpdateAll:764-776`、`SyncHeldPoseAfterLateHolderFrameChange` | difference | 写逻辑 frame/position，归 R3。 |
| `SimEntityCollision` | late line779；只有 base/interface 空实现，无生产 override | unreachable | 调用点可达但当前无状态效果；未来新增 override 必须重新核销。 |
| 六个 `Suppress*UntilTick` | runtime声明/reset与pass readers | unreachable | 全仓生产源码没有 writer；值保持0。`SuppressFrameTickUntilTick` 连 reader 也没有。不能称为正在工作的 spawn adapter。 |
| `SimInputBuffer` next-tick event queue | InputSystem callbacks、FrameInputSet current-tick enqueue、entity poll | adapter | Unity渲染回调到固定逻辑帧的宿主适配；60-tick cleanup不定义战斗规则。 |
| duplicate player slot | public `FrameInputSet` 可构造；正式 providers 每tick每slot唯一 | contract-only difference / production-unreachable | 权威重复项会重复Poll/cooldown/history；Unity只poll实体一次。正式 Host provider固定slot0/1各一次，Unity dense timeline按slot去重。 |

## 7. 根因去重与修复边界

| 根因 | 关联 ID | 最小修复边界 | 必要验证 |
|---|---|---|---|
| R1 Results early return缺失 | `FLOW.05` | tick header和results input之后阻止所有普通战斗pass；不要求借机实现完整菜单。 | results active实体运行1 tick，frame/position/HP/rest/stage/opoint不变。 |
| R2 input cooldown ownership错误 | `IN.CD.02` | DefendLock按权威的人类输入路径递减，不得由全实体 pass 递减。 | human/AI各测lock=3，确认只有人类 poll 路径递减。 |
| R3 late二次held逻辑同步 | `FLOW.09` | 删除或证明该同步只刷新表现；不得在权威没有的相位写held逻辑真值。 | holder late改frame，比较本tick末held frame/X/Y/Z和下一tick早期held sync。 |
| R4 ReleaseTick缺失 | `RT.LINKS.01` | 增加runtime storage、reset、weapon release writer、snapshot/hash投影。 | 普通drop/consume两条release路径断言current tick。 |
| R5 parity projection错误 | `RT.CHECK.01` | 按真实renamed storage投影；默认槽必须由同一默认契约生成。 | default400 slots + block/transform/weapon/release场景逐域hash对照。 |

## 8. 237-ID 集合完整性

以下按前缀列出权威唯一集合计数，合计237；本报告没有制造新权威 ID：

| 前缀 | 数量 | 处理 |
|---|---:|---|
| `FLOW` | 10 | 第2节；2 confirmed-difference。 |
| `CARRIER` | 6 | frame input/event carrier，equivalent/Unity-adapter。 |
| `IN` | 33 | 第2节；1 confirmed-difference。 |
| `MOVE` | 17 | character/shared-DAT action mapping。 |
| `AI` | 48 | 第3节；保留权威异常语义。 |
| `FT` | 26 | 第4节。 |
| `FA` | 3 | 第4节。 |
| `FL` | 17 | 第4节；含三个下划线ID。 |
| `PH` | 20 | 第4节。 |
| `DATA` | 7 | 第4节。 |
| `RT` | 28 | 第5节；1 confirmed-difference、1 missing。 |
| `WRAP` | 10 | Unity dispatch adapter。 |
| `UNRES` | 5 | authority-unresolved。 |
| `DEP` | 7 | authority-unresolved，交给 interaction/world/RNG/parser总账。 |
| **合计** | **237** | 4 difference + 1 missing + 12 unresolved + 220 located。 |

机械提取时必须使用允许下划线的 ID 正则；若只允许字母数字和点，会漏掉 `FL.CASE2_4_12_14.A`、`FL.CASE2_4_12_14.B`、`FL.CASE6_9`，错误得到234。

## 9. 验收声明

- 已完成：权威集合覆盖、主生产调用链、RNG occurrence口径、5个差异/缺失ID、Unity-only关键分支和根因去重。
- 未完成：剩余220个ID逐行状态拆分、编译、self-check、真实Play Mode操作与跨工程同输入trace运行。
- 因此当前只能称“静态差异审计已形成可执行根因清单”，不能称“输入/帧/物理分区已完全对齐”。
