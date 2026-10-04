# C056 原 Battle Scene 合体停帧与 OPoint 定向 Play（2026-10-01）

权威为当前 336B44 正式 EXE 对应 playable 源码和正式 resources/runtime；本报告的逐 tick 源侧结果来自按正式源码重链的 GameSession28 受控探针，不冒充根目录正式 EXE 的实测。Unity 使用原项目原 NTSD_Battle.unity、正式 OID7/8/51/213 暂存 DAT、同组 OID7/8、主角 action9/HP100、源 X304/伙伴 X300、Z600、正常模式与难度0、seed682973786、中性输入、完整 SimulationTickDriver.StepOneTick。项目自有地图会将 Z600 限到源域 Z481；这是既有自有地图边界，本案比较合体动作/计数/出生时序，不把场景坐标差写成正式战斗规则同值。

首次 [c7-scene-01](fusion-hold-c7-scene-01.json) 保留为无效前置的首差：探针遗漏了双角色 AiControlled=false，且伙伴朝向未按源例设为左；tick1 为 OID7/action650，未合体，结构出生 0/0/0。不能从此轮单独断言究竟是 AI 还是朝向造成失败；修正初态后才取得下面两组正反结果。此轮仍实际退出 Play、池2→0、Scene clean/哈希不变。

| 初始动作计数/停顿 | 正式源码 tick1～3：OID/action、计数、停顿、出生 | Unity 原 Scene tick1～3 | 结果 |
|---|---|---|---|
| 7/3 | OID51/action290；计数 7/7/1，停顿 2/1/0，出生 0/0/0 | [c7-scene-02](fusion-hold-c7-scene-02.json)：相同 OID/action、计数、停顿、结构出生；伙伴休眠；池2→2→2，退出后0 | SCOPED_PASS |
| 0/3 | OID51/action290；计数 0/0/1，停顿 2/1/0，出生 1/1/0 | [c0-scene-02](fusion-hold-c0-scene-02.json)：相同 OID/action、计数、停顿、结构出生；两次最后出生 OID213，Unity活动数1→2→2；池2→3→4，退出后0 | SCOPED_PASS |

两次测量都从原 Editor 的单个 clean Battle Scene、非 Play 状态发起，Play clone 在 Bootstrap Start 前设置 OID7/8；内容根为 Assets/NTSD/Content/LoganRuntime。每次3个完整 Driver tick，独立请求/独立结果。两次均退出 Play，原 Battle Scene 前后 SHA-256 3A089236328ACAE1510F8A831B77D4895CC34028DDCDEBE542BEF0DA8EC235ED，编辑器场景 clean。Menu Scene、GameConfig、ProjectBattleModeConfig 的保护哈希也未变化。原 Editor Tundra 编译0 error；最新生成 Editor 项目 dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:q -clp:ErrorsOnly exit0、235 warning、0 error。

证据边界：shutdownStageAfterExit=-1 表示域重载后旧 Driver 引用不可读，**没有**本次 Scene 的十一阶段关闭序列观测；引用池归零与 Scene clean 是已观测事实。此处“正式源码探针只记录出生、未枚举活体”是 2026-10-01 的历史快照，已由下文 2026-10-04 独立活体诊断补证。当前根正式 EXE 同受控计数/停顿入口亦未实测；C056父项及Q07仍为 RUNTIME_PENDING，不能据此标整组完成。

后续只读核查正式根可控入口：根 EXE 哈希仍为 336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3。对应 playable main.cpp 的 headless LFR 选项仅能覆盖初始 action/facing/MP 和输入起始行；没有直接设置 frame_counter 或 motion_hold_timer 的选项。game_session_lfr.cpp 的初始 manager 记录 base_hp，回放以 base_hp 同时恢复初始 HP/base HP；并不序列化这个受控案例所需的“HP100、baseHP500、动作计数7/停顿3”独立初态。因此不能把本源码探针封成 LFR 后称为根 EXE 的同态复现。正式根的该条件需自然战斗达到相同门，或另有不改变发行 EXE 的可观测输入载体；在找到之前保留明确条件门，不反向改 DAT/生产规则，也不让这个不可注入条件阻止其它独立 Q07 首差推进。

2026-10-04 补证：当前正式336B44对应playable完整GameSession探针新增逐槽活体OID213计数，计数7/停顿3为0/0/0、计数0/停顿3为1/2/2，与本报告两次有效原Scene JSON逐tick一致；结构出生亦同。只关闭源活体子门，原Scene十一阶段关闭轨迹及根EXE受控入口仍待，C056/Q07仍开放。[独立报告](../NTSD28-336B44-Q07-C056-SOURCE-LIVE-COUNT-001/REPORT.md)。
