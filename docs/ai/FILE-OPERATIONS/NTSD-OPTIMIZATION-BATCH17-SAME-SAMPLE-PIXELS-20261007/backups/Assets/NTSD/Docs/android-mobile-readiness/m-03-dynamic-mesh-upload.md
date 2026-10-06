# M-03 中央动态 Mesh 构建与上传优化方案

> 优先级：高（2026-10-06由中调高，原ID/路径保留）
> 状态：`OPEN / RUNTIME_PENDING / SCOPED_DYNAMIC_PUBLICATION_PASS / PROFILING_REQUIRED`；既有计数/局部A-B和自然窗口证据保留在各子批。
> 最后更新：2026-10-07
> 本轮共同合同与启动门：[2026-10-06复核](../battle-optimization-rebaseline-and-start-gates-20261006.md)；用户已批准普通优化实施，子批06～10补计数/报告、11～13局部基线/A-B、14原Battle小roster自然计数窗口、15warm相机消重限定通过；完整M0/专项门不解冻。
> 主登记表：[Android 移动端就绪度与 1000 AI 风险清单](../android-mobile-readiness-priority-risk-register.md)

## 问题与边界

CentralOnly主体由动态Mesh提交；当前已有显示插值，publication不变但alpha变化会重新物化，并上传活动顶点，不是“每publication最多一次”。命令数取决于actor/weapon/effect/shadow/foot/health，不把旧约3000条推演当当前固定数。UV采样边界已进入顶点payload（现有stride44），旧上传估算需重算；桌面耗时不能外推手机。

## 解决方案

1. 按publication变化、alpha变化、完全重复显示三类统计物化次数/复用率，再拆Build/Resolve/WriteQuads/submesh/upload/Foot/health/录制提交/GPU；给出每显示帧与每publication两种口径。
2. 配合H-11预热并seal全部缓存；当前只上传活动范围，但未证明跳过未变化chunk。A1作为待实施设计，拆不可变UV/绑定/拓扑与插值位置等显示脏区；不能以publication不变跳过位置更新。
3. 对不可见、未变化或无有效 binding 的命令做有证据的早期过滤，同时保持 hit-stop、排序和 first-visible tick。
4. 先冻结A3取样/排序/first-visible/latency合同，再决定是否选择A4候选表示，最后定型A2 job输出；实例表示未批准时仍用现有顶点表示。保留canonical A/B，避免先MeshData再instancing重复设计。

## 验收条件

- seal/warmup后完整物化—上传—录制—提交热路径0B/frame、0动态扩容；不得用残余许可放宽既有0GC硬合同，诊断自身开销另记。
- 1000 visible 下无 buffer resize、capacity reject、stale submission 和未知 submesh 重建。
- CPU render stages 与 GPU P95 满足设备预算，优化前后像素/排序一致。
- 关闭、重进和 chunk 缩放无旧数据 ghost 或 Native 资源泄漏。

## 测试条件

| 测试 | 条件 | 通过标准 |
|---|---|---|
| 微分段计时 | 100/500/1000 visible | 各阶段随规模曲线可解释 |
| 上传范围 | 部分可见、跨 chunk 边界 | 只上传有效数据，无越界/旧 Quad |
| 0GC | 1800 sampled frame | managed allocation 为 0 |
| 像素/排序 | actor/weapon/effect/shadow/health | 与基线一致 |
| Android GPU | Adreno/Mali × Vulkan/GLES3 | driver、CPU native transition、GPU 分离记录 |

## 证据与留痕

- 当前观察：有效帧仍执行命令解析、顶点写入、submesh 与 `SetVertexBufferData`。
- 保存 Profiler marker、chunk/command 计数、上传字节、设备/API 和 A/B 报告。
- 2026-09-06：方案建立；尚无 Android 热点结论。
- 2026-10-06：本项由中调高，保留M-03 ID。重扫`BattleCentralRenderSystem.cs:397-406,624-629,668-682`及`BattleDynamicMeshBackend.cs:443-449,663-686`；补同publication插值重建、UV边界和H-11容量门。没有运行测量。

