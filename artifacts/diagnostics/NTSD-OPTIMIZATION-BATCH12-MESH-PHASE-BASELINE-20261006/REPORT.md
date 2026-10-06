# 第十二批 M-03 阶段成本受控基线

当前 SCOPED_MESH_PHASE_BASELINE_PASS：受控热点定位通过，生产未改，父项OPEN。范围、采样和不变量见 [Task](../../../docs/ai/TASKS/NTSD-OPTIMIZATION-BATCH12-MESH-PHASE-BASELINE-20261006.md)。
原Editor PID19040/6401 Menu clean/8roots/idle/nonPlay，Unity2022.3.62f3/URP14.0.11；仅fixture新增8组阶段取样与原12组同轮控制。
production不改，不启动完整/EXT1 M0/Play/Profiler/GPU或Android；父M03 OPEN/RUNTIME_PENDING。
逐次原件/实际状态/观测税及范围外核验在本报告后续追加。

## 本批当前结论：SCOPED_MESH_PHASE_BASELINE_PASS，父项OPEN

只有Editor fixture改动，没有生产埋点/算法/资源修改。8组phase+原12组无诊断控制=20/20 Passed，
相关旧submesh3/bounds5/upload18/detail8=34/34 Passed，54唯一fullName全部通过。
36000 sampled Build，1280 warmup排除；每组1800+64；六phase×8×1800=86400有效阶段样本。
所有组prepared Build+recorder sample reset/完成/读取/累计窗口当前线程managed0B、growth0；
实际API payload/segment/chunk、stream0 stride44、Mesh引用/instanceID保持；不是完整物化—录制—提交0GC。

高分段组的descriptor/native metadata阶段已定位为本受控backend热点：
1000物理segment中SetSubMeshes包含阶段mean1.1372～1.2041ms，Upload parent约1.3354～1.4068ms，
占parent约85%；不是纯native API或GPU draw实测。此时顶点上传API返回阶段mean0.0057～0.0077ms。
各1000组实际成功vertex API仍1次/Build、4000顶点/176000bytes；不是减少上传或GPU带宽改善。
单segment mean约0.0011～0.0012ms metadata；4097双chunk双segment约0.0029～0.0031ms。
观察支持优先选择同descriptor/顺序/segment/尾部/高水位/bounds语义的metadata最小A/B；没有批准改变这些语义。

## 采样与环境

原Editor PID19040/6401，Unity2022.3.62f3/URP14.0.11，WindowsEditor/D3D11/RTX4070，
i5-13600KF（14core/20logical）；另两Unity进程本任务不控制、不读取未脱敏命令行。
合成resolver固定Resolved/null Texture/Material/white tint，variant可交错，两个frame提前创建/位移0.25。
与子批11资源模型同，不是当前catalog/真实drawable/pixel、自然publication/插值/AI量。
Runner临时untitled scene.path空；原Menu前后clean/8roots/nonPlay，不主动切换/保存Scene。
没有Play、测试场景资产/输入资源修改、Profiler窗口/录制、GPU capture、FrameDebugger、完整/EXT1 M0或Android。

Compile scope-all请求后domain reload一次暂不可用；最终原Editoridle/CS0，EditorDLL22:48:57晚于source22:48:41。
production DLL保持22:15:24/SHA2A8A4B...A2；[source/assembly manifest](compiled-source-manifest.json)六项。
[编译状态](compile-current.json)、[reload原件](compile-reloading.json)、[原Scene](editor-before.json)、[收尾Scene](editor-after-tests.json)。

## 阶段结果（全部为每次Build的mean ms）

