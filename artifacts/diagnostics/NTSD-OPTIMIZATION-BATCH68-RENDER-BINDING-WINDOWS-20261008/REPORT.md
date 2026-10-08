# 第68批最终限定结果：实际复用覆盖0，不采用

最新：20/20 GREEN＋四actual1000AI120+180完成；NO_REUSE_COVERAGE / NO_CAUSAL_GAIN / NOT_ADMITTED / PERFORMANCE_FAIL。两ON reuse0、prepare与OFF同，ON logic P95123.040/120.434ms、drop787/753、显示约3.928/4.004FPS；不称优化达标。24末字段/完整snapshot同、四可靠logic steady0event、关闭0残留；402guards/9副本/两源/HEAD同。defaultOFF，下一PairExactLoop主热点；H07/H11 OPEN、Goal active。四窗详细证据见下方“实际窗口”。

## test-first阶段原始留痕（历史）

历史：RED_CONFIRMED / WINDOW_IMPLEMENTATION_PENDING。Change NTSD-OPT-H07-RENDER-BINDING-WINDOWS-068 / CODE_WRITTEN（tests-only）。此时非H07成功、FOCUSED_TEST_PASS或新FPS/0GC报告，已由上方最终限定结果取代。

原Suite同文件新增20case：开关/恢复/重入/漂移/无owner保留外部值8，fresh同camera Execute8，四request4。新Execute允许同publication；相机identity负值有效、零无效，不要求logic推进/GPU完成。

首次import：本批5个NonParallelizable属性触发当前NUnit接口CS0234，discovery0，这不是RED；错误原件保留，仅删除5个本批属性，未改框架/旧tests/断言。新domain reload后actualdiscovery20，唯一job7349568f6ac540dea06456ecf06a117e terminal failed、20/0pass/20fail/0skip、1.345236s。缺Apply6/predicate8/builder4/restore2；catalog9963不代表执行数。XML源/副本SHA A3E7E08A405D21A39586771E89026DC3D61D880ACEDCB65B8650CBE4855D5AD5，已先归档避免后续callback覆盖。

SuiteSHA BFE0CA745229563374A92F9046A7BC67FD6890FB23676A9B27061A81C8BAD9D2，去新增块后旧实现/旧tests文本完全同dirty副本；Feature仍67SHA F5F1173E63B18A2F90F9073B87AA933FBDC83731822487DEE38DE336587FB6C2、默认OFF。未写窗口/Feature/Query/Driver/Harness/Scene/资源/Settings/Q06 body；七副本核同，362guards按保留SDF与原Temp缺失重新基线，67 UNKNOWN_CAUSE不归因/不恢复。

准备期间本项目Editor重新出现，新PID98492/6402/Unity2022.3.62f3，非本任务启动；测试后live Menu1savedclean8roots、idle/nonPlay/nontests。不清Console，不把初始无errorCS观察冒充最终历史错误消失。

首validator退出0、4314历史warnings/0errors。相对HEAD四治理EOF错误真实保留，但dirty副本尾缀同，证实不是本批新增，不能为了绿清理旧EOF。七before-relative git --no-index --check仅差异exit1及CRLF转换warning，无本批whitespace error；末核另见terminal-audit-01.json。

下一同批实现窗口正常/失败/退出owner接线、fresh-camera primitive→GREEN→一次四actual1000AI120+180OFF/ON；不得只补测试API而漏实际Suite路径。保留可靠logicGC/末态/关闭门，覆盖低或无收益则默认OFF不采用，不重采旧cost/1800刷PASS。H07正式P95<33/drop0及H11完整0GC仍未达。

47已执行（22—68）/阶段4/6/Goalactive；专项不升格、不Instancing/M0/Profiler/GPUcapture，无第二Editor/文件删除/恢复/Git丢弃/push。

末核UTC2026-10-08T05:12:20.2940774Z：7backup保持、361保护同＋1授权callback创建且与原件SHA一致、0范围外变化、HEAD/两源同。validator退出0/1365Records/6governed差异/4314历史warnings/0error，本批C# diff --check退出0，七dirty副本相对检查无新增whitespace error；不把既存四EOF失败称全树PASS。Editor98492仍live；本批现有代码只改Suite新tests，窗口实现继续待。
## 四窗终态实证：真实复用覆盖为零，不采用

结论：SCOPED_WINDOWS_COMPLETED / NO_REUSE_COVERAGE / NO_CAUSAL_GAIN / NOT_ADMITTED / PERFORMANCE_FAIL。实际20/20 GREEN及四千人短窗完成；H07/H11未达，目标active。以下是当前结果，原WINDOWS_READY与tests-only快照保留在后文。

