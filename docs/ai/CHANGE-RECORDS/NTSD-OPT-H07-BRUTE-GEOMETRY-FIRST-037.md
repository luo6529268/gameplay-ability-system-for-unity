<!-- CHANGE-RECORD
id: NTSD-OPT-H07-BRUTE-GEOMETRY-FIRST-037
status: RUNTIME_PENDING
code-path: Assets/NTSD/Scripts/Animation/Character/BruteForceSceneQuery.cs
code-path: Assets/NTSD/Scripts/Test/Editor/RoleAwareCollisionShadowSelfCheckTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleOptimizationWindowsAiSuiteEditor.cs
authority: approved six-item H07 and Goal section13 measured hotspot continuation; no rule/default promotion
evidence: artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH37-BRUTE-GEOMETRY-FIRST-20261007/REPORT.md
-->
# 第37批普通Brute几何粗判前置
test-first已写：原Formal/Suite两文件新增8collector＋3request反射用例，核心flag/请求方法尚不存在；下一11有效RED，未有收益或默认推广。11当前副本/379非写域已核，初始原Editor6400 Menu idle/非Play。

PLANNED，尚无候选实现/性能收益。需求、原状、准确符号、副作用、8+3 test-first、一次条件短窗、风险和回滚见[Task](../TASKS/NTSD-OPTIMIZATION-BATCH37-BRUTE-GEOMETRY-FIRST-20261007.md)。
只重排纯几何拒绝与只读GetVRest；HasVrest失效绑定清理必须按原base/slot条件保留，不能降低kind5或RNG门。新增bool/counters不创建cache/owner，原硬容量/11阶段/默认与专项门保持。先保存11当前dirty副本与非写域SHA；每次脚本变化立即追加实际证据，native准入未有不推广。

RED 实际终态 failed，job c2336d8c47044b9caf647be3decde931，completed 11；10项缺候选flag/API是有效test-first RED，另1项AttackExempt=true的fixture错误预设旧路径不清理stale binding，实际已清空。不能把此fixture误报为候选错误或11项全有效RED。原始完整响应保存在本批red-tests-01.json；随后按当前调用链修正fixture，保留完整序列/私有binding状态A/B一致性断言，不改变生产规则。

CODE_WRITTEN：原6400 Editor Menu idle/非Play/无测试后，三源最小修改。核心default-false flag仅ordinary Brute exact-cache传入；geometry miss仍按原base/slot条件读IsBound保存HasVrest清理副作用，pass复用旧coarse结果、无新容器/扩容。原Role调用默认参数false。新请求仅改输出路径、复用36边界拒绝，实际窗口菜单尚未接入。fixture根因重扫：SimulationWorld.CaptureCollisionFrameSnapshotsAll→PrepareAttackerRestForCandidateAll→ClearAttackExemptIfCurrentFrameCannotHit在target.AttackExempt<=0写Arest=0，tracker setter EnsureActiveBinding清理失效绑定；因此旧路径两控制都null，修改原错误预设而保留A/B比较。尚未GREEN或实际收益。

第一GREEN job 803a3479aae04c749ef156e398ccf587，11/11 PASS，2.7377856s；1000逻辑实体40active/960inert局部4warm/8交替均值cached23.1568625→geometry20.196525ms（非1000AI/FPS，GC UNKNOWN）。因此接入37独立菜单：复用原两个65字段请求，显式32/34/36/37，累计应用差值/观测max reject、四开关恢复；仅37观测在sample阶段读取post-preparation容量，原36证据不改。下一原Formal＋Suite全量最窄回归及条件短窗，仍不推广默认。

最终源编译后 focused job d8fa5b77de6d4c94a924bf81df1a5650：原Formal＋Suite共164/164 PASS，16.3053776s，0失败/跳过。局部fixture cached22.558275→geometry20.105775ms（约10.87%），非1000AI/FPS或0GC/native准入。core/测试/Suite SHA分别 FFBC1A8A4725FA04CB0150E8743EA14C56D6749D168D44382A8CE4320CC68A92 / 003B58517BFCD373A8C2156B75CB285DCF4EDAE24AE500D29291BCA98AD2F620 / 893E7F3624D244AB1CA195D087DC7BB825AFC7EF640E09A289BD8E4879F7167D；从此至两窗终态禁止C#/Assets改动/refresh。H07下一原两个同口径候选短窗未有结果；生产default仍false。

两短窗已完成（非pending）：原37菜单一次，suite DONE/MEASUREMENTS_COMPLETED，各120+180、实际1000AI/16观察、cache/geometry各全300应用0回退、四flag恢复。对36同65字段仅output非同期：logic84.245228→79.670794 / 86.222018→80.611748ms，collector52.604885→48.307897 / 54.097501→50.116486ms；显示249.77/235.68ms、SetPass1994.3146/1995未变，仍PERFORMANCE_FAIL。末tick300各20hash与完整snapshot同36，非逐tick/native；capacityCritical0，post-preparation容量1050/5250/14700，roster下界4204B非全预算。rawGC UNKNOWN/H11 strict FAIL不撤销。原Menu clean/8roots/idle恢复、11阶段三残留0、双Scene同/379保护11备份HEAD及source同，最终审计续见本批原件。RUNTIME_PENDING/SCOPED_GAIN_NOT_ADMITTED为整体准入/完整0GC/性能未过，不切默认、不重跑37刷PASS；父关闭0/累计16/Goal active。

最终validation：Tools/Validate-ChangeLedger.ps1 exit0，1333records/12governed code paths覆盖（4247既有历史warnings，非零warning）；声明tracked路径git diff --check exit0，仅LF/CRLF提示。三source至终态SHA保持，379保护/11备份0差异、HEAD同；逐11当前SHA及文件操作关闭见Operation after.json。Record仍RUNTIME_PENDING、不是候选VERIFIED或父项完成。