### 2026-10-06 子批06：实际顶点上传计数（聚焦通过，父项未关闭）

[Task](../../../../docs/ai/TASKS/NTSD-OPTIMIZATION-BATCH06-VERTEX-UPLOAD-COUNTERS-20261006.md) /
[Change](../../../../docs/ai/CHANGE-RECORDS/NTSD-OPT-M03-VERTEX-UPLOAD-COUNTERS-006.md) /
[报告](../../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH06-VERTEX-UPLOAD-COUNTERS-20261006/REPORT.md)。
生产只补BattleCentralBuildDiagnostics三个标量，中央chunk.Upload成功API返回处更新：
VertexUploadCallCount、UploadedVertexCount、UploadedVertexBytes（后两者long）。
每获准进入Build/Clear重置；准入前拒绝保留旧诊断，Build中途异常只记此前完成的API，
该异常口径不是允许partial submission。重复Build照实上传计数，A1 dirty-chunk仍待实施。
字节按Mesh创建时缓存的实际stream0 stride计算；不硬编码44、不把chunk预留容量当活动上传。
Prepare/索引/子网格元数据/辅助Foot/Health/RenderPass/GPU流量均不在本计数范围。

原Editor实际编译；新18/18、旧132/132通过。两模式各48次预热后的Build/Upload局部managed0B。
没有运行完整M0/Profiler/Play/设备测量；未读Q06活跃方法体、未改PERF/ATLAS/EXT-1正文或专项门。
本批是诊断前置，不是减少上传或帧率收益证据。下一仍需publication/alpha/完全重复分组、
benchmark/报告出口接线、完整链0GC和同布局基线；不得将未批准A4表示或资源格式作为默认。

### 2026-10-06 子批07：可选报告与完成帧快照（聚焦通过，父项未关闭）

[Task](../../../../docs/ai/TASKS/NTSD-OPTIMIZATION-BATCH07-UPLOAD-REPORT-20261006.md) /
[Change](../../../../docs/ai/CHANGE-RECORDS/NTSD-OPT-M03-UPLOAD-REPORT-007.md) /
[报告](../../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH07-UPLOAD-REPORT-20261006/REPORT.md)。
生产只修改BattleRenderingBenchmark：centralVertexUploadCalls(count)、centralUploadedVertices(count)、
centralUploadedVertexBytes(bytes)可选frame/summary字段，并注明limitations.centralVertexUploadScope。
成功Present且workload校验后冻结值；完成帧等待时不读live新值；只已接受尝试进入summary，
warmup和被拒绝retry不进入，不是全运行累计；Legacy/null diagnostics unavailable，实际0保留为available0。
统计仅benchmark-local中央Mesh API完成vertex payload，不等于GPU流量/真实batch或生产RenderPass/submission。
v5 mandatory metricAvailability注册表与PASS判据完全保留，可选字段不加入必测schema。

原Editor实际compile，GREEN14/14、旧168/168通过（去重182）；等待/重试/异常/预热/单位/大字节值/
不可用/投影/原v5合同均通过。central/Legacy各128次预备后新增helper局部managed0B；
benchmark CaptureFrame/report本来有分配，未声称完整链0GC。未启Play/完整M0/Profiler/GPU/设备。
上传算法/次数、segment/排序/fail-closed、slot/lease/关闭合同不变；未读取Q06活跃body。
下一仍需publication变化/alpha变化/完全重复分类、当前生产同布局基线、完整链0GC及脏区设计；
这些报告不会把当前冻结benchmark workload晋升为生产动态publication或1000AI性能证书。

### 2026-10-06 子批08：显示request三分类与Prepare入口计数（聚焦通过，父项未关闭）

