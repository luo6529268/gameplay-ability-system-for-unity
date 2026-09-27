# Q08 结果 350 的正式 LFR 适配器边界

状态：`READ_ONLY_ADAPTER_LIMIT_CONFIRMED / TERMINAL_TRACE_SCOPED_MATCH / FULL_PARITY_OPEN`。此复核不把失败的正式 LFR 报告改写成通过，也不关闭 Q08 或 BATCH-04。

2026-09-27 新鲜核对的正式根 `NTSD2.8-Logan.exe` SHA-256 为 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`。现有正式回放报告 `formal-hp3-365-rooted-report.json` 明确为 `passed=false`、`failureCode=46`、`completedTicks=365`。失败点不是结果字段比较：配对 playable `game_session_lfr.cpp` 的 `GameSessionLfr28::capture_after_step` 要求 `world->sequence() == next_row_ + 1` 且有同序号 `last_tick`；`GameSessionLfrPlayback28::step` 在调用 `session.step()` **之后**要求 `world->sequence() == completed_ticks_ + 1`，否则返回该报告中的错误。正式 `main.cpp` 的回放循环在检查 `step_ok` 前先输出诊断 trace，因此失败调用的结果状态仍被正式 EXE 记录。`game_session.cpp` 在 `battle_flow_.step` 得到非零 transition 后、执行战斗 driver 前返回；`battle_flow.cpp` 在 timer 350 将普通 mode 的 transition 置为 2，并把存储 timer 清零。

本次从未修改的正式 trace 最后两行重新解析，得到 `tick=365,365`，World `stateHash=dbeb45e5030b3fbf,dbeb45e5030b3fbf`；结果 timer `349,350`，transition `0,2`，整体确定性 hash 改变。这说明第二行是一次**发生了结果宿主状态变化、没有战斗 World tick** 的调用。对应 Unity 366 行的最后一行是 output timer 350、stored timer 0、phase 3、transition 2；既有 `comparison.json` 对 366 行、每行五个映射结果字段共 1830 项给出零差异。该终点是正式根 EXE 可观察 trace 的限定匹配，不是成功的 LFR 回放证书，也不是全状态同态证明。

处置：当前 LFR 的“每行必须增加 World 序号”合同与这个停战宿主调用互斥。**不要重复以同一 365 行输入尝试把终点报告跑绿，也不要修改正式 EXE/配对源码来伪造成功。**Q08 选中护甲场景的 timer 80/101/349 增量步及 350 宿主结果字段已有各自限定证据；后续若需更强终点证书，应设计不把宿主调用冒充 World tick 的独立观测合同。Q08 的其他正式可达 mode、结果和事件出口继续处理；自然整场、RNG/全部字段与视听验收仍由其原定 Q/R 出口承担。

本复核只读解析既有 JSONL/JSON 与正式源码，没有运行 Unity、Play 或正式 EXE 新会话；未修改 DAT、Scene、Asset、生产/测试脚本或非战斗行为。
