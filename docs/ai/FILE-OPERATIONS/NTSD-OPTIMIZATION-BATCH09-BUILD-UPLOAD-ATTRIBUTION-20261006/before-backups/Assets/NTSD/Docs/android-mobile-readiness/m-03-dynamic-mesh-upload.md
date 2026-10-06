# M-03 中央动态 Mesh 构建与上传优化方案

> 优先级：高（2026-10-06由中调高，原ID/路径保留）
> 状态：`OPEN / RUNTIME_PENDING / UPLOAD_COUNTER_FOCUSED_PASS / UPLOAD_REPORT_FOCUSED_PASS / REQUEST_COUNTER_FOCUSED_PASS / PROFILING_REQUIRED`
> 最后更新：2026-10-06
> 本轮共同合同与启动门：[2026-10-06复核](../battle-optimization-rebaseline-and-start-gates-20261006.md)；用户已批准普通优化实施，子批06/07/08补中央Mesh上传计数、可选报告和显示request分类，完整M0/专项门不解冻。
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