## 实际窗口

Unity 2022.3.62f3，Windows Editor98492，同一源版本、原saved Battle与生产Brute四默认、actual1000AI，120 warmup＋180 sampled、seed1314149188、33ms正常/max2。四65字段requests组内只有outputPath不同；当前candidate开关由冷owner应用，Applied/Unchanged/Restored均true。未运行Profiler/FrameDebugger/GPU capture，不是Android/120FPS/正式1800证书。

| 场景／开关 | body准备总数 | body复用总数 | logic mean ms | logic P95 ms | dropped ticks | 显示帧 mean ms | FPS估算 |
|---|---:|---:|---:|---:|---:|---:|---:|
| 分散／OFF | 177135 | 0 | 83.601 | 133.901 | 772 | 251.932 | 3.969 |
| 分散／ON | 177135 | 0 | 83.313 | 123.040 | 787 | 254.602 | 3.928 |
| 混战／OFF | 177154 | 0 | 86.180 | 119.003 | 763 | 250.591 | 3.991 |
| 混战／ON | 177154 | 0 | 86.394 | 120.434 | 753 | 249.746 | 4.004 |

每窗89有效同camera新Execute样本；camera instance44876（具体identity见原observation）。body准备/复用是CPU命令录制计数，不是presenter submission或GPU batch。分散两窗segment mean1990.281／CPU draw mean1991.281，混战两窗1990.494／1991.494，ON/OFF相同；多1 draw包含辅助显示，不能混称body/GPU batch。

ON两个窗口的reuse都是0、prepare与OFF完全相同，helper真实开启未漂移。依照当前AppendSegmentDrawCommands，只有相邻实际Texture引用及BindingMode相同才复用；因此本workload没有这项覆盖。具体资源/排序交替原因未实测，标UNKNOWN，不读Q06活跃方法体猜测。显示mean变化分散+1.060%、混战-0.337%；logicmean变化-0.343%/+0.248%，没有本候选复用工作变化可解释，不能作为收益。FPS只是1000/mean Unity frame间隔，非用户旧截图同机同环境A/B。仍P95>33ms及大量drop，不能追加1800刷PASS。

## 必要正确性与关闭

四窗workloadValid/harnessValidity/teardownRestored均true、actualAI最小1000。前后已校准完整同步StepOneTick观察覆盖有效，四原steady policy0allocation events；不缩scope、不把此logic证据晋升H11完整物化—上传—录制—提交。两组24末Parity/Lockstep字段0差异、两完整checksum JSON同SHA；只是末tick，不冒充逐tick/正式EXE全World对照。三已报告sample rejection（roster缺失/变化/pool扩容）均0；中央independent unresolved/stale字段仍UNKNOWN，不能由source/resolved相等代替。

正常11阶段关闭objects/slots/borrowers0、Battle磁盘身份和clean保持、原Menu恢复。最终Editor98492 idle/nonPlay/单saved Menu8roots、compilerConsole error CS0。402保护/9phase04备份/两C#冻结/HEAD保持，源码diffcheck0；旧四EOF与历史warnings不清理。公共终态同SHA另存shared-terminal-after01，首compile失败、20RED、20GREEN以及67未知事件全部保留。

## 决定及下一步

停止采用本候选，不是停止目标。defaultOFF，不切生产默认、不删候选脚本、不重复67cost/本四窗。当前CandidateCollect/PairExactLoop ON mean53.094/55.776ms，是明确的主要逻辑热点；下一审查同普通Brute逐对调用和必要绑定副作用，声明具名有据的减负动作后才编辑。保持33ms/pass/checksum/RNG、公用publication只读、segment/failclosed、11阶段/Join；不解冻Role-aware生产、EXT1/ATLAS/Mono，不重开历史全项目对齐。

证据：green-terminal-01.json、green-results-01.xml、windows-analysis-01.json、windows-terminal-audit-01.json、windows-01/suite-result.json与四report/observation/logic-gc/final-checksum/terminal原件。正式H07、H11完整scope、61budget与central独立门保持OPEN。

## 前阶段原始留痕（历史）

实际20有效RED→20/20 GREEN，job b834b93a3c444344b1bfdcd73fccab86 /1.3478885s，fresh XML同SHA7BDC64D54862E948A93316F3F4DC0EEFFEBFD36F2EF47F87388B84CAED994AD0。两源真实接线和有效camera观察已写；380guards/12副本同，9阶段04当前dirty备份先核。尚无四短窗或新FPS收益，候选defaultOFF。下方为前阶段保留历史。
