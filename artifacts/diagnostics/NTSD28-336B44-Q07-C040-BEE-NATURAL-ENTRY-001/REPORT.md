# Q07/C040 奇拉比普通动作攻击补按到动作 73

状态：`VERIFIED_SCOPED_FORMAL_DISCRETE_INPUT_ENTRY`。C040、Q07、总目标仍开放。日期：2026-10-03。

权威：正式根 `NTSD2.8-Logan.exe` SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`，对应 playable `GameSession28::step` / `input_routing.cpp` 和正式 OID75 `c/bee/bee.dat`。正式帧63的 `hit_a:66`、帧69的 `hit_a:70` 与70→73的 next 链是输入日程候选，实际可达性由完整 tick 验证。新增 `c040_bee_natural_attack_probe.cpp` 为诊断程序，不是正式 EXE。

初态：mode0/seed0、奇拉比 OID75/action0/slot0/X500/Y0/Z400/HP-MP500/team1，远距凯 OID97/action0/slot1/X1200/Y0/Z400/team2，中性 AI 输入。奇拉比攻击键 tick1～2，第二次两 tick 按键起点7～14，第三次两 tick 按键起点16～30，形成120组；另有只按第一次的1组控制。各组跑40个完整 `GameSession28::step`，未覆盖 DAT、设置动作或注入关系/命中/停顿。目标远距，故本包只测选招，不测护甲命中。

| 样本 | 动作链/结果 | 说明 |
| --- | --- | --- |
| 第二次攻击起点7、第三次起点16 | tick2 action65，tick8 frame63，tick9 action66，tick14 frame69，tick16 action70，tick19 action73。 | 最早一组阳性，LFR 已回放。 |
| 第二次起点8/9/10且第三次对应16～18 | 另7组也在40tick进入70/73，部分到73为tick21。 | 本矩阵共8/121阳性。 |
| 只按第一次攻击 | 到63为止，未到66/69/70/73。 | 缺补按控制。 |
| 第二次起点11、第三次16 | action66迟至tick19、frame69为tick24，40tick未到70/73。 | 有限时序反例；不能推断其它时间不可达。 |

121组中82组进入action66，8组进入action70，8组进入action73。`run1/`与`run2/`的完整tick、RNG、summary、代表LFR四件双跑逐SHA相同，分别是 `066EDC9CBCB84EBE4D28EEF998046BAFB1F4512E3AEF1D133129DAD03BDACF20`、`D81D7AAFC0E83C06AD1A4626229559EF685BC5B40D801892D88418061B881B87`、`D600EA57D3FDFB13915895D1F1787D1755967294DEE6AC6D40039BF569A715EF`、`99F09B3FA74BAFC52379C54BB1689A76EC52250F9898257D825C7EE8C57E75AB`。g++ 使用当前 playable 闭包相关源编译退出0，输出空。

代表LFR由正式根EXE以同正式资源、OID75/OID97、X500/X1200、MP500和`--p2-human`回放，进程exit0、`passed=true`、failureCode0、declaredTicks40。独立选取 tick1～40 的输入相位、动作、攻击边沿、comboAJ、X/HP、两套RNG共12字段逐tick比较，**40×12=480/480** 一致，firstDifference=null。原始 argv/report/trace/比较在 `root-first-action73/`；根报告标注 `nativeParityClaim=false`，不能据此宣称全部内部状态等价。

边界和下一动作：奇拉比从普通动作0到73的**离散输入**正式可达已证，尚未在与角都和凯同场的近距离条件下保持完全相同连段；早期拳可能命中并改变停顿/帧推进。当前自然链到73为tick19，按旧混合初态结果预计护甲命中在随后tick，但必须实测。角都抓取的按键需要延后到护甲命中之后且正停顿仍在；不能简单平移旧tick7结论。原Unity Battle Scene物理键、D-024比例坐标、Game View、C040/Q07仍待。

改动与审计：脚本前 Task/Change/Ledger/STATE/handoff 已登记；只新增 `Tools/NTSD28Q07Diagnostics/c040_bee_natural_attack_probe.cpp` 和诊断原件、状态文档，没有修改正式源码、Unity生产/test、DAT、图片、Scene、菜单或非战斗内容。Scoped/full Ledger 与目标 diff-check 的执行结果见同目录输出；本包不替代原场景验收。

最终检查：`ledger-scoped.txt`、`ledger-full.txt` 均 exit0/PASSED，且新探针被对应 Change ID 覆盖；目标文档 `git diff --check` exit0（见 `diff-check.txt`），新探针尾随空白0。检查只覆盖执行时的工作树状态，后续修改须重新验。
