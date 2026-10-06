# 第十二批 M-03 受控 Mesh 阶段成本 Task Contract

Task NTSD-OPTIMIZATION-BATCH12-MESH-PHASE-BASELINE-20261006 / SCOPED_MESH_PHASE_BASELINE_PASS；Change NTSD-OPT-M03-MESH-PHASE-BASELINE-012 / VERIFIED（Editor fixture限定）。
授权：本会话用户“开始执行下一批的任务”，承接子批11phase定位；只受控Editor采样，不是专项/完整M0。
事前原Editor PID19040/port6401，Unity2022.3.62f3/URP14.0.11，Menu clean/8roots/idle/nonPlay。
机器另有两个Unity进程；本任务只连已核原项目6401，不读未脱敏命令行、不启动/终止任何Editor。

## 准确代码范围

仅编辑 Assets/NTSD/Scripts/Animation/Rendering/Editor/BattleMeshUploadBaselineEditorTests.cs，保留原12项无诊断基线与schema。
新增8组阶段基线：1000 Ordered/Strict/交错variant，各同/位置变化；4097 Ordered同/变化。
复用现有BattlePresentationPhaseDiagnostics与BattleTickDetailPhaseDiagnostics，不修改任何生产埋点/算法。
每组64预备/1800采样；无诊断12组同轮对照，标明不同case非配对/同屏/生产A/B。
BeginTick/CompleteTick只是fixture recorder样本标识，不推进World或逻辑tick，不写生产counter。
记录coarse ResolveAndWrite/Resolve/Write/Upload，以及detail SetVertexBufferData/SetSubMeshes。
父阶段包含子阶段，不直接相加为总时间；SetSubMeshes包括descriptor遍历/生成和native API，不等于纯API。
记录每sample Build外层时间、phase合计和percentile、残余；标明逐命令timestamp/marker观测税。
断言计时开启不改变真实API payload/物理segment/Mesh身份、prepared current-thread allocation0B/growth0；
断言完成样本sequence与非零phase；不对耗时设脆弱阈值。
使用原synthetic resolver/null Texture/Material，既有两帧提前创建；JSON和断言/反射在窗口外。
本地using Dispose，不接入Runtime owner/queue/resource/lease，无新增关闭阶段。

## 不可变边界与验证

33ms/3ms/Host max2、checksum/pass/RNG/slot/generation/publication/排序/UV/segment/fail-closed/11阶段保持。
A1 dirty-chunk跳过仍未来设计，不改顶点上传；不读Q06活跃方法体，不改PERF/ATLAS/EXT1/MONO正文。
无Scene/InputAction/资源/Settings/Server修改；EXT1 PROPOSED/MODIFY_REQUIRED、MONO USER_HOLD、ATLAS专项门保持。
仅原Editor非Play编译、具名EditMode；Runner临时untitled scene不是原生产Menu/Battle采样。
不主动切Scene/Play/Profiler/FrameDebugger/GPUcapture，不运行1000AI/完整M0/Android。
运行相关submesh/bounds/upload计数旧回归；Validator/diff/links/保护哈希。
父M03仍OPEN/RUNTIME_PENDING，只有本fixture限定通过可VERIFIED；没有生产性能收益/设备证书。
下一依据现有native元数据成本再选择最小A/B；不同表示/合并/银行格式不是本批授权。

## 文件操作与恢复

八个现有dirty文件（七治理文档+fixture）已准确SHA备份，358范围外文件含前十一批证据/生产/Q06 hash-only。
[Operation](../FILE-OPERATIONS/NTSD-OPTIMIZATION-BATCH12-MESH-PHASE-BASELINE-20261006/RECORD.md)；[报告](../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH12-MESH-PHASE-BASELINE-20261006/REPORT.md)。
只apply_patch追加/精确编辑；不删除/移动/Git丢弃/提交/push；回滚需另授权，不能用HEAD恢复dirty。

实际8阶段+12控制=20/20，旧submesh3/bounds5/upload18/detail8=34/34，去重54/54 Passed。
36000有效Build/1280warmup；prepared Build+recorder reset/完成/读scalar窗口当前线程0B/growth0，非全链。
六phase86400有效样本；原Editor编译CS0、生产源码/DLL保持；原Menu前后clean/8roots/nonPlay。
高分段descriptor/native阶段mean1.14～1.20ms是下一最小A/B目标，不称纯API或移动收益。
无诊断控制与instrumented为顺序非配对case，计时差不是优化收益；没有生产publication/资源/AI/GPU样本。
本fixture非bugfix，没有伪造RED；输出截断/session空输出/reload暂不可用/rg路径与补文档patch失败见Operation。
