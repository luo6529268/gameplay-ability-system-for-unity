<!-- CHANGE-RECORD
id: NTSD-OPT-H11-NATIVE-MESH-PREWARM-005
status: RUNTIME_PENDING
change-kind: CODE
code-path: Assets/NTSD/Scripts/Animation/Rendering/BattleDynamicMeshBackend.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleNativeMeshPrewarmEditorTests.cs
authority: user approval to execute next documented H-11 presentation-only prewarm batch; formal336B44 rules unchanged
evidence: artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH05-NATIVE-MESH-PREWARM-20261006/REPORT.md
-->

# H-11 原生Mesh恢复前移到既有预热入口

[Task](../TASKS/NTSD-OPTIMIZATION-BATCH05-NATIVE-MESH-PREWARM-20261006.md)；[Operation](../FILE-OPERATIONS/NTSD-OPTIMIZATION-BATCH05-NATIVE-MESH-PREWARM-20261006/RECORD.md)。
原状/调用链：PrepareCapacity→EnsureChunk(已有managed chunk不恢复Mesh)→PrepareSubMeshCapacity；
Build.Upload/GetChunkMesh→EnsureMesh(null)→CreateMesh(new Mesh/native vertex/index buffer)。
预计生产hunk仅EnsureChunk后预先访问Mesh；原EnsureMesh实现/丢失恢复语义保持，时点前移到seal前。
副作用：启动时先承担既有重建；没有新增缓存或长期owner，不改render geometry/GPUAPI/segment。
释放沿原backend Dispose、原End解除封口及有序关闭；不改战斗突发毁损恢复政策，不称完整封闭。
准确文件/符号、验收/未验证/rollback见Task；test-first RED/GREEN与原120具名回归后按事实更新。
八文件before/backup已经SHA匹配，保护前四批dirty；无删除/移动/破坏性Git/Q06 body读取。

2026-10-06 test-first：新增12具名case已写，生产仍before原状；CODE_WRITTEN仅测试。
原EditorRED待运行，12项覆盖四容量恢复、选择性/零/封口/解除、四Build/Upload局部GC与尾部转换。

2026-10-06 有效RED ee3e05dddd9e4c148e75d35074b652d7完成12：10项在stored native Mesh仍为空处预期失败，
zero/sealed两控制PASS；不是fixture构造失败。Editor实际编译20:13:15，CS error0，生产SHA仍before。
随后仅PrepareCapacity的EnsureChunk后访问Mesh（两行边界说明），既有EnsureMesh/geometry/恢复策略不改；
GC断言在RED未到达，不宣称原状测得GC。GREEN/回归待实际原Editor运行。

2026-10-06 原Editor生产程序集20:15:22重编译（晚于最终源文件时间），Editor测试程序集20:13:15；
idle/CS error0，Menu非Play；注释方向更正后另一次refresh确认编译字节，唯一生产3插入行。
GREEN请求已启动，实际结果待回收；编译不代替运行门。

2026-10-06 GREEN job8a13f03c355d4d76b68db49d665ae954，新12/12全部PASS、0失败/跳过。
四容量stored Mesh恢复、有效身份/选择性恢复/零容量/sealed拒绝/Unseal保持，
stride44/UInt16/初始化descriptor/index/bounds及原存储身份通过。
四17/4097 × Ordered/Strict的恢复后首次Build+8轮稀疏→空→密集共25 Build，
含resolver/write/mesh Upload局部0B（先暖API，恢复后首次Build未豁免），无缓存增长/身份变化。
这是fixture毁损后重新准备，不是实际关闭Domain Reload的Battle重进或完整central0GC；
战斗中突发Mesh丢失仍为既有恢复政策，未误称封口后的所有native创建都已禁止。
旧120项相关回归请求已发，结果待回收；父H-11/完整链/Scene/预算/设备开放。

2026-10-06 相关旧回归jobd40f11c3b4454c3c9b134fff59cbdeb8实际120/120通过，0失败/跳过。
resolver23+前三批38+mesh8+LatestFrame13+motion2+Foot7/Health8+common6+子批04新15；
本批12与旧120去重132项。原Editorpost Menu clean/idle/nonPlay/CS error0，无第二实例。
唯一生产diff为PrepareCapacity三个插入行；首次恢复后的25 Build/Upload局部0B，
并非真实Battle重进、完整热路径/Native/GPU/Android验收；状态RUNTIME_PENDING（聚焦通过）。
未做Scene保存/设置/资源/Q06/PERF/ATLAS/MONO/EXT-1/Server或任何Git写入。

2026-10-06 静态交付：Ledger PASSED1301 Records/13 code files，4238既有warning/0error；
diff --check exit0、34项高12中14低8、107个限定链接无缺失。99保护/前序/其它文件与8备份SHA保持。
初诊断triplet展平导致错误false汇总，原件保留并独立逐项复核、更正v2，不是文件变化；
未执行清理/恢复/覆写证据。Operation仅文件操作VERIFIED，Task/本Record仍RUNTIME_PENDING。
