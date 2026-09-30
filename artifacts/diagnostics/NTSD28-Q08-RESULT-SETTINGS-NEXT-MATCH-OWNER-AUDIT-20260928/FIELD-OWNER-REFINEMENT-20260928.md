# Q08 结果设置字段归属收窄

> **2026-09-29 范围更正：** 用户确认结果页设置/重赛交互不属于本次战斗逻辑对齐；下文待办只作历史静态审计，不能据此启动生产改动或阻塞 Q08。见[当前复核](CURRENT-RECHECK-20260929.md)。

状态：`READ_ONLY_FIELD_OWNER_MAP / PRODUCTION_REPAIR_PENDING`。本项只用当前生产脚本和既有自然KO完整tick结果收窄 `REPORT.md` 的下一场配置建议；没有修改代码、DAT、Asset、Scene或非战斗流程。正式发行没有所检 Unity 结果设置动作的同条件入口，因此下表中“当前 Unity 消费者”不能自行晋升为正式规则。

| 设置路径 | 当前写入 | 已见当前消费者和风险 | 下一包必须判定 |
|---|---|---|---|
| 所有 Settings Attack | `CommitResultTableValues()` 将两个2×11结果行组合为 `ResultCommittedTotal/Hp`，`BattleResultsWriter.SyncReserveOwner()` **只复制**到当前 `BattleRuntimeState.ReserveCommitted*` 并设 `ReserveOwnerValid`，不会生成后备角色或重算缺额。 | `BattleResultsReserveHostWriter.TrySpawnBeforeResults()` 在结果页激活前的当前 World mode4 分支读取矩阵，并经其 `SynchronizeOwner()`统计缺额；已激活 Settings 页不再进入这个生成读者。设置提交后的矩阵仍进入snapshot/checksum，但所检生产战斗行为消费者未见。正式同条件mode4设置动作未见证。 | 区分结果页激活前正式mode4 reserve所需的当前场矩阵，以及结果页设置提交后应由谁在下一场消费；不能将前者整体迁走，也不能把后者仅复制到旧World当作设置功能完成。 |
| cursor0 Attack | `ApplyFallDamage()` 写所有活动角色的 `FallDamageDiv`，随后设 `PendingHostAction=Rematch`。 | 自然KO完整tick在native timer14/transition0实测 `FallDamageDiv 0→100`；伤害计算与checksum读取此字段。`PendingHostAction`没有找到生产消费者。 | 若设置意在下一局生效，须记录下一局参数且旧角色战斗字段保持不变；同时声明取消/重赛失败如何处理。不可只删写入而丢失设置功能。 |
| cursor1/5 Attack | 设置 `PendingHostAction=BootstrapDirect`。 | 当前值仅在World结果状态、snapshot/checksum和测试中读到；直接战斗重赛由`BattleTestBootstrap.TryHandleFirstBattleOnlyResult()`按native transition2重建World，没有读取它。 | 判断该命令在保留的Unity结果UI中代表哪种下一步，并给它真正的host消费者或明确废弃其旧占位字段。 |
| cursor2 Attack | 将结果页 `Phase` 切到201。 | 当前只是结果UI的页内状态；用户保留该页面。 | 保留页内行为，并验证不提前写战斗真值。 |
| cursor3 Attack | 修改旧World `Match.StageIdx/RandomStage`；计数读旧`RuntimeStageCount`。 | `SimulationTickDriver.ApplyMatchConfig()`创建新场时从 `MatchConfig.backgroundId`重设这些字段；原正式背景表被用户排除，结果设置的正式同条件stage入口未见证。 | 明确项目地图/下一场选择的有效值来源；不能把正式24个背景ID硬写为项目stage计数。 |
| cursor4 Attack | 修改旧World `Match.Difficulty`。 | 新场由`MatchConfig.difficulty`覆盖；当前结果页未写该配置。 | 若保留设置功能，需要事务化传给下一场，并在取消/返回选人时明确归属。 |

