# 第九批 M-03 Build 与上传分类归属 Task Contract

Task NTSD-OPTIMIZATION-BATCH09-BUILD-UPLOAD-ATTRIBUTION-20261006 / RUNTIME_PENDING；Change NTSD-OPT-M03-BUILD-UPLOAD-ATTRIBUTION-009 / RUNTIME_PENDING。
用户授权：本轮“开始执行下一批的任务”，按M-03把已完成request分类与实际Build/API诊断关联。
本批不改上传算法、不启动完整M0/EXT-1专项M0；生产报告导出另批，公开diagnostics为本批读出口。

## 原状与最小设计

backend.Build在disposed/null resolver/容量准入拒绝前不递增MutationVersion且不reset诊断；
进入Build后MutationVersion递增一次，diagnostics只统计已成功返回的vertex上传API。
Central geometry catch会Clear该诊断。因此必须在Build wrapper finally、外层Clear之前冻结累计，
不能以Prepare attempt或旧diagnostics推导这次上传，不因失败已完成API而提交partial pixels。
本次重扫backend:Build/MutationVersion/Reset/chunk.Upload及中央staging Build/catch；不读Q06 sorter body。

准确生产路径：Assets/NTSD/Scripts/Animation/Rendering/BattleCentralRenderSystem.cs。
符号：BattleCentralRuntimeDiagnostics；新增BattleCentralMaterializationBuildCounters、
RecordBackendBuild、BuildMeshForMaterialization；物化核心PrepareFrameForMaterialization及原PrepareFrameImmediate。
保留原两参PrepareFrameImmediate作为直接/self-check入口，委托同核心且kind=None，避免改变已有反射测试合同；
queued生产请求将已分类kind作为局部参数传入核心，不用可被reentrant request覆盖的全局current-kind。
唯一stagingBackend.Build调用用wrapper，backend算法/签名/已完成API计数实现不改。
测试：Assets/NTSD/Scripts/Animation/Rendering/Editor/BattleMaterializationBuildAttributionEditorTests.cs/.meta。

每组8个long：BuildInvocationCount、AdmittedBuildCount、CompletedBuildCount、
RejectedBeforeBuildCount、FailedBuildCount、VertexUploadCallCount、UploadedVertexCount、UploadedVertexBytes。
总组及publication/alpha/repeated三组分别累计；None/直接诊断/benchmark不纳入生产分类。
invocation=completed+failed+rejected；admitted=completed+failed。empty/null/unresolved正常Build是completed0upload，
完成Build不证明有效command、pixel submission或GPU。admission拒绝不重复旧API诊断，entered失败仅累计此前完成API。

## 生命周期与边界

四个计数组对象随现有RuntimeDiagnostics启动创建并复用，无热路径new/容器/扩容/Unity资源/lease。
新增只读getter出口，无外部setter；ResetMaterializationCounters/ResetRuntime清总组及三分类；
per-frame reset不清，原shutdown阶段5保持，无新增关闭阶段。诊断额外CPU成本未测。
新增wrapper在原Build call处执行，异常仍抛给原catch，不更改fail-closed或资源回收。
33ms/3ms/max2interval、pass/RNG/checksum/publication、alpha时钟/排序/UV/segment/slot/lease/GPUconsumer不改。
不读取或修改Q06活跃方法体；不改PERF/ATLAS/MONO/EXT-1正文、bank/预算/资源格式、Scene/Settings/InputActions/Server。
EXT-1 PROPOSED / MODIFY_REQUIRED；MONO USER_HOLD；未授权专项不因本批解冻。

## 验证与恢复

原Editor PID19040/TCP6401 idle Menu root8/clean/非Play/CS0/URP14，复用原Editor具名EditMode RED→GREEN；
不切Scene/Play，不执行真实Profiler/GPU/设备测量。八个dirty备份及250保护范围见Operation。
聚焦：三分类总数/真实Mesh stride/跨chunk/empty/null/unresolved/Strict模式/同帧重复Build；
sealed/null resolver/disposed准入前拒绝保留旧诊断而分类不重复；
首/第二chunk上传异常仅已完成API，finally冻结后Clear不丢计数，Resolve异常0upload；
forced cached-plan Prepare attempt而Build为0、None/Legacy排除；
累计per-frame保持/Reset清零、long大值及预备后scalar helper/完整wrapper+Build局部managed0B。
既有202限定回归、实际compile/CS0/Menu clean、Ledger/diff/链接/source/backup/protected SHA及准确after。
父M-03 OPEN；生产报告/完整链0GC/动态Play alpha/当前同布局基线/实际收益/1000AI/Android待验收。
回滚另授权，仅从dirty before逆向本批hunk；保留前八批和用户内容，不从HEAD覆盖。

[Operation](../FILE-OPERATIONS/NTSD-OPTIMIZATION-BATCH09-BUILD-UPLOAD-ATTRIBUTION-20261006/RECORD.md)；
[报告](../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH09-BUILD-UPLOAD-ATTRIBUTION-20261006/REPORT.md)；
[进度](../../../Assets/NTSD/Docs/battle-optimization-progress-tracker.md)。

## 实际执行结果（原事前验收清单保留）

原Editor实际compile；有效RED23项missing wrapper/group/helper后，GREEN23/23、相关旧回归202/202，
共225去重case，0失败/跳过。每次测试只选具名相关项，不执行全量benchmark/真实Profiler。
聚焦边界：三类真实Build/API累计、sealed/null resolver/disposed准入前拒绝、首/第二chunk异常已完成API、
empty/null/unresolved正常Build0payload、None/Legacy/force cached-plan排除与Reset均通过。
scalar helper预备64/采样512局部managed0B；两模式各预备后144次wrapper+Build/96API/3456顶点局部managed0B。
不代表完整物化/Foot/Health/录制/提交/RenderPass/GPU 0GC、像素成功或帧率收益。
唯一生产文件相对准确dirty before125插入/2删除；新fixture/meta。后续生产报告接线另批，
动态alpha比例/同布局生产基线/完整链0GC/dirty设计/Play/1000AI/Android仍待验收，父M-03 OPEN。
文件/账本/链接等最终验证结果见报告及Operation after，不因子批PASS撤销原专项门。
