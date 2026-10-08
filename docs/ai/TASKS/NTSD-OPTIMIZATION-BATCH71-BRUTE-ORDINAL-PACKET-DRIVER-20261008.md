# 第71批：原序packet完整Driver资格

状态PLANNED / IMPLEMENTATION_NOT_STARTED，来源70已实测局部mean/median两布局≥5%减少；上一Goal轮为PROGRESS，非状态重述。本批必要信息是packet在完整pass/AI/结构变化后是否仍逐tick等价，不重复69几何/70cost。

## 精确路径与冻结依赖
只改Assets/NTSD/Scripts/Test/Editor/BattleBruteProductionAdmissionEditorTests.cs，before SHA 391A7973AF7D52A66D33F3A6A1ED62CCC546172F40BA392B7F3225454F8A2B55。70 Query 394EF397D3A5C8C66635855D3D8CC98A0854C1EE9F80BED533C5699DEC33FAD8及Role tests 6EF06591A28B9DBD8A8969D4B6BF66D640C906D98C6E3028248B934438F7C941保持；Suite/Feature/Host/Kernel/native及Q06活跃方法体不写不读。保留旧Run/CompareEveryTick/CompareRoot/SaveNew与所有旧case：Run只增加nullable ordinalPacket参数（在最后composeEnvelopeBindingEligibility之前，旧last参数断言保持）、显式本批设置/诊断捕获；未请求本批的旧call行为不变，旧断言不放宽。新private RunOrdinalPacketDriver入口只newcase使用。仅Editor结果字段，不加生产计时器/observer。

原四admitted true，普通ForceBruteForce；baseline packet=false、candidate=true；kind5/eligibility/envelope/proof/rejectedBinding/dispatch/timing均false，任何未声明组合拒绝资格。candidate始终生产默认OFF，不能自动promote。原序/IsBound首次base gate/精确survivor/chunk/segment/33ms/3ms/max2/0GC/11stage不变。

## 固定矩阵与test-first
新5case：默认/显式入口1；right-x550与left-x350各12ticks OFF/ON2；Dispersed1000/Combat1000各32tick OFF/ON2。5个反射新helper缺失先有效RED，不以compiler error充RED；实现再GREEN，另旧OrdinaryBruteDefaults与ProductionRequest(index0/1)纯3必要回归。四组88同tick配对/176完整Driver step，不称真实Scene性能窗。
每tick CompareEveryTick已有entities/完整声明extended或parity域/lockstep/native及legacy RNG calls，并本批比较完整NativeRandom scalar state与legacy State，slot/AI counts。1000AI每tick实际1000、canonical profile/postcommit0、cache应用无fallback；候选packet真实应用/负证明/提前reject必须>0，OFF全0，array identity/容量保持、不隐式扩容，活跃实体新增不得截断。原WithLogan... shutdown World/claimed slot/logic borrower三残留0保持。
FormalSha336B44...7BD3；适用root trace两SHA AA327E22...15FBE / CF270170...9B2本轮已核；只既有oid/action/hp/vx（打印精度容差），不能说整native全域bit-exact；root不足16无需强造packet reject。文件fixture源、DAT正式runtime、project mode保持，不重开native campaign。

## 生命周期、预算与范围
借用现有临时Driver/World/logic pool：stage1禁tick、2Join、既有11stage shutdown；无新worker/borrower/cache owner。新TickRow是测试冷结果，不宣称热路径0GC。70 packet完整cold CPU array仍按steady/resize旧＋新transition记账；stride/bytes UNKNOWN，本批不重试失效counter、不改ATLAS。
不Scene/Prefab/resource/importer/Settings/Input/Gen/Plugins/Server写，不Instancing/M0/Profiler/GPUcapture/真实Windows。资格通过才后继准确Task考虑四短窗；不能只报告PASS缩掉正式120+1800门。

## 操作、保护与验证
事前manifest 2026-10-08T07:13:18.0840372Z HEAD527350afa08633357ba20e7453a9d260eaa2b97c，7准确write/510去重guards，原Editor78296/6402 Menu savedclean8roots idle nonPlay；先Change/Operation/index，准确7dirty副本（Index含本批登记bootstrap，须实录与更早manifest差异，其余6核同），然后唯一脚本。公共Temp XML有70 cost原件并必须before07保存；各终态结果先核case数/Unix UTC freshness后Copy到ABSENT RED/GREEN。四Driver输出仅本71driver-qualification-01/SaveNew UUID CreateNew，旧文件不可覆盖。
命令apply_patch、Copy-Item -LiteralPath无Force、manage_asset exact import、run_tests具名filter及同job get_test_job、terminal XML、source/before-relative diffcheck、Validate-ChangeLedger；不启动/重启Editor、不Git丢弃/commit/push。首次manifest stdout截断导致JSON解析失败只是只读，压缩重采成功，没写文件。
回滚只本71before01及准确逆hunk另获用户批准，不Git restore。范围内第一差异可修受影响接线，原失败保存，不改权威或断言。
阶段4/6、49已执行＋71准备、H07/H11 OPEN，Goal active；次数只审计，不因本批结束停目标。
