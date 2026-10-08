# 第38批普通Brute kind5存在性缓存
PLANNED / NO_DEFAULT_PROMOTION；新批累计17（22–38）。37实际collector48.31/50.12ms仍61–62%，性能FAIL。源重扫发现PassesReleaseCoarsePrefilterCached普通union miss后总扫描ExactItrRectCount寻找kind5（6044–6090）；BuildRoleAwareFormalExactAttackCache已每次建立同一exact ITR列表（4242–4305），可同期派生是否存在kind5，不更改几何/规则。

## 唯一候选
新增default-false EnableBruteKind5PresenceForDiagnostics，仅32/34/36/37全部显式启用的ordinary Brute exact-cache可使用；Role/platform/default保持。已存在participant缓存中增加派生bool HasExactKind5Itr，每次build先false、非null ITR kind==5时true。仅普通union miss且bool=false时省略无可能命中的kind5扫描；true完整原扫描和顺序不变。计数仅whole collection应用/每call省略扫描次数，不认作pair总量或FPS。
原i<j/a→b/b→a、RNG/kind4/payload/body、kind5独立rect、rest清理、容量不足整份回旧、seal无扩容/33ms/3ms/max2/publication/segment/failclosed/11阶段不变。新bool只是已有participant数组metadata，无独立buffer/cache owner；真实CLR布局字节未测，纳入既有冷容量驻留/替换峰值，不能以1050逻辑bool推造精确bytes或认定设备预算达标。原capacity预热不变，无热增长；native/逐tick准入缺口仍禁止默认推广，不重开全项目对齐。

## 准确写域与备份
三源：BruteForceSceneQuery.cs、RoleAwareCollisionShadowSelfCheckTests.cs、BattleOptimizationWindowsAiSuiteEditor.cs；7docs：Ledger/STATE/OperationIndex/handoff/唯一tracker/Goal/H07；Temp/NTSD_ProductionEntityStress.result仅既有suite终态可更新，旧37先存副本。Task/Record/Operation/本批diagnostics为新增输出；before.json先核11当前dirty字节/sha/Git/所有原37非写域，保护Q06/Scene资源/Settings/Gen/Plugins/Server/HEAD，before副本中的Index含本批已登记头、此前正文保留。
全部文本apply_patch，Copy-Item只fresh副本。恢复另获逐文件授权，不reset/checkout/clean/stash/delete/push/覆盖无关改动。只原PID19040当前6400 Editor；不启动第二Editor、Profiler、FrameDebugger、capture、EXT1M0或改Scene/资源。

## 事前冻结test-first
新9 collector case：MultiBody kind0/kind4/nearest三完整候选/RNG/Kind4/defaultfalse；无kind5远距离省略扫描1；kind5-only/mixed且普通union外仍保留两控制；同一ITR kind0→5→0跨collection重建1；冷exact rect容量不足全回旧且不增长1；1000逻辑实体40active/960inert、每attack8普通ITR、4warm/8交替局部完整序列和成本1。新3 request=两请求与37全部65字段仅output不同、越界拒绝1。先12有效RED，再原Formal/Suite全量最窄GREEN，噪声/无收益不得推进实景或刷PASS。
局部有收益才接入独立38菜单一次原两个实际1000AI120warm+180sample，原37同字段非同期baseline；显式五flags、全collection应用/观测max省略/恢复、post-preparation容量、末tick300各20hash/完整snapshot、11阶段/双Scene/原Menu恢复。运行中source/Assets冻结，不refresh/restart，不重复dispatch；超时继续同句柄/状态，原窗口7200s安全上限不扩。
12与窗口只对应层级证据，rawGC UNKNOWN/H11 strictFAIL/H07性能未过保持。Goal active、父关闭0；次数只§13复盘，不解冻EXT1/Mono/ATLAS、其余28项或正式默认。
