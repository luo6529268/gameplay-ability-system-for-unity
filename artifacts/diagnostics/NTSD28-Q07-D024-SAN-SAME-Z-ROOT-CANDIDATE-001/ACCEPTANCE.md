# Q07/D-024 正式 San 与项目地图共同源 Z400 候选限定验收

日期：2026-09-29。结论：`SAME_SOURCE_Z_FIRST_CANDIDATE_MATCH / FULL_TICK_FIRST_DIFFERENCE_OPEN`。本包只给已有 Tools 诊断增加正式背景 ID1 的可选模式；Unity 内容仍是**项目自有 Sunagakure 地图**，原版 `b/San/b.dat` 只由正式发行 runtime/EXE 用于对照，不导入 Unity，不修改任何 DAT 值。

正式 `decoded_dat/data/data.txt` 的 ID1 为 `b/San/b.dat`，正式 `zboundary: 375 575`；当前 Unity 项目地图物理 Z237..760按统一投影对应源≈150.1823..481.5972，所以源 Z400 双方均在各自合法舞台内。先前 ID23/Hos 使正式 Z400 在 tick1 钳到542，仅限制那组背景组合，不能概括所有正式背景。

工具从正式 `ntsd28_playable/scripts/build.ps1` 的28个 coreSources 与同一 playable 的 `game_session.cpp`、`selection_flow.cpp`、`lfr_recorder.cpp`、`game_session_lfr.cpp` 组成闭包，以 g++17/O2 编译到本目录唯一 `han-san-z400.exe`，退出0。正式根 `NTSD2.8-Logan.exe` 本轮 SHA-256 为 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`。新模式 `san-z400-jump3` 两例生成独立 CSV/LFR，重复执行在已存在首个输出处退出4且该 CSV SHA 不变；旧模式/输出名未变。

| 输入与首个 action146 | 正式配对 playable | SHA确认正式根 EXE | 原 Unity Battle Scene 已存报告 |
|---|---|---|---|
| 韩X500/李X520，双方源Z400；Attack tick1–2，Jump tick3–4 | tick10，候选1；韩X/Z547/400、李508/399 | LFR `passed=true`、`failureCode=0`；tick1 Z400/400，tick10 X/Z及动作与配对相同 | 完整Driver tick14（相位 +4），首次完成行候选1；源X/Z547/400、508/399 |
| 韩X500/李X580，同输入与Z | tick10，候选0；韩535/400、李580/400 | LFR `passed=true`、`failureCode=0`；tick1 Z400/400，tick10 X/Z及动作与配对相同 | Driver tick14，候选0；源X/Z535/400、580/400 |

两份正式根 LFR 均声明50 tick，根报告完成51 tick（回放尾部额外 tick）；正式配对 CSV 与根 trace 的动作、X、MP/HP、抓取关系等8个选定字段各400/400同值。原 Unity 的近/远报告在项目可走多边形内启动、physical Z≈631.2329，先前已验证关停借用0及五保护文件稳定；本轮复用旧报告，没有重新启动 Editor 或把不同背景像素称作一致。

**保留的首差：** 将已存 Unity 报告的9个完成行按其固定启动相位与正式根 tick2–10配对，韩动作、双方源X/Z及李动作六个所选字段各52/54同值。两例均在正式 tick4 对应 Unity Driver tick8出现李动作 `formal 1 / Unity 0`，tick8对应 Driver tick12出现 `formal 2 / Unity 1`；其余所选字段同，首次146行两端又收敛。Unity 李在运行中注册而正式李随 Session 建局，这**可能**是目标年龄/初始化相位差，尚未同出生相位验证，不能写成已证生产 bug，也不能宣称全 tick/全 World 对齐。Unity `OBSERVED_CACHE_LIMIT` 报告也未证明 collector 内部首次拒绝谓词。下一出口需冻结同出生年龄与输入相位，再核此两处动作和更多状态，而非增加角色矩阵或调整 DAT。

**2026-09-29 相位纠正，覆盖上一段的“李动作首差/出生年龄待判”解释：** 上述52/54把韩的输入响应相位强加给无输入的李，是比较时钟不一致。正式 `simulation_tick_driver.cpp` 先把 `input_update_phase_4a0b90` 从0推进到1，首 tick 不采样按键；tick2 的 phase0 才采样，所以根 trace 的韩从 tick2 进入动作60。Unity 旧探针直接向 `StepOneTick` 提交 `FrameInputSet`，韩在其 relative tick1 已进入动作60。李双方均从动作0出生且无输入：按各自**出生后完成 tick 数**对齐，正式 tick1–8 与 Unity relative tick1–8 的李动作、源X、源Z在近/远两例各24/24相同（正式 tick4/8 与 Unity relative tick4/8 同转动作1/2）；韩按输入响应对齐，正式 tick2–10 与 Unity relative tick1–9 的动作、源X、源Z各27/27相同。这样此前两处李动作差异已归因为**诊断比较相位**，不能据它修改通用帧逻辑，也不能把不同 tick 的两名角色拼成同一 World 快照。首次146候选分支近1/远0仍限定通过；统一输入相位、同 tick 全状态及正式/Unity完整可观察表现继续开放。证据为本包两份根 trace、两份旧 Unity 报告和正式源码 `SimulationTickDriver28::step` 的输入采样门槛；本次仅只读重算，不重跑 Editor 或改生产脚本。

证据文件：`native-san-z400/han-action0-x520-jump3-bg1-z400.csv/.lfr` 与 X580 对应文件；`root-x520-bg1-z400-report.json`、`root-x520-bg1-z400-trace.jsonl` 及 X580 对应文件；Unity `../NTSD28-Q07-D024-HAN-CANDIDATE-BRANCH-001/q07-han-inmap-near-z400-20260929-a.json` 和远距离对应文件。Battle/Menu Scene、SunagakureMap、GameConfig、ProjectBattleModeConfig 五个 SHA-256 仍与[上一限定验收](../NTSD28-Q07-D024-HAN-INMAP-PAIRED-CANDIDATE-001/ACCEPTANCE.md)列出的完整基线一致。Q07/BATCH-04和总目标继续开放；项目地图 stage 双域、AI/出生/worker等消费者的独立修复任务也不因这组共同Z候选通过而消失。
