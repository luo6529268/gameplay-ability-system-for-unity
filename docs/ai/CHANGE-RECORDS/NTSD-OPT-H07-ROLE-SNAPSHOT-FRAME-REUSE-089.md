<!-- CHANGE-RECORD
id: NTSD-OPT-H07-ROLE-SNAPSHOT-FRAME-REUSE-089
status: FOCUSED_TEST_PASS
code-path: Assets/NTSD/Scripts/Animation/Character/BruteForceSceneQuery.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleHitCandidatePairSnapshotFactory.cs
code-path: Assets/NTSD/Scripts/Test/Editor/RoleAwareCollisionShadowSelfCheckTests.cs
authority: effective six-item optimization contract sections0-8; current336 release candidate payload and unchanged RNG/order; default-off four validated authored frame reads reuse only
evidence: docs/ai/TASKS/NTSD-OPTIMIZATION-BATCH89-ROLE-SNAPSHOT-FRAME-REUSE-20261009.md
-->
# 第89批 Role snapshot frame reuse
PLANNED：尚无脚本实现/编译/测试/收益；默认OFF，不推广production collector。
原状与改后职责、准确三源符号、生命周期、不变量、影响域矩阵、未验证域及恢复见[Task](../TASKS/NTSD-OPTIMIZATION-BATCH89-ROLE-SNAPSHOT-FRAME-REUSE-20261009.md)。[Operation](../FILE-OPERATIONS/NTSD-OPTIMIZATION-BATCH89-ROLE-SNAPSHOT-FRAME-REUSE-20261009/RECORD.md)保存当前dirty逐文件清单及准确before副本。目标只省已校验四帧查询，不能跳过eligibility/reject/nearest/capacity/select/RNG/store或缓存实时previous/holder/group。
H07仍FAIL，已有5项本阶段收口不重开；报告交付不能标Goal complete。每次实际操作后追加证据，不把PLANNED说成收益。

test-first代码已写：FormalCollector原类新增SnapshotFrameReuse_用例/helper，既有case不改。runtime/factory尚未改；先原Editor导入/有效RED，不称编译或收益。一个Change追加patch误用局部句作完整行anchor失败，原record未被部分改写；已用真实完整行补登记。68已执行/阶段5of6/H07未达/Goal active。

有效RED：原Editor job f1ff2f67ef124b388018274ba829e845实际20/0PASS/20FAIL、0skip，UTC2026-10-09 00:16:52—00:16:54，XML CC2B393256E66260973DD1094B5EAC9B731CCB998C3675EE54ED31F9830E0F1E；19项缺默认开关/计数、1项缺factory新入口，符合预期。catalog10377不是执行数。red-01/tests.xml及before-02/test-first.cs均复制新absent目标/核SHA；import后的第一次state TCP拒绝处于domain reload，随后唯一run_tests提交成功并同job终态，不重启或重复提交。新增fixture两静态字段拼写在首次import前已核当前模型修正为frameId/characterData，不是生产改动。下一只实现准确runtime/factory；Task所称TryCollectBodyHitCandidatesCached更正为本次重扫真实RecordOverlappingBodyCandidatesCached，旧名称不作为证据。after-pair测试hook存在时明确不走新入口，保留可能修改帧的测试边界。

runtime/factory实际CODE_WRITTEN：Query仅新增默认false开关、collection重置的long实际入口次数，RecordOverlappingBodyCandidatesCached仅Role且无after-pair测试hook调用CaptureWithResolvedFrames；原legacyCapture优先、Brute/fallback/84guard/31pair reuse均保持。Factory原四lookup与guard保持，其后的原previous/live字段payload搬到唯一CapturePayload；新入口保留Runtime/Frame/FrameCache/tick-null拒绝，不缓存previous/group/holder/oid/type/facing，无新buffer/owner或关闭阶段。尚未GREEN/成本，不称优化达标。

