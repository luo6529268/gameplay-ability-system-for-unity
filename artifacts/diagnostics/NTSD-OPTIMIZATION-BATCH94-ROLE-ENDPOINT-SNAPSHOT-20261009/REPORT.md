# 第94批：Role参与者端点快照复用限定评估

结论：FOCUSED_TEST_PASS / LOCAL_ONLY / NOT_ADMITTED / DEFAULT_OFF。H07 PERFORMANCE_FAIL，Goal保持active，六项阶段5/6，34父项关闭0；本包完成编号94/累计实际执行73。方案清晰度复盘仍以编号100为准，不把性能未达当暂停条件。

本包实际修改了候选runtime与聚焦测试，不只是整理文档。没有得到最终版真实1000AI/完整Driver/显示FPS收益证明，未推广默认，不启动本版实际窗口或1800正式采样。V2局部成本可辨度有限；最终又收紧World-wide纯类型保护，旧数字只作为候选线索，不冒充最终源版本性能结果。

## 方法与精确落点

- BruteForceSceneQuery.cs:233 新default-off开关；1060冷flag ON才预热两primitive数组，OFF不额外驻留；1703每次收集独立失效；3567在既有Role exact cache/occupancy检查后准备；6731验证全部World active实体为exact LF2Character（覆盖不参与geometry的holder），任一unknown/custom整段回原；6758按已验证handle slot惰性捕获一次并组合不同pair。未预热/越界原Capture回退，无热扩容或局部截断；sealed增长明确拒绝。
- BattleHitCandidatePairSnapshotFactory.cs:8 readonly endpoint仅原7int/valid/facing；34捕获endpoint；47组合仍每pair在原点live解析holder，原Capture与89方法主体不改。候选count/nearest/kind4/RNG/store写仍原record时点与顺序。
- RoleAwareCollisionShadowSelfCheckTests.cs:446起新 EndpointSnapshot_及冷helper。仅本三C#，没有新manager/worker/queue/引用owner。新缓存只有scalar值，ResetFormalSpatialBroadphase失效，owner仍原query/World关闭阶段，不改11阶段顺序。

这是31“同一pair方向多ITR/body复用”的跨pair端点候选，不是重新做89四帧引用复用，也不是Burst/Grid/新AI决策频率方案。全World类型限制意味着混合未知派生实体会回退；未做真实工作负载适用性证明。

## 测试：保留失败，不汇总成同版全PASS

| 作业 | 原始结果 | 范围 |
| --- | --- | --- |
| RED 1f520148ee534ad99bca00410ec2307a | 15/15预期FAIL | 缺新API；不是0case或编译错误 |
| first GREEN d51f6a2ff55646e7a9da85e166823a07 | 38 PASS/1 FAIL | D成本把pair数与endpoint读取数误同单位；其它payload/RNG/计数/重捕获/0GC通过；原件保留 |
| V2 GREEN 46581fe5197043c39fec75815e7e1282 | 39/39 PASS、0skip | 15新+24邻域，含两新1000无AIcollector局部成本；不是实际1000AI |
| cold safety 7fd2d8e505b6401ca6b56584690c8ec7 | 37/37 PASS、0skip | 原成本不重复，default OFF容量0、seal后增长拒绝/数组identity |
| final 2b9d0660db9747688288cbd1445c1a80 | 38/38 PASS、0skip | 再加不参与collision frame的custom holder边界；最终算法非成本 |

最终XML UTC 2026-10-09 05:39:08Z—2026-10-09 05:39:10Z，SHA 4C74A089A53552FF9659291C158E5F17CB0591A688CF26B9A32F7A573FF1E77E，green-04-final/tests.xml。V2 XML SHA BD2D67910F43154DEF459C75C77F64CE622F37791E2C813B1EFA5AA4E73F2CAE；RED E53EE01CE5CEB07D717DF98BF97CFBF84457CC48FE8474800CCDC9AD796EEC88；first failure 520A46850B4B558D8E2E8746533CDF48C92A80516D893FB56C770B71CD0ED4E0。全部保存新absent目录，不删除旧失败或改变旧断言。

暖collector managed allocation case仅4个logic-only角色，32warm/16sample的CaptureCollisionFrameSnapshotsAll→Collect→End scope0B；反射/结果复制在scope外。不是可靠完整Driver1800或真实AI0GC认证；H11已过CPU域不重开，不冒充Android/GPU/native全域。

## V2局部成本线索（最终World-wide guard收紧前）

两布局各1000真实LF2Character实例，但没有AI/显示；每25个一个attack，2ITR/3body，8warm+16sample同进程交错OFF/ON，seed相同，candidate sequence/full payload/RNG相同。这里只给mean，没有variance/CI或真实AI适用性，不能断言稳定因果收益。

