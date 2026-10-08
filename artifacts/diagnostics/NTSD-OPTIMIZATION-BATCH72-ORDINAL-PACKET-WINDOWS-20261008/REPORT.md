# 第72批原序packet真实千人 OFF/ON 结果

SCOPED_WINDOWS_COMPLETED / MIXED_RESULT / NOT_ADMITTED / PERFORMANCE_FAIL。41/41窄测试＋唯一四actual1000AI120warm+180sample均完成，非H07/120FPS/Android认证。候选生产默认OFF，不追加1800或复跑本四窗找PASS。Goal active、H07/H11 OPEN、阶段4/6、51已执行（22—72）、34父项关闭0。下一转已确认成对筛选主热点的下一有效动作选择，不重复packet微调或资格。

## 当前实测

| 窗口 | logic平均ms | logic P95ms | PairExactLoop平均ms | 显示帧间隔平均ms | 按平均帧间隔换算约FPS | dropped ticks |
|---|---:|---:|---:|---:|---:|---:|
| dispersed1000 OFF | 83.983 | 101.544 | 53.738 | 252.935 | 3.954 | 780 |
| dispersed1000 ON | 83.522 | 139.546 | 51.019 | 274.953 | 3.637 | 875 |
| combat1000 OFF | 86.861 | 110.708 | 56.650 | 248.939 | 4.017 | 759 |
| combat1000 ON | 81.086 | 104.539 | 50.878 | 232.514 | 4.301 | 678 |

同场配对：分散logic平均减少0.549578%，P95增加37.423613%，显示平均帧间隔增加8.705275%；混战logic平均减少6.648485%，P95减少5.572018%，显示帧间隔减少6.597989%。PairExactLoop平均减少5.060203%/10.189267%，ON仍51.019206/50.878184ms；内部热点有减但总体收益混合/边际，无法解决logicP95<33ms/drop0。单轮不能证明分散尾延迟升高完全由算法引起，不用历史不同机况或局部cost数字宣称真场收益。Frame计数150/150/150/150是实际原记录数，换算FPS不是Unity Stats截图FPS或GPU帧率；不称<1FPS已解决。记录maximumBacklogTicks=0但四drop>0，不能据此宣称无卡顿/达标。

## 正确性、应用覆盖与内存/GC边界

四Run实际最低AI/roster1000，120warm+180sample/300logic；65request字段除output外逐字段同。两组各24终态字段（两schema、两tick、20声明域hash）0diff，完整final-checksum JSON规范序列化同；输入/RNG/world/slot/rest/events等同。不是形式化全游戏/所有future输入逐位证书，71完整Driver资格复用、不重跑。

四packet观察数16/17/17/16，只fresh sampled logic tick的Last诊断，不报300应用或累计reject。ON真实ObservedApplied，最大reject48265/45059、proof59808/60518；OFF应用false/计数0，四ObservedFallbackfalse。容量firstsample66→maximum66、stabletrue；coldbeforepopulation值另保留，不把0→66冷seal预热误称热扩容。现有capacityPressure每窗180tick/0violation/0critical/0growth rejection/0throttle。primitive观察不能外推未观察tick的全部packet Last事件；无新增热反射/扩容，70/71冷容量原证据保持。

四完整同步main-thread Driver GC均 before/after 1MiB positive校准PASS/empty0、300begun/ended/accepted、180steady/120nonsteady、invalid/rejected/sequence0、coverageValid、steadyAllocationEvents0、CALIBRATED_FULL_DRIVER_ZERO_EVENTS。严格限定原fullLogic同步作用域；不覆盖H11物化—上传—录制—提交完整相机路径或GPU/native/device bytes，不抹43迟发12events。packet值array原生命周期/预算归70，byte计量UNKNOWN、ATLAS bank/纹理/预算不改。

中央source/resolved平均两场1991.202247/1992.056180同，segment约1990.280899/1990.494382、CPU DrawMesh命令约1991.280899/1991.494382，OFF/ON同。CPU命令不是GPUbatch/SetPass；未启动新Profiler/FrameDebugger/GPUcapture/M0，真实GPUbatch UNKNOWN。独立unresolved/stale精确计数未提供，不能由source==resolved平均推0；保留UNKNOWN门。

## Runtime关闭和工作区