[Task](../../../../docs/ai/TASKS/NTSD-OPTIMIZATION-BATCH08-MATERIALIZATION-COUNTERS-20261006.md) /
[Change](../../../../docs/ai/CHANGE-RECORDS/NTSD-OPT-M03-MATERIALIZATION-COUNTERS-008.md) /
[报告](../../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH08-MATERIALIZATION-COUNTERS-20261006/REPORT.md)。
仅BattleCentralRenderSystem新增标量观测：有效CentralOnly queued request按前一有效request
的world/publication version/displayAlpha分类。首次或identity变化归PublicationChanged；
identity不变但alpha改变归AlphaChanged，其余Repeated。queued identity包括world/frame/tick/mode，
不把版本等同于逻辑tick；分类不驱动渲染分支，也不改变原ResolveAlpha取样与去重条件。

request总数/三类、PrepareFrameImmediate入口attempt总数/三类分开；另外累计
SameUnityFrameReuseCount、SameSampleReuseCount、ReentrantSkipCount，均是原return gate次数，
不保证返回的plan有效或成功复用了像素。force进入Prepare仍可能只返回cached plan，
不能把attempt当实际Build、vertex上传、成功submission或GPU draw。
只主线程显示入口观察，不给Queue/worker加写计数；ResetRuntime清全部计数与world baseline，
原每帧诊断reset不清累计。无新增资源/owner/lease或关闭阶段。

原Editor实际compile，新20/20、旧182/182（去重202），0失败/跳过；
64次预备后512次cached request+attempt helper当前线程managed0B，反射/创建/断言在窗口外。
同Unity帧新publication/同样本/busy原gate、force cached-plan无实际上传、Legacy/expectedWorld拒绝、
world/version/精确alpha分类、计数隔离和Reset生命周期均聚焦通过。
未启Play/实际Profiler/完整M0/GPU/设备；辅助观测开销与真实动态alpha比例未测。

下一剩余是把类别与实际Build/上传及生产报告正确关联、同布局生产基线、
完整物化—上传—录制—提交0GC、有证据的dirty区/像素排序/资源生命周期和1000AI/Android验收。
A1跳过未变chunk仍未来设计，A4未授权；请求分类通过不等于减少上传或帧率提升。

### 2026-10-06 子批09：实际Build与成功API payload按request分类归属（聚焦通过，父项未关闭）

[Task](../../../../docs/ai/TASKS/NTSD-OPTIMIZATION-BATCH09-BUILD-UPLOAD-ATTRIBUTION-20261006.md) /
[Change](../../../../docs/ai/CHANGE-RECORDS/NTSD-OPT-M03-BUILD-UPLOAD-ATTRIBUTION-009.md) /
[报告](../../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH09-BUILD-UPLOAD-ATTRIBUTION-20261006/REPORT.md)。
仅CentralRenderSystem：总组+PublicationChanged/AlphaChanged/Repeated三组各8个long，分别记录
BuildInvocation、Admitted、Completed、RejectedBeforeBuild、Failed、VertexUploadCall、UploadedVertex和Bytes。
有效queued request的局部kind传入物化core；旧两参Prepare诊断入口None保留，不算生产分组。
Build wrapper finally以MutationVersion变化识别准入；准入前拒绝保留旧diagnostics但本次payload不重复计，
进入后失败在外catch Clear前冻结已完成API，不能理解成授权partial submission。
empty/null/unresolved正常返回为completed0payload，不证明像素/提交成功；force cached-plan无Build。
只中央实体Mesh，排除Foot/Health、索引/submesh、录制/提交/RenderPass/GPU流量和benchmark-local。
四组启动创建、per-frame保持、ResetRuntime复用清零，未新增owner/lease/资源或关闭阶段。

