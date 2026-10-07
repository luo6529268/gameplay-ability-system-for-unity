# 第25批有界准入评估
状态：SCOPED_ADMISSION_ASSESSMENTS_COMPLETE。用户已确认六项首阶段并“开始执行吧”；本批为新子批4/8，H06/M13/M14有限评估已有据结论，19/19实际执行，33＋1原件保留；不是优化算法实施。新鲜counter正对照失败导致H11的0GC证据待补，H07 3/3到限待用户方向；阶段4/6，不能Goal complete。以下冻结范围为事前合同。

## 精确写域
正对照反例的必要文档更正写域追加：H11方案、M03方案、NTSD-OPT-H11-M03-FULL-CHAIN-CLOSURE-023 Record、Batch23 REPORT与Task，只追加证据更正（不改生产代码/旧raw/测试事实）；五现有文件当前准确backup见evidence-correction-before.json。原protected169这五条转精确写域，其余164不动。不新开H11实现/诊断矩阵或自动重测，未验证计数器不能通过本阶段0GC门。
新增 Assets/NTSD/Scripts/Test/Editor/BattleOptimizationAdmissionAssessmentEditorTests.cs 与 Unity meta；本Task、Change NTSD-OPT-H06-M13-M14-ADMISSION-PROBE-025、同名Operation/独立artifacts；以下既有文档只增量留痕：Ledger、STATE、handoff、FILE-OPERATIONS/INDEX、原优化总表、有界Goal合同、H06/M13/M14三个独立方案。没有生产代码、配置、Scene、资源、Server、Q06方法体改动。

## 冻结评估与验收
必要证据修订（18/18后、脚本改前）：真实Capture有大量明确new对象但counter为0，是本批实际反例，不能当0GC；仅新增一次AllocationCounterPositiveControl，用存活1MiB byte[]验证同API的正响应，记录结果不要求读数为正才测试绿。只跑这个新case，不重跑四形状/声音/加载。仍原有诊断1批/无第三候选/无生产改动，原报告和18case保留；若counter不响应，0GC认证待补而不是“零分配”。
脚本前实现细化：H06的Direct候选使用本地query.ForceRoleAwareDirectForDiagnostics=true，nested/sweep仍按现有8192 crossover自动选择；另一候选forced-tree。仅本地fixture，不改变生产配置或262144默认选择。两候选家族不增加。
原2022.3.62f3 Editor/PID19040当前Menu idle/非Play/clean。只EditMode fixture，不进Play/另开Editor。性能是Windows Editor collector/synthetic sink/load-time方法成本，不宣称完整1000AI tick、设备、GPU或正式权威一致。
- H06：4个1000-participant collector形状：Dispersed（线性20px、itr偏移10000）、Combat（50列×20行、间距20/30）、Concentrated（20列×50行、间距1）、OPointBurst-shaped（760角色+240specialAttack，40列×25行、间距16/20；不是实际OPoint生成）。seed0x41C64E6D，无输入/AI/tick推进。baseline ForceBruteForce；仅两已有候选家族 RoleAware-adaptive Direct/Sweep 与 forced-tree，不修改阈值或默认。每形状先复用既有RunCollection/AssertRunsEqual，比较候选顺序、target handle/generation、hit字段和RNG；成本scope为CaptureCollisionFrameSnapshotsAll→CollectCollisionCandidatesAll→EndCollisionCandidateConsumption，不包含fixture/reflection/结果复制。每backend4warm+两份8sample，记录ms/当前线程alloc/pair/fallback/aborted/direct/sweep/tree。固定focused：Formal_OccupancyEpochMutationAbortsAndRestoresBruteRngAndCandidates、Formal_InputValidationRoutesPreserveCandidatesRngAndWarmedAllocations、CandidateStore_GenerationTargetReuseFaultIsolationAndGrow、Formal_ForcedDirectAndTreeMatchForZeroOneAndManyItrs。完整hit/OPoint/stats/checksum/双RNG/native当前权威未被collector probe覆盖，未闭合即NO_DEFAULT_PROMOTION/AUTHORITY_EVIDENCE_PENDING，不重开nativecampaign。
- M13：复用既有私有音频Fixture，实际路径前128个正式WAV身份但使用fixture合成静音clip，不解码/替换资源。1000事件同tick，unique cue=1/8/128，4warm+两份16sample；只测实际NTSDSoundPlayer.PresentSounds，比较次数单独按当前Ordinal扫描统计，不混入timing。记录voice/play/reject/drop与当前线程alloc。选定H07 smoke声音事件0是实际窗口反证，没有真实多cue热点就NO_IMPLEMENTATION_THIS_PHASE；最多一最小未来候选预分配cue-id/首次出现顺序，不实现。fixed focused：SameTickSharedCueAggregatesOnceWithoutMergingNextTick、DifferentDynamicCuesAndBuiltinIdentityRemainIndependent、VoiceReplacementDoesNotRetainStaleBattleIdentity、DestroyedOwnerReleasesCopiesAndCannotRecreateThem。本评估不声称synthetic零GC或实际1000AI音效成本。
- M14：从现有GameConfig asset读取RuntimeRoot和ProjectBattleModeConfig快照；一次真实LoganVisualContentCandidate.Capture（包含自身Assert）+两次AssertInputsCurrent，记录时间及当前线程分配、Images计数/存在/缺失和可证明图片读字节下界。只warm-cache、非cold启动/物理磁盘字节或整个startup。文件可变且HashFile关闭stream，不构成immutable包/read lease；前置未成立则DEPENDENCY_NOT_READY/KEEP_ALL_GUARDS，不删除校验、不缓存或用mtime/size代替身份。

结果可为准入拒绝/待依赖；测试失败是证据，不改变规则使其绿。诊断最多1批/每父最多3修复复验，成本噪声INCONCLUSIVE不反复择优。仅必要compiler/fixture修正，不加角色矩阵/额外架构。原Shared request/result、Scene和原dirty文件SHA保护，临时fixture资源只按既有Dispose释放。输出CreateNew，不覆盖旧报告。所有production/default保持。

## 留痕、风险与回滚
脚本前PLANNED Change/Ledger/STATE/handoff/Operation准确before副本；new file不含runtimeowner。Fixture暂用既有GameConfig._instance但finally Dispose恢复，隐藏临时GO/合成AudioClip回收，退出检查原Scene clean/hash、GameConfig身份。集中collector复制可能占用内存，只四冻结形状；测试Timeout120s，失败停止该形状不扩矩阵。脚本回滚不得Git丢改；只在用户批准后按before精确hunk恢复/移除新增文件并另留痕。最终实际EditModejob/报告、Validate-ChangeLedger.ps1与git diff --check，未经实现或运行的域明确未知。

