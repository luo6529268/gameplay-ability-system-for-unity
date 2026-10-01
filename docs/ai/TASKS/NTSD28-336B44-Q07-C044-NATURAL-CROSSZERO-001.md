# NTSD28-336B44-Q07-C044-NATURAL-CROSSZERO-001

状态：`RUNTIME_PENDING / BOUNDED_NATURAL_CROSS_ZERO_NEGATIVE`。父目标 `NTSD28-UNITY-BATTLE-REALIGNMENT-001`，BATCH-04/Q07/C044/R12。当前规则权威为正式 EXE SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 及对应 playable `GameSession28::step()`、`SimulationTickDriver28::step()`、`BattleWorld28::advance_catch_relations()`；内容使用正式 `resources/runtime` OID16 `gaa.dat`，不改 DAT。

本包只新增 `Tools/NTSD28Q07Diagnostics/gaa_negative_decrease_lfr_probe.cpp` 和独立 `artifacts/diagnostics/NTSD28-336B44-Q07-C044-NATURAL-CROSSZERO-001/`。已只读确认初始 action245/363/364/365 的 kind-3 抓取分别通向120/400/420/370，终端 action128/407/427/377 含 `decrease:-3`。用 OID16 对 OID2 正常角色、相同种子与中性输入，近远位置分别运行完整 Driver；逐 tick 记录双方动作、关系、抓取剩余时间、帧计数、位置/速度、`catch_relations.released_relations`/`thrown_relations`、命中和 RNG，另存 LFR。不得手设 catch slot、timeout、动作或速度来冒充自然入口；无抓取、只到终端帧但未跨零、自然跨零须分开判定。先跑最窄正反位置，不做无边界全角色矩阵。

仅当正式源码自然跨零，才在同初态 LFR 用选定根 EXE 验证并另立 Unity 原 Scene 子任务。若自然不跨零，以受控低 timeout 的独立源/Unity机制证据查逐 pass 首差；受控初态不能自动晋升为根/场景自然对齐。任何生产修复前必须先建立准确新 Task/Change，并坚持只改共用战斗 writer，不改非战斗、Scene、DAT、项目自有地图或 Unity/GAS 架构。验证记录编译、正反、根身份、首差、保护哈希和账本；回滚只审阅本包新增诊断文件，不动已有脏工作。不使用 computer-use 或第二 Unity 项目。

执行结果：正式OID16初始245/363/364/365四近距完整Driver均tick1抓取、tick31负decrease帧timeout38→35，关系pass走投掷而非跨零释放；四远距均无抓取。g++编译0、八次源码运行退出0，LFR/CSV留存；受控低timeout的逐pass机制和Unity首差另包，不把自然阴性写成C044已对齐。详[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C044-NATURAL-CROSSZERO-001/REPORT.md)。
