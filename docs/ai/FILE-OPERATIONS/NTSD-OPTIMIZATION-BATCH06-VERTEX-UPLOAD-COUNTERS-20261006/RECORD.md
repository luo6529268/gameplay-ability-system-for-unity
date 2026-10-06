# 第六批中央 Mesh 顶点上传计数操作记录

当前：VERIFIED（限定文件操作与留痕）；Task/Change RUNTIME_PENDING，父M-03开放。
下面PLANNED为事前快照，不当作最新状态。

Operation NTSD-OPTIMIZATION-BATCH06-VERTEX-UPLOAD-COUNTERS-20261006 / PLANNED；Task IN_PROGRESS；Change NTSD-OPT-M03-VERTEX-UPLOAD-COUNTERS-006 / PLANNED。
执行者 Codex /root，工作目录 I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity。
用户授权：本轮“开始执行下一批的任务”，沿已批准优化文档 M-03 基础诊断缺口推进。
准确九个已有文件的绝对路径/字节/SHA/Git状态见before.json，备份为before-backups镜像。
先记录、登记索引，再Copy-Item -LiteralPath到不存在的备份目的地并核SHA；禁止覆盖备份。
已有dirty全部保留；新增Task/Change/test/.meta/rawJSON/报告/after只创建本批具名文件。

生产范围：BattleCentralBuildDiagnostics新增本次Build的API顶点上传次数、顶点数、字节数；
BattleDynamicMeshBackend.Build/Upload在SetVertexBufferData成功返回后计数，
CreateMesh在创建时缓存真实stream0 stride；不改变调用次数、顶点布局、segment/排序/容量/失败策略。
本批不是dirty-chunk优化、EXT-1专项M0或GPU batch/带宽测量；不改PERF/ATLAS正文。
新统计无资源owner，依附既有backend/diagnostics/chunk，Clear按原入口reset，
Dispose及十一阶段关闭不变；不增加worker/queue/GPU buffer/lease或释放协议。

拟执行：apply_patch最小编辑；现有Editor PID19040/TCP6401 Menu clean非Play，
refresh_unity(mode force, scope all, compile request, wait false)及EditMode run_tests/get_test_job；
先RED确认缺失指标，再生产修改/GREEN/旧132相关聚焦回归。
只测fixture Build/Upload局部managed分配，不启动Play、完整M0、Profiler或设备测量。
Validate-ChangeLedger.ps1、git diff --check、保护/备份SHA、限定文档引用核对、准确after。
保护范围见protected-before.json；不读Q06活跃方法体，不更改Scene/Prefab/资源/Settings/InputActions/
33ms/3ms/2interval/checksum/ATLAS/MONO/EXT-1/Server，EXT-1保持PROPOSED / MODIFY_REQUIRED。
无删除/移动/Git写入；恢复必须另获批准，仅逆向本批hunk，不能从HEAD覆盖现有dirty。

## 实际执行与范围验收

2026-10-06T20:32:13+08建立before；20:33:57完成九个不存在目的地的精确dirty备份并核SHA。
索引本批新增行曾局部撤去以核回before SHA，再Copy-Item/核SHA后恢复该行；
只操作本批新行，未撤销任何既有内容或使用Git恢复。原始Task/Change在脚本前建立。
实际apply_patch：新18项fixture/.meta；两个生产文件15插入行；九existing具名范围内文档/
Ledger/STATE/handoff/索引与新Task/Record/rawJSON/报告。没有额外脚本/资源/Scene范围。

现有Unity Editor PID19040/TCP6401：refresh all/compile request/wait false；EditMode run_tests，
get_test_job。第一次RED请求时domain reload返回retry，未启动测试；原件保存后正常重试。
RED55dbdae760a44456833c5c515e2f873d完成18预期失败，生产SHA仍before；
GREEN14486f1f5037435fa49389e8dd3be893新18/18；
REGRESSION47356cdbfea543a3b760504767ebfe91旧132/132，去重150项。
回归第一次shell输出限额截断，随后只读获取完整JSON，未覆盖原结果或重跑。
生产DLL20:38:00/Editor20:38:05；post Menu clean/idle/nonPlay/CS error0。
fixture DestroyImmediate只释放本批临时内存Mesh，不删文件或用户资源。

20:43:10 Validate-ChangeLedger.ps1 exit0/PASSED1302 Records/14 code files；
4238既有warning/0error。20:43:49 diff --check exit0（仅LF/CRLF提示），34项高12中14低8；
111个本批限定链接无缺失，127前五批/保护/其他文件及九backup SHA均保持。
新原始证据及本报告目录只创建本批文件；准确最终字节/清单见after.json（不包含自身递归hash）。
不改33ms/3ms/2interval/checksum/pass/RNG/geometry/segment/fail-closed/slot/lease/GPUconsumer/
11阶段关闭，不读Q06body；不启Play/完整M0/Profiler/GPU/Player/设备，局部GC不晋升父项。
没有PERF/ATLAS/MONO/EXT-1正文/Scene/Prefab/资源/Settings/InputActions/Server修改。
没有Git add/commit/push/reset/checkout/clean/stash、文件删除/移动或memory写入。
文件操作VERIFIED不表示上传已减少或Android达标；恢复另获授权，仅逆向本计数hunk与新fixture。

20:45:51最终复核116限定链接无缺失；保护127/备份九/编译版三源SHA保持，
PERF/ATLAS/MONO/AGENTS/CURRENT-AUTHORITY Git状态clean。新增全文件空白扫描的7条
都在三治理文档精确dirty备份中逐条同值存在，无本批新增尾空白，不清理任务外历史。

