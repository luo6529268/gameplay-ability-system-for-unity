# 第十一批 M-03 中央 Mesh backend 受控基线 Task Contract

Task NTSD-OPTIMIZATION-BATCH11-MESH-BACKEND-BASELINE-20261006 / SCOPED_BACKEND_BASELINE_PASS；Change NTSD-OPT-M03-MESH-BACKEND-BASELINE-011 / VERIFIED（Editor测试限定）。
用户当前“开始执行下一批的任务”；承接子批10，先当前代码受控局部基线，不启动完整M0/EXT-1。
原Editor PID19040 / port6401，Unity2022.3.62f3 / URP14.0.11；Menu clean/8roots/非Play/空闲。
生产publication没有真实战斗样本：本批不将fixture分类或命令数冒充自然publication、visible实体或AI数量。

## 准确范围和设计

唯一新增代码：Assets/NTSD/Scripts/Animation/Rendering/Editor/BattleMeshUploadBaselineEditorTests.cs/.meta。
不修改生产脚本；直接运行原BattleDynamicMeshBackend.Build，容量预热后seal。
100/500/1000 quad commands，OrderedChunks同几何/预先创建的两份位置变化frame；
1000 StrictOrderedDraw与material-variant交错对照；4097 Ordered跨chunk同/变化对照。
每case预热64、采样1800，记录Stopwatch CPU elapsed的p50/p95/p99/max、实际Mesh stream0 stride、
成功vertex upload API数/顶点/字节、physical segment/chunk、容量增长与当前线程托管分配。
资源resolver为固定的合成Resolved binding，null Texture/Material；不是catalog/资源/driver渲染证书。
数据只由真实Build/API诊断获得，不调用RecordBackendBuild伪造production三分类或自动采样。
Stopwatch/读标量测量开销包含在局部采样内，不启Profiler；准备/断言/JSON分配在窗口外。
结果通过NUnit output导出，再由主代理保存新的唯一证据文件，不在fixture内写文件。

## 验收、权限边界及恢复

测试仅原Editor具名EditMode；不切Scene、不进入Play、无Profiler/FrameDebugger/GPU capture/完整M0。
局部预热后0B、capacityGrowth0、实际API payload按有效顶点/stride复算；
重复frame依旧上传是现状基线，不作为应当失败的修复RED；没有性能阈值/FPS/收益判据。
CPU样本排除publication捕获、真实Resolve/插值、Foot/Health、RenderPass/DrawMesh、GPU与模拟/AI；
新代码仅Editor测试，没有runtime owner/queue/resource接入，不改变11阶段关闭。
每caseusing Dispose本地backend；只释放本fixture内存对象，不涉及文件删除。
生产代码/Q06 hash-only，33ms/3ms/Hostmax2、checksum/pass/RNG/排序/UV/slot/lease/资源/segment/fail-closed不变。
PERF/ATLAS/EXT-1/MONO正文及USER_HOLD/PROPOSED-MODIFY_REQUIRED门保持，bank/预算/格式/Scene/Settings/Server不写。
七dirty文档备份已核SHA；324保护文件包括前十批代码/证据、Scene/设置与Q06。
回滚需另授权，只撤本批新fixture与具名文档追加；不能从HEAD覆盖dirty文件。
[Operation](../FILE-OPERATIONS/NTSD-OPTIMIZATION-BATCH11-MESH-BACKEND-BASELINE-20261006/RECORD.md)；[报告](../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH11-MESH-BACKEND-BASELINE-20261006/REPORT.md)。

实际新12/12、相关旧26/26，38去重Passed；原Editor最新compile-CS0且test DLL晚于source。
局部stride44/0B/零growth及API/segment复算通过，12组CPU p50/p95/p99/max已存，非生产帧率或收益。
测试Runner临时untitled scene并非原Menu/Battle生产窗口；原Menu前后clean/8roots/非Play。
生产World/publication/自然alpha/完整0GC/关闭重进/1000AI/Android仍待具名验收，父项OPEN。
