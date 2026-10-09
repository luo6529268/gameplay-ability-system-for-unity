# 第88批：生产者快照复用资格与真实千人收益
结论：SCOPED_WINDOWS_COMPLETED / GAIN_SIGNAL / H07_PERFORMANCE_FAIL / DEFAULT_OFF / NO_DEFAULT_PROMOTION。第88批冷诊断接线与限定验证完成，不等于H07或生产优化完成。67已执行、首阶段5/6、34父项关闭0，Goal保持active；次数与报告交付不触发停止。

## 范围与证据口径
Task：NTSD-OPTIMIZATION-BATCH88-PRODUCER-SNAPSHOT-WINDOWS-20261009；Change：NTSD-OPT-H07-PRODUCER-SNAPSHOT-WINDOWS-088。
只在两个声明的Editor脚本新增明确owner、request、资格与测试；World的87候选、87局部测试、Harness和生产默认字节保持。未创建新Grid/Burst/worker/backend，未解冻EXT1/Mono/ATLAS或修改资源/Scene/Input。沿现有原Editor2022.3.62f3、PID74724/MCP6402，不启动第二Editor、不保存Scene。
冻结见[窗口版本](windows-freeze-02.json)；真实数值/原report摘要见[四窗复算](measurements-summary-01.json)，原件均在windows-02。仅Windows Editor、单角色OID1 Sakura及既有minimalAI/Canonical固定workload，非普通App全角色、Android/Player/120FPS认证。

## test-first、修复与实际执行
- 首RED误把新Driver反射入口写成旧容量候选，25/1PASS/24FAIL；20新冷接口缺失有效，4Driver不是本候选有效资格。red-01原件保留，1PASS不算新准入。
- 正确入口Driver RED02实际5/0PASS/5FAIL，原件red-02；随后两Editor冷helper接线。冷GREEN01实际80/80/0skip（96c2ba64e6dc41d3b0842cc3af37ae7f）。
- 真实Driver资格4/4/0skip（5fe59d1709064013b11123eebdafe301），green-driver-01与四新增GUID JSON保留。两formal336根方向各12配对tick，仅声明oid/action/hp/vx原root字段；两1000AI布局各32配对tick，OFF刷新各32000、ON省复制各32000/fullRefresh0，实际AI每tick1000。全部既有20-domain/Lockstep hash、完整Native scalar RNG/legacy state、collector/数组/容量/关闭断言保持。共88配对/176完整Driver step，不冒充正式EXE千人全World证明。
- windows-01首OFF120+180后新增owner错误要求录制器仍live，正常Harness teardown已经关闭它；Suite按原gate PARTIAL/DONE/1of4自然退出。此原件不当作有效对照，余三request未执行，全部保留。
- 新三生命周期用例RED3/2PASS/1FAIL（a1ff2eed87b640e4a5f8266015d748a0）；修正只分开结构owner与实时录制状态，实时Observe仍拒绝关闭录制，结束后不再重复实时Observe。两已知输出根保持terminal识别，重新输出只用新windows-02。
- 冷GREEN02实际83/79PASS/4FAIL，仅同包新request测试仍期待旧输出root；只改该新字面量，完整65字段其余相等断言不变，受影响GREEN03实际4/4（f1fe0e6f6ef4495988b612724b5a4c19）。复用未改变的79与Driver4，87去重具名通过，不声称最后单job87全重跑。未重跑无依赖变化的完整Driver或历史矩阵。
编译/导入及上述实际测试通过；最初Console error CS0，资格后的新Console扫描传输超时，不能把该超时当作新的Console0。后续Editor state即时返回idle并正常运行四窗。所有失败、超时、一次patch完整行anchor失败和摘要录入更正均留在Change/Operation；未据观测超时重启或重复提交作业。

## 同版四窗结果
冷资格后才重新唯一启动02菜单；所有4窗实际1000AI、120warmup+180sample、同源/同请求，只有outputPath不同。显式Role collector、共享纹理绑定，87reuse由冷owner切换；85输入边界计时两侧均ON，83采样计时和84容量候选关闭。不是Role生产默认切换。
| 窗口 | logic mean ms | logic P95 ms | producer refresh ms | CharacterInput ms | 全run丢弃债务tick | 显示FPS估算 |
|---|---:|---:|---:|---:|---:|---:|
| 0 Dispersed1000 OFF | 42.803407 | 48.497775 | 0.968130 | 10.921588 | 272 | 7.050 |
| 1 Dispersed1000 ON | 42.108892 | 47.181850 | 0.431109 | 10.250961 | 257 | 7.195 |
| 2 Combat1000 OFF | 43.373261 | 51.622060 | 0.968160 | 10.896393 | 265 | 7.105 |
| 3 Combat1000 ON | 42.648791 | 50.517480 | 0.429989 | 10.201825 | 259 | 7.190 |

