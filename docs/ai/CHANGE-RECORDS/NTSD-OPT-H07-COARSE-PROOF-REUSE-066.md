<!-- CHANGE-RECORD
id: NTSD-OPT-H07-COARSE-PROOF-REUSE-066
status: FOCUSED_TEST_PASS
code-path: Assets/NTSD/Scripts/Animation/Character/BruteForceSceneQuery.cs
code-path: Assets/NTSD/Scripts/Test/Editor/RoleAwareCollisionShadowSelfCheckTests.cs
authority: approved six-item effective Task0-8; proven identical ordinary coarse overlap reuse only; formal336 invariants unchanged
evidence: artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH66-COARSE-PROOF-REUSE-20261008/REPORT.md
-->
# 第66批：ordinary coarse 成功证明复用

2026-10-08本轮已写新27纯语义参数case及两个Cost case（仅精确原Test文件）；Query尚未改。新接口通过反射断言，准备真实27-case RED，不执行Cost。容量fixture在准备前直接给低容量，不把不会缩小的Math.Max预热误当fallback。原Editor新鲜idle/Menu，准确Test import已请求；实际编译/运行结论待job/XML。

真实RED launch：run_tests EditMode groupNames `^NTSD\.Test\.RoleAwareCollisionFormalCollectorSelfCheckTests\.BruteCoarseProofReuse_(?!Cost_)`，initTimeout180000，返回实际job `ac1d2f3c1f7c45bcaf93bafb6714eb56` running。后续两次同job观察6402暂拒绝连接，PID19040及原StartTime仍存在；此为观察失败而非终态，不重复提交/重启，不据此认定RED或0case通过。Query保持初始SHA，等待同job/结果原件。

RED实际回调XML于03:36:35.5697243Z更新，27cases/0pass/27fail，全部是新反射接口缺失断言，非编译失败或0case；fresh red-tests-01.xml SHA6AC76E16376EAF568A43105BC4BD8BA150FF4A839977C4646A92771E58C6371F已复制核同。同job最终failed/initTimeout（completed0/resultnull）与回调实际27case不一致，原terminal JSON保留，不宣称job正常完成；回调提供实际test-first RED证据。原Editoridle/is_runningfalse，job已terminal后才改Query。

CODE_WRITTEN：只三个primitive property、入口本项Last清零、survivor局部bool及完整前置/无kind5证明，传原coarseFilterPassed；53dispatch开启时保守不复用、不计实际reuse。原builder/coarse谓词/绑定与每itr循环未改，新flag默认false。GREEN、成本、收益与Driver尚未验证。

Query准确import后等待真实reload完成，未在重载期间启动GREEN；原PID19040/Menu/idle/noCompile/noTest于observed1791430893745确认后才提交。GREEN group为`^NTSD\.Test\.RoleAwareCollisionFormalCollectorSelfCheckTests\.(?:BruteCoarseProofReuse_(?!Cost_)|BruteCoarseEnvelope_(?!1000Participants)|BruteEnvelopeBranchTiming_)`（新27+旧58/65直接影响纯检查，明确排除全部cost），initTimeout180000，实际case/job终态后登记。Tools/Validate-ChangeLedger.ps1已运行exit0/PASSED/1363Records/4 governed diff，无ERROR；现有大批旧Record声明未当前diff的WARNING保留，不据其关闭旧任务。

GREEN首轮job289df5a4c7554d0e94b1e466bfa250a7实际63case/62pass/1fail（新26通过＋旧36通过），03:42:14.9642915Z callback XML SHA415AF5900F6D6F0D9B3DDD4C80DDDF914394F7EA211E15ED4504E1F69F3C0D9E及terminal JSON原件保留。唯一新Default测试误用GetQuery夹具，该helper明确把四production flag置false，故在line738四defaults断言失败；不是Query默认改变。仅该新测试改为原58Default相同的直接SceneQuery获取，四断言保持，Query不改；只复验此一个case，不重跑62已过项，成本仍未执行。

唯一失败复验job10d7334774ca4d2ca7212587403f88ae terminal succeeded/1pass/0fail，finished1791431094538；fresh XML及job JSON核存。故63去重聚焦case（新27＋旧36）均有通过证据，不能称一次63/63新鲜整轮；首轮失败保留。Query SHA1534E0BF9429AE1D3987D823684AF32B65F17B6C3BBCE0BC3086614528A72105 / finalTest SHA2759690A0A5E77C654B40506A43E242B32CA63163AE638BE8BD994660182E791冻结。下一唯一Cost_两1000participant布局/每側4warm+8sample，尚无收益或Driver实景证据。

成本job60b2537f2fba4298ba9f2448b1c4ad51 terminal succeeded/2pass/0fail，finished1791431134365，COST XML6C0508A60DAAB806E7B21377668CEB2D5B94828CF098A61F540AB093B16B57C5已fresh复制核同。spacing120 OFF17.5847375/ON17.621775ms（+0.2106%）、reuse960；spacing12 OFF42.3000125/ON42.5899125ms（+0.6853%）、reuse7643。原8sample全部在XML/报告，不重复采样。小差异不能证明稳定变慢，唯一结论NO_GAIN_OBSERVED / NOT_ADMITTED；默认OFF，停止此候选推广/Driver/实景，不是停止Goal。

