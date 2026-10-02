# Q07/C040 角都普通动作到 AJ 抓取入口

状态：`VERIFIED_SCOPED_FORMAL_DISCRETE_INPUT_ENTRY`。C040、Q07、总目标仍开放。日期：2026-10-03。

权威：根目录正式 `NTSD2.8-Logan.exe` SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`，及其对应 playable `GameSession28::step` / `input_routing.cpp` 和正式 `data/data.txt`、`c/kaku/kakun.dat`。新增的 `c040_kakuzu_physical_combo_probe.cpp` 只链接正式源码作诊断，本身不是正式发行 EXE。正式目录把角都映射为 OID25；其 frame60 与 frame65 都声明 `hit_aj:320`，frame322 有 kind3 `catchingact:336`/`caughtact:130`，frame336 的 CPOINT 选择 `vaction:132`。

初态：mode0、source seed0（与本次正式根回放初态配对）、角都 OID25/action0/slot0/X500/Y0/Z400/HP-MP500/team1，远距奇拉比 OID75/action0/slot1/X1200/Y0/Z400/HP-MP500/team2；没有注入特殊动作、关系、命中或停顿。13 组仅改变跳键开始 tick=3～15：攻击键在 tick1～2 保持，跳键在所选起点及下一 tick 保持，其余中性；每组完整 `GameSession28::step` 32 tick。远距目标仅提供合法第二方，本包不试抓取命中。

| 跳键起点 | 正式源码完整 tick | 结论 |
| --- | --- | --- |
| 3、4 | tick2 角都进入 action65；tick4 进入 action320；tick6 进入有 kind3 抓取框的 action322 | 两个输入日程在当前宿主采样相位下等价，LFR SHA 相同；均未对远距目标建立关系。 |
| 5～15 | 32 tick 内未进入 320/322 | 仅排除所测 11 个日程，不推断其它键序或距离不可达。 |

正式根 `--headless-playback-lfr` 对起点3的 v2 LFR 使用相同双方位置、MP、正式内容及 `--p2-human`，进程 exit0；报告 `passed=true`、failureCode0、declaredTicks32、completedTicks33（含初始行），并明确 `nativeParityClaim=false`。独立逐字段比较 tick1～32 的 input phase、双方动作相关的角都 action、双方 X/HP 和两套 RNG 状态共 **32×11=352/352** 一致、firstDifference=null。这里只证明这 11 个选定字段的 source/root 同态；根报告的 PASS 本身不是全部内部状态等价。

初版源码 seed682973786 在角都 action320/322 的结果上与根一致，但 CRT state 从 tick1 起 32 行不同；源/根启动种子不一致，不能称同 seed。初版 `run1/`、`run2/`、`root-jump3/` 和逐字段首差文件保留。Change Record 中登记后仅将诊断 seed 改为根默认0，重编为 `c040-kakuzu-input-v2.exe`，`v2-run1/`、`v2-run2/` 的 source-ticks、summary、jump3/4 LFR 四文件各自双跑 SHA 一致：CSV `E544DB88A7CA740AFB6B6A8AEBEF711837C01BF3EF762BAB3F2E05FEF79566D0`，summary `0B62417B60D39F132E5432D0D19F1043A8FE602E6CC1AE75E063E025E8708D7B`，两份阳性 LFR `50805EF57434BACFF50C6A0F9A052C9EAE544081E824003EB6F616A198792D62`。v2 根原始 argv、report、trace 和选定字段比较在 `root-jump3-v2/`。

初版探针在 tick2 实际进入的是 frame65，而不是静态候选中先看到的 frame60；两个帧都有 `hit_aj:320`。初版编译与 v2 编译均 g++ exit0、编译输出空。第一次根命令的 PowerShell 目录创建参数错误、无有效回放；第二次直接启动 GUI EXE 有报告但没有可靠进程退出码；最终以 `ProcessStartInfo` 隐藏窗口、等待并读取进程退出码取得上述 exit0。失败尝试不算通过证据。

边界和下一动作：这是真实**离散战斗输入**通过正式完整 `GameSession` 从动作0进入抓取帧，并非玩家在 Unity 原 Battle Scene 按物理键的证据。当前第二方在远距，抓取关系为无；尚未证明角都的抓取与奇拉比护甲命中能在同一链形成 C040 正停顿分源，更未验 Unity D-024 比例坐标、Game View 或整场。因此不关闭 C040/Q07。下一独立包应先用正式三实体完整 tick 调整奇拉比攻击与第三方护甲命中的时间及站位，阳性后正式根 LFR，再送原 Unity Battle Scene。

审计：Task、Change Record、Ledger、STATE、handoff 在新增脚本前登记；代码只新增 `Tools/NTSD28Q07Diagnostics/c040_kakuzu_physical_combo_probe.cpp` 并在同一 Record 下修正诊断种子，没有修改正式源码、Unity生产、DAT、图片、Scene、菜单或非战斗内容。最终 scoped Ledger 和 diff-check 结果见同目录的检查输出；全工作树存在并行用户 UI 改动，其独立审计不能由本包代签。

最终审计更正：上述“全工作树存在并行用户 UI 改动”是运行前快照；检查时该两份 UI 脚本已不在当前 Git diff。`ledger-scoped.txt` 对新探针报告 COVERED/exit0；`ledger-full.txt` 报 PASSED、1174 Records、当前 diff 内治理代码1文件且本探针 COVERED、exit0。目标文档 `git diff --check` exit0，新探针尾随空白0。验证仅针对执行时的工作树状态，不替代后续变更的再检查。
