<!-- CHANGE-RECORD
id: NTSD-OPT-H07-BRUTE-DEPTH-DRIVER-076
status: CODE_WRITTEN
code-path: Assets/NTSD/Scripts/Test/Editor/BattleBruteProductionAdmissionEditorTests.cs
authority: approved H07 continuation under bounded-goal effective sections0-8; formal336 authority and original Driver order unchanged
evidence: artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH76-BRUTE-DEPTH-DRIVER-20261008/REPORT.md
-->
# 第76批：深度拒绝候选完整Driver资格

覆盖判定修正已写：两个新pure guard先经原Editor有效RED 2/2（jobab704ddeb303466aa1faeed59e465ba0，缺显式4参数，XML4630A1...CEEC8），随后仅CompareDepthRejectDriverRows新增requireDepthProof默认false和1000case按Combat强制proof，并显式归档COVERED/NO_REJECT_COVERAGE；checksum/RNG/AI/方向/数组容量/规则未改。第一轮8的真实7PASS/1FAIL保留，恢复备份09EA...67DE；GREEN只失败Dispersed＋新2guard共3，原Combat正覆盖sum26与旧case不重跑。未有新收益。

覆盖合同修正事前声明（同76/IN_PROGRESS，未放宽规则比较）：首轮实测7/8，唯一失败是Dispersed32tick负拒绝数为0的覆盖要求；此前全部逐tick一致性/两RNG/AI1000/prepared容量已通过该case，Combat32tick明确正覆盖sum26/max26。修订资格合同，不更改actualAI/布局/input/tick：独立区分正确性与分支覆盖，Dispersed允许NO_REJECT_COVERAGE并不得宣称该窗有深度收益；Combat仍强制至少一次真实负拒绝。先新增CoverageContractKeepsParityGuards(bool requireProof)两pure comparer测试，以缺四参数比较入口有效RED；再仅新增显式requireDepthProof参数和千人调用按Combat要求proof，保留所有比较/prepare/Run/default/源规则及原7通过case。两个守卫检查零覆盖允许/强制失败、正覆盖通过且checksum/nativeScalar/AI漂移仍失败，防止因修覆盖而豁免等价。修后最窄复验只有2guard＋原失败Dispersed共3，不重跑已通过Combat/两root；合并资格为10个唯一case，首轮失败保留，实际GREEN3结果未产生前不标通过。SaveNew输出带coverage标识仍UUID/CreateNew。脚本仍仅Admission，生产候选OFF；这不是H07/0GC/FPS或所有分散情形优化认证。

恢复验收追加（IN_PROGRESS / DRIVER_RUNNING_PREP / NOT_ADMITTED）：2026-10-08 用户已通知“Unity项目已经启动，并且空闲，你可以继续你的处理了”，撤销仅纯脚本阶段的Editor等待。仅继续既定76资格，不新增资源/ATLAS/生产推广授权。原NTSD Editor PID74724/6402、Unity2022.3.62f3，菜单唯一loaded Scene clean/8roots/非Play/idle/noTests，error-CS Console0；源码09EA6B...67DE、HEAD及602准确保护重扫0变化。第一个恢复guard包装读错manifest字段给出count1，不采用；展开manifest及SHA核验后实际602/0。内联602 JSON因Windows命令长度被拒，零执行；改为现有文件递归只读。不安装Roslyn/Pipeline/插件，execute_code缺action/现无Roslyn两次请求都未执行snippet，使用现有场景接口。上轮额外validator包装执行已终止但exit未捕获，不冒充PASS；采用此前确实完成的1372/4/exit0原件。现在只一次8冻结GREEN；无实际RED、不删实现重造RED、不启动千人耗时窗口/SetPass/0GC，资格结果与后继Task分别记账。公共Temp XML事前仍absent、旧75归档不改；正常Callback产物出现后核freshness/case/job并Copy至ABSENT green-results-01.xml，不覆盖历史。测试前所有loaded scene clean，避免MCP SaveDirtyScenes触发保存。

末审计/交付追加：scope-audit-01.json（2026-10-08T11:35:39Z）602保护0变化、6before＋1tests-only共7副本SHA同、原test/helper段与旧tail去除新增DTO字段后全文保持，source09EA6B...67DE/HEAD0e580f7bf94a7d645958f8dbace80a9abf9a4ae1/staged空。实际validator exit0/1372Records/4scriptdiff全covered，4323历史warning未清理；准确diffcheck0。旧源/rsp/compiler DLL hash逐项在scope audit。两次只读包装失败（Git LF警告混入JSON、PS数组concat未括号）已纠正为准确literal paths，非真实文件缺失；一次组合patch重复tracker目标拒绝零应用，改为同一target两hunk，未覆盖/删除/恢复任何用户内容。最终CODE_WRITTEN/ISOLATED_CSHARP_COMPILE_PASS/UNITY_DRIVER_PENDING/NOT_ADMITTED，Operation PARTIAL；没有effective RED、8case或性能/0GC结果。用户通知前不操作Unity/test/测量、不重复催问，候选OFF；55执行（本批仅pure脚本）、阶段4/6/H07H11OPEN/Goalactive，不把本批收口当阶段完成。

