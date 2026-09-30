# G0 规则版本漂移：旧 B1E13 与当前 336B44 的源码影响盘点

状态：`READ_ONLY_VERSION_DELTA / AUTHORITY_CHOICE_PENDING`。本报告只比较已保存的更新前源码与当前根目录同路径源码，辅助决定哪些 Unity 对齐证据需要版本化回访；**不把 336B44 自动提升为正式战斗权威，也不把静态差异当运行时首差**。原始逐文件字节、SHA-256 和行数清单见 [changed-files.csv](changed-files.csv)，共 29/29 个被保存文件确实改变，清单 SHA-256 `799DDDD26A350C2947A8A646BCC57346D5C1987993549C00665C6852495F6BBD`。

## 身份与范围

- 已确认旧规则身份是 `docs/ai/CURRENT-AUTHORITY.md` 与根 `AGENTS.md` 指定的 EXE SHA `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`。`battle_scene_reverse/parity/runs/logan35_port_20260929_v1/pre_update/NTSD2.8-Logan.exe` 本轮实测与它逐 SHA 相同。现根 EXE 为 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。
- 备份的 29 文件中：正式 playable Core/Session 规则源码及头文件 14，offscreen 专用入口 1，构建/测试脚本 2，测试源码 12；29 个均字节变化。当前 `source` 排除 `build/` 两份测试 EXE 后的 159 文件/4,371,509 字节树 SHA 是发布说明所列的 `2924DDD8C153EE34378081578D21D7EF9EB7E8BDF73799695C89C56DC3418CA1`。当前 runtime 2,656 文件/101,099,439 字节树 SHA 是 `F3EA4516BD903F10131217A856947F7C0F91DB72F506F6E2A7F88D6281F3FD41`。发布说明称 runtime 与更新前相同；本轮没有可独立重算的更新前 runtime 备份树，因此这部分只能记为发布说明声明加当前树复核。
- `RELEASE_INFO.md` 将更新描述为 35 个固定差异 ID 的限定机制修复，`BATTLE_REPAIR_PROGRESS_20260928.md` 记录各 ID 的聚焦验证和局限；这不是所有场景、Unity或设备对照完成证书。29 文件包括测试和离屏诊断，并非 29 条 Unity 待修功能。
- 2026-09-30 补做旧源码树只读复原核对：对当前 159 文件源码树，在内存中以 `pre_update/source` 的 29 个同路径文件替换其哈希和长度，再逐一尝试剔除其余单个路径；唯一匹配旧发布说明 158 文件、4,239,978 字节、规范化树 SHA `3695197B04B6291C494684B309E483C824A821F9858B73FFA092CDF6AD469488` 的剔除项为 `ntsd28_playable/tests/c032_audio_device_probe.cpp`。未在磁盘重建或覆盖源码。这证明 29 个备份加当前未变文件足以逐文件复原**旧发布源码包的身份**；不单独证明旧 EXE 的编译产物与每一源码文件对应，也不证明旧运行时资源仍与更新前逐字节相同。旧备份 EXE 则已独立实测为 B1E13 正式身份。

## 实际生产规则改动与 Q/R 回访映射

| 当前源码变更点 | 代表固定 ID | 若选择新版时的战斗影响与 owner |
|---|---|---|
| `ntsd28_core/src/simulation/battle_flow.cpp`：输入写 350 后下一 tick 才执行退出；两组复活时暂停计时并刷新赢家 | C008、C009 | **Q08/R07/R15**：战斗胜负、结果计时与事件。N01 的结果页重赛宿主流程属于用户排除的 G-07 操作，不能拿它另开菜单/重赛改造。 |
| `input_routing.cpp`：移除 type0 `HP<=0` 的全局清键/早退 | C017 | **Q07/R02/R18**：零血但仍处站立动作时输入是否可路由；不表示倒地或尸体均可出招。 |
| `frame_machine.cpp`、`battle_world.cpp`：kind2 CPOINT 保持帧、空中 state0→212 的计数时点、`next=13xx` 同步随机相对动作、终局 state14 计数清零 | C022、C023、C024、F05 | **Q07/R02/R05/R06/R15/R18**：正式可达帧/组合技/复活时序与同步随机；现有旧版局部证书不能无条件跨版本复用。 |
| `simulation_tick_driver.cpp`、`battle_world.cpp`：每 tick 单独翻转传送相位，仅新相位 0 运行 state400/401；特殊命中锁存于活跃物件帧尾清除 | C011、C012 | **Q07/R04/R05/R09/R18**：传送隔 tick 与连续命中候选；只复验对应自然入口，不重做全部输入或碰撞矩阵。 |
| `physics_integrator.cpp`、`battle_world.cpp`：type4/6 高速 state1000 动作40、CPOINT kind2 抑制积分、严格穿地的 state12/18、环境标记尾清理与声道6事件、恰达最大HP仍保留回血计时 | F02、C029、C031、F03、C032、F04 | **Q07/R06/R07/R08/R18** 处理物理/资源，声道6实际战斗事件转 **Q10/R17**；设备听感仍需另证。 |
| `defense_resolution.cpp`、`hit_response.cpp`、`battle_world.cpp`：防御读 `bdefend`，特殊BDY提前响应仍写45，effect22/23 冲量、effect21 当前状态及整攻击者终止、linked rest跳垂直反应、`hit_Uj`读锁存动作 | F01、C048、C050～C053 | **Q07/R09/R10/R18**：命中候选、伤害、防御与动作读口。当前没有这些分支全部自然出现的证据，不因源码差异扩跑角色笛卡尔积。 |
| `battle_world.cpp`：抓取定位动作/CPOINT来源、投掷计数、负 decrease 延后冲量、`recover`与`cover`分离、失效关系尾；融合帧锁存与解融合精确坐标 | C040、C042～C045、C054、C056 | **Q07/R12/R13/R16/R18**：持有/投掷/融合及复用时序，只有新可达首差才触发相应回访；DAT值不改。 |
| `render_snapshot.cpp`、`game_session.cpp`：原生计时传递、可选 combo 的 times/等号/镜头边界/非角色拥有者 | N02、combo-times、combo-boundary、C059、C060 | **Q09/R14** 仅正式可达且非例外的战斗 combo 命令；原生 HUD 计时 N02、完整 KO feed 默认显示属 P-17/P-19 用户排除，不能列为战斗画面必修。 |
| `render_snapshot.h` 的 KO feed 默认开关与 `offscreen_gate_main.cpp` 显式探针 | KO-default | 默认结果/KO feed 的原生图文是 P-19 例外；战斗 KO 事件本身仍属 **Q08**，若战斗音效有独立可达消费者才进入 **Q10**。离屏探针不是正式根窗口像素。 |

