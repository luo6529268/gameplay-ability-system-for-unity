# Unity frame/input 独立映射草案（2026-07-18）

## 0. 审计边界

- 唯一权威：`J:\QQFile\NTSD2.4\ntsd_release_C#`。
- Unity 目标：当前工作树的 `Assets/NTSD/Scripts` 生产源码。
- 本轮只读生产代码，只新增本审计稿；未修改生产代码、测试或两份主文档。
- Unity-native 的 `GameObject`、`Transform`、CLR 壳类型、对象池、渲染回调可作为适配保留；只有改变权威 pass 顺序、runtime 真值、RNG 消耗或可观察战斗结果才判差异。
- DAT 读取适配、固定 camera、T8 默认 `stage.dat` 部署、F1/F8/Mode2/step-wait、results UI、普通 HUD/音频和完整 rollback 不计本轮差异。Results active 后普通战斗 pass 是否停止仍属于战斗逻辑。

### 勘误说明（2026-07-18）

本草案曾沿用 authority ledger 的误读，把 `IN.JUMP.03` 判为成功 `DoFrameJump` 少清 `CdDefendLock`。权威 `InputRuntime.cs:926-934` 实际与 Unity 一样，只清七个普通 Cd并保留 `CdDefendLock`；该项现改为等价。R2 仍保留，但只包含 `IN.CD.02` 的 AI cooldown ownership 差异；五个根因总数不变。

## 1. 机械完整性

权威 ledger 已按 UTF-8 完整读取 426 行。表格首列机械提取得到 237 个唯一 ID；允许下划线的正则必须覆盖 `FL.CASE2_4_12_14.A/B` 和 `FL.CASE6_9`。

| 前缀 | ID 数 | Unity 落点 |
|---|---:|---|
| `FLOW` | 10 | `SimulationTickDriver`、`NTSDBattleTickSystem`、`SimulationWorld.Passes` |
| `CARRIER` | 6 | `FrameInputSet`、`SimulationWorld.FrameInput`、`SimInputBuffer` |
| `IN` | 33 | `NTSDInputStateModule`、`NTSDEntityRuntime`、`LF2Entity/LF2Character` |
| `MOVE` | 17 | `LF2CharacterActionResolver` 与 current-DAT shared shell helpers |
| `AI` | 48 | `SimulationWorld.AiInput.partial.cs` |
| `FT` | 26 | `LF2Entity.RunCommonFrameTick`、late pass、opoint factory |
| `FA` | 3 | `SimTransit/SimTU`、shared frame advance |
| `FL` | 17 | `LF2Entity.RunHitFa*FrameLogic`、weapon frame-logic resolver |
| `PH` | 20 | `CharacterMechanics`、shared/non-character frame advance |
| `DATA` | 7 | Unity DAT models/converter 与 simulation constants |
| `RT` | 28 | `NTSDEntityRuntime`、`LF2Entity` proxy、parity snapshot |
| `WRAP` | 10 | Unity pass/entity forwarding adapter |
| `UNRES` | 5 | 保持 `authority-unresolved` |
| `DEP` | 7 | 已定位 interaction/world/RNG/DAT 跨分区依赖 |
| **合计** | **237** | **集合无遗漏、无重复** |

本草案不伪造 220 个尚未逐行拆分的 `equivalent`/`Unity-adapter` 精确计数。完整集合中已独立闭环 5 个根因（其中 4 个战斗规则/状态根因、1 个 trace 根因）；其余 ID 已定位到上述生产实现族，但仍需最终 ledger 逐行拆分。

## 2. 五个重点根因复核

### R1 Results active early return 缺失

状态：`confirmed-difference`，正式可达。关联 `FLOW.05`。

- 权威 `GameTick.Run` 在 tick header 后发现 results active，仅执行 results tick 后返回。
- Unity `SimulationTickDriver.cs:180-205` 每 tick 固定调用 `ApplyFrameInputSet -> NTSDBattleTickSystem.RunReleaseTick`。
- Unity `NTSDBattleTickSystem.cs:17-29` 没有 `Results.IsActive` 分支，继续进入 cooldown、输入、frame advance、interaction、stage、late 与 tail。
- `SimulationWorld.StageRender.partial.cs:174-264` 可在正式 mode1 流程把 `Results.Phase` 激活到 200；`BattleRuntimeState.cs:298` 的 `IsActive` 因而不是测试专用状态。

