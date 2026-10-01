# C044 正式 OID16 自然跨零可达性

状态：`NATURAL_THROW_REACHED / NEGATIVE_DECREASE_CROSS_ZERO_NOT_REACHED`。当前336B44 playable源码、正式 `resources/runtime` OID16 `gaa.dat`、OID2鸣人、种子682973786、mode0/BG1、中立输入、双方HP/MP500。仅从正常初始动作和位置运行完整 `GameSession28::step()`；没有手设抓取关系、timeout或速度。

| OID16 初始动作 | 目标 X（近/远） | 近距结果 | 远距结果 |
|---|---|---|---|
| 245 | 550 / 1200 | tick1抓取；tick31到action128，timeout38→35，投掷1、跨零释放0 | 无抓取/投掷 |
| 363 | 600 / 1200 | tick1抓取；tick31到action407，timeout38→35，投掷1、跨零释放0 | 无抓取/投掷 |
| 364 | 700 / 1200 | tick1抓取；tick31到action427，timeout38→35，投掷1、跨零释放0 | 无抓取/投掷 |
| 365 | 800 / 1200 | tick1抓取；tick31到action377，timeout38→35，投掷1、跨零释放0 | 无抓取/投掷 |

每例完整运行160 tick，编译退出0、八次运行退出0；原始 `source-ticks.csv`、RNG CSV、LFR均在各自目录。近距在终端负 `decrease:-3` 帧虽进入关系 pass，但 timeout仍为正，因此按正式顺序继续 `throwvx:3` 投掷，不走跨零释放。X550例tick27～31的原始行可见 timeout69→59→52→45→38→35，tick31 `thrown_relations=1`、`released_relations=0`。这是一个有界阴性结论，不证明所有输入或战斗条件都不可能跨零，也不证明 Unity 跨零逻辑已对齐。

下一步按当前正式源码 `battle_world.cpp` 的跨零分支及同源 `battle_world_tests.cpp::test_native_catch_relation_negative_decrease_releases_below_zero`、`test_native_negative_decrease_release_uses_impulse_fields` 建受控低 timeout 2与未跨零4的逐 pass对照，再在 Unity共用 `BattleCpointWriter.RunKind1` 建先RED后修复的聚焦测试。受控初态不由正式根LFR承载，不冒充正式根/自然Scene。Q07/C044/总目标开放；本包未改生产、DAT、Scene或非战斗。
