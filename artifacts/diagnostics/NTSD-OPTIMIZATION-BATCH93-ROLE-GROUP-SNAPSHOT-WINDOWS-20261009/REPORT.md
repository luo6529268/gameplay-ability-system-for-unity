# 第93批：真实千人Role group snapshot成本收口

结论：WINDOWS_COMPLETED / NO_GAIN_PROVEN / DEFAULT_OFF。本批冷入口与观测工具已通过原Editor限定验证；第92候选在本次真实千人顺序OFF/ON窗口没有建立整段逻辑收益，不推广，不重复本版窗口或升级1800长窗。H07 PERFORMANCE_FAIL继续；六项阶段5/6、34父项关闭0、最新完成编号93、实际执行72，Goal active。

用户编号100边界按执行合同8.1：只检查方案是否落实清楚，不是性能截止日期。性能未达不单独暂停；不为用完编号拆准备、诊断、测试包。已有历史证据与失败均保留。

## 实际落地和测试

唯一C# BattleOptimizationWindowsAiSuiteEditor.cs增加独立Batch93菜单/输出、冷selector、单World/query owner、OFF/ON请求、录制期观测与幂等恢复；沿既有PostBindingsRole/完整Driver GC记录与十一阶段关闭。未改第92runtime算法/生产默认/资源/Scene/Prefab/Settings/Packages/Server/EXT1/ATLAS/Mono，未读Q06方法体。

- 原Editor2022.3.62f3，PID74724/6402；unity-cli要求Unity6，本次按已披露的既有UnityMCP bridge使用同一原Editor，没有安装/升级或启动第二实例。
- test-first原job041f8d2888c545be83f6233dc2a5a129，原XML24/24预期missing API/字段失败，0skip；red-01/tests.xml SHA 7AEA22288DD84E469E331A74E87969BB016D5B09112A8E8B598250AEEC15DA61。
- 最小实现后原job e1e31533a671434a893b6f5826f1b941。桥接job manager先报初始化超时/completed0，但未重启，随后原XML实际04:17:19—04:18:24Z完成85/85 PASS、0skip，逐case24新+61邻域、0意外。green-01/tests.xml SHA 9784E9B5E9E8DDE095792A8BBAFE7BAB330131A56851B8F19C78A3EB3D78CF8A；以真实XML而非作业全目录count或初始化失败标记判断结果。
- 单C#实际SHA DF63DFB1A4C01BAF32672022B22AA089EE9D59006822EE8C3867CFCD26188013；原BeginSuite17参数、旧tests/候选算法保持。92已有125局部与Driver8资格未重复。
- 一次调用菜单 NTSD/Optimization/Run Batch93 Role Group Snapshot Windows，四实际1000AI/Sakura OID1，120warm+180sample，DataOrientedCanonical/ForceRoleAware/共享渲染绑定，65DTO字段不扩。OFF/ON只诊断开关与输出路径不同。

## 完整逻辑实测，不以局部代替

单位ms。来自四report.json与measurement-audit-01.json，不是native或Android证书。

| 工作负载 | 开关 | logic均值 | logic P95 | dropped backlog ticks | CandidateCollect均值 | PairExactLoop均值 |
|---|---|---:|---:|---:|---:|---:|
| Dispersed1000 | OFF | 48.309674 | 63.747075 | 406 | 16.051197 | 8.255408 |
| Dispersed1000 | ON | 49.960042 | 102.851170 | 357 | 17.051426 | 8.987421 |
| Combat1000 | OFF | 57.639042 | 88.605835 | 505 | 19.701257 | 10.379653 |
| Combat1000 | ON | 70.614245 | 107.259470 | 729 | 23.041194 | 12.735143 |

顺序对比ON均值分别+3.42%/+22.51%，P95分别+61.34%/+21.05%；本次没有收益证据。单次顺序窗口及其它未受改动phase也波动，不能把上述差值证明为算法导致的因果回归；也不能从局部D -5.54%/C -13.64%宣布真实增益。决策是“不采用/不推广本候选”，不是再跑直到出现PASS。

ON采样last collection拒绝最大值D1322/C2126，OFF0；这是Editor离散观测，不是all-tick total。四owner applied/unchanged/restored为true，Role实际/期望3、观测无mismatch，abortedTicks0。说明候选确实触发，而非没开启；并不说明每tick collector route全域证明。

实际显示帧均值OFF/ON：D173.746445/162.785928ms，C192.452273/244.102288ms。CPU完整logic与显示帧口径分开，不把logic预算换成显示120FPS通过。四SetPass mean/P95/max均5（每窗89个completed frame）；中央CPU submission DrawMesh计数2、segment1，source/resolved command均值匹配。它们不是GPU batch/SetPass capture、单实体draw或透明排序完整证书。

## 正确性、完整0GC与关闭

