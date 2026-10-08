# 第73批：PairExactLoop下一有效动作选择

当前 STATIC_CANDIDATE_SELECTED / IMPLEMENTATION_NOT_STARTED / PERFORMANCE_UNMEASURED。重扫当前XY粗筛/Z exact/binding cleanup/每collection cache，选定同普通Brute保守深度上界负拒绝；只long超过maxI+maxB拒绝，等号/survivor走原exact，保留失效binding清理/原序。计时还包含roster/cache构建，不把51ms全归内层。详[73报告](../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH73-PAIR-LOOP-NEXT-ACTION-20261008/REPORT.md)；[74准确候选Task](NTSD-OPTIMIZATION-BATCH74-BRUTE-DEPTH-REJECT-CANDIDATE-20261008.md)仅READY，本轮无C#/Unity/tests/性能新采集。累计52执行含本静态选择，阶段4/6、H07/H11 OPEN、Goal active。下文READY/51为本Task事前快照。

READY / READ_ONLY_SELECTION_PENDING / IMPLEMENTATION_NOT_STARTED。准备Task不增加51已执行数，Goal active、阶段4/6、H07/H11 OPEN，次数仅审计不是停止目标边界。本Task只用现有证据/静态链选择下一动作，非第二次perf采集或新微候选；源码前必须另建准确实施Task/Change/Operation。

## 为什么换方向

72唯一四实景已完成：packet分散logic mean只-0.549578%、P95+37.423613%，混战mean-6.648485%，ON PairExactLoop51.019206/50.878184ms、logicP95139.545640/104.539175、drop875/678，两个H07门失败。正确性/GC/恢复通过不代表普遍收益。70局部cost、71Driver、72完整原件复用，不再packet宽度微调/重复资格/cost/四窗，不追加1800刷证书。

## 一次性有据选择（当前只读）

先读authority/effective总Task0—8/current73与72REPORT，保留dirty、538保护思路。只读普通Brute实际分支：Assets/NTSD/Scripts/Animation/Character/BruteForceSceneQuery.cs 的CollectByBruteForce/CollectCandidatesForBruteExactDirection/CollectCandidatesForPairCached/PassesReleaseCoarsePrefilterCached/PreserveBruteRejectedBinding；Assets/NTSD/Scripts/Animation/Character/LF2ItrRestTracker.cs 的IsBound/HasVrest/EnsureActiveBinding/ClearBinding；沿真实caller定位必要entity/RuntimeRestStore/handle定义，外部Server/package只读不可写。既有49—65相关资格/实际窗口仅按符号需求检索报告，不读所有历史、重开对齐或活跃Q06方法体。

72重新扫得 Query2529—2571仍逐原i/j检查、packet只负几何；2735/6541—6550保留binding失效副作用；LF2ItrRestTracker317—325的EnsureActiveBinding会在失效时ClearBinding，不能把IsBound当纯读取直接删。是否主要成本由绑定/重复资格/普通coarse proof造成尚未有独立新分支比例证据，必须标推断；不要从51ms总计拍脑袋指定唯一原因或新增热timer。

输出只选一个最有证据的动作，证明减少的实际工作、原序/同tick缓存有效边界、slot/generation与binding清理、副作用/完整checksum/0GC/11stage/成本验收，明确冷容量预算。如果只是将旧微候选组合但没有有效额外覆盖或重复设计，应淘汰该动作，不把淘汰误当H07关闭；转下一有据热点。没有可证路线时报告具体缺合同/确实需要的新授权，不假定Role-aware/EXT1/Mono/ATLAS专项已批准。不得因批数或候选已结束停止Goal，不创建无证据抽象。

## 当前排除与后续边界

本Task不写任何C#/Scene/Prefab/resources/Settings/Input/Gen/Plugins/Server、不启动Unity tests/性能/Profiler/GPU/M0；不修改生产default、不切Role-aware收集器，不重跑72。下一实现须主agent冻结精确symbols/窄test-first、记录before-current备份，按影响域复用旧证据并验证真实新增行为。H07两真实120+1800/P95<33/drop0等完成门、H11完整相机迟发FAIL及未知独立central/bytes门不缩减。
