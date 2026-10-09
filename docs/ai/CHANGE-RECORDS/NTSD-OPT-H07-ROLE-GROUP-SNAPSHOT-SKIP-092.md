<!-- CHANGE-RECORD
id: NTSD-OPT-H07-ROLE-GROUP-SNAPSHOT-SKIP-092
status: FOCUSED_TEST_PASS
code-path: Assets/NTSD/Scripts/Animation/Character/BruteForceSceneQuery.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleHitGroupEligibilityResolver.cs
code-path: Assets/NTSD/Scripts/Test/Editor/RoleAwareCollisionShadowSelfCheckTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleBruteProductionAdmissionEditorTests.cs
authority: effective six-item contract sections0-8; current336 unchanged; existing pure group rejection and full snapshot semantics
evidence: docs/ai/TASKS/NTSD-OPTIMIZATION-BATCH92-ROLE-GROUP-SNAPSHOT-SKIP-20261009.md
-->
# 第92批Role阵营拒绝避免无用完整快照

PLANNED；准确路径、方法、规则/字段、局部矩阵、owner/容量/关闭/权限/恢复见Task及同名Operation。只有default-off现有精确record路径候选，无生产推广/索引变更；接受路径保留完整payload，拒绝witness不能存入候选。保留旧pair capture控制与例外/RNG/顺序。脚本修改前创建，实际代码/测试/成本/失败和未验逐次追加。H07仍FAIL/Goal active。
2026-10-09 test-first有效RED：原Editor job d171ff0ae8ba4653bb20b7c3b70aa812，24/24具名case全部missing-property预期FAIL；新absent red-01/tests.xml保留。已写仅新测试，运行时代码待；9dirty副本/1000保护/5治理入口事前登记完毕。第92批已实际执行，累计71，阶段5of6/H07 FAIL/Goal active，不是新增性能收益。
实现事前修正：保持原Resolve(fullpair)及freeze函数主体原样，避免default OFF也增加witness转换成本。新的ResolveWitness仅在候选ON里，将已验证的13字段投影为仅filter使用的stack输入，调用原Resolve；此局部filterInput绝不写SceneQueryHit/store/publication/checksum。Task原“原Resolve转witness”由此最小表示取代，规则只有原函数一份，不增加heap/owner。旧Factory及kind5 holder路径未改，接受仍Capture完整payload；source/status仍待GREEN。
第一版GREEN失败/未采纳：job0b6659fd957a4264a43df95364ac50f2，125/124/1，原件green-first-01/tests.xml。唯一warm-fixture未设Role导致actualSkip0，不是GC失败；全部候选payload/序列/RNG/group pure/atomic原断言通过。两局部成本均反向且onCapture半数；排除重复witness构造后追加精确方向stack复用及group拒绝该ITR剩余body的无副作用早退，见Task追加事前矩阵。未启动Driver/Play/实际AI或改默认，失败保留，先保存三C#当前版本再修改。

2026-10-09 V2已完成局部GREEN：job ffc275cda40048d18fea753ac4cebac1，125/125 PASS/0 skip；新原件 green-02/tests.xml SHA E0C707020E6404733E7657B88C362A6CB36CDDA790BCA671BDDA12724E454EA2。分散11.8481375→11.191925ms（-5.54%），集中155.5233375→134.3128625ms（-13.64%）。仅managed collector局部收益信号，非真实AI/完整Driver或FPS证明。累计71已执行/阶段5of6/H07 FAIL/Goal active，默认OFF。

## Driver资格扩展（脚本前冻结）

仅新增第4准确C#写域 Assets/NTSD/Scripts/Test/Editor/BattleBruteProductionAdmissionEditorTests.cs；其dirty当前版已保存到before-03-driver，SHA650E5F8ECDF7C0ADBD7EE4578B908EF60AB6A9E34EF9D6F34FB9F49F7510C7D5，before-manifest-02继承前件且排除此写域，其余999保护不动。

新增RoleGroupSnapshotDriver_五case：default OFF；正式root左右各12tick（冻结trace/current正式EXE指纹，原oid/action/hp/vx对照）；实际OID1 Sakura、DataOrientedCanonical AI1000的Dispersed1000/Combat1000各32tick，同输入OFF/ON比较每tick extended/lockstep checksum、完整native scalar与legacy RNG；actual AI1000、实际Role路由、不abort、prepared数组identity/容量不变、三项关闭归零。真实千人必须actual group rejection skip累计>0，不松断言刷PASS，无应用即NOT_APPLICABLE、不进耗时window。

