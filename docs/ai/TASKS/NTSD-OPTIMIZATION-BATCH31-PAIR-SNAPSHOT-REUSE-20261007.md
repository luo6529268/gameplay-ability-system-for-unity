# 第31批 H07 精确候选循环：同一方向命中关系快照复用

执行结果：EVALUATION_COMPLETED / PERFORMANCE_INCONCLUSIVE / NO_DEFAULT_PROMOTION。120/120及两个真实1000AI120+180窗口已跑、末tick300原JSON同、容量/关闭与232保护通过；局部约21%降耗不代表真实混战收益。分散44.16ms、混战46.90ms/步仍>33，可见帧158–162ms；GC UNKNOWN，H11可靠采样FAIL保持。原件见同名REPORT.md/comparison-01.json/final-audit-01.json；不重复同构测量、默认切换或扩大六项范围。下面PLANNED保留为事前合同历史。
状态：PLANNED / NO_DEFAULT_PROMOTION。需求源：六项首阶段与Goal合同第13节；上一目标轮完成30正式失败报告收集，属于progress，未完成优化目标。新批累计10（22–31），不重置次数。

## 证据与最小范围
30 Dispersed1精确循环9.547925ms、排序0.530899ms；当前RecordOverlappingBodyCandidatesCached每个重叠body调用TryRecordReleaseCandidate重新Capture同一攻击者/目标关系。Factory仅读current/prev/collision frame、oid/type/team/facing/neutral-holder字段；record写Kind4SourceCount92、候选/nearest/RNG，不写这些snapshot输入。只在一次cached direction调用的栈上惰性Capture一次，跨itr/body复用；fallback/legacy保持逐body读取。invalid snapshot也不跳过原拒绝路径；每body的group eligibility、kind4计数、容量/selection/RNG与candidate写入照旧。快照不跨方向/tick保存，不加缓存/lease/新容量，不改输入/规则/顺序。

准确脚本：
1. Assets/NTSD/Scripts/Animation/Character/BruteForceSceneQuery.cs：CollectCandidatesForPairCached、RecordOverlappingBodyCandidatesCached、TryRecordReleaseCandidate局部拆分；测试专用旧读取开关与采集计数、每collection reset。
2. Assets/NTSD/Scripts/Test/Editor/RoleAwareCollisionShadowSelfCheckTests.cs：test-first多itr/body普通/Kind4/nearest序列、RNG、完整PairSnapshot、无相交零capture、不同方向与后续collection不复用；1000逻辑参与者固定fixture局部A/B（不是1000AI/FPS）。
3. Assets/NTSD/Scripts/Test/Editor/BattleOptimizationWindowsAiSuiteEditor.cs：复用既有生命周期/两role smoke，独立31新输出和request全字段回归；原26/29/30入口不改变。

## 冻结验证与收口
新增7条具名collector case（3 kind + 无相交 + 多target + 后续collection + 1000 fixture）和3条request case先RED；再实现，GREEN本RoleAware类与Suite request类，不全9361测试。局部A/B1000参与者、共享帧3 bodies/2 itrs、分簇位置、Role collector，预热8轮、旧/新各16采样（交替顺序），仅collection+消费结束计时，记录两均值/分配未知/计数差；无收益/噪声INCONCLUSIVE不自动推广。
生产：原Editor31菜单一次，两个与29全字段同的Dispersed1000/Combat1000 request，仅output不同，120+180、strict requireZeroGc、seed0x4E545344、正常renderer/sound、DataOrientedCanonical/no worker/max2、末tick300全JSON与29快照对照。代码变化后这是受影响域验证，不重跑30四长窗/27正例/28同构观察。GC FAIL保持原FAIL；性能无收益不称FPS修复。十一阶段/三残留0/双SceneSHA/Menu恢复后再验证保护和ChangeLedger。

## 不变项与边界
测试夹具更正：nearest用已有kind0/vrest0路径而非kind7，1000 fixture显式DesktopExtended/2000及ForceRoleAware；这是测试准确化，数量/窗口/生产规则不变，失败原件保留。
正式336、33ms/3ms/max2、checksum/RNG/slot/pass/OPoint/声音、公发表现只读、排序/segment/fail-closed/十一阶段Join不改；无默认broadphase切换、Scene/资源/Settings/Server/EXT1/Mono/ATLAS或活跃Q06方法体操作。固定共享terminal30字节备份允许既有runner写31终态；request不写/删。H11严格0GC仍未证，Windows末tick仅限定一致性非逐tick/native；H07/Goal仍OPEN，5/6交付不等于性能达标。
风险：virtual读取必须纯值，跨body记录路径不允许新增snapshot输入写方；候选影响常见Role路径而正常默认brute不受益。回滚准确Operation current-byte备份，另得批准后最小patch，不破坏性Git。运行前11备份/232非写域已核，原PID19040/6401/Menu idle。
