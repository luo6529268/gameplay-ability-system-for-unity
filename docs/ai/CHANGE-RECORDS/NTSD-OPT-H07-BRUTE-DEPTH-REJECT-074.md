<!-- CHANGE-RECORD
id: NTSD-OPT-H07-BRUTE-DEPTH-REJECT-074
status: FOCUSED_TEST_PASS
code-path: Assets/NTSD/Scripts/Animation/Character/BruteForceSceneQuery.cs
code-path: Assets/NTSD/Scripts/Test/Editor/RoleAwareCollisionShadowSelfCheckTests.cs
authority: approved H07 continuation under effective bounded-goal sections0-8; formal336 authority exact depth predicate and original binding order unchanged
evidence: artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH74-BRUTE-DEPTH-REJECT-CANDIDATE-20261008/REPORT.md
-->
# 第74批普通Brute保守深度负拒绝

局部成本末审计：2026-10-08T10:27:10.8868475Z，597保护0变化、9准确副本全部SHA核同、两C# postSHA保持、HEAD/staged不变；原EditorMenu savedclean8/nonPlay/idle/nottests/filtered CS0。实际validator exit0/1370Records/4governed当前diff全covered，4323历史warning未清理；准确diff --check exit0。详cost-audit-01.json；不把局部收益或本审计当作H07/H11完成。

## 最新局部成本终态（2026-10-08，覆盖上方COST_PENDING）

FOCUSED_TEST_PASS / LOCAL_GAIN_SIGNAL / NOT_ADMITTED。没有新的C#、Scene、资源、插件或渲染排序写入；只在原Editor6402运行本Task预声明唯一两cost，没有再采正确性GREEN或重复成本。

- 请求：NTSD.Test.RoleAwareCollisionFormalCollectorSelfCheckTests.BruteDepthReject_1000ParticipantsBalancedCostAndFullSequence(Dispersed1000,Combat1000)。原test4warm＋8sample平衡交换OFF/ON，包含capture/cache/collect/end与每collection summary rebuild；不是AI runtime。
- job8c2a8a20b6744381934b45837bc6f6e0 succeeded，实际2/2 PASS、0skip，3.3394184s。完整序列/RNG/handles一致、真实depth负拒绝应用、prepared arrays identity保持断言通过。既有50/50与失败原件均保留。
- Dispersed1000：OFF mean27.8521625ms、ON mean19.5443ms（减少29.8284289416%）；median27.5078→19.5252ms（减少29.0194054050%）。
- Combat1000：OFF mean28.325275ms、ON mean19.6903875ms（减少30.4847437492%）；median28.22755→19.47335ms（减少31.0129642849%）。
- 原始8组样本保存在cost-summary-01.json；请求/终态cost-job-01.json；公共XML2026-10-08T10:16:28.4689889Z经actual-case核验后按准确ABSENT目标归档cost-results-01.xml，源/副本SHA相同：696483D8FA67C63CE13305144A0E74A1C49D57DA4A228C1926F48F88A60D932F。
- 本数值只能作为局部收益信号；候选默认OFF。每tick完整Driver/RNG/checksum、正式可用witness、1000AI窗口、完整logic GC、H11 scope、容量内存仍由后继门验收，不把此结果称性能门通过，不宣称SetPass下降。
- 用户SetPass质疑的下一优先仍是中央实际资源/模式/分段断点。尝试原Editor只读execute_code读取既有manager与DrawPolicy时，snippet编译前CodeDom命令长度os206拒绝，未执行；不能从失败推定当前AtlasPolicy或DrawMode，不安装Roslyn/修改插件/重启Editor来绕过。既有report缺少完整tuple与break原因，需要准确限定后继取证，不猜根因。
- 53已执行、阶段4/6、H07/H11 OPEN、Goal active。EXT-1保持PROPOSED/MODIFY_REQUIRED，无专项M0/instancing；ATLAS/Mono专项边界和生产默认不动。当前最终保护/Editor/validator审计另追加。


最新：FOCUSED_TEST_PASS / LOCAL_GAIN_SIGNAL / NOT_ADMITTED。预声明两局部cost实际2/2 PASS；均值27.8521625→19.5443ms / 28.325275→19.6903875ms，scope与完整序列/RNG/array-identity断言见REPORT末节。默认OFF；无Driver/千人AI/完整0GC/FPS/SetPass收益或生产准入结论。下方COST_PENDING为历史事实，不重跑50 GREEN或两cost，不改变源代码/collector/default。

历史正确性终态：

