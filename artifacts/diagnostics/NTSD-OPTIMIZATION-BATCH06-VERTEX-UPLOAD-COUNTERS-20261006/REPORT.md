# 第六批 M-03 每 Build 顶点上传计数报告

Task NTSD-OPTIMIZATION-BATCH06-VERTEX-UPLOAD-COUNTERS-20261006 / RUNTIME_PENDING；Change NTSD-OPT-M03-VERTEX-UPLOAD-COUNTERS-006 / RUNTIME_PENDING（聚焦通过，父项开放）。
本批只增加CPU Mesh API顶点payload计数，不改变上传算法、segment、绘制、GPU或模拟。
test-first新18项已写，两个生产脚本尚未修改，原Editor已请求refresh/compile，RED待回收。
原Editor PID19040/TCP6401 Unity2022.3.62f3 URP，Menu clean非Play/idle/CS error0事前已观察。
九existing精确dirty备份匹配；保护清单保存。本报告只按实际结果追加，不提前宣称PASS。
父M-03/完整0GC/性能收益/完整M0/1000AI/设备均待验收；EXT-1/MONO/ATLAS专项门保持。

## 已实际完成的验证

| 证据 | 实际结果 | 边界 |
|---|---|---|
| RED job55dbdae760a44456833c5c515e2f873d | 完成18，全部在缺失上传计数属性处预期失败 | 生产仍before SHA；GC断言未到达，非原状GC证据 |
| 实际编译 | 生产DLL20:38:00、Editor DLL20:38:05，晚于源文件20:37:50；idle/CS error0 | 原Editor PID19040/TCP6401，未另开实例 |
| GREEN job14486f1f5037435fa49389e8dd3be893 | 新18/18，0失败/跳过 | 直接实际Mesh API，但无场景绘制/GPU捕获 |
| REGRESSION job47356cdbfea543a3b760504767ebfe91 | 旧132/132，0失败/跳过 | 前五批/解析/mesh/物化/插值/辅助/共用绑定；去重共150 |
| 局部GC | Ordered/Strict各预热后48 Build/Upload，current-thread managed0B | 只fixture路径，无materializer/录制/RenderPass/辅助/GPU/完整Battle |

新18项：1/17/4096/4097活动前缀；empty/null；Ordered/Strict段数不同但upload相同；
完全重复Build重新计数而不累加；缩小/Clear不计inert metadata；准备/native恢复不计索引；
全缺失/部分缺失按resolved payload；seal超限准入前拒绝保持旧诊断；
第一个/第二个chunk非有限数据异常只计先前成功API；两模式预热循环局部0B。
failure计数是已完成API的CPU工作量，不授权partial submission，不改变原异常/central fail-closed合同。

## 实施内容与统计口径

生产exact diff对本批dirty备份：两个文件净新增15行。
BattleCentralBuildDiagnostics：VertexUploadCallCount(int)、UploadedVertexCount(long)、
UploadedVertexBytes(long)及Reset；BattleMeshChunk.Upload成功SetVertexBufferData返回后累加；
CreateMesh在SetVertexBufferParams后缓存真实stream0 stride，每次原生恢复亦重新取值。
没有每upload额外native stride查询、没有新容器/资源owner/worker/queue。
保留原顶点布局、UV/tint/slice、命令顺序、segment、插值/first-visible、slot/lease/11阶段关闭。
未读Q06方法体，未改模拟pass/RNG/checksum/33ms/3ms/2interval。

每获准进入Build单独reset，Clear亦reset；disposed/null resolver/容量准入前拒绝不覆盖旧诊断。
重复Build仍上传活动chunk，计数并不声称dirty-chunk skip；A1仍待实施。
bytes是Mesh API调用的顶点数×真实stride，非预留capacity、CPU vertices写入次数或GPU实际传输。
不包括index buffer、submesh metadata、Foot/Health、RenderPass、DrawMesh或GPU batch。
新属性已可从既有diagnostics读取；尚未接benchmark/报告导出或publication/alpha分类。
未来分组/基线先证明热点和像素/排序合同，再实施缓存，不跳到未经批准A4/MeshData输出决策。

## 原件及未运行项

raw JSON：editor-preflight/refresh-red/red-start/red-result/refresh-green/compile-state/
compiled-artifacts/green-start/green-result/regression-result；initial-reload-retry保存
第一次domain reload中的retry答复（未启动测试），实际RED是重试后上述具名job。
回归输出第一次shell预算不足截断，后续重新只读获取完整result；结果来自完整summary132，
不是progress.total9104（全库发现数）。未因截断重新跑测试或修改任何已有结果。
九原件备份、127保护清单见Operation目录；最终静态/after另追加。

未运行Play/完整SelfCheck/完整M0/Profiler/Frame Debugger/GPU capture/Player/Android。
不把synthetic4097渲染命令解释成1000AI压测、logic parity/0GC/设备性能通过。
本批无性能收益承诺。EXT-1=PROPOSED / MODIFY_REQUIRED，无专项M0/instancing；
PERF/ATLAS/MONO正文、bank/预算/格式、Scene/Prefab/资源/Settings/InputActions/Server未修改。
Pixel/Unity技能用于确认既有URP/原Editor的安全验证边界，不套用通用fixedDeltaTime或改变采样设置。

## 静态交付收尾

20:43:10 Ledger validator exit0/PASSED1302 Records/14 governed code files，
4238既有warning/0error（与上一批warning数一致，未清理历史）。
20:43:49 git diff --check exit0，仅LF/CRLF提示；34项=高12/中14/低8，父项关闭0。
111个限定链接无缺失；127前五批/保护/其它文件与九个精确dirty备份SHA均保持。
post原Menu Scene clean/idle/非Play/CS error0，无第二Editor、无Scene切换/保存。
真实生产改变只15插入行及新fixture，无Git写入/删除/移动/资源格式选择；
Task/Change RUNTIME_PENDING，Operation仅限定文件操作VERIFIED。
完整原件/after见[Operation](../../../docs/ai/FILE-OPERATIONS/NTSD-OPTIMIZATION-BATCH06-VERTEX-UPLOAD-COUNTERS-20261006/RECORD.md)；
[Task](../../../docs/ai/TASKS/NTSD-OPTIMIZATION-BATCH06-VERTEX-UPLOAD-COUNTERS-20261006.md) /
[Change](../../../docs/ai/CHANGE-RECORDS/NTSD-OPT-M03-VERTEX-UPLOAD-COUNTERS-006.md) /
[总进度](../../../Assets/NTSD/Docs/battle-optimization-progress-tracker.md)。

20:45:51最终静态复核：116限定链接无缺失、127保护/九备份保持、三脚本SHA与
20:38实际编译/测试版本一致；PERF/ATLAS/MONO/AGENTS/CURRENT-AUTHORITY Git状态均clean。
额外全文件空白扫描在三个大型治理文档发现7条既有尾部空格；逐一对精确before-backups
排序对比7条内容完全相同，新脚本与本批新增内容无此问题，不修改用户历史文档空白。
static-final/whitespace-baseline原件保留；既有白字符不是本批diff --check错误。