新增私有RunRoleGroupSnapshotDriver及IDisposable冷owner，唯一差异为explicit query flag，finally恢复默认OFF后原Driver有序关闭。旧Run仅在原最后composeEnvelopeBindingEligibility之前增加nullable roleGroupSnapshotSkip，默认null/不改变旧调用；新TickRow字段roleGroupSnapshotSkipEnabled/roleGroupSnapshotSkips，仅显式资格输出。禁止与84/87/89等未准入候选组合，不改旧测试、native原trace、Suite或生产接线。保存原source备份后反射missing-helper RED5，再实现，GREEN五新+三个旧default/composition接口守卫（明确匹配名称后执行），不是重跑旧无收益成本。

诊断输出仅本ID新absent driver-qualification-01/ 内 SaveNew(CreateNew,Guid)左右root与两千人layout的四JSON命名族，保留actual filenames/SHA；RED/GREEN XML另新目录不覆盖。完整Driver 0GC/性能、1800 formal窗口、native全域/逐位最终认证仍未验；共享stress request/result保持不写、非Play/Menu clean8、无Profiler或M0。本次输出性质为资格而非耗时比较。该扩展不改运行时算法或目标边界，回滚仅本hunk/上述当前dirty副本须用户批准。
Driver有效RED job4776f4778ad44d7a90b1bdf60b0352a0，5/5具名新case全部missing RunRoleGroupSnapshotDriver FAIL，原件driver-red-01/tests.xml；999保护/HEAD/暂存/request核证不变，Menu空闲重载已完成。仅测试新增已编译，开始原合同明确的冷owner和nullable Run扩展实现，不修改旧测试。
2026-10-09 Driver资格最终PASS：job879ef968328a43358891f092194814d2，8/8 PASS/0skip，UTC02:51:09—02:59:19；driver-green-01/tests.xml SHA3D5B09FDA1838EAAD871B75398428AB9F778C83CD9FBE013C279139DF8E0E1E6。88配对tick/176完整step：左右12+12、Sakura1000AI两32+32；extended/lockstep hashes与完整native/legacy RNG逐tick无first-difference，三prepared数组identity/容量原断言通过、actualRole/noabort、AI两布局全tick1000。OFF0，ON D576450/C515802实际拒绝出口，三项关闭0，owner恢复默认OFF后Shutdown；四个CreateNew Guid JSON文件在driver-qualification-01且各SHA见最终audit，performanceEvidence=false/gcEvidence不作证书。原Editor非Play/空闲/唯一Menu clean8。FOCUSED_TEST_PASS / LOCAL_GAIN_SIGNAL / DRIVER_QUALIFIED / DEFAULT_OFF；H07仍FAIL，累计71/阶段5of6/34父0/Goal active。本批未做新真实耗时、完整Driver0GC/1800/native全域/device/GPU/M0。下一最小动作是独立准确窗口合同中只差此flag的真实千人OFF/ON一次对比；不重测125局部、此资格、V1失败或旧89，不自动推广collector或候选。
最终静态/运行后核证：999范围外保护0drift；10份事前当前dirty备份与3份V1准确版本均0drift，Menu/Battle及Packages原指纹保持；共享stress requestabsent/result未写，暂存空、diff --check退出0/仅8行末提示。外部HEAD于2026-10-09T10:55:12+08:00从a10c358eeb956f2153e7d5bfbb60c3c1b353fd50推进至a0ecb01588e77feeda7fd369bc2011fc099b1173（旧HEAD为祖先，已包含本批四C#，文件bytes指纹未改）；/root未执行commit/丢弃/恢复。普通Validator退出0/1387Record/0当前code diff/4461旧declared-not-diff warning，不把0diff当覆盖；额外对四准确C#各用-SimulateChangedPath只读校验，均exit0/1governed/92 Record覆盖/0ERROR，WARN4416/4458/4437/4446；外部提交已使旧baseline diff变化，不回退用户提交。具名全部证据与SHA见artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH92-ROLE-GROUP-SNAPSHOT-SKIP-20261009/audit-final-01.json，最终文件manifest将保存after-manifest-01.json。旧事前“HEAD同”只描述当时核证，不是当前HEAD结论。
