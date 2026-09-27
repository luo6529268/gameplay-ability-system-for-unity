# Q08 Unity 结果设置对旧战斗状态的写入复核

状态：`STATIC_COUPLING_CONFIRMED / NATURAL_FULL_TICK_PENDING`。本次只读核对当前源码；没有修改生产脚本、DAT、Scene 或模式 Asset，也没有取得新的 Unity Play / 正式 EXE 同条件结果。

## 为什么属于 BATCH-04 / Q08

总表 §0.14 的 BATCH-04 出口包含结果 timer、world 事件及模式流程；P-19/G-08 允许 Unity 保留自己的结果页与选择画面，但不允许其显示状态改变正式战斗规则。旧耦合审计 `NTSD28-Q08-RESULT-TRANSITION-HOST-AUDIT-001/P19-EXCEPTION-LOGIC-COUPLING.md` 曾列出结果设置写入。此处复核的是 2026-09-27 当前代码，不重开已经修复的结果页输入抑制和 timer350 战斗冻结。

## 当前源码路径

1. `BattleResultsOutcomeHostWriter.UpdateSummaryActivation` 在旧 `BattleEndPhase >= 11` 时激活 Unity 自有结果页；`BattleResultsRuntimeState.IsActive` 是 `Phase >= 200`。这个显示时点是用户保留的结果页例外，不能单凭它要求改到正式 timer101。
2. `NTSDBattleTickSystem.RunTick` 先调用 `AdvanceNativeBattleResultsBeforeCombat`；只有 `NativeTransitionState != 0` 才在战斗 pass 前返回。未转场时，完整战斗 pass 后调用 `BattleResultsFlow`；若旧结果页在 tick 开始已激活，则调用 `BattleResultsWriter.RunActiveTick(frameInput)`。
3. 旧页 `Phase == 202`、`SettingsCursor == 0` 且 P1/P2 有 Attack pressed edge 时，`RunSettingsPhase` 调用 `ApplyFallDamage`。它遍历当前 World 的 active 角色，将 `results.ResultMultiplier` 派生值写入 `LF2Entity.FallDamageDiv`；这一字段随后在 `LF2CharacterDamageStateResolver` 的环境/伤害路径被读取，并进入 checksum / parity snapshot。设置页其他 cursor 还会改 `Match.Difficulty`、`StageIdx`、reserve committed 值和 `PendingHostAction`。
4. `BattleRuntimeSelfCheck.CheckBattleResultsActiveStateMachineContracts` 的直接 writer 夹具明确预期 `FallDamageDiv` 变为 150/200。该旧断言只证明 Unity 自有结果设置路径的意图，不能证明这项旧 World 写入符合当前正式版的战斗时序。全项目当前对 `PendingHostAction` 的生产调用搜索只有写入、快照、checksum，未找到消费 owner。

正式 `BattleFlow28::step` 仍在结果可见阶段将 timer 推至 350，且在 `>=144` 时接受 held Attack/Jump 提前完成；正式 playable 的 `GameSession28::step` 在转场前持续走战斗 host。所检正式发行入口没有等价的 Unity 结果设置菜单动作，因此这里是**已证 Unity 内部可达写入 + 尚待同条件裁决的规则耦合候选**，不是正式 EXE/Unity 已测出的 first difference，也不能从源码负搜索推断必须删除 Unity 设置功能。

## 验证边界与下一出口

- 本次 `unity status --format json` 返回 `STATUS_NO_INSTANCES`，因为此 Unity 2022.3 项目没有该 CLI 所需的 Pipeline 实例；进程列表仍显示原项目 Editor PID 11944。随后通过原项目 UnityMCP 端口 6402 发送最小 `execute_code`，`auto` 响应为 Editor 侧 Mono 临时编译器“文件名或扩展名太长”；强制 `roslyn` 响应为 `Microsoft.CodeAnalysis` 不可用。连接和错误响应不能算运行见证；没有启动第二 Editor，也没有改变 Play/Scene。
- 下一项必须是原 Editor 的**一个**聚焦 full-tick 见证：自然 KO 后旧页已激活且 native timer 小于 144，在 Settings cursor0 的 Attack edge 前后比较 `FallDamageDiv`、native timer、战斗 checksum；另有同输入无旧页的对照。此前自然 KO / timer350 / held continue / 70-tick KO 寿命的通过项不重跑。
- 若该见证确认旧战斗字段被 UI 提前写入，生产修复需把结果设置保留在下一场配置事务中，并证明旧 World 的 combat 字段在 native transition 前后均不被 UI 改写；同时保留自有结果画面、精确更新旧 self-check 断言、验证重赛配置实际消费 owner。没有明确 owner 前不以简单禁用设置页或删除字段写入代替修复。

## 2026-09-27 原 Editor 后继见证

`NTSD28-Q08-RESULT-SETTINGS-FULL-TICK-WITNESS-001` 已按上述一项正反完整 tick 完成原 Editor 聚焦 1/1：自然 KO、旧结果页激活、P1 进入 Settings 并导航 cursor0 后，无 Attack 保持旧角色 `FallDamageDiv=0`，Attack 将其改为100；两例 native timer 均14、transition均0。详该 ID 的 `ACCEPTANCE.md` 和原始 job JSON。此前 `NATURAL_FULL_TICK_PENDING` 是此见证前状态；当前应读作 `UNITY_RUNTIME_COUPLING_CONFIRMED / FORMAL_SAME_ACTION_UNAVAILABLE / PRODUCTION_REPAIR_PENDING`。下一步先定义下一场配置 owner，不通过简单删旧结果页功能来消除差异。

Q07 仍 `IN_PROGRESS`，Q08 与 BATCH-04 仍 `IN_PROGRESS`；本审计不增加 Q07 的案例数量或改变其有限出口。
