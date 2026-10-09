# 第92批：Role 阵营拒绝前置，避免无用完整pair payload

PLANNED；Change NTSD-OPT-H07-ROLE-GROUP-SNAPSHOT-SKIP-092，Operation同名。上一goal turn重新扫描空间路径，确认已有Direct/Sweep/tree，不应重新发明Grid；本轮由91 PairExactLoop约8.1/8.4ms及83 CandidateRecord相对热点继续。89仅帧引用复用已无一致收益，本候选不同：阵营已拒绝时不读取PreviousFrame、tick frame重新解析或linked holder，不构建完整pair payload。

## 准确写域

- Assets/NTSD/Scripts/Animation/Character/BruteForceSceneQuery.cs：default-off EnableRoleRejectedGroupSnapshotSkipForDiagnostics、每次Collect复位LastRoleRejectedGroupSnapshotSkipCountForDiagnostics；RecordOverlappingBodyCandidatesCached几何/CandidateAccepts之后且完整snapshot尚未捕获时，使用已验证当前participant和live group/action/facing形成typed eligibility witness，调用同一个纯resolver，拒绝仅跳过完整snapshot/后续无副作用分支。ForceLegacy/after-pair测试hook/输入fallback/Brute不走候选。
- Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleHitGroupEligibilityResolver.cs：独立readonly BattleHitGroupEligibilityWitness，仅包含Valid、OID/type/group/action/current-state/facing；原Resolve(fullpair)转该witness，ResolveWitness复用原分支；Kind5 holder特殊路径保留原fullpair。拒绝witness不得作为SceneQueryHit/store/checksum完整payload。原public/internal API签名保持。
- Assets/NTSD/Scripts/Test/Editor/RoleAwareCollisionShadowSelfCheckTests.cs：新增GroupSnapshotSkip_反射test-first、same-team拒绝实际capture减少、不同队完整payload、kind/state例外、生产producer三模式、Brute/fallback/legacy/hook不触发、later collection live更新、zero warmed allocations；复用现有fixture与pair比较。

## 容量与关闭

仅stack value witness，无新的heap cache/container/worker/owner/slot，不跨direction/target/tick持有；旧prepared容量、seal、fallback和11阶段关闭保持。Role flag默认false、不连生产Driver/profile/Suite。可溢出counter为long，无容器增长；候选完整顺序及RNG保持。

## 具名验证与出口

新missing-property反射RED有效选择后实现。GREEN选择所有GroupSnapshotSkip_及原PairSnapshotReuse_/SnapshotFrameReuse_、两个既有HitGroupEligibility pure/atomic完整类（规则resolver受影响）。1000 managed fixture两布局8warm+16sample，OFF/ON交替且两顺序，候选序列/全部pair payload/RNG/源计数同，实际skip>0；不当实际1000AI/FPS，收益不明/反向即NOT_ADMITTED，不重复本窗找PASS。任何后续真实Driver/window需另追加精确Task/Change路径和备份；本轮不启动1800formal或M0。

正式EXE本轮SHA336B44已核；current source battle_world.cpp candidate_passes_native_group_filter当前只读4918附近的纯字段分支及Unity原resolver一致。此为现有纯计算表示优化，不改变native规则；未重新闭合全82源码与正式EXE行为，不主张native全域验收或默认推广。old snapshots/previous-state/effect后消费保持fullpair。33ms/3ms/max2、checksum/RNG/pass、readonly publication、排序/segment/fail-closed/十一阶段及专项USER_HOLD保持。

## 恢复及进度

before01为9dirty当前副本/1000保护、HEAD同/暂存空、唯一Menu clean8/Editor idle；先Task/Change/Operation/INDEX后脚本。恢复须明确授权，仅本包hunk或准确dirty备份，不恢复HEAD。70已执行/阶段5of6/34父0/H07 FAIL/Goal active；本候选限定完成不等Goal完成。下一按实际两布局成本决定准入或换同范围有据方向。
2026-10-09 精确候选修订（先留失败原件及before-02-first-candidate三个准确当前版本）：第一版job0b6659fd957a4264a43df95364ac50f2，124/125PASS，唯一warm-GC fixture末断言skip=0因未显式设置Role collector；0B断言已过。原成本分散11.39184375→11.67753125ms、集中155.6794375→163.83003125ms均反向，捕获减半但每body重复生成witness，不能进真实窗口或声称收益。新版本在CollectCandidatesForPairCached方向stack仅捕获一次witness（不跨target/tick），Record收到ref witness/bool；几何/CandidateAccepts后首次纯group拒绝直接return该ITR的后续body循环，因group字段无body依赖、拒绝前无kind4/RNG/写入。不同ITR仍各用原kind/effect判断，接受仍完整Capture；counter计实际group拒绝出口，不表示少捕获的精确次数。同队fixture2ITR因此新预期2，不是6次body重复判定；warm fixture显式Role+snapshot初始准备，不改生产或原0B断言。追加本版本一次同矩阵局部成本；两old89/31本身的cost不重跑，不重复第一版找PASS。原“Resolve转witness”已更正为仅候选ON的typed witness→filter-only输入调用原Resolve，默认路径原函数不变。

2026-10-09 V2已完成局部GREEN：job ffc275cda40048d18fea753ac4cebac1，125/125 PASS/0 skip；新原件 green-02/tests.xml SHA E0C707020E6404733E7657B88C362A6CB36CDDA790BCA671BDDA12724E454EA2。分散11.8481375→11.191925ms（-5.54%），集中155.5233375→134.3128625ms（-13.64%）。仅managed collector局部收益信号，非真实AI/完整Driver或FPS证明。累计71已执行/阶段5of6/H07 FAIL/Goal active，默认OFF。

