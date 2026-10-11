# M03 段内纹理尺寸复用：有界候选结果

结论：COMPLETED_NO_ADMISSION / NO_DISCERNIBLE_FULL_BUILD_GAIN_DEFAULT_OFF。候选实现和聚焦正确性、像素、可靠完整Build 0GC均通过，但四布局联合收益门未通过，本版不采用，不重复测量、不推广默认。Change ABANDONED表示停止采用，默认OFF代码及全部证据保留，不是回滚或删除。
Task/Operation：NTSD-M03-SEGMENT-UV-DIMENSION-REUSE-20261010；Change：NTSD-M03-SEGMENT-UV-DIMENSION-REUSE-001。

## 代码前后与范围

仅三脚本：
- BattleDynamicMeshBackend.cs：Build只在既有canAppend、真实sealed prepared resolver、OrderedChunks、实例开关ON时准备/复用本段Texture.width/height。首quad仍实际读取；首次有效append再读取一组，后续同段复用两个int。不缓存整个UVbounds，不重排command。私有WriteQuad共用原geometry/vertex写入，保留0.5f除法与Mathf.Min次序，44字节stride/Index/Upload/兼容判定/容量算法未动。
- BattleCatalogCentralResourceResolverEditorTests.cs：复用原ResolverFixture，构造真实catalog/Source2D/array/page夹具；加入完整native payload/metadata/publication/边界/getter计数及一次Explicit完整Build成本工具。
- BattleCentralSameSamplePixelEditorTests.cs：复用MakePublication/DisplayMotion/独立quad像素oracle，增加真实prepared候选命中与ON/OFF像素对照。

实例开关默认false、无生产调用启用；Clear/Dispose关闭。宽高/ready归每次Build栈，换段/新Build/空frame/异常返回后不跨调用保留；unresolved关闭当前段，下一resolved的!canAppend重置ready。自定义resolver、Strict、SingleMeshDiagnosticOnly及OFF保留逐quad读取。原owner在关闭stage5 Clear时失效；其他十阶段顺序与依赖无新责任。Editor getter计数按Build/Clear/Dispose清零，仅测试启用，成本关闭。无static缓存/World配置/新manager/资源改动。

## 原Editor测试

原Unity2022.3.62f3 / PID39108 / TCP6402。全程EditMode，未启动第二Library实例、未Play/保存Scene、未读Q06方法体。未重新执行FindEffect/H06或三版UV数学微测。

| Job | 覆盖 | 结果 |
|---|---|---|
| f38ce7c846a24caf9cffb74c9ae3b5ad | 缺开关RED | 1项预期FAIL，missing flag原件保留 |
| 859b7b0be10f41088fc92c1027bfcb6c | 新候选语义/边界 | 8/8 PASS |
| f76115c4fce84c128fef4420c1cdcf98 | 加强恢复、prepared invalid、三绑定像素 | 5/5 PASS；恢复用例取代前版较弱断言，不重复算独立case |
| f8e89b00db6c4cfb823aed1b43e741e4 | 唯一完整Build成本组 | 1/1 NUnit PASS仅执行及硬门，收益联合门FAIL |

原程序集mtime晚于相应源码，实际新方法执行通过；source-assemblies-before-cost.json与audit-and-analysis.json核两DLL/三源码及resolver/catalog/recorder成本前后指纹不变。一次状态查询在domain reload中ConnectionRefused，未派测试；ready后才执行，无代理重启Editor。

语义覆盖：
- Source2D/array/page各0、1、64、4096、4097命令；完整active vertex原始字节（Position/Color/UV/AtlasSlice/UvSampleBounds所有位）、UInt16索引、全部submesh/segment/bounds/diagnostics相同；frame全部实例字段/数组/值类型位不变。引用型catalog/manager检查身份，不声称外部可变对象图深拷贝。
- 正常64/4096长段quad尺寸读取对数2（首quad1+首有效append1），4097跨chunk为3；单条1。OFF逐有效quad；Strict/SingleMeshDiagnosticOnly/custom原路。counter只计quad尺寸读取对，不代表resolver全部getter。
- 同纹理不同UV/color/flip/array slice等价；array slice可同段、page换页断段；混合array/source材质/纹理/绑定变化、unresolved后同纹理断段。
- catalog无效UV首条（零、负、NaN）fail-closed，后三条valid实际解析3/读取2；prepared本来不能返回非法binding，不绕过它伪造命中。custom原路另验invalidUV/null/destroyed/first invalid then valid、tiny UV和Resolve中途resize。
- Build间resize/catalog replacement、大到小到空再大、Clear/Dispose、native Mesh销毁后正常PrepareCapacity恢复。加强版明确4097 resolved/两chunk/3次读取，修正前同名测试可能因旧catalog identity被两边拒绝而比较空结果；该弱覆盖已被新鲜加强证据取代，未改生产。
- seal容量前置拒绝保留前态、NaN位置原ArgumentException及Clear恢复、custom异常退出后Clear/rebuild均通过。

像素三绑定×alpha0/.25/.5/.75/1×OFF/ON共30次96x96对照，共276480像素，最大通道误差0，最少内容像素1147；alpha.5反序负控制分别814/819/814像素有差异。每次实际prepared候选读取2组、OFF4组，不拿custom fallback冒充命中。publication/captured frame保持。PNG/result.json在pixels-*；临时Texture/Material/Mesh/RT由测试owner finally销毁，未碰共享项目资源。像素测试无0GC主张。

## 唯一完整Build含native上传成本

