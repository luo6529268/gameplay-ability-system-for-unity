<!-- CHANGE-RECORD
id: NTSD28-Q06-NONCHARACTER-ARMOR-FEEDBACK-001
status: VERIFIED
change-kind: PREARMOR_FEEDBACK_UNITY_TRANSACTION
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06PrearmorFeedbackEditorTests.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleNativeOrdinaryHitPrelude.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleOrdinaryCharacterDamageRouteResolver.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/BattleHitCandidateSequenceRunner.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs
authority: Formal unarmored prelude and armor feedback/rest/first-body order; PREARMOR-FEEDBACK-SOURCE-WITNESS-001 original function vectors.
evidence: Current BDEFEND256 direct968/Shadow1000 differences per profile and source768 reveal prelude/RNG/rest/first-body order issues beyond the missing feedback return.
-->

# 非角色护甲反馈与前置事务

IN_PROGRESS / TEST_FIRST_ONLY。当前准确write scope仅新增单测试脚本，生产路径尚未授权到此Record。先复现原向量Before三实体raw47/3、显式links、3x3rest，实际统一candidate消费后的完整raw/links/rest/spark/CRT/native对照。测试输入用正式Logan converter及合成World；源冻结后初值明确设置到Unity，不声称默认spawn links已经对齐。

原CPP的type1会因ratio15<Bdefend17进入bypass，不得泛化所有type1跳过prelude；active Bdefend0及state7 defense先从源补证。影子/非Mono和Mono实际入口及关闭随后接续，任何生产脚本修改前先更新准确code-path和完整前置/后置条件。

不修改Scene/资源/非战斗/GAS/Server、随机算法、schema或有序关闭。验收首先是可解释RED且before input匹配，再实现后取得完整源码矩阵/Shadow/selfcheck/真实Play/关闭证据；不能把RED仅归为已有例外。回滚仅本任务差量且遵守用户授权规则，禁止computer-use。

实际新增NTSD28Q06PrearmorFeedbackEditorTests，正式Logan parser/converter、冻结候选与同原source初值、四consumer共用的实际runner入口，before/after raw47/3+links/rest/sparks及CRT/native/legacy轨迹比较。c447868a两组各984完成且before0差异，after各4641条/712case，RED保留。Unity编译0 error；本轮未改生产。下一步按照artifact REPORT明确单一普通hit前置owner/准确生产Record再实施，不只在damage尾部早返。

本轮交付检查：ChangeLedger通过（567记录/12个工作树脚本diff覆盖），git diff --check退出0；逐文件复核上一批7生产脚本和Scene hash未变。当前测试2组FAILED为已确认RED，不能标FOCUSED_TEST_PASS或VERIFIED。

## 实际生产接入（修改前）

先准确三生产路径加已有测试，不改其它文件：新增无资源BattleNativeOrdinaryHitPrelude（immutable plan/Apply/feedback append），复用既有armor/defense resolver计算源入口分支；BattleOrdinaryCharacterDamageRouteResolver增加专用ResolveForNativeEntry，允许当前type1..6并保留type1直返/普通defense rest失败fallback，原Resolve兼容入口保持；SequenceRunner仅对原kind0/4/5且effective kind0延后live vrest，执行native prelude后决定unsupported/feedback/rest，并使first-body仅在unarmored非feedback后执行。

Prelude是唯一正式candidate前置owner；既有重物legacy consume flag在这些已接管的候选上传false，避免同一候选两次release随机。ZeroAttackerHp旧consume位置保留；kind非0仍沿既有vrest及consume路径。本步不修改LF2Entity旧兼容helper，也不私自扩展noncharacter reduced damage实现。暂存的plan是局部值类型，不写persistent schema，无队列/pool/注册；停止上游tick/input即停止该writer，实体记录仍由既定关闭阶段回收。

计划完整写入：reciprocal原始链接及2/-2判断；同步0xEC/6写child Native raw action、清两link state、保留槽历史；attacker.ItrRest[child]=45、target.ItrRest[child]=30；target Vy原精确常量，child Vy不改；special suppression按Native action_latch帧首bdy/state、property、effect/caughtact，支持dormant。type1 broken fallback的-1在prelude前写；首type2先prelude再unsupported。type0非角色反馈在普通rest及firstbody之前。

验收先984 before/after实际路径量化；Shadow此前观察旧consume的位置必须后继同步，不能关掉Shadow或以raw通过替代完整验收。任何Shadow脚本改动前进一步登记准确路径。回滚只本差量，保留原RED和上一批用户工作，不改Unity/GAS/Scene/资源/Server。

第一阶段实际三生产路径已写，compile0错误；fa73e4ba两profile各984 before0，after由4641/712case降为1518/232case，210feedback无首差，其余集中既有weapon/reduced分支，完整FAIL保留after-native-prelude。

