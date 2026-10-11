# NTSD-M03-SEGMENT-UV-DIMENSION-REUSE-20261010
Status: IN_PROGRESS
Human-authorized M03 handoff current thread turn 01a12535-e296-7631-935b-d964417ebd67; exact contract artifacts/diagnostics/NTSD-M03-MATERIALIZATION-REUSE-REVIEW-20261010/execution-handoff.txt.

Original WriteQuad reads Texture native width/height for every valid quad. Planned Build canAppend/prepared/OrderedChunks gate with instance default-OFF flag and stack width/height/ready; no cross-Build cache. Clear/Dispose disable flag; existing shutdown stage5 Clear invalidates, no new stop/drain/dependency in other stages. Editor correctness counter owned backend, reset each Build/Clear/Dispose, enabled only via tests, disabled in costs. Test resources owned by fixture finally Dispose, no shared-resource destruction.
Risk: stale segment dimensions/custom resolver resize/float drift/negativecontrol overhead. Validation exactly delegated RED->compile->payload/counters/edges/pixels then one four-layout fullBuild ABBA reliable scoped0GC. First hardfail stops, no rerun; no automatic default promotion.
Rollback: preserve original dirty bytes, remove only task delta after reviewed authorized audit, never HEAD restore/unrelated overwrite. No irreversible battle-rule change.

## Exact delegated contract
继续当前原优化方案M-03（Mesh物化与上传）的一个有界修复任务。直接人类授权是本讨论会话用户要求“你来继续验证M-03：Mesh物化与上传，直到可以发送给那个会话进行修复”，并已有准确指令直接发送约定。以下由讨论会话完成独立评审后发出，不是恢复旧Goal、34项队列、H07或额外优化点。

建议Task NTSD-M03-SEGMENT-UV-DIMENSION-REUSE-20261010，Change NTSD-M03-SEGMENT-UV-DIMENSION-REUSE-001。先读本轮REPORT.md与analysis-summary.json；创建准确Task/Change/Operation、真实dirty字节备份及唯一进度。修改前检查当前Editor/用户工作；只用原Editor，不启动第二实例。原讨论诊断任务收口，不重做三版微测。

问题与证据：BattleDynamicMeshBackend.BattleMeshChunk.WriteQuad每个有效quad读取Texture.width/height计算半像素UV界。同一Build内已有IsCompatible/canAppend严格判断同Texture/Material/variant/binding/page/slice（array例外原样）。既有同fixture旧千人OFF报告89camera平均1992.056命令、segment恒1，说明连续段确实存在；这不是本轮新实景收益。新8项旧契约测试通过；独立UV数学三版结果全保留。最终segment模型单array/单2D段有局部信号，41边界比特等价，512完整微测scope前后校准有效/GC事件0；但三纹理每quad换段负控均值约5.05%开销，超过事前5%界，整体NO_DISCERNIBLE_LOCAL_SIGNAL。不得把该结果写成所有布局通过或生产收益；也不因该风险去做第四版本地微测。本任务用真实backend验证一个避免miss路径额外工作的精确接法。

准确生产改法：仅在已存在canAppend分支、preparedCatalogResolver非空、OrderedChunks、开关ON时启用段内尺寸复用；新段第一quad一律走原WriteQuad路径，不为它执行尺寸缓存查找/写入。第一次真正canAppend且UV正宽高/Texture存活时，才读取本段宽高，后续同段复用。UV半像素公式、除法顺序/float运算完全保持，不缓存整个UV边界（同纹理不同sprite UV仍不同）。换段（含unresolved打断、材质/page/slice/纹理变化、chunk边界）、空frame、新Build和异常都不得沿用旧段尺寸；局部栈状态随Build失效。invalid UV仍按原条件处理，不能把无效值标为已准备。custom resolver（可能Resolve中resize资源）、StrictOrderedDraw、开关OFF保持原读取路径；Texture2D/Texture2DArray原资源内容不改。不要依赖“只有array”、跨Build资源尺寸缓存、两槽LRU或调换命令。复用已有canAppend判断，不再做额外全帧扫描，不新建manager/cache/dictionary，不改resolver/资源配置。

只有一个backend实例候选开关，默认OFF，沿既有backend disposal/reset约定禁用/失效；无static全局开关、World/Host/Suite新配置。普通public构建API保持调用兼容。geometry/vertex计算尽量共用同一实现，避免复制整份WriteQuad；重构范围严格限于支持已准备宽高的私有helper/参数，不修改Upload、Index、segment兼容规则。生产默认推广不在本Task自动实施范围。

