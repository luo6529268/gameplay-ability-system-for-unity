<!-- CHANGE-RECORD
id: NTSD28-Q06-TYPE5-MATCHED-PAIR-EARLY-001
status: VERIFIED
change-kind: NATIVE_TYPE5_MATCHED_PAIR_AND_FRAME_READERS
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06Type5MatchedPairEditorTests.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs
authority: Current playable matched-pair initial gate, reset/latch/native-frame domain and hold ownership; TYPE5-MATCHED-PAIR-SOURCE-WITNESS-001.
evidence: Source138/78933 PASS; four Unity matrices initial state equal and3432 differences before patch, now0 differences. Broader regressions running; SelfCheck/Play pending.
-->

# Type5 matched早返与native帧域接入

生产实施范围（有效RED之后声明）：DamageWriter.ApplySpecialAttackDamage增加无armor type5 matched入口，TryApplyNativeType3MatchedPairEarlyBranch读取native current，ApplyNativeType3PairReset读取native latch并raw绑定；HitPlan.CanProjectType5UnarmoredDamageWriterEffect/ProjectType5UnarmoredDamageWriterEffect复用独立matched投影，CanProjectType3StateSyncDamageWriterEffect扩展同一无armor type5范围并移除非权威目的帧存在性门，ResolveType3PairResetAction使用native getter。共享type3 reset读取范围随之修正，需旧type3回归；type3后置分支类型不扩大。保留rest→target reset→attacker reset→hold release，保留HP/运动/count等早返字段；不增加runtime状态、队列、schema或关闭阶段。回滚仅这两文件本批最小增量，保护此前未提交改动。编译/138四矩阵/旧type3与585及weapon回归/SelfCheck/真实Play未验前保持未关闭。

当前VERIFIED / DECLARED_NO_ARMOR_TYPE5_MATCHED_SCOPE。source138/78933、双跑一致；Unity四138 before0/diff0，24/24相关回归及另旧type3四项4/4，SelfCheck15:07:54Z、Play552 15:08:59Z、关闭15:09:11Z全部PASS。最后matched本地回放job a5cb22d424fe49c8b143881be3a41db2于15:14:32Z终态2/2 PASS，40代表/配置×两配置=80场景、160重放tick。XML已归档tests-matched-replay-2-pass.xml。只在原World恢复，跨World epoch缺口未关闭；完整type5/noncharacter/Q06/资源迁移未据此关闭。

以下保留测试先行过程：准确先新增Editor fixture。直接读取source row.params.dat三份实际DAT文本，通过正式parser/converter加载；按明确的冻结candidate后before初值恢复动态字段，不用source after作为输入。比较三实体raw47/3、已映射extra/rest/sparks/RNG/audio和显式hold释放finalize。yResolved没有已确认Unity字段，只作为source-only诊断保留，不伪造carrier。严格before零差异后才判断执行差异。

source完成/RED后再声明两生产文件和准确方法。必须保留type3 matched和普通type5/weapon回归、前置/live-rest/feedback/first-body顺序，不能移动全局pass或改字段schema。没有创建新runtime队列或生命周期owner。未完成compile/focused/SelfCheck/Play证据不标完成。禁止computer-use、Scene/资源/非战斗/Gen/Plugins/Server修改。回滚仅本差量，不回退已有工作。

2026-09-14 本次恢复补记：用户再次确认HUDBg x30属于其或其他任务，保留。source最终138例/78933检查，root重新执行独立validator PASS。Unity首次job `72c81e1df2834671844984311bf97137` 四项全部失败，每组before414，仅baseMaxMp初始化错误，不能据此认定全部4260 after差异是生产缺陷。已保存 `artifacts/diagnostics/NTSD28-Q06-TYPE5-MATCHED-PAIR-EARLY-001/fixture-before-failure/` 原结果和XML；测试夹具由Health.MaxPP改为实际raw绑定的Health.MaxMP（LF2LivingObject.cs / NTSD28UnityEntityRawCapture.cs已核对），未改生产。

