<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C053-TOBI-PHYSICAL-REACH-001
status: RUNTIME_PENDING
change-kind: CODE
code-path: Tools/NTSD28Q07Diagnostics/c053_tobi_physical_reach_probe.cpp
authority: selected 336B44 playable GameSession28 and formal Tobi jump hit_Fa 510 to frame512 OPoint OID251
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C053-TOBI-PHYSICAL-REACH-001.md
-->

# Q07/C053 Tobi普通输入候选有限探针

脚本前登记。当前Unity对应生产规则不改；正式DAT静态跳跃`hit_Fa:510`和上一个受控初态四组阴性见Task。仅新增一个Tools离线探针，用当前正式playable完整GameSession测两种明确初态、每案最多40tick，记录输入消费/动作及OID251出生。阳性才写LFR并回放当前正式根；阴性停止。未知受控action212能否等价普通跳跃，不能直接把它当玩家选招证书。输出拒绝覆盖，正式源/EXE/DAT/Unity生产/Scene/非战斗不动。验收为工具当前闭包编译与有界原件，阳性时再加根LFR对应字段；原Unity Play另待。回滚仅针对新文件及文档，删除另行审计授权。

2026-10-04 初轮：新工具以当前源码闭包编译exit0；两例各40tick均无510/512/OID251。普通action0案于tick8送同时防+右+攻，tick9转83；受控212案tick1送同组、tick2转83。`run-a/summary.csv`和逐tick原件保留，尚不能判字段为何被覆盖。为确定输入首差，在同一已声明诊断路径追加`combo_state[0]`和首个input action来源/请求/结果字段；新exe、新输出目录，禁止覆盖初轮。此增量仍只读同两例，不增加搜索矩阵。

2026-10-04 补充轮：`c053-tobi-physical-reach-v2.exe`按当前闭包编译exit0、无诊断，`run-b`两例各40tick仍无510/512/OID251。普通案tick8抽样mask88/phase0、combo_fa4、action_count0；tick9记录普通空中攻击请求80、tick尾83。受控212案tick2同样combo_fa4，普通空中攻击请求80、tick尾83。实际代码仅新增`Tools/NTSD28Q07Diagnostics/c053_tobi_physical_reach_probe.cpp`并扩同路径只读字段；报告`artifacts/diagnostics/NTSD28-336B44-Q07-C053-TOBI-PHYSICAL-REACH-001/REPORT.md`。状态`RUNTIME_PENDING`，因为字段消费者首差未闭且没有阳性LFR/正式根回放或Unity原Scene。未改DAT、Unity生产、Scene、Prefab或非战斗；未执行删除、移动、覆盖已有原件。回滚仍限新工具及文档，若要删除另行审计授权。

2026-10-04 第三轮脚本修改前增量声明：既有两输入例不变，仅在相同Tools文件记录每tick调用前Tobi action、其正式加载definition当前帧`hit_Fa`值，以及definition frame212的`hit_Fa`值。目的只分清正式内容读取与输入消费时序；仍写新exe和新run目录、不覆盖旧原件。若读值非510，先定位catalog/parser；若为510而无尝试，继续审输入前写者。此声明不授权改正式源码、DAT、Unity生产或增加无界输入矩阵。

2026-10-04 第三轮实测及第四轮脚本修改前纠正：`run-c`两例均读到当前角色OID53加载frame212的`hit_Fa=-1`。重新逐行核正式`decoded_dat/data/data.txt`：OID0是`c/tobi/tobi.dat`，OID53是`c/tobi/ttobi.dat`，此前只读审计误将两者合并。前者frame212有`hit_Fa:510`且frame512有OID251 OPoint；后者frame212无该字段、无510/512和该OPoint。故原OID53阴性是错误角色前置，不是输入消费或parser首差。现在仍在同一唯一工具路径将测试actor/config OID53准确改为OID0，原两输入/seed/mode/坐标/tick不变；编译为v4新exe并写run-d新目录，保留前三轮原件。正式DAT/源码、Unity生产/Scene均不改。任何后继结果须注明身份纠正。

2026-10-04 正确身份结果：唯一工具`Tools/NTSD28Q07Diagnostics/c053_tobi_physical_reach_probe.cpp`已将测试actor/config OID改0并在初始化断言正式加载frame212 `hit_Fa=510`；v4当前闭包编译exit0/无诊断。`run-d`普通action0跳跃案tick8进入510/tick11自然生成OID251/action51；受控212案tick2进510/tick5出生。两案各40tick LFR由正式336B44根EXE独立回放均`passed=true/failureCode=0`，选择的实体动作/Y、抽样mask/phase及子体槽/动作/X/Y共640/640零差，报告与原件见`artifacts/diagnostics/NTSD28-336B44-Q07-C053-TOBI-PHYSICAL-REACH-001/REPORT.md`。两位置下子体从3转60等地面帧、未到action0；原Unity Scene、自然第三体action0双Uj及全World仍待，故状态维持`RUNTIME_PENDING`。没有修改DAT、正式源码/EXE、Unity生产/Scene/非战斗，也未删除或覆盖旧原件。

