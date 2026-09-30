# Q08/C008 正式根结果退出时点见证

状态：`FORMAL_ROOT_HOST_BOUNDARY_VERIFIED_SCOPED / LFR_TERMINAL_ABORT_46`。本包补齐 C008 的根 EXE 可观察退出时点；Q08 整组、上层页面与整场 parity 未关闭。

当前根 `NTSD2.8-Logan.exe` SHA-256 重新核对为 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。只新增仓库原生诊断[探针](../../../Tools/NTSD28Q07Diagnostics/result_continue_host_lfr_probe.cpp)，按所选 playable 源码闭包编译，未改正式 EXE、源文件或 DAT。正式 OID2 Naruto 位于 X500/Z650，对手 OID2 X525/Z650、HP10；第2轮的一次普通 Attack 在第8轮经原有 authored ITR 造成20伤害和KO。根 trace 直接记录 slot0→1 applied/20伤害/KO，结果计时从下一轮开始。

最终持续按住组采用 [held-v3 源LFR](held-v3/source-packets.lfr)及[源逐host CSV](held-v3/source-host-rows.csv)。源 requested 和 sampled Attack 在host150–152均为1，根trace `inputCurrentMask` 连续为16，确认按键在计时门槛前已按住。正式根直接输出：

| Host行 | World tick | 根计时 | 根转换 | 观察 |
|---|---:|---:|---:|---|
| 150 | 150 | 142 | 0 | 已按住Attack，未退出 |
| 151 | 151 | 143 | 0 | 持续按住，未退出 |
| 152 | 152 | 350 | 0 | 满足继续门槛，写350；本轮仍推进World |
| 153 | 152 | 350，timerAfter0 | 2 | 下一host行开始转换，World tick不增加 |

对源/根host1–153比较12字段（World tick、采样Attack、phase、timer、timerAfter、转换、转换开始、upper事件、winner、双方动作、目标HP），**1836/1836一致、无首差**；[机器对照](held-v3/comparison.json)。冻结边界的RNG保持；两实体所有打印字段只发生slot0 `inputPendingMask:16→0`，符合正式 `GameSession28` 转换路径清pending。没有把清pending误判为战斗继续推进。

无继续输入的[natural对照](natural/comparison.json)在host357计时349/转换0，host358自然达到350并在同轮转换2，World保持357。该组host1–358的11字段 **3938/3938一致、无首差**，冻结前后所有打印实体字段与RNG完全相同。它确认自然计时350和输入写350的退出轮不同。

两份正式根回放报告均真实保留为 `passed=false / process exit46 / failureCode46`：[持续按住报告](held-v3/root-report.json)、[自然计时报告](natural/root-report.json)。原因是正式 `GameSessionLfrPlayback28::step` 要求每个输入行包括终步都递增World tick；C008转换正确地冻结World，回放器于是报 `did not advance exactly one tick`。`main.cpp` 在这个检查返回后仍先写出当前trace行，因此上述退出边界由根EXE直接观测。不能把这些报告改称回放PASS，也不能以此制造新的战斗修复。报告内 `nativeParityClaim=false`；根载体在此终止，源码额外的上层2→1行不冒充根证据。

保留的[初始held](held/comparison.json)与[held-v2](held-v2/comparison.json)都是输入时点控制，未被当成最终持续按住证据。v2在非采样相位开始请求Attack，正式录制器保存的是采样后current而非raw request，门槛前那行仍录neutral；v3从前一个phase0开始，新增sampled字段后补齐条件。三个源码版本快照与构建输出均保留在本目录。

C008 Unity已有原Editor RED→GREEN 2/2、相邻13/13和同一自然双轮KO/held结果Play1/1，详[原修复报告](../NTSD28-336B44-Q08-C008-DEFERRED-RESULT-EXIT-001/REPORT.md)。本轮只补正式根证据，没有更改Unity生产代码或重跑Unity测试；结合这些证据，C008的“输入350后下一host才退出”可记为限定已验证。主菜单、选人、结果页设置/重赛UI继续排除。

收尾校验：Change Ledger PASS（1052 records、共享diff中12个受管代码文件）；`git -c core.safecrlf=false diff --check` PASS。根EXE及Battle/Menu Scene、GameConfig、ProjectBattleModeConfig保护SHA保持；正式与Unity暂存Naruto DAT同字节SHA，当前探针与实际编译v3快照SHA相等，见[文件身份清单](artifact-hashes.json)。下一G1为Q07/C017，再C011/C012；Q07/Q08整组和总目标保持开放。