分散整tick均值减少0.694514ms（1.622568%）；混战减少0.724469ms（1.670314%）。两侧ProducerSnapshotRefresh约0.968→0.430ms、CharacterInput约10.90→10.20ms，与实际skip一致，判断为小幅收益信号；单轮顺序A/B不能外推稳定收益、将全部均值差归因于候选或自动推广。
ON P95仍47.181850/50.517480ms >33ms，全run dropped257/259，H07仍FAIL。backlog当前/峰值0是丢债务政策下的值，不代表追帧债务未丢或性能达标；catch-up最多2保持。显示FPS由1000/Time.unscaledDeltaTime窗口均值估算约7.19，非纯logic倒数、GPU帧率或120FPS证书；不能把测试初始化/完整snapshot I/O时长当logic成本。
全帧Profiler SetPass四窗平均/最大都5，中央CPU提交draw平均/最大都2、segment/chunk均1；不等真实GPU batch。此项既有收益保持，不是本候选进一步减少draw。未运行Frame Debugger/GPU capture。

## 一致性、0GC、容量和关闭
两对各65 request字段仅output不同；20 finalParity/finalLockstep hash相同，末tick300完整snapshot逐字节相同。两个布局分别SHA F1A2CAC863D1AA995D49BEF7DA2D5C1B30337465FA846DB0CEFAF86D8AD7B6AA / E4916ACA8C5BE675B8192D5A6C1429029FD94DB6F838CF6C82E532B030836B12。末态一致不替代所有正式tick/GPU或Native全域证明；新32tick Driver证据是单独资格。
四窗完整同步主线程StepOneTick GC observer前后1MiB正/空负校准通过，accepted300/steady180/nonsteady120、invalid/rejected/sequence0，steady allocation events0/proven steady0B。nonsteady events142/24/24/24，不能声称全300tick或完整camera/GPU/worker0GC。H11第86批已通过范围复用，不重开。
每窗四输入边界call累计均180000；sampled Editor观察9/9/10/10次，OFF最大skip0/fullRefresh1000，ON最大skip1000/fullRefresh0，applied/unchanged/restored都true。这里是last completed pass观测最大值，不是全tick累计。每slot/热路径容量红线未改。
capacityPressure四窗180tick都passed、violating0/criticalDelta0/runtimeGrowthRejectDelta0/poolExpansion reject0，runtime slot1050；central source/resolve平均和最大匹配、finalRefusal为空，仅说明这些有效采样统计，非单独GPU/所有相机stale认证。每窗teardown restored、对象/entity/slot/active pool/reference pool0；最终orderedShutdown true，objects/slots/borrowers0，inactive1050不作为active残留。Menu sole clean8，双Scene SHA保持，SharedRequest始终不存在。

## 余项与下一安全动作
本Task的必要候选资格/短窗收益已闭合，默认关闭，不重复87局部资格、这四窗或用更多相同短窗找PASS；P95/drop未达时不启动1800正式证书。H07继续OPEN，不能因批次数或本报告停止Goal。
当前ON CandidateCollect14.182/14.801ms，PairExactLoop6.943/7.271ms，CharacterInput10.251/10.202ms、NativeSampledState约2.25ms、LateEntityUpdate约5.24—5.28ms。优先结合第83批既有采样，只读闭合当前Role精确候选记录的重复状态访问/查找；有安全可辨工作才新Task/Change实施，不再次启动已拒84guard。未找到有效同范围候选时再转已测NativeSampledState，不能跳过规则、降低cadence或凭猜测引入架构。Role生产默认冷选择仍需明确授权，只约束该推广，不停止其它H07安全动作。
正式336B44、33ms/3ms/max2、RNG/输入/pass、publication只读、排序/segment/fail-closed和十一阶段关闭保持。Q06活跃方法体未读；无删除/移动/Git丢弃/commit/push、Scene保存或资源/设置/第三方变更。技能影响仅原共享Editor安全流程。

## 审计
818保护/10 expected absence无漂移，9原current-dirty副本及before-02 Suite副本保持；冻结8文件无漂移，HEAD同/staged空。源/证据摘要、最终Editor和Ledger/diff实际结果见audit-final-01.json与Operation；校验未运行前不预写PASS。准确命令：两manage_asset import、具名run_tests/get_test_job、各输出根菜单一次、guard/source SHA核对、Tools/Validate-ChangeLedger.ps1、git diff --check。原件而非工具catalog总数定义case数。

最终实际校验：Tools/Validate-ChangeLedger.ps1 exit0、1383 Records、15 governed code files covered、4180历史WARNING字符串、0ERROR；git diff --check exit0，仅既有CRLF提示。相对准确dirty before为Suite321新增/3处原行改写、Admission155新增/0删；没有HEAD整体回退。最后get_goal返回active，未作状态变更；没有新的1800、M0、Profiler捕获或默认推广。