2026-10-04 v5脚本修改前补证范围：Task原要求逐tick记录production Spawn事件，v4仅记录子体首次实体可见。只在同一Tools工具追加`step->spawns.events`中OID251的事件计数、状态、slot/source_line到CSV；两例及模拟参数不变，写新exe/run-e目录且拒绝覆盖。比较run-d/run-e的LFR SHA，若完全相同复用已成功的根回放；若不同则重新回放且查首差。此补证不改正式/Unity生产或DAT。

2026-10-04 v5实际修改与验收：实际代码路径仍仅`Tools/NTSD28Q07Diagnostics/c053_tobi_physical_reach_probe.cpp`。在40tick循环的`step->spawns.events`中仅读取OID251事件，新增CSV计数/首事件status/slot/source_line；不改变会话配置、输入、模拟或LFR。当前336B44 playable 33源文件编译exit0/无诊断，新`c053-tobi-physical-reach-v5.exe`写`run-e`。普通案tick11及受控案tick5各一条status0(`spawned`)、slot50、source_line1917；同tick子体首次可见。run-d/run-e两条LFR SHA完全一致（普通`C36C6326DEAEE9DE10B6E80BEBCF2FEFF6763CF540E31DA826DDCBFA9C9FBDE3`，受控`F009A8C03A73D8A0DFF05CDFB6160CBEF3001E036FF39BD686E80CB84F56F82D`），因此复用先前相同字节正式根回放，不声称本次重新运行EXE。回滚范围仍仅此新Tools文件和文档；无删除/移动/覆盖旧原件。原Unity Scene、自然action0双Uj及全World未验，状态保持`RUNTIME_PENDING`。

2026-10-04 v6脚本修改前增量：正式`firz.dat`飞行帧0～3均有`hit_Fa:7`；当前336B44 playable `NativeObjectHitFa`行为7在整数Y>-25时写action60，现有两案OID251于action3到该阈值，故静态`3 next:0`尚未实际到达。只在同一Tools文件的Profile列表新增受控OID0/action212/Y-130第三案，保留原两案与参数，40tick上限不变。预期若更高出生则可能先到action0；仅以实测裁决。新v6 exe/run-f，不覆盖已有输出；阳性新增根正式EXE LFR回放与选定字段比较。无Unity生产、DAT、正式源码、Scene、非战斗改动。验收为当前闭包编译、三CSV及两旧LFR稳定；第三案如阳性有根报告和明确未覆盖的Unity Scene门槛。回滚只涉及诊断Tools与记录；删除仍须另行审计授权。

2026-10-04 v6实际改动/纠正：唯一Tools文件`c053_tobi_physical_reach_probe.cpp`仅在Profile列表追加受控OID0/action212/Y-130；原两案/40tick/输入/seed/mode未变。v6当前336B44 playable闭包编译exit0/无诊断，输出独立新`run-f`。两旧案LFR与run-e逐SHA相同。第三案OID251于tick5 `spawned`，tick13～21保持action3，tick22切60，40tick未进action0；正式根EXE同LFR `passed=true/failureCode=0`，动作/位置/输入相位六字段×40tick 240/240零差。随后按当前正式`frame_machine.cpp`复核，`next:0`分支状态是`stayed`；v6前“高度足够或许从3跳0”的脚本前推断被证伪，绝不改DAT迎合推断。Unity代码静态已有`rawNext==0`保留帧和非角色`hit_Fa:7`阈值分支，但原Scene新程序集/运行时未验。回滚范围仅此Tools与记录，无删除/覆盖已有原件；状态仍`RUNTIME_PENDING`。

2026-10-04 v6收口检查：`Tools/Validate-ChangeLedger.ps1`返回exit0，日志`artifacts/diagnostics/NTSD28-336B44-Q07-C053-TOBI-PHYSICAL-REACH-001/ledger-v6-validation.log`首行`Change ledger validation PASSED`（Records1231、当前diff governed32）。`git diff --check`所检总表/STATE/handoff/Ledger无格式错误；正式根EXE重新SHA确认为336B44，Battle/Menu场景磁盘SHA保持`D88AD211...CDF6`/`9EAAA0B4...C1BA`。原Editor CLI检测到同项目PID105896运行但无Pipeline连接，新C053探针晚于Editor程序集文件，故未启动原Scene Play、未提升状态。
