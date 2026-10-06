<!-- CHANGE-RECORD
id: NTSD-OPT-M03-MATERIALIZATION-COUNTERS-008
status: RUNTIME_PENDING
change-kind: CODE
code-path: Assets/NTSD/Scripts/Animation/Rendering/BattleCentralRenderSystem.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/Editor/BattleMaterializationCountersEditorTests.cs
authority: user approval to execute next documented M-03 diagnostic batch; formal336B44 rules unchanged
evidence: artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH08-MATERIALIZATION-COUNTERS-20261006/REPORT.md
-->

# M-03 显示请求分类与物化入口计数

[Task](../TASKS/NTSD-OPTIMIZATION-BATCH08-MATERIALIZATION-COUNTERS-20261006.md)；
[Operation](../FILE-OPERATIONS/NTSD-OPTIMIZATION-BATCH08-MATERIALIZATION-COUNTERS-20261006/RECORD.md)。
事前原状、准确符号、scope、风险、生命周期与回滚见Task。
只补有效CentralOnly request分类/Prepare attempt/原去重及busy原因累计计数；
没有actual Build/upload/GPU对应结论，不改原条件、时间取样或控制流结果。
diagnostics现有生命周期，ResetRuntime清observer world引用，无新增关闭阶段。
每请求标量额外开销尚未测；局部helper 0B不等于完整链0GC。
真实Battle/1000AI/Android/同布局基线与后续脏区设计待验证。

2026-10-06 21:31:25八个精确current/dirty备份SHA通过；211保护保持。
脚本前Task/Change/Operation/INDEX/Ledger/STATE/handoff已登记，开始reflection test-first。

首次测试编译2项CS0246：当前NUnit不含NonParallelizable attribute；仅移除此测试标记，
原始错误保留，不计RED或通过；原生产SHA仍与before相同。

第二次测试编译2项CS0117：fixture误用Backend，改为现有MeshBackend属性。
旧DLL首次job673c93b2a4384b899aa7fae5f43cffda实际选中0项，原件保留，不算RED/PASS。

有效RED f478898e79d04960b658409c5c9dd45d完成20项，20条missing helper/counter预期失败；
桥接最终result=null，保留completed=20及20条failure，不伪造summary。
测试DLL21:34:53晚于fixture21:34:43，原EditorCS0；生产21:35仍before，开始最小生产实现。

实际生产：同文件新增sample-kind/11个long累计/三scalar helper；有效CentralOnly入口观察、
原same-frame/same-sample/busy return记原因、Prepare前attempt、ResetRuntime清baseline。
原skip predicate与ResolveAlpha/Prepare实现不变；只有观测计数，不改Mesh/slot/排序/publication。
CODE_WRITTEN，GREEN/既有182回归待运行；真实收益与完整链验收保持待验证。

原Editor生产DLL21:36:33/测试DLL21:36:35晚于生产源21:36:25，CS0；reload retry原件保留。
GREEN1495592fcaf448968d18c1f57f6fd727实际20/20，0失败/跳过。
预备64后512次cached request+attempt scalar helper当前线程managed0B，反射/创建/断言在窗口外。
forced synthetic cached plan测试attempt2但vertex upload计数不变，不声称像素已提交。
既有182限定回归正在运行；父M-03 OPEN，本Change RUNTIME_PENDING。

旧回归01af1db25eb243fe873421f9c238d973实际182/182，0失败/跳过；含旧168与子批07新14。
新20+旧182去重202。21:39:56八backup/211保护SHA及HEAD保持。
保护初次辅助检查误用sha256字段，按manifest真实expected字段重跑0差异，无任何保护文件改动。
后续实际Build按类/报告出口与完整链0GC、动态Play alpha和真实性能尚未闭合，不扩大局部结果。

静态限定交付：21:43 Ledger PASSED1304/17、4237既有warning/0error；全diff exit0。
126限定链接完整，新test/meta无尾空白；post Menu clean/idle非Play、CS0。
Task/Change/父项RUNTIME_PENDING保持，最终精确after及保护/source复核见Operation与本批报告。

21:47:01最终Ledger同样PASSED1304/17、4237warning/0error，全diff exit0；127限定链接完整。
交付source与已编译SHA一致，8backup/211保护再次保持；Operation after不含自身递归SHA。