## Driver资格扩展（脚本前冻结）

仅新增第4准确C#写域 Assets/NTSD/Scripts/Test/Editor/BattleBruteProductionAdmissionEditorTests.cs；其dirty当前版已保存到before-03-driver，SHA650E5F8ECDF7C0ADBD7EE4578B908EF60AB6A9E34EF9D6F34FB9F49F7510C7D5，before-manifest-02继承前件且排除此写域，其余999保护不动。

新增RoleGroupSnapshotDriver_五case：default OFF；正式root左右各12tick（冻结trace/current正式EXE指纹，原oid/action/hp/vx对照）；实际OID1 Sakura、DataOrientedCanonical AI1000的Dispersed1000/Combat1000各32tick，同输入OFF/ON比较每tick extended/lockstep checksum、完整native scalar与legacy RNG；actual AI1000、实际Role路由、不abort、prepared数组identity/容量不变、三项关闭归零。真实千人必须actual group rejection skip累计>0，不松断言刷PASS，无应用即NOT_APPLICABLE、不进耗时window。

新增私有RunRoleGroupSnapshotDriver及IDisposable冷owner，唯一差异为explicit query flag，finally恢复默认OFF后原Driver有序关闭。旧Run仅在原最后composeEnvelopeBindingEligibility之前增加nullable roleGroupSnapshotSkip，默认null/不改变旧调用；新TickRow字段roleGroupSnapshotSkipEnabled/roleGroupSnapshotSkips，仅显式资格输出。禁止与84/87/89等未准入候选组合，不改旧测试、native原trace、Suite或生产接线。保存原source备份后反射missing-helper RED5，再实现，GREEN五新+三个旧default/composition接口守卫（明确匹配名称后执行），不是重跑旧无收益成本。

诊断输出仅本ID新absent driver-qualification-01/ 内 SaveNew(CreateNew,Guid)左右root与两千人layout的四JSON命名族，保留actual filenames/SHA；RED/GREEN XML另新目录不覆盖。完整Driver 0GC/性能、1800 formal窗口、native全域/逐位最终认证仍未验；共享stress request/result保持不写、非Play/Menu clean8、无Profiler或M0。本次输出性质为资格而非耗时比较。该扩展不改运行时算法或目标边界，回滚仅本hunk/上述当前dirty副本须用户批准。
Driver GREEN精确选择补记（执行前）：RoleGroupSnapshotDriver_全部五case，加RoleDriver_DefaultBruteAndExplicitQualificationOnly、RoleCapacityDriver_DefaultOffAndExplicitQualificationOnly、EnvelopeBindingEligibilityQualificationDoesNotPromoteDefaults三既有接口/default守卫，共8case。未选择旧千人成本或84 Explicit未触发失败；使用同一原Editor EditMode。
2026-10-09 Driver资格最终PASS：job879ef968328a43358891f092194814d2，8/8 PASS/0skip，UTC02:51:09—02:59:19；driver-green-01/tests.xml SHA3D5B09FDA1838EAAD871B75398428AB9F778C83CD9FBE013C279139DF8E0E1E6。88配对tick/176完整step：左右12+12、Sakura1000AI两32+32；extended/lockstep hashes与完整native/legacy RNG逐tick无first-difference，三prepared数组identity/容量原断言通过、actualRole/noabort、AI两布局全tick1000。OFF0，ON D576450/C515802实际拒绝出口，三项关闭0，owner恢复默认OFF后Shutdown；四个CreateNew Guid JSON文件在driver-qualification-01且各SHA见最终audit，performanceEvidence=false/gcEvidence不作证书。原Editor非Play/空闲/唯一Menu clean8。FOCUSED_TEST_PASS / LOCAL_GAIN_SIGNAL / DRIVER_QUALIFIED / DEFAULT_OFF；H07仍FAIL，累计71/阶段5of6/34父0/Goal active。本批未做新真实耗时、完整Driver0GC/1800/native全域/device/GPU/M0。下一最小动作是独立准确窗口合同中只差此flag的真实千人OFF/ON一次对比；不重测125局部、此资格、V1失败或旧89，不自动推广collector或候选。
最终静态/运行后核证：999范围外保护0drift；10份事前当前dirty备份与3份V1准确版本均0drift，Menu/Battle及Packages原指纹保持；共享stress requestabsent/result未写，暂存空、diff --check退出0/仅8行末提示。外部HEAD于2026-10-09T10:55:12+08:00从a10c358eeb956f2153e7d5bfbb60c3c1b353fd50推进至a0ecb01588e77feeda7fd369bc2011fc099b1173（旧HEAD为祖先，已包含本批四C#，文件bytes指纹未改）；/root未执行commit/丢弃/恢复。普通Validator退出0/1387Record/0当前code diff/4461旧declared-not-diff warning，不把0diff当覆盖；额外对四准确C#各用-SimulateChangedPath只读校验，均exit0/1governed/92 Record覆盖/0ERROR，WARN4416/4458/4437/4446；外部提交已使旧baseline diff变化，不回退用户提交。具名全部证据与SHA见artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH92-ROLE-GROUP-SNAPSHOT-SKIP-20261009/audit-final-01.json，最终文件manifest将保存after-manifest-01.json。旧事前“HEAD同”只描述当时核证，不是当前HEAD结论。
