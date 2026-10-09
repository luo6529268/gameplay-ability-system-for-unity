# 第92批：阵营拒绝前置与完整快照省读候选

当前结果：FOCUSED_TEST_PASS / LOCAL_GAIN_SIGNAL / DRIVER_QUALIFIED / DEFAULT_OFF。H-07仍PERFORMANCE_FAIL，Goal active；累计71个已执行子批、六项有限首阶段5/6、34父项关闭0。此计数不构成停止条件。

## 改动与不变量

现有Role-aware cached exact路径，在几何相交与CandidateAccepts之后、首次完整pair快照之前，以只含group filter所需字段的stack typed witness调用同一原Resolve。方向内仅懒捕获一次witness；纯group拒绝时结束当前ITR的剩余body循环。拒绝分支没有候选写入、kind4 source count或RNG副作用。不同ITR仍按原kind/effect判断；接受仍捕获包含previous/tick/holder的原完整pair payload。witness不进入候选store/publication/checksum。

涉及BruteForceSceneQuery、BattleHitGroupEligibilityResolver及两个既有测试文件；后者Driver入口仅本候选的explicit冷owner，finally恢复false后沿原有序关闭。生产默认、collision backend、旧候选默认均未推广；旧89、84、87候选不组合。ForceBrute/legacy/fallback/测试mutation hook保持原路径。无新增heap缓存、worker、slot或热路径扩容；33ms/3ms/max2、publication只读、透明排序/segment/fail-closed和十一阶段关闭保持。未读取Q06活跃方法体。

当前正式EXE本批核SHA336B44；读取当前candidate_passes_native_group_filter纯字段分支及原Unityresolver，不以Unity定义规则。未重新运行全82 closure/native campaign；下面root对照仅原冻结四字段，不冒充原生全域逐位证书。

## 失败与修订必须保留

- 首次有效RED job d171ff0ae8ba4653bb20b7c3b70aa812：24个新反射case均预期missing property FAIL；red-01/tests.xml。
- 第一版GREEN job0b6659fd957a4264a43df95364ac50f2：125总/124 PASS/1 FAIL，green-first-01/tests.xml。唯一失败为warm fixture末断言actualSkip0，原因是未显式设Role；0B断言已通过，不是GC失败。未修改生产来满足夹具。
- 第一版局部成本：分散11.39184375→11.67753125ms，集中155.6794375→163.83003125ms，均反向，NO_GAIN/NOT_ADMITTED。捕获虽减半，每body重复witness成本抵消收益；不据此进入真实窗口，不覆盖失败。
- 保存三份第一版C#到before-02-first-candidate，核SHA；事前追加准确V2合同，再修为方向stack懒复用/纯group拒绝早退、warm fixture显式Role+初始snapshot准备。同队双ITRcounter为两实际拒绝出口，不是六次重复body判定。新V2只测一次相同矩阵，不重采旧89/84/31成本。

## V2聚焦与局部成本

job ffc275cda40048d18fea753ac4cebac1，原Editor实际125/125 PASS、0skip，UTC02:38:37—02:38:50；green-02/tests.xml SHA E0C707020E6404733E7657B88C362A6CB36CDDA790BCA671BDDA12724E454EA2。

准确选择：GroupSnapshotSkip_（24新），PairSnapshotReuse_(?!1000Participants)、SnapshotFrameReuse_(?!1000Participants)，两原HitGroupEligibility pure/atomic完整类，共101相邻，不重测其旧千人成本。

| managed1000 collector夹具 | OFF mean ms | ON mean ms | 相对变化 | OFF / ON full captures | ON group rejection出口 |
|---|---:|---:|---:|---:|---:|
| 分散 | 11.8481375 | 11.191925 | -5.54% | 15360 / 7680 | 15360 |
| 集中 | 155.5233375 | 134.3128625 | -13.64% | 614400 / 307200 | 614400 |

每布局两个相反顺序、每顺序8warm+16sample；同candidate序列、全部pair payload、RNG和source count；计时scope CaptureCollisionFrameSnapshotsAll→CollectCollisionCandidatesAll→End。夹具40attacker及其他body实体，无真实AI；cost scope GC=UNKNOWN。独立warm collector分配测试通过0B，仅覆盖其局部调用，不当完整Driver0GC/FPS/正式P95证书。时间有Editor/其他进程噪声，不凭此默认采用。

## Driver资格（非耗时窗口）

新增准确第4C#范围及before-03-driver/current dirty备份在脚本前写入Task/Record/Operation；before-manifest-02排除此写域，其余999保护核同。反射缺RunRoleGroupSnapshotDriver有效RED job4776f4778ad44d7a90b1bdf60b0352a0，5/5预期FAIL；driver-red-01/tests.xml SHA573FA116E0B9E36896EC2AB4DFF14E0B1EF59551E0C91F98D69B8AC4C0E5BA32。