| 命令数 | 模式 | 几何 | variant | Build | Resolve+Write父 | Resolve子 | Write子 | Upload父 | Vertex API子 | SubMeshes包含子 |
|---|---|---|---|---|---|---|---|---|---|---|
| 1000 | OrderedChunks | 同几何 | 固定 | 1.0695 | 0.8709 | 0.2744 | 0.3992 | 0.1977 | 0.0051 | 0.0012 |
| 1000 | OrderedChunks | 位置交替 | 固定 | 1.0764 | 0.8740 | 0.2759 | 0.4006 | 0.2015 | 0.0049 | 0.0011 |
| 1000 | StrictOrderedDraw | 同几何 | 固定 | 2.2349 | 0.8618 | 0.2797 | 0.3629 | 1.3719 | 0.0070 | 1.1722 |
| 1000 | StrictOrderedDraw | 位置交替 | 固定 | 2.2769 | 0.8688 | 0.2817 | 0.3654 | 1.4068 | 0.0077 | 1.2041 |
| 1000 | OrderedChunks | 同几何 | 交错 | 2.2284 | 0.8920 | 0.2773 | 0.3520 | 1.3354 | 0.0057 | 1.1372 |
| 1000 | OrderedChunks | 位置交替 | 交错 | 2.2560 | 0.8992 | 0.2795 | 0.3555 | 1.3557 | 0.0059 | 1.1573 |
| 4097 | OrderedChunks | 同几何 | 固定 | 4.4424 | 3.6330 | 1.1503 | 1.6633 | 0.8076 | 0.0371 | 0.0031 |
| 4097 | OrderedChunks | 位置交替 | 固定 | 4.3146 | 3.5145 | 1.1095 | 1.6125 | 0.7984 | 0.0307 | 0.0029 |

coarse ResolveAndWrite包含Resolve/Write及segment处理，Upload包含两API子阶段/finite checks/mesh.bounds。
SetSubMeshes子阶段包含descriptor生成/遍历与native调用，不拆成纯API；所有父子有包含关系，禁止直接求和。
report各phase保存p50/p95/p99/max/mean/timestampSum，并列Build外/ResolveWrite子外/Upload子外三个残余mean。
残余包含未设更细timer的有限检查和bounds等，不猜测全部归属于哪条API。
BeginTick/CompleteTick只是本地recorder样本id，不执行SimulationWorld/tick、不修改production counter。

## 同轮无诊断控制（Build ms）

| 命令数 | 模式 | 几何 | variant | p50 | p95 | p99 | max |
|---|---|---|---|---|---|---|---|
| 100 | OrderedChunks | 同几何 | 固定 | 0.0706 | 0.0783 | 0.1082 | 0.1941 |
| 100 | OrderedChunks | 位置交替 | 固定 | 0.0702 | 0.0757 | 0.0979 | 0.1689 |
| 500 | OrderedChunks | 同几何 | 固定 | 0.3468 | 0.3709 | 0.4714 | 0.7046 |
| 500 | OrderedChunks | 位置交替 | 固定 | 0.3454 | 0.3639 | 0.4756 | 0.7323 |
| 1000 | OrderedChunks | 同几何 | 固定 | 0.6912 | 0.8175 | 1.0605 | 1.0951 |
| 1000 | OrderedChunks | 位置交替 | 固定 | 0.6901 | 0.7565 | 1.0294 | 1.0975 |
| 1000 | StrictOrderedDraw | 同几何 | 固定 | 1.7894 | 1.9930 | 3.0424 | 3.1731 |
| 1000 | StrictOrderedDraw | 位置交替 | 固定 | 1.7899 | 1.9197 | 2.3977 | 3.3786 |
| 1000 | OrderedChunks | 同几何 | 交错 | 1.8447 | 2.1522 | 2.9638 | 3.2210 |
| 1000 | OrderedChunks | 位置交替 | 交错 | 1.8471 | 2.1138 | 2.6840 | 3.9235 |
| 4097 | OrderedChunks | 同几何 | 固定 | 2.8298 | 3.2448 | 4.2357 | 5.0207 |
| 4097 | OrderedChunks | 位置交替 | 固定 | 2.8369 | 3.1774 | 4.3431 | 4.4287 |

所有phase case先于controls顺序运行，不是随机交替配对。1000组instrumented Build mean比相应control高0.33～0.46ms，
4097高1.43～1.56ms；有per-command StopWatch/marker/recorder观测成本与时间窗口环境差异，
不能称精确测量税或优化收益。无诊断控制1000单段p50约0.69ms、高segment约1.79/1.85ms，复现上批局部趋势。
Prepared直接Build强制执行，未经过queued same-sample去重门，不称生产每帧都会重复Build。

## 当前代码证据（本批重扫，非旧文档行号）

- [backend:156](../../../Assets/NTSD/Scripts/Animation/Rendering/BattleDynamicMeshBackend.cs)支持两个既有diagnostics可选参数；198起coarse resolve/write，350起Upload。
- [Upload:698](../../../Assets/NTSD/Scripts/Animation/Rendering/BattleDynamicMeshBackend.cs)有限坐标校验在vertex API timer之外；
  714/717 API计时，748起descriptor/native timer。
