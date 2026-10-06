# 第五批 H-11 原生Mesh重新预热 Task Contract

Task NTSD-OPTIMIZATION-BATCH05-NATIVE-MESH-PREWARM-20261006 / RUNTIME_PENDING；Change NTSD-OPT-H11-NATIVE-MESH-PREWARM-005 / RUNTIME_PENDING。
需求来源：用户“开始执行下一批的任务”，继续H-11最小预热与容量合同，不解冻专项。
本次原状：BattleDynamicMeshBackend.PrepareCapacity对existing chunk只准备descriptor数组；
其native Mesh失效时EnsureMesh直到Build.Upload或GetChunkMesh才创建Mesh和native buffers。
因此代码已确认存在晚创建路径；实际Battle重进触发频率/成本未测，不写成已复现每帧GC。

生产exact code path：Assets/NTSD/Scripts/Animation/Rendering/BattleDynamicMeshBackend.cs，
仅PrepareCapacity；EnsureChunk后访问chunks[chunkIndex].Mesh使原EnsureMesh在启动预热先恢复。
新增测试Assets/NTSD/Scripts/Test/Editor/BattleNativeMeshPrewarmEditorTests.cs。
不新增模拟字段/owner/queue/native格式；chunks仍由backend持有、Clear/Dispose按原关闭路径；
原SealCapacity/UnsealCapacity/EndBattleCapacitySeal、slot/lease/退役和十一阶段关闭保持。
正常existing Mesh身份/geometry不改，只有已失效资源的既有重建提前；可能增加启动成本，
本轮不冻结总内存/帧率预算；战斗中突发native Mesh丢失的原恢复策略仍另列未闭合风险。

test-first原Editor PID19040/TCP6401 Unity2022.3.62f3 URP，Menu clean非Play：
毁损0/1/17/4096/4097容量与精确重新准备，两个chunk各恢复；准备不Build、不提交；
descriptor/storage身份、顶点stride44/UInt16/index buffer/bounds初始化及控制路径不退化；
sealed Prepare仍拒绝、不偷偷恢复；0容量不创建；Unseal后原准备可恢复；
恢复后首次Build包含Upload的局部managed GC、连续严格/兼容/稀疏/空帧转换与跨chunk回归。
测试的DestroyImmediate仅释放本fixture的临时内存Mesh，不删除文件或用户资源。
原120项相关聚焦回归、实际compile/Console CS0、Menu clean、保护SHA、Ledger/diff/本批引用。
原生构建/GC结果仅限定本调用范围，不覆盖command materializer/Q06、RenderPass执行/GPU消费、
真实Battle enter/exit/re-enter、no-domain-reload实际重进、完整SelfCheck/M0/1000AI/Android。

红线：33ms/3ms/2interval、同input checksum、publication/插值/排序/first-visible/latency、UV/tint/
slice/segment/fail-closed、CPU lease≠GPU完成、11阶段关闭均不变。
不读取活跃Q06方法体，不改Scene/Prefab/资源/Settings/PERF/ATLAS/MONO/EXT-1/InputActions/Server。
EXT-1仍PROPOSED / MODIFY_REQUIRED，无专项M0或instancing、bank/预算/格式选择。

准确dirty备份和恢复来源：[Operation](../FILE-OPERATIONS/NTSD-OPTIMIZATION-BATCH05-NATIVE-MESH-PREWARM-20261006/RECORD.md)；
回滚须获批准，仅逆向本批Prepare hunk和具名新测试，保留前四批及任务外工作。
证据按实际追加：[报告](../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH05-NATIVE-MESH-PREWARM-20261006/REPORT.md)；
[统一进度](../../../Assets/NTSD/Docs/battle-optimization-progress-tracker.md)。

实际限定结果：有效RED12项10预期失败/2控制通过；修补后新12/12、旧120/120、去重132。
原Editor PID19040/TCP6401生产程序集20:15:22实际compile/CS error0，Menu clean非Play。
四组恢复后首次与尾部25 Build/Upload局部0B；没有真实Battle无Domain Reload重进、
完整SelfCheck/central0GC/1000AI/native/GPU/Android证书，不称父H-11关闭。
