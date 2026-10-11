# M03 分组有限值检查：有界候选结果

结论：**COMPLETED_NO_ADMISSION_DEFAULT_OFF / 本版不采用**。Change NTSD-M03-GROUPED-FINITE-GUARD-001 为 ABANDONED（未通过联合收益门，候选和证据保留默认OFF；不是删除或回滚用户文件）。

候选实际正确性、容量、异常/恢复和当前线程0分配事件门通过；唯一完整 Backend.Build 成本组的2000 quad布局未达到事前均值与尾部判据，4097布局通过。不得只选通过的布局推广，不追加窗口找PASS。此Task已经完成有限评估；M03父项、整tick/H07和Android认证仍开放。

## 实现与范围

仅两个脚本增量：
- Assets/NTSD/Scripts/Animation/Rendering/BattleDynamicMeshBackend.cs：默认false的实例 GroupedFinitePositionValidationEnabled，Build仅向Upload传bool；原guard位置ON按quad检查已写入的v0.xyz/v1.y/v2.x，OFF原逐vertex循环；Clear/Dispose置false。
- Assets/NTSD/Scripts/Test/Editor/M03FiniteVertexScanDiagnostics.cs：实际生产候选正反例和独立Explicit完整Build一次性成本入口。旧局部镜像诊断/原件保留且未重跑。

矩形四顶点含12个位置分量，但仅left/bottom/z/top/right五个独立值。检查存储后的结果保留了算术溢出拒绝，检查点仍在EnsureMesh/ActiveQuadCount/native mutation之前。异常类型、消息、chunk升序及前chunk成功/后chunk失败语义保持。private矩形WriteQuad是当前唯一staging writer；任意反射篡改重复坐标不在此不变量内，负控保留，未来writer变更必须重新审查。

新增状态只有backend owner的capacity1 bool，不分配资源/缓存，不改十一阶段；现有第5阶段Clear与Dispose将其关闭。实际WriteQuad段、Upload native后半段、旧UV属性与任务前真实dirty逐字一致，见unchanged-sections.json。冷配置只出现在明确测试入口，正常默认仍原扫描；旧UV候选始终OFF，无UNITY_EDITOR生产算法分叉。

## 实际测试

原项目 Unity2022.3.62f3，原单一进程39108，既有TCP6402桥。未启动第二Editor，未进入Play或保存Scene。

1. RED job17683785fdeb4f2eb5b0a8c635617381：缺少候选属性的预期失败，原件保留。
2. 原Editor脚本编译/reload后，GREEN job0be8757fdc58452fa8f4b7e056bdbb02：**13/13 PASS**，suite duration80.5120568s。
3. 仅增加同诊断文件完整Build入口后再次实际编译/reload，原Editor空闲、Menu clean8再启动成本；job400521c20fb444fcb7be7c6c3840f58c：**1/1执行完成**，suite8.6146116s。NUnit PASS只表示运行和硬断言通过，性能采用结论以原始gain字段为准，实际为不采用。

13项包含：
- 三绑定Source2D/TextureArray/PrebakedPage × Ordered/Strict六case：完整active44byte顶点/index/submesh/segment/bounds/diagnostics与publication，0/1/4096/4097/缩小/空/恢复；Destroy Mesh→Prepare重建；Seal4097后4098整帧拒绝保持；Unseal/Clear/重建。
- 默认OFF、Clear/Dispose幂等一case。
- 三绑定 × NaN/+Inf/-Inf各xyz（两开关），再invalid command0/4096：相同ArgumentException/message，失败chunk native字节不变，先前成功chunk上传计数保持；按原Clear恢复。三case，每case包含上述矩阵。
- 三绑定 × 四种真实写入算术溢出（有限位置/size/pivot输入，断言实际resolved），拒绝结果/CPU staging相同及native未改；inactive-tail、v3.z反射破坏负控。三case。

首个GREEN状态查询曾主线程忙而超时30s；最终结果实际13/13，未重跑测试或重启Editor。本段恢复后一次脚本reload中的连接拒绝随后自然恢复，未采取状态强改。静态对照片段第一次边界定位拼错public/internal，未产生输出文件；修正定位后全3项一致。以上不隐去失败，但不充当测量/行为失败。

## 唯一完整Build成本组

固定真实array resolver，sealed容量4097，同布局不可变frame。每布局OFF→ON→ON→OFF，每窗8warm+32sample；总8窗、64warm、256 measured Build。全scope包含完整Build和native上传；Stopwatch仅在外层，fine/presentation diagnostics默认为null，旧UV计数关闭。旗标配置/反射/读取payload/assert/序列化均在scope外。完整Build的耗时不与任何子计时相加。

