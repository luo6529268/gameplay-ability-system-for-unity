# Q07/C040 三人普通输入的正式可达性

状态：`VERIFIED_SCOPED_FORMAL_SOURCE_ROOT`。2026-10-03。当前战斗规则权威为根目录 SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 的正式 EXE 及对应 playable live path。本包只增加独立 C++ 诊断，不修改正式源码、Unity 生产或测试、DAT、图片、Scene 与非战斗逻辑。

## 输入与边界

用当前正式 `GameSession28::step` 完整推进。角都 OID25、奇拉比 OID75、凯 OID97 均从 action0 开始，mode0/seed0、Y0/Z400、HP/MP500。奇拉比使用三个攻键脉冲，角都延迟攻键两 tick 后跳键；近距奇拉比/凯 X540/560、X560/580，另有角都远距反例。无初始化之后的动作、关系、停顿、伤害或位置注入。八组奇拉比补按时点 × 七组角都起键 × 两种近距几何，加一远距控制，共 113 案，每案 40 完整 tick。

## 结果

- 正式源码诊断用 `compile-argv.txt` 中命令编译，退出 0、无诊断。两次独立矩阵运行各 `cases=113 positive_cases=8`，四件原始文件 `source-ticks.csv`、`source-rng.csv`、`summary.csv`、`first-positive.lfr` 逐件 SHA-256 相同，见 `determinism-hashes.json`。
- 八个阳性均为角都攻键起点 tick19，奇拉比第二/第三次攻键起点分别为 (9,17)、(9,18)、(10,17)、(10,18)，两种近距几何各四例。X540/560 在 tick25、X560/580 在 tick26 达到正停顿分源。其余 105 案没有满足完整阳性条件；包括早抓取时奇拉比 hold0/action132 的近距反例，以及角都 X500、另两人 X1200/1220 时无 kind3 抓取的远距反例。这些反例只约束本矩阵。
- 首个阳性 `b9-17-k19-x540-g560`：奇拉比 tick21 进 action70、tick24 进73；角都 tick22 进320、tick24 进322。tick25 奇拉比对凯 `selected_armor` 命中、角都 kind3 抓取同轮实际 `applied`；结算角都 action336/vaction132，奇拉比 action130/hold3，双向抓取关系均在，`active_relations=1`、`synchronized_targets=1`，当前受击者帧与 vaction 的中心减 CPOINT 定位有差异。奇拉比没有从受控 action70 开始。
- 首阳 LFR 经根目录正式 EXE 独立回放，进程 exit0，`root-report.json` 为 `passed=true/failureCode=0`，40 个声明 tick 完成。将正式源码与根 trace 的输入相位、三者动作/位置、奇拉比停顿/关系/HP、凯 HP 及五个 RNG 字段逐 tick 对照，共 40×16=640 值，首差为零；见 `paired-comparison.json`。根报告明确 `nativeParityClaim=false`，故此处的字段同态是本包自行逐项比对，不能把报告 PASS 单独当成全部规则同态。

## 未关闭出口

本包证明正式规则中三名普通初态、离散输入可以自然抵达该 C040 正停顿分源；尚未证明原 Unity Battle Scene 的物理键、生产完整 Driver、D-024 比例坐标及 Game View 与同一链一致。C040、Q07、Q09、Q12 和总目标仍开放。下一证据门是在原项目 Editor 安全空闲、保护已有 Menu/Scene 工作的前提下，仅对这条明确的输入链做原 Battle Scene 逐 tick 与比例/画面定向验收。

审计：`Tools/Validate-ChangeLedger.ps1 -StagedOnly -SimulateChangedPath Tools/NTSD28Q07Diagnostics/c040_all_natural_triad_probe.cpp` 退出0，覆盖本诊断；完整 `Tools/Validate-ChangeLedger.ps1` 退出0，当前四个未跟踪诊断脚本均有记录。`git diff --check` 对本轮四份已跟踪文档退出0。validator 输出包含大量历史 Record 指向当前 diff 之外文件的 WARNING，不是本包失败。未运行原 Unity Editor 编译、SelfCheck、Play 或设备验证；本包没有 Unity 代码改动。