原Editor实际compile，有效RED23→GREEN23/23、相关旧202/202，去重225，0失败/跳过。
scalar64预备后512次局部managed0B；Ordered/Strict各预备后144 wrapper+Build、96API/3456顶点局部managed0B。
这是夹具内当前线程wrapper+Build/upload范围，不替代完整物化—上传—录制—提交0GC和像素验收。
未运行Play/完整M0/真实Profiler/GPU/设备；没有改变上传算法或减少次数，没有性能收益证据。
下一生产分组报告/当前同布局基线/完整链0GC及有证据dirty设计，1000AI/Android仍待验收。
EXT-1/MONO/ATLAS专项门与Q06待确认边界保持，不把本批通过升级为A4或资源布局授权。

### 2026-10-06 子批10：生产计数快照/JSON与受控窗口（聚焦通过，父项未关闭）

[Task](../../../../docs/ai/TASKS/NTSD-OPTIMIZATION-BATCH10-MATERIALIZATION-REPORT-20261006.md) /
[Change](../../../../docs/ai/CHANGE-RECORDS/NTSD-OPT-M03-MATERIALIZATION-REPORT-010.md) /
[报告](../../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH10-MATERIALIZATION-REPORT-20261006/REPORT.md)。
新增显式生产读出口RuntimeDiagnostics.CaptureMaterializationReport()，冻结43个long：
request4/Prepare attempt4/原return gate3及total/三类Build/API各8；复用BattleCanonicalJson。
累计cumulative-since-reset和window-delta分开；窗口保留start/end供复算，
同source token/epoch才可差值，空baseline/不同来源/Reset/非累计端点/任何counter倒退整份拒绝。
token不含World/Unity资源，epoch仅Reset递增；原阶段5清累计不变，无新owner/queue/lease。
报告只纯managed快照，导出字典修改不反写；原上传/渲染/模拟/benchmark v5不改。
Capture/projection/JSON有分配，只主线程writer不并发的显式非热路径；无Mono/自动采样/文件writer，
不是完整M0或显示帧/tick/时间分母，不把counter0或completed Build当像素/FPS/GPU/设备通过。

原Editor实际compile，RED完成58/25capped缺入口失败原件保留；新58/58+旧225/225，去重283。
预备64后512次原aggregation+新增epoch Reset当前线程managed0B，非报告导出或完整链。
未运行Play/真实Profiler/完整M0/GPU/设备；父OPEN，下一具名生产采样窗口及同布局baseline，
完整链0GC/dirty设计/像素排序/退出重进/1000AI/Android仍待验收，未测收益保持未知。
EXT-1/MONO/ATLAS专项门保持；不读取Q06活跃body，不冻结bank/预算/资源格式。

### 2026-10-06 子批11：当前backend受控基线（限定通过，父项未关闭）

[Task](../../../../docs/ai/TASKS/NTSD-OPTIMIZATION-BATCH11-MESH-BACKEND-BASELINE-20261006.md) /
[Change](../../../../docs/ai/CHANGE-RECORDS/NTSD-OPT-M03-MESH-BACKEND-BASELINE-011.md) /
[报告](../../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH11-MESH-BACKEND-BASELINE-20261006/REPORT.md)。
只新增Editor fixture，生产不变；新12/12、相关旧26/26，共38去重Passed。
各64预热/1800采样，共21600 Build，当前线程managed0B/capacityGrowth0，Mesh身份保持，
实际stream0 stride44/成功vertex API/有效顶点bytes/physical segment均复算。
1000命令Ordered单段p50约0.70ms；Strict/交错variant1000物理段约1.79/1.85ms，
相同176000bytes/Build。4097两chunk两API/721072bytes，不是slot容量/GPU流量。
仅原Editor RTX4070/D3D11/i5-13600KF，synthetic binding/null Texture/Material；
这是backend CPU/API返回成本，非真实catalog/publication/插值/RenderPass/GPU或1000AI/Android。
强制同frame Build仍全上传，但本fixture绕过已有queued同样本去重门；
不能宣称生产重复显示每帧都Build，production自然分类/alpha比例仍未测。
TestRunner临时untitled场景，原Menu前后clean/8roots/非Play；未主动切场景/Play/完整M0。
下一先复用既有phase timing定位分段CPU成本，再最小A/B，不跨chunk合并/改segment/排序/UV；
A1 dirty-chunk仍未来设计。父项真实生产窗口/完整链/像素关闭重进/收益/设备门及所有专项门保持。

