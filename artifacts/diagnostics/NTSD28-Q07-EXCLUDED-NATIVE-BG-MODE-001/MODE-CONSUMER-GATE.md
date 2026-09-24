# Q07 原版 mode DAT 退场前调用者门槛

用户于 2026-09-24 确认不用原版 `data/mode.dat` 与 `data/mode/ntsd.dat`，也不用 `data/bg_mode.dat` 系列及 `b/*/b.dat`。本文件是当前代码的静态调用链清单，不是退场验收；两项 mode DAT 原件仍在生产内容根，不能直接移走。

| 层 | 当前读写者 | 脱钩时必须保持的合同 |
|---|---|---|
| 内容捕获 | `LoganObjectCatalog` 调用 `LoganModeComboInput.Capture`，读取两个 DAT；无 `mode.dat` 时返回 null。 | 明确项目模式的配置来源与缺省值，不再从正式 DAT 取值。 |
| 内容身份 | `LoganContentIdentity.FromBattleComponents` 纳入 mode 原始/语义指纹；`BattleRuntimeDataCatalog` 检查相同输入。 | 同一次发布的内容身份、新鲜度和 World seal 必须一致；不能只删文件而沿用旧指纹/候选。 |
| 战斗首 tick | `SimulationTickDriver.ApplyPublishedModeComboBeforeFirstTick` 将 `bound/facing/respond/caughtact` 写入 `NativeCombo`。 | 用项目自有值或明确默认值，保留一次性首 tick、reset/snapshot/checksum 合同。 |
| KO 战斗与表现 | `ApplyPublishedKnockoutFeedBeforeFirstTick` 同时设置 feed lifetime、KO 音频、展示；`BattleKnockoutFeedRowProjection` 消费 feed；`CharacterAnimtorManager` 和 `LoganVisualContentCandidate.NativeKillIconInput` 消费图标记录。 | 模式配置、音频事件、图标/文字候选与展示 gate 要一并处理；不能只断开 combo 字段。 |
| 现有项目配置 | `GameModeConfig` 只有 `gameModeId` 和 `battleGameModeId`；`MatchConfig` 有 KO 显示开关及名字覆盖。`Assets/NTSD/Config` 未找到项目自己的 mode DAT。 | 这不是等价替代数据；数值来源/默认行为必须先决定，不能擅自把原版 DAT 数值硬编码或默默丢失。 |

退场顺序：先确认项目自有模式值策略；以独立 Task/Change 修改内容捕获、身份、发布和全部 battle/KO 消费者；运行聚焦 EditMode、标准自检及原项目 Battle Scene Play；核对双 Scene 磁盘哈希；最后再由用户按明确范围清理原件。`mode.dat` 双文件和 metas 已作为**备份副本**放入统一 zip，但原件仍在 `Assets`。当前误启动的全量 EditMode 作业不能充当聚焦验收。
