# 第76批：保守深度负拒绝完整 Driver 资格

覆盖合同修正事前声明（同76/IN_PROGRESS，未放宽规则比较）：首轮实测7/8，唯一失败是Dispersed32tick负拒绝数为0的覆盖要求；此前全部逐tick一致性/两RNG/AI1000/prepared容量已通过该case，Combat32tick明确正覆盖sum26/max26。修订资格合同，不更改actualAI/布局/input/tick：独立区分正确性与分支覆盖，Dispersed允许NO_REJECT_COVERAGE并不得宣称该窗有深度收益；Combat仍强制至少一次真实负拒绝。先新增CoverageContractKeepsParityGuards(bool requireProof)两pure comparer测试，以缺四参数比较入口有效RED；再仅新增显式requireDepthProof参数和千人调用按Combat要求proof，保留所有比较/prepare/Run/default/源规则及原7通过case。两个守卫检查零覆盖允许/强制失败、正覆盖通过且checksum/nativeScalar/AI漂移仍失败，防止因修覆盖而豁免等价。修后最窄复验只有2guard＋原失败Dispersed共3，不重跑已通过Combat/两root；合并资格为10个唯一case，首轮失败保留，实际GREEN3结果未产生前不标通过。SaveNew输出带coverage标识仍UUID/CreateNew。脚本仍仅Admission，生产候选OFF；这不是H07/0GC/FPS或所有分散情形优化认证。

恢复验收追加（IN_PROGRESS / DRIVER_RUNNING_PREP / NOT_ADMITTED）：2026-10-08 用户已通知“Unity项目已经启动，并且空闲，你可以继续你的处理了”，撤销仅纯脚本阶段的Editor等待。仅继续既定76资格，不新增资源/ATLAS/生产推广授权。原NTSD Editor PID74724/6402、Unity2022.3.62f3，菜单唯一loaded Scene clean/8roots/非Play/idle/noTests，error-CS Console0；源码09EA6B...67DE、HEAD及602准确保护重扫0变化。第一个恢复guard包装读错manifest字段给出count1，不采用；展开manifest及SHA核验后实际602/0。内联602 JSON因Windows命令长度被拒，零执行；改为现有文件递归只读。不安装Roslyn/Pipeline/插件，execute_code缺action/现无Roslyn两次请求都未执行snippet，使用现有场景接口。上轮额外validator包装执行已终止但exit未捕获，不冒充PASS；采用此前确实完成的1372/4/exit0原件。现在只一次8冻结GREEN；无实际RED、不删实现重造RED、不启动千人耗时窗口/SetPass/0GC，资格结果与后继Task分别记账。公共Temp XML事前仍absent、旧75归档不改；正常Callback产物出现后核freshness/case/job并Copy至ABSENT green-results-01.xml，不覆盖历史。测试前所有loaded scene clean，避免MCP SaveDirtyScenes触发保存。

末审计/交付追加：scope-audit-01.json（2026-10-08T11:35:39Z）602保护0变化、6before＋1tests-only共7副本SHA同、原test/helper段与旧tail去除新增DTO字段后全文保持，source09EA6B...67DE/HEAD0e580f7bf94a7d645958f8dbace80a9abf9a4ae1/staged空。实际validator exit0/1372Records/4scriptdiff全covered，4323历史warning未清理；准确diffcheck0。旧源/rsp/compiler DLL hash逐项在scope audit。两次只读包装失败（Git LF警告混入JSON、PS数组concat未括号）已纠正为准确literal paths，非真实文件缺失；一次组合patch重复tracker目标拒绝零应用，改为同一target两hunk，未覆盖/删除/恢复任何用户内容。最终CODE_WRITTEN/ISOLATED_CSHARP_COMPILE_PASS/UNITY_DRIVER_PENDING/NOT_ADMITTED，Operation PARTIAL；没有effective RED、8case或性能/0GC结果。用户通知前不操作Unity/test/测量、不重复催问，候选OFF；55执行（本批仅pure脚本）、阶段4/6/H07H11OPEN/Goalactive，不把本批收口当阶段完成。

