# Q07/F02 正式物理按键拾取到高速武器释放

状态：`VERIFIED_SCOPED_SOURCE_ROOT_PICKUP_RELEASE`。本包用正式playable完整GameSession与336B44根EXE证明了**已有type4武器→真实按键拾取→真实按键投掷→高速释放**的可达前置；尚未让武器在高速时被kind10/15命中并进入下一物理tick的state1000门。因此F02、Q07及总目标继续开放，原Unity Battle Scene未运行。

权威：正式根`NTSD2.8-Logan.exe`本轮SHA-256为`336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`；所选playable `SimulationTickDriver28::step`、`NativeInputRouter28::route_native_standing_attack`、`BattleWorld28::settle_held_refill_objects`、`PhysicsIntegrator28::step`及正式`resources/runtime`目录。正式`w/6.dat` OID600/type4具有初始action0、地面action64、持有action24/100及投掷action40～；鸣人`c/nar/nar.dat` frame47 WPOINT声明`dvx=55`。诊断EXE是对应源码的测试载体，不替代正式根身份。

受控初态：mode0、seed`0x28A55A5A`、项目外的正式背景ID23只作原生会话配置；鸣人slot0/OID2/X200/Z542/team1，李slot1/OID7/X1200/Z542/team2，正式OID600武器slot2/action0/X190/Y-20/Z542/team1/HP250。两名角色与武器均由`GameSession28::initialize`配置创建；初始化后不手设动作、速度、关系或持有。探针只经`set_input`给鸣人离散攻击键。远距控制仅把武器初始X改为800，其他初态、随机种子及首次攻击时点保持。

正式源码完整tick的关键观测：

| tick | 近距结果 |
| --- | --- |
| 17 | 武器自然落地至action64/state1004。 |
| 18 | `pickup_attack`令鸣人action115、关系4/child2；OID600 action24/state1001、关系-4。首次探针误把type4关系当2，输出的pickup=-1是探针判据错误，原件`near-01`保留。 |
| 24 | 释放攻击键后再发`light_throw_attack`，正式输入路由选鸣人action45；武器仍处持有state1001。 |
| 29～30 | 鸣人进入frame47，持有结算读取WPOINT `dvx=55`；tick30武器关系归0，action41/state1002、`Vx=55`、`Vy=-4`。动作40在同tick后续帧推进到41。 |
| 31～33 | 武器仍state1002、`Vx=55`；tick34正式投掷帧的`dvx=-1`使后续`Vx=-1`。这一短窗口是下个kind10/15复现的时序约束。 |

修订版`probe-v3.exe`近距`near-03`与远距`far-02`均完成128tick、exit0；两例tick18均发相同`pickup_attack`。远距武器仍X800/action64，鸣人action65/无持有，128tick内pickup=-1、未投掷。近距`near-02`与`near-03`的完整CSV和LFR各自逐字节同SHA：CSV`FABA40624A7F41DAF9563CEA081A018EAC9F107E26A21358F00705DAC388E159`，LFR`E41653B80A5BF1B6D464DEDAC95CC01F98ED3B081844F27ED2034257306ECFDE`。`far-01`曾未发攻击键，不作为正式对照；所有旧结果保留。

同一近距LFR原样交正式根EXE，根[报告](root-near-01/root-report.json)`passed=true / failureCode=0 / declaredTicks=128 / completedTicks=129`；多出的一个为载体终行，`nativeParityClaim=false`，不把报告PASS独立说成战斗同态。独立比较[源码CSV与根trace](root-near-01/paired-comparison.json)的初始tick0及128个完整tick，slot0鸣人与slot2武器每tick17项动作、state、关系、链接、X/Y、Vx/Vy及身份字段，**2193/2193数值一致**。根trace明确复现tick18拾取、tick24动作45和tick30 OID600/action41/state1002/Vx55；比较范围不涵盖整个World、音频、画面或设备输出。

编译：精确参数见`compile-argv-v3.txt`，`g++ -municode`链接所选playable源码闭包exit0，`compile-output-v3.txt`为0字节。第一次编译和`near-01`为受控失败/更正过程，三个二进制版本和结果均保留。仅新增`Tools/NTSD28Q07Diagnostics/f02_pickup_throw_entry_probe.cpp`；未改Unity生产、DAT、图片、WAV、Scene、ProjectSettings或非战斗代码。

治理校验：本包代码路径的`Validate-ChangeLedger.ps1 -StagedOnly -SimulateChangedPath ...` exit0/PASSED；跟踪文档的`git diff --check` exit0；新C++无尾随空白且以LF结尾。全工作区Ledger校验exit1，唯一报错为既存且不属本包的`Assets/NTSD/Scripts/UI/UIButton.cs`未记录改动，未触碰该文件或替它编造记录。原Unity Editor编译、Play、设备画面本包均未运行，不能由源码/根报告推断通过。

后继出口：用这条已证高速释放链，在tick30～33真实让正式kind10/15生产者命中OID600，记录武器从state1002切回action0/state1000以及下一物理入口摩擦后严格`|Vx|>9`，同时确认落地没有覆盖动作40。先正式源码+根、再原Battle Scene同条件输入/实体/画面；若初态或LFR无法运输，必须明示边界。新脚本先建独立Task/Change，不把手设速度或关系充作自然阳性。
