# 第93批：Role rejected-group snapshot候选真实千人成本闭环

状态WINDOWS_COMPLETED / NO_GAIN_PROVEN / DEFAULT_OFF；执行者/root；开始2026-10-09T12:04:33.4870130+08:00。本包把冷接线、最窄测试与四真实窗口合在同一批，不额外拆准备/诊断。第92已完成资格证据不重跑。最新已完成93/实际执行72，本包计一次执行；阶段5/6、H07 PERFORMANCE_FAIL、Goal active。[本批报告](../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH93-ROLE-GROUP-SNAPSHOT-WINDOWS-20261009/REPORT.md)。

## 来源、假设与范围

需求为有效六项合同第0—8节、用户持续H07优化及第100批仅方案落实清晰度的澄清。正式336B44/33ms/3ms/max2保持。
第92 V2 managed collector仅D -5.54%、C -13.64%局部信号；125case与真实Driver8case/88paired ticks已经通过，但未测完整千人逻辑实际成本。第91 Role ON两布局CandidateCollect约15.73/16.16ms、整logic P95仍60.10/57.30ms；仅局部幅度不足以推断H07性能门可过。
本包唯一问题：92的group reject前置在原千人Driver完整logic上是否真实有收益、是否保持完整logic0GC与公共关闭。拒绝重复资格、旧计时采集或提高测试数量冒充性能提升。无收益则不推广，用真实热点调整方向，不重复此版窗口找PASS。

唯一脚本：
- Assets/NTSD/Scripts/Test/Editor/BattleOptimizationWindowsAiSuiteEditor.cs

新增RoleGroupSnapshotSkipOutputRoot、cold selector/World与query owner、SuiteState mode、RunState观测、BeginRoleGroupSnapshotSkip/BuildRoleGroupSnapshotSkipRequest/Apply/QueryValid/Observe/ObservationValid/Restore；只接既有92诊断开关。
BeginSuite17参数保持；沿BeginPostBindingsRole calibrated full Driver GC owner，四请求只有output/formalCollectorMode相对BuildBruteProductionRequest变化。不改92runtime算法及测试、producer/frame reuse、84capacity guard、83/85/91timing、render与生产默认。
同包修正总执行合同开头仍残留的旧“未达H07门或方案不清楚暂停”摘要，保留为被8.1澄清替代的历史；不修改8.1正式条款或性能门。

## owner、安全与关闭

启动须原Editor idle/nonPlay/noncompiling/nonTest、Menu clean8，shared request absent，旧公共terminal具名owner。
candidate只在新诊断World的首tick前应用；记录原flag、单owner、禁止与producer/input/role timing mode共享或使用foreign World/query。容量/三scratch array不变，不新增worker/job/GPU。
owner结构有效性不依赖runner终态已停止的diagnostics；live观测要求录制存在。观测last collector skip maxima只为采样Editor观察，不声称all-tick total。
CompleteRun先核配置然后恢复；ShutdownAndExit与外部Play exit同一幂等Restore，清World/query引用后才进入现有十一阶段关闭。没有新生产runtime module/关闭阶段，也不改变顶层顺序。发生owner或route漂移判窗口无效、fail closed，不重跑掩盖。
保留透明publication/segment/failclosed/33ms/checksum/RNG，Q06方法体不读；EXT1/ATLAS/Mono保持hold，不改资源/Scene/Prefab/Settings/Packages/Server，不启第二Editor，不破坏Git或删除文件。

## 冻结测试矩阵

RoleGroupWindow_ test-first共24case，原方法/字段尚不存在时验证RED，再最小实现：
- request4 / outside2；
- OFF与ON apply+drift+restore2；
- stopped-recording结构owner1 / live录制观测2；
- null/Brute/foreign/no-recording/double-owner拒绝5；
- backward-compatible session1 / stable-owner及实际skip判定4；
- current request路由1 / exclusive cold Begin拒绝2。
GREEN合并上述24与原PostBindingsRole_、InputBoundaryOwner_、ProducerSnapshotOwner_共61case，预计85；不得再运行92局部125或Driver8八分钟资格。仅新代码编译/相关冷route回归，不宣称native全域验证。

## 冻结真实窗口与判断

新Menu NTSD/Optimization/Run Batch93 Role Group Snapshot Windows，一次四窗口：
00 Dispersed1000 OFF、01同布局ON、02 Combat1000 OFF、03同布局ON。
全部实际1000AI/Sakura OID1/MobileExtended/ForceRoleAware、seed/request既有65字段保持、120warmup+180sample、render shared绑定生产状态保持。只group skip flag OFF/ON；其它diagnostic候选OFF。
新output artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH93-ROLE-GROUP-SNAPSHOT-WINDOWS-20261009/windows-01；no overwrite，公共result每窗按原SaveNew先保存；公共xml当前92通过原件先备份后由run_tests覆盖，RED/GREEN各新absent副本保留。
采可靠GC before/after calibration/full300 Driver scopes/180 steady，compare整logic mean/P95/drop、显示帧、CandidateCollect与PairExactLoop、actual collector/abort/AI1000、capacity rejects、central unresolved、末tick十hash/fullJSON/RNG可用域、owner恢复、shutdown三0/Menu/Scene SHA。
只同期顺序OFF/ON，环境噪声不冒充算法因果；不从SetPass/CPU draw推GPU。末tick一致不当全native逐tick证书，沿用92资格边界。
只有两短窗均有效并满足H07性能/安全门才考虑按原合同正式1800窗口；短窗未过不做同版长窗/重跑找PASS，留下真实FAIL/是否值得采用结论。default OFF不自动推广Role-aware或92candidate。

## 精确文件、恢复与留痕

9既有写域见[Operation](../FILE-OPERATIONS/NTSD-OPTIMIZATION-BATCH93-ROLE-GROUP-SNAPSHOT-WINDOWS-20261009/RECORD.md)/before-manifest，单C#及治理6、公共XML/result。当前HEAD a0ecb01588e77feeda7fd369bc2011fc099b1173，staged empty，原内容按dirty bytes备份，不以HEAD代替。
新文件仅本Task/Change/Operation Record/manifest/备份与同ID artifacts REPORT/audit、test red/green与窗口原件。文件操作先登记/新absent无Force复制并核SHA，再apply_patch。
回滚需要独立批准，只恢复本包hunk/现存dirty备份；禁止reset/checkout/clean/stash/delete/commit/push。外部.codex/config.toml新dirty观测仅SHA保留、不读内容/修改/纳入本包。
实际验证、状态和失败追加[Change](../CHANGE-RECORDS/NTSD-OPT-H07-ROLE-GROUP-SNAPSHOT-WINDOWS-093.md)，当前唯一总表，不新建重复总表。第100仅方案落实复盘，性能FAIL不能单独暂停Goal。
