# 第32批 Brute 无攻击框方向前置拒绝
最终限定交付：SCOPED_GAIN_NOT_ADMITTED / RUNTIME_PENDING。130/130及一次两个真实1000AI短窗已完成、旧26同请求末tick300完整JSON同、关闭零残留/保护通过；logic231.616/236.767ms、可见帧614.468/582.806ms仍未达标，不默认推广。当前整体状态/实际命令/未知项见本Change和唯一优化总表，以下PLANNED为事前冻结历史。没有新增性能窗口/专项授权/终止Goal。
状态PLANNED / NO_DEFAULT_PROMOTION；新批累计11（22–32）。§13用户要求真实热点继续；复用26/29/30/31报告，不重跑四长窗/GC观察。

准确3脚本：BruteForceSceneQuery.cs（CollectCandidatesForPair前置纯无itr guard、默认false控制与测试计数）；RoleAwareCollisionShadowSelfCheckTests.cs（Formal类内7新case）；BattleOptimizationWindowsAiSuiteEditor.cs（独立32两brute smoke、原菜单不改变、控制owner及恢复）。

原状：CollectCandidatesForPair先CandidateCollectionPairAllowed（pending/AttackExempt/vrest/关系），后GetCollisionFrameData查itrs；没有itr时最终无候选/RNG/写方。基类getter+LF2FrameCache为只读数组查询，Animation域重扫无override。候选仅在诊断显式打开：先读取同collisionFrame并空/null itrs立即返回；非空继续原判定且复用已读frame。默认false保持原门/读取顺序，不自动选Role/更换索引。行为/数据权威不变；适用native trace未齐不得默认推广。

冻结test-first：3多body case kind0/kind4/nearest kind0，完整候选/PairSnapshot/顺序/RNG/Kind4逐body计数旧新同；全inert零RNG/门；后续collection新增itr重查；active AttackExempt仍拒绝；1000逻辑参与者固定40个有itr/960无itr，4warm+旧新各8sample交替order，计时仅capture/collect/end，pair gate实际次数减少（999000→39960每轮）。不设收益阈值/0GC证书。Suite2全字段同既有BuildRequest(0/1)仅output不同以及非法index1case，共3 request tests。先具名10RED，再同Formal＋Suite两类GREEN，不全套。

真实矩阵：独立32菜单一次，两个120warm+180sample actual1000AI、seed0x4E545344、brute/DataOrientedCanonical/正常renderer+sound/noWorker/max2/strict原门、末tick300全JSON与26原brute对应窗比较。新增guard不是request字段，Suite state必须明示；StartRun Configure后且0 warm/sample、原query获取成功才开，至本run结束后恢复旧值，shutdown fallback幂等恢复，不改变Scene/资源文件。原6/29/30/31菜单request保持。CPUcounter未校准raw0B不能覆盖H11FAIL。

11写域与前一批同（3code＋Ledger/STATE/Index/handoff/Tracker/Goal合同/H07计划＋runner shared terminal），当前字节备份、232原保护及31新增证据保护，在before.json。原PID19040/6401 Menu8roots clean/idle，没有第二Editor。文本apply_patch，备份新目录Copy-Item核SHA；shared31terminal先存，不写删sharedrequest。

正式336、33ms/3ms/max2、RNG/checksum/pass/输入/命中/OPoint/声音/publication只读/排序segment/failclosed/11阶段Join保持。不读Q06 Shadow方法体，不写Scene/Prefab/资源/Settings/Gen/Plugins/Server/EXT1/Mono/ATLAS。不新增持久cache/lease/容量，diag布尔不定义规则，owner原SceneQuery；Suite结束恢复flag并关闭现有owner。风险是getter必须纯读取、以后override须重新评估；本轮完整性能/权威/0GC未证。收益混合/无收益INCONCLUSIVE不推广、不追加同构测量。H07/H11/Goal保持开放；恢复需另得准确批准后最小patch当前备份，不破坏性Git。