当前：FOCUSED_TEST_PASS / COST_PENDING / PERFORMANCE_UNMEASURED / NOT_ADMITTED。GREEN2为50/50，cost两case未运行；默认OFF。2026-10-08T10:05:12.3643140Z核验597/597保护相同、9/9事前准确副本相同，源SHA Query1FF48CA62EE7DB21F3956D48B9FDF59D8BD96EC3936E969EE2C23A8B9758BD8B、testsEE755D691AF92B396C4F0730AFBE2B45401F5ABF72F4B58E94766042FAC842AA；Menu savedclean8/idle非Play、filtered CS0。后续validator/diff审计另追加。不将正确性通过解释成性能收益或生产准入。

历史test-first准备（以下保留）：

IN_PROGRESS / TESTS_WRITTEN / RED_PENDING / PERFORMANCE_UNMEASURED / NOT_ADMITTED。已追加13具名correctness方法43case、一个cost方法2case与冷helpers，只tests；Query未改。真实RED/编译尚待。Task docs/ai/TASKS/NTSD-OPTIMIZATION-BATCH74-BRUTE-DEPTH-REJECT-CANDIDATE-20261008.md；唯一Change NTSD-OPT-H07-BRUTE-DEPTH-REJECT-074。原Editor6402/PID78296/Menu savedclean8/nonPlay/idle，Unity2022.3.62f3。53已执行含本批、阶段4/6、H07/H11 OPEN、父关闭0、Goal active。

## 需求与原状

FOCUSED_TEST_PASS：只补新fixture三处geometry=true后，同50case job1165145f85884db2854bf464bb8e7f6e终态50/50 PASS（新43＋旧7），时长2.8977103s，原件green-results-02.xml/green-job-02.json。原16FAIL/43RED保留，没有改旧断言或生产helper。默认OFF，本批cost尚未执行，实景/FPS/GC收益未知。用户SetPass质疑已静态核backend一chunk仍约1990segment、每segment一DrawMesh；根因分类待核，不以CPU命令推SetPass或私自解冻instancing/ATLAS。

CODE_WRITTEN：有效43RED之后，只Query上述路径新增默认OFF gate/每collection reset与两个int bounds、原序严格负拒绝+binding清理。optional参数默认false，旧caller/其它candidate不变。无新热timer/array/owner；原helper未改。候选编译/GREEN与收益仍待，尚未准入。
用户要求持续完成已批准H07，不以报告/次数替代优化。73静态确定现有XY coarse无Z提前拒绝，但约51ms PairExactLoop还含roster/cache。只新增默认OFF的保守depth负证明，不换collector或default，不改BodyDepthOverlaps严格谓词/规则/RNG/序列。Query before394EF397D3A5C8C66635855D3D8CC98A0854C1EE9F80BED533C5699DEC33FAD8；tests before6EF06591A28B9DBD8A8969D4B6BF66D640C906D98C6E3028248B934438F7C941；准确副本/当前dirty9路径及593保护见Operation before-manifest。

## 精确代码路径/符号与副作用

有效RED已归档：job9895d7a0537b40a79ed96a636bf92b0b实际43/43 failed，XML09:47:10.2905145Z，SHA A1DBEA07C39A95BC71CCAEB2E67BCFA6E340581DE72C4AF3D229E90860082BF2，逐case为缺EnableBruteDepthReject opt-in。原Editor编译errorCS0；不能用catalog10085当actual。tests准确43与冻结一致，Query尚为before394E。RED terminal结果字段null但status/progress终态和fresh具体XML断言可审计，原件保持。
Query仅默认false EnableBruteDepthRejectForDiagnostics、Last applied/rejected primitive；每collection reset/gate、TryBuildBruteExactCache optional depth派生；BuildRoleAwareFormalExactAttackCache/BodyCache两个默认false参数和既有循环的max；RoleAwareFormalParticipant两个int/constructor；CollectCandidatesForBruteExactDirection在原序负拒绝但保留PreserveBruteRejectedBinding。geometry/common/exact helper、各独立candidate/default及其它collector不变。
two int=max所有nonnull ITR有效zwidth（正值或15）及实际IsReleaseBody非负ZWidth。long delta/radius，只有严格超过上界拒绝，等号与没有entry沿原exact。所有其它candidate/timer OFF、guard/roster/exact/geometry有效普通Brute且完整cache时应用。每collection重建，容量失败原fallback不扩容。
tests仅追加以下具名方法和冷helpers，不改旧案例。透明/渲染/SetPass不减少。
现有participant owner冷preparation/BeginBuild/CompleteBuild与World-owned cache关闭归属不变，无新增worker/queue/cache owner。逻辑增量8×capacity，实际stride/bytes未测UNKNOWN；无热路径数组/反射/timer/new owner/增长。

