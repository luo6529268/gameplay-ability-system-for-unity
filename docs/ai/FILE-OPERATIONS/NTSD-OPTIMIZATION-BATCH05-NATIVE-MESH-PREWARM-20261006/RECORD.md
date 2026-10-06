# 第五批原生Mesh重新预热操作记录

当前状态：VERIFIED（仅文件操作及留痕）；Task/Change RUNTIME_PENDING，父H-11开放。
下方PLANNED是事前快照，保留授权、范围和原始计划，不能当最新实施状态。

Operation NTSD-OPTIMIZATION-BATCH05-NATIVE-MESH-PREWARM-20261006 / PLANNED；Task IN_PROGRESS / Change NTSD-OPT-H11-NATIVE-MESH-PREWARM-005 PLANNED。
用户授权：本轮“开始执行下一批的任务”，沿已批准优化文档与统一进度表，限定H-11缓存预热。
执行者Codex /root；工作目录 I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity。
精确八个existing路径/SHA/字节/Git状态见before.json（含此前dirty），备份镜像before-backups。
先建立本Record/before，再Copy-Item -LiteralPath到此前不存在的镜像位置并核SHA；
之后索引/Task/Change，之后测试脚本。禁止覆盖备份；恢复另获批准只逆向本批hunk。

生产范围仅BattleDynamicMeshBackend.PrepareCapacity在原EnsureChunk后预先访问该chunk的Mesh，
将既有EnsureMesh恢复提前至seal前；不增加owner/缓存，不改变原EnsureMesh战斗突发丢失恢复策略。
新增BattleNativeMeshPrewarmEditorTests.cs/.meta、Task/Change/原件JSON/报告/after均create-only。
实际已有文件修改仅before.json八路径，均apply_patch最小补丁；无删除/移动/Git丢弃/外部部署。

拟执行test-first：新增毁损native Mesh夹具，原Editor PID19040/TCP6401，Menu clean非Play；
refresh_unity scope all / compile request / wait false、run_tests EditMode、get_test_job。
RED确认生产仍before SHA，随后PrepareCapacity一处预热，实际compile/GREEN及原120项相关回归。
GC断言只覆盖已预热后的mesh Build/Upload，不能宣称完整central0GC、GPU完成或帧率收益。
最后Validate-ChangeLedger.ps1、diff --check、保护/前序/备份SHA、准确after；保存实际命令结果。
不读取Q06方法体；不改Scene/资源/Settings/InputActions/33ms/3ms/2interval/checksum/排序/UV/
lease/11阶段关闭/PERF/ATLAS/MONO/EXT-1/Server；EXT-1保持PROPOSED / MODIFY_REQUIRED且无专项M0。
真正Battle退出重进、无Domain Reload重进、完整SelfCheck/0GC、native/GPU预算/1000AI/Android仍待验收。

## 实际执行与限定收尾

20:10:19 +08建立before，20:10:51完成8个此前不存在备份并核SHA；原有dirty完整保留。
99个前四批/保护/其他文件在事前、中途及20:21:24独立复核无mismatch；8备份亦一致。
实际apply_patch：新12项测试/.meta；唯一生产PrepareCapacity三插入行；
八existing准确范围内优化/治理文档、Task/Change/本Record/索引及新rawJSON/报告留痕。
Copy-Item只创建原件镜像，不覆盖；不删除或移动任何文件；DestroyImmediate仅fixture临时内存Mesh。

原Editor PID19040/TCP6401，refresh all/compile request/wait false后EditMode run_tests/get_test_job：
RED ee3e05dddd9e4c148e75d35074b652d7，12执行/10预期失败/2控制PASS，生产原状SHA保持；
GREEN 8a13f03c355d4d76b68db49d665ae954，新12/12；
REGRESSION d40f11c3b4454c3c9b134fff59cbdeb8，旧120/120，去重132。
最终生产DLL20:15:22、新测试DLL20:13:15，post Menu clean/idle/非Play/CS error0。
首次编译后说明句方向更正，仅注释，再refresh确认当前源时间后的DLL；没有误说明版测试。
未启动第二Editor/Scene/Play/完整SelfCheck/M0/GPU/Native/Android测量；25 Build/组仅局部managed GC。

20:20:17 Validate-ChangeLedger.ps1 exit0/PASSED，1301 Records/13 governed code files，
4238既有warning/0error；20:21:07 git diff --check exit0（LF/CRLF提示不当error），34项高12中14低8。
107个限定优化/Task/Change/本批文档链接当次全部可达。
首次static-validation.json triplet数组被PS foreach展平，导致两汇总false而各项均true；
未据此更改文件。20:21:24独立对象逐文件核实99/8无差异，static-validation-v2.json追加更正，原件保留。
没有因为该诊断包装错误执行恢复/清理/覆盖；不改变测试或生产结论。

实际入口：apply_patch、Copy-Item到不存在备份、Get-FileHash/Get-NetTCPConnection、
既有MCP refresh/run_tests/get_test_job/state/scene/pipeline/console、Validate-ChangeLedger.ps1、
只读git status/rev-parse/diff；HEAD仍5a5cde34b9685739326b638f9c5550b69eda7ef6。
无Git add/commit/push/reset/checkout/clean/stash、文件删除/移动或Memory写入。
未读Q06活跃方法体；未改PERF/ATLAS/MONO/EXT-1/Server/Scene/Prefab/现有资源/ProjectSettings/InputActions。
原始前四批dirty保留，native突发丢失容错政策及11阶段/33ms/排序/segment/GPU consumer判据均不改。
准确操作后逐文件manifest见after.json，备份见before.json及before-backups；恢复另获批准只逆向本hunk。
本文件操作VERIFIED不替代实际Battle重进、完整0GC/预算/1000AI或Android父项验收。
