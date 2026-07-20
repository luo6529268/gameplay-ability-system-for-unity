# Authority-Unresolved Input / Interaction Verification (2026-07-18, historical snapshot)

> **历史报告标记：** 本文保留最初核验时的 `UNRES.02-05 authority-unresolved` 标签，不能作为当前状态依据。BATTLE-AUDIT11 的最终代码定性已将 `UNRES.02/.03/.05` 归为 equivalent、`UNRES.04` 归为 confirmed code difference；当前状态以两份主对齐文档及 `final-verify-unres-02-05-code-parity-20260718.md` 为准。

本报告只核验代码层面的 `UNRES.01-05` 与 `DEP.INT.01-04`。权威为
`J:\QQFile\NTSD2.4\ntsd_release_C#`；未进行 Play Mode、资源或表现验证，也未修改生产代码。

## 结论汇总

| ID | 结论 | 代码层证据 |
|---|---|---|
| UNRES.01 | `equivalent`（字段名仍保持 authority 原名不可译，但 Unity 已有对应语义字段） | 权威 residual 默认/reset/copy；Unity runtime reset/copy 与 parity projection；`Unk31C` 对应 weapon durability，`Unk324/33C` 对应 transform original/target，`Unk318` 对应 render pic offset |
| UNRES.02 | `authority-unresolved`（行为等价，label 语义无权威定义） | 两端 helper 调用顺序、OID 分组、条件和 RNG 分支一致；`Label591/Label435` 仅命名标签 |
| UNRES.03 | `authority-unresolved`（表达式行为等价，意图未定义） | 两端均保留 `PP < negative mpDelta`，不改成 `PP < -mpDelta` |
| UNRES.04 | `authority-unresolved`（不可达分支保留） | 两端均先处理 `value > 500`，随后保留实际不可达 `value == 550` |
| UNRES.05 | `authority-unresolved`（疑似 authority line-cover typo，Unity 忠实保留） | 两端均使用 `target/team` 条件而不是 candidate 的关系检查；没有权威意图证据可纠正 |
| DEP.INT.01 | `equivalent` / Unity-adapter | authority `ResetHitCandidates` 清 scratch、HitConfirm2、abort 和 20-slot arrays；Unity 在 candidate collection 清 carriers、candidate counters/distances，并使用同样固定候选快照 |
| DEP.INT.02 | `equivalent` / Unity-adapter | authority `SetFrameImmediate` 只写 Frame + FrameWaitCounter=0；Unity `SetFrameTickImmediateRawDirect` 同样写 raw frame + FrameWaitCounter=0，不清 attacking |
| DEP.INT.03 | `equivalent` / Unity-adapter | authority late pass 按 slot 执行 pre-collision、FrameTick、collision、late cleanup、opoint、tail；Unity `LateEntityUpdateAll` 保持同一 serial 顺序 |
| DEP.INT.04 | `equivalent` / Unity-adapter | authority hit-record capacity 为 10，满容量拒绝；Unity `MaxHitRecordSlots=10`、`AddHitRecord` 同样拒绝并采用同样 record-owner/位置公式 |

## 逐项证据

### UNRES.01: unknown `Unk*` 字段

Authority runtime 定义 residual 字段及默认值：
`NtsdEntityRuntime.cs:326-350`；reset：`:371-396`；copy/apply：`:398-470`。
Unity runtime 对应字段/适配字段定义及 reset：
`Assets/NTSD/Scripts/Simulation/NTSDEntityRuntime.cs:103-161,327-479`。

具体映射已经闭合：

- `Unk31C` 在 authority 中由 weapon hp 初始化、扣减、归零（`FrameAdvance.cs:285-893`, `FrameTick.cs:405`, `Physics.cs:279-359`, `HitResolve.cs:495-497,844-846`）；Unity 对应 `WeaponFlightCounter` 的初始化、扣减和归零（`LF2Entity.cs:3950,5105-5169`; `LF2Weapon.cs:24-25`）。
- `Unk324/Unk33C` 在 authority 中作为 transformation original/replacement OID（`CPointRuntime.cs:348-349`; `GameTick.cs:1044-1067`）；Unity 对应 `TransformOriginalObjectId/TransformTargetObjectId`（`LF2Entity.cs:368-377,3372,4314-4315,4531-4532`; `SimulationWorld.Passes.partial.cs:624-656`）。
- `Unk318` 在 authority 仅作为 residual integer 写入和 hash/snapshot 字段（`GameTick.cs:919,1659`; `CharacterSync.cs:297`）；Unity 保留为 `RenderPicOffset`，并由 sprite frame projection 消费（`LF2Entity.cs:4099`; parity projection `BattleParitySnapshot.cs:530-538`）。
- `Unk328/32C/330/334/338/360/3FC/400` 直接保留同名 runtime 字段，AI、split/merge 和 late coordinate 分支均有对应消费者（`SimulationWorld.AiInput.partial.cs:47-63,472-516`; `SimulationWorld.Passes.partial.cs:140-285`）。

因此没有发现代码层缺字段或错误生命周期；差异仅是权威保留的历史匿名命名，归类为 equivalent，不把未知名称本身当成行为差异。

### UNRES.02: `Label591` / `Label435`

Authority 调用顺序和 helper：
`InputRuntime.cs:480-496,597-608`；`AiUpdateOid52_1_2_21PreLabel591Decision:1742-1794`；`AiUpdateLabel591Oid51_2_18_7Decision:1796-...`；`AiProcessSubLabel435PressurePrewrite:2242-2286`。