- 每窗actualAI最小1000、base roster最小1000、有效180sample；workload/harness valid，未用slots冒充AI。
- 四GC before/after正/空校准均通过；每窗完整同步主线程Driver begun/ended/accepted300、steady180，invalid/rejected/sequence error0；steady0 allocation event/0 proven B，CALIBRATED_FULL_DRIVER_ZERO_EVENTS。warmup事件保留，不豁免steady首帧或缩scope。
- 四capacityPressure通过，critical/rejected/growth/throttle delta0；中央已接受帧source/resolved均值相同。未取得跨所有tick/all-camera stale或GPU完成证书，不扩大0声明。
- D与C各一对tick300终态十hash全部一致、全checksum JSON字节相同。D SHA F1A2CAC863D1AA995D49BEF7DA2D5C1B30337465FA846DB0CEFAF86D8AD7B6AA；C E4916ACA8C5BE675B8192D5A6C1429029FD94DB6F838CF6C82E532B030836B12。完整终态/RNG一致不是native全tick对齐；沿92已有声明正确性资格。
- 四正常停止restored/active state/driver/logger恢复，对象、slot、object borrower、reference borrower及cleanupException均0；suite DONE/MEASUREMENTS_COMPLETED，completed4、ordered shutdown true、remaining三0。
- 原Editor已idle/nonPlay/nonTest/noncompiling，sole Menu clean8；Menu SHA F2EE25D075548B78380BE817209152047B919C4C15DA8743D75D2E0C165CBEA1、Battle SHA 253B2EBAD322AA6EC18488DFBBF555BB93AD897940A342E4130E77CE778F9010保持。
- 首窗inactive pool从10→1050是既有预热缓存容量，不是active残留；未trim/delete。共享请求一直未创建，旧公共terminal每窗先SaveNew保留后更新。

## 下一方案边界：不要继续无效微候选

已落实：92单候选代码/资格、93真实成本/完整GC/关闭与不采用决定，H11选定CPU阶段86通过。未落实：H07有效正式1800性能门、生产Role默认推广及新千人AI策略；本批不把它们写成已实施。

本次较大成本仍是CandidateCollect16.05—23.04ms、CharacterInput12.28—17.13ms（实体输入11.87—16.66ms），而NativeBuiltins小子步骤在91约1ms。不能继续围绕同一group payload做无证微改。

下一安全动作先复用85/91/93，沿SimulationWorld.RunCharacterInputPasses（当前3036附近的producer→routing→snapshot refresh）和LF2Character.RunCharacterInputProducerPhaseForKnownCharacterDat（818附近的PrepareAiInputBasic）定位能整块消除的重复工作；只是代码范围内方案落地点核查，不再新开同构计时包，不已批准Burst、AI降频或网格重构。只有找到具名重复工作与可证明依赖才创建下一最小实施Task；若改为用户提议的千人AI行为策略，须明确模式、确定性/RNG/回放、空间区候选范围与溢出行为后获得所需方向授权，不能把nativeAI一致单独当否决理由。未定位的算法改法不写成已经确定。

不再测试本候选长窗；不重开五个已过阶段、旧对齐、EXT1/ATLAS/Mono或GPU专项。不为凑到100批制造“方案已清楚”；100时按具体热点、改法/代码落点、已做/未做和下一动作据实复盘。

## 留痕及验收层级

[Task](../../../docs/ai/TASKS/NTSD-OPTIMIZATION-BATCH93-ROLE-GROUP-SNAPSHOT-WINDOWS-20261009.md) / [Change](../../../docs/ai/CHANGE-RECORDS/NTSD-OPT-H07-ROLE-GROUP-SNAPSHOT-WINDOWS-093.md) / [Operation](../../../docs/ai/FILE-OPERATIONS/NTSD-OPTIMIZATION-BATCH93-ROLE-GROUP-SNAPSHOT-WINDOWS-20261009/RECORD.md) / [总表](../../../Assets/NTSD/Docs/battle-optimization-progress-tracker.md)。

measurement-audit-01.json及windows-01保存真实四报告/请求/终态/GC/观测/共享terminal前后和suite/progress。before-manifest与before-01保护9份dirty原件，1049范围外guard按exists/sha语义复核。最终validator/diff/after审计完成后写Operation，不以旧作业返回推断新验证。

本批没有独立生成工程build、额外SelfCheck、正式1800、native全World/全tick、真实GPU、Android/device或120FPS验收。聚焦和真实诊断通过只闭本批工具范围，不闭H07或父项。

最终审计：12:43:20+08 final-audit-01.json实际1049guard/9backup0drift；HEAD/staged不变，两Scene SHA保持，唯一C# diff且实现SHA与测试/测量时同。Validate-ChangeLedger.ps1 exit0/PASSED、1388Records、code diff1/error0、4428WARN未逐条归因；不称0WARN、不顺手清理。git diff --check本包7路径exit0。两request对65字段仅outputPath差异。Operation after-manifest-01.json记录本包文件最终SHA并即时复核，旧失败与备份保留。

