# 第58批准备：普通 Brute 保守粗判包络资格

状态：READY / IMPLEMENTATION_NOT_STARTED。只有本只读语义定位与资格合同，没有本批C#修改、实景或收益；不计入已执行36批。来源为六项阶段内H07继续及57实际PairExactLoop51.102/53.671ms/P95失败，不因名称GC通过停止Goal，不重复57。

## 要回答的问题与区别

普通Brute仍大量访问几何不相交方向。45既有全period诊断约82%–85% coarse拒绝只是频次，不当耗时因果；57再次证明PairExactLoop主成本。当前PassesReleaseCoarsePrefilterCached在ordinary union不相交后逐ExactItrRect寻找kind5，53仅移动原入口、38仅缓存kind5存在性，均不等于本包的保守全包络拒绝。拟复用当次已建WorldRect，提前形成ordinary union与各kind5 world rect的外包络，只在该包络与目标BodyUnionWorld严格不相交时拒绝；相交仍走原完整粗判/允许门/精确路径，不能把包络相交直接当候选成立。收益是假设，先资格与局部可辨成本，不无限微调或先跑实景找PASS。

## 本次重扫定位（只读，后继改动前重新核行号）

- BruteForceSceneQuery.cs 2342 CollectCollisionCandidatesBruteForce；2397附近原i/j顺序及两方向。
- 4382 BuildRoleAwareFormalExactAttackCache，已一次构建全部exact world rect；ordinary union在4441，与kind5 rect分开。
- 5887 CollectCandidatesForPairCached，粗判拒绝仍调用PreserveBruteRejectedBinding，不能省掉其清理副作用。
- 6252 ProbeBruteRejectedBinding、6280 PreserveBruteRejectedBinding、7196 CandidateCollectionPairAllowedCached；本候选不启用48绑定复用或49资格缓存。
- 6300 PassesReleaseCoarsePrefilterCached；8500 Overlap严格四端点比较；8901 WorldRect四int字段，没有float NaN问题。不做坐标再投影、归一化或改变原溢出值。
- Q06文件仅既有hash保护，不读方法体。正式336权威/已有准入证据不改变；当前可变C++源码不自动变为新formal证书。

## 初始准确代码范围（写前另Change/Operation）

只允许Assets/NTSD/Scripts/Animation/Character/BruteForceSceneQuery.cs及Assets/NTSD/Scripts/Test/Editor/RoleAwareCollisionShadowSelfCheckTests.cs的既有FormalCollector fixture（不更改旧断言）。初始不改Suite、Host、Kernel、renderer、Scene、资源或生产flags；确有资格/可辨成本后才冻结必要完整Driver/实景接入附件，不能先测或自动推广。任何C#前先唯一Change Record、精确current dirty备份/保护、test-first RED，记录实际符号和预算；不借READY跳过这些条件。

默认关闭opt-in；仅ordinary ForceBruteForce+已有exact-cache/geometry路径适用，不换collector、空间索引、pair顺序或Role-aware路径。优先复用现有participant固定容量与每次collection构建生命周期，拟增一WorldRect及presence元数据（四int16B，含标志/对齐和实际驻留尚待确认，不冻结ATLAS设备预算）。每次构建刷新，禁止跨tick/collection/World缓存；原容量不足仍走原合法路径，不热扩容或截断。没有新runtime owner/worker/服务，原11阶段关闭不改。

## 必要证明与资格矩阵

1. 包络包含原ordinary union及所有kind5 world rect的原始端点范围；严格disjoint只能推出原coarse=false，不能出现false-negative。反转端点、零尺寸、负坐标、整数边界保持原Overlap逻辑，只取minX1/minY1/maxX2/maxY2，不重做坐标加法。
2. 包络相交但各成员不相交的gap假阳性必须走原路径，保持空候选/nearest/rest/RNG等；kind5在ordinary union外仍不能漏。
3. 拒绝必须保留原资格门下ItrRest.IsBound失效清理；不得提前批量清理，也不得遗漏invalid-binding场景。空ITR、无body、suppression、destroy、陈旧handle、容量fallback和同tick重复collection保持。
4. 保持原i<j及a→b/b→a顺序、ITR/body/nearest/handle/可见结果；不减少正式规则事件或模拟频率。默认四flags true、其它既有candidate false，不靠开启已未准入组合取得收益。
5. 复用现有geometry/kind5/invalid-vrest/Driver适用权威夹具，新增本差异具名case及1000-participant两分布平衡局部cost。局部正确性与成本可完成候选资格，但不称1000AI/FPS/完整0GC通过。若只不可辨小信号，停止该候选推广而非重采/参数搜索，回到其它有据范围内动作。
6. 必要热scope可靠正负校准；新增容量/metadata准确预算、关闭与保护不退化。实际Driver/权威逐tick/两1000AI冒烟只在候选值得且事前声明准确范围后启动，本Task不提前授权新的菜单或Profiler/GPU/M0。

## 出口与回滚

SCOPED_QUALIFICATION_PASS、NO_MEASURABLE_GAIN、SEMANTIC_REJECTED或明确DEPENDENCY_PENDING，必须有actual证据，不以一份方案标完成。源码dirty保护与回滚只按准确Operation副本且仍需批准，不Git reset/restore/clean/删除。H07/H11及Goal active、阶段4/6保持；当前下一安全动作是写前准确Change/Operation和test-first资格，不等待用户再说下一批，也不擅自切Role-aware/EXT1/ATLAS/Mono。
