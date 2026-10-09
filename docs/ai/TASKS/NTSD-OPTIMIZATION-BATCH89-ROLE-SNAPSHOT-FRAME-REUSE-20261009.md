# 第89批：已校验帧引用复用（H07）
> 当前限定评估已结束：FOCUSED_TEST_PASS / NO_CONSISTENT_GAIN / NOT_ADMITTED / DEFAULT_OFF。20 RED→33 GREEN，局部分散-2.8123%/集中+5.7546%，不进入Driver/实际千人或重复cost；H07仍FAIL/Goal active，68执行/阶段5of6/34父0。[报告](../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH89-ROLE-SNAPSHOT-FRAME-REUSE-20261009/REPORT.md)。以下PLANNED为事前快照，不定义当前结果；真实调用方法是RecordOverlappingBodyCandidatesCached（事前误称已在Change更正）。

状态：PLANNED / DEFAULT_OFF / PERFORMANCE_PENDING；H07仍未通过，首阶段5/6，不重开其它五项。
来源：有效首阶段合同第0—8节和用户继续优化要求；第83批Record为最高采样分支，第88批CandidateCollect约14.18/14.80ms、exact约6.94/7.27ms。旧次数停点不是完成条件；本批没有生产默认推广权。
## 已观察事实与假设
当前Role精确缓存已保存并校验双方CurrentFrame和CollisionFrame（BruteForceSceneQuery.RoleAwareFormalExactCacheIsCurrent）；随后TryCollectBodyHitCandidatesCached又由BattleHitCandidatePairSnapshotFactory.Capture查询相同四帧。CandidateAccepts仅只读返回，记录链写候选/count/nearest/kind4/RNG，不写帧缓存。LF2Entity.GetCollisionFrameData返回Frame.Prev2的native authored data，不是显示Frame.D。本批只省这四个重复读取，收益仍待测；不缓存previous/group/facing/holder/oid/type，不改变读取时点或pair payload。
## 准确脚本范围（只有三个）
- Assets/NTSD/Scripts/Animation/Character/BruteForceSceneQuery.cs：默认false EnableRoleSnapshotFrameReuseForDiagnostics及每collection实际复用计数；仅roleDirection且既有exact缓存有效时调用已解析帧的factory入口，旧Brute/fallback及test-only legacyCapture保持。无新容器、slot、线程或owner。
- Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleHitCandidatePairSnapshotFactory.cs：新增CaptureWithResolvedFrames；原Capture查询、顺序和guard保持，经一个共有私有payload builder构造；新入口同样拒绝无Runtime/Frame/FrameCache或null tick frames，previous与全部其它字段仍在当前capture时刻读取。
- Assets/NTSD/Scripts/Test/Editor/RoleAwareCollisionShadowSelfCheckTests.cs：仅FormalCollector类新增SnapshotFrameReuse_具名case/helper；不改旧case/assertions。以现有完整candidate sequence/pair payload/count/RNG/source-count/store健康门比对OFF/ON。
## 必要测试与成本门
先test-first：默认OFF、multi-itr/body及nearest/kind4/不同kind、四producer模式、后继collection实时group/facing/previous变化、无重叠、Brute/fallback、legacyCapture优先、不同current/tick frame、热factory0GC、两1000实体局部交错OFF/ON成本。实际反例先修本包，不降低断言；现有PairSnapshotReuse与缓存失效/fault必要用例按准确方法筛选。
只运行原空闲Editor/MCP6402的聚焦EditMode；公共Temp/Goal18_LastTestResults.xml覆盖前已有before副本，每轮自然结果另复制新absent phase目录并核SHA。局部managed collector fixture不是真实1000AI、FPS或完整0GC；无法据它声明H07达标。没有本批Play/千人窗口/Profiler/GPU/M0。若局部无收益则NOT_ADMITTED；有收益下一准确Driver资格/真实窗口另包，不把报告作为完成。
## 不变量、生命周期与恢复
正式336B44/playable闭包、33/3ms/max2、原seed/input/pass/RNG/candidate处理顺序/checksum/publication只读、排序/segment/fail-closed、11阶段关闭、seal禁扩容全部保持。现有缓存生命周期不变；新数值只collection重置，无关闭资源/新模块。Q06活跃排序方法体不读；不解冻EXT1/ATLAS/Mono，不新增Grid/Burst或改Scene/资源/Settings/Server/Gen/Plugins。
三代码+五治理文档+公共XML为准确现有写域；新增Task/Record/Operation/manifest/report及新phase输出为absent范围。保护继承88before并补88全部实际after/newFiles和三源meta。每次脚本前核当前dirty before SHA，回滚只参考before-01准确原件与本包hunk，经授权恢复；不使用HEAD覆盖或破坏性Git。结束运行Validate-ChangeLedger.ps1、git diff --check和保护域核对，更新唯一优化总表。