当前纯脚本交付：CODE_WRITTEN / ISOLATED_CSHARP_COMPILE_PASS / UNITY_DRIVER_PENDING / NOT_ADMITTED。新5tests先保存缺入口source63303A...E57AD，再实现显式depth入口/旧Run可选mode/冷诊断和三prepared数组/容量保护，post09EA6B...67DE。隔离1220行原Editor rsp只改out/refout，Unity自带csc exit0/0error/6既有warning，非Unity导入或Driver验收；actual RED与8case均未运行。用户当前禁止提前Editor验收，待其通知再执行。不新增窗口/生产默认/资源授权，旧规则和第75批SetPass结论不变。

用户本轮更正（2026-10-08）：先处理纯脚本逻辑，其处理完其他工作后会通知；此要求优先于下文原计划的实时RED前置。现在不再操作Unity/运行任何测试测量，先追加5测试并保存缺新入口的准确源状态，再接纯C# Driver资格实现。有效RED未运行不能报告RED，通过静态或隔离编译也不能标Driver PASS；用户通知后只运行8冻结case，不删实现重新制造RED、不额外扩大矩阵。生产candidate仍OFF。下文原时点计划保留；新增纯脚本授权不解冻资源/ATLAS或批准生产推广。

PLANNED / TEST_FIRST_PENDING / DRIVER_PENDING / NOT_ADMITTED。有效执行范围来自有限首阶段合同第0—8节；次数是审计信息，不是停止边界。上一交互轮只是回答SetPass及重读既有证据，无新的代码或测量，本轮转到已有安全必要动作。第75批根因诊断已收口，不重复；资源/ATLAS共享绑定修复未获新增授权，只限制该修复，不暂停Goal。

## 需求、准确写域与依赖

第74批局部成本两case有约30%信号，但没有完整Driver逐tick资格。本批只改 `Assets/NTSD/Scripts/Test/Editor/BattleBruteProductionAdmissionEditorTests.cs`，dirty before SHA `88186BA3B1AEFC3F314E8459989A642B13ED403332BFB18871F2735B253EABA0`，保留全部旧case/helper/断言。Query/Role测试/WindowsSuite/Host/Kernel/native/render/Q06/资源不写；不读Q06活跃方法体。Change `NTSD-OPT-H07-BRUTE-DEPTH-DRIVER-076`；Operation 同本Task ID。

既有Driver/WithLoganScenarioForReplayTests/PrepareThousandAi/CompareEveryTick/CompareRoot/SaveNew可复用。只新增显式RunDepthRejectDriver入口、可选nullable depthReject（保留Run最后compose参数及旧调用）、冷TickRow诊断；未请求本批的旧Run行为不变。四admitted fast flags保持true；普通ForceBruteForce，baseline depth=false/candidate=true；ordinal/kind5/eligibility/coarseEnvelope/proof/rejectedBinding/dispatch/timing均false，任何未声明组合拒绝资格。不更改候选默认false、不改任何生产C#。

当前74 Query SHA `1FF48CA62EE7DB21F3956D48B9FDF59D8BD96EC3936E969EE2C23A8B9758BD8B`；Role tests `EE755D691AF92B396C4F0730AFBE2B45401F5ABF72F4B58E94766042FAC842AA`；75Suite `43ECB5B1FA0570A6E5B413CE8FB28EC371B8255879978E0B5B2EC10254883D8C`保持。本轮已核FormalSha336B44...7BD3及两root trace SHA `AA327E22F85000294699EA689795E90B02EFF38194954294CE41024D1D415FBE` / `CF2701700E543AB7C37B132D1D1A0337DB1814C6440846FC8F16C6EAAD5BB9B2`；规则33ms/3ms/max2、RNG/输入/绑定/20上限/源序及关闭不变。