| 布局 | OFF collector mean ms | ON ms | 变化 | 16次端点capture / pair compose |
| --- | ---: | ---: | ---: | ---: |
| Dispersed | 12.384075 | 12.280950 | -0.83% | 16000 / 15360 |
| Concentrated | 186.8899625 | 170.43286875 | -8.81% | 16000 / 614400 |

每pair原来需两端，正确比较baseline endpoint读取2*composed与capture；不能直接要求compose>capture。D差异不足独立支持稳定收益，因此不为局部C信号启动昂贵实际窗口/1800或生产推广。最终新增World全active类型guard没有复测cost，上表只保留V2限定证据。

CPU scratch budget：OFF没有这两数组的容量分配；显式cold ON按slot预热，暖期两数组identity/长度稳定。元素仅7int、2bool，另byte状态；1050容量未实测结构stride/对象header/RSS/transition peak，因此不把字段下界当真实驻留预算，不改变ATLAS预算/格式。未推广的新CPU scratch正式预算仍需最终准入包核实际prepared长度/stride；不是当前默认的额外缓存。

## 已排除的重复方向与下一动作

本次重读84原报告：相同冻结Sakura1000实际Driver的kind0非nearest满候选skip为0，故仅把该同条件守卫从record移到ITR/body前仍不能证明实际适用；不实施这种同构候选或重跑84。Role逐paircache revalidation已是test-only，实际默认只参与者一次，不能称“去掉逐pair校验”能改善当前耗时。当前AI已走IndexedCanonical，不再优化未用legacy全场遍历。

下一安全动作仍针对已测CharacterInput块：现有91归属RemainingAiDecision约5.1ms、producer snapshot refresh约1.0ms，先追SimulationAiDecisionModule.TryPrepareAiDecisionIndexedCanonical与production producer→freeze→commit的整块重复读写，核字段写时点/owner后选择能实质少做工作的改法。不为候选计数、子批或未达33ms制造新停点；没有收益明确不采用，真正的新方向另按准确Task范围。新Grid/Burst/千人AI行为模式不由本包自动落地。

## 原Editor/保护/留痕

原PID74724/6402/Unity2022.3.62f3；import domain reload两次短拒绝未提交测试，随后原端口恢复，无第二Editor/重复job。最终原Menu8roots clean/nonPlay/nonTest/noncompile；两Scene SHA、Settings/资源/Packages/Server/Q06/外部.codex原当前值守护。没有Play/Profiler/GPU/FrameDebugger/M0或Scene保存。

9份当前dirty before精确SHA保持；1103范围外guards0drift，HEAD a0ecb01588e77feeda7fd369bc2011fc099b1173/staged空。开始五治理头插入早于备份的代理错误已仅移除自己的新hunk并核原SHA再备份/重插，记录保留，不损用户旧内容。无删除/移动/破坏Git/commit/push。

Validate-ChangeLedger.ps1实际6>&1捕获：exit0/PASSED，1389Records，4governed code diff（本3+原93Suite），error0/warnings4355。不把第一次2>&1的错误“warnings0”当事实，不清理/逐条归因这些warning。C# diff --check exit0，仅LF→CRLF提示。末次范围/文档/source SHA与after manifest见同目录final-audit-01.json及Operation after-manifest-01.json。

Task docs/ai/TASKS/NTSD-OPTIMIZATION-BATCH94-ROLE-ENDPOINT-SNAPSHOT-20261009.md；Change docs/ai/CHANGE-RECORDS/NTSD-OPT-H07-ROLE-ENDPOINT-SNAPSHOT-094.md；Operation docs/ai/FILE-OPERATIONS/NTSD-OPTIMIZATION-BATCH94-ROLE-ENDPOINT-SNAPSHOT-20261009/RECORD.md。唯一进度总表已更新本H07进度、说明、证据与下一动作；没有新重复总表或修改PERF/ATLAS/Mono/EXT1正文。

收口更正：最后八文本写域diff --check曾检出本包新增tracker尾空行，精确移除该空行后实跑exit0。guard采集器对旧引用缺exists字段一度误报40条，使用显式exists或sha推导后fresh1103条0drift、9份backup SHA同；这不是外部资源变化。XML discovery testcasecount10478/10479不是执行量，五作业实际total依次15/39/39/37/38。三次文档apply_patch校验拒绝后按hunk文件顺序成功，保留旧93历史、当前入口94；工具过程详Operation。再次Ledger实跑exit0/error0/warnings4355，不声称无警告。