### 2026-10-06 子批12：既有阶段计时受控成本（限定通过，父项未关闭）

[Task](../../../../docs/ai/TASKS/NTSD-OPTIMIZATION-BATCH12-MESH-PHASE-BASELINE-20261006.md) / [Change](../../../../docs/ai/CHANGE-RECORDS/NTSD-OPT-M03-MESH-PHASE-BASELINE-012.md) /
[报告](../../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH12-MESH-PHASE-BASELINE-20261006/REPORT.md)。只扩展子批11 Editor fixture，不新增生产埋点/改变算法。
原Editor8阶段+12控制20/20、相关旧34/34，54去重Passed；各64预热/1800采样，36000 Build/1280warmup。
六phase样本86400；局部prepared Build+recorder reset/完成/读scalar当前线程managed0B/growth0，
实际payload/segment/Mesh身份保持。原Menu前后clean/8roots/nonPlay，Runner临时untitled scene非生产窗口。
1000单段Upload mean约0.20ms、SetSubMeshes含descriptor约0.0012ms；1000物理段Upload约1.34～1.41ms，
其中SetSubMeshes含descriptor遍历/生成与native metadata约1.14～1.20ms（占Upload约85%）。
同1000 payload176000bytes，SetVertexBufferData API返回计时约0.006～0.008ms，不是GPU执行/带宽证据。
coarse/detail同时开启有每command timestamp/marker观测税；1000组Build mean较顺序控制高0.33～0.46ms，
不把差值当配对收益或精确计时税。父子phase重叠禁止直接相加；Upload残余含finite checks/bounds，未进一步归因。
下一优先验证相同descriptor/排序/physical segment/尾部清零/高水位/bounds合同的native metadata最小A/B，
只有行为/0GC/收益证据支持后落地，不跨chunk合并、不改变DrawMesh数或GPU排序。
A1 dirty-chunk未来设计、A4待批保持；生产动态publication/素材/完整链/像素/退出重进/1000AI/Android继续待验。

## 子批13：稳定多子网格metadata批量更新（2026-10-06）

[Task](../../../../docs/ai/TASKS/NTSD-OPTIMIZATION-BATCH13-SUBMESH-BATCH-UPLOAD-20261006.md) /
[Change](../../../../docs/ai/CHANGE-RECORDS/NTSD-OPT-M03-SUBMESH-BATCH-UPLOAD-013.md) /
[报告](../../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH13-SUBMESH-BATCH-UPLOAD-20261006/REPORT.md)。
唯一生产hunk在BattleMeshChunk.Upload：stable active>1且active==retained physical count复用SetSubMeshes；
growth/range change既有batch不改，小prefix/单段仍旧路径，避免inactive high-water tail反复批量遍历。
不跨chunk、不合并segment、不减少DrawMesh或宣称GPU batch下降，不改任何顶点/UV/绑定/排序/lease合同。
原Editorcompile、改前26/26、改后20/20、相关71/71，91去重Passed；
两端40报告有效Build72000/warmup2560，局部当前线程managed0B/growth0，20合同投影相同，stride44。
1000 Strict/交错variant无诊断mean约1.878～2.000→0.996～1.051ms，降46.33～47.48%；
metadata inclusive含descriptor/native约1.159～1.209→0.315～0.340ms。
顺序synthetic A/B非随机/自然publication/素材/1000AI/整场/GPU/device。
单段开计时mean约增2～3%保留，无诊断p50近似保持，不把采样波动无证据归因或算为收益。
新三合同检查两mode移动/range/bounds/UV/colors、tail/empty/recovery、小prefix0B，
旧random bounds、native mesh再预热、整帧限位拒绝/旧read lease及atlas identity通过。
首次新增exact平移bounds断言失败改2e-5几何容差，旧/新两端通过；非production RED，原件保留。
生产Change及父M03仍RUNTIME_PENDING；A1 dirty-chunk待实施、A4待批/未启动专项M0保持。
下一真实中央物化/publication-alpha显示窗口、像素/完整链0GC/实际收益验证，再按证据选后续热点；
原Battle退出重进/1000AI/Android门仍开放，不以本Editor结果替代设备认证。

