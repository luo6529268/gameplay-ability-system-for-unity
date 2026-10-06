<!-- CHANGE-RECORD
id: NTSD-OPT-M03-MATERIALIZATION-REPORT-010
status: RUNTIME_PENDING
change-kind: CODE
code-path: Assets/NTSD/Scripts/Animation/Rendering/BattleCentralRenderSystem.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/BattleCentralMaterializationReport.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/Editor/BattleMaterializationReportEditorTests.cs
authority: user next documented M-03 production report batch; formal336B44 and render contracts unchanged
evidence: artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH10-MATERIALIZATION-REPORT-20261006/REPORT.md
-->

# M-03 生产分组计数显式快照/JSON出口

[Task](../TASKS/NTSD-OPTIMIZATION-BATCH10-MATERIALIZATION-REPORT-20261006.md)；[Operation](../FILE-OPERATIONS/NTSD-OPTIMIZATION-BATCH10-MATERIALIZATION-REPORT-20261006/RECORD.md)。
Task列现状、精确符号、43字段/窗口拒绝语义、副作用、验收和逆向本批hunk恢复。
仅非热路径report/epoch/token/Capture，Build/gate/上传/模拟不变，复用BattleCanonicalJson。
无自动报告/采样、新World引用或owner/queue/资源/lease。先备份及治理登记再fixture RED。
实际执行/证据后追加，尚无PASS。

22:12:05八份准确before已核SHA、脚本前INDEX/Ledger/STATE/handoff登记。
测试fixture22:13:05→实际EditorDLL22:13:30，CS0；生产SHA仍FAAF0C...913C。
有效RED e810c8d7b63646e5abd0889b4c60e03e完成58项，status failed/result null；
failure列表25条capped，均missing CaptureMaterializationReport，原件保留，不伪造58失败summary。
随后新增report固定43long copy/窗口整份拒绝/JSON，既有diagnostics加token/epoch/Capture共9行；
原计数/gate/Build/API算法、benchmark v5、排序/提交等均不变。尚待实际GREEN与旧225回归。

原Editor实际生产DLL22:15:24/测试DLL22:15:27均晚于生产22:15:04及fixture22:13:05，CS0。
GREEN3cd51629c2834313a9433fccfd494a03实际58/58，0失败/跳过；
43字段long冻结、三类/拒绝失败payload、全部43倒退拒绝、跨Reset/来源/非累计端点拒绝、
窗口end/start复算、projection不反写report、Capture不改live、不触发Build均通过。
预備64/采样512既有aggregation+新增epoch Reset当前线程managed0B，非Capture/JSON或完整链。
新report198行/9048bytes，既有central仅9插入，fixture322行；原Build/上传/benchmark不改。
主代理自审：报告私有固定数组且projection重建，窗口只共享不可变endpoint数组；
sourceToken仅object不留World/backend，报告不接热路径/自动采样，生命周期原阶段5Reset保持。
旧225回归已启动030b0ed491c445d7a5b75457e587ceef，最终父项运行/真实测量仍待验收。

行数更正：上一条人工行数198/322有误，实际Get-Content计数report199、fixture309；
既有central9插入与新report9048bytes正确，源码/行为没有因该文档更正变化。

旧225回归030b0ed491c445d7a5b75457e587ceef实际225/225，0失败/跳过；
与新58共283 fullName唯一且Passed。Task/Change最终RUNTIME_PENDING / PRODUCTION_COUNTER_REPORT_FOCUSED_PASS；
父M-03 OPEN。真实动态baseline/完整链0GC/dirty/像素关闭重进/1000AI/Android未验收，不称性能收益。
最终账本/链接/diff/原Editor/保护哈希和准确after见报告；前九批与外部提交保持。