时间边界的生产调用链已收窄：`NTSDBattleTickSystem.BattleResultsFlow()` 在 tick 开始时结果页已激活的分支只调用 `RunActiveBattleResultsTick()` 并立即返回，不调用 `UpdateBattleResultsFlow()`；`BattleResultsOutcomeHostWriter.UpdateSummaryActivation()` 自身也在 `results.IsActive` 时立即返回。`IsActive` 为 `Phase >= 200`，当前 `RunSettingsPhase()` 的 Attack 分支只保持 202 或转至 201，`ResetLiveGuard()` 不改变 Phase。另须区分两个同名近似方法：Settings 的 `SyncReserveOwner()` 是矩阵复制；生成读者内部的 `SynchronizeOwner()` 才清点活动实体/缺额。因此，**Settings Attack 提交的 `ReserveCommitted*` 在该已激活结果页的当 tick 及其持续活跃期间，不会被当前World的 mode4 reserve 生成读者消费**；所检 `GetBattleResultsReserveMissingCount()` 仅为内部包装，生产脚本未见额外调用。这个结论是 Unity 生产代码静态控制流证明，不是 mode4 同条件完整 tick 或正式 EXE 的对照证据；结果页退出、World 重建后这些矩阵的保留/消费仍需单独追踪。读者确实存在于结果页激活之前，不能把这条时序结论推广为“所有 reserve 矩阵都应迁出当前World”。

当前 `MatchConfig` 只有 `backgroundId`、`difficulty` 等赛前字段，没有团队 `FallDamageDiv` 或结果reserve配置；菜单战斗由 `AppManager.CurrentMatchConfig` 调 `ApplyMatchConfig`，直接战斗首次重赛则 `RecreateWorld()` 后自行重建阵容并恢复RNG/输入相位，二者没有共用的“结果设置下一场配置”消费者。一次性事务需要覆盖这两条入口，但不能为此改菜单UI或非战斗逻辑。

下一场边界进一步确认：`NTSDBattleTickSystem.RunReleaseTick()` 在结果页已激活、native transition仍为0的 tick 继续执行输入、帧推进和交互；Settings 写入旧角色并非被“结果页已显示”自动隔离。直接战斗第一次普通结果转场只检查 `NativeTransitionState == 2`，有序关闭旧 World 后调用 `SimulationTickDriver.RecreateWorld()` 新建 `SimulationWorld`，随后 `SetupTestCharacters()` 重建角色；这条路径未读取 `PendingHostAction`，也未把旧 `FallDamageDiv`/reserve/StageIdx/Difficulty 投到新 World。Menu→Battle 的 `ApplyMatchConfig()` 则先 `ResetRuntimeState()` 再从 `CurrentMatchConfig` 写入赛前字段，而设置页没有写回该配置。故目前的设置提交同时具有“旧场当下可见”和“所检两类下一场入口没有消费”的问题；这是当前 Unity 生产控制流结论，不代表正式发行有等价结果页操作。下个生产包必须把保留的结果UI提交与战斗旧World隔离，并给出下一局建局边界的实际消费者；不能只阻断旧写入。

精确源：`BattleResultsWriter.RunSettingsPhase/SyncReserveOwner/ApplyFallDamage/AdvanceStageSelection`；`BattleResultsRuntimeState.CommitResultTableValues/FallDamageDivForTeam`；`BattleResultsReserveHostWriter.TrySpawnBeforeResults/SynchronizeOwner`；`SimulationTickDriver.ApplyMatchConfig`；`BattleTestBootstrap.TryHandleFirstBattleOnlyResult`；`AppManager.SetMatchConfig/InitializeBattleAsync`；`MatchConfig`。旧 `REPORT.md` 的“设置不能提前修改旧战斗”应限于**已证改变正式转场前战斗规则的字段**，不能直接推广为“所有旧World结果数据都要迁出”。

下一生产 Task/Change 仍须先冻结结果设置的生效时点、两种建局入口和mode4 reserve归属，再写RED/GREEN。只需复用既有timer14正反例加一个下一局实际消费例；普通KO、350、70tick和已过双轮案例不例行重跑。Q07/D-024、Q08/BATCH-04及总目标保持开放。