## 子批14：原Battle自然物化窗口与关闭重进（2026-10-07收尾）

[Task](../../../../docs/ai/TASKS/NTSD-OPTIMIZATION-BATCH14-PRODUCTION-WINDOW-20261006.md) /
[Change](../../../../docs/ai/CHANGE-RECORDS/NTSD-OPT-M03-PRODUCTION-WINDOW-014.md) /
[报告](../../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH14-PRODUCTION-WINDOW-20261006/REPORT.md)。
只新增Editor camera begin/end只读观察，不新增生产统计/强制物化/新lease/人工输入或配置；
原Editor实际compile、101确认去重case通过，两自然窗口各tick8→104/96tick，camera128/136、Build256/272。
publicationChanged96/96、alphaChanged160/176、repeated0；成功vertex API与Build同数，
实体Mesh每Build704bytes，窗口180224/191488bytes；2实体/4command/4物理segment/1chunk，stride44。
source publication与captured隔离、command范围/绑定有效、bounds有限；两slot实际观察，growth/unresolved/failed/rejected0。
camera envelope/observer与已有tick/Update/Late/PlayerLoop端点均0B；各作用域不可相加，
Editor硬门/collection控制支持false，非完整链/其他线程/native/GPU/Player/1000AI的0GC证明。
两轮按现有11阶段关闭objects/slots/borrowers0，Scene clean/SHA不变，原Menu恢复；
有Domain Reload正常重进不覆盖H-11无Domain Reload再预热/突发native丢失的全部验收。

新鲜场内证据：SimulationStageRenderModule.PresentLatestFrame调用force Flush，
BattleRenderFeature.AddRenderPasses又相机物化；camera begin→end generation全+1、slot0→1，
window Build=2×camera样本。当前自然小roster两个入口实证，不能外推所有配置或保证50%收益。
下一最小候选：先闭合早期plan/辅助消费者、采样时刻、publication age/first-visible/failclosed与legacy，
再test-first评估相机前只排队而不重复构建几何；不改逻辑/排序/UV/segment/slot语义。
本批未实施消重；A1 dirty-chunk仍未来设计、A4/EXT1仍待批无专项M0。

初v1因end-only采样混叠强制双slot而FAIL，生产检查和关闭通过，原件/v1备份保留；
修正begin/end观察在新corrected-02输出。自动第二轮未进入Play的内部原因待确认，
只增显式idle第二轮入口接续，第一份PASS不覆盖；两个旧UnityTest缺最终case不计通过。
本子批SCOPED_PRODUCTION_WINDOW_PASS不关闭父M03/既有生产Change的所有验收；
像素/GPU/透明重叠/first-visible/完整0GC、真实高segment收益、120FPS/1000AI/Android继续开放。

## 子批15：warm CentralOnly host/camera 消重（2026-10-07）

[Task](../../../../docs/ai/TASKS/NTSD-OPTIMIZATION-BATCH15-CAMERA-MATERIALIZATION-20261007.md) /
[Change](../../../../docs/ai/CHANGE-RECORDS/NTSD-OPT-M03-CAMERA-MATERIALIZATION-015.md) /
[报告](../../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH15-CAMERA-MATERIALIZATION-20261007/REPORT.md)。
SCOPED_CAMERA_MATERIALIZATION_PASS；生产Change/父项保持RUNTIME_PENDING，不关闭动态/完整链/设备门。

