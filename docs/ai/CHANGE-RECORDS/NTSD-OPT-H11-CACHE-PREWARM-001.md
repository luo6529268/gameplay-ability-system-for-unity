<!-- CHANGE-RECORD
id: NTSD-OPT-H11-CACHE-PREWARM-001
status: RUNTIME_PENDING
change-kind: CODE
code-path: Assets/NTSD/Scripts/Animation/Rendering/BattleCentralRenderSystem.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/BattleDynamicMeshBackend.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattlePresentationCapacityPrewarmEditorTests.cs
authority: current user approval to start documented optimization; H-11 presentation-only cache prewarm; formal336B44 rules unchanged
evidence: artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH01-CAPACITY-20261006/REPORT.md
-->

# NTSD-OPT-H11-CACHE-PREWARM-001

Task：[第一批合同](../TASKS/NTSD-OPTIMIZATION-BATCH01-CAPACITY-20261006.md)。
需求：用户批准按优化文档开始，H-11现有两个预热遗漏；只表现缓存，不改规则。
原状：central PrepareBattleCapacity预热backend/submission/catalog而未预热DisplayMotion；
backend PrepareCapacity没有为chunk准备SubMeshDescriptor[]，Upload首次/新高水位可分配。
先新增聚焦RED；后复用DisplayMotion.PrepareCapacity与chunk自有预热函数。

准确职责：
- BattleCentralRenderSystem.PrepareBattleCapacity：按entityCapacity接入DisplayMotion现有缓存预热。
- BattleDynamicMeshBackend.PrepareCapacity/BattleMeshChunk：每chunk描述容量上界为
  min(QuadsPerChunk,commandCapacity-chunkIndex*QuadsPerChunk)，按成功quad最大计数推导，
  不是按原始命令纹理位置分册；未解析命令不能增加该上界。只增不缩、描述引用保留。
- 新test：中央入口数组已预热、高slot无数组增长、描述高水位/严格模式/跨chunk及幂等，不使用Play。

预期副作用：启动时更早分配现有managed缓存，增加可计量steady驻留，不改变绘制顺序/顶点/物理submesh；
绝不把native/GPU首次结构分配说成已消除。现有diagnostic恢复/增长策略保留，
完整sealed capacity fail-closed合同尚未在本子批闭合，父H-11继续开放。

所有权/关闭：DisplayMotion沿用central static lookup，chunk缓存沿用backend owner/Dispose；
无需新增服务、worker、queue或重排11阶段；central关闭先退休submission，保留读lease依赖。
不可回退边界：不修改权威/资源/Scene/ProjectSettings/Server/Q06排序/EXT-1，无数据迁移。
验收：RED→GREEN、已有submesh/bounds回归、Unity编译/聚焦证据、保护SHA和ChangeLedger；
原Battle/full热路径0GC/跨架构checksum/设备证书未运行则明确pending。
回滚：另获用户批准后审查本包逆向hunk；固定before commit及精确哈希见Operation。

2026-10-06 RED：原Editor精确新class共7个检查完成，job117da49c9b9340b6a02cde0683ddecaf
记录6个预期失败：central数组16<1050、4个描述预热null、存储复用null；
display自行预热正例未失败。progress.total9028是全测试树计数，不是运行9028个；本次completed7。
原Editor Menu/idle/非Play，运行无Scene切换；MCP execute_code因CodeDOM命令长度失败，
未执行C#预检；改用原有get_editor_state/manage_graphics资源成功，实际URP确认。
本次只改两runtime文件的现有预热入口/私有chunk缓存函数，先前shader/UV与排序原文保留；
不把未使用预热入口的diagnostic fallback增长宣称已关闭。

2026-10-06 GREEN：原Editor新class NTSD.Test.BattlePresentationCapacityPrewarmEditorTests
job2bc5c122aa65405ba5066eba0a8fcd57：7/7 PASS；submesh/bounds两旧class
jobe9e9a108d17c4512850c3d472c81ee95：8/8 PASS。第二请求误写新class namespace，
所以只执行旧8项；随后用准确NTSD.Test class单独7项并留两份原件，未把缺跑当通过。
实际Assembly-CSharp/Editor编译时间18:38:09/10，最新Editor error CS查询0。
Display lookup预热后64次测得0B、5个数组引用不变、publication PreciseX仍120；
mesh预热覆盖1/32/4096/4097，严格模式1→32 segment无描述数组替换，缩容量不缩存储；
已有尾段descriptor/physical high-water/bounds/empty/chunk回归通过。
仅对应managed缓存提前分配，不改变quad或segment结果；steady缓存成本前移，未测总峰值。
未运行：完整物化/上传/录制/提交0GC、seal/overflow矩阵、真实Battle/退出重进、
checksum跨架构、Player/Android/1000 AI与GPU证书。父H-11继续OPEN，非全域VERIFIED。
