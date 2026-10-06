# 第七批 M-03 上传计数报告出口 Task Contract

Task NTSD-OPTIMIZATION-BATCH07-UPLOAD-REPORT-20261006 / RUNTIME_PENDING；Change NTSD-OPT-M03-UPLOAD-REPORT-007 / RUNTIME_PENDING。
用户授权：继续下一批，按M-03“benchmark/报告出口接线”补前六批计数，无算法或专项授权扩大。

原状：backend当前Build诊断已有三个计数，benchmark v5尚未导出；完成帧延后drain，
直接读live diagnostics可能读到后续变化。v5拒绝未登记availability项，不能把可选指标混进必测注册表。

准确生产路径 Assets/NTSD/Scripts/Animation/Rendering/BattleRenderingBenchmark.cs：
BattleRenderingBenchmarkFrame属性/ToProjection，BattleRenderingBenchmarkReport.BuildSummary/limitations，
BattleRenderingBenchmarkSession待完成帧快照/BeginCompletedFrameRequest/CaptureSample。
测试 Assets/NTSD/Scripts/Animation/Rendering/Editor/BattleVertexUploadReportEditorTests.cs/.meta。

centralVertexUploadCalls(count)、centralUploadedVertices(count)、centralUploadedVertexBytes(bytes)为可选诊断；
Present成功且workload验证后冻结当前Build值；只已接受的completed-frame样本进入summary，
warmup/被拒绝尝试不进入，不当全运行累计。Legacy或null diagnostics是unavailable，不冒充0。
仅中央Mesh API已完成vertex payload，不含索引/辅助/GPU流量或生产RenderPass/draw/batch。
run/suite v5 schema与mandatory availability/verdict不变；增加可选字段和明确scope。
报告原有分配保留；新增快照仅标量复用，无新生命周期owner，既有有序关闭不变。

验收：值/单位/大字节值、available0与unavailable、两次独立样本、延后drain快照、
warmup/无效重试/Present异常、稳定导出、v5必测/判据不变、预备后helper局部managed0B。
原Editor test-first RED/GREEN、新测试与既有150+限定benchmark回归、实际compile/CS0/Menu clean；
八备份/174保护SHA、Ledger/diff/限定链接/准确after。
真实1000AI、完整物化—上传—录制—提交0GC、publication/alpha分组、收益、GPU/Android均待验证。

不读Q06活跃body；不变33ms/3ms/2interval/pass/RNG/checksum/publication/排序/segment/fail-closed/slot/lease；
不修改PERF/ATLAS/MONO/EXT-1正文及bank/预算/格式，不启动专项M0/instancing/Play/场景改动。
EXT-1 PROPOSED / MODIFY_REQUIRED，MONO USER_HOLD保持；父M-03 OPEN，报告接线不等于性能优化完成。

[Operation](../FILE-OPERATIONS/NTSD-OPTIMIZATION-BATCH07-UPLOAD-REPORT-20261006/RECORD.md)；
[报告](../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH07-UPLOAD-REPORT-20261006/REPORT.md)；
[统一进度](../../../Assets/NTSD/Docs/battle-optimization-progress-tracker.md)。
恢复另获批准，使用精确before只逆向本批hunk，不从HEAD覆盖既有修改。

2026-10-06 实际限定结果：先14项缺失字段/helper RED，唯一生产文件36插入行；
原Editor实际生产/测试DLL重编译，GREEN14/14、旧150+限定benchmark18=168/168，去重182，0失败/跳过。
两模式各128次新增helper局部managed0B；实际Editor仍非Play，注入数值与IsPlaying策略上下文仅fixture。
首编译缺命名空间3项CS已加using修复，首次选中0项/reloadretry均保留且不冒充RED或PASS。
父M-03/完整链0GC/publication-alpha分类/真实性能/1000AI/Android尚未关闭；静态交付另见报告。
