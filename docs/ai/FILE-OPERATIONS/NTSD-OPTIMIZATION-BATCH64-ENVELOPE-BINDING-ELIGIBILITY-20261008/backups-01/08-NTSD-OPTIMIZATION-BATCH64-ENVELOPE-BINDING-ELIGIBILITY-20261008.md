# 第64批：既有包络＋绑定复用基线上，参与资格复用的组合资格与真实收益

状态：READY / EXECUTION_NOT_STARTED。没有本批脚本修改、测试或实际收益；尚不计入已执行42批。用户六项继续授权及有效Goal合同第0—8节覆盖既有普通Brute路径，不是新collector/backend、规则或专项变更授权。

## 依据与新问题

63四actual1000AI固定120+180已完成：两側包络ON，binding ON后的PairExactLoop平均39.799789/41.444106ms，logic P9588.318355/92.455340ms与drop619/598仍失败。当前Query CollectCollisionCandidatesBruteForce（本次重扫2347—2429）仍在内层重复entity/PS/IsPendingFlushDestroy/IsCollisionCandidateSuppressed；现成CollectBruteExactPairsWithCachedEligibility（2441—2466）在本collection同一participants上以同i/j与同双向顺序遍历，跳过这部分重复资格读取。TryBuildBruteExactCache（2520—2569）先过滤并重建每次collection，不跨tick缓存。

50已测eligibility单独OFF/ON有logic mean9.50%/11.38%信号，52混合的是eligibility＋kind5且结果不稳定，不能外推本组合。63包络＋绑定复用与eligibility共同ON尚无Driver组合资格/可靠logicGC/实景收益；现有Admission Run的coarseEnvelope和rejectedBindingReuse分支（本次重扫569—594）明确要求eligibility false，因此不能据旧49/62 PASS冒称组合已通过。这是新交互证据，不重跑原49/62原矩阵或52kind5窗口。

当前源/旧证据指纹在实施前由准确Operation manifest重新保存；定位行号只是本次只读扫描，不是永久合同。63源/109guard与旧qualified证据未改。

## 准确范围与非目标

唯一拟改C#：
- Assets/NTSD/Scripts/Test/Editor/BattleBruteProductionAdmissionEditorTests.cs：增加显式三机制组合资格入口/具名最窄cases，记录真实应用；原Run/旧49/51/59/62断言语义必须保留。组合入口或显式参数必须与旧null调用可区分，不把旧assert改为无条件宽松，不复制另一套driver。
- Assets/NTSD/Scripts/Test/Editor/BattleOptimizationWindowsAiSuiteEditor.cs：只有上述资格通过且有信息量时，新增明确opt-in组合窗口；沿用现有request/scope/owner/恢复基础，不改旧63/60模式准入或生产请求。三flag保存原值/独占owner/逐tick不变/正常和异常恢复必须闭合。

BruteForceSceneQuery.cs、ProductionEntityStressHarness.cs、BattleLogicTickGcObserverEditor.cs和所有生产runtime只读，不修改默认值/collector、AI、输入、pass、逻辑33ms/3ms/max2、候选消费/排序。三机制均已存在，不新增容器、缓存算法或几何判断；kind5/dispatch/branch timing等仍false。Scene/资源/settings/Input/Gen/Plugins/Server不改，Q06只hash，EXT1/Mono/ATLAS USER_HOLD保持。不启动专项M0/instancing，不默认推广，61字节预算UNKNOWN不靠估算晋升。

## 事前合同和保护

修改任何脚本前另建本批独立Change Record（PLANNED或IN_PROGRESS）、Operation/准确current-dirty副本/HEAD及保护清单；在Ledger/STATE/handoff回链。既有两个目标已经dirty，备份必须取当前字节，不能用HEAD或旧63before代替。公共Temp测试XML/压力终态及旧原件在可能覆盖前也先保护。使用apply_patch、原Editor唯一实例、安全idle窗口；不重启/第二Editor/清空Console或测试状态、不删除/move/reset/restore/checkout/clean/stash/push。

新增entry/恢复owner不是生产模块；实例World结束须复用原11阶段/worker Join/关闭事务，异常恢复先解除候选flag owner但不得越权清World。保持完整logic scope与H11范围，不把局部0GC替代完整表现门。

## 一次性最小验证

1. 新组合具名测试先有效缺接口RED，再GREEN。只新增组合语义和失败边界；复用83接线和既有正确性依赖指纹，不因新编号重跑全部campaign或为未改接口添加大量同构case。
2. 同formal根两场景各12 paired tick、同千人Dispersed/Combat各32 paired tick；两侧四admitted=true/coarseEnvelope=true/rejectedBindingReuse=true，仅eligibility OFF/ON。保持相同seed/input/profile/roster/ForceBruteForce/noWorker；逐tick declared hashes/RNG/entities/原适用native roots一致、无fallback。既有kind5始终false；实际应用与probe/reuse只按计数语义，不混作时间收益。不能重用62 terminal-only或旧49独立PASS代替三机制交互。
3. 资格通过后，同包一次四新120warm+180sample Windows窗口，65request字段与63对应工作负载除output相同。基线是63三机制中的前两项ON，候选只再开启eligibility；记录logic mean/P95、collector/PairExactLoop、frame/dropped、中央CPU draw/Profiler SetPass各自口径，不称GPU batch。
4. 可靠GC继续完整StepOneTick前后empty＋known-positive校准、300有效scope/稳态180，禁止TimeNanoseconds→bytes或复跑61失效CurrentThread API。新mode接线不削弱旧owner/scope guard；容量拒绝、declared终态20hash/完整snapshot、两侧三flag恢复、11stage残留/Scene保护如实验证。
5. 短窗P95/drop仍失败则不加1800找PASS；无收益/不一致明确不采用，复盘既有热点，不停止整个Goal。有效固定正式120+1800及H11/central独立门仍属后续必需验收，短窗不能替代。
6. 文件后审查精确dirty diff，跑pwsh -NoProfile -File Tools/Validate-ChangeLedger.ps1和git diff --check；按影响域复用未变证据，报告实际命令/原失败/未验证项。无本机Unity证据时不把静态/生成编译称runtime通过。

## 收口与回滚

本包只回答“既有三机制在同普通Brute路径是否兼容，是否有实际增量收益”。不以test数量/报告交付称H07完成，不从单顺序AB推稳定百分比或Android/120FPS证书。主进度仍仅原优化总表，其他强制治理文档只回链状态，不复制完整报告。

回滚只针对准确before之后本批hunk，需另批准和留痕；不能覆盖已存在用户工作。候选默认OFF/NOT_ADMITTED保持，当前没有运行句柄，不能凭READY文件称已开测。
