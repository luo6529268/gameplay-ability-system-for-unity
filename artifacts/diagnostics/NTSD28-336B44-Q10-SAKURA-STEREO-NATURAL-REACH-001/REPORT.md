# Q10 Sakura 双声道 cue：336B44 自然输入可达性

2026-10-02。结论：`SOURCE_NATURAL_EVENT_AND_FORMAL_ROOT_ACTION_SCOPED_PASS / UNITY_PLAYBACK_PENDING`。这证明一条当前正式内容的低血量玩家按键链实际产生 `c/saku/w/tra.wav`，不证明 Unity 已播放它或已按原生双声道输出。

正式根 `NTSD2.8-Logan.exe` 在诊断前后 SHA-256 均为 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。探针只编译当前对应 playable 的 28 Core、`GameSession28`、Selection 与 LFR live path，读取当前正式 `resources/runtime`，没有改正式源码/资源。Unity 的正式暂存对象 DAT 中同路径帧声明已由上一[13 双声道 cue 清单](../NTSD28-336B44-Q10-STEREO-CUE-REACHABILITY-20261002/REPORT.md)逐 SHA 核对。正式 WAV `vfs/c/saku/w/tra.wav` 本轮再次以 `wave` 解析：2 声道、44,100 Hz、16-bit、123,466 帧，SHA-256 `A6D36A499DBEAB218690BC3FDEE5071E5B97165D5DB34427116A3CDE8660EBC7`。

输入条件：正式 Sakura OID1 槽0，action0、X500/Z650、当前 HP100、MP500、team1；Lee OID7 槽1，X1200/Z650、HP/MP500、team2；mode0、stage23、seed682973786。相对 tick1～2 按防御、3～4 按纵深上、5～6 按攻击，之后中性；见到 action172 后下一 tick 新按跳跃一次。没有直接设置 240、172 或 340，也没有人工入队声音。HP100 是必要条件：正式 `state:1150145` 经 `InputRouter28::apply_action` 在 HP>150 时将请求动作240重定向到145。本包先以 HP500 有界 36 组输入测得全都不进入240，[阴性网格](source-run-04/timing-grid.csv)与[控制逐tick](source-run-04/source-events.csv)保留，不能把高血量阴性解释为 stereo cue 不可达。

在 HP100 的第一组 2/2/2 释放防御时点即出现正例：[源码逐tick事件](source-run-05/source-events.csv)于 tick6 action240/MP150、tick7 action165、tick23 action172、tick24 新跳键 action340，并在 tick24 `GameSession28::last_tick().audio_events` 发出 `source=1/frame_sound`、`world_x=500`、`c/saku/w/tra.wav`。后续 tick25 仍 action340，未重复发该帧音效。独立第二次源码运行结果相同；两次 [timing-grid.csv](source-run-05/timing-grid.csv) SHA `084644387B3E962831E08DFC9D07263EFE0D4CFF69A88FE877E68725DF5F03A3`、`source-events.csv` SHA `DCBB853A5634D5A7F8A57B391D0BB439A304B2648E9261662022FF2879FDEA77`、LFR SHA `F1805AAD31EC3BEC8ADD07BE299BC3EEEC8F0FBC1B8A155A1AB487BB5190FEED` 各自一致。

两次 LFR 均由根目录正式 EXE 无界面回放；第二次用 `System.Diagnostics.ProcessStartInfo` 等待取得明确进程退出码 0。[正式报告](root-run-06-report.json) `passed=true/failureCode=0`、声明55 tick、完成56 tick、trace57行，`nativeParityClaim=false` 是报告的内建边界提示。两次根 trace SHA 均 `1CC4600616979AB3D60CFC510EFBC2E82B6B6D5B030352EFAF54A430534168B9`；源逐tick与根 trace 在声明的55 tick 上，槽0 action、MP、相机 X 共 **165/165 同值，首差0**，[机器对照](root-source-comparison-05.json)。第56个完成 tick 是 LFR 宿主末端额外步，不算作录制的55 tick 同态证据。根 trace 不公开 `audio_events`，所以原生 cue 身份/队列时点的直接证据限对应 playable `last_tick`；根 EXE 证明同 LFR 动作/资源门/相机行为，而不是扬声器或逐事件的根程序导出。

失败尝试均保留：v1 编译参数误覆盖 Core include，v2 编译入口缺 `-municode`/`WinMain`，v3/v4 编译为0诊断；source-run-01/02 输入资源根层级错误、未进战斗；source-run-03/04 的 HP500 有界阴性促成正式 `state:1150145` 条件复核。最终 v5 编译 exit0/stderr0。没有删除或覆盖以上原件。

当前 Unity 战斗音频的两个查找根对此相对路径仍为0/2个命中；这是静态资源缺口，**不能单凭本包声称 Unity 玩家已听到静音**。下一独立 Q10 包应在原项目只为这条已证 cue 接正式 WAV，验证 Unity 正式 DAT 帧事件→待播→解码 clip 2 声道→真实 battle voice，并用受控左右 PCM 与正式 stereo 对角矩阵核对；固定完整背景的声像策略仍等待用户选择，勿借此改相机或 DAT。Q10、Q11、Q12及总目标均开放。

本包只新增 Tools 诊断、Task/Record 与本地证据；未改 Unity 生产、DAT、WAV、Scene、Prefab、ProjectSettings、菜单或结果页。`Tools/Validate-ChangeLedger.ps1` exit0/PASSED、1152 Records、29 governed code paths，新增 probe 显式 COVERED；`git diff --check` exit0。详细校验留于[原始日志](change-ledger-validation.txt)和 Change Record。
