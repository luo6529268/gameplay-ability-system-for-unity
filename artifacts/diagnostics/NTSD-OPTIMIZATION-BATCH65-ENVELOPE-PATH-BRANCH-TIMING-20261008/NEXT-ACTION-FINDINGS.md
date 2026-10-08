# 第65批后继：复用已成立的普通 coarse 结果

本次重新扫描当前 BruteForceSceneQuery.cs（SHA ED518287DFFB8F870D774AD4E168833DD1D743AA99F2B968DF5CDBA7FF6A0DEC），不读 Q06 BattlePresentationShadowBuild 方法体。只记录已证重复工作与下一有界候选，没有代码/收益或推广。

## 本次场内证据

- BuildRoleAwareFormalExactAttackCache，4444—4526行：4456先清 HasExactKind5Itr，4468—4469对任何非空kind5标记；4497—4502只有kind5才先写包络；4517—4522把 ordinary union 写入包络。因此本 collection 无kind5、ordinary union存在且包络实际已构建时，BruteCoarseEnvelopeWorld直接等于OrdinaryItrUnionWorld，不是含间隙的大union。
- CollectCandidatesForBruteExactDirection，2608—2645行：有效包络入口保持攻击者/目标非空且不相同；只在存在包络、目标release body/body union、二者Overlap成立后进入survivor。无kind5且ordinary union存在的该子集，已有一次与原ordinary coarse相同矩形的成功比较。
- PassesReleaseCoarsePrefilterCached，6396—6420行：原前置后在HasOrdinaryItrUnion和同BodyUnion的Overlap成功即返回true；这一前置仍须在候选证明中全部成立，不能只凭“包络overlap”。该分支没有binding/候选/RNG副作用。
- CollectCandidatesForPairCached，5994起：已有coarseFilterPassed参数（53既有专用入口复用），geometryFirst且未passed才调用原coarse；其后的PairAllowed/ItrRest/itr/body/depth/candidate/多body顺序必须保持。复用该参数不要求新buffer、新collector或开启旧coarseDispatch。
- 65实景：计时OFF PairExactLoop平均42.118/44.256ms仍超33；ON粗判/拒绝binding/PairAllowed/exact work实际均被采样。分支clock仅stride64子集，coverage含warm＋steady，不能扩大成全量成本或把分支相减算未归因成本。

## 必须限定的假设

新候选只对“无kind5＋ordinary union存在＋包络本collection实际已构建＋原coarse前置全部有效＋包络成功”的子集，用原coarseFilterPassed表达已证明true，去掉第二次相同纯比较/字段读取。此子集占比和收益未测；不把pairAllowedVisits当复用数。kind5/mixed union/gap、无ordinary union、缺前置、无包络、fallback、Role-aware和旧诊断路径均原样。

它不同于53方向dispatch（再次执行同原谓词）及64eligibility复用；不重跑旧候选，不修改release coarse/pair allowed/hit/binding/RNG规则，拒绝路径首次清绑定时点不动。先新test-first及一次局部balanced两spacing资格；实测无清楚收益则不进Driver/实景推广，不因本候选被拒关闭Goal。

准确下一Task：docs/ai/TASKS/NTSD-OPTIMIZATION-BATCH66-COARSE-PROOF-REUSE-20261008.md，当前仅READY；代码/测试/测量未启动。Stage4/6、H07/H11 OPEN；61byte预算UNKNOWN/43迟发12camera events/EXT1与Mono/ATLAS/Role-aware推广门保持。
