# 第53批：普通 Brute 粗判拒绝 dispatch 候选

当前评估终态：FOCUSED_TEST_PASS / LOCAL_SMALL_SIGNAL / NOT_ADMITTED。26/26、两局部平衡窗仅6.28%/1.44%，不进入完整Driver/实景或生产默认推广；不重跑此候选刷PASS。原PLANNED为事前快照，实际证据见同名报告。六项阶段未完成，Goal active。

状态：PLANNED / DEFAULT_OFF / LOCAL_GAIN_PENDING。
Change：NTSD-OPT-H07-BRUTE-COARSE-DISPATCH-053。授权：用户已批准六项首阶段并更正完成边界，执行合同第0—8节；不扩大六项、切collector或推广生产默认。

## 问题与准确范围

52结果不支持eligibility＋kind5组合推广。复用45抽样覆盖及52成本：多数方向粗判拒绝，PairExactLoop占collector约98%，绝对45—63ms已超33ms预算。当前普通精确方向仍进入含分支计时/精确收集的大方法；拟将未开启branch timing时的粗判拒绝放在普通方向入口，获准方向复用一次粗判结果。收益未知，不把拒绝频率当耗时比例。
本轮早先生成IL证据否定“in auto-property造成大participant防御复制”假设，不做该无据重构。

仅两个C#：
- Assets/NTSD/Scripts/Animation/Character/BruteForceSceneQuery.cs：新增默认false EnableBruteCoarseDispatchForDiagnostics、每collection direction/reject标量；CollectCandidatesForBruteExactDirection入口；CollectCandidatesForPairCached内部仅新增coarseFilterPassed默认false参数；共用拒绝副作用helper。不得改粗判函数/几何边缘、原i/j顺序、candidate/RNG/nearest/kind4、binding首次清理或role路径。
- Assets/NTSD/Scripts/Test/Editor/RoleAwareCollisionShadowSelfCheckTests.cs：仅FormalCollector类新增具名用例/显式反射开关，不改原断言。
另七写域准确清单及60只读保护见Operation(before-manifest)。治理五路径仅留痕，唯一主进度仍原总表；不改Suite或Admission、H07方案/已过证据。

## 所有权/生命周期

无新buffer、pool、worker、module或缓存；沿现有query scratch/预热硬容量、每collection reset、原十一阶段关闭。新拒绝快路只有coarse false时计一次原ExactDirection和原GeometryReject并调用相同binding cleanup；accepted调用共享路径且coarseFilterPassed=true避免二次粗判。null/same guard一致；branch timing=true、geometry=false、exact-cache失败或role保持原路径，不吞已有counter。
不新增热路径分配/扩容，不改变336B44/D023/33ms/3ms/max2/输入/checksum/publication/排序/segment/failclosed。Q06只hash、不读方法体；Scene/Prefab/resources/importer/ProjectSettings/Input/Gen/Plugins/Server及EXT1/ATLAS/Mono专项均不动。

## 冻结验证与决策

原Editor2022.3.62f3/MCP6402 fresh idle/nonPlay/noTest，Menu clean8roots；不第二Editor。先新增接口缺失有效RED，再实现并GREEN。
新矩阵：默认OFF/原四ON；kind0/4/5及nearest完整序列/handles/RNG/kind4；拒绝binding复用OFF/ON；post-snapshot失效binding的exempt两例；geometry/cache/timing三禁用门；capacity fallback不增长；role不应用；同tick移动后重建；两千人逻辑夹具各4warm+8sample平衡OFF/ON，40攻击者，位置间隔120及12，实际应用和全序列对照。局部GC UNKNOWN，不冒称0GC或真实AI/FPS。
旧影响域只选择原geometry拒绝/sequence、kind5 union边界、branch timing coverage门；不重跑全部旧cost、52组合或Driver。
仅显著/一致局部信号才另声明必要Driver资格与真实Windows窗口；无收益或矛盾则停止采用该候选，不刷相同窗找PASS、不停止Goal。正式H07 P95<33ms/drop0/可靠0GC等和H11仍未完成。

## 回滚

准确current-dirty七文件副本、SHA及HEAD见Operation；只反向本批hunk且按用户准确恢复批准，不reset/restore/checkout/clean/stash/delete/push。保留所有失败/过程和未采用代码默认false，不以HEAD覆盖dirty。相关方法主代理自审后validator/diff-check，实际运行/未运行分层报告。