结果：summary 激活后的后续 tick 仍可移动、换帧、碰撞、命中、生成 opoint 或推进 stage，与权威直接冲突。

### R2 CdDefendLock 递减拥有者差异

状态：`confirmed-difference`，正式可达。关联 `IN.CD.02`。

权威 `PollHumanInput -> TickCooldowns` 清楚限定在人类 poll 路径；AI `PrepareBasic` 只 roll/clear/edges。Unity 普通 7 Cd 在 `NTSDInputStateModule.cs:160-169` 的 human poll 递减，但 `CdDefendLock` 被拆到 `NTSDEntityRuntime.cs:318-322`，再由 `SimulationWorld.Passes.partial.cs:962-970 VrestTickAll` 对全部 active entity 调用。因此 AI 的 lock 也每 tick 递减。

该根因只需修正 AI/人类 cooldown ownership；测试覆盖 human/AI 的 lock 递减即可。成功/失败跳帧不再属于差异修复项。

### R3 late holder 换帧后二次 held 同步

状态：`confirmed-difference`，正式可达。关联 `FLOW.09`、`DEP.INT.04`。

- Unity `SimulationWorld.Passes.partial.cs:764-776` 记录 holder late tick 前 frame；若 `SimFrameTick` 改帧，立即调用 `SyncHeldPoseAfterLateHolderFrameChange`。
- `SimulationWorld.Passes.partial.cs:828-853` 经 `LF2HeldObjectRuntime.SyncHeldPose` 更新 held 的逻辑 frame/朝向/frame delay/X/Y/Z，而不只是 `Renderer.ForceRefreshPresentation`。
- 权威 late pass 在本 tick 的 earlier held sync 之后没有第二次 held sync；holder late 换帧只影响下一次权威同步边界。

因此该分支会让 held 武器在同 tick 提前一阶段取得新逻辑挂点与帧，不能按“Unity 渲染适配”保留。

### R4 ReleaseTick 缺失

状态：`missing`，正式可达。关联 `RT.LINKS.01`、`RT.RESET.01`、`RT.CHECK.01`、`DEP.INT.04`。

- 权威 `WeaponRuntime.cs:293,303` 在两条 release 路径写 `held.ReleaseTick=currentTick`；字段默认/reset 为 -1，并进入 `CharacterSync` runtime match/hash。
- Unity `NTSDEntityRuntime.cs:12-175` 无 `ReleaseTick`；全仓生产代码也无 release tick writer。
- Unity `BattleParitySnapshot.cs:447` 直接输出 `releaseTick=-1`，掩盖正式 release 后的状态差异。

`GrabbedTimer` 与 `StuckVictimSlot` 也没有独立 Unity storage，但权威当前只有 reset/copy/hash 读写，未发现正式业务 writer；它们是 runtime/trace 契约缺口，不与生产可达的 ReleaseTick 混为一个行为差异。

### R5 parity snapshot 错投影

状态：验证工具 `confirmed-difference`；它不能单独证明战斗规则错，但会制造或掩盖 trace 差异。关联 `RT.CHECK.01`。

`BattleParitySnapshot.ProjectEntityRuntime` 的问题包括：

| 投影 | Unity 当前 | 正确 Unity storage/语义 |
|---|---|---|
| `ownerId` | `OwnerSlotIndex`（line 394） | `LF2Entity.OwnerId` 代理 `OwnerStableId`（`LF2Entity.cs:62-67`）；slot owner 是另一字段 |
| `unk364` | `RelationTeam!=0 ? RelationTeam : Team`（line 397） | `RelationTeam` 的合法 0 必须原样保留，不能 fallback Team |
| `grabbedTimer` | `GrabbedBy`（line 440） | 两者语义不同；authority `GrabbedTimer` 当前无业务 writer |
| `releaseTick` | 固定 -1（line 447） | Unity 缺 storage/writer，不能硬编码掩盖 |
| `Block*` | 四项固定 0（lines 522-525） | `XBoundPositive/Negative`、`ZBoundPositive/Negative` |
| `Unk318/31C/324/33C` | 固定默认（lines 530-538） | `RenderPicOffset`、`WeaponFlightCounter`、`TransformOriginalObjectId`、`TransformTargetObjectId` |
| 空槽 `category` | 固定 3（line 390） | 权威 runtime default/reset 为 0 |