准确脚本写域仅三文件：
1 Assets/NTSD/Scripts/Animation/Rendering/BattleDynamicMeshBackend.cs：Build局部段尺寸状态、实例冷开关及WriteQuad半像素段私有接入，必要最小Editor-only只读验证访问器。不得改upload行为、透明顺序、stride44/index格式/容量算法。
2 Assets/NTSD/Scripts/Animation/Rendering/Editor/BattleCatalogCentralResourceResolverEditorTests.cs：复用现有ResolverFixture/真实sealed resolver、新增候选语义/完整backend成本Explicit入口；可在本文件为测试补足page/尺寸变更夹具，不改生产resolver或catalog。
3 Assets/NTSD/Scripts/Test/Editor/BattleCentralSameSamplePixelEditorTests.cs：新候选实际prepared resolver的同样本像素覆盖，复用已有像素oracle。原CustomResolver路径通过只证明fallback，不得当新候选命中证据。
治理文档/报告/请求/备份可准确新增。原M03MeshUvDimensionReuseDiagnosticsEditorTests.cs、BattleScopedGcAllocationRecorder.cs、GC observer不修改。API/fixture确实无法在三域满足时先交回准确缺口，不静默扩大生产写域。

先最窄RED/GREEN与原Editor编译：证明开关OFF及各fallback保留；真实prepared catalog命中一个长段且实际getter准备次数由有效quad数降到有限次（第一次quad原读+首次append读取），计数仅正确性诊断、不进成本窗。比较完整原/候选vertex payload（Position/Color/UV/AtlasSlice/UvSampleBounds所有float位）、index、submesh及segment/bounds/diagnostics、输入publication完整内容未改；不以仅Vector4 helper代替。Texture2D、array、page模式；same texture不同UV/color/flip/slice；invalid/null/destroyed；首quad UV无效而后续有效；材质等断段、unresolved后同纹理、strict/custom resolver、custom中途resize；Build之间resize/replacement/dispose/rebuild；0/1/4096/4097和先大后小、恢复原生Mesh的相关边界。非有限位置原fail-closed与容量行为保留。必要像素ON/OFF相同alpha0/.25/.5/.75/1、重叠透明顺序与负控制，既有source/array/page oracle可复用；像素测试不宣称0GC。

前置通过后仅一次完整backend成本组，四布局各OFF→ON→ON→OFF：4000quad连续SourceTexture2D、4000quad连续TextureArray、4000quad三纹理每quad换段OrderedChunks、同混合StrictOrderedDraw。固定相同frame/resolver/资源/预备容量、每窗8warm+32sample；所有生产内部细分计时关闭，外层Stopwatch只包住完整Build（含native Mesh upload），不含fixture/Configure/日志/assert/readback。原生Mesh和CPU数组先按正常PrepareCapacity准备，不改变warm次数。真实sealed resolver必用，不能只测自制UVhelper。使用现有BattleScopedGcAllocationRecorder同步主线程围住完整测量调用，前后正/空校准有效，scope无wrap/saturation/invalid，事件必须0；GC.GetAllocatedBytesForCurrentThread在本Editor明确校准不响应，不得用其0值证明0GC。raw单位不是bytes；此为CPU Build/Upload范围，不是GPU/Driver/Player证书。

事前结论门：两连续布局各两个ON mean均低于两个OFF mean，ON最坏P95/P99不高于OFF最坏值；两个fallback/fragmented负控各ON最大mean/P95/P99不得超出本布局OFF最大值，不能用其它布局平均抵消退化。完整payload/像素/计数/容量与GC门全过才可建议限定准入，仍只CPU局部。任一正确性/GC/容量/异常清理硬门失败停止剩余窗并保留原件；无可辨收益/负控退化则保持OFF并具名结束此版，不重复同版找PASS，不追分配/改JIT/加warm/Profiler。已通过测试不机械全量重跑。默认开关不自动开启，不追加实景1800/完整Driver/Android/GPU/发布构建；若要下一步实景接线，先交本组完整结果供讨论会话独立审查。

收尾：独立实施说明记录原代码→新代码→解决什么→实际结果与未证范围，报告/原数据/源码与程序集指纹、dirty保护、原Scene Menu clean8/临时Texture/Material/Mesh/RT全部自有释放，原shared资源不销毁；沿既有关闭，不改变十一阶段。运行Tools/Validate-ChangeLedger.ps1及scoped diff-check，更新Task/Change/STATE/handoff/唯一M03进度后停止。M03父项仍OPEN，原H07~12ms未解决/暂缓、producer/95 OFF、H06不续、旧Goal取消，33ms/确定性/publication只读/透明segment/fail-closed/容量0GC及后期设备门不降低。不改Scene/Prefab/资源/Settings/InputActions/Server/Gen/Plugins，不解冻ATLAS/Mono/PERF/EXT1或上新渲染系统。此次无需读活跃Q06体。