Unity 对应调用和 helper：
`SimulationWorld.AiInput.partial.cs:260-305,544-674,755-766`。

OID 分组、距离/状态门槛、RNG 调用和 `return` 短路均保持原顺序。`Label591`/`Label435` 没有权威 enum、字段或注释定义，故只能确认行为等价，不能确认标签意图。

### UNRES.03: negative `mpDelta`

Authority `FrameTick.cs:180-191` 明确执行 `if (entity.Pp < mpDelta)`，其中 `mpDelta < 0`；否则 `Pp += mpDelta` 并做 refund display。Unity `LF2Entity.cs:4943-4976` 完整保留该表达式和分支顺序（`Health.PP < mpDelta`、`Health.PP += mpDelta`、`SpendPpDisplay(-mpDelta)`）。

这是可观察代码行为已对齐，但表达式意图在 authority 中未定义；不得擅自改成绝对值比较，故保留 authority-unresolved。

### UNRES.04: `value == 550`

Authority `FrameAdvance.cs:1019-1045` 先以 `value > 500` 转换 `value - 550` 并 return，随后保留 `value == 550` 清零分支，导致该分支实际不可达。Unity `LF2Entity.cs:5642-5667` 逐句保留相同结构。该项无 Unity 行为差异，但 authority 分支意图不可由源码确定。

### UNRES.05: AI line-cover relation check

Authority `InputRuntime.cs:2288-2314` 的 line-cover 循环在 candidate 过滤中使用 `target.Unk364 != self.Unk364`（而不是 candidate 的关系字段）。Unity `SimulationWorld.AiInput.partial.cs:778-793` 对应使用 `Team(target) != Team(self)`，即同一 target/self 关系条件；其余 candidate 状态、HP、Z 和区间过滤一致。该条件可能是 authority 历史 typo，但没有权威意图证据，Unity 不能擅自改为 `cand` 检查。

### DEP.INT.01: candidate reset

Authority `CollisionCollect.CollectCandidates:14-38` 对每个 active entity 调 `ResetHitCandidates`；`Entity.cs:151-162` 清 `Mp/Mp2/Mp3/Mp4` scratch、`HitConfirm2`、abort 和 20-slot candidate arrays。

Unity `BruteForceSceneQuery.CollectCollisionCandidates:236-254` 在同一收集入口清 `ClearHitCandidateCarriers`、`HitCandidateCount` 及三个距离 scratch；`LF2Entity.ClearHitCandidateCarriers:4446-4449` 清 HitConfirm2；候选数据由 `_candidateCache` 固定后消费（`:256-294`）。Unity 使用命名的 candidate counters 替代 authority `Mp` scratch，是 adapter，不改变候选固定/容量行为。

### DEP.INT.02: immediate frame write

Authority `FrameRuntime.SetFrameImmediate:10-14` 只写 `entity.Frame` 和 `entity.FrameWaitCounter=0`。
Unity `LF2Entity.SetFrameTickImmediateRawDirect:5593-5598` 调 raw frame write 后仅清 `Runtime.FrameWaitCounter`；`DirectWriteFramePreserveWaitCounter:3900-3903` 保持 wait counter/attacking 语义。没有发现 Unity `ImmediateFrame` 副作用泄漏到该路径。

### DEP.INT.03: late FrameTick lifecycle

Authority `GameTick.RunLatePerEntityUpdatePass:1533-1555` 按 runtime slot 串行执行 `RunLateEntityUpdate`，其中 `RunLateEntityUpdate:1525-1550` 依次执行 state pre-collision、recovery、`FrameTickRuntime.Tick`、collision/death handling 和 post cleanup。

Unity `SimulationWorld.Passes.partial.cs:740-815` 按同一 runtime slot 顺序执行 `RunStateSpecialPreCollision`、recovery、`SimFrameTick`、`SimEntityCollision`、late death/opoint cleanup、tail、`MirrorLatePrevFrame`，并在每个可能销毁点重新检查 active。`NTSDBattleTickSystem.cs:74-84,198-201` 将该 pass 放在 interaction、render dispatch 和 frame postprocess 之后，顺序闭合。

### DEP.INT.04: hit-record capacity/replacement

Authority `NtsdEntityPresentationRuntime.cs:5-24` 使用 10 槽数组；`CharacterPresentation.TryAddHitRecord:37-48` 在 `count >= length` 时拒绝。`HitResolve.RecordKind0Hit:1148-1193` 选择 record owner、计算 damage/anchor 和 RNG 偏移。

Unity `LF2Entity.cs:459-480` 定义 `MaxHitRecordSlots=10`、同样的满容量拒绝；`RecordKind0Hit:483-535` 使用同样的 Z/slot tie-break、damage 公式、center/anchor 计算及 `BattleRandInt(0,9)-4` 偏移。未发现容量或 replacement helper 的代码差异。

## 最终边界

本报告确认了 4 个 `DEP.INT` 项在代码层面无正式差异，5 个 `UNRES` 项中 2 个仅因 authority 异常表达式、2 个因匿名 label/字段语义、1 个因疑似 line-cover typo 保持 unresolved。它们都不是 Play Mode 结论；Naruto 连招、武器跟手、命中表现等场景仍由用户自行验证。
