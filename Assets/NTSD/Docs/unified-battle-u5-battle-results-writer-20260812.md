# U5 Battle Results Writer 收口（2026-08-12）

## 1. 结论

战斗结算状态机已从 Unity 表现模块迁到每个 `SimulationWorld` 持有的 `BattleResultsWriter`。表现层只读取 `BattleResultsRuntimeState`，不再推进 phase、timer、cursor、队伍表、难度、stage 选择、rematch 或 route intent。

活动结算态严格保持 C# `GameTick` 的早退边界：先消费 post-cooldown 输入，再运行结算状态机并返回，不进入普通角色、碰撞、命中、opoint 与 late pass。非结算态仍在完整战斗 pass 结束后运行 `UpdateBattleResultsFlow`。

## 2. 权威依据

- `J:\QQFile\NTSD2.4\ntsd_release_C#\src\BattleCore\Simulation\GameTick.cs`：`RunResultsTick`、`UpdateBattleResultsFlow` 与 active-results 早退顺序；
- `J:\QQFile\NTSD2.4\ntsd_release_C#\src\BattleCore\Runtime\ResultsState.cs`：phase、cursor、table、commit、fall-damage 与 host action 数据契约。

Unity 侧把权威字段放入 `BattleRuntimeState.Results/Match/Flow`。`BattleLockstepChecksumModule` schema 由 2 升为 3，并纳入 stage/random-stage/runtime-stage-count、results 全部数组和矩阵、reserve commit、exit/route/mode2 intent，防止未来回放或联机只校验实体却漏掉结算真值。

## 3. 聚焦验证

`BattleRuntimeSelfCheck.CheckBattleResultsActiveStateMachineContracts` 覆盖：

- summary -> settings 的 attack 按下沿、SFX 与 timer；
- 长按 attack 不重复触发；
- phase 201 -> 202；
- summary table -> phase 210 与 jump 取消；
- difficulty 从 0 向左环绕到 2；
- stage 列表末尾进入 random sentinel `0x64`；
- rematch commit、fall damage、reserve owner/矩阵与 host intent。

`BattleLockstepChecksumEditorTests` 另验证 results/stage/mode2/reserve 任一变动都会改变 runtime checksum。

## 4. 最终证据与限制

- Unity fresh compile：0 C# error；
- U5 联合 EditMode job `b55c2edd04964be7b784f7bec65ab0f5`：220/220 PASS；
- 完整 `BattleRuntimeSelfCheck` 于 `2026-08-12 20:34:10` fresh PASS；
- Authority400 full/full：6/6 `equal-diagnostic`、`firstDifference=null`；
- 1000 AI 报告 `Temp/NTSD_ProductionEntityStress.u5-battle-results-writer-1000ai-60-20260812.json`：30 warmup + 60 sample，average/P95/max 为 20.6265/25.7384/28.0438 ms/tick，正式 tick 0 B、Gen0/1/2 collection 为 0、cleanup restored；最大 backlog 7、丢弃 backlog 29。

压测结束后仍处于 Play Mode 时，完整 self-check 的表现探针曾读取到 `actualCount=0`。原因是该夹具的 `RenderDispatchAll` 在 Play Mode 不会同步调用 `PresentLatestFrame`，不是 stress cleanup 状态泄漏；退出 Play Mode 后 fresh self-check 通过。后续应固定采用“停止 Play Mode -> refresh/compile -> self-check”的顺序。

本记录关闭单机结算逻辑的 U5 所有权迁移。服务器 room/ACK/jitter/reconnect 不在 U0～U9 范围；U9 仍需解决外层 backlog/drop 并完成更长稳态验证。
