# 第37批普通Brute几何粗判前置
状态 RUNTIME_PENDING / SCOPED_GAIN_NOT_ADMITTED / NO_DEFAULT_PROMOTION；新批累计16（22–37），次数仅复盘不归零。164/164和两真实1000AI短窗已完成：logic79.67/80.61ms、collector48.31/50.12ms有有限收益但未达标，显示249.77/235.68ms、SetPass约2000不降；详见本批REPORT。整体准入/native/逐tick/完整0GC/性能仍待，H07/H11/Goal不关闭，不重跑36/37或把候选当生产FPS。

## 唯一候选与证据
默认false EnableBruteGeometryFirstForDiagnostics，仅36精确缓存ordinary Brute分支可显式启用；复用当前PassesReleaseCoarsePrefilterCached，不新建索引/矩形，不修改geometry或kind5 union语义。先纯几何判定，miss拒绝本方向；pass进入原cached exact规则且不重复粗判。原i<j/a→b/b→a、candidate/每body/RNG/kind4不变；Role/platform不接入。
本次重扫：BruteForceSceneQuery 5735–5805原common/vrest→coarse→ITR/body；6020–6066粗判只读已有union及kind5 rect。LF2ItrRestTracker 203–207/317–342：HasVrest会验证并ClearBinding失效绑定，不能无证跳过；miss时仅在原common base允许、attacker slot>=0且target tracker存在条件下读取IsBound以保留同一验证/清理副作用。外部依赖RuntimeRestStore 579–583/638–655及Sparse.TryGet108–122是只读，无Server写入。
不触碰规则/33ms/3ms/max2/正式336/AI/pass/publication/segment/failclosed/11阶段；native准入仍待，禁止默认推广。

## 准确范围与恢复
三源=BruteForceSceneQuery.cs、RoleAwareCollisionShadowSelfCheckTests.cs、BattleOptimizationWindowsAiSuiteEditor.cs；7原docs=Ledger/STATE/OperationIndex/handoff/唯一tracker/Goal/H07；既有Temp/NTSD_ProductionEntityStress.result只既有suite终态覆写，旧36先备份，shared request不写删。before.json精确11当前dirty副本与非写域SHA先于脚本。36/35全部非写域代码/证据、Scene/资源/Settings/Q06/Gen/Plugins/Server/HEAD保护。
Task/Record/Operation及本批diagnostics为新增输出，apply_patch文本，Copy-Item仅fresh副本。恢复另获准确授权，不reset/restore/checkout/stash/delete/push/覆盖旧证据。

## test-first与有界运行
固定新8collector：kind0/kind4/nearest三完整payload/RNG/Kind4；远距离且vrest>0；失效binding且AttackExempt0/1两控制（通过private witness而非触发IsBound）；kind5在ordinary union外不得丢失；原1000逻辑实体40active/960inert4warm+8交替局部成本/完整候选。新3request=两request与36全部65字段仅output不同、越界拒绝。先有效RED，再原Formal/Suite最窄GREEN；局部收益不成立则不运行真实窗口。
若有局部收益，允许独立37菜单一次原两个实际1000AI120warm+180sample，原36 baseline非同期；显式32/34/36/37、完整累计应用/几何拒绝、flag恢复、末tick30020hash/完整snapshot、11阶段/双Scene/原Menu恢复。37 observer在preparation后捕获既有roster/exact容量，原36准备前0原件不改。无Profiler/FrameDebugger/GPUcapture/专项M0/新长窗。运行期间不改C#/Assets、refresh、重启/新Editor；句柄timeout继续观察，不重复dispatch。
没有缓存/新容器/owner：只bool和计数器归SceneQuery，沿既有生命周期，无新关闭阶段/预算对象。热路径禁止扩容，原36容量不足整份回旧保持；default/专项门不变。测试/短窗只相应限定证据，rawGC仍UNKNOWN、H11FAIL不撤销，H07性能未达/Goal active/父关闭0。

