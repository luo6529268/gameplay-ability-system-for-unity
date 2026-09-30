# Q08 结果设置的下一场配置归属审计（2026-09-28）

> **2026-09-29 范围更正：** 用户明确本次只对齐战斗场景的战斗逻辑；结果页的设置、改难度/地图与重赛交互属于 G-07 保留的 Unity UI，不作为 Q08 出口，下面的“下一生产包”建议作废。代码观察保留为历史。见[当前复核](CURRENT-RECHECK-20260929.md)。

状态：`READ_ONLY_OWNER_MAP / PRODUCTION_REPAIR_PENDING`。本报告只核对当前代码和已取得的原 Editor 完整 tick 见证；没有修改生产逻辑、DAT、Scene 或项目模式 Asset。

2026-09-28 字段归属更正：下方“设置不能提前修改旧战斗”只对已证会改变转场前战斗规则的字段成立，不能推断所有结果数据都属于下一场配置。当前有两个不同的reserve路径：结果页激活前的mode4生成读者 `TrySpawnBeforeResults()` 会通过 `SynchronizeOwner()`统计当前场后备缺额；已激活Settings页的 `SyncReserveOwner()` **只复制**提交矩阵，控制流不会在同一活跃期间再调用生成读者。设置提交后的矩阵目前进入snapshot/checksum，但所检生产战斗行为没有再次消费；页退出/World重建后矩阵归属仍待验证。cursor0 `FallDamageDiv` 的自然tick风险仍成立。六种 cursor 的写入/消费者/时间边界见 [`FIELD-OWNER-REFINEMENT-20260928.md`](FIELD-OWNER-REFINEMENT-20260928.md)。

## 已观察的写入和时点

- `NTSD28-Q08-RESULT-SETTINGS-FULL-TICK-WITNESS-001` 的自然 KO 正反例在 native result timer 14、transition 0 时，旧结果页 Settings cursor 0 的 Attack 将旧角色 `FallDamageDiv` 从 0 写为 100；无 Attack 保持 0。该字段参与伤害计算和 checksum。正式发行 frontend 没有同一设置动作，因此这是已证的 Unity 内部战斗状态耦合，不是正式 EXE 同输入首差。
- `BattleResultsWriter.RunSettingsPhase` 在 Attack 时先提交结果表、写 `BattleRuntimeState.ReserveCommitted*`，再依 cursor：0 写旧角色 `FallDamageDiv`、设置 Rematch 标志；1/5 设置 BootstrapDirect 标志；3 改当前 `Match.StageIdx`；4 改当前 `Match.Difficulty`。这几项都写入当前 World。`PendingHostAction` 在生产脚本搜索中只有写入，没有消费；快照、checksum 和测试会读它。
- 正式 native result transition 仍由 `BattleResultsOutcomeHostWriter` 与 `SimulationTickDriver.TryDispatchOrdinaryResultTransition` 处理。直接战斗第一次转场的 `BattleTestBootstrap.TryHandleFirstBattleOnlyResult` 关闭旧 World、重建 World/阵容，并恢复 RNG 和输入相位；它没有读取上述设置值。普通 Menu→Battle 则由 `AppManager.CurrentMatchConfig` 在 `InitializeBattleAsync` 中交给 `SimulationTickDriver.ApplyMatchConfig`，后者重置 runtime，再投影 stage/difficulty/阵容等字段。当前设置页没有把所选值提交给这一路入口。
- 已激活结果页且 native transition 为0时，`NTSDBattleTickSystem.RunReleaseTick()` 继续执行本 tick 的输入、帧推进和交互，故设置页对旧角色 `FallDamageDiv` 的写入具有当前战斗可见性。普通结果转场需要 `NativeTransitionState == 2`；第一次直接重赛随后 `RecreateWorld()` 构造新 `SimulationWorld`，新阵容由 `SetupTestCharacters()` 创建。旧结果矩阵、角色字段和 `PendingHostAction` 没有被该入口读取；Menu→Battle 入口同样从原 `CurrentMatchConfig` 初始化。这是当前 Unity 控制流的旧场/下一场双边界证据，不代表正式 EXE 存在等价结果设置动作。详[字段归属和时间边界](FIELD-OWNER-REFINEMENT-20260928.md)。

## 有界结论与下一执行包

结果页显示可保留，但其设置不能提前修改仍在正式转场前运行的旧战斗。下一生产包应先明确一个**下一场配置事务 owner**：在结果页确认时记录待生效选择，在真正建立下一 World/下一局的边界应用一次，并在取消、返回选人、关闭及重赛失败时明确丢弃或保留语义。必须分别覆盖直接战斗重赛和 Menu→Battle；不能仅删除 `ApplyFallDamage`、仅更改一个字段，或把未消费的 `PendingHostAction` 当成已实现的 host 命令。

修复前先立独立 Task/Change，列出各 cursor 对应的持久配置字段、`MatchConfig`/World 重建及 mode-4 reserve 的所有读写者。聚焦 RED/GREEN 应复用已通过的 timer14 正反完整 tick：旧 World 的战斗字段与 checksum 不受结果设置影响；再以一例真实重赛验证下一局消费配置。现有自然 KO、timer350、双轮重赛、70-tick KO 寿命证据不例行重跑。此审计不关闭 Q08、Q07 或 BATCH-04，也不决定 Q07/D-024 碰撞域。

依据：`Assets/NTSD/Scripts/Simulation/Ecs/Results/BattleResultsWriter.cs`、`BattleResultsReserveHostWriter.cs`、`Assets/NTSD/Scripts/Simulation/Runtime/BattleRuntimeState.cs`、`Assets/NTSD/Scripts/Simulation/Host/SimulationTickDriver.cs`、`Assets/NTSD/Scripts/Test/BattleTestBootstrap.cs`、`Assets/NTSD/Scripts/App/AppManager.cs`，以及 `NTSD28-Q08-RESULT-SETTINGS-FULL-TICK-WITNESS-001` 验收记录。