末审计2026-10-08T08:42:11.0935809Z：538/538保护、8/8准确副本同，源9E07921E与双SceneSHA同，HEAD0e580/staged空，窗口33原件；简要终态final-audit-01.json及实际最终validator数据final-validator-01.json保存。此后只文档/新审计文件，无source变更、额外测试/测量或用户文件删除。

suite-result MEASUREMENTS_COMPLETED/DONE、completed4/runIndex4、无error；四StoppedCleanly/workloadValid/teardownRestored、ownedflag恢复。原11stage最后orderedtrue、objects/slots/logicBorrowers三0，各worldEntities/claimed/pool active/reference active0、cleanupException0。inactive pool1050是保留冷缓存不是active残留，不擅自trim/destroy。
原Editor78296/6402经唯一菜单08:21:38Z启动、08:27:01.1561962Z终态，当前非Play/idle/noncompiling，Menu savedclean8roots。Menu/Battle磁盘SHA与preplay同，无Scene保存/资源设置改动。公共stress request始终ABSENT，result由原Runner四窗覆写且逐窗准确terminal归档；最终公开文件同最后owned archiveSHA6D3A8EFC082BA5C29D8E8198C6A40E35DAC28274E1F1369C1392422DC3BDB5BE，保留不删除。

08:31:24Z 538/538保护字节同（含Q06只hash/status/正式EXE336/TMP/71原件），8副本此前preplay核同；HEAD0e580f7b/staged空，Agent无Git写。source冻结9E07921EC8E37FBCEC31918898E1D68A2CF6614A4C2451D1E56FAEB0450F4BD4，只Suite Editor before-relative562insert/3delete（31新tests＋窗口接入），旧test suffix全文同/旧17params不变；源diff --check0。windows33CreateNew产物SHA清单在after-audit-01.json，未覆盖原archive；实际完整A/B数据见windows-analysis-01.json。

## test-first与实际执行

首编译fixture枚举误名CS0103已仅改到本文件真实CollisionFormalCollectorMode；第一次run_tests旧catalog0actual不能算RED/PASS，原compiler-stale-catalog-results-01.xml SHA2FC8B00E...保留。
有效RED jobc1028260b0934bec8c68e6e9979d7cd9，31actual/31FAIL（缺窗口入口），fresh08:17:00.8568824Z，red-results-01.xml SHA DF412FF36EC2FC4C2C4BD0F7F627A46D0C762DD3A63F04DCAB1DAF671435982E。
GREEN jobdd5ec77ff2ac469f8de7a21cc54b049e，41actual/41PASS/0FAIL/0SKIP（新31＋原10），2.4372243s，fresh08:19:54.0858738Z，green-results-01.xml SHA99F9EAE215AC1257776A553D2CC1528E14E0860ABE30A91EDD293299668F27C6。XML actual identity/UnixUTC先核后ABSENT Copy，不把catalog10040当执行量。
现有桥接manage_asset精确import→run_tests/get_test_job终态→execute_menu_item唯一Batch72→正常runtime结束；无第二Editor/重启/test重跑/额外资格/成本窗。Validate-ChangeLedger.ps1 preplay exit0/PASS 1369records/current2governed/0errors/4382历史warnings。最终治理随后追加；未声称末Console0。

## 阶段裁定与下一步

最终治理2026-10-08T08:40:21.0313753Z：pwsh -NoProfile -File Tools/Validate-ChangeLedger.ps1 exit0/PASS，1369Records/current2governed/0errors/4382历史路径warnings；git diff --check exit0（仅CRLF提示）。08:40:22Z原Editor非Play/idle/noncompiling/nonupdating、不测试，Menu savedclean8roots。完整源和保护字节末轮审计随后保存，不声称Console0。

本批对照接线/恢复及限定实景正确性可以收口为VERIFIED（仅这些职责）；packet不得推广默认，H07性能阶段未完成。没有足够普遍收益，不继续几何packet微调/重复本窗/追加1800刷证书。已有主热点ON约51ms，相对33ms整个logic预算已更大，下一准确Task只用现有证据/静态调用链选择一个能减少实际pair主工作的动作，闭合缓存失效/原序/binding副作用再实施；没有证据就报告所缺合同/新授权，不发明新重构。H11完整scope与central独立门、byte预算、最终120+1800门继续保留；次数不是Goal停止边界。
