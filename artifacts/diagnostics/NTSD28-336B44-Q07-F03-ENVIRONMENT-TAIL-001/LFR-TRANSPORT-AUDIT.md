# F03 正式根 LFR 初态载体审计

2026-09-30，只读。当前正式根 EXE 是 SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。F03 验证的是物理公共尾在最终帧不为 state12 时清 `environment_state_320`；Unity 既有聚焦测试使用初始标记10。该值必须与正式根同初态，才能把逐 tick 对照称为根同态。

当前正式 playable `game_session_lfr.cpp` 的 `GameSessionLfrPlayback28::load` 只从 manager 的13列物理快照重建 active、objectId、team、X/Y/Z、MP、owner、baseHP 等，构造 `CombatantConfig28` 时没有填写 `environment_state_320`；`initialize_session` 只额外恢复 owner、participantClass、damageScale。`game_session.cpp` 初始化战斗实体时从默认 `BattleConfig28::p1_environment_state_320=0` 写入该字段。根 `main.cpp` 已检 LFR 覆盖参数只有 slot0/1 action、facing、MP 等，没有环境标记覆盖。因此，一个**手动配置非零初始标记**的源码录制即使 LFR checksum 通过，也不会让正式根从同一标记开始；若直接比较最终0，可能把没有执行有效清理的假阳性写成 PASS。

可行的下一个入口是在正式 source live path 中找**同一场战斗自然产生的非零标记**，例如 `battle_world.cpp` 的实际命中关系/投掷写者，再由同输入的 LFR 使源码和根都自然产生。写者候选位于 `battle_world.cpp` 的 impact 伤害目标写入、抓取投掷伤害写入；只有确认正式 DAT 和真实可达帧、录制中先观察非零、再观察最终帧尾清，才可选作 F03 证书。当前尚未取得这样的案例，也未运行新正式根/Unity F03 对照。不得为了制造同态而修改正式 DAT、根 EXE、发行源码、LFR 格式或生产规则。

证据层级：这是**只读源码与参数审计**，不是 F03 根测试或自然 Play PASS。父 F03 仍为 `UNITY_FULL_TICK_PASS / RUNTIME_PENDING`；Q07 与总目标继续开放。
