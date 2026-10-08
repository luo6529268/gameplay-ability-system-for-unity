# 第76批：深度粗筛完整Driver资格

覆盖合同修正事前声明（同76/IN_PROGRESS，未放宽规则比较）：首轮实测7/8，唯一失败是Dispersed32tick负拒绝数为0的覆盖要求；此前全部逐tick一致性/两RNG/AI1000/prepared容量已通过该case，Combat32tick明确正覆盖sum26/max26。修订资格合同，不更改actualAI/布局/input/tick：独立区分正确性与分支覆盖，Dispersed允许NO_REJECT_COVERAGE并不得宣称该窗有深度收益；Combat仍强制至少一次真实负拒绝。先新增CoverageContractKeepsParityGuards(bool requireProof)两pure comparer测试，以缺四参数比较入口有效RED；再仅新增显式requireDepthProof参数和千人调用按Combat要求proof，保留所有比较/prepare/Run/default/源规则及原7通过case。两个守卫检查零覆盖允许/强制失败、正覆盖通过且checksum/nativeScalar/AI漂移仍失败，防止因修覆盖而豁免等价。修后最窄复验只有2guard＋原失败Dispersed共3，不重跑已通过Combat/两root；合并资格为10个唯一case，首轮失败保留，实际GREEN3结果未产生前不标通过。SaveNew输出带coverage标识仍UUID/CreateNew。脚本仍仅Admission，生产候选OFF；这不是H07/0GC/FPS或所有分散情形优化认证。

恢复验收追加（IN_PROGRESS / DRIVER_RUNNING_PREP / NOT_ADMITTED）：2026-10-08 用户已通知“Unity项目已经启动，并且空闲，你可以继续你的处理了”，撤销仅纯脚本阶段的Editor等待。仅继续既定76资格，不新增资源/ATLAS/生产推广授权。原NTSD Editor PID74724/6402、Unity2022.3.62f3，菜单唯一loaded Scene clean/8roots/非Play/idle/noTests，error-CS Console0；源码09EA6B...67DE、HEAD及602准确保护重扫0变化。第一个恢复guard包装读错manifest字段给出count1，不采用；展开manifest及SHA核验后实际602/0。内联602 JSON因Windows命令长度被拒，零执行；改为现有文件递归只读。不安装Roslyn/Pipeline/插件，execute_code缺action/现无Roslyn两次请求都未执行snippet，使用现有场景接口。上轮额外validator包装执行已终止但exit未捕获，不冒充PASS；采用此前确实完成的1372/4/exit0原件。现在只一次8冻结GREEN；无实际RED、不删实现重造RED、不启动千人耗时窗口/SetPass/0GC，资格结果与后继Task分别记账。公共Temp XML事前仍absent、旧75归档不改；正常Callback产物出现后核freshness/case/job并Copy至ABSENT green-results-01.xml，不覆盖历史。测试前所有loaded scene clean，避免MCP SaveDirtyScenes触发保存。

末审计/交付追加：scope-audit-01.json（2026-10-08T11:35:39Z）602保护0变化、6before＋1tests-only共7副本SHA同、原test/helper段与旧tail去除新增DTO字段后全文保持，source09EA6B...67DE/HEAD0e580f7bf94a7d645958f8dbace80a9abf9a4ae1/staged空。实际validator exit0/1372Records/4scriptdiff全covered，4323历史warning未清理；准确diffcheck0。旧源/rsp/compiler DLL hash逐项在scope audit。两次只读包装失败（Git LF警告混入JSON、PS数组concat未括号）已纠正为准确literal paths，非真实文件缺失；一次组合patch重复tracker目标拒绝零应用，改为同一target两hunk，未覆盖/删除/恢复任何用户内容。最终CODE_WRITTEN/ISOLATED_CSHARP_COMPILE_PASS/UNITY_DRIVER_PENDING/NOT_ADMITTED，Operation PARTIAL；没有effective RED、8case或性能/0GC结果。用户通知前不操作Unity/test/测量、不重复催问，候选OFF；55执行（本批仅pure脚本）、阶段4/6/H07H11OPEN/Goalactive，不把本批收口当阶段完成。

最新纯脚本终态：CODE_WRITTEN / ISOLATED_CSHARP_COMPILE_PASS / UNITY_DRIVER_PENDING / NOT_ADMITTED。用户要求先pure脚本、待其通知，不再发Editor命令/test/测量。先追加5具名test并保存准确缺入口源副本（63303A2574E279AC227149BDE37BB6087595BC921BE2C53105C2CBA1783E57AD），再在唯一Admission写域实现RunDepthRejectDriver、nullable depthReject（最后compose参数/旧caller保持）、冷ScalarRNG/AI1000/depth diagnostics以及prepared三数组identity/三容量逐tick断言；post09EA6B464281C7A4651113D9009EB37CF501D60459F52742083B0265AED467DE。未启动候选生产推广。

2026-10-08T11:31:47.7667520Z实际隔离Editor assembly C#编译exit0/0error/6既有warning。使用Unity随附NetCoreRuntime dotnet与DotNetSdkRoslyn csc，原1220行rsp仅out/refout指向本批ABSENT static-compile-01，所有defines/refs/analyzers保留，未写Library/Scene/Settings。记录isolated-compile-01.json；不把它当Unity import/Driver8case/实际RED或performance证据。旧case/helpers/hash/guard/备份/validator末审计随后追加。55已执行（本批仅pure脚本）、阶段4/6，H07/H11OPEN/Goalactive；SetPass1995未降。

PLANNED / TEST_FIRST_PENDING / DRIVER_PENDING / NOT_ADMITTED。74局部收益约30%仅是信号；本批验证同普通Brute完整Driver，唯一Editor Admission写域，不切生产默认/collector，不处理被hold的SetPass资源方案。

精确Task与Change先建，原dirty source88186BA3...EABA0、602保护/6存在副本；原Editor进程0/6402拒绝（2026-10-08T11:22:49Z），无作业，不启动替代。公共Temp旧XML不存在，旧75归档存在；只记录当前事实，不恢复或猜删除原因。

冻结新5：default/explicit1、right/left各12tick两case、Dispersed/Combat各32tick两case；有效RED后实现，再5新＋3默认/request旧case共8GREEN。88配对/176step，完整声明checksum域/两RNG完整scalar state/AI1000/实际depth负拒绝/准备数组identity/无fallback/十一阶段三0；root证据只既有四字段。无需重跑74cost/71packet/75诊断或历史全量。

当前没有本批脚本/编译/测试通过或性能证据；完成测试准备后如实更新TESTS_WRITTEN/RED_PENDING。55累计（54执行＋本76准备）、阶段4/6，H07/H11OPEN/Goalactive；SetPass最新1995未降，ATLAS修复等用户准确授权，其他既有工作继续。
