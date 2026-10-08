# 第45批：普通Brute低频分支计时

## 最终：SCOPED_SAMPLED_DIAGNOSTIC_VERIFIED / PERFORMANCE_FAIL

最终治理：原六进度文档已同步；pwsh Tools/Validate-ChangeLedger.ps1 -RepositoryRoot 当前仓库exit0，1341 Record/14代码diff/4257历史warning/0error；git diff --check exit0（CRLF提示），validation-final-01.json。38仍PLANNED，本批不宣称优化收益或父项通过。

四窗口于2026-10-07T12:36:38.7233565Z完成，MEASUREMENTS_COMPLETED / DONE，completedRuns4、error空。各实际1000AI、120warm+180sample、workloadValid true；计时flag和stride恢复，四生产默认不变，cache/geometry300应用0fallback。原Editor19040已确认Menu单Scene8roots clean/idle/非Play/非compiling，无第二Editor。

| 短窗口 | logic平均/P95 ms | collector平均 ms | display平均 ms | droppedBacklogTicks |
|---|---|---|---|---|
| Dispersed OFF | 77.605 / 92.222 | 48.312 | 246.400（倒数4.058FPS） | 742 |
| Dispersed stride64 ON | 80.305 / 96.881 | 50.968 | 246.893 | 756 |
| Combat OFF | 79.950 / 103.270 | 50.740 | 234.793（倒数4.259FPS） | 688 |
| Combat stride64 ON | 84.230 / 108.543 | 54.280 | 242.604 | 724 |

collector同代码OFF→ON差2.656ms/5.498%、3.540ms/6.977%；比44全pair计时实测差35.295/11.034ms小，但顺序运行环境仍混入差值，不能证明全部差由计时造成，不能报纯分支成本或生产优化/FPS收益。不预造“足够低”的硬阈值，不追加同构计时细化/重复窗口。

诊断总coverage（含warm+sample，不是steady-only）：
- Dispersed eligible50,284,665 / timed785,688，约1.562480%；rejected-binding visited42,672,114/timed666,641；pairAllowed7,612,551/119,047；exact5,323,630/83,416。
- Combat eligible46,509,444 / timed726,703，约1.562485%；rejected-binding38,141,115/595,801；pairAllowed8,368,329/130,902；exact5,746,262/89,986。
- OFF所有coverage0，开关/stride不反写逻辑。拒绝方向占整个warm+sample约84.861%/82.007%，这是访问频次，不是耗时占比。detail phase均值仅steady180tick，与全period coverage口径不同；不乘64外推全量纯cost，不将nested sum解释整tick。

两OFF/ON末tick300完整checksum JSON字节同、20个Parity/Lockstep hash均同；不代替逐tick/native对照。capacity critical/reject0、teardown World对象/实体/槽/active pool/ReferencePool0、cleanup exception0；suite有序关闭及Scene恢复通过。terminal-source-audit-01.json保留真实remaining字段（原windows-audit-01选取不存在remainingPoolBorrowers为null，不把null当0）。三源码/27guards/9backup/HEAD全稳定，Q06仅hash。

221/221实际focused与19新case有效，局部准备后128collection managed0B只证明所测诊断路径。原logic rawGC counter未校准/精确GC.Alloc handle不可用仍UNKNOWN，H11完整camera失败不消去。所有logic P95仍>33ms，累计dropped非零；H07正式120+1800门未过，本批不是性能证书/Android/120FPS证书。

下一必要动作转实际候选资格：已有38仅PLANNED的kind5存在性缓存，对应本次大量coarse拒绝及当前PassesReleaseCoarsePrefilterCached在ordinary union不相交后逐ITR寻找kind5的确切路径。先更新38旧事前默认false语境为当前40生产默认基线、精确备份/测试与固定无kind5/有kind5夹具的正确性/收益资格；有收益才考虑原千人矩阵，不因旧编号自动实施/推广，不把频次本身当已证收益。不是新索引/collector，也不重开H06/全历史对齐。此阶段不继续细化同一个计时器。

H07/H11 OPEN，六项阶段4/6、限定产物5/6、34父关闭0、23已执行+38仅PLANNED累计24，Goal active；次数只复盘。M03/H06/M13/M14已过限定门不重开，EXT1/Mono/ATLAS/Role-aware推广不解冻，33/3ms/max2与全部运行红线保持。最终Ledger/diff另补实际结果。
下方WINDOWS_READY/PLANNED为历史快照。


WINDOWS_READY：原Editor实际221/221 PASS、0skip（16.9464759s），test-regression-detail-01.json含完整具名结果。三源码/27guards/9backup/HEAD冻结；原Menu单Scene8roots clean、idle、非Play/非compiling、errorCS0。尚未测新四窗口/tax/FPS；阶段4/6、产物5/6、父关闭0、23已执行+38 PLANNED累计24、Goal active，H07/H11未达。像素技能仅守现有URP/画面保护，不改相机/PPU/1/60；没有声称调用提供的DetectPipeline方法或做渲染修复。

首GREEN实际19/19 PASS(2.7013149s)，job9bde15ef15674e15a9e53ca775a0952a，test-green-01.json；有效RED为10query+9Suite两作业，namespace selector修正只补未执行9，不抹去原件。DefaultOFF/stride1兼容、stride64旋转/完整覆盖、候选RNG和局部128collection managed0B、normal/abort共用恢复方法/四request保持通过；相关旧门及千人窗口/开销尚未验证，不当H07性能通过。

PLANNED / NO_NEW_CODE_OR_MEASUREMENT。依据44实际OFF/ON干扰，只同算法clock stride64资格和完整coverage，三个准确C#，先RED/GREEN及必要旧门。未有新FPS/1000AI/全链0GC证据；H07/H11仍未达，Goal active。Task/Record/Operation事前已登记；按当前六项完成边界执行，不因批次次数停止。
