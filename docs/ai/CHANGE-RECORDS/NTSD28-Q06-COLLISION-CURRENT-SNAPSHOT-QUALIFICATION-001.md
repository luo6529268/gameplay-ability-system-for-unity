<!-- CHANGE-RECORD
id: NTSD28-Q06-COLLISION-CURRENT-SNAPSHOT-QUALIFICATION-001
status: IN_PROGRESS
change-kind: ORDINARY_COLLISION_SNAPSHOT_QUALIFICATION
code-path: Assets/NTSD/Scripts/Animation/Character/BruteForceSceneQuery.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleHitCandidatePairSnapshotFactory.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06CollisionQualificationEditorTests.cs
authority: Current BattleWorld28 scan_direction uses snapshot itr/body independently of scan_platform_direction current itr; current/previous state absent becomes0 in pair snapshot; original480 qualification cases and two full-driver kind3-change-before-hit witnesses.
evidence: Existing original336 comparison leaves24 extra current-null rejections per Unity route; Kind0EffectAllowed still legacy previous getter, pair factory rejects missing current rather than state0.
-->

# 普通碰撞与当前状态查询的职责分离

IN_PROGRESS / TEST_FIRST。准确三脚本。原480端点273候选/207过滤、273eligible在两current变1000后保持；完整driver两例均2候选、1抓取、1普通命中，slot1的current10/998无itr但snapshot0仍参与消费，slot2 HP499。先复用原输出写Unity RED，再编辑生产。

BruteForceSceneQuery预定准确符号：IsCollisionCandidateAttackerEligible；CollectCandidatesForPair及Cached；PassesReleaseCoarsePrefilter及Cached；RecordOverlappingBodyCandidates及Cached；TryRecordReleaseCandidate/TryRecordNearestPathCandidate；ItrAllowedCore/Kind0EffectAllowed；AcceptReleaseSelectFlagCandidate/AcceptReleaseKind1TowardVictim；ResolveReleaseRejectFlag/ApplyPrev2GroundRejectFlag；RuntimeConsumeItrAllowed；QueryBodyHits各战斗重载和QueryBodyHitsImmediate。其余作用保持：snapshot需有效且含几何；current仅供对应状态过滤、缺失取0；普通分支不额外要求current itr/body。cached与ordinary共同修改，不动平台操作current规则、空间索引布局或其缓存观测职责。

PairSnapshotFactory.Capture允许current缺失并记录currentState0，previous仍Native；snapshot双方无帧仍拒绝，符合geometry前置。Kind0EffectAllowed previous点查Native并缺失0，不改effect/group决策。禁止拿frame0/快照的state假冒缺失currentState0。

测试：480原case两profile、普通/role-aware，frozen pair字段和后继current1000的eligibility；原336所有旧失败必须重验，无豁免。真实driver两case另验证原HP/current/snapshot行为，明确CRT2和C17既有例外。若发现还需其他生产符号/文件先更新Record及source依据，不扩大到hit伤害writer、CPoint raw/throw、kind8旧lead-in等未审计reader。

风险：nullable current沿多层selector传递，需检查所有null早退和state读取、缓存一致性、actual/Shadow消费；禁止只改首个gate把错误移到下一层。编译、focused、相关旧回归、SelfCheck/真实Play及有序关闭验收后才能VERIFIED。无persistent/schema/关闭模块变化；回滚仅本差量且按规则授权。保留用户工作，禁止computer-use/Scene/资源/非战斗/GAS/Server改动。
已写新测试：480原端点×两profile×普通/role-aware，raw47、冻结pair非零state、实际候选与current1000后的分类；missing candidate明确失败，不用direct pair掩盖。两full-driver原输入尚待Unity联合测试。生产尚未改，先编译RED。