此外 `jumpInitPending/suppressJumpInit` 固定 false。Unity common FrameTick 用局部变量承载正常有效 DAT 路径，tick tail 后确实归 false；但若要做逐分支 checksum，仍需确认权威异常/越界 frame 早退是否会把字段保留到 tick 末，不能直接以常量代替完整契约。

## 3. 237 ID 完整登记

下列登记逐项覆盖权威 ID；每组给出精确生产映射族。除上节点名的 ID 外，本轮未发现新的可闭环差异。`equivalent/Unity-adapter` 的最终逐行拆分仍由正式 ledger 完成。

| ID 集合（逐项） | 数量 | Unity 证据族 | 本草案结论 |
|---|---:|---|---|
| `FLOW.01`, `.02`, `.03`, `.04`, `.05`, `.06`, `.07`, `.08`, `.09`, `.10` | 10 | driver/tick system/passes | `.05/.09` difference；其余已映射 |
| `CARRIER.01`, `.02`, `.03`, `.04`, `.05`, `.06` | 6 | `FrameInputSet`, `SimInputBuffer`, provider | carrier adapter；`.04` 为 Unity 正式输入宿主而 authority buffer 是预留 |
| `IN.HUMAN.01`, `IN.CD.01`, `.02`, `IN.EDGE.01`, `.02R`, `.02L`, `.02U`, `.02D`, `.02A`, `.02DEF`, `.02J`, `IN.HIST.01`, `.02`, `.03` | 14 | input module/runtime/late N30 | 仅 `IN.CD.02` 属 R2；edges/history 已映射 |
| `IN.APPLY.00`, `.01A`, `.01D`, `.01J`, `.02`, `.03`, `.04`, `.05`, `.06`, `.07`, `IN.COMBO.01`-`.06`, `IN.JUMP.01`, `.02`, `.03` | 19 | input module + shared current-DAT input | `IN.JUMP.03` 双方等价清七个普通 Cd并保留 lock；其余已映射 |
| `MOVE.STAND.01`-`.03`, `MOVE.WALK.01`-`.02`, `MOVE.RUN.01`-`.04`, `MOVE.JUMP.01`, `MOVE.DASH.01`-`.02`, `MOVE.HEAVY.01`, `MOVE.LAND215.01`, `MOVE.RECOVER.01`, `MOVE.VTAIL.01`, `MOVE.HASDIR.01` | 17 | character action resolver/shared helpers | 已映射，CLR shell/current-DAT dispatch 是 adapter |
| `AI.PREP.00`-`.19` | 20 | `SimulationWorld.AiInput.partial.cs:41-306` | 已映射 |
| `AI.TARGET.01`, `AI.COORD.01`, `AI.ROLL.01`, `AI.STATE.01`, `AI.DIST.01`, `AI.BETWEEN.01`, `AI.COORD.02`, `AI.S3000.01`, `AI.OID331916.01`, `AI.OID521221.01`, `AI.OID512187.01`, `AI.FIRST.01`, `AI.GUARD.01`, `AI.OID1.01`, `AI.OID1CLOSE.01`, `AI.OID4.01`, `AI.OID5.01`, `AI.SUBOID.01`, `AI.SUB.01`, `AI.PREWRITE.01`, `AI.PRESSURE.01`, `AI.HELD.01`, `.02`, `.03`, `AI.TEAM.01`, `AI.MOVEMODE.01`, `AI.NOTARGET.01`, `AI.SOUND.01` | 28 | `SimulationWorld.AiInput.partial.cs:309-853` | 已映射；sound 表现排除 |
| `FT.TICK.00`-`.08`, `FT.NEXT.01`-`.05`, `FT.TAIL.01`, `FT.SOUND.01`, `FT.OP.00`-`.03`, `FT.SPAWN.00`-`.05` | 26 | common FrameTick + opoint factory/late pass | 已映射；sound adapter |
| `FA.ADV.00`, `FA.VEL.01`, `.02` | 3 | `SimTransit/SimTU`, shared frame velocity | Unity shell dispatch adapter |
| `FL.ROOT.00`, `FL.TARGET.01`, `FL.CASE10`, `FL.CASE1`, `FL.CASE5`, `FL.CASE8`, `FL.CASE2_4_12_14.A`, `.B`, `FL.NOTARGET.CATCH`, `FL.CASE11`, `FL.CASE6_9`, `FL.CASE13`, `FL.CASE3`, `FL.CASE7`, `FL.NOTARGET.DRIFT`, `FL.Z.01`, `.02` | 17 | `LF2Entity.RunHitFa*FrameLogic` | 已映射 |
| `PH.ROOT.01`, `PH.X.01`, `PH.Z.01`, `PH.TYPE3.01`, `PH.FRIC.01`, `.02`, `PH.BOOM.01`, `PH.Y.01`, `PH.GRAV.01`, `PH.AIR.01`, `PH.GROUND.00`, `.CHAR13`, `.SHURIKEN`, `.FLY`, `.BALL`, `.999`, `PH.LAND.GENERIC`, `PH.SYNC.01`, `PH.WCOUNT.01`, `PH.SOUND.01` | 20 | `CharacterMechanics` + shared/non-character frame advance | 已映射；sound adapter |
| `DATA.FRAME.01`, `.02`, `DATA.OP.01`, `DATA.CP.01`, `DATA.CHAR.01`, `DATA.CONST.01`, `.02` | 7 | Unity DAT converter/models/constants | DAT 读取差异按用户要求视为 adapter，不形成 backlog |
| `RT.RESET.01`, `RT.COPY.01`, `RT.CLONE.01`, `RT.COPYENTITY.01`, `RT.APPLY.01`, `RT.SYNC.01`, `RT.CHECK.01` | 7 | runtime reset/entity proxies/snapshot | `RT.CHECK.01` 属 R5；copy/reset 需随 R4 字段补齐 |
| `RT.IDENTITY.01`, `RT.TRANSFORM.01`, `RT.MOTION.01`, `RT.FRAME.01`, `RT.LINKS.01`, `RT.TRANSIENT.01`, `.02`, `.03`, `RT.STATS.01`, `RT.RESIDUAL.01`, `.02`, `.03`, `.04`, `RT.INPUT.01`, `.02`, `.03`, `.04`, `.05`, `.06`, `RT.PRESENT.01`, `RT.ENTITY.01` | 21 | `NTSDEntityRuntime` + entity/world split storage | `RT.LINKS.01` 缺 ReleaseTick；其余结构适配已定位 |
| `WRAP.AI.01`, `.02`, `WRAP.INPUT.01`, `WRAP.FRAME.01`, `.02`, `.03`, `.04`, `WRAP.PHYS.01`, `WRAP.DISPATCH.01`, `WRAP.CATEGORY.01` | 10 | Unity forwarding/dispatch | adapter |
| `UNRES.01`, `.02`, `.03`, `.04`, `.05` | 5 | 保留原名/原异常表达式 | `authority-unresolved` |
| `DEP.INT.01`, `.02`, `.03`, `.04`, `DEP.WORLD.01`, `DEP.RNG.01`, `DEP.DATA.01` | 7 | interaction/framework/RNG/DAT ledgers | `.INT.04` 同 R3/R4；其余跨分区已定位 |