| Quad | 窗口 | mean ms | P95 ms | P99 ms |
|---|---|---:|---:|---:|
| 2000 | OFF1 | 2.441050000 | 2.5703 | 2.6774 |
| 2000 | ON1 | 2.197246875 | 2.3776 | 2.5125 |
| 2000 | ON2 | 2.433021875 | 3.5605 | 3.5987 |
| 2000 | OFF2 | 2.401609375 | 2.5040 | 2.8655 |
| 4097 | OFF1 | 5.227906250 | 7.0562 | 7.5456 |
| 4097 | ON1 | 4.770296875 | 5.5702 | 5.9560 |
| 4097 | ON2 | 4.540009375 | 5.2092 | 6.5456 |
| 4097 | OFF2 | 5.063975000 | 7.3515 | 7.3680 |

事前判据每布局：max(ON mean)<min(OFF mean)，max(ON P95/P99)<=max(OFF P95/P99)，两个布局均须成立。
- 2000：2.433021875不小于2.401609375；P95 3.5605>2.5703，P99 3.5987>2.8655。三项FAIL。
- 4097：三个判据PASS，只保留该布局信号。
- 联合不通过。2000 ON2尾部波动的具体原因未测，不归因GC/OS/算法，也不启动新诊断或重跑。

原rawticks frequency10000000，独立Python重新计算均值和nearest-rank P95/P99全部一致，见independent-analysis.json；未由均值推分位数。

GC复用未修改BattleScopedGcAllocationRecorder同步主线程：前后正例各1event、空例0event、校准passed。256scope全部valid/calibratedBefore、无wrap/saturation、allocationEvents总0。unit=TimeNanoseconds，结论为**该测量scope零分配事件**，不称byte计数或GC回收停顿。完整Driver、整游戏及设备不在范围。

全部8窗actualflag正确、旧UV关闭、容量/对象身份稳定、publication字段未变，完整Backend输出与对应布局OFF基线相同。两脚本+两只读fixture/pixel代码+两程序集共6指纹成本窗前后完全相同。

## 证据复用与未运行

- 复用上一UV候选任务prepared三绑定像素证据（30对比/276480像素无误差）及有限scan review原3+2 guard/IEEE证据：writer、shader、resolver、上传native suffix和资源均未改；本轮新实际候选完整字节与异常时点断言补足受影响范围。旧像素不是本轮新实测，旧mirror不能冒充actual ON。
- 旧UV收益失败、publication工作负载、H06成果/H07约12ms及95旧失败保留原范围，不互相覆盖。
- 未重跑局部镜像成本、未跑第二完整Build组、真实千人Driver、120+180/120+1800正式性能窗、GPU/Android/IL2CPP/ARM64验证。未变更默认生产路径。
- 不读Q06方法体、不修改其它优化点、Scene/资源/ProjectSettings/InputActions/Server/Gen/Plugins，无旧Goal/队列恢复或消息派发。

## 审计与实际命令

Task/Change/Operation在脚本修改前已登记；10份真实dirty备份SHA有效，2442个已登记范围外保护文件无漂移。HEAD/暂存不变。task.diff源自真实dirty备份，不把HEAD当任务前版本。最终原Editor idle，Menu一个Scene/8root/isDirty=false。

实际通过既有TCP桥调用：
- refresh_unity(scope=scripts, compile=request, wait_for_ready=false)
- get_editor_state、manage_scene(action=get_loaded_scenes)
- run_tests(mode=EditMode, testNames=[NTSD.Test.Editor.M03FiniteVertexScanDiagnostics.GroupedGuard_DefaultOffClearAndDispose]) 用于RED；实际GREEN method清单及请求见green-start.json（6+1+3+3 case）。
- run_tests(mode=EditMode, testNames=[NTSD.Test.Editor.M03FiniteVertexScanDiagnostics.GroupedGuard_OneFullBuildAbbaGroup])
- get_test_job(job_id=上述具体job, include_details=true, include_failed_tests=true)

实际本地只读/审计：git status --short、git rev-parse HEAD、git diff --cached --name-status；Python从rawticks复算并校验SHA256/真实备份及原片段；Get-Process Unity；Get-Content Editor.log过滤编译错误；脚本文本修改使用已登记Python局部替换/每次intermediate真实preimage，不删除/移动任何文件。

ChangeLedger validator及最终diff结果见下面追加的收尾记录。下一只审阅此报告，由用户选择是否有新的具名范围；本候选不自动重测、推广或转入其它优化点。
