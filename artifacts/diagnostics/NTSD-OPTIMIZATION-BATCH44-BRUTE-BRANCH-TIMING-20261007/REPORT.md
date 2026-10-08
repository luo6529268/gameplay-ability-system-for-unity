# 第44批普通 Brute 分支成本诊断

## 最终诊断出口：SCOPED_DIAGNOSTIC_VERIFIED / COST_ATTRIBUTION_PERTURBED

最终治理已完成：Ledger实际exit0/1340Record/14governed diff/4257历史warning/0error；diff--check exit0、仅CRLF提示。原错误捕获方式不据其0warning作证，准确原件与更正见validation-final-01.json。

四窗口已实际结束，suite-result.json为MEASUREMENTS_COMPLETED / DONE、completedRuns4、error空；不是仍准备中。每窗120warm+180sample、实际最低1000AI、workloadValid/harnessValidity/teardown均通过，cache/geometry300应用、0fallback，诊断flag已恢复、四生产默认不变。原Editor19040最新MCP为Menu单Scene8roots clean / idle / 非Play、无测试或编译；没有第二Editor。原始窗口及失败记录全部保留。

| Windows Editor短窗口 | logic平均/P95 ms | collector平均 ms | display平均 ms（倒数估算FPS） | droppedBacklogTicks |
|---|---|---|---|---|
| Dispersed1000，新增计时OFF | 80.727 / 102.777 | 49.414 | 260.192（3.843） | 810 |
| Dispersed1000，新增计时ON | 133.857 / 234.156 | 84.708 | 360.161 | 1257 |
| Combat1000，新增计时OFF | 84.127 / 110.107 | 52.626 | 245.465（4.074） | 742 |
| Combat1000，新增计时ON | 94.375 / 118.043 | 63.660 | 270.174 | 853 |

current/maximum backlog均0，但不能据此宣称无drop。重扫ProductionEntityStressHarness.cs 5816—5825：droppedBacklogTicks统计本次runner accumulator超出最大debt时被舍弃的33ms interval数量；它不是跳过已执行的规则tick，也不是post-warmup专属delta。累计非零与logic P95超33ms均不满足H07阶段门；保持既有max2，不改cadence或丢债策略掩盖性能问题。

本批新增计时OFF→ON，collector平均差Disp+35.295ms（+71.426%），Combat+11.034ms（+20.968%）。这是仪器开启与顺序运行环境变化共同影响下的实测差，不可严格全归因于计时调用，也不可把ON子phase当纯生产成本比例。子phase均为采样父区间的一部分；父减子还包括clock/累加、循环/roster等未分类成本，不将其解释成已定位函数。没有新FPS优化收益。

两同工作负载OFF/ON末tick300完整checksum JSON逐字节相同；20个finalParity/Lockstep hash各全部相同。只证明本次末快照，不提升为逐tick/native/全角色/Android证书。capacity critical/reject0，各teardown World对象/槽/active pool/borrower0、cleanup exception0，ordered shutdown与原Scene/SHA恢复。inactive pool保留1050是既有cache，不是active泄漏，本批未trim。

原202/202 focused PASS仍有效；final terminal-source-audit-01.json重新核五源码/22guards/11backup/HEAD全相同（Q06只hash）。四request除output与40相同的65字段对照有效；原Properties.Count数组schema瑕疵由windows-request-schema-correction.json明确纠正，不篡改原件。

logic原0B counter未校准，GC.Alloc精确handle不可用，UNKNOWN仍保留；全帧GC recorder平均约237—239KB不能直接当逻辑热路径分配。原H11完整FAIL不被本批raw gate=true覆盖。不运行Profiler/capture/GPU/M0/Android，不改Scene/资源/Settings、渲染语义、collector或四默认。

下一最小必要动作：为既有四phase提供有界低频计时及覆盖计数，先证明候选顺序/RNG/绑定副作用/0GC和normal-abort恢复，再用同代码OFF/低频ON对照验证是否降低仪器干扰。未实现/未测的低频数据不能外推全量成本。不重复当前全pair计时窗口或仅凭其raw比例选择第38批候选；H07/H11 OPEN、六项阶段4/6、有限产物5/6、34父项关闭0，Goal仍active。

下方WINDOWS_READY / 尚未测量为事前历史快照；当前运行事实以上述终态及windows-audit-01.json、off-on-comparison.json为准。最终Ledger/diff检查另记录实际结果，不预称通过。


## 当前检查点：FOCUSED_TEST_PASS / WINDOWS_READY

原Editor13有效RED→13/13 GREEN；首次受影响202执行只有旧报告schema44→48失败，原件保留。事前补第五精确测试路径/backup11，再只改schema数量；最终job750141db25ab4380bf35353a344174f9实际202/202 PASS、0skip、19.2191183s。原件test-regression2-01.json。

生产四Brute默认/逻辑/候选顺序不变；新增四phase默认不开。在普通geometryFirst+新flag+既有recorderEnabled三个条件都满足时GetTimestamp/RecordPhaseElapsed计时，无每pair Profiler Begin/End。关闭与开启候选序/RNG同，reject只coarse/binding、geometryFirst关计时0、恢复旧值false/true并释放引用均通过。

五源码SHA/22guards/11备份/HEAD45bbed41已冻结；phase两long数组新增64B/recorder(4*8*2，payload下界非全部诊断内存)，四static ProfilerMarker/string与报告采样存储另有冷增量尚未全量记账。无新Renderer/GPU/World owner。计时ON的clock/累加开销必须由OFF/ON实际比较，不宣称纯函数百分比或FPS收益。

git diff--check exit0；首次Ledger validator exit0/errors0、4257历史WARNING行，最终仍需按当前五路径复核。Unity2022.3.62f3/URP原依赖与画面配置保护；像素技能不驱动相机/资源/1/60调整。未启动Profiler/capture/M0。四1000AI120+180窗口尚待终态，H07/H11未达、Goal active。

状态：PLANNED / NO_NEW_MEASUREMENT。

问题：当前普通collector约50ms、占逻辑约60%，旧PairExactLoop无法拆出coarse/binding/pair allowed/exact work。只诊断，不增算法或切生产默认。

Task/Record在docs/ai回链；全部10备份当前SHA匹配，before.json保存。原Editor19040/Menu clean idle已核。新测试/编译/四1000AI OFF/ON窗口均待，不声称新FPS/0GC/正式性能证书。

四新增子phase为父PairExactLoop的子区间，禁止将全phase sum解释整tick；ON需用同代码OFF对照测仪器开销。原H07/H11未达，六项4/6/限定产物5/6，Goal active；本批仍PLANNED、非已执行。