GREEN job879ef968328a43358891f092194814d2已启动：五新case加三个原default/composition API守卫，共8。左右正式root各12tick，原oid/action/hp/vx对照；1000实际OID1 Sakura/DataOrientedCanonical AI两布局各32tick，逐tick extended/lockstep checksum、完整native/legacy RNG、actualAI1000、actualRole/noabort、prepared数组/容量、关闭三0；必须真正skip>0，否则NOT_APPLICABLE且不进真实耗时窗。不把桥接25秒观察超时当作作业失败或重启作业。

2026-10-09 Driver资格最终PASS：job879ef968328a43358891f092194814d2，8/8 PASS/0skip，UTC02:51:09—02:59:19；driver-green-01/tests.xml SHA3D5B09FDA1838EAAD871B75398428AB9F778C83CD9FBE013C279139DF8E0E1E6。88配对tick/176完整step：左右12+12、Sakura1000AI两32+32；extended/lockstep hashes与完整native/legacy RNG逐tick无first-difference，三prepared数组identity/容量原断言通过、actualRole/noabort、AI两布局全tick1000。OFF0，ON D576450/C515802实际拒绝出口，三项关闭0，owner恢复默认OFF后Shutdown；四个CreateNew Guid JSON文件在driver-qualification-01且各SHA见最终audit，performanceEvidence=false/gcEvidence不作证书。原Editor非Play/空闲/唯一Menu clean8。FOCUSED_TEST_PASS / LOCAL_GAIN_SIGNAL / DRIVER_QUALIFIED / DEFAULT_OFF；H07仍FAIL，累计71/阶段5of6/34父0/Goal active。本批未做新真实耗时、完整Driver0GC/1800/native全域/device/GPU/M0。下一最小动作是独立准确窗口合同中只差此flag的真实千人OFF/ON一次对比；不重测125局部、此资格、V1失败或旧89，不自动推广collector或候选。

此前“已启动”是事前执行快照，现已成功终结；并非观察超时失败或重启作业。运行中未修改测试或运行时脚本。

## 验证与边界

实际命令：apply_patch四准确C#；Copy-Item -LiteralPath到新absent备份/XML（无Force）；原TCP6402 manage_asset import→域重载→run_tests EditMode groupNames→get_test_job；git diff --no-index仅查看本批dirty备份差异。Validator/diff/最终guard及Menu需最终闭合，不凭历史宣称当前已过。

无新Play/FPS、120+180 window或1800正式窗；无Android/120FPS/GPU batch/FrameDebugger/M0、Burst、Grid或AI密度调度。用户关于降低密集区域AI频率/分散/攻击优先级仍仅讨论，属于改变输入与战斗行为，未实施。旧91归因已闭合，不用细化无决策价值计时替代优化。

回滚必须用户明确批准，仅本批hunk或准确dirty备份，不恢复HEAD、不丢用户改动。Task NTSD-OPTIMIZATION-BATCH92-ROLE-GROUP-SNAPSHOT-SKIP-20261009 / Change NTSD-OPT-H07-ROLE-GROUP-SNAPSHOT-SKIP-092 / Operation同名，均在仓库既有治理和原进度总表回链。此候选完成不是H07或总Goal完成；下一步仅由真实资格结果决定。

最终静态/运行后核证：999范围外保护0drift；10份事前当前dirty备份与3份V1准确版本均0drift，Menu/Battle及Packages原指纹保持；共享stress requestabsent/result未写，暂存空、diff --check退出0/仅8行末提示。外部HEAD于2026-10-09T10:55:12+08:00从a10c358eeb956f2153e7d5bfbb60c3c1b353fd50推进至a0ecb01588e77feeda7fd369bc2011fc099b1173（旧HEAD为祖先，已包含本批四C#，文件bytes指纹未改）；/root未执行commit/丢弃/恢复。普通Validator退出0/1387Record/0当前code diff/4461旧declared-not-diff warning，不把0diff当覆盖；额外对四准确C#各用-SimulateChangedPath只读校验，均exit0/1governed/92 Record覆盖/0ERROR，WARN4416/4458/4437/4446；外部提交已使旧baseline diff变化，不回退用户提交。具名全部证据与SHA见artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH92-ROLE-GROUP-SNAPSHOT-SKIP-20261009/audit-final-01.json，最终文件manifest将保存after-manifest-01.json。旧事前“HEAD同”只描述当时核证，不是当前HEAD结论。