第二次job `a8f32f45a53d45fab1c784f344d68fb6` 终态4 FAIL：两profile × direct/Shadow，每组138例、before0、after/finalize等3432差异，Shadow没有额外差异。原JSON/XML归档 `production-red/`。明确首差为matched未早返，target错误扣血并写Fall/Bdefend，双方raw action/counter未重置；这是有效生产RED，尚未修复。下一步先补两生产路径到本Record，再实施已审阅的matched gate、native latch/reset及独立Shadow投影。当前无测试运行，未跑本批SelfCheck/Play。Ledger PASSED（576 records/24 governed files），git diff --check通过。

生产准确两文件/前述符号已修改，状态CODE_WRITTEN（父仍IN_PROGRESS）。编译刷新后Console error CS为0条，定向job 2f14606469fe40a6a8dab6c794c2ee2d进行中。旧type3命名空间需NTSD.Test.Editor补跑；不把未选择测试当通过。

独立reviewer再次只读审阅两生产patch，未发现新增确定性错误；确认type3后段gate未扩大、rest/reset/hold顺序保持。共享type3 native读取变化仍需回归。Scene SHA BCD1047B…0E9FB6保持。

24/24 matched+普通type5+weapon通过，另旧type3四项4/4通过，终态XML已归档。原声明Editor测试路径新增NTSD28Q06Type5MatchedPairPlayProbe，沿用已有请求机制，仅Play中暂停场景world并创建独立fixture world，138×两factory×direct/Shadow=552，finally回收Renderer/关闭World，验证场景checksum及borrower守恒。无新增runtime模块或停止阶段。需刷新编译后执行；本地matched snapshot replay仍未补。

> 2026-09-14 15:09Z当前游标：TYPE5-MATCHED-PAIR-EARLY生产两文件已修复；四138 before0/diff0，24/24相关测试及另旧type3四项4/4 PASS。SelfCheck15:07:54Z PASS；真实Play15:08:59Z 552 PASS（两factory/direct+Shadow、Renderer2→2、场景checksum保持），关闭15:09:11Z PASS（恢复4→4、World/slots/两pool0、两帧Stopped），Scene dirtyfalse/root14。所有证据同ID artifact。唯一剩余本批验收：专门matched local snapshot replay未补（普通type5/weapon replay已通过，不能替代）；补该项及source/主Task治理收口后再进入NONCHARACTER-REDUCED108。当前无运行测试或build/Play probe，下一步不用重做已过矩阵与Play。总目标ACTIVE，Q07未部署，禁止computer-use/非战斗修改，HUDBg30保留。


追加验收预声明：仅既有NTSD28Q06Type5MatchedPairEditorTests.cs新增MatchedStateSurvivesLocalSnapshotReplay及ReplaySignature，按matched state/latch/reset/current/relation选代表，命中后完整tick稳定边界capture，推进两tick、原world restore再重放，比较checksum、全部三实体pending/count/links/hit records、3×3 rest与随机标量，并验证旧随机cursor失效。不改生产或snapshot schema，不据此证明跨World allocation epoch恢复。

最终验收：VERIFIED / DECLARED_NO_ARMOR_TYPE5_MATCHED_SCOPE。source138/78933、双跑一致；Unity四138 before0/diff0，24/24相关回归及另旧type3四项4/4，SelfCheck15:07:54Z、Play552 15:08:59Z、关闭15:09:11Z全部PASS。最后matched本地回放job a5cb22d424fe49c8b143881be3a41db2于15:14:32Z终态2/2 PASS，40代表/配置×两配置=80场景、160重放tick。XML已归档tests-matched-replay-2-pass.xml。只在原World恢复，跨World epoch缺口未关闭；完整type5/noncharacter/Q06/资源迁移未据此关闭。
