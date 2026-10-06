# 第十批 M-03 生产计数报告

状态RUNTIME_PENDING / PRODUCTION_COUNTER_REPORT_FOCUSED_PASS；新58/58、旧225/225，父M-03仍OPEN。
[Task](../../../docs/ai/TASKS/NTSD-OPTIMIZATION-BATCH10-MATERIALIZATION-REPORT-20261006.md)；[Change](../../../docs/ai/CHANGE-RECORDS/NTSD-OPT-M03-MATERIALIZATION-REPORT-010.md)。
只显式非热路径快照/JSON及同来源/同epoch窗口，不改原上传/渲染/模拟。

## 产物与用途

BattleCentralRuntimeDiagnostics新增MaterializationCounterEpoch、内部source token、CaptureMaterializationReport；
既有central只9行新增，新增BattleCentralMaterializationReport 199行/9048bytes，fixture309行。
报告固定冻结43个long：request4、Prepare attempt4、原return gate3、四组Build/API各8。
三组为publicationChanged/alphaChanged/repeated，另有total；Build调用/准入/完成/拒绝/失败及成功API
上传次数/顶点数/字节数分开，沿用第九批口径，不把completed当成功pixels或GPU draw。

累计scope=cumulative-since-reset；窗口scope=window-delta，记录start/end原counter供复算。
baseline缺失、不同source、Reset epoch变化、窗口充当累计端点、任一43counter倒退，
整份窗口拒绝，out=null/常量reason，不clamp、不提交partial报告。
全部long/单位正确，不把真实counter0当“没有观测即性能通过”；无verdict/FPS/GPU证书字段。
epoch只Reset递增，token随现有diagnostics启动创建且不含World；report仅token+long数组，
不持有World/backend/material/纹理/lease，窗口仅共享私有不可变endpoint数组。
每次projection返回新的字典；修改导出字典不改变冻结report。

## 明确调用边界

公开读出口：BattleCentralRenderSystem.RuntimeDiagnostics.CaptureMaterializationReport()。
显式主线程、writer不并发条件下，先Capture baseline，经过已批准窗口后Capture end；
end.TryCreateWindow(baseline,out window,out reason)，成功后window.ToJson()。
也可单份累计ToJson；复用BattleCanonicalJson，无新JSON依赖，不改benchmark v5或mandatory指标。
Capture/字典/JSON有分配，只非热路径显式调用，不自动接Update、RenderPass或worker；
不新增Mono/采样器/菜单/文件writer，不在本轮开启采样窗口或写出实际生产性能report。
该schema不是完整M0，也不提供wall-clock/帧/tick/pub数分母；来源/布局/设备/时间门仍需具名。
导出不是0GC热路径；诊断开销未知，无上传减少/FPS/Android收益结论。

shutdown原阶段5Reset仍清累计且变epoch；旧纯managed报告可留证，不消费已关闭资源；
无新owner/queue/Unity资源/lease或关闭阶段。33ms/3ms/Hostmax2/checksum/pass/RNG/publication/
alpha/排序/UV/segment/fail-closed/slot/GPUconsumer及11阶段不变。
Q06只文件状态/哈希，不读活跃方法体；EXT-1 PROPOSED/MODIFY_REQUIRED、MONO USER_HOLD、
PERF/ATLAS正文、bank/预算/格式、Scene/Settings/InputActions/Server不变。

## test-first和实际编译

fixture22:13:05→原EditorDLL22:13:30，CS0；生产仍第九批FAAF0C...913C。
RED e810c8d7b63646e5abd0889b4c60e03e完成58，status failed/result null；
failure列表25条capped均missing CaptureMaterializationReport，原件保留，不伪造58失败summary。
实现后生产22:15:04→DLL22:15:24，EditorDLL22:15:27，CS0。
新GREEN 3cd51629c2834313a9433fccfd494a03实际58/58，0失败/跳过。
覆盖冻结大于Int32 long/单位/JSON、三类成功/拒绝/失败payload、全部43倒退拒绝、
Reset catch-up也拒绝、来源/空baseline/非累计端点、窗口复算、字典隔离、只读出口。
原aggregation+新增epoch Reset预备64/采样512当前线程managed0B，非Capture/JSON或完整链。
相关旧225已启动030b0ed491c445d7a5b75457e587ceef，结果及最终静态验收后追加。

回归最终225/225，0失败/跳过；与新58共283 fullName唯一且Passed。
实际原件：[GREEN](green-result.json)、[旧回归](regression-result.json)、[选择](regression-targets.json)、
[编译身份](compiled-source-manifest.json)，不以全树9219当本批分母。
完整BattleRuntimeSelfCheck/Play/设备未运行，不把这些EditMode结果升级为运行时验收。

## 范围与剩余门

原Editor19040/6401，Unity2022.3.62f3/URP14，Menu8roots/clean/非Play/CS0；
只refresh/compile/具名EditMode，未开Play/第二Editor/Profiler/GPU/完整M0/1000AI/Android。
八准确before备份22:12:05核SHA；286保护22:16:57重核0差异，HEAD2cccd597保留。
所有文本apply_patch；无删除/移动/清理、Git add/commit/push/reset/checkout/clean/stash。
父M-03真实动态生产baseline、完整物化—上传—录制—提交0GC、dirty优化、
像素/透明顺序/退出重进及1000AI/Android仍开放。
技能仅用于原Editor/pipeline/表现边界验证，不采用通用timestep/Camera/HDR/importer改动。

下一按共同启动门确定原Battle平台/API/workload/运行窗口和允许的采样范围，再采真实生产基线；
本批完成可调用读出口，不把benchmark-local冻结样本冒充生产动态表现。

## 最终静态与留痕

[账本](change-ledger-validation.json)：exit0，1306Records/3governed；
4275全库warnings，本Change及两新路径0warning、0error；未扩大清理，非无警告声称。
[原Editor](editor-post.json)：Menu8roots/clean/idle/非Play，CSerror0。
[初次静态](static-initial.json)保留4处本批追加EOF空行的diffcheck exit2；
只去掉这些新空行后[静态验证](static-validation.json)exit0，133selected links0missing，
34项（高12/中14/低8），new source/meta无尾随空白，全部编译source SHA保持。
准确before备份/最终after见Operation；最后after落盘后仅只读核对，不再改源码/证据。
