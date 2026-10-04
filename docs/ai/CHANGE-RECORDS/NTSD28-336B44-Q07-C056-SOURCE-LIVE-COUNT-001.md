<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C056-SOURCE-LIVE-COUNT-001
status: VERIFIED
change-kind: CODE
code-path: Tools/NTSD28Q07Diagnostics/fusion_hold_counter_probe.cpp
authority: 336B44 formal root identity and corresponding playable GameSession28/BattleWorld28 full-tick fusion path
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C056-SOURCE-LIVE-COUNT-001.md
-->

# C056 正式源码活体 OID213 数诊断

脚本修改前记录：C056 原 Scene 已测得计数0/停顿3时两个 OID213 活体在 tick1/2/3 为 1/2/2，但正式源码报告仅给每 tick 出生数和 slot2 OID。不能以累计出生替代活体枚举。本包仅为现有 C++ 诊断在 CSV 行尾增加 World 公开实体接口枚举所得 OID213 活体数与活体总数，保持其它列及初态/输入/顺序不动；不修改正式源码、EXE、Unity 生产/Scene/DAT/非战斗。

预期副作用仅为新诊断 CSV 多两列与唯一输出目录。验收、失败边界及回滚见 [Task](../TASKS/NTSD28-336B44-Q07-C056-SOURCE-LIVE-COUNT-001.md)。正式根 EXE 的内部计数/停顿不可由现有 LFR 独立注入，故本结果不能晋升为根 EXE 同条件实测。当前状态 `PLANNED`。

2026-10-04 实施与验证：实际仅改上述 C++ 诊断 `run_case` 的每行写者和 CSV 表头，新增逐槽活体OID213与World活体总数两列。根EXE哈希复核为336B44…7BD3；从其对应正式源码当前28 Core及3 playable文件用C++17 `g++` 重链 exit0/0诊断。首次传发行包根作资源根导致初始化失败，129字节不完整CSV/失败日志原件保留；用正式 `resources/runtime` 重跑新A/B，各进程exit0、CSV同SHA `9DF886…BF47C`。原Scene c7/c0共六tick活体数及六次结构出生逐值一致；旧源码原始CSV不可得，故仅与已记报告字段配对，不声称旧文件逐字节回归。`git diff --check` 该C++文件exit0；Unity Editor本包未启动，正式根LFR受控入口及本Scene关闭阶段继续待。限定状态`VERIFIED`只适用于本活体数子门。[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C056-SOURCE-LIVE-COUNT-001/REPORT.md)。

交付门：`Tools/Validate-ChangeLedger.ps1` exit0/PASSED（1214 Records、7个当前代码差异文件均覆盖），输出保存在报告的 `20261004-01/change-ledger-validation.txt`；本包已跟踪代码/文档的 scoped `git diff --check` exit0。上述结果不改变正式根EXE和原Scene关闭轨迹的待验边界。
