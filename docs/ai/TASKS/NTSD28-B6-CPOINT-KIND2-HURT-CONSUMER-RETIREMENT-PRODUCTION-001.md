# NTSD28-B6-CPOINT-KIND2-HURT-CONSUMER-RETIREMENT-PRODUCTION-001

本合同在本轮任何脚本修改前建立。状态：BLOCKED / PRECHANGE_SCOPE_BLOCKED / PRODUCTION_UNCHANGED_FROM_P3。
需求来源：用户2026-09-10批准三个独立退休包并限定文件范围，硬性要求既有测试保持PASS；Goal14 triage仅为定位线索，当前Authority需重新核验。
范围：退休无Authority的CPoint kind2 hurt覆盖；字段/converter保留，旧snapshot布局可读取但续跑规则改变，不承诺旧规则等值重放。
原状与实际路径：
- Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs
- Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterDatHitResolver.cs
- Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterHitResolver.cs
- Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs

新增focused测试及meta已获授权，尚未创建；实施前须将实际测试路径登记code-path。无新runtime模块、无关闭阶段变化。
前置：包1沿当前playable caught/damage/settlement链正向证明无hurt consumer；包2/3完整repo caller/serialized/反射名称审查，发现生产caller即硬停。当前整体暂停源于旧测试范围冲突，见Temp/Goal17_PrechangeScopeReview.md；不得擅自修改未授权旧测试。
不变量：schema/Snapshot/Checksum/shell、NTSDSpec本体、DAT/converter、Scene/资源不动；不引入FrontHurtAct/BackHurtAct替代，不修改其他effect/impact/caught逻辑，不清理用户修改。
验收：新focused先RED后GREEN；三包一次共享B6(583+新增)、前置92/80/17/72/24/23/32/140、refill9、fullSelfCheck、双build0error、validator、固定Scene SHA/dirtyfalse。新测试失败与旧测试失败分开报告；本轮尚未运行上述检查。
副作用/兼容：仅改变各自退休行为；旧snapshot字段布局不变，但不能承诺旧行为续跑等值。外部程序集/API兼容与repo内部caller区别记录，不凭repo扫描断言不存在所有外部调用。
回滚：用户明确批准后仅反向本包增量，保留当前P1/P2/P3未提交基线。当前脚本增量为0，无需回滚。禁止Git清理/提交/push与跨包顺手修复。
当前验证：只读工作树、源码和Unity状态检查；未运行本轮RED/编译/回归/Play。没有报告新测试失败；这是确定的事前授权范围冲突。


## 2026-09-10 用户追加授权恢复
IN_PROGRESS / TEST_FIRST。用户确认恢复本Goal17三包；此前范围阻塞解除。新增测试路径：Assets/NTSD/Scripts/Test/Editor/NTSD28B6CpointKind2HurtConsumerRetirementEditorTests.cs及meta。全批次一次共享回归；P3 Record保持已确认VERIFIED/USER_REVIEW_ACCEPTED，不再修改。
复核方穷尽证明由用户明确接受并要求直接引用：playable闭包cpoint injury/cover唯一消费块battle_world.cpp6090-6165为持有扣血/资源转移/scale340/KO/HP/effective_max/3/score和cover1/2/3计时器门；6183、6194-6196为cover%10与/10正反朝向门；2815为fusion资格、2418为frame_0mp保护，均无hurt动作覆盖。以这份完整正向证明作为退休依据，不以关键词未命中裁决。持有drain/recovery完整家族依赖B11/B8延后，本包绝不实现。追加旧测试仅SelfCheck14887-14907/18545-18567、HitPlanTests1522-1647、CPointResolvedTests13-121。仅修hurt覆盖或被删helper调用，其它语义不动，类外硬停。新值按standard damage链另记证明。旧snapshot字段仍可读取但续跑采用新规则，不能宣称旧规则等值。

包1新期望正向推导：resolve_standard_damage_interaction(battle_world.cpp4655起)解析kind0 ITR，正式unarmored消费6744-6748调用resolve_unarmored_reaction(1162-1220)。type0 ground(整数Y==collision_reference)、HP>0、fall1/10且无frozen/falling前态时进入0<timer<=20档(1211-1213)，action220；因此SelfCheck14892/14897和HitPlan双朝向应为220，方向只在air或21..40档决定222/224。type3在1182-1188提前返回且selected_action保持无动作，故BATTLE-C30 fall1/currentaction0保留0，而非310/320。Fall80路径与HP93/关系断言不改。UnityApplyStandardFall(2228起)及ApplySpecialObjectHurtTail(1721起)已有上述主体，此包仅删后置cpoint覆盖，不改主体。converter旧tests改为直接读现有valueadapter字段保留数值，禁止反向重建consumer。


最终状态VERIFIED / SCOPED_RETIREMENT / REVIEW_PENDING。已验证的范围为Unity CPoint kind2 hurt动作覆盖consumer退休。四个生产文件全部对应调用/helper/投影已删除，不用FrontHurtAct/BackHurtAct替代。数据结构、converter及snapshot字段均保留；旧布局仍可读取，但续跑使用当前规则，不承诺历史规则/旧checksum结果等值。Authority持有扣血drain/recovery语义仍受B11/B8前置约束，未在本包实现。
旧期望修改全部位于用户追加范围：SelfCheck原14892/14897的230/232改220；原18546/18549的310/320改0；HitPlan原1524方法名改Ignores，原1643-1644两动作期望改220，原230/232 fixture sentinel、tick、候选/writer数量与关系断言保持；CPointResolvedTests原25/30/46/51/69/74/90/95/114/119共10个helper调用改全限定NTSD.Simulation.BattleCatchPointValueAdapter.FromLegacy(...).Injury/Cover，原converter/alias数值断言不变。16个连续diff块完整旧/新文本和当前行号见Temp/Goal17_OldExpectationSites.json。标准动作220/type3保留0的正向证明见前文1162-1220/6744-6748链。
新focused13项实测：RED10FAIL/3PASS→GREEN13PASS；real/shared双朝向230/232→220、type3双朝向310/320→0、Fall80和内容字段控制通过。没有新的Play Mode或正式EXE执行声明；本包验证级别为批准范围内的consumer退休、真实Unity生产方法定向调用和完整SelfCheck，不声明全B6对齐。
共享B6610、refill9、oldcompat7、fullSelfCheck、双build0error和固定Scene/范围检查已通过，完整证据见同IDRecord最终节。报告后停止。
