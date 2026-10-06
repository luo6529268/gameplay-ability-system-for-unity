# 第19批：真实项目纹理高命令数CPU提交桥报告

结论：SCOPED_REAL_TEXTURE_CPU_BRIDGE_PASS（限定受控CPU桥）；M-03/H-11父项仍OPEN/RUNTIME_PENDING。仅新增Editor fixture与meta，无生产脚本/DLL修改，不报告新优化收益。

## 已执行与失败留痕

原Unity2022.3.62f3/PID19040/6401，URP14.0.11/Ultra/Gamma/D3D11/RTX4070。原Menu clean8roots/idle/nonPlay，使用现有EditMode Runner；Runner临时untitled场景不是原Battle自然窗口。没有主动Scene load、Play、Scene保存、第二Editor、Profiler/FrameDebugger/GPU capture、完整M0或设备测试。
refresh_unity(force/all/compile=request)后get_editor_state idle/不编译，read_console(error CS)0；[编译](compile-state.json)/[启动状态](editor-preflight.json)。

初job d70510b5287c4498bc1064e99cee388c：7项完成，作业failed；六性能窗口的内部断言均PASS，但交错1000案例NUnit捕获MCP Client handler NetworkStream disposed未预期Error log。
原因证据：[只读中途查询超时](test-job-poll-timeout.json)与[初作业唯一失败日志](focused-initial-job.json)；socket timeout后客户端断开，Editor仍在同步采样，晚到响应导致连接已释放日志。没有源码断言失败；本批没有屏蔽LogAssert或清除失败原件。
仅修正观测方式：运行期间不查询Editor。补跑唯一受日志影响case＋54相关回归，job42f2b585714c40bba418d87fb5b425d9 **55/55 Passed，0failed/0skipped，11.7232852s**；[完整原件](corrected-and-regression-job.json)/[准确过滤条件](test-dispatches.json)。
初五其它性能case及独立overflow/held-lease case不重跑；不把初job写成7/7 PASS，也不把9269 discovered当实际执行数。
既有MCP TestJobManager.cs:534-544只在Succeeded时序列化Result，因此初failed作业result=null，不自行补写缺失leaf payload；progress.completed7/failures_so_far1与每窗结果分开保留。

## 采样结果

两actual Unity imported Texture2D来自项目现存Naruto/Sasuke BMP，各800×560/RGB24，instance identity221522/221524；source/meta SHA在各原件中，不改asset/importer、也不销毁shared texture/shader。
它们不是临时合成8×8纹理，但不等于生产DAT decoder/crop/catalog路径；本测试UV覆盖整sheet、quad64×64、预先给定命令序列及相邻motion，不冒充角色帧语义或实际AI。
每组64warm＋1800samples；六唯一接受窗口 **10800 samples/384warm**。含日志影响旧窗与补跑，实际执行7性能窗 **12600 samples/448warm**；旧受日志影响窗不作接受基线，但原件不删除。
所有接受窗：当前线程managedAllocatedBytes0、capacityGrowth0、remainingCpuLeases0、source/Scene不变，两slot独立frozen frame和Mesh身份稳定，vertex stream0实读stride44。
alpha在0/.25/.5/.75/1循环，每次capture仍由同一预排序publication复制；这故意强制Build，不从测试循环推导生产同样本去重率或每帧重建频率。

| 命令 | 模式/绑定 | chunk / physical segment | CPU桥 mean / P95 ms | capture+motion mean | Build+upload mean | 录制 mean | Execute API mean | 原件 |
|---|---|---|---|---|---|---|---|---|
| 100 | OrderedChunks / 连续纹理 | 1 / 2 | 0.4317 / 1.0240 | 0.1963 | 0.2022 | 0.0116 | 0.0178 | [原件](run-241e22dfe4054e04ba8bfd09ec3b8a21/result.json) |
| 500 | OrderedChunks / 连续纹理 | 1 / 2 | 1.6982 / 2.8384 | 1.0057 | 0.6250 | 0.0197 | 0.0406 | [原件](run-43e842e98a4a44158a56542c5981d3db/result.json) |
| 1000 | OrderedChunks / 连续纹理 | 1 / 2 | 2.7375 / 4.3950 | 1.6481 | 1.0232 | 0.0185 | 0.0401 | [原件](run-341271539d7e4208aa232289bc7a73a1/result.json) |
| 1000 | OrderedChunks / 交错纹理 | 1 / 1000 | 4.8768 / 5.6408 | 1.4783 | 1.2303 | 1.3454 | 0.5354 | [原件](run-2be0b2c98b32443cb9f569820bafd065/result.json) |
| 1000 | StrictOrderedDraw / 连续纹理 | 1 / 1000 | 5.1959 / 8.0566 | 1.5533 | 1.2788 | 1.4510 | 0.5993 | [原件](run-83dfd593c02541289e3d7c61a3885b1d/result.json) |
| 4097 | OrderedChunks / 连续纹理 | 2 / 3 | 10.0096 / 13.3890 | 6.1449 | 3.7487 | 0.0327 | 0.0684 | [原件](run-77254764c6af4c6baa54bd0b99508593/result.json) |

