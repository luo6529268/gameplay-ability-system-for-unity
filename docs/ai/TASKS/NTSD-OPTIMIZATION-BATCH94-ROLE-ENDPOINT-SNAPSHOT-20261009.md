# 第94批 Role 参与者端点快照复用
状态 FOCUSED_TEST_PASS / LOCAL_ONLY / NOT_ADMITTED / DEFAULT_OFF；94限定评估完成，最终38/38安全回归。当前latest94/73已执行/5of6/H07 PERFORMANCE_FAIL/Goal active；最终版无实际1000AI/Driver性能收益证据，不以局部PASS收口H07。编号100只检查方案落实清楚，不要求届时达标。以下具体范围及事前状态按历史记录阅读。

## 对象、依据与方法
现有83/91证据：CandidateRecord为Role精确子段最大样本，PairExact约8ms、整Collector约16ms；93阵营拒绝候选无真实收益，保持OFF不重跑。当前31只按pair方向复用payload；本次减少跨不同pair重复的OID/type/group/action/current/previous/tick/facing读取，不改命中/选择/RNG/record写。不是89的四帧引用复用：本次以每个参与者的完整只读scalar端点为单位，原holder lookup仍在每pair原捕获点执行，不缓存holder或改变自定义虚方法调用。

三准确C#：
- Assets/NTSD/Scripts/Animation/Character/BruteForceSceneQuery.cs：新增default-off EnableRoleEndpointSnapshotReuseForDiagnostics；PrepareBattleCapacity冷预热端点primitive值数组/byte状态；Collect每次独立重置、Role exact cache验证之后按handle slot建立eligibility；RecordOverlappingBodyCandidatesCached首次pair capture优先尝试端点组合，失败走原Capture。无热扩容，无新的World/entity引用；ResetFormalSpatialBroadphase清状态。
- Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleHitCandidatePairSnapshotFactory.cs：新readonly scalar endpoint/CaptureEndpoint/ComposeFromEndpoints；原Capture与89路径主体保持，linkedHolder每pair仍原live lookup。字段仅原BattleHitCandidatePairSnapshot投影，不新增逻辑含义。
- Assets/NTSD/Scripts/Test/Editor/RoleAwareCollisionShadowSelfCheckTests.cs：只新增 EndpointSnapshot_ 具名反射test-first与局部cost/helper；不重写旧断言或fixture。

## 安全/容量/生命周期
缓存只在ForceRoleAware既有cached direction路径使用；exact LF2Character且有有效handle与数组slot才合格。自定义派生类/virtual reader继续原Capture；任一AfterRoleAwareExactPairForSelfCheck存在则整段禁用，ForceLegacy控制保持。每次Collect独立失效，重复同tick不可复用旧字段；slot/generation复用不得跨收集。原步骤中record只写candidate store/count/nearest/kind4/RNG，不写上述端点；现有占用epoch/精确cache验证/异常atomic abort不动。未prewarm/slot越界仅原Capture fallback、不增长/截断/漏候选。既有Prewarm/owner query生命周期；新数据无引用，在Shutdown原reset阶段失效，不新增owner、队列或阶段。容量以runtime slot数，非chunk；冷准备seal后若需增长直接拒绝。记账按实际结构大小与两数组length，不声称RSS/GPU或ATLAS改变。

## 冻结验证
RED只 EndpointSnapshot_ 新case（6种kind的sequence/payload/count/RNG/kind4 parity；default OFF、no-prewarm fallback、custom subtype fallback、pair-hook bypass、同tickteam/facing/action变化重捕获、容量/数组identity、warm collector allocation；两1000真实LF2Character无AI局部布局cost）。GREEN新case + 原 PairSnapshotReuse_ 非1000cost 与原RoleSnapshot相关安全case的精确列表；不泛跑历史重成本。
局部cost同进程交错OFF/ON，8warm+16sample，Dispersed按40分组/Concentrated同区，各1000、每25一个attack、2ITR/3body。固定seed、实际Reuse>0、captures≤参与者；报告全collector成本，非实际AI/FPS。只有两布局均有收益信号、payload/RNG/容量/warm0GC通过才在同包扩展准确Driver/窗口路径与cold owner；先事前登记/当前dirty备份，再真实Driver/1000AI对比，不把测试/窗口拆新编号。若局部无收益/错误，保持OFF、收口本候选，复盘下一大块热点，不补1800或刷PASS。

