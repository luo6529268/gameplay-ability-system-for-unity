# Q07/F02 正式 kind10 复位后的高速武器再次进入动作40

状态：`VERIFIED_SCOPED_SOURCE_ROOT_F02_POSITIVE`。本包证明正式源码与336B44根EXE在同一受控完整战斗输入中，自然发生武器拾取、投出、kind10命中复位以及F02高速动作40选择。**没有运行原Unity Battle Scene的同条件链**；F02的正式侧触发门已找到，Unity侧完整tick、碰撞比例域及画面验收仍待，Q07和总目标开放。

权威：根目录`NTSD2.8-Logan.exe`本轮SHA-256为`336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。源码使用其对应playable构建闭包的`GameSession28::step`、`SimulationTickDriver28::step`、`BattleWorld28`命中消费及`PhysicsIntegrator28::step`；正式内容为`c/nar/nar.dat`、`c/tay/tay.dat`和`w/6.dat`。诊断程序只是同源码完整GameSession测试载体，不能替代正式根EXE身份。

初态：mode0、seed`0x28A55A5A`；鸣人slot0/OID2/X200/Z542/team1，多由也slot1/OID36/X530/Z542/team2/初始action243，武器slot2/OID600/type4/X190/Y-20/Z542/team1/初始action0。三者经`GameSession28::initialize`创建。后续只向鸣人提交离散Attack输入；不注入武器速度、关系、动作或命中。正式根回放显式采用载体支持的`--lfr-slot1-action 243`，初始根trace已核为243。背景ID23仅用于正式原生会话配置，不要求部署到Unity。

| tick | 正式完整tick观测 |
| --- | --- |
| 17～18 | OID600自然落地动作64；鸣人tick18攻击拾取，关系4/持有武器state1001。 |
| 24、30 | 第二次攻击选轻投动作45；tick30持有结算按鸣人WPOINT释放武器，OID600为action41/state1002/Vx55。 |
| 31 | 多由也slot1→武器slot2有`kind10/status=applied`；命中后武器action1/state1000/Vx51.401869。tick31的物理在命中前，所以不能把同tick的state1000误当物理入口。 |
| 32～35 | kind10继续实际命中；武器保持state1000，X速度逐次降至34.251236。tick35帧持有计时3，随后36～37为2/1。 |
| 38～39 | tick38仍action1/state1000/Vx34.251236，hold归0；tick39物理满足type4、state1000和阻尼后严格`|Vx|>9`，选择action40；正式根帧事件同tick明确为slot2 `40→41`，tick尾action41/state1002/X584/Y-64。tick39没有kind10命中，不能把结果归于再次命中。 |

`kind10-x530`的`source-ticks.csv`、`opponent-ticks.csv`及`relation-hits.csv`记录128个完整tick与命中状态，`summary.txt`给出拾取18、投掷24、释放30、首次高速state1000为31、F02返回39。此前`near-regression`仍与旧`near-03`逐字节相同：源CSV SHA-256 `FABA40624A7F41DAF9563CEA081A018EAC9F107E26A21358F00705DAC388E159`，LFR SHA-256 `E41653B80A5BF1B6D464DEDAC95CC01F98ED3B081844F27ED2034257306ECFDE`。

独立远距对照：同一个已编译探针与同 seed/鸣人/武器/输入，仅将多由也初始X改为800，`kind10-x800-control`完成128tick，拾取18、轻投24、释放30均保留，kind10 applied数0、state1000高速交集0、F02返回tick不存在。相同LFR交336B44根EXE，[根报告](root-x800-control/root-report.json)仍为`passed=true/failureCode=0`；[初始+128tick独立比较](root-x800-control/paired-comparison.json)三槽24字段**3096/3096**一致。根tick31武器action42/state1002/Vx55，tick39 action50/state1002/Vx-1；这组控制仅改变碰撞几何前置，不以单例推断所有远距情形。

同一`kind10-x530/source-packets.lfr`交根正式EXE，根[报告](root-x530-01/root-report.json)为`passed=true/failureCode=0/declaredTicks=128/completedTicks=129`，多出一行是载体终行；`nativeParityClaim=false`。独立[比较](root-x530-01/paired-comparison.json)核对初始和128个完整tick，鸣人、多由也、武器每tick24个选定身份/动作/state/位置/速度/关系字段，共**3096/3096数值一致、首差0**。根trace在tick31记录实际kind10 applied，在tick39记录slot2帧`40→41`。这个比较不覆盖全World、音频、像素或设备表现。

诊断`probe-kind10-v1.exe`按[精确参数](compile-argv.txt)以`g++ -municode`链接正式playable源码，exit0且[编译输出](compile-output.txt)为0字节。代码改动仅为`Tools/NTSD28Q07Diagnostics/f02_pickup_throw_entry_probe.cpp`增加显式`kind10`模式和对手/命中记录；正式源、Unity生产、DAT、图片、声音、Scene、ProjectSettings及非战斗代码均未改。原Battle Scene完整Driver、真实Input System、碰撞比例映射、Renderer及退出清理尚未在本包验证；下一包须在不改DAT数值的条件下按同初态/输入做原Scene定向首差，不能由本包的正式源码/根同态直接宣称Unity通过。

治理验证：跟踪文档`git diff --check`通过，新C++无行尾空白；本代码路径[限定Ledger校验](ledger-scoped-check.txt)exit0；[全工作区Ledger校验](ledger-full-check.txt)本轮也exit0/PASSED（其余路径仅出现声明路径未进入当前diff的WARNING）。本包无Unity脚本修改，因此尚无新的Unity脚本编译或Play结果。
