<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C053-NATURAL-REACH-001
status: RUNTIME_PENDING
change-kind: CODE
code-path: Tools/NTSD28Q07Diagnostics/type3_latched_uj_reachability_probe.cpp
authority: selected 336B44 playable GameSession full tick and formal OID702 63 808 DAT C053 reachability
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C053-NATURAL-REACH-001.md
-->

# C053 正式完整 tick 可达性诊断

脚本前登记。Unity C053 共用解析及两项原 Editor 聚焦 2/2 已存在；仍缺 OID808 实际同 tick 锁存/当前帧分歧且进入 Uj 响应的运行证据。正式 OID702/63 frame554 可生成 OID808/action150；从静态 DAT/source 无法推出子体一定到达可命中窗口。本包只增一个源码诊断脚本，受控正式帧起点和中性输入均写入结果，不动正式源码/内容、Unity 生产或场景。

预期副作用只在仓库诊断脚本与新证据目录；运行输出可包含大型 trace，需逐案保存并核身份/双跑。验收：编译、正式内容身份、完整 tick 子体出生/帧/锁存/标准命中记录、阴性不假报、必要时正式根 LFR 和原 Unity Scene 另包。状态不得因脚本编译或静态链推进为 C053 VERIFIED。回滚仅审新增脚本和本 ID 文档；不清理其它未提交工作。

v1 实际脚本已写且以当前正式源码编译0错误；OID702三例、OID63两例各120tick均 `spawn=-1`。诊断直接从 action554 初态进入时没有经过帧计数0入口，正式 `materialize_native_frame_zero_entry` 因 `frame_counter !=0` 跳过 OPoint；本结果只暴露诊断初态错误，不作生产阴性证明。v1输出原样保留。修改同一脚本前追加范围：改从正式前驱 action553（wait0,next554）开始，并记 actor counter、spawned 计数，使用新可执行/输出名 v2；若仍不生成，继续诊断首差，不为绿改 DAT/正式源码。

v2 实际编译 exit0，OID702/X550、700、1200与OID63/X550、700五例各120tick均在tick1产生OID808/slot50，tick6/7/8分别153/155/156，`childHits=0`。这是正式内容生产完整tick的生成/帧链阳性、Uj命中阴性，不是C053运行验收。按原Task已声明的根回放范围，修改同一诊断脚本加入 `GameSessionLfr28` 录制及 LFR 文件；不改初态/命中/正式源码，v2原输出保留，新可执行/目录为v3。正式根若拒绝该 LFR，保留错误，不声称根同态。

v3 实际：当前正式源编译0错，OID702/X700两次120tick的CSV/RNG/LFR逐字节相同；根336B44接受LFR，`passed=true/failureCode=0/completedTicks=121`。独立只比实体动作/位置/计数与RNG计数16字段×120tick，1920零差；不把根 `nativeParityClaim=false` 改写成全态通过。CID C053 Uj仍无 hit。v4修改同一脚本前声明：可选第三战斗对象正式OID875/action55/team2，位置X参数；保留旧两人模式并记录三者身份/首命中。其正式帧含持续kind0/effect2，可验证泛型Uj消费是否在OID808可达窗口发生。三人初态为受控战斗对象配置，非物理键自然选招；根LFR三人能力未证，先只在当前正式源完整Driver搜正反X，不改Unity/正式内容。

v4 实际同一诊断路径已写，当前正式28 Core+3 playable源编译0错误；第三对象X580/620/650/700各120tick均tick7命中OID808，`WorldStandardHitStatus28::applied`、effect2、Uj响应156。当前与锁存帧同值，所以未验证父C053分读；三人源阳性各单跑。根336B44三人LFR `failureCode=46`，首tick HP1500/1000，headless只配置两人，属回放载体限制，保留失败报告。改前脚本 v1 夹具错误与全部阴性也保留。实际修改仅 `Tools/NTSD28Q07Diagnostics/type3_latched_uj_reachability_probe.cpp`；无生产/DAT/Scene改动。生成/编译、正式两人根回放、v4泛型命中是本包证据；原Unity Scene、三人正式根同态、区分锁存/当前帧的真实命中均未验。详细原件和SHA见[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C053-NATURAL-REACH-001/REPORT.md)。状态保持`RUNTIME_PENDING`，父C053/Q07开放。回滚只审阅本ID新增脚本/文档，不触动工作树其它修改。

v5 脚本修改前范围：已有 v4 X620 的单次Uj=156双跑原CSV/RNG/LFR逐字节相同；正式源按攻击者槽序消费且victim-rest按攻击者单独记录。只扩同一已登记CPP，使可选第四正式OID875/action55成为槽3，输出每个子体命中的status/effect/Uj响应序列，寻找首击后当前156/锁存153条件下第二次命中：锁存frame153 Uj156，当前frame156缺Uj会走默认20。先源完整Driver少数位置组合，旧模式保持、失败留存；不改正式内容/Unity代码。四人根CLI能力未证，不能把回放拒绝写成规则首差。实际结果和复验后更新本Record与总表。

v5 实际：同一CPP新增可选第四正式OID875/action55/独立X，原双/三人模式仍可调用；本次改动只在诊断脚本，没有Unity/DAT/Scene修改。当前336B44正式28 Core+3 playable源 `g++` 编译 exit0/日志0字节；四种第三/第四X的120tick各于tick7两次applied/effect2/Uj156。X620/620的CSV/RNG/LFR双跑同SHA，详[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C053-NATURAL-REACH-001/REPORT.md)。这是正式源码受控完整Driver有鉴别力的分支；根四人LFR入口无证且已知三人失败，原Unity Scene未验。Record保持`RUNTIME_PENDING`，父C053/Q07不关闭；后续另建Scene Task/Change，禁止将源码阳性直接晋升为用户画面体验或完整正式根证书。
