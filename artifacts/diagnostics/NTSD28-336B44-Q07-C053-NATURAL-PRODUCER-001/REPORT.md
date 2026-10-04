# C053 正式 OPoint 生产者的受控自然链

**2026-10-04 追加更正：** 下文“根trace不公开逐hit”过窄。对本报告保存的336B44根trace逐行复核，tick7确有`events.kind=hit`：slot51→slot50、candidate0、`applied`、HP伤害25；与正式源码CSV的40tick目标命中计数40/40一致。根事件未导出effect/Uj，不能声称根直接观测`effect2/Uj156`，且此样本只有一次目标Uj，不关闭双Uj。详[独立复核](../NTSD28-336B44-Q07-C053-ROOT-HIT-EVENT-REINTERPRET-20261004/REPORT.md)。旧原件及其按当时可见字段计算的480/480结论不改。

状态：`VERIFIED_SCOPED_SOURCE_ROOT_PRODUCER / C053_OPEN`（2026-10-02）。本包找到不必将 OID875/action55 直接放入 LFR 槽2/3 的正式内容入口；**没有证明玩家物理键自然选招、两次命中、Unity 原 Scene 同态或 C053 整项完成**。

权威身份：根正式 `NTSD2.8-Logan.exe` SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`；诊断按当前 playable 闭包编译，读取同版 `resources/runtime`。正式 `data/data.txt` 的 OID65=`c/ank/ank.dat`、OID702=`c/jira/sag.dat`；`ank.dat` action511/512/513 OPoint 建 OID875/action50，OID875 `c/ank/a/atk.dat` action50 `next:55`，`sag.dat` action553 OPoint 建 OID808/action150。运行前静态路径不作为阳性证据。

[诊断源码](../../../Tools/NTSD28Q07Diagnostics/c053_natural_producer_probe.cpp) 以 seed 682973786、mode0、背景ID1、两个战斗角色 slot0 OID65/team1 初始action511、slot1 OID702/team2 初始action553 跑完整 `GameSession28::step`，两者均给中性后续输入。两个初始动作是**受控设置**，不是物理按键选招；OID875与OID808的生成、后继帧和命中则由正式资源/完整战斗pass自然推进。首轮安科初始Y=-50、X460～600八组40tick均在tick4生875/tick5进55、tick1生808，但无Uj；`X520` 轨迹显示攻击体tick7 Y=-92、目标Y0。这是限定阴性，不代表该机制不可达。

第二轮安科初始Y0、X550/580/610/640/670/700/730，Jira X500，各40tick；X580～700五组于tick7有OID875→OID808的 `applied/effect2/Uj156`，X550/730无Uj。选定X610的[源码逐tick CSV](source-run-y0/ank610-y0-jira500.csv)：OID808 slot50 tick1出生，OID875 tick4以action50出生、tick5进55；tick7 hit序列 `51:0:2:156`，目标结束action156、HP475，攻击者结束action11。[完整LFR](source-run-y0/ank610-y0-jira500.lfr)由此源会话录得。相同源码二次运行的7 CSV+7 LFR **14/14 文件逐SHA相同**，见[哈希表](source-repeat-hashes.json)。本包诊断编译 `g++` exit0，编译参数与产物见 `compile-argv-v3.txt`、`c053-natural-producer-v3.exe`；前两版编译产物/运行结果保留，不覆盖。

同一X610 LFR交由根正式EXE，显式只覆盖其支持的槽0 action511、槽1 action553、两槽MP500；第二次用同步进程等待取得**exit0、stderr空**。[正式根报告](ank610-root-v2-report.json) `passed=true/failureCode=0/declaredTicks=40`，`completedTicks=41` 含终端额外tick，`nativeParityClaim=false`。根[trace](ank610-root-v2-trace.jsonl)在tick7有OID808 slot50/action156/HP475、`opointActionLatch=153`、OID875 slot51/action11；根trace不公开逐hit事件，不能把源码hit序列写成根直接观察。[独立选定字段对照](ank610-source-root-comparison.json)将源码CSV与根trace的两主角动作、首OID875动作/XYZ与数量、OID808槽位/动作/XYZ按声明的 **40tick×12字段=480/480 零差**；终端tick不计入。两次根trace SHA完全相同。

本证据改变下一行动：可以用 OID65+OID702 两名初始角色和正式 OPoint，在原Battle Scene 的生产Driver中复现单次C053 Uj，避免人工建立OID875/action55；先按同初态逐tick找到首差，再决定是否需要最小的逐hit诊断。它**没有**触发受控旧案例的双Uj，也没有证明第二次命中读取锁存帧的 Unity 运行值。C053、Q07及总目标继续开放；DAT、资源、Scene、Unity脚本与非战斗逻辑均未改。

治理与保护核验：[Change Ledger validator](ledger-validation-v1.txt) exit0/PASSED、1115 Record/61个当前差异代码文件；本包相关已跟踪文档 `git diff --check` exit0，新C++诊断行尾空格0。原Battle/Menu Scene、GameConfig、ProjectBattleModeConfig四文件SHA仍分别为`93448372…7BF60`、`DD6A48A3…B9DC3`、`0527D737…CB8EA7`、`B57CFEF3…85B82`。本包未调用Unity Editor/Play，四哈希只证明磁盘保护文件在本轮核对时未变，不是新的场景验收。
