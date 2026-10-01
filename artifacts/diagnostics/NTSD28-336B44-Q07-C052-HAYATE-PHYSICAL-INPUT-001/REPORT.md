# Q07/C052 疾风站立入场物理按键可达性限定证据

状态：`VERIFIED_SCOPED_SOURCE_ROOT_PHYSICAL_INPUT / UNITY_SCENE_PENDING`。规则权威仍是 SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 的根正式EXE与对应playable live source，内容为正式`resources/runtime`；诊断重编EXE不作为权威。没有修改正式DAT、角色图、C++正式源码、Unity生产或场景。

静态前驱：正式`c/hay/hay.dat`的action212/213/214等帧含`hit_Fa:160`；`input_routing.cpp`在水平攻击组合成立时读该字段。新增[只读诊断](../../../Tools/NTSD28Q07Diagnostics/hayate_physical_input_reach_probe.cpp)用OID73/action0/X500/Z400/team1、两名OID2/action0/X589/619/Z400/team2、mode0/difficulty0/seed682973786。v1单tick脉冲两例双跑无阳性，因本地输入只在交替`input_update_phase==0`采样，不能据此判规则不通；原件保留。v2只将jump及组合键按住覆盖采样相位，并导出实际phase/sampled mask，当前正式28Core+playable链接编译exit0/stderr0，诊断EXE SHA-256`AC21D915BE993FAFAA5A498BBB17E091B1B91E246646495D3F555770EF581595`。

**站立入场阳性**：[v2逐tick](mode0-hold-v1/source-ticks.tsv)中jump在tick1～4提交，tick2正式采样并进入action210；角色tick7进入action212，tick8正式采样defend+right+attack并进入action160、MP500→300；tick11自然生成OID417/slot50，tick29生成OID211/slot51，tick31该子体的候选按applied/rejected-rest/applied顺序使两目标HP500→420。[逐hit原件](mode0-hold-v1/source-hits.tsv)。两个独立60tick运行的tick、hit、LFR三文件各自逐SHA相同；受控初始dash213组合键正控制也于tick2进入action160、tick5/23出生两层、tick25双目标命中，双跑三文件亦同SHA。

相同[v2站立LFR](mode0-hold-v1/source-packets.lfr)已直接送根正式336B44 EXE，独立进程exit0，[根报告](root-standing-v3-report.json)为`passed=true/failureCode=0/declaredTicks=60`。按60tick×19项角色动作/源坐标/MP、两目标动作/HP/X、OID417/211数量／首槽／动作／X，[独立逐字段比较](root-standing-v3-comparison.json) **1140/1140零差**，根tick8 action160、tick11/29出生子体、tick31两目标HP420。最初以PowerShell调用根GUI子系统EXE的v1/v2没有等待进程也没有结果文件，空stdout/stderr保留，不计正式回放；v3使用等待进程结束的subprocess取得报告和trace。根LFR仍不能覆盖第三目标初始朝向，源码该目标为左、根默认右，所以不主张完整初态／全World同态；根报告自身`nativeParityClaim=false`。

下一出口是在**原Unity Battle Scene**对同一固定按键序列（相对tick1～4 Jump、tick8～10 Defend+Right+Attack）从站立action0推进，比较至少到tick31及退出状态。当前还没有Unity此键流证据，不能因受控初始action160的旧Scene例通过而宣布C052/Q07关闭。
