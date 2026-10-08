# 第74批准备：普通Brute保守深度负拒绝候选

2026-10-08 最新：FOCUSED_TEST_PASS / LOCAL_GAIN_SIGNAL / NOT_ADMITTED。唯一预声明两局部cost已实际2/2 PASS（job8c2a8a20b6744381934b45837bc6f6e0）；Dispersed1000 mean27.8521625→19.5443ms、Combat1000 28.325275→19.6903875ms，包含capture/cache/collect/end与每次重建。只1000合成参与者，不是1000AI完整Driver/FPS/SetPass/0GC认证；候选默认OFF，不重复采cost。后续先中央分段实际取证；Driver资格另精确Task，当前无生产准入。下方COST_PENDING为先前历史，证据见REPORT最新局部成本追加。

历史正确性终态：

2026-10-08 最新状态：FOCUSED_TEST_PASS / COST_PENDING / PERFORMANCE_UNMEASURED / NOT_ADMITTED。新43有效RED后，最终50/50 GREEN（新43＋旧7），job1165145f85884db2854bf464bb8e7f6e；16项新fixture漏geometry开关的首GREEN失败原件保留，三处只修新fixture后通过。生产默认OFF；两局部成本case未启动，Driver/千人/显示/0GC收益均未验证。9备份与597保护已核，原EditorMenu clean8、非Play/idle、filtered CS0。53已执行、阶段4/6、H07/H11 OPEN、Goal active。本轮后续插入用户SetPass诊断，render代码/资源/排序零写；M-02/EXT-1/ATLAS边界不自行解冻。下方为事前历史，不代表当前仍未实施。

2026-10-08 IN_PROGRESS。Change NTSD-OPT-H07-BRUTE-DEPTH-REJECT-074、Operation同Task，9副本已核、597保护、53执行。实施前精确测试矩阵见Change：新43正确性/两cost；GREEN受影响旧7 = BruteCoarseEnvelope_DefaultOffPreservesProductionDefaults(1)、BruteOrdinalPacket_MultiBodyPreservesSequenceRngAndKind4(4)、BruteGeometryFirst_StaleBindingClearMatchesOriginalBaseGate(2)。新43实际计数以XML核实，未来数值不冒充已跑。下方READY为事前历史，由本段覆盖。

READY / IMPLEMENTATION_NOT_STARTED / PERFORMANCE_UNMEASURED。准确依据为[73只读选择](../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH73-PAIR-LOOP-NEXT-ACTION-20261008/REPORT.md)。同已批准H07普通Brute路径，不换collector/default/规则，非EXT1/M0/Mono/ATLAS；本准备不增加执行数。实施前另建Change NTSD-OPT-H07-BRUTE-DEPTH-REJECT-074及同IDOperation，冻结当前dirty副本。

## 准确脚本范围（本Task获实施时只有两文件）

1. Assets/NTSD/Scripts/Animation/Character/BruteForceSceneQuery.cs：候选EnableBruteDepthRejectForDiagnostics默认false；last applied/rejected原primitive统计；CollectCollisionCandidatesBruteForce、TryBuildBruteExactCache、CollectCandidatesForBruteExactDirection；BuildRoleAwareFormalExactAttackCache/BodyCache仅新增默认false的派生参数，在既有遍历计算两个int；RoleAwareFormalParticipant constructor/metadata。原common geometry/helper/PairAllowed/PreserveBruteRejectedBinding/其余collector及所有default不改。
2. Assets/NTSD/Scripts/Test/Editor/RoleAwareCollisionShadowSelfCheckTests.cs：复用现有Register/CreateCharacter、候选序列/RNG/handle比较与balanced成本工具，追加BruteDepthReject具名新测试及两个cost；不删除/改旧案例、Admission/Suite不在本批写域。

新summary取所有non-null exact ITR的max(itr.zwidth>0?itr.zwidth:15)及实际exact body max(max(0,ZWidth))。每方向long delta=target.CollisionZ-attacker.CollisionZ；long radius=maxI+maxB；只delta>radius或delta<-radius拒绝，等号走原exact；没有entry/有效缓存沿原路径。不得Max(int)相加溢出/Abs(int.MinValue)/用Transform或未初始化sourceRule/忽略kind5宽度/将包络命中当接受。

候选只在ForceBrute/emptyguard+roster/exact/geometry完整且其它独立候选、branch/envelope timing OFF时应用；禁止绕过eligibility专用循环、与packet组合。未知/容量不足last applied false、原cache fallback不增长。每collection构建metadata、每次切flag/frame/位置/depth/slot-generation重建；不使用tick stamp代替collection有效性。

负拒绝仍调用原PreserveBruteRejectedBinding，在相同base gate清理失效binding；原i/j与两个方向、eligible survivor完整PairAllowed/ItrAllowed/逐body/candidate snapshot/handles/body source order/20上限、RNG/input/checksum不变。只新增表现无关的collector派生cache，不改逻辑结果、33ms/3ms/max2/十一阶段或publication。

## test-first与成本（实施时先冻结实际case名单）

新测试必须先在缺feature/缺拒绝的当前代码实际RED，再实现GREEN，分别断言：
- 默认OFF/非Brute/缺guard-roster-exact-geometry/其它候选与timer不应用；真实负拒绝计数和survivor完整payload。
- 远Z且XY重叠双方向拒绝；边界±radius及内外1保持原exact结果；int extremes用long，非正ITR默认15/negative body0。
- 宽body/宽ITR以及kind5不会误拒；多个body含宽但XY不重叠者仅false-positive，仍原逐body source order；empty/null/control-only frames不误接受。
- bound/unbound/失效或reused generation、AttackExempt和pending/suppressed与原base cleanup一致；same-tick再collect位置/frame/depth变化不复用旧summary；数组identity/容量fallback不增长。
- 复用旧多body序列/RNG与原4个production-default断言，只受影响窄域，不重跑72/全部历史。

两个局部1000participant cost采用既有canonical Dispersed/Combat初始BuildSpawnPosition作固定XY/Z fixture，不是1000AI运行；4warm+8sample平衡OFF/ON交换，包含每collection cache/summary rebuild，完整序列同、负拒绝实际应用、prepared owner identity保持。资格未通过不能采cost；有可辨局部收益才另准备完整Driver/实景接线Task。无收益/无法辨就不采用，不重复参数/计时窗找PASS、不停止Goal。既有timer关闭，不增热timer。不要把局部cost称FPS/千人0GC/正式性能门。

## 容量、生命周期与证据边界

只给现有participant增加两个int、复用现有owner/cold preparation；逻辑增量8×capacity（1050例8400B），真实stride/bytes未测不得假称预算通过，不重复已失效Marshal/GC接口。禁止热扩容、新数组/owner/队列或修改ATLAS；metadata跟owner BeginBuild/回收重置。Change注明十一阶段既有owner关闭归属，无新增关闭阶段。

可靠完整logic GC、每tickchecksum/RNG/事件、适用正式root witness以及原Scene/central/H11 scope由后继准确资格与实景门判定；本Task不宣称这些已过。当前正式helper与权威不改，不依据可变native源码改 inclusive/itr.z/负宽语义。Q06方法体禁止读取。

实施前核原Editor空闲/编译状态，只用原实例，不启动第二Editor或保存Scene。只C#已有路径，不改Scene/Prefab/resources/importer/Settings/Input/Gen/Plugins/Server，不做删除/覆盖旧原件/破坏Git。回滚由准确副本和owned hunk、另需操作批准。阶段仍H07/H11 OPEN、4/6、Goal active，最终Windows120+1800/P95<33/drop0及可靠0GC/central独立门不缩减。