## 范围/留痕/回滚
既有文档与公共XML写域及before逐项SHA见同IDOperation/before-manifest-01.json。治理：Change/Ledger/STATE/handoff/原优化总表同步；不改H07方案正文、PERF/ATLAS/Mono/EXT1/Scene/资源/Settings/Packages/Server/Gen/Plugins/Q06。仅原idle Menu clean8 Editor6402，不启第二Editor。run_tests会覆盖已备份Temp/Goal18_LastTestResults.xml，原件新absent RED/GREEN目录保留。
rollback仅本hunk/dirty副本或准确HEAD blob，实际恢复/删除需用户明确批准；不得reset/checkout/clean/stash/delete/move/commit/push。失败/旧事实追加不抹除。Validate-ChangeLedger.ps1、diff --check与范围外SHA必跑。

GREEN准确filter在首次运行前冻结：RoleAwareCollisionFormalCollectorSelfCheckTests.EndpointSnapshot_；RoleAwareCollisionFormalCollectorSelfCheckTests.PairSnapshotReuse_(?!1000)；RoleAwareCollisionFormalCollectorSelfCheckTests.SnapshotFrameReuse_(?!1000)。只新两1000局部cost，排除31/89旧1000cost。RED实际15具名全部expected missing-property FAIL，运行时现已写；数值/性能结论仍待。

V2事前更正：端点缓存条件收紧为全部formal参与者均exact LF2Character；自定义派生不只是其pair回退，而是整collection回原，防其virtual reader改变其它参与者scalar。custom具名case保持原序列/payload/RNG并assertcompose0。局部成本用2*composed作为baseline endpoint读取数，captured必须更小，因每pair有两端不能直接与composed比较。冷resize移至checked容量校验之后；同39精确GREEN重测一次用于V2，仅两新成本，不重旧31/89/实际AI窗口。

冷容量最终修正事前：V2算法/两局部成本已39/39 PASS（D -0.83%、C -8.81%），分散差异不足单独建立稳定收益；不启动Driver/真实AI/1800本版窗口，不推广。未准入候选不能给默认OFF生产路径额外驻留数组：PrepareBattleCapacity仅显式candidate flag ON才分配这两数组；测试冷预热先ON、Prepare、再OFF保持基线，同时default test断言OFF后容量0；PreparedArrays原case追加seal后增长拒绝/identity不变。此修正只冷准备/默认资源影响，不改V2热循环；最终GREEN精确为 EndpointSnapshot_(?!1000)、PairSnapshotReuse_(?!1000)、SnapshotFrameReuse_(?!1000)，37case，不重复两已证局部成本。V2原cost在候选预热状态相同下复用，不包装为新性能结果。

最终自审补一真实边界：linkedHolder查询只要求World active，不要求它进入formal几何参与者，因此仅formal类型检查仍漏不参与碰撞的custom holder。eligibility改为遍历World active实体，任一未知派生整段禁用；只额外新 EndpointSnapshot_CustomHolderWithoutCollisionFrameDisablesReuse 证明此边界，38非成本GREEN。V2两成本仅旧窄guard版的候选线索，最终World-wide guard实现没有新成本实测，不冒充同版收益；不为这个安全收紧再重复两成本/启动实际AI。

本包限定评估完成：FOCUSED_TEST_PASS / LOCAL_ONLY / NOT_ADMITTED / DEFAULT_OFF；最终38/38、V2两局部cost保留为线索，最终guard版无新成本；不进实际AI窗口/1800、不推广。94已执行完成/73累计，六项5of6/H07 PERFORMANCE_FAIL/34父0/Goal active；下一复用91热点查当前IndexedCanonical producer→freeze→commit整块重复，不重84条件无适用性或93窗口。完整结果/失败/未知见artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH94-ROLE-ENDPOINT-SNAPSHOT-20261009/REPORT.md。