真实sealed resolver，同layout固定同frame/resources/capacity，4000quad，每窗8warm+32sample；唯一OFF→ON→ON→OFF，总16窗512sample。正常PrepareCapacity/SealCapacity，内部细分计时null、正确性counter禁用；外层Stopwatch完整Build含native Mesh upload，不含fixture/Configure/断言/日志/readback。GC同步主线程覆盖整个Build调用；成本期间未并发shell哈希/validator。没有Profiler捕获、JIT准备或warm变化。

前后各1MiB正例/空例校准通过；512scope全部valid/calibratedBefore/无wrap/saturation，事件全0。单位TimeNanoseconds，rawValueTotal不是bytes，事件不是回收停顿；未用已知不响应的GetAllocatedBytesForCurrentThread。每布局四窗payload SHA一致，CPU数组身份/容量/原生Mesh身份、publication不变，growth0。

原数据full-build-cost.json及create-new full-build-cost.started保留，禁止同版重跑。单位ms/完整4000quad Build；P95/P99从32原ticks排序线性插值，独立Python复核全部数值：
| Layout | Switch | Mean ms | P95 ms | P99 ms | GC events |
|---|---|---:|---:|---:|---:|
| continuous-source2d | OFF1 | 4.948259 | 6.030820 | 7.210477 | 0 |
| continuous-source2d | ON1 | 4.797206 | 6.079965 | 6.928160 | 0 |
| continuous-source2d | ON2 | 4.776253 | 5.719685 | 6.750585 | 0 |
| continuous-source2d | OFF2 | 4.978794 | 6.488695 | 7.964425 | 0 |
| continuous-array | OFF1 | 4.757259 | 5.015480 | 5.055782 | 0 |
| continuous-array | ON1 | 4.987853 | 6.455935 | 6.903822 | 0 |
| continuous-array | ON2 | 4.679184 | 5.836470 | 6.759946 | 0 |
| continuous-array | OFF2 | 4.917887 | 5.771070 | 6.001494 | 0 |
| fragmented-source2d | OFF1 | 6.163831 | 7.408400 | 8.872454 | 0 |
| fragmented-source2d | ON1 | 6.000788 | 6.183855 | 6.236099 | 0 |
| fragmented-source2d | ON2 | 6.053922 | 6.396140 | 6.640852 | 0 |
| fragmented-source2d | OFF2 | 6.084891 | 6.405050 | 6.559456 | 0 |
| strict-mixed-source2d | OFF1 | 6.071622 | 7.469085 | 8.387514 | 0 |
| strict-mixed-source2d | ON1 | 5.903206 | 6.151850 | 6.334017 | 0 |
| strict-mixed-source2d | ON2 | 5.790231 | 6.026440 | 6.071236 | 0 |
| strict-mixed-source2d | OFF2 | 6.030822 | 6.900450 | 7.915421 | 0 |

事前门不变：
- 连续Source2D两ONmean低于两OFF且maxON tails<=maxOFF，PASS，只说明此局部窗口信号。
- 连续array ON1 mean4.987853高于OFF4.757259/4.917888；maxON P95/P99=6.455935/6.903822超OFF5.771070/6.001494，FAIL。array OFF自身mean差3.320428%，ON差6.386005%，不能用ON2较快或合并均值接近宣布收益。
- fragmented/Strict负控各maxON mean/P95/P99未超自身maxOFF，PASS。Strict不走候选，观察更快不能归因候选。
- 联合门需四布局全过，故不采用，不自动改为source-only准入、不追加窗口/新版本，不将微测收益写成生产收益。

## 命令、保护与未执行

实际MCP：refresh_unity(scope=scripts,compile=request,wait_for_ready=false)；run_tests(mode=EditMode,testNames为上表具名方法)；get_test_job(job_id,include_details=true,include_failed_tests=true)；get_editor_state；manage_scene(action=get_loaded_scenes)。原响应在本目录，编译/测试均在原Editor实际执行。
Shell：git status/diff；Python真实dirty备份/SHA/原ticks重算；pwsh -NoProfile -File Tools/Validate-ChangeLedger.ps1 -RepositoryRoot I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity；git diff --check -- 三源码及具名治理。最后两项结果在末尾追加。

2366个NTSD脚本/Scene/ProjectSettings/Packages保护文件无漂移；11份实际dirty before备份SHA全同；HEAD/staged保持，只三准确脚本增量。最初全Assets只读hash范围过广，停止本任务Python7380后改为上述范围，未删除/修改资产，不声称全仓资源逐字hash覆盖。共享Temp测试XML替换前备份，旧失败/中间版本保留。末Editor idle/非Play/非测试/非编译、唯一Menu clean8同前。无reset/checkout/clean/stash/push、删除/移动、恢复旧Goal。

复用：独立评审已过8项publication/物化/显示去重契约和未改Upload/Index/排序历史证据；用本候选命中域完整native载荷/独立像素补门，不重跑全库。没有把三版微测替代完整Backend；旧25/82/H06/91/93/95各scope不互换，旧Driver失败不因本scope0GC被抹去。

未执行且不声称通过：生产默认启用、正常App实景接线/1800正式长窗、全Driver逐tick双RNG/checksum与0GC、GPU/Player/Android认证。局部Build不是整帧/tick，不能外推H07输入约12ms或33ms。M03父项OPEN，H07暂缓/约12ms未解决、producer/95 OFF、H06不续、冻结专项及旧Goal取消保持。

下一步：本有限候选结束，原讨论会话可独立审查结果；未发消息或恢复自动任务，不自动开始H08/H10或array-only新路线。

Final governance: Validate-ChangeLedger PASSED exit0; 4358 historical warnings retained, scoped git diff --check exit0 (newline warnings only); authored-source trailing whitespace0 and implementation-card links valid. No further code edits or performance executions.
