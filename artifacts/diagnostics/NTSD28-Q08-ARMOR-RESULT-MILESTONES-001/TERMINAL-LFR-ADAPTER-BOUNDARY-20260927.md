# Q08 结果 350 的正式 LFR 适配器边界

状态：`READ_ONLY_ADAPTER_LIMIT_CONFIRMED / TERMINAL_TRACE_SCOPED_MATCH / FULL_PARITY_OPEN`。此复核不把失败的正式 LFR 报告改写成通过，也不关闭 Q08 或 BATCH-04。

2026-09-27 新鲜核对的正式根 `NTSD2.8-Logan.exe` SHA-256 为 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`。现有正式回放报告 `formal-hp3-365-rooted-report.json` 明确为 `passed=false`、`failureCode=46`、`completedTicks=365`。失败点不是结果字段比较：配对 playable `game_session_lfr.cpp` 的 `GameSessionLfr28::capture_after_step` 要求 `world->sequence() == next_row_ + 1` 且有同序号 `last_tick`；`GameSessionLfrPlayback28::step` 在调用 `session.step()` **之后**要求 `world->sequence() == completed_ticks_ + 1`，否则返回该报告中的错误。正式 `main.cpp` 的回放循环在检查 `step_ok` 前先输出诊断 trace，因此失败调用的结果状态仍被正式 EXE 记录。`game_session.cpp` 在 `battle_flow_.step` 得到非零 transition 后、执行战斗 driver 前返回；`battle_flow.cpp` 在 timer 350 将普通 mode 的 transition 置为 2，并把存储 timer 清零。

本次从未修改的正式 trace 最后两行重新解析，得到 `tick=365,365`，World `stateHash=dbeb45e5030b3fbf,dbeb45e5030b3fbf`；结果 timer `349,350`，transition `0,2`，整体确定性 hash 改变。这说明第二行是一次**发生了结果宿主状态变化、没有战斗 World tick** 的调用。对应 Unity 366 行的最后一行是 output timer 350、stored timer 0、phase 3、transition 2；既有 `comparison.json` 对 366 行、每行五个映射结果字段共 1830 项给出零差异。该终点是正式根 EXE 可观察 trace 的限定匹配，不是成功的 LFR 回放证书，也不是全状态同态证明。

处置：当前 LFR 的“每行必须增加 World 序号”合同与这个停战宿主调用互斥。**不要重复以同一 365 行输入尝试把终点报告跑绿，也不要修改正式 EXE/配对源码来伪造成功。**Q08 选中护甲场景的 timer 80/101/349 增量步及 350 宿主结果字段已有各自限定证据；后续若需更强终点证书，应设计不把宿主调用冒充 World tick 的独立观测合同。Q08 的其他正式可达 mode、结果和事件出口继续处理；自然整场、RNG/全部字段与视听验收仍由其原定 Q/R 出口承担。

本复核只读解析既有 JSONL/JSON 与正式源码，没有运行 Unity、Play 或正式 EXE 新会话；未修改 DAT、Scene、Asset、生产/测试脚本或非战斗行为。

## 2026-09-28 宿主末步双侧冻结复核

新建机器可读结果 `terminal-host-freeze-reconciliation-20260928.json`（SHA-256 `15DA24DBA31E5FA7E1572277A51422B8B20F135021679980B7BCFAF86CEBCE18`），只重读未修改的正式根 trace、正式失败报告、原 Editor Unity raw 和结果 sidecar。四输入 SHA-256 分别为 `464B0A342AAA59D06FBF004EC4A3779200A90830B11E05CDC4AC15D8D793BFC6`、`04467AFC9DB4E217CAE97CA73DA25CCFCBD349345FFE58D6B223BB3D858F6AE3`、`639FB4A1EFEC4FB25C839740A202B9CB817875D766CE5516F749166B06A95C59`、`6855B3E073882A8E0451CC919031AAD18F072322BE3D4832DEE84ABB6D76B561`。正式根 EXE 身份本次复核为 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`。

正式末两行 World tick `365→365`、`stateHash dbeb45e5030b3fbf→同值`、RNG 对象逐字段相同；`deterministicStateHash` 从 `5d3dc679aea46860` 变为 `3917ee6b881d56ef`，因为宿主结果字段、事件列表与诊断行变化，不能拿它否定 World 冻结。两行顶层差异恰为 `battleFlow`、`deterministicStateHash`、`events`、`diagnostics`。原 Editor Unity raw 的第365/366调用各有2个导出实体，两个实体数组逐字段完全相同；结果 sidecar 输出计时 `349→350`、存储计时 `349→0`、phase `2→3`、transition `0→2`、存活组掩码 `2→2`。这与正式 World 不推进而结果宿主推进的边界相符，也进一步排除“Unity终步必须新增一帧实体运动”这一错误验收预期。

证据范围仍有限：Unity raw 只覆盖两名导出实体，不是全 World/注册表 hash；不同引擎的 hash 没有互相比值。正式 LFR 报告继续为 `passed=false / failureCode=46 / completedTicks=365`，绝不能改写为通过。原有 366×5 结果字段同值和本次双侧内部冻结只构成结果末步限定宿主证据，不关闭 Q08、自然整场或 BATCH-04。本次没有重跑正式 EXE、Unity、Play，也未修改脚本、DAT、图片、Scene 或非战斗行为。
