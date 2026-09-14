# 非角色普通命中前置与护甲反馈实施检查点

IN_PROGRESS / PRELUDE_FEEDBACK_RUNTIME_PASS_DAMAGE_DEPENDENCIES_OPEN。前置/反馈部分已有新鲜运行证据；完整984矩阵仍有后续damage差异，父包及总目标未关闭。

## 实际修改

- BattleNativeOrdinaryHitPrelude：局部immutable plan及无资源writer，复用既有armor/defense resolver；原始reciprocal关系检查、0xEC/6同步随机→child Native raw action、清2/-2保留槽历史、正确45/30 rest方向、精确target Vy、special-link-rest及dormant支持、broken armor -1时点。
- BattleOrdinaryCharacterDamageRouteResolver：新增专用ResolveForNativeEntry，支持当前非角色定义，并保留type1直返与普通defense rest早返后fallback的差异；原角色Resolve入口保留。
- BattleHitCandidateSequenceRunner：对原kind0/4/5且effective kind0延后live vrest；先native prelude，再unsupported/feedback/普通rest，再first-body。首type0非角色feedback跳过伤害和first-body；已接管的heavy consume flag不再二次执行旧release，旧ZeroAttackerHp消费位置保留。
- BattleEcsHitExecutionPlan：独立NativePrelude value-token前后观察，使用只读同步随机cursor预测，不执行actual writer；验证rest、速度、帧及保留字段、完整Native随机scalar。旧consume保留，已观察前置不再重复预测legacy heavy release；feedback writer只投影spark。每个候选断言一次前置观察，feedback断言一次writer观察。

没有新增manager/queue/pool，没有persistent schema变更，未改Unity/GAS框架、非战斗功能、Scene、资源、shared Kernel或Server。

## 已运行验证

- Unity编译0 error。38项回归任务d83b7c：原Spark/C01/完整driver/local replay相关34项全部PASS；新增完整984四组FAIL仍保留。
- 完整source984 × 两profile × direct/Shadow：before受测raw47/3+links/rest/sparks均0差异；after每组从4641条/712case降到1518条/232case。四组一致，Shadow额外错误0，每例前置观察恰好1。原三种记录及全过程失败分别保留red/、after-native-prelude/、after-shadow-prelude/。
- source结果定义的684个前置/反馈/提前返回契约（210 feedback、294 rejected、180 unsupported），四组均PASS，共2736例。完整984测试仍保留，没有把该子集合说成所有damage已对齐。focused/tests-8.xml同时保留Bdefend父失败。
- BDEFEND256测试的direct组纠正到与C++完整ordinary入口对应的正式候选流水线；原raw/HitStateCount241/C25恢复断言保持。当前每profile direct192、Shadow208，正好64武器反应×3 raw字段加16无armor type5预测覆盖guard。独立Record BDEFEND-FORMAL-CANDIDATE-ENTRY-ORACLE-001仍等待父损伤依赖闭合，不把四组FAIL标通过。
- 完整BattleRuntimeSelfCheck于09:17:43Z PASS，结果selfcheck-pass.txt。之后只有测试/文档修改，已复核生产hash保持。
- 真实NTSD_Battle Play：两factory × direct/Shadow × source684，共2736例PASS，before/after differences均空，Scene checksum不变，Renderer借用2→2。见play-2736-pass.json。
- 现有有序关闭验证PASS：原位恢复4→4，World对象、slots、logic/render borrowers全0，连续两帧Stopped。见shutdown-pass.json。Scene未保存，文件hash保持bcd1047b…。
- 本地回放：8源场景 × 两profile=16，先执行并验证前置事务，再在完整tick1边界capture，恢复后tick2/3共32个replayed ticks。完整checksum、3x3rest、链接历史、spark、CRT和同步随机实际状态一致，Shadow有效；replay-final-2-pass.xml两组PASS。
- 首次回放仅因测试把内部cursor失效代次SynchronizedGeneration纳入游戏状态比较而失败，原XML保留replay-diagnostic-generation-fail.xml。现有随机Restore故意推进该代次；测试改比较实际随机状态并新增旧cursor不能commit断言，随机实现和canonical allocation epoch政策均未修改。

## 剩余差异和下一顺序

完整984剩232case =124个无护甲/绕过护甲的武器命中 +108个非角色reduced命中（90 type1 active、18 ordinary defense）。这些仍是实际战斗差异，不能用Shadow与旧backend相同来抵消source首差。

1. 下一唯一执行Task UNARMORED-WEAPON-REACTION-001。处理完整frame/team/hit reaction/rest/legacy随机尾部；不只把某个夹具动作改成186。旧Bdefend64和新984中的124例均应作为回归入口，并按源补必要state/height边界。
2. TYPE5-HIT-PLAN-COVERAGE-AUDIT-001，剩无armor16 guard；首type0反馈的16个guard已消除。
3. NONCHARACTER-REDUCED-HIT-TRANSACTION-001，接正确reduced backend完整HP/资源/credit/type6例外/durability/rest/动作/声音/spark顺序，不能只取消角色类型guard。
4. 回完整984、BDEFEND256和父collision/qualification；reader/display其它职责完成后才Q07正式DAT/角色图迁移。

当前新代码是候选流水线的正式前置owner，typed damage writer仍是路由后的片段；不要为了错误测试边界在两个层级重复消耗RNG。完整raw仍47 bound/3 missing，跨World allocation epoch缺口、stage.dat USER_HOLD及所有用户例外保持。禁止computer-use。总目标ACTIVE / FULL_ALIGNMENT_INCOMPLETE。
