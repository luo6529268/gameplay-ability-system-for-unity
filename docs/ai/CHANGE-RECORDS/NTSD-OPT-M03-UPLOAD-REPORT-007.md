<!-- CHANGE-RECORD
id: NTSD-OPT-M03-UPLOAD-REPORT-007
status: RUNTIME_PENDING
change-kind: CODE
code-path: Assets/NTSD/Scripts/Animation/Rendering/BattleRenderingBenchmark.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/Editor/BattleVertexUploadReportEditorTests.cs
authority: user approval to execute next documented M-03 report batch; formal336B44 rules unchanged
evidence: artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH07-UPLOAD-REPORT-20261006/REPORT.md
-->

# M-03 上传计数冻结与可选报告出口

[Task](../TASKS/NTSD-OPTIMIZATION-BATCH07-UPLOAD-REPORT-20261006.md)；
[Operation](../FILE-OPERATIONS/NTSD-OPTIMIZATION-BATCH07-UPLOAD-REPORT-20261006/RECORD.md)。
修改前原状、准确符号、口径、风险、验证与回滚见Task。
生产只新增三标量快照/可选frame及summary/limitations，不改变必测v5注册表或判据。
Present成功并验证后冻结，避免completed-frame等待时live值污染；Legacy/null unavailable。
所有对象仍原Session/report生命周期，无新增资源/worker/lease，无关闭阶段变更。
副作用：每successful Present读取三值、报告构造增加三可选属性，实际开销与收益未测。
完整链0GC/真实Battle/1000AI/GPU/Android开放；不改显示表示、资源格式或专项门。

2026-10-06 20:59:10八个精确备份完成。test-first新14项case/.meta已写，生产仍before。
首次测试编译缺少Presentation namespace（3项CS）；原始输出保留，已仅加using修复。
首次job1d4be43a0b8e4dd690ddcdca136398f9选中0项，不能算RED或PASS，等待实际重编译重试。

有效RED b1c1bc6c3b424f79912141c011da62db完成14项、14项均在缺失可选属性/字段/helper处失败。
桥接最终result为null，完整14条failure和completed=14保存在原件，不伪造summary；无fixture构造失败。
21:02:24测试DLL实际更新、原EditorCS0；生产21:03:27仍before。局部GC断言未到达不声称测出分配。

有效RED后仅BattleRenderingBenchmark生产hunk：三属性/投影/summary/limitations、
三个pending标量、成功Present后快照helper、CaptureSample引用。原v5availability及判据未改。
无上传算法/拓扑/资源/slot/lease/排序/模拟修改，CODE_WRITTEN，GREEN和相关回归待运行。

原Editor实际生产DLL21:04:57/测试DLL21:05:08晚于源21:04:52，CS0。
GREEN df4cf4969eab4408bb15a7243a8b390d实际14/14，0失败/跳过；
central/Legacy预备后各128次新增快照helper当前线程managed0B，不包括CaptureFrame/report原有分配。
大字节值/等待快照/独立样本/warmup/重试/异常/不可用/导出/v5未变均聚焦通过；
既有150与18项限定benchmark回归待实际结果，父M-03仍OPEN，本Change RUNTIME_PENDING。

旧回归103d73738f634593850c56252a956f80实际168/168，0失败/跳过，含旧150及18个限定benchmark合同。
GREEN14与旧168去重182；无整套benchmark/实际Profiler或Play。八backup/174保护SHA仍保持。
Ledger21:07:02 PASSED1303 Records/16code、4237warning/0error；余final静态核对如实另记。

最终限定静态：21:11:30 Ledger同样PASSED1303/16、4237warning/0error；
首次三个治理文件新EOF空行导致diff exit2，去自身新增空行后21:13:14全diff exit0，未清理历史。
122限定链接完整，八备份/174保护SHA与HEAD保持；Menu clean/idle/非Play，CS0。
精确after清单见Operation；限定文件操作VERIFIED不晋升本RUNTIME_PENDING或父项M-03。
