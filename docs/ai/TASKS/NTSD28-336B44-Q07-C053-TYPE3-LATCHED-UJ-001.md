# NTSD28-336B44-Q07-C053-TYPE3-LATCHED-UJ-001

状态：`FOCUSED_TEST_PASS / RUNTIME_PENDING`。父目标 NTSD28-UNITY-BATTLE-REALIGNMENT-001，BATCH-04/Q07/C053。

当前336B44 playable `BattleWorld28::resolve_confirmed_unarmored_standard_hit` 在 type3 目标响应中按分支读取不同动作源：`hit_Fj` 从目标当前 action 帧，`hit_Uj` 从 action-latch 帧；锁存帧缺字段时再回退20。正式内容 OID808 `a/kat/kat.dat` 帧153含 `hit_Uj:156`、帧156缺该字段。Unity `BattleDamageWriter.ApplyNativeType3TargetGenericContinuation` 和 `BattleEcsHitExecutionPlan` 的投影预检/写入三处现都从 `target.Frame.D` 读 `hit_Uj`，在 current156/latch153 条件下静态结果变成20，而正式为156。现有 `Trans.WaitCounter` 是 Unity 动作锁存帧载体，`BattleDamageWriter.ApplyNativeType3PairReset` 已读取它。

仅修改以下战斗代码：`Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs` 增加一个共用 type3 响应动作解析并在普通写入使用；`Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs` 的预检和投影调用同一解析；`Assets/NTSD/Scripts/Test/Editor/NTSD28B5Type3TargetGenericContinuationEditorTests.cs` 增加 current/latch 正反例。先写测试，再做最小生产修复；不改变 type3 kind-table transform、匹配 pair reset、DAT、Scene、GAS、非战斗模块。

验收：原Editor测试先对旧实现呈现 `hit_Uj` 的20/156首差，再验证修后聚焦通过、生成工程/原Editor编译0错；需要时原Battle Scene完整Driver同正式OID808可达条件与正式根同态。Editor尚未导入时只能报告 `CODE_WRITTEN / UNITY_RUNTIME_PENDING`，不关闭C053/Q07。回滚仅逐行审阅本包三脚本改动，保留工作树其它内容。

2026-10-01 代码/测试已写，生成工程编译0错。原Editor未导入，预期RED、修后聚焦、真实Play和正式根同态均待运行；不得把静态首差和编译当作C053验收。

2026-10-01 追加：原 Editor PID11944 导入/编译后，用完整测试名运行 Uj 锁存帧与 Fj 当前帧两项 EditMode，job `4c72782ca2874157937b226ec28d9522` 实际 2/2 Passed、0 Failed。此前过滤器缺 `.Editor` 的作业虽返回 succeeded，但执行数0，作废且不计通过。原 Battle Scene 仍为活动Scene、Editor idle非Play；正式OID808完整tick链与根/Unity Play同态仍待。状态 `FOCUSED_TEST_PASS / RUNTIME_PENDING`。[进度](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C053-TYPE3-LATCHED-UJ-001/PROGRESS.md)。

2026-10-01 只读候选已收敛：正式 Jiraiya/Hiruzen frame554 可生成 OID808/action150，其153/154接地分支可能进入155，后继156可使锁存与当前动作不同。但这尚未是实际 GameSession 事件、命中或 Uj 消费证明。先找正式完整tick阳性再做根/原Scene；详[进度](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C053-TYPE3-LATCHED-UJ-001/PROGRESS.md)。
