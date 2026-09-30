<!-- CHANGE-RECORD
id: NTSD28-Q10-ORASENGAN-JUMP-NATIVE-LFR-001
status: FOCUSED_TEST_PASS
change-kind: CODE
code-path: Tools/NTSD28Q10Diagnostics/orasengan_jump_lfr_probe.cpp
authority: formal root NTSD2.8-Logan EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and paired playable GameSession28 BattleWorld28 GameSessionLfr28
evidence: docs/ai/TASKS/NTSD28-Q10-ORASENGAN-JUMP-NATIVE-LFR-001.md; artifacts/diagnostics/NTSD28-Q10-NARUTO-ORASENGAN-CUE-IDENTITY-20260929/REPORT.md
-->

# NTSD28-Q10-ORASENGAN-JUMP-NATIVE-LFR-001

> **2026-09-30 状态更新：`FOCUSED_TEST_PASS`，仅旧 B1E13 原生诊断子包。** 在系统 Temp 隔离重建旧发布源码树，实际158文件/4,239,978字节/树SHA `3695197B…D469488`；按旧脚本28个Core编译单元加旧Session/Selection/LFR和既有本包探针编译exit0、运行exit0。旧源码 tick34 action326/MP250/`data/078.wav`事件一次、55tick LFR与此前当前源码LFR逐SHA相同；逐SHA同旧权威的归档 EXE 对该 LFR 报告PASS，tick30～38动作/MP 9/9同。旧EXE trace不导出声音，Unity物理输入/clip/设备仍未验；当前根336B44是否更换权威未决。下方BLOCKED是隔离旧版入口建立前的历史事实，不代表本子包现状态；Q10/BATCH-05/总目标仍开放。[限定验收](../../../artifacts/diagnostics/NTSD28-Q10-ORASENGAN-JUMP-NATIVE-LFR-001/ACCEPTANCE-20260930.md)。

> **2026-09-30 追加验证，状态仍 BLOCKED：** 本轮未改脚本，旧 B1E13 逐哈希归档 EXE 对已有55tick LFR 无窗口回放报告 PASS，tick30～38 slot0 action/MP 对当前源码CSV 9/9 同，tick34 253→326、MP350→250。旧EXE trace不导出声音，故当前源码一次 `078.wav` 不代表旧版实际音频；当前根身份336B44且版本选择未决，不能提升整包/正式声源或Unity听感状态。原有“未运行根回放”是此前快照；本次从归档路径运行的是同SHA旧版，不覆盖根文件。[机器证据](../../../artifacts/diagnostics/NTSD28-Q10-ORASENGAN-JUMP-NATIVE-LFR-001/ACCEPTANCE-20260930.md)。

改前：正式 DAT 的 241～253 `hit_j:325` 与 326 `sound:data\078.wav`、正式/旧 Unity PCM 差异已静态测定。已有 Q07 正式 LFR 只在253持续帧按 Attack，验证 300/301 分支；Jump 后续分支尚无完整 Session 和正式根 LFR 事件证据。Unity 当前 `NTSDSoundPlayer` 读旧 Sound，不能据静态 DAT 直接宣称自然可听差异。

计划：仅添加 Task 中声明的单用途 Tools 诊断入口。当前源码预期行为和正式根身份为依据；用既有相同初态与前6 tick 输入，仅更换第一253后相位的 Jump 输入，记录相位、动作、MP和音频事件，生成非覆盖 LFR，再交正式根回放。预期副作用仅新增本包诊断源与输出；不触及生产、资源、Scene 或非战斗。不可回退边界是既有用户脏文件及正式目录；回滚只针对本 ID 新增内容，删除需按仓库规则批准。

验收和失败策略：见 Task；若未到326或无音频，保留原始失败并查输入相位，禁止伪造动作或把源码模型当正式根音频。脚本后补充实际文件/符号、编译/运行/验证、风险及后续 Unity 物理键出口；本 Record 不能先写 VERIFIED。

实际变更：只新增 `Tools/NTSD28Q10Diagnostics/orasengan_jump_lfr_probe.cpp`。原来没有该后续 Jump 自然输入诊断；现在以 `GameSession28::set_input`/`step`、`last_tick.audio_events` 和 `GameSessionLfr28` 记录 55 tick 与 LFR。没有修改正式源码、Unity生产、DAT/WAV、Scene或非战斗逻辑。首次错误资源根运行和首次错误要求 tick 末 action325 的失败均原样保留；修正诊断断言后新 `run-v2` 编译 exit0、源运行 exit0、tick34 phase0/action326/MP250/audio078一次。详[验收](../../../artifacts/diagnostics/NTSD28-Q10-ORASENGAN-JUMP-NATIVE-LFR-001/ACCEPTANCE-20260930.md)。

阻塞及风险：2026-09-30 当前根 EXE SHA 为 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`，与本 Task 的已确认正式权威 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033` 不符。故未运行根回放，不声称正式可达音频事件、Unity clip或听感已证。需用户确定版本并按[重盘文档](../NTSD28-BATTLE-ALIGNMENT-RESET-20260930.md) G0 核 closure 后继续。回滚边界仍是该新增工具及本包记录/工件，删除须依仓库保护规则授权；既有用户文件不动。