## 4. runtime 字段映射完整性

权威文档声称 137 个字段/数组，但 8.1-8.3 的显式字段 token 按组相加为 138（10+9+8+13+16+6+13+23+32+8）。这是权威 ledger 的机械计数不一致，不是 Unity 行为差异；最终 137-field certificate 前必须先确定哪一项不计入 field 总数。

按显式字段集合，Unity 映射如下：

| 权威组 | Unity storage | 例外 |
|---|---|---|
| Identity | runtime + registry active + current-DAT category | `OwnerId -> OwnerStableId`; `Unk364 -> RelationTeam`，不得 fallback Team |
| Transform | `X/Y/Z`, ints, offsets, `Dir` | Facing 由 `Dir` adapter 承载 |
| Motion | runtime 同名/renamed fields | 完整定位 |
| Frame | runtime + `LF2FrameInfo` + `FrameTransistor` | PrevFrame 在 `Frame.Prev`; jump flags 为局部控制流 |
| Links | runtime slot/stable-id link fields | `ReleaseTick` 缺；`GrabbedTimer/StuckVictimSlot` 无独立 storage |
| Transient | runtime Mp carriers + scene-query candidate cache | scratch 结构 adapter，checksum应排除 |
| Stats | runtime/Health proxies | 完整定位 |
| Residual | runtime renamed fields + local abort control flow | snapshot 未读取多项已有 storage |
| Input | runtime + `NTSDInputStateModule` mirror | `CdDefendLock` 递减拥有者差异 |
| Presentation | runtime + `LF2Entity` hit-record arrays | 结构 adapter |

