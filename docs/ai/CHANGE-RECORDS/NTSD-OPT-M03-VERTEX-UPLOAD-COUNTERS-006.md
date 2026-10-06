<!-- CHANGE-RECORD
id: NTSD-OPT-M03-VERTEX-UPLOAD-COUNTERS-006
status: RUNTIME_PENDING
change-kind: CODE
code-path: Assets/NTSD/Scripts/Animation/Rendering/BattleCentralRenderTypes.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/BattleDynamicMeshBackend.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleVertexUploadDiagnosticsEditorTests.cs
authority: user approval to execute next documented M-03 diagnostics batch; formal336B44 rules unchanged
evidence: artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH06-VERTEX-UPLOAD-COUNTERS-20261006/REPORT.md
-->

# M-03 中央 Mesh 实际 API 顶点上传计数

[Task](../TASKS/NTSD-OPTIMIZATION-BATCH06-VERTEX-UPLOAD-COUNTERS-20261006.md)；[Operation](../FILE-OPERATIONS/NTSD-OPTIMIZATION-BATCH06-VERTEX-UPLOAD-COUNTERS-20261006/RECORD.md)。
原状：Build.Reset后解析/写入/按活动chunk.Upload，只有command/chunk/segment，没有直接payload统计。
最小改动：diagnostics三个标量及Reset，Upload成功返回后计数，
CreateMesh预备时缓存实际stream0 stride，Build传已有diagnostics，无容器分配/新owner。
只CPU Mesh API payload口径；不改变上传/拓扑/排序/失败/slot/lease/关闭合同，
不当成GPU流量/batch/性能证书。具体异常/准入前拒绝/预热口径及test-first见Task。
原Editor具名EditMode验证，不启Play/完整M0/设备；父M-03及H-11完整链门开放。
准确九个current/dirty备份先完成后测试；不读Q06 body，不改Scene/PERF/ATLAS/MONO/EXT-1/资源。
预计副作用只每实际上传三标量算术、每创建Mesh一次stride查询；未测开销/收益。
恢复需另批准，依精确backup只逆向本批hunk，保留前五批及任务外dirty。

2026-10-06 test-first：新18项case及meta已写；生产两个文件仍before原状。
18项含容量/两模式/重复/缩小/准备/缺失/整帧拒绝/第一次及第二chunk异常/局部0GC。
反射仅在断言/准备阶段使用；分配窗口只现有Build/Upload，不运行动态反射或assert。
九个20:33:57备份SHA匹配；索引新增条目先局部撤去核回before SHA、备份后恢复本条，
只涉及本批自建索引行，未撤销其它内容或执行Git回退。原Editor RED待运行。

2026-10-06 有效RED job55dbdae760a44456833c5c515e2f873d完成18项，全部在缺失
VertexUploadCallCount属性处预期失败；非fixture构造失败，GC断言未到达，不能声称原状测出GC。
Editor测试程序集20:36:39实际编译、CS error0；两个生产SHA在20:37:22仍before。
127保护文件均无变化。随后两个生产文件仅加计数/reset/Upload参数与创建期stride缓存，
已CODE_WRITTEN；上传次数/拓扑/原失败策略不变，GREEN/旧132回归待实际执行。

2026-10-06 实际compile：生产DLL20:38:00、Editor测试DLL20:38:05，晚于生产源20:37:50，
原Editor idle/Menu clean非Play/CS error0；无第二Editor。
GREEN job14486f1f5037435fa49389e8dd3be893新18/18通过，0失败/跳过。
首尾/跨chunk/解析缺失/异常计数/整帧拒绝/重复Build口径均通过；
Ordered/Strict各预热后48次Build（稀疏→空→4097命令）fixture调用当前线程managed0B。
该GC范围不包括完整materializer/录制提交，不是1000AI/Native/GPU/Android性能证书。
旧132相关具名回归已请求，父M-03仍OPEN；本Record RUNTIME_PENDING（新聚焦通过）。

2026-10-06 旧回归job47356cdbfea543a3b760504767ebfe91实际132/132通过，0失败/跳过；
包含前五批/解析/mesh/物化/插值/辅助/共用绑定，新18与旧132去重150项。
本次生产两个文件净新增15行，只有属性/Reset/Upload计数参数和实际stride缓存，
无上传算法、segment或数据布局修改。M-03/主总表已如实记RUNTIME_PENDING，父项开放。

2026-10-06 静态交付：Ledger PASSED1302 Records/14 code files，4238既有warning/0error；
diff --check exit0（LF/CRLF提示不当error）、34项高12中14低8，111限定链接无缺失。
127保护/前五批/其它文件与九backup SHA保持，原Editorpost Menu clean/idle/非Play/CS error0。
Operation限定文件操作VERIFIED，Task/本Record仍RUNTIME_PENDING；真实Battle/完整链/收益/
1000AI/Android未验，不晋升父项。最终after清单记录准确字节，不丢弃原状/RED/retry证据。

