<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C044-NATURAL-CROSSZERO-001
status: RUNTIME_PENDING
change-kind: CODE
code-path: Tools/NTSD28Q07Diagnostics/gaa_negative_decrease_lfr_probe.cpp
authority: selected 336B44 playable GameSession and BattleWorld negative-decrease catch pass with formal OID16 DAT
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C044-NATURAL-CROSSZERO-001.md
-->

# C044 正式内容负 decrease 自然可达性诊断

脚本修改前创建。原状：正式 OID16 有多个 kind-3 抓取入口与负 `decrease:-3` 终端帧；正式源码跨零先写 pending impulse 后由物理消费，Unity 共用 `BattleCpointWriter.RunKind1` 当前同处立即写速度，但尚无运行跨零首差。当前任务只允许新增声明的一份独立 C++ 探针和诊断产物，先判实际自然抓取/跨零，避免按静态字段直接改生产。

预期副作用限手动运行时生成 EXE/CSV/LFR；不进入正式构建，不改玩法。验收与失败、根回放门、保护范围和精确回滚见 Task。状态 `PLANNED`，无编译或运行证据。Q07/C044/总目标开放。

实际只新增声明的C++诊断源；当前源码g++编译退出0。正式OID16四近距完整Driver均tick1抓取/tick31负帧timeout38→35、投掷1/释放0；四远距无抓取，八次运行退出0。未出现跨零，故没有根LFR或Unity跨零首差证书；根LFR亦不携带手设timeout。受控低timeout须独立Task/Change，生产/DAT/Scene/非战斗未改。状态`RUNTIME_PENDING`，证据[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C044-NATURAL-CROSSZERO-001/REPORT.md)。