限定结论：FOCUSED_TEST_PASS / MIXED_LOCAL_COST / NO_CONSISTENT_GAIN / NOT_ADMITTED / DEFAULT_OFF。原Editor确认GREEN job95abe2f98fcd431eb610d718fb1be407实际33/33、0skip、16.038938s，UTC2026-10-09 00:19:38—00:19:54；20新增＋13既有必要回归（31 pair reuse7、pre-loop失效/异常回退2、payload/store carrier4），原件green-01/tests.xml SHA F5E865E35693B028D2FFEBE83ED8F283A4EDC59A5957C17D4E039D9A3DCED16B。Warmed factory4096直接delegate捕获0B，仅该函数局部，不称完整logic/render 0GC。
两1000实体managed fixture各8warm+16交错OFF/ON：分散14.935725→14.51569375ms（-2.8123%），集中194.4171625→205.60508125ms（+5.7546%）；实际复用次数15360/614400、OFF0、完整candidate/payload/RNG先比较同。没有一致成本收益，不进入Driver/真实1000AI/Play窗口，不重复成本寻找PASS或生产推广；保留默认OFF代码及证据，不删除/覆盖已有工作。不是实际1000AI、FPS或全角色/native规则证书。68已执行/阶段5of6/34父0，H07仍FAIL/Goal active；下一既有NativeSampledState约2.25ms输入热点。
工具异常：runtime导入后state仍compiling时一次run_tests请求header前断连，未取得job；随后原PID74724 domain reload后明确idle/无当前job且公共XML仍RED相同，才提交上述已确认GREEN；不把断连尝试当验证或判断未知启动行为，不重启Editor。窄global Editor.log仍有旧其它测试CS报错，不作本包新CS0结论/无关修复；当前源成功编译并执行33项为真实证据。手工main review：三Query hunk准确；移除本包测试插入块后旧测试normalized全文同；Factory从previous lookup起原payload余文normalized全文同。Scope内pointer复用未碰任何规则、RNG/nearest/select、slot/store、资源或Q06活跃方法体。最终保护域/validator稍后追加。

最终审计（2026-10-09 UTC00:32）：Validate-ChangeLedger.ps1 实际由子pwsh捕获，exit0 / PASSED，Records1384、governed code diff16、4177条WARNING字符串、0条ERROR；WARNING不视为本包新增或已解决，git diff --check exit0，只有CRLF提示。首次在同进程2>&1未捕获Write-Host导致输出截断及JSON解析失败，不能使用该次“warningStrings=0”作为结果；上述为后继完整捕获的真实计数。
保护manifest修正：原before-manifest-01继承后先排除写域、再additionalGuards重新加入6个本包合法写域，事前910条0漂移，事后仅该6个合法写域SHA变化。未修改原manifest；新增before-manifest-02在最后排除准确9写域，实际904范围外保护项0漂移，9个before-01副本SHA均同；原6项变化保留在audit-final-01，不以排除新范围隐匿越界。HEAD a10c358eeb956f2153e7d5bfbb60c3c1b353fd50不变、staged空、SharedStress request不存在且88result保持原SHA。
三C# SHA：Query CD68A227AB23F9BEFE91ADCA3B7F749EE35DD790813DD9EED8625DFCFC10C216；Factory1888ADFC288F3FBDC61367CEFA095BAB9C39703BD2384595849BF70F0BAA380D；Test B769A7B3D31B84235089662CECAC3A483043050AB4B37B7728DA04EC1C8B858A。Menu/Battle Scene SHA保持before一致；原Editor最后observed_at_unix_ms=1791505662956，Menu唯一clean8、idle且非Play/Test/Compile。本包没有Driver/Play/实际1000AI、Profiler或全链0GC证书。
仅此准确批次的代码/聚焦测试/一次局部成本/留痕已收口：FOCUSED_TEST_PASS / NO_CONSISTENT_GAIN / NOT_ADMITTED / DEFAULT_OFF；不是H07优化完成。后验逐文件SHA见after-manifest-01.json，保护与工具异常见audit-final-01.json。H07仍FAIL/Goal active，下一现有NativeSampledState输入热点；无新增生产默认、专项或无证据架构授权。