RED job465a71b72a0a4b96a3a7c3e782ea5411四组全部FAIL：明确复现current1000消费被拒、候选current body/itr/null门、previous声明999 state19的effect20过滤遗漏；before raw无差异。原JSON/XML归red。补充准确符号CandidateAccepts（只作入口检查，允许nullable current，不删除snapshot检查），其余原声明函数逐个按源修改，非battle模块不涉及。
生产两文件已写：ordinary/cached carrier及geometry/select链不再要求current itr/body/non-null，current state nullable0；pair当前null记0但snapshot需存在；Kind0EffectAllowed previous Native。Immediate/volume和consumer同步去current资格，保留snapshot有效性。实际还有ItrAllowed（Core直接caller）取消两处current/snapshot fallback，补记此前符号清单未单列该wrapper；没有新增文件或跨模块。准备编译与原480/336联合重验。
追加同测试脚本完整driver对照（修改前）：原两例×两profile×frame/physics两mode，NTSDBattleTickSystem完整RunReleaseTick+ShadowCompare、raw before/after、HP499及caught frame/snapshot0、CRT2和既有稀疏C17 legacy1；不预判通过，实际Shadow诊断单列。
新测试已追加两例完整RunReleaseTick+ShadowCompare（两profile/两frame modes），raw before/after、CRT2/C17计数和Shadow诊断输出。两生产文件未再新增改动；准备联合原336+新480+完整driver。

联合jobc7244b8f770b4e9aaf23983492a468a2终态12FAIL。480组每route由1390降为128，全是64个attacker current缺帧的候选/载体缺失；pair/分类/raw差异已清，最后定位CandidateAccepts仍检查其attackerFrame参数非null，而formal caller传的是current，不是snapshot。补齐同函数最后一门。两完整driver的HP499/抓取动作/快照均一致、Shadow有效/2effects/mask0；独立残留target bdefendAccumulator0/45及未细分RNG计数，先完善输出再按源追DAT默认与RNG，不改expected。原输出归after-first-fix。

最终gate后job106613fc5dd74f52954001db604d68bd实际8PASS/4FAIL：旧336四组和新480四组全部0差异，共3264端点；完整driver四组仍各4差异（两例×bdefend/RNG）。具体RNG legacy3/native0/CRT0，原CRT2；不是掉落例外可豁免的那1次。源combat_records将缺失bdefend读0，原battle_world6743无护甲分支直接bdefend_accumulator=45，排除DAT默认值假设。DatHitResolver.SpawnSpark828/829用BattleRandInt两次，为额外legacy调用明确入口。

新完整SelfCheck请求06:26:21Z，06:27:14Z PASS。下一仅同测试脚本追加真实Play probe：120选择（previous都0或current都0的原480子集）×两factory×两query=480，明确仅候选/冻结分类，完整driver缺口另任务保留；无需重跑未改生产的SelfCheck。

## 当前检查点06:40Z（仍IN_PROGRESS）

原336四组+新480四组共3264端点候选/pair/raw/classification全部0差异。完整driver四组仍FAIL：8例的bdefend0/45和RNG legacy3/CRT0对源CRT2，HP/caught/current/snapshot与其它raw一致、Shadow2effects/mask0。源45是battle_world无护甲分支显式覆写，非DAT默认；DatHitResolver Spark两次旧随机已定位。下一UNARMORED-BDEFEND-WRITER-AUDIT，再HIT-SPARK-TRANSACTION-AUDIT，最后回当前完整driver。

新SelfCheck06:27:14Z PASS；原70回归69PASS/1旧期望，独立修订后4矩阵PASS；Play480/Renderer2→2/Scene checksum保持；Shutdown06:38:10Z恢复4→4、World/slots/两pool0、两帧Stopped。最终CS0/Editor idle/Scene dirtyfalse/root14/hash bcd1047b…；接口6402由状态文件发现。完整证据见COLLISION-CURRENT-SNAPSHOT-QUALIFICATION-001/REPORT.md。资源/非战斗/框架未改，父不标VERIFIED。