## 5. RNG 与 Unity-only 反向审计

### RNG

- AI：权威与 Unity 均为 70 次调用 occurrence。Unity 只有 65 个含调用的源码行，是因为复合行/短路表达式，不是缺 5 次 RNG；`SimulationWorld.AiInput.partial.cs:41-306,378-853` 的 helper 顺序与 authority ID 顺序一致。
- FrameLogic：Unity `LF2Entity.cs:1234-1238,1323,1336,1340-1341,1671,1673-1674` 共 11 次 occurrence，对应权威 case8 的4次、case6/9 的1+2次、case13 的3次，未发现顺序差异。
- 本轮没有用“含 RNG 的行数”判定差异。

### Unity-only 分支

| 分支 | 精确证据 | 分类 |
|---|---|---|
| results active 后继续普通 pass | `NTSDBattleTickSystem.cs:17-29` | `difference`（R1） |
| late holder frame-change resync | `SimulationWorld.Passes.partial.cs:764-776,828-853` | `difference`（R3） |
| `SimEntityCollision` | late line779；base/interface 空实现，全仓无生产 override | `unreachable` 状态效果 |
| 六个 `Suppress*UntilTick` | runtime lines106-111/reset414-419；pass readers716/765/1013/1027/1049等 | `unreachable`：全仓生产无 writer；`SuppressFrameTickUntilTick` 连 reader 也没有 |
| `SimTransit + SimTU` 双调用 | `SimulationWorld.Passes.partial.cs:366-386` | `adapter`：character壳前者推进，weapon/special/other壳后者推进；base另一端为空 |
| frame packet 入 `SimInputBuffer` 后同 tick poll | `SimulationWorld.FrameInput.partial.cs:19-47` -> `PostCooldownHumanInputAll:54-65` | `adapter`：Unity callback/固定tick边界 |
| duplicate player slot | public packet可重复；生产 provider 每slot每tick唯一 | 公开 contract difference，但 production-unreachable；不计正式战斗根因 |

## 6. 结论与执行顺序

本轮独立确认：

1. R1 results early return：正式战斗差异。
2. R2 CdDefendLock：仅 AI 被全实体 pass 递减这一条正式差异。
3. R3 late held resync：正式战斗差异。
4. R4 ReleaseTick：正式 runtime 字段/写入缺失。
5. R5 parity snapshot：观测工具错投影，另含 `ownerId/grabbedTimer/unk364` 错映射。

建议依赖顺序：先修 R1/R2/R3/R4 的 runtime 与 pass 行为，再修 R5 trace；随后补 focused self-check，运行 fresh Unity full self-check，并对 held/opoint/input/results 做定向 Play Mode/trace 验证。当前只有静态审计证据，不能宣称 frame/input 分区已运行时对齐，更不能据此宣称整个战斗逻辑完全对齐。