`ntsd28_playable/scripts/build.ps1` 增加 `OutputRoot`，`test.ps1` 增补固定 35 项验证；它们改变构建/验证工具，不直接定义 Unity 战斗规则。`ntsd28_playable/src/offscreen_gate_main.cpp` 是专用诊断入口，不等于正式根播放路径。上述映射是依赖触发索引，不是修改授权或完成判断。

## 当前 Unity 可直接定位的条件性静态首差

以下结论**仅在用户决定 336B44 为新规则权威后**升级为待修候选，仍需同状态运行确认时点与副作用；继续旧 B1E13 时，不据此改 Unity。

1. 新 `battle_flow.cpp:115-127` 对结果继续输入在本轮写 350、下一轮执行退出；Unity `Assets/NTSD/Scripts/Simulation/Ecs/Results/BattleResultsOutcomeHostWriter.cs:68-93` 在写 350 的同轮设置 `NativeTransitionState` 并清 timer。新规则下有明显静态时序差；不涉及结果页按钮布局。
2. 新 `input_routing.cpp:1517` 明确无全局零血输入门；Unity `Assets/NTSD/Scripts/Simulation/Input/NTSD28InputTwoPassModule.cs:88-107` 对零血 type0 跳过路由，`NTSD28NativeComboStateMachine.cs:40-45` 还清空输入。需按 action0/零血与 state14 两例验证具体首差。
3. 新 `simulation_tick_driver.cpp:388,591` 仅每隔一轮执行 native state400/401 传送；Unity `Assets/NTSD/Scripts/Simulation/Core/NTSDBattleTickSystem.cs:425-427,694-697` 每轮调用 `NativeTeleportAll()`，该调用的 `BattleEarlyFrameAdvanceModule.RunNativeTeleport` 无相位参数。`FrameToggle` 存在于另一 early-specials 路径，不能因此推定这里已经有门。
4. 新 `simulation_tick_driver.cpp:1082` 在活跃物件 tick 尾清 `special_hit_latch_0eb`；当前 Unity 所检生产写者会置真，`NTSDEntityRuntime` 的清零发生在重置，尚未在生产 tick 尾定位清零。需覆盖同 tick 阻断与下一 tick 恢复两个方向，不从 `rg` 阴性单独宣称完整缺陷。

其它条目仍需沿原 Q/R 的现行 Unity writer/reader和实际可达状态核对。Q10 后续 Jump 的同一 LFR 已在逐 SHA 相同的**旧版归档 EXE**隔离回放：报告 PASS，tick30～38 的鸣人 action/MP 与当前源码诊断 9/9 同，tick34 为 action326/MP250。又以唯一复原的旧发布源码树隔离编译/运行本包探针，旧源码 `GameSession28::last_tick.audio_events` 同 tick 确有一次 `data/078.wav`，其LFR与当前源码生成物逐SHA相同；但旧EXE公开trace无audio，不把旧源码事件写成旧EXE扬声器输出。详[Q10 限定验收](../NTSD28-Q10-ORASENGAN-JUMP-NATIVE-LFR-001/ACCEPTANCE-20260930.md)。除此限定入口外，35项的新旧同条件EXE回访、Unity Play/编译/自检均未在本轮执行。

## 下一步

等待用户对 `B1E13…` 或 `336B44…` 的明确版本选择。旧版 EXE 与 158 文件旧发布源码树身份已可逐 SHA 复核，Q10旧版源码音频事件和旧EXE动作各有局部证书；若继续旧版，仍需正式确认Unity声音consumer、实际clip与设备出口。若选新版，则先正式更新权威文档与版本标签，再只对可能受影响的 Q/R 证据复验。两个分支都无需改非战斗场景、原版背景/mode DAT 或 Unity 项目框架；本报告不启动生产修复。
