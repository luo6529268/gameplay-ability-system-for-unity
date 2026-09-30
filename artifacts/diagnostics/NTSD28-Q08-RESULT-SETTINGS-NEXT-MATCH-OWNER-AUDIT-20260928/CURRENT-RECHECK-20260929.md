# Q08 结果设置当前代码复核（2026-09-29）

状态更正（2026-09-29）：`READ_ONLY_RECHECK / G-07_USER_EXCLUDED / NOT_Q08_EXIT_GATE`。用户明确本次只对齐战斗场景的战斗逻辑；Unity 自有结果页设置、改难度/地图及重赛交互不属于本次需求。下述观察只保留为静态审计历史，**不启动设置功能的 Task/Change，不作为 Q08/BATCH-04 关闭门槛，也不要求用户决定生效时点**。本次未改生产脚本、DAT、场景或配置 Asset，未运行新的 Unity Play。正式 NTSD 2.8-Logan 没有本项目结果页 Settings 的同条件操作，故这里不是正式 EXE 的同输入 first difference。

| 入口 | 当前事实 | 证据边界 |
|---|---|---|
| Settings Attack | `BattleResultsWriter.RunSettingsPhase()` 先提交结果矩阵并复制到当前 World 的 `ReserveCommitted*`；cursor 0 把当前活动角色的 `FallDamageDiv` 改掉，cursor 3/4 改当前 World 的地图选择/难度，cursor 1/5 只设置 `PendingHostAction`。 | 源码复核；旧 `NTSD28-Q08-RESULT-SETTINGS-FULL-TICK-WITNESS-001` 在自然 KO 后 timer14/transition0 实测 cursor0 的 `FallDamageDiv 0→100`。本次未重跑该 Play。 |
| 结果页持续活跃 | 当前 `RunSettingsPhase()` 的 Attack 不退出活跃结果页；`SyncReserveOwner()` 只复制矩阵，不重新生成当前 World 的后备角色。 | 当前源码与 `FIELD-OWNER-REFINEMENT-20260928.md` 的控制流证明；不能推广为所有 mode4 reserve 矩阵都无当前场用途。 |
| 直接重赛 | `BattleTestBootstrap.TryHandleFirstBattleOnlyResult()` 以 `NativeTransitionState == 2` 为门，关闭并 `RecreateWorld()`，重建角色和地图，未读取上述结果设置或 `PendingHostAction`。 | 当前直接战斗入口的源码静态结论；设置后真实重赛消费尚未测试。 |
| 菜单再进战斗 | `AppManager.InitializeBattleAsync()` 传入 `CurrentMatchConfig` 给 `SimulationTickDriver.ApplyMatchConfig()`；结果页设置未写回该配置。 | 当前菜单进入战斗入口的源码静态结论；不能由此推断用户期望的设置生效时点。 |

`PendingHostAction` 在现有生产脚本搜索中只见结果 writer 写入及 runtime/state、快照、checksum；未见下一场 host 消费。它不能替代真实的重赛配置传递。旧场的 `FallDamageDiv` 是参与规则/校验的字段，因此不能仅因“结果页已显示”就认为改旧场无影响。

四个复核文件 SHA-256：`BattleResultsWriter.cs` `CE2B8B5164811839594E47A02887A28CC401360AB01B022E408C300138540E1B`；`AppManager.cs` `D80E66EE914B58F006CDFB223DB7E88FF6E37CD351976EE06BB9D3B1C2C91C60`；`BattleTestBootstrap.cs` `4B4A69E2CE2FD51A79615999A4DB0EA8610FFC88984DFC434B362F2C0F78AE33`；`MatchConfig.cs` `E7F37D9C6DA1B8B85117E238586463BA44B8B112DA72017FDBDC83D824E4DE87`。

原先拟继续追设置的建议已由本文件首段的用户范围更正取代，不再执行。Q08 仍只针对战斗胜负判定、结果计时、战斗事件以及会影响模拟的 mode/stage 规则寻找非例外首差；已通过的普通 KO、寿命与双轮代表门不例行重跑。Q07、Q08、BATCH-04 与总目标均开放，但开放原因不包括本结果页设置项。
