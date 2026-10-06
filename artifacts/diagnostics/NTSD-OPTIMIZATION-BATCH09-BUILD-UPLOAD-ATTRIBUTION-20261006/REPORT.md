# 第九批 M-03 Build/API 分类报告

状态 RUNTIME_PENDING / BUILD_ATTRIBUTION_FOCUSED_PASS；按三类有效queued生产request关联Build admission与已完成vertex API。
[Task](../../../docs/ai/TASKS/NTSD-OPTIMIZATION-BATCH09-BUILD-UPLOAD-ATTRIBUTION-20261006.md)；[Change](../../../docs/ai/CHANGE-RECORDS/NTSD-OPT-M03-BUILD-UPLOAD-ATTRIBUTION-009.md)。
用户“开始执行下一批的任务”授权普通M-03诊断小批；父M-03 OPEN，生产report导出/真实基线/
完整链0GC/1000AI/Android开放。本批不是上传减少、120FPS或Android证书。

## 产物与正确口径

唯一生产文件BattleCentralRenderSystem，准确dirty before之上125插入/2删除。
新BattleMaterializationBuildAttributionEditorTests.cs/meta；其余是8份dirty备份、Task/Change/留痕与证据。
总MaterializationBuilds及PublicationChangedBuilds/AlphaChangedBuilds/RepeatedSampleBuilds，
每组8个只读long，累计BuildInvocation/Admitted/Completed/RejectedBeforeBuild/Failed及
VertexUploadCall/UploadedVertex/Bytes。kind从queued request局部传参，旧两参Prepare反射入口保持None。
准入前disposed/null resolver/sealed拒绝保留backend旧diagnostics，本次不重复计payload；
wrapper finally在外catch Clear之前冻结进入后失败已完成API，异常仍交原catch，不能partial submit。
empty/null/unresolved正常Build属于completed0payload，不等于pixel submission成功；forced缓存没有Build。
总组等于三个类别组之和；None/直接诊断/benchmark-local不算生产分组，per-frame不清，ResetRuntime复用清零。
仅中央实体Mesh成功返回SetVertexBufferData的CPU API payload，实际stream0 stride，不是GPU带宽或batch。
排除Prepare/索引/submesh/Foot/Health/录制提交/RenderPass/GPU，没改变backend Build签名/算法/上传次数。

本次重扫：CentralRuntimeDiagnostics与四组21-209行；queued局部传参648；旧Prepare/core672-685；
真实staging wrapper905-913、原异常Clear/catch931-941；wrapper1024-1051、ResetRuntime1917。
backend准入拒绝与MutationVersion156-179；成功API后计数717-727。
Q06仅状态/哈希，不读BattlePresentationShadowBuild活跃方法体。未改alpha取样/排序/UV/segment/
slot/lease/GPUconsumer、33ms/3ms/Hostmax2interval、模拟/pass/checksum/RNG及11阶段有序关闭。

## 实际编译与test-first

原Unity Editor PID19040/TCP6401，Unity2022.3.62f3/URP14，Menu8roots/clean/非Play。
先写reflection fixture再compile：测试DLL21:56:38晚于fixture21:56:09，CS0，生产仍before SHA。
有效RED4a40f63dbb364b9dad27c2d3ca35a359完成23项missing wrapper/group/helper失败，
bridge result=null，保留真实progress.completed=23与23条failures，不伪造summary或先实现后称RED。
GREEN前生产source21:58:10→DLL21:58:18；fixture21:56:09→EditorDLL21:58:21，CS0。
实际compiled-source-manifest.json与green-compile.json保存身份，非隔离编译替代原Editor。

| 运行 | 实际结果 | 原始证据 |
|---|---|---|
| 新fixture GREEN d3662f9a12fb46a29e9fc518e15ee2e8 | 23/23，0失败/跳过 | [GREEN](green-result.json) |
| 具名旧回归0ed03b3d6bf944488132c9699f071e97 | 202/202，0失败/跳过 | [回归](regression-result.json)、[选择](regression-targets.json) |
| 去重合计 | 225具名case通过 | 两个真实summary，不用bridge全树9161当分母 |
| 新scalar helper | 64预备后512次，当前线程managed0B | GREEN PreparedScalarAggregation |
| wrapper+Build/upload | Ordered/Strict各预备后144次Build、96API、3456顶点，当前线程managed0B | GREEN WarmedAttributionWrapperAndBuild两case |

覆盖三类真实Build/实际stride、空/null/unresolved、跨chunk/Strict、重复Build仍上传、
准入拒绝旧值隔离、首/第二chunk异常完成API冻结、Resolve异常、long大值、
per-frame/Reset复用、None/Legacy/force缓存不Build。
GC窗口排除反射/delegate绑定/创建/断言，只有预备夹具标量或wrapper+实体Mesh Build/upload，
不是完整物化—上传—录制—提交、辅助Mesh或benchmark CaptureFrame/report 0GC。

## 安全与验证范围

21:54:27八份当前dirty备份SHA一致，先建Task/Change/Operation/Ledger/STATE/handoff才写脚本。
22:01:40再次核实250保护/8备份0差异，HEAD5a5cde34b9685739326b638f9c5550b69eda7ef6保持。
一次manifest验证命令误用protected/backup属性，产生空Path错误，未写文件；
改用protected-before.files[].expected与before.files[].backupPath完成有效验证，不能称真实漂移。
22:04原Editor idle/非Play/非compile/Menu clean8roots，CS error0。
Tools/Validate-ChangeLedger.ps1 exit0，1305Records/18governed code files；
4237既有warnings，0errors，不在本批扩大清理。diff/链接/完整after的最终校验见后追加。

未开Play/第二Editor/真实Profiler/完整M0/FrameDebugger/GPU capture/1000AI/Android；
不修改Scene/Prefab/资源/ProjectSettings/InputActions/Server、PERF/ATLAS/MONO/EXT-1正文和专项门。
EXT-1仍PROPOSED / MODIFY_REQUIRED；MONO USER_HOLD；ATLAS bank/预算/格式及fail-closed不改。
无Git add/commit/push/reset/checkout/clean/stash或文件删除/移动，未覆盖任何准确备份。
技能只用于原Editor编译验证、像素渲染边界和pipeline核对，不应用通用固定步长/相机/HDR/importer建议。

## 剩余门与下一安全批

下一只把当前公开diagnostics归属数据接入明确生命周期的生产报告，区分累计/窗口/拒绝，
不得将冻结benchmark-local报告冒充生产publication动态分布。
后续真实同布局生产基线、完整链0GC、dirty区设计和像素/透明排序/退出重进、1000AI/Android待验收。
诊断自身CPU/native开销未知；本批通过不给未获批A4/资源格式或性能fast path默认授权。

## 收尾静态验证

[账本](change-ledger-validation.json)、[原Editor](editor-post.json)、[静态验证](static-validation.json)。
34项（高12/中14/低8），父项关闭0；127处选定文档本地链接0missing。
git diff --check exit0；新fixture/meta无尾随空白；16条LF/CRLF提示不作代码错误。
准确dirty before恢复链与最终after见Operation，不以旧HEAD作为恢复来源。
源码SHA与实际GREEN编译manifest一致；225个fullName唯一且均Passed。
最后after落盘后仅只读哈希/diff核对，不再修改本批源码或证据；最终快照不含其自身。