## 冻结矩阵与test-first

新5case，全类 `NTSD.Test.Editor.BattleBruteProductionAdmissionEditorTests`：

- DepthRejectDriver_DefaultOffAndExplicitQualificationOnly：1。
- DepthRejectDriver_PreservesApplicableFormalRootAndEveryTick：right-x550 / left-x350各12tick OFF/ON，共2。
- DepthRejectDriver_ThousandCanonicalAiPreservesEveryTick：Dispersed1000 / Combat1000各32tick OFF/ON，共2。

先新增5具名测试与反射/比较helper，RunDepthRejectDriver尚缺；原Editor实际RED证明缺入口后才接入Run/冷结果字段，编译错误不是有效RED。GREEN只新5＋必要旧3：OrdinaryBruteDefaultsEnableAdmittedFastPath、ProductionRequestDoesNotChangeFrozenWorkloadOrEnableCandidate(0/1)，总8。不重复74的50case/两cost、71packet资格、75分段诊断，不运行历史全量/native campaign。

四对固定输入合计88配对tick/176完整Driver step。逐tick保持完整声明checksum域、lockstep、entities、native/legacy calls与完整NativeRandom scalar state/legacy State；actualAI同且两千人每tick为1000；candidate真实depthApplied、真实负拒绝>0，OFF全0；exact cache无fallback、生产其他candidate不继承。内部几何拒绝统计不单独定义规则权威，但原方向访问计数须保持。

实际prepared participant/body/ITR owners引用与容量每tick不变、无截断/热增长；派生depth每collection由既有74代码重建，覆盖同原完整pass后的变化。root只oid/action/hp/vx既有打印精度字段，不宣称完整native逐位证书。小root无depth拒绝可正常，不强造负样本；1000AI必须实际拒绝。

## 生命周期、预算、执行前置与回滚

owner是既有临时Driver/World/logic pool；stage1禁tick、stage2Join及11stage保持，WithLogan旧对象/claimed slot/borrower三0断言不动。新增字段/JSON/反射都是Editor冷结果，不是热路径0GC证据，无新worker/array owner。74的8×prepared participant容量增量为派生尺寸，不冒充实际stride/内存测量；本批不改steady/transition或ATLAS预算。

2026-10-08T11:22:49.4847296Z新鲜检查：Unity进程0/6402 listener0，原连接明确拒绝，无测试作业。已非阻断询问用户重开；不启动第二Editor/重启/安装Pipeline或插件。可以先准备测试和留痕，但未获得有效RED前不接入qualification实现；记录TESTS_WRITTEN/RED_PENDING，不称编译或通过。用户打开后先核正确项目/Menu idle clean8/非Play/CS0再run_tests。

事前602保护/6存在目标副本＋公共Temp XML缺失事实（不恢复、不归因），逐路径/头/staged/哈希在before-manifest；原XML75独立归档保护。Index先追加bootstrap登记后备份包含该一条，须记录其新SHA及与初始manifest的唯一差异；其余5副本先核同再脚本。结果路径仅本批driver-qualification-01的UUID CreateNew，RED/GREEN公共结果核case和freshness后Copy到ABSENT归档，不覆盖旧结果。

只apply_patch、准确Copy-Item无Force、已有Editor连接具名import/run_tests/get_test_job；Validate-ChangeLedger/diff/602 guards/6副本须核。回滚为准确before副本或owned逆hunk且另请求批准，不reset/restore/clean/stash/delete/push。禁止Scene/Prefab/资源/Settings/Input/Gen/Plugins/Server写入、EXT1/M0/Instancing/ATLAS/Mono解冻、生产切换。

资格通过后再以准确后继Task考虑千人短窗/正式120+1800门；本批不测FPS/GC/SetPass。当前54执行＋76准备（累计55），阶段4/6，H07/H11 OPEN，Goal active。
