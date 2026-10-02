# Q08/C009 返组赢家字段的战斗消费者边界

权威版本：根正式 `NTSD2.8-Logan.exe` SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 及对应 playable live source。本报告只读复核现有 12 tick 正式源/根与原 Battle Scene 限定证书，未重跑、未修改战斗代码或 DAT。

正式 `source/ntsd28_core/src/simulation/battle_flow.cpp` 中，`BattleFlow28::classify` 对当前存活阵营生成有序列表；普通 mode0 在至少两组存活时 `winner_group=-1`，`BattleFlow28::step` 刷新 `outcome_`、保留计时并返回。下一次仅剩一组时刷新赢家并从保留计时递增。`source/ntsd28_playable/src/game_session.cpp` 将 `battle_flow_step_.outcome.winner_group` 写入 render snapshot 的 `native_scoreboard_winner_group`。现有 C009 正式源/根 OID304→OID56 返组原件已在 12 tick×7 声明字段 84/84 同态；原 Battle Scene 的计时、输出计时、组 mask、出生等 12 tick×6 字段 72/72 同态。它们没有覆盖“再次单组”、完整 World 或实际画面。

Unity 生产 `BattleResultsOutcomeHostWriter.AdvanceNativeFlowBeforeCombat` 扫描角色实体，写 `NativeLivingGroupMask`、`NativeResultTimer`、`NativeResultOutputTimer`、`NativeResultPhase` 与 `NativeTransitionState`。多组分支保留计时，已由 C009 聚焦 RED→GREEN、正式源/根及原 Scene 证据限定验收。`NTSDBattleTickSystem` 在 combat pass 前调用此 writer 并在非零转场状态时停止 combat；`SimulationTickDriver` 的 tick 准入和战斗出口同样读取 `NativeTransitionState`。对 `Assets/NTSD/Scripts` 当前引用检索未发现战斗 tick/输入/命中/生成消费者读取 `BattleResultsRuntimeState.Winner` 或 `PendingWinner` 作为原生分组判定。

Unity 的 `Winner`/`PendingWinner` 由另一条 `BattleResultsOutcomeHostWriter.UpdateSummaryActivation` 自有结果摘要路径写入；`BattleResultsWriter.RunActiveTick` 以 `IsActive` 驱动结果页交互，当前检索未见它直接读取这两个赢家字段。它们进入项目快照/校验和，但不是 `AdvanceNativeFlowBeforeCombat` 的正式 `winner_group` 载体。不能用这两个字段冒称已证明正式返组赢家与 Unity 结果画面一致，也不能为了本战斗计时证书将正式赢家强塞进自有结果页。用户已排除结果页设置与重赛 UI；若后续发现赢家值被非例外战斗规则直接消费，再以实际 consumer 和首差另立 Task/Change。

裁决：C009 的**战斗计时/组别返回暂停**保留现有限定通过；“再次单组恢复”仍需正式可达、同初态的根与原 Scene 后继证据。正式 scoreboard 赢家像素和 Unity 自有结果页设置/重赛不作为当前 C009 战斗规则修复前置。Q08 与总目标保持开放。下一按总表寻找非例外、可达的新首差；本报告不产生运行时新 PASS。

工具边界：原项目 Unity 2022.3.62f3 Editor 仍在运行；本机 `unity status --format json` 返回 `STATUS_NO_INSTANCES`（当前 Editor 未连接 Pipeline）。未启动第二 Editor/项目，也未通过该 CLI 操作 Scene。
