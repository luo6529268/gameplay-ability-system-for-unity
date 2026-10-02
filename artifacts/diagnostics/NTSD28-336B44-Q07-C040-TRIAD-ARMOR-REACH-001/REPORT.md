# Q07/C040 三实体护甲停顿与抓取定位分源

状态：`VERIFIED_SCOPED_FORMAL_SOURCE_ROOT_CONTROLLED_ACTION`；C040、Q07、总目标仍开放。日期：2026-10-03。

权威身份：根 `NTSD2.8-Logan.exe` SHA-256 为 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。诊断按当前 playable 构建闭包链接 `GameSession28::step`，自身 EXE 不是正式发行版。正式解码 DAT SHA-256：`c/jira/jira.dat` `0399A48AAF06D292B4491477F56520D38219B9AAA8C12ADC98E4594EDB821B72`，`c/bee/bee.dat` `0E52DFBE044178D654C3E5D1694B9A733AC3D15AE1E8BBFE8E0E33169ACA7A4F`，`c/guy/gei.dat` `5CE66F8A53236C69DEA0D47D64FC60E1F2424DAF085B96B7A2355B8738ED190C`。

前置与触发：mode0、seed682973786、中性离散输入、源 Z400/Y0。Jiraiya OID21/slot0/team1/X500/action415，Bee OID75/slot1/team2/X620/action73，Guy OID97/slot2/team1/X640/action0，HP/MP500。只设角色、合法 DAT 动作和站位；没有注入抓取关系、命中、停顿、位置或速度。Jiraiya 415 的 kind3 指向 catcher action417/caughtact130；417 的 hurtable1/vaction132。Bee 动作130的中心减 CPOINT 偏移为(-2,40)，132 为(-19,32)。正式 tick 顺序是 type0 槽序命中在前、抓取关系结算在后；selected-armor reduced 命中可写攻击者正停顿且不走普通命中释放尾。

新版探针 `Tools/NTSD28Q07Diagnostics/c040_triad_armor_natural_probe.cpp` 用完整 GameSession 跑16 tick，保存 `source-ticks.csv`、`source-rng.csv` 和 `source-packets.lfr`。两轮 `g++` 编译均退出0、`compile-output*.txt` 空；v1 的 `c040_positive` 错把护甲命中与抓取限定为**同 tick**，因实际两者相隔一 tick 而误报阴性。`bee620-guy640/`、`bee620-guy670/` 初版原件保留。修订 v2 后以累计 `armor_hit_seen` 判定，不放宽正停顿、双向关系、settlement active/synchronized、动作/定位差异或 phase guard。

| v2 站位 | 源码完整 tick 结果 | 正式根 EXE 回放 |
| --- | --- | --- |
| Bee620 / Guy640 | tick1 Bee→Guy selected armor hit，Guy HP500→497，Bee hold0→3；tick2 Jiraiya kind3 applied，Bee hold3→2，动作130保留，Jiraiya结算动作为417/vaction132，定位偏移不同，双向关系0↔1且结算active/synchronized=1；`first_c040_positive=2` | exit0、`passed=true`、failure0；初始加16 tick 中选定9字段×16=144/144一致、首差无；根tick2 Bee action130/hold2/X641/Y-8/Z399，tick3仍action130/hold1；根事件tick1 hit slot1→2、tick2 relation hit slot0→1/action417/130。 |
| Bee620 / Guy670 | tick2抓取，但首次护甲命中在tick13；抓取时Bee hold0并切到动作132，C040分源0 | exit0/PASS，144/144选定字段一致。 |
| Bee1200 / Guy1220 | tick1护甲命中，但16 tick无抓取，C040分源0 | exit0/PASS，144/144选定字段一致。 |

另外 Bee1000/Guy1020 源码16tick先有护甲命中、tick12才抓，未得到分源；这只是额外有限对照。阳性案独立双跑的 CSV、RNG、LFR 三文件逐SHA相同：CSV `DD98B8D7B3E035E6DDB1F20590308C48F3B0B98515080328E93BF5D5646EC1FA`，RNG `971C163413300E303268D7442BC9781EA858FA7FC6FB0BC92E4AD19363875CC5`，LFR `8C599FFCAEEC4DE3E67EAC0685ACA8F0565675F8B383415A956FE2D91738AA8F`。三份根报告、trace、原始 argv、逐字段比较在 `root-positive/`、`root-no-early-armor/`、`root-far/`。根报告自身明确 `nativeParityClaim=false`；144格对照是本包额外比较，也只覆盖列出的9个实体后状态字段。

边界：初始 Jiraiya415/Bee73 是受控合法动作，不是从普通站立靠物理键自然选招；根 trace 无逐 pass CPOINT 读取字段，源码结算顺序及源/根动作、停顿、位置是分源的组合证据。尚未在原 Unity Battle Scene 用同三实体完整 Driver、D-024比例坐标及 Game View 复现，更未形成整场证书；因此 C040/Q07 不关闭。下一独立 Scene 包应比较 tick1～4 的动作/hold/双向关系/源坐标/视口投影，并保护用户当前未保存的 Menu Scene。无正式源码、Unity生产、DAT、图片、Scene 或非战斗修改。

审计：新 Tools 文件已由 `NTSD28-336B44-Q07-C040-TRIAD-ARMOR-REACH-001` 的 Task/Change 预登记；scoped `Tools/Validate-ChangeLedger.ps1 -StagedOnly -SimulateChangedPath Tools/NTSD28Q07Diagnostics/c040_triad_armor_natural_probe.cpp` 退出0并报告该路径 COVERED，原输出在 `ledger-scoped.txt`。文档 `git diff --check` 退出0，新 C++ 文件无尾随空白。**全工作树** `Tools/Validate-ChangeLedger.ps1` 退出1，明确报未登记的 `Assets/NTSD/Scripts/UI/SettingsPanelController.cs` 和 `UIButton.cs` 两份并行用户 UI 修改；完整输出在 `ledger-full.txt`。本包没有修改或代签这两份文件，因此不能声称整个工作树审计通过或可整体交付。
