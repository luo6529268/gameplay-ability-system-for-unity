# C050 正式 kind50 首 BDY / 非零 dvy 完整 tick 正例

状态：`VERIFIED_SCOPED_SOURCE_ROOT`。父 C050/Q07/总目标仍开放；原 Unity 同态见独立 `NTSD28-336B44-Q07-C050-UNITY-DRIVER-001`。

权威身份：根 `NTSD2.8-Logan.exe` SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`；正式 playable 的 `BattleWorld28::resolve_confirmed_unarmored_standard_hit` 在 `special_link_rest` 时跳过 `HitResponseResolver28::accumulate_unarmored_vertical`，门由 `native_effect_action_override_is_suppressed` 对锁存帧首个 BDY kind50/52等条件决定。正式 `catalog.csv` OID24=`c\hid\hid.dat`、OID56=`c\hid\rea.dat`；OID24 action38 的 ITR kind0/effect1/dvy-5，OID56 action259 首BDY kind50。

输入：正式资源 `resources/runtime`、GameSession28 mode0、seed682973786、角色24/action37/X500/Z400/HP500/MP500/team1，对手56/action259/X520、X550或X1200/Z400/HP500/MP500/team2，40完整tick、全程中性输入。此为正式动作起始状态的受控完整会话；不证明玩家物理按键能自然选择起始 action37/259。

新诊断 `Tools/NTSD28Q07Diagnostics/hidan_bdy50_vertical_lfr_probe.cpp` 用正式28 Core+playable源重编；g++ exit0并产生 4,809,187 字节 EXE。空诊断时编译日志文件未生成，原包装命令在读取缺失日志时打印了错误，不能把它当编译失败。X520、X550、X1200各双跑，`source-ticks.csv` 和 LFR 在同输入两次运行 SHA 各自相同。X520及X550：tick2一条 applied kind0/dvy非零命中，命中前锁存259、首BDY kind50，`vertical_accumulated=0`，目标HP500→465、Vy0、hold-3。X1200：40tick无 applied 普通命中，完整反例。

正式根 EXE 使用两条原样 LFR（X520、X1200）和相同角色、起始动作、MP、背景重放，两个报告 `passed=true/failureCode=0/declaredTicks=40`；报告本身 `nativeParityClaim=false`。独立比较根 trace 的 tick1～40 与源码CSV：攻击者/目标动作、目标HP、Vy、hold 每例5×40=200/200字段同态、首差0。根 tick2近距同为HP465/Vy0/hold-3；远距无命中。原始编译 argv、两次源CSV/LFR、根argv/report/trace均在本目录。

边界：根 trace 没有直接导出 `vertical_response.status`，该字段来自正式对应源码完整会话；根只证选定可观察字段。Unity生产是否多施加垂直冲量、真实 Battle Scene Play、物理按键自然选招、完整World/像素/音频均未证。DAT、场景、生产代码与非战斗没有在本子包修改。
