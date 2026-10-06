# 第六批 M-03 顶点上传统计 Task Contract

Task NTSD-OPTIMIZATION-BATCH06-VERTEX-UPLOAD-COUNTERS-20261006 / RUNTIME_PENDING；Change NTSD-OPT-M03-VERTEX-UPLOAD-COUNTERS-006 / RUNTIME_PENDING。
用户授权：继续下一批，按M-03“保存chunk/command计数与上传字节”及共同启动门先补缺失instrument。
原状：中央Build诊断已有command/chunk/segment数，没有直接记录SetVertexBufferData的数据量；
每次Build仍上传各活动chunk的有效顶点前缀，A1 dirty-chunk跳过未实施。

准确生产路径/符号：
- Assets/NTSD/Scripts/Animation/Rendering/BattleCentralRenderTypes.cs：BattleCentralBuildDiagnostics属性和Reset。
- Assets/NTSD/Scripts/Animation/Rendering/BattleDynamicMeshBackend.cs：Build.Upload参数，
  BattleMeshChunk.Upload成功API返回处、CreateMesh缓存stream0 stride。
新测试：Assets/NTSD/Scripts/Test/Editor/BattleVertexUploadDiagnosticsEditorTests.cs/.meta。

口径：VertexUploadCallCount(int)、UploadedVertexCount(long)、UploadedVertexBytes(long)。
统计当前获准进入Build的尝试中已经成功返回的中央实体Mesh SetVertexBufferData调用；
字节=实际提交顶点数×该Mesh创建时查询的stream0 stride，不硬编码44。
Build.Reset/Clear清零，不累加多个Build；重复显示Build照实计数，不暗示去重。
PrepareCapacity/原生重建/索引上传/子网格元数据/Foot/Health/RenderPass/GPU数据量不在本计数范围。
准入前null resolver/disposed/容量拒绝没有开始Build，按原合同不改变上次诊断；
准入后异常可以保留已成功API调用的部分计数，不代表部分submission获准。
只给CPU Mesh API payload口径，不把它当GPU实际传输、GPU batch或性能收益。

验收：空/null帧、1/17/4096/4097与两draw模式、同帧重复、
缩小/Clear、准备/恢复不计入、解析缺失、seal整帧拒绝不改变旧诊断、
非有限数据第一次/第二chunk失败口径；预热后的两模式循环Build局部0B。
test-first经原Editor实际编译/RED/GREEN，旧132相关具名回归、Console CS0、
Menu clean、九备份与保护SHA、Ledger/diff/链接检查。
保留canonical几何/命令顺序/插值/first-visible/segment/fail-closed/publication/lease/
GPUconsumer判据/十一阶段关闭。没有新增资源owner；统计随原backend生命周期复用/清理。

完整central物化—上传—录制—提交0GC、publication/alpha分组导出、benchmark报告接线、
真实Battle/像素/1000AI、Native/GPU/Android预算仍开放，父M-03不关闭。
不读Q06活跃方法体，不改Scene/资源/Settings/InputActions/Server/PERF/ATLAS/MONO/EXT-1。
EXT-1仍PROPOSED / MODIFY_REQUIRED，不启动专项M0或instancing/资源决策。
[Operation](../FILE-OPERATIONS/NTSD-OPTIMIZATION-BATCH06-VERTEX-UPLOAD-COUNTERS-20261006/RECORD.md)；[报告](../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH06-VERTEX-UPLOAD-COUNTERS-20261006/REPORT.md)；
[统一进度](../../../Assets/NTSD/Docs/battle-optimization-progress-tracker.md)。
回滚另获批准，仅逆向本批计数hunk和新fixture；before-backups是精确dirty来源，不用HEAD替代。

实际限定结果：原状18项缺失属性RED；原Editor生产/测试实际重编译；
GREEN18/18、旧132/132通过，去重150项，0失败/跳过。
两个模式各48 Build/Upload当前线程managed0B；不包括完整物化/录制提交/GPU/1000AI。
目前诊断前置已聚焦通过，父项M-03与完整运行/性能门开放。静态交付记录另追加。