总65去重case通过证据（新27纯＋旧36纯＋2cost），代码仅两个精确C#，原defaults/Builder/coarse/绑定/逐body顺序不改。没有本批完整Driver/native/可靠GC/真实AI/FPS或61byte证书；65性能FAIL与43camera12event/专项门保持。最终Scene Menu1savedclean/8roots，原PID19040 idle/noCompile/noTests，同源指纹/234guards/10backup/HEAD逐核。validator04:00:32.6696129Z exit0/1363Records/4320WARNING/0ERROR；扩大治理diff exit2既有4文档EOF空行保留，不称全树pass，准确hunk与副本尾部核对后另记。后继67仅准备现有中央RenderPass重复纹理准备资格（不是合批/M0），本66两个C#不再改。

04:04:01.7221262Z实际复核：234guards/10backup漂移0、HEAD同、两66源SHA同；C#/唯一总表diff exit0（仅3条CRLF warning），6个本批新Markdown无尾随空白。四治理文件最后200字符与本批初始dirty副本完全相同、前后末尾换行bytes均3，证明扩大diff所报EOF是既有工作区内容，不能替用户清理。最后公共XML仍本次Cost同SHA，无隐藏重跑。后置完整audit原件另存terminal-audit-01.json。

输出审计更正：terminal-audit-01.json误包含工具1000token截断提示，JSON.parse实际失败，原件保留但不可作为完整audit证据。仅重做必要SHA/原Scene进程/HEAD只读核验，4000token足额输出先JSON.parse成功才创建fresh terminal-audit-02.json；234guards/10backup漂移0、两源与Cost XML/原PID及HEAD同。未重新测试或测量、未覆盖01失败原件；后置完整audit以02为准。

PLANNED。前轮65四actual窗口和当前路径取证为PROGRESS，当前继续有据最小优化，不据旧次数停点停止。Task docs/ai/TASKS/NTSD-OPTIMIZATION-BATCH66-COARSE-PROOF-REUSE-20261008.md完整定义范围；65代码取证与当前二次重扫一致，无kind5时包络直接等ordinary union，且同一次collection成功overlap/全部原前置有效才可复用原true。

## 准确现状与变更
- BruteForceSceneQuery：新增默认false EnableBruteCoarseProofReuseForDiagnostics、两个primitive Last/Total long计数；CollectCollisionCandidates入口仅重置本项Last（不修改Q06候选引用reset循环）；CollectCandidatesForBruteExactDirection包络survivor完整证明后传现成coarseFilterPassed，继续原PairAllowed/ItrRest/每itr/body/depth/record顺序。原BuildRoleAwareFormalExactAttackCache/PassesReleaseCoarsePrefilterCached/所有规则谓词不改；kind5/mixed/gap/缺前置/未包络/容量fallback/其它collector走原链。只替换重复纯比较，不开启53dispatch/64eligibility或改四defaults。
- RoleAwareCollisionShadowSelfCheckTests同文件实际FormalCollectorSelfCheckTests：计划新BruteCoarseProofReuse_纯语义27参数case及Cost_两个spacing120/12固定1000participant fixture。默认/多body kind0/4/5与nearest、kind5-only/mixed/gap、五inactive门、两fallback、四positive/rejected stale-binding门、sameTick重建、缺itr/body、role-aware隔离、三ordinary边界、Last/Total。通过前实际discovery/XML核数量，反射接口RED不以编译失败/0case充数。
- 新cost仅一次两布局，每侧4warm+8sample balanced/order交替，双方exact/geometry/envelope/rejectedBinding ON，eligibility/kind5/dispatch/branchTiming OFF，仅proof OFF/ON；先全candidate/snapshot/handle/RNG与ON实际reuse计数验证。没有实际AI/Driver/GC/FPS/生产准入；没有收益则不推广不重复找PASS。

## 容量、关闭与红线
只Query两个long和一个bool/stack证明，无新数组/容器/participant字段/heap/worker/queue/owner；原Query/World11stage生命周期不变，无热扩容或新释放阶段。新增primitive不进入checksum，不能据字段数量冒称61全预算测量闭合。
正式336、33ms/3ms/max2、RNG/输入/pass/slot/body/order/首次binding保留；publication只读/透明顺序/segment/failclosed/lease/fence/11stage/Join保持。Q06 BattlePresentationShadowBuild只status/hash；不读活跃body，不改Scene/Prefab/资源/settings/Input/Gen/Plugins/Server或非战斗/第三方。EXT1/Mono/ATLAS/Role-aware推广仍USER_HOLD。

## 事前保护与恢复
Operation NTSD-OPTIMIZATION-BATCH66-COARSE-PROOF-REUSE-20261008/before-manifest-01.json：10准确当前dirty写入/公共Temp输出、234逐fileguards/HEAD527350afa08633357ba20e7453a9d260eaa2b97c、rootStatus36条；234guards中含65全部原窗口/源与旧63/64失败、并行TMP/Scene/settings/authority/Q06 hash。先写本Record/Operation/new manifest，再复制10初始现状（目标不存在、source/copy SHA等初始manifest），在任何旧治理头/Task/C#修改之前完成，不复用65旧副本。
回滚只另授权来自本66准确副本的精确本项hunk，不reset/checkout/clean/stash/delete/move或覆盖用户工作。Task初始READY保留为历史；65性能FAIL/43camera12event/61budget UNKNOWN不改。

## 验收/证据/未验证
原Editor19040/6402 fresh idle/nonPlay/nonCompile/noTest，Menu实际clean门再核；准确import/test job，先纯27 RED，再最窄GREEN＋直接受影响旧58/65（不跑旧cost），然后仅新两Cost case。记录job/UTC/准确XML原件/SHA/失败，观察同job不重复重启；任何timeout不是terminal。
必要scoped diff/234guards/10backup/Scene/HEAD/Tools Validate-ChangeLedger验证，实际结果后追加。尚无本批C#、测试、成本/收益、完整Driver0GC或千人实景；局部候选成功不能关闭H07/H11。
