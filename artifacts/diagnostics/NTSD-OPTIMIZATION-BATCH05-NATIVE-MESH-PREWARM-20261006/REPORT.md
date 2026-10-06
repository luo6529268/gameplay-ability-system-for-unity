# 第五批 H-11 原生Mesh重新预热限定报告

Task NTSD-OPTIMIZATION-BATCH05-NATIVE-MESH-PREWARM-20261006 / RUNTIME_PENDING；Change NTSD-OPT-H11-NATIVE-MESH-PREWARM-005 / RUNTIME_PENDING。
结论：唯一生产hunk（三插入行）把既有native Mesh恢复提前到启动PrepareCapacity；
新12/12、原120/120，去重132具名case实际通过。H-11父项仍OPEN，不是Android/完整0GC认证。

## 本次重扫与实际改动

BattleDynamicMeshBackend.PrepareCapacity先EnsureChunk，再准备描述数组。已有managed chunk的native
Mesh被Unity销毁后，原EnsureChunk不恢复；直到Build.Upload或GetChunkMesh的EnsureMesh才创建。
现于EnsureChunk后访问原Mesh getter，原EnsureMesh/CreateMesh实现、顶点布局、材质/UV/颜色、
模式、segment/顺序、Clear/Dispose均未改；只将原恢复提前到seal前。
现有Mesh直接复用，0容量不创建，不需要的高chunk不提前恢复；sealed Prepare仍先拒绝。
引用本次新扫：生产文件105–107为新增，原Prepare/Seal/EnsureMesh/CreateMesh原语义仍保留。

原生Mesh可能在关闭Domain Reload的Play退出后失效，是源码注明的使用边界；
本批fixture证明毁损→重预热，不把它当真实Battle退出重进已经验收，也不猜实际频率/耗时。
战斗中突然丢失native Mesh的原懒恢复仍可能分配，是剩余风险，不在此批私改其容错政策。

## Test-first 与实际编译

现有Editor PID19040 / TCP6401，Unity2022.3.62f3，URP14现有pipeline，Menu clean/非Play。
没有启动第二Editor；只用既有连接refresh/run_tests/get_test_job及状态查询，不切Scene/Play。
新Editor测试程序集20:13:15、最终生产程序集20:15:22（晚于最后生产源文件修改），idle/CS error0。
生产3插入行中初说明句主语方向有误，立即更正为managed chunk存活，额外refresh后确认最终编译；
未进行该误说明版测试，不影响实际生产hunk行为。原件refresh/编译时间均保存。

- RED ee3e05dddd9e4c148e75d35074b652d7：12项执行，10个stored native Mesh为空预期失败，
  0容量与sealed拒绝两个控制PASS；生产SHA当时902CFBC6C966AD584294B59426D0CF8FC5C3073A92AF401A5D7C3FB64C9B5BB9。
  MCP失败任务result字段为空，保留progress.completed=12/十具名失败；total=9086为发现全集，不当所选分母。
  原状失败在GC区间前，不能称RED测得多少分配。
- GREEN 8a13f03c355d4d76b68db49d665ae954：新12/12、0失败/跳过。
- REGRESSION d40f11c3b4454c3c9b134fff59cbdeb8：相关旧120/120、0失败/跳过。
  resolver23、前三批38、mesh8、LatestFrame13、motion2、Foot7/Health8、common6、子批04新15；
  与新12去重132，不重算为前四批父项已关闭。

新12具名case覆盖：
1/17/4096/4097 required chunk全部失效恢复四项；living/选择性恢复、零容量、
sealed拒绝、不改变原解除后再准备四项；17/4097 × Ordered/Strict首次与tail Build四项。
直接读取fixture存储的mesh字段，不调用会懒创建的getter来掩盖缺口。
恢复后vertex stride44、UInt16、初始单inert submesh/index/bounds及descriptor/array/Mesh身份保持。
全部临时Mesh由fixture/backend Dispose销毁，只内存资源，没有删除/改写用户资源文件。

## 局部0GC结果与边界

四组Build测试先暖通用Mesh API，然后毁损、重预热与seal。
测量区间包含恢复后的第一次Build及8轮 sparse→empty→dense，共25个Build/组；
包括synthetic resolver、quad write、mesh Upload及inactive clear，四组断言均为0B当前线程managed分配。
恢复后的第一次Build没有额外预跑或豁免，反射/NUnit/fixture构建/预热和Dispose均在区间外。
不覆盖实际catalog/publication物化、Q06排序/插值、CommandBuffer录制、RenderPass提交、GPU consumer、
native分配或GPU内存，也不把NUnit用时解释为游戏帧率/CPU收益。

## 红线、风险与未验证项

33ms/3ms/max2interval、战斗pass/RNG/checksum和publication只读未改；模拟脚本未动。
Q06活跃方法体未读写；first-visible/透明重叠相关待确认状态不闭合。
CPU lease/退休/GPU完成判据及11阶段关闭未改；CPU lease归零不是GPU完成证明。
EXT-1仍PROPOSED / MODIFY_REQUIRED，未启动专项M0/instancing；无bank/预算/资源格式/segment变更。
未改PERF/ATLAS/MONO/Scene/Prefab/ProjectSettings/InputActions/Server或现有角色/音频资源。
未做完整SelfCheck/原Battle真实enter/exit/re-enter（含无Domain Reload）/完整central0GC、
完整M0、Native/GPU预算/峰值/1000AI/Android构建/设备测量。父H-11继续RUNTIME_PENDING。
启动恢复成本和总native预算未知；此批只重排原有分配时点，不降低长期峰值作承诺。

## 留痕与恢复

[Task](../../../docs/ai/TASKS/NTSD-OPTIMIZATION-BATCH05-NATIVE-MESH-PREWARM-20261006.md)；
[Change](../../../docs/ai/CHANGE-RECORDS/NTSD-OPT-H11-NATIVE-MESH-PREWARM-005.md)；
[Operation](../../../docs/ai/FILE-OPERATIONS/NTSD-OPTIMIZATION-BATCH05-NATIVE-MESH-PREWARM-20261006/RECORD.md)；
[统一34项进度](../../../Assets/NTSD/Docs/battle-optimization-progress-tracker.md)。
改前八文件精确dirty备份/SHA已于20:10:51匹配，99个保护/前四批留存文件中途SHA保持。
无删除/移动/Git丢弃/add/commit/push或Memory写入。回滚另获批准，仅逆向本hunk，保留用户工作。
raw artifacts：editor-pre/post、red/green/regression-start/result、refresh-red/green/comment-sync、
red/green-compile-host/state、static/links/ledger-validation与Operation before/after。
最终静态检查结果按实际追加，不把文件操作VERIFIED晋升Runtime VERIFIED。

## 最终静态检查

20:20:17 +08，Validate-ChangeLedger.ps1 PASSED：1301 Records / 13 governed code files，
0 error、4238既有warning（与前批相同数量）；production/new test准确覆盖，不宣称全历史无warning。
20:21:07 diff --check exit0（仅既有LF/CRLF提示），34唯一项/高12中14低8、107个限定本地链接无缺失。
第一次static-validation.json的PowerShell foreach把[路径/SHA/boolean]数组展平，造成汇总两个false；
原始逐文件boolean均true，未据此执行删除/恢复/覆盖。20:21:24独立逐项对象复核99保护/前序文件、
8备份无mismatch；[更正静态报告](static-validation-v2.json)复原triplets并保留独立复核。
原错误包装保留，明确是摘要计算错误而非文件变化；没有改写历史证据。

