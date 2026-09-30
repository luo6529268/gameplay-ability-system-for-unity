# Q07/F01 Guren CAG 正式根自然命中见证

状态：`FORMAL_ROOT_NATURAL_CONTACT_VERIFIED`，仅关闭本诊断子包；后续 Unity Play 子包已使 F01 字段门限定关闭，Q07 仍开放。

当前正式根 `NTSD2.8-Logan.exe` 的 SHA-256 为 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。用其对应 playable 源码闭包编译[有界探针](../../../Tools/NTSD28Q07Diagnostics/guren_cag_defense_lfr_probe.cpp)，读取未修改的正式 `resources/runtime`：Guren OID84 从 action150 的原有 OPoint 自然生成 OID619/action304；Lee OID7 从 action110/state7 防御，起点 X600；双方 Z650，20 个无输入逻辑 tick。探针输出[源逐 tick CSV](source-x600/source-ticks.csv)与[原生 LFR](source-x600/source-packets.lfr)。这是所选源码的诊断运行，不是正式根 EXE 自身。

源记录在 tick11 首次出现 CAG（slot50/action304/X531）；tick12 的 kind0 ITR `effect=1/bdefend=61` 命中 Lee，`defense_kind=0`，Lee HP 500→450、action110→186。没有修改 DAT 或构造额外碰撞规则。冻结的正式根 EXE 用同一 LFR、显式初始 action/MP、相同正式 runtime 运行 [headless 回放](source-x600/root-argv.txt)，进程 exit0，[报告](source-x600/root-report.json)为 `passed=true/failureCode=0/declaredTicks=20`。根 [trace](source-x600/root-trace.jsonl) 在 tick12 直接给出 slot50→slot1 `applied/hpDamage=50`。源 CSV 与根 trace 在声明的 tick1–20 比较 Guren action、Lee action/HP、CAG slot/action/X，**120/120 个字段相等，首差无**；机器可读[对照](source-x600/comparison.json)。回放包装器的额外 tick21 未纳入比较。

正式根 trace 的 hit 事件没有直接打印 ITR `bdefend` 或 defense decision；这些字段由同一正式内容的源探针观测，正式根直接证实的是自然命中后的 `applied/50 HP` 结果。报告自带 `nativeParityClaim=false`，所以该结果不等同于内建全状态 parity 证明，也不证明 Unity 原 Battle Scene 的自然 Play。F01 的 Unity 共用防御字段修复已有聚焦 2/2、相邻 18/18 和完整自检 PASS；下一出口是原 Battle Scene 中同内容的自然 CAG 接触及可观察结果，不能把本诊断子包的完成写成 Q07 完成。

收尾校验：`Tools/Validate-ChangeLedger.ps1` PASS（1050 records、共享 diff 中 10 个受管代码文件）；`git -c core.safecrlf=false diff --check` PASS。Battle/Menu Scene、GameConfig 与 ProjectBattleModeConfig 哈希保持本轮保护值；正式根 EXE 哈希在运行前重新确认。此子包未进入 Unity Play，未运行新的 Unity 测试。

后续状态索引：[原Battle Scene限定Play](../NTSD28-336B44-Q07-F01-GUREN-CAG-SCENE-PLAY-001/REPORT.md)已在20 tick六字段与本源CSV 120/120一致并退出Play。前文“下一出口”的表述是本根见证子包完成时的时序记录，不表示当前仍缺这一限定 Play。