修改Shadow前声明准确第4生产脚本BattleEcsHitExecutionPlan：新前置观察以局部值类型token携带扩展ConsumeEffectsSnapshot，不新增队列/持久schema；Capture/独立projection覆盖target Vy、broken armor值、原45/30 rest方向、child raw frame与保留counter/slots、完整Native随机scalar。用只读synchronized cursor预测0xEC，不调用实际writer或改变全局RNG。通过现有world.HitExecutionPlanForInteractionModule接入，prelude前后独立观察，ZeroAttackerHp旧consume仍在原位置；Entry标记前置已观察，旧consume不再重复预测legacy heavy release。

同时使Shadow的kind0 disposition推迟vrest，feedback writer预测只追加spark，不走HP/武器反应预测；保持其它已有观察与失败标记。同一新增测试加Shadow两profile，保留full984失败和所有未覆盖项；不能用关闭Shadow或不观测新事务换取通过。

Shadow已写局部token前后观察及Entry当前pass标记；扩展Snapshot字段只在NativePrelude token中启用指纹/差异检查，原legacy consume观测合同保持。新增只读diagnostic计数随原Plan.Reset归零，测试每个候选断言1次新前置观察，避免没执行观察却认为Shadow已过。仍无persistent schema/新资源owner。

d83b7c四个完整984组仍FAIL但各1518条/232case；Shadow额外错误0、每例前置观察恰好1，上一批34回归全PASS。按原source结果单列684个前置/反馈/拒绝契约（210 feedback+294 rejected+180 unsupported），保留全984测试不变与所有失败，新增focused和两factory/direct-Shadow Play2736验收；反馈还明确断言writer观察1次，不将该子集合等同完整984已过。

当前四生产文件实际职责已落地：NativeOrdinaryHitPrelude计划/写入；Resolver专用native入口；Runner延后rest/firstbody并唯一调用前置、旧heavy flag避免重复；HitPlan新增独立前置token预测、保留legacy consume、feedback独立spark预测。d83b7c最终38项34PASS/4完整矩阵FAIL，四组均1518条/232case，所有Shadow额外错误0。旧34回归未退化；仍待focused684/selfcheck/Play和剩余damage子任务，不提前关闭父包。

focused684四组均PASS，Bdefend重接正式入口后direct192/Shadow208，剩64武器反应及16type5覆盖guard；完整SelfCheck09:17:43Z PASS。真实Play684×两factory×direct/Shadow=2736 PASS、before/after0差异、Scene checksum保持、Renderer2→2；有序关闭PASS/全0/两帧Stopped。最后补同测试文件本地回放：先执行源前置向量并验证，再在完整tick1边界capture，恢复后tick2/3检查checksum/rest/links/sparks/Native随机及Shadow。禁止把手工C11中途快照当正式恢复边界，不改Snapshot/Server策略。

首个本地回放测试两组FAIL仅首先发现NativeRandom.SynchronizedGeneration 2→3；读取现有NTSD28NativeRandom.TryRestoreScalarState/ResetFromSeed/AdvanceSynchronizedGeneration确认它是故意失效旧只读cursor的内部代次，不是还原的战斗随机状态。保留FAIL，测试签名改为比较CRT state/count、table seed/hash、同步counter/index/calls/lastSite，并追加旧cursor在恢复后不可commit断言；不修改随机实现或已知canonical allocation epoch恢复问题。

当前检查点IN_PROGRESS / PRELUDE_FEEDBACK_RUNTIME_PASS_DAMAGE_DEPENDENCIES_OPEN。前置684四组、Play2736/关闭、SelfCheck09:17:43Z、本地16场景/32replayed ticks均PASS；完整984仍四组各1518/232case，不能标全包VERIFIED。下一unarmored weapon→type5覆盖→noncharacter reduced；详细报告已重写，原RED报告保留。

依赖回访更正2026-09-15：weapon/type5/matched/reduced及gain已完成各自限定验收，原984/684/Bdefend/prelude replay已通过；qualification四个完整driver于job65d082698b534717b51999e3be656a5b 4/4 PASS（证据见reduced parent artifact qualification-driver-revisit-4-pass.xml）。旧232/108或Bdefend/Spark首差不再当前阻塞。父Record仍需逐项核对自身明确Play/关闭出口，不以本追加自动扩大VERIFIED；不得重做已过端点矩阵。

VERIFIED / DECLARED_PRELUDE_FEEDBACK_TRANSACTION。依赖weapon/type5/matched/reduced/gain已限定验收；原984/684/Bdefend/prelude replay通过，当前full driver四组EditMode4/4及真实Play driverOnly8例16:54:03Z PASS，Scene checksum不变/Renderer2→2；16:55:03Z关闭PASS，restore4→4、World/slots/两pool0、两帧Stopped。现有端点Play480/读帧Play168/前置Play2736及各自SelfCheck、当前完整SelfCheck16:36:40Z与reduced Play3948共同闭合声明出口，非全部reader或B5/B6/Q06完成。新增证据在qualification artifact driver-play-8-pass.json / driver-play-shutdown-pass.json；旧失败保留。
