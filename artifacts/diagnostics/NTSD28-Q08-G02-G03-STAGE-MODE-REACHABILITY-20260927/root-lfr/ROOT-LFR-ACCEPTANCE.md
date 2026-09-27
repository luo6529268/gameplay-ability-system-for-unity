# 鸣人 OID33 stage gate 正式根 EXE 三 tick 回放

2026-09-27，`NTSD28-Q08-CLONE-STAGE-GATE-ROOT-LFR-001 / RUNTIME_PENDING / SCOPED_ROOT_TRACE_PASS_RNG_UNMATCHED`。正式根 `NTSD2.8-Logan.exe` 在本包执行前后 SHA-256 均为 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`。它直接消费配对 playable `GameSessionLfr28` 录制的三 tick LFR，使用正式 `resources/runtime` 双根，不改正式 EXE、正式资源或 Unity 生产代码。根回放的 RNG 与录制 Session 不同，因此本包**不是完整同状态回放证书**。

录制输入：Stage23、seed682973786、mode0、difficulty1、所选 mode stage gate1；slot0 鸣人 OID2/team1/X0/Z650/HP/MP500/action121/facing0，slot1 李 OID7/team2/X1200/Z650/HP/MP500/action0/facing1；三 tick 无键。诊断 helper `Tools/NTSD28Q08Diagnostics/naruto_clone_stage_gate_lfr_probe.cpp` 从 playable 同一 28 个 core 源与 `game_session.cpp`、`selection_flow.cpp`、`lfr_recorder.cpp`、`game_session_lfr.cpp` 编译，`g++` 退出0。helper SHA-256 `7B87585A0AB20F23131CD800BC25F3169926AD0141C0B93358A9902C16598691`，其 exe SHA-256 `F8A9E1C1B47A58AE6C8445744429B6A6289B063AAFE82D5F7D9D4AA340B990C1`。录制器输出三 tick、9976 字节 LFR；独立 CSV 为每 tick slot0/1/50 的 oid/X/action，gate均1。

正式根 EXE 以 `--resource-root`、`--complete-vfs-root` 指向同一正式 runtime，并以 `--headless-playback-lfr`、`--headless-playback-report`、`--headless-playback-trace` 回放；显式 `--lfr-slot0-action 121 --lfr-slot1-facing 1 --lfr-slot0-mp 500 --lfr-slot1-mp 500` 固定所比较的角色字段。报告 `passed=true / failureCode=0 / declaredTicks=3`，scene OID2/OID7/Stage23，初始 actor 和 opponent 的动作、朝向及 X 均在 trace tick0 核实。报告的 `completedTicks=4` 与 trace tick4 是回放包装器额外终步；本合同只比较声明的 tick1–3，不把 tick4 算入场景输入。报告 `nativeParityClaim=false`，因此本包证明的是**正式根 EXE 在该输入下直接输出的可观察 trace**，不把 LFR 管理器的 PASS 扩大成全战斗 parity 证书。

| 完成 tick | 正式根 EXE OID33 type0 slot50 X/action | 原 Editor 源规则 X | 原 Editor 物理 X | 所列字段比对 |
|---:|---|---:|---:|---|
| 1 | 未出生 | 未出生 | 未出生 | 同值 |
| 2 | -42 / 241 | -42 | -42 | 同值 |
| 3 | 0 / 242 | -49 | -49 | 首差 |

根 trace 每行 `battleMode=0 / selectedModeStageGate50=1 / difficultyLevel4A0C30=1`，初始 slot0/1 的 OID/X/action 与 Unity 严格三 tick夹具相同。机器对照 `formal-root-vs-unity-three-tick-comparison.json` 仅断言上述三个 tick 的分身身份、出生及所列 X 字段差异序列 `[false,false,true]`。正式根 report/trace SHA-256 分别为 `EA8771D2CDF2348D61FAC0FFF7330EAAA4D4A5A83711941265AEC4C40836C7BC` / `69E1DFB15CC6D67398DCF2730F75E1C8187BF7AA69EA6E0ABE9102E0FCCB8B0C`，LFR SHA-256 `43F87FAFAEF4D329F1E8462C5FEE73DB2EE1EFEBFFCD397FDE435D8501164155`，Unity sidecar SHA-256 `A09E114316323C7661FB4335CE849AD06B2528D344D3E766EEA29F5F7CC6B765`。

RNG 限制的直接实测：在同一 helper 加列并重新编译后，v2 源码 SHA-256 `A352E24E55D43D5BEE78E20EE5607946B745373AA8CDDD40944B494EB8AD74BD`，其录制 LFR SHA 与首版完全相同；录制 Session tick1 CRT state `1758127634`、calls3000、同步 RNG counter/index/calls=`1/1/1`。正式根 LFR 回放 tick1 CRT state `3374725112`、calls3000、同步 `0/1/0`，provenance=`locked_lfr_manager`；根 tick3 同步 calls6，录制 Session calls7。早先配对 scenario tick1 也为 CRT state `1758127634`。`GameSessionLfr28` 从 manager 表恢复同步 RNG，并将 provenance 设为 locked manager；当前发行 CLI 未见指定 CRT seed 的 LFR override。明细见 `rng-carrier-audit.json` 与 `../root-lfr-rng-v2/naruto_clone_stage_gate_source_ticks.csv`。这解释了为什么“同角色/输入”不能写成“全部初态相同”；根 EXE 的 gate1 边界直接输出仍是事实。

本包使 G-02/G-03 的 mode gate 1/type0 slot50 边界行为具有正式根 EXE **直接输出**见证，并与配对源码结果一致；由于 RNG 不同，正式根对 Unity 的完整同状态首差证书仍待。后继生产候选应另建 Task/Change：从项目自有 `ProjectBattleModeConfig` 为所选战斗模式提供 stage gate，作为确定性 battle runtime 状态进入 DataOriented 与 Legacy 两个 type0 边界写者、snapshot/checksum 和复位；对 gate1 与 gate0、低槽及高槽做聚焦 RED→GREEN，并在原 Editor 完整 Driver 复测。用户排除的原版背景和两类 mode DAT 不得接回，D-024 比例位移和 D-025 非角色可行走区 10 秒例外保持。Q07/Q08/BATCH-04/总目标仍开放。