## 冻结矩阵
RED只新correctness，GREEN新correctness＋旧默认/多body/geometry窄域；cost只有正确性GREEN后两个case。新方法：
BruteDepthReject_DefaultOffPreservesProductionDefaults [1]；
BruteDepthReject_BoundaryAndWidths [12]；
BruteDepthReject_WideBodyAndItrKeepOriginalExact [3]；
BruteDepthReject_GapFalsePositiveStillUsesEachBody [1]；
BruteDepthReject_BothDirectionsUseIndependentDepth [1]；
BruteDepthReject_InactiveGateKeepsOriginalPath [14]；
BruteDepthReject_StaleBindingKeepsBaseGate [2]；
BruteDepthReject_RebuildsSameTickDepthAndFrames [1]；
BruteDepthReject_CapacityFallbackKeepsPreparedOwners [2]；
BruteDepthReject_EmptyNullAndControlRemainConservative [3]；
BruteDepthReject_PendingAndSuppressedRemainExcluded [1]；
BruteDepthReject_TwentyLimitAndSourceOrder [1]；
BruteDepthReject_ReusedSlotRebuildsHandleAndDepth [1]。
合计43新correctness，actual counts以fresh XML核实，有误只能记录更正，不冒充全catalog。GREEN旧7已在实施前按真实方法冻结：BruteCoarseEnvelope_DefaultOffPreservesProductionDefaults(1)、BruteOrdinalPacket_MultiBodyPreservesSequenceRngAndKind4(4)、BruteGeometryFirst_StaleBindingClearMatchesOriginalBaseGate(2)。本句取代下文待确认旧矩阵说明；原说明只是bootstrap时点。
cost BruteDepthReject_1000ParticipantsBalancedCostAndFullSequence(Dispersed1000,Combat1000) [2]，canonical BuildSpawnPosition，1000participant/40attacker，4warm+8sample交换OFF/ON，含capture/cache/summary rebuild/collect/end，fullsequence/RNG/handle相同、数组identity同。不是实际AI/FPS/GC或正式native门，收益不可辨不继续推广，不重采寻找PASS。旧矩阵冻结为BruteCoarseEnvelope_DefaultOffPreservesProductionDefaults [1]、BruteOrdinalPacket_MultiBodyAndFullPayloadRemainIdentical(当前真实方法名待静态确认后固定，不猜)；实施前使用真实现有方法名，禁止改旧断言。

## 验收/回滚/边界

首次GREEN job9d48a6acd40d49a8a85ca71a433bf495终态50actual/34pass/16fail，09:50:38.7672790Z XML SHA0B10C2E76C508A230244A9C4B52E934FCCC1764B557569580D9FEF7F84E344E3独立green-failed-01.xml保留。失败为newfixture未显式启用geometry（既有GetQuery诊断helper关闭该生产flag），候选未应用，不能称GREEN或采cost；下一只核helper并补本批fixture明确geometry，旧断言不动。用户转问SetPass渲染实现，read-only先核backend/segment/绑定，不解冻专项。

首次仅tests导入报CS0246 CandidateRun：本文件有Shadow和FormalCollector两fixture，新段误插前者导致helper不可见；只移动本批新增段至已有RoleAwareCollisionFormalCollectorSelfCheckTests，旧全文保持，不改断言/规则。这是fixture接入修正，尚非有效RED。预声明实际fullyqualified均NTSD.Test.RoleAwareCollisionFormalCollectorSelfCheckTests，group regex新BruteDepthReject_且排除cost。另manifest四150分片尾4已先核在guard-addendum，实际597保护；末审计必须合并，不漏GraphicsSettings/ProjectVersion/公共stress路径。
新tests先实际缺feature RED，再候选GREEN、受影响旧矩阵、唯一两cost；成功后才后继Driver/实景Task。本批无Admission/Suite/Scene/Prefab/DAT/资源/Settings/Input/Gen/Plugins/Server写入，不读Q06体，不解冻EXT1/ATLAS/Mono/Role-aware，不做Git写/删除/清理。回滚来自9准确当前副本和owned hunk，恢复另需批准，不覆盖任何用户dirty。Validate-ChangeLedger及diff/guards必须过；实际命令/结果随后追加。
H07最终120+1800/P95<33/drop0/reliableLogicGC/central独立门、H11原完整scope0GC未达仍保持；不将此局部候选称完成。


## 末审计追加

2026-10-08实际 Tools/Validate-ChangeLedger.ps1 exit0，Records1370、当前governed code diff4均covered；本批仅Query/tests两源，其余既有Suite/Admission改动未触碰。大量历史Record非当前diff warnings保持，不顺手清理。准确范围git diff --check exit0；CRLF提示非error。audit-01.json记录597保护0变化、9备份全同、两源postSHA、GREEN50/50与cost未运行。原Editor只读get_editor_state/manage_scene get_active/read_console filtered error CS均success，Menu savedclean8/nonPlay/idle/nottests、CS0。仅可报告窄正确性通过，完整Driver/成本/千人/FPS/SetPass收益/0GC和Android验收均未完成。用户SetPass问题的本次新增核验只读，没有改renderer代码/资源/排序或启动新的测量。