最新 CODE_WRITTEN / ISOLATED_CSHARP_COMPILE_PASS / UNITY_DRIVER_PENDING / NOT_ADMITTED：按用户纯脚本方向，先追加5case并保存缺入口source63303A2574E279AC227149BDE37BB6087595BC921BE2C53105C2CBA1783E57AD，再实现RunDepthRejectDriver/nullable Run.depthReject（最后compose参数保留）/prepared三数组identity与三容量断言/冷TickRow depth字段；postSHA09EA6B464281C7A4651113D9009EB37CF501D60459F52742083B0265AED467DE。旧CompareEveryTick/CompareRoot/PrepareThousandAi/SetCandidate/SaveNew与旧case未改；原Run未请求depth不执行新增分支，冷结果schema仅加默认false/0字段。Query/Role/Suite/default不改。

实际隔离C#编译2026-10-08T11:31:47.7667520Z exit0/0error/6既有warning：Unity随附NetCoreRuntime dotnet＋DotNetSdkRoslyn csc，复用1220行既有Editor rsp，仅out/refout重定向到本批static-compile-01，保留defines/refs/analyzers，结果isolated-compile-01.json。没有启动Unity/测试/Profiler/测量；effective RED未运行，8case Driver资格全部待用户通知再验，不称Unity COMPILE_PASS或Runtime/收益/0GC通过。6before＋tests-only副本保留；602保护/原差异及validator审计随后追加。阶段4/6/H07H11OPEN/Goalactive。

追加用户方向：先纯脚本、等用户通知再操作Unity。替代下文原实时RED前置：新5test-first先保存准确缺入口源状态、再pure C# RunDepthRejectDriver接线；实际RED/Driver/Unity编译均未运行，未来直接8窄case验收，不删实现重造RED。候选/default/范围不扩。

PLANNED / TEST_FIRST_PENDING / DRIVER_PENDING / NOT_ADMITTED。修改脚本前创建本Record及Task/Operation/索引，准确写域仅Admission现有Editor测试。需求是74局部gain不足以证明完整Driver等价，不重复有效74/71/75取证。

原状/职责：dirty Admission before SHA88186BA3B1AEFC3F314E8459989A642B13ED403332BFB18871F2735B253EABA0，已有完整Run/shared Driver与71packet资格，尚无depth入口；74 Query/Role源码及75Suite只保护。改后拟新增5test-first、显式depth Driver private入口/nullable Run参数/冷TickRow字段，旧case/断言/default/最后compose参数不变。有效RED前仅tests；不生产默认、不Query/Suite/render/资源改动。

权威及副作用：336B44正式EXE身份、本轮重核两root traces，只既有oid/action/hp/vx打印精度证据；新4对12/12/32/32tick候选OFF/ON同完整Driver，checksum/两完整RNG scalar state/entity/actualAI/原访问计数/数组容量identity/noFallback/真实depthReject及11stage三0。保持33/3/max2和原binding/source序/规则。测试冷分配/序列化不能当0GC或性能收益。

owner/资源/不可回退边界：旧临时Driver/World/logic pool并十一阶段清理，无新owner/worker。74派生depth字段容量不改，实际bytes未知，steady/transition不改；EXT1/ATLAS/Mono/Q06受保护。恢复准确dirty副本或owned hunk另获批准，不以HEAD代替用户改动。

验收：新5有效RED再新5＋旧默认及request0/1共8GREEN，四组88配对/176step；不重跑74cost/71packet/75诊断，不启动Windows或GPU/M0。原Editor现已无Unity进程/6402拒绝，前置等待用户重开；先可写测试，编译/RED/Driver未知。602 guard和6准确备份、Ledger/diff必须核。命令及实际状态后追加，不冒充未运行证据。

54执行＋76准备（累计55）、阶段4/6/H07H11OPEN/Goalactive。Task：../TASKS/NTSD-OPTIMIZATION-BATCH76-BRUTE-DEPTH-DRIVER-20261008.md；Operation：../FILE-OPERATIONS/NTSD-OPTIMIZATION-BATCH76-BRUTE-DEPTH-DRIVER-20261008/RECORD.md。
