# Q07/C040 角都普通输入抓取与奇拉比护甲停顿同链

状态：`VERIFIED_SCOPED_FORMAL_SOURCE_ROOT_MIXED_INITIAL_ACTION`。C040、Q07、总目标开放。日期：2026-10-03。

权威：根正式 `NTSD2.8-Logan.exe` SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`，其对应 playable `GameSession28::step`、`SimulationTickDriver28::step`、护甲选取/减伤停顿、kind3关系命中与 CPOINT 结算，以及正式 OID25 角都、OID75 奇拉比、OID97 凯 DAT。诊断程序 `Tools/NTSD28Q07Diagnostics/c040_kakuzu_triad_reach_probe.cpp` 只链接正式源码，不是正式发行 EXE。

初态为 mode0/seed0、三角色 Y0/Z400/HP-MP500；角都 slot0/team1/action0/X500，经真实离散输入攻击 tick1～2、跳跃 tick3～4 到 AJ 抓取链；奇拉比 slot1/team2，凯 slot2/team1/action0。**奇拉比的初始动作仍由诊断配置指定**，没有注入持有关系、停顿、伤害或位置更新。矩阵为奇拉比初始 action69～73 × X540/560/580 × 凯相对 X+20/+40，共30例，另有 action70/X1200/凯1220的远距控制；每例完整 `GameSession28::step` 16 tick，其他角色离散输入中性。

| 代表样本 | 完整 tick 结果 | 判定 |
| --- | --- | --- |
| Bee action70/X540，Guy X560；另 Bee X560/Guy X580 | 角都 tick4 action320、tick6 action322；奇拉比 tick5 对凯的 selected-armor hit applied，凯 HP500→497，奇拉比 motion hold0→3；tick7 角都 kind3 applied，结算动作为336、CPOINT vaction132，奇拉比 hold2→1且被抓动作130，双向关系0↔1、active/synchronized各1。 | **两例 C040 阳性**：奇拉比当前 frame130 的中心减CPOINT为(-2,40)，vaction frame132为(-19,32)；正停顿使当前130保留，定位读口实际分源。 |
| Bee action71/X540，Guy X560 | 护甲命中在tick3，tick7抓取时hold已归零、奇拉比转132。 | 时点反例；不是同一正停顿分源。 |
| Bee action70/X580，Guy X600 | 护甲命中tick5，但角都tick8才抓，结算前hold已降至0。 | 站位延迟反例。 |
| Bee action70/X1200，Guy X1220 | tick5护甲命中，16tick无抓取。 | 远距反例。 |

`run1/`与`run2/`完整矩阵的 `source-ticks.csv`、`source-rng.csv`、`summary.csv` 和两份阳性 LFR 五件逐SHA相同：依次为 `2331E210A3A5ADBBD0EF94BE630E208DE2FC2F278494849655290784D47233FD`、`987AEDD7E7CDBC4951B8839B1BB42C73670EAB8744D99BB7F0EDAC7520B60374`、`883B13A76B80AB2C6CD24D6C5E3C3F0A07647FDC36A25400CC59B348F1F489F3`、`587FBD44E2551AE514D0E33EC442552500739F2B9A6F80F6D960E48E25B93FBF` 和 `465B7F9A683B9353144906F3BF7CBCB4889135D13F90C679E2BD0B97F92D119B`。编译使用当前 playable 闭包相同的 Core/playable 源清单，g++ exit0、输出空。

正式根 EXE 对两份阳性 LFR 都用当前正式资源、`--character 25 --enemy 75 --background 1`、相同 P1/P2 位置与 MP、`--lfr-slot1-action 70`、`--p2-human` 回放；进程均 exit0，报告 `passed=true`/failureCode0/declaredTicks16，trace 含第三角色凯。分别独立比对 tick1～16 的角都/奇拉比动作、奇拉比 hold、双向 catch slots、三者 X、奇拉比/凯 HP 和五个 RNG 字段，均 **16×15=240/240** 同态，firstDifference=null。原始 argv、report、trace、逐字段比较在 `root-positive-x540/` 与 `root-positive-x560/`。根报告的 `nativeParityClaim=false` 仍成立；这里仅证明列出的15字段和 source 结算证据，不声称所有隐藏字段同态。

边界：角都从 action0 走完整输入链是真实离散输入可达；**奇拉比从 action70 开始仍是受控合法初始动作**，尚未由普通动作经物理按键自然进入相同护甲命中时点。源 CPOINT 分源由正式帧字段和完整 tick 关系结算确认；正式根 trace 不单独导出 CPOINT 的每个内部读口。Unity 原 Battle Scene 的完整 Driver、D-024比例 X/Z、Game View 和玩家物理按键均未在本包运行，C040/Q07不能关闭。下一步可从奇拉比 action0 的正式攻击输入链把 tick5 的 action73/护甲命中带入同一三实体初态；若成立，再在原项目 Battle Scene 验证，保护当前 Menu 用户工作。

改动与审计：脚本前登记 Task/Change/Ledger/STATE/handoff；仅新增一个 Tools 诊断、独立原始证据和状态文档。未修改正式源码、Unity生产/test、DAT、图片、Scene、菜单或非战斗内容。Scoped Ledger、全工作树 Ledger 与目标 diff-check 的实际结果另记于同目录输出；不以源/根局部阳性代替 Unity 运行时验收。

最终检查：`ledger-scoped.txt` 与 `ledger-full.txt` 均 exit0/PASSED，并将本新增探针映射为对应 Change ID；目标文档 `git diff --check` exit0（输出在 `diff-check.txt`），新探针尾随空白0。完整检查以运行时工作树为准；后续并行修改须再验。