- [metadata分支:790](../../../Assets/NTSD/Scripts/Animation/Rendering/BattleDynamicMeshBackend.cs)：范围变化batch，
  稳态else800–804逐active submesh更新；这只是静态调用事实，没有新增native API计数器，不能推出GPU draw。
- [presentation recorder:114](../../../Assets/NTSD/Scripts/Simulation/Presentation/BattlePresentationPhaseDiagnostics.cs)完成后复制sample；
  [detail recorder:343](../../../Assets/NTSD/Scripts/Simulation/Diagnostics/SimulationWorld.DetailTimingDiagnostics.cs)读取当前sample累积。
- [fixture:44](../../../Assets/NTSD/Scripts/Animation/Rendering/Editor/BattleMeshUploadBaselineEditorTests.cs)新8项phase入口；
  原12项入口/schema保留，RunBaseline null recorder路径与production Build不变。
- Q06方法体未读取/修改，只保护hash，不把排序/first-visible/透明重叠推广为完整闭合。

## 实际执行、失败与范围限制

1. baseline job fee8c39d9afb4f10b8a02c0d1cec8fc2：20/20、0failed/0skipped，67.1896762秒。
   [原结果](baseline-result.json)、[20份采样JSON](baseline-samples.json)、[8case时进度](baseline-first-progress.json)。
2. regression job17476eed227049fd8cf4eca1e6a1595d：34/34、0failed/0skipped，2.0601896秒；
   [原结果](regression-result.json)。discovered9239不是执行9239，[唯一case/对照观察](observation-and-unique.json)。
3. 预扫描JSON输出预算截断/误解析session空stdout、reload暂不可用/两rg路径错误为工具读取失败，未改生产/未中断test；
   文档multi-file apply_patch有一次上下文不匹配，重新核目标显示零落盘，再按实际行精确apply_patch成功。
   无测试失败/修复RED，不把工具错误写成生产首差。原错误说明见[Operation](../../../docs/ai/FILE-OPERATIONS/NTSD-OPTIMIZATION-BATCH12-MESH-PHASE-BASELINE-20261006/RECORD.md)。
4. 不运行全SelfCheck/权威trace：没有生产规则变动；未重开旧alignment campaign/扩大非战斗职责。
   parent OPEN/RUNTIME_PENDING；自然生产窗口/完整0GC、实际资源像素/排序、退出重进、收益/1000AI/Android未证。
5. EXT1 PROPOSED/MODIFY_REQUIRED、MONO USER_HOLD、PERF/ATLAS正文/segment/bank/预算/格式保持，不启动专项M0。

## 下一最小包与验收边界

当前局部数据支持先比较existing native metadata批量更新与逐段更新成本，保持完整descriptor字段、
物理high-water、active/inert tail、range/bounds、命令顺序、UV、finite/fail-closed及lease。
实施前另立Change、先行为/0GC断言，再同fixture前后A/B；收益未成立或descriptor/像素首差则不晋升。
不优先跨chunk合并、GPU Instancing/资源格式或dirty上传重构，不新增未经证据支撑的重构项。
34项仍高12/中14/低8，parent closed0；更新[M03方案](../../../Assets/NTSD/Docs/android-mobile-readiness/m-03-dynamic-mesh-upload.md)
与[统一进度](../../../Assets/NTSD/Docs/battle-optimization-progress-tracker.md)。治理收尾实测在下文追加。

## 治理窄验收

22:55:38 Validator exit0/PASSED、1308Records/当前dirty governed4；全局4275历史warning、0error，新增Change/fixture0warning。
首轮Write-Host未捕获的截断输出不作为完整warning统计；全流捕获原件[ledger](ledger-validation.json)。
22:55:40 static：358保护SHA/8准确dirty备份/六source-compiled身份全部保持，164links零缺失，
diff-check0、fixture trailing0；[static](static-validation.json)，首轮六个相对line-link已精准修正并保留[旧观察](static-first.json)。
原Editor最终idle/nonPlay/notest、Menu clean/8roots/error-CS0；[final](editor-final.json)。
前十一批/用户其它改动保护不动；HEAD2cccd597保持，不删/移/清理/Gitadd/commit/push。
Change VERIFIED只Editor fixture，Task SCOPED_MESH_PHASE_BASELINE_PASS；父M03仍OPEN/RUNTIME_PENDING。
pixel-perfect技能只确认URP、Unity连接技能只复用现有2022 MCP；无Unity6 Pipeline/依赖安装、相机/60Hz/Importer改动。
精确after冻结见Operation目录after.json（自身除外）；冻结后仅只读复验。