各阶段只记录该CPU桥方法段，含Stopwatch观测税、Editor/native API可能等待；总桥还含Publish/lease/record/退租/Pool释放与标量采样，不把阶段直接相加当整帧，也不当GPU耗时。
1000命令每sample4000vertex/176000entity vertex payload bytes，两段和1000段上传字节相同；4097命令每sample16388vertex/721072bytes、2API/3segments，4096仍仅chunk尺寸。
中央CommandBuffer.DrawMesh计数=有效physical segment；一sample一次测试Graphics.ExecuteCommandBuffer。这里没有生产RenderPass/ScriptableRenderContext.ExecuteCommandBuffer或benchmark-local Graphics.DrawMesh，也没有全帧Profiler/GPUbatch/SetPass统计。
两次交错1000 mean5.2956→4.8768只运行波动/补跑，不是生产优化A-B或收益。

## 新代码、热窗与生命周期

[fixture](../../../Assets/NTSD/Scripts/Test/Editor/BattleRealTextureSubmissionEditorTests.cs)496行（SHA06CCF6376BA6A9E36B06B179F7921A82644234563089580C21E59C7137F1DB60）；新meta GUID e59bb7475680487bafb9263644165829。
准确热窗：submission CaptureFrame→DisplayMotion Prepare/Apply→Mesh Build/upload→Foot/Health disabled空构建→Publish→TryAcquire→CommandBufferPool/Get/DrawMesh→Graphics.ExecuteCommandBuffer→计数Record→归还buffer/read lease→Retire，及固定timing array/标量累加。startup资产加载/reflection/typed delegates/JIT、warmup、断言/readback/排序/JSON外置；没有热扩容/局部截断。
snapshot、motion、backend与submission分别预备/seal；slot command上限=count（不是chunk4096），overflow拒绝整份capture且旧good数据保持，retired仍held lease时拒绝capture直到退租。
无新production owner/worker/queue/lease模块，无关闭11阶段重排。finally退租/退休、dispose自建三个backend/material/RT、恢复原RenderTexture.active，不销毁项目texture/sharedshader。
CPU lease0与Graphics.Execute返回不等于GPU消费完成；此处仅沿用Unity Mesh/CommandBuffer生命周期，未闭合未来instance buffer的fence合同。
辅助Foot/Health只禁用空构建：没有覆盖实际活动辅助几何/全部表现类别。纯prepared当前线程1800同步样本0B不是1800真实显示帧、生产RenderPass/PlayerLoop/其他线程/native/Player/Android0GC；父硬合同不放宽。
只消费公开Frame/motion/backend/submission接口；不读取或修改活跃Q06排序body，不新闭合first-visible/透明重叠/pixel/latency或formal checksum。本批没有Simulation tick/AI/RNG执行，不能因此声称1000AI逐位回放认证。

## 审计与进度

Tools/Validate-ChangeLedger.ps1 使用pwsh/显式RepositoryRoot执行：exit0，1315Records/11governed code files；本新fixture被本Change覆盖、当前Change警告0。4260条为既有历史Record声明文件不在当前diff的警告，本批不清理；git diff --check exit0，LF/CRLF提示不当编译/测试error。新fixture全文人工复核、七文档对照当前字节before仅定向进度/留痕追加，未发现新增阻断项。

7当前字节备份、679保护SHA零漂移，正式336B44、Scene/资源/Settings、PERF/ATLAS/EXT1正文保持；生产DLL A49C69CE527D7E0268661F96F6A5A28B0C79E6204872BDA6EAF1A1930270AE8D，EditorDLL3F506A83F28122ED6E9A8949CED20A863977DF44329DC622C57F7440ADBA558A。
最后原Menu clean8roots/idle/nonPlay/test不运行、error CS0；HEAD2cccd597dbf6cb32826eb0b6e98d7462c1eac48d/staged空，未删除/移动/Git丢弃/commit/push/覆盖旧输出。
[最终审计](final-validation.json)/[Operation](../../../docs/ai/FILE-OPERATIONS/NTSD-OPTIMIZATION-BATCH19-REAL-TEXTURE-SUBMISSION-20261007/RECORD.md)/[汇总](window-summary.json)保存结果、失败与窗口范围；文件冻结不关闭父项。
进度34=高12/中14/低8，父关闭0；EXT1 PROPOSED/MODIFY_REQUIRED无专项M0、MONO USER_HOLD、ATLAS bank/budget/format/segment/failclosed未解冻。
[Task](../../../docs/ai/TASKS/NTSD-OPTIMIZATION-BATCH19-REAL-TEXTURE-SUBMISSION-20261007.md)/[Change](../../../docs/ai/CHANGE-RECORDS/NTSD-OPT-M03-REAL-TEXTURE-SUBMISSION-019.md)。

## 下一优先与未知

本受控桌面链中snapshot+motion随规模增长，1000约1.48～1.65ms，4097约6.14ms；高segment录制约1.35～1.45ms、Execute API约0.54～0.60ms。只定位候选成本，不从合成snapshot结构/整sheet binding推导生产瓶颈，也不因CPU DrawMesh多就称GPU batch多。
下一优先将真实生产publication/catalog/活动辅助和实际RenderPass接到有界高负载/完整0GC验收，逐段确认snapshot/motion与录制成本后才选最小优化；不继续重复本组六case，不无证据改Q06/排序/slot/fence架构。
1000AI、120FPS、真实GPU batch、屏幕latency、ARM64/IL2CPP与Adreno/Mali持续性能仍未知。