重新扫描确认CentralOnly旧Renderer/overlay/spark按mode立即绕过，不依赖早期geometry。
仅interactive/non-batch、CentralOnly、配置>30FPS、已有有效非stale plan且原renderer route近期有效：
host Queue并保留原ResolveDisplayAlpha时钟取样，geometry留既有camera入口；pending版本仍拒绝旧lease。
冷启动/world变化/stale/renderer失败/<=30/Edit/batch/legacy/shadowbuild与显式Flush全部保留原路径。
Central除新20行helper外全字相同（只规范化换行比较），Stage只替换一处调用。
无新slot/缓存/worker、不改相机取样公式/排序/segment/UV/44stride/11阶段关闭；Q06活跃body未读/改。

test-first旧14completed观察3预期RED；新14/14+相关135/135共149去重Passed，
含整帧容量拒绝/保留good submission、辅助/Resolver/lease/计数和4关闭测试。
warm入口16预热后64次0B，同tick空World checksum不变，仅局部fixture，不是正式全场回放。
原saved Battle默认2实体/4command，各96tick：
before132camera/264Build/185856bytes；after143/143/100672bytes与118/118/83072bytes。
归一后Build/camera2→1、entity vertex bytes/camera1408→704，所记录工作少50%，
不是全CPU耗时/Foot-Health上传/GPU流量/GPU batch/FPS/整体性能50%。
三窗physical segment4/recorded CPUdraw5未变，latest publication/captured隔离、finite bounds/stride44/两slot通过，
0growth/failed/rejected；after2一次camera间隔tick95→97，因此publication类别95而非96，未改变latest/max2合同。
各层分配计数/camera envelope/observer分别0B；Editor两个硬门false，不当完整0GC/Android证书。
两次after有Domain Reload正常关闭三残留0、Scene clean/SHA同；原Menu clean/8roots/idle/nonPlay恢复。
A1 dirty-chunk仍未来设计；EXT1/MONO/ATLAS/资源与专项门不解冻。
下一先动态实体/运动/生成退休、首可见/latency/透明像素对照，再高负载/完整链/设备收益。

## 子批16：动态publication显示验证（2026-10-07）

[Task](../../../../docs/ai/TASKS/NTSD-OPTIMIZATION-BATCH16-DYNAMIC-DISPLAY-20261007.md) /
[Change](../../../../docs/ai/CHANGE-RECORDS/NTSD-OPT-M03-DYNAMIC-DISPLAY-016.md) /
[报告](../../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH16-DYNAMIC-DISPLAY-20261007/REPORT.md)。
SCOPED_DYNAMIC_PUBLICATION_PASS；production及父项RUNTIME_PENDING/OPEN。
只扩展原Editor探针，生产三文件/compiled DLL字节未改；正常InputSystem移动/组合技，
不强制step/改帧/HP/MP/位置。原saved Battle自然240tick/308camera、50/50相关回归；
21实际source坐标变化、1183snapshot身份/帧/可见字段同；18新增与16离开publication身份，
最多8实体/12command，所有body来自最新publication、slot generation复用身份分离。
11有body身份首可见publication与body同一观测tick；9 oid518无body不由EntityVisible旗标推导资格，
不判遗漏或全first-visible已闭合。camera采样可能漏中间tick/短生命周期实体；离开publication不等于GPU退休。
307Build=238publication+69alpha，308request一个既有sameUnityFrame gate；397408entity vertex bytes，
0growth/failed/rejected，camera/observer各0B；非新收益A/B、GPUbatch、全chain/设备0GC。
关闭objects/slots/borrowers0、Scene clean/SHA同，原Menu恢复。8当前备份/582保护审计，
EXT1/MONO/ATLAS/Q06body/Scene资源边界保持。
下一同显示样本透明像素/alpha/latency，再高负载、完整链与设备；不重复当前已通过窗口来替代这些门。
