# 第十三批：多子网格元数据批量更新

Task NTSD-OPTIMIZATION-BATCH13-SUBMESH-BATCH-UPLOAD-20261006 / SCOPED_METADATA_AB_PASS；Change NTSD-OPT-M03-SUBMESH-BATCH-UPLOAD-013 / RUNTIME_PENDING。
只减少backend metadata native API交互；不是DrawMesh/GPU合批、顶点上传skip或EXT1实施。
[Task](../../../docs/ai/TASKS/NTSD-OPTIMIZATION-BATCH13-SUBMESH-BATCH-UPLOAD-20261006.md)；
[Change](../../../docs/ai/CHANGE-RECORDS/NTSD-OPT-M03-SUBMESH-BATCH-UPLOAD-013.md)；
[Operation](../../../docs/ai/FILE-OPERATIONS/NTSD-OPTIMIZATION-BATCH13-SUBMESH-BATCH-UPLOAD-20261006/RECORD.md)。
原Editor非Play Menu clean/8roots；原样20case同源码fixture改前后对照已完成，实际结果见下文。
父M03仍OPEN/RUNTIME_PENDING；完整链/真实Scene/像素/1000AI/Android未知。
## 结果与范围

Task SCOPED_METADATA_AB_PASS；生产Change RUNTIME_PENDING（Editor compile/聚焦/局部A/B通过）。
唯一生产hunk：BattleMeshChunk.Upload在既有publishBatch为true之外，
仅desiredActiveSubMeshCount>1且等于retainedSubMeshCount时也批量SetSubMeshes。
数组/descriptor生成顺序及内容、growth/range change强制batch不变；小prefix/单段维持原路径，
避免巨大physical高水位inactive tail每次被整体批量重传。
这只是每chunk metadata API交互的优化，不合并物理segment、不改变DrawMesh/GPU batch、
不跳过顶点上传、不变更ATLAS资源布局/预算，也不是EXT1实施。

## 编译、代码身份与场景

只原Editor PID19040/6401、WindowsEditor/D3D11/RTX4070、Unity2022.3.62f3/URP14.0.11；
CPU沿用本机i5-13600KF/20逻辑核心（本批未重新硬件盘点）。
原Menu前后clean/8roots/nonPlay/idle，CS过滤0error。TestRunner临时untitled scene非生产Battle窗口。
Quality Ultra/Gamma/HDR/MSAA/渲染scale未改，pixel技能只核对管线/保持布局，不套用60Hz或相机设置。
CLI Pipeline要求Unity6，不安装/升级；沿用现有Unity-MCP，零额外Editor/手动场景切换/Play。
生产before SHA5A6E2CE8... / after F5519E8F...，完整身份见
[改前manifest](pre-source-manifest-02.json) / [改后manifest](post-source-manifest.json)。
Editor DLL23:06:39/F6FABB8E...保持，生产DLL22:15:24/2A8A4BA9... →23:08:47/B2B46521...。
public ABI无改，Editor测试assembly未需重编；domain reload后已加载生产新DLL。
baseline fixture、central、Q06 hash-only及两个meta身份保持；未读Q06活跃方法体。

## 实际调用及结果

现有MCP Python transport仅调用本地原Editor，不用于文件读写。实际调用：
- refresh_unity(mode=force,scope=all,compile=request,wait_for_ready=false)，get_editor_state与read_console(error CS)核实重载/编译。
- run_tests(mode=EditMode,testNames=SubMeshEditorTests+BattleMeshUploadBaselineEditorTests)：
  ed521799e0e04d8394b1de51ca854a91，改前26/26 Passed，68.5763836s。
- run_tests(mode=EditMode,testNames=BattleMeshUploadBaselineEditorTests)：
  8fccdaced17f4cb6ae64acc653c1979d，改后20/20 Passed，54.461312s。
- run_tests(mode=EditMode,testNames=七个具名相关fixture)：
  b1dc62692c2a404e8490837bf539aeb1，71/71 Passed，5.4629239s。
- get_test_job(job_id=上述各ID,includeDetails=true,includeFailedTests=true)取得最终summary及输出；
  exec_command若返回session先write_stdin续取，不重启job。
- Tools/Validate-ChangeLedger.ps1、git diff --check、逐文件SHA/链接/优先级计数。

回归71：SubMesh6（新3+旧3）、bounds5、VertexUpload18、detail8、native recovery12、capacity seal16、common atlas binding6。
共117成功case执行、91去重fullName；不能把discovery9242写成运行9242。
[改前原件](pre-tests-result.json) / [改后原件](post-tests-result.json) / [回归](regression-result.json) /
[去重与合同汇总](unique-and-contract-summary.json)。

首次87adbfc11d1645b9a794914db0638778完成25，但两个新增translated bounds-size exact断言失败；
result=null，不推造23/25或保存不可取得的baseline；[失败原件](pre-first-failed.json)保留。
生产尚未改，这是测试浮点断言问题，不是production bug RED。
浮点在新坐标重新计算左右边，尺寸不能与旧坐标逐位相等；改为逐轴2e-5几何容差，
整数index/范围/segment和UV/colors仍精确。新case经旧/新backend两端通过。
重载暂不可用、首次EOF空行与错误独立segment文件定位均留痕；无生产编译/改后测试失败。

## 无诊断 A/B（单位ms）

各case原64warmup/1800sample；同fixture、同Editor、同布局，先完整A再完整B，非随机/交叉/同时配对。
不是自然publication、alpha分布、真实catalog/素材/AI/RenderPass或完整M0。
计时 bracket仅Build（含timestamp成本），读scalar在allocation窗口内，JSON/反射/读回在窗外。
percentile受OS调度/Editor抖动影响，没有脆弱阈值或统计显著性认证。
[全部逐项比较](ab-comparison.json)；[pre20](pre-samples.json) / [post20](post-samples.json)。

| 命令数 | 模式 | workload | before mean | after mean | mean降幅 | p50 before→after | p95 before→after |
|---|---|---|---|---|---|---|---|
| 100 | OrderedChunks | 相同几何 | 0.081377 | 0.070086 | 13.87% | 0.070900 → 0.069300 | 0.114300 → 0.073000 |
| 100 | OrderedChunks | 位置交替 | 0.073787 | 0.070691 | 4.20% | 0.069900 → 0.069600 | 0.104800 → 0.075000 |
| 500 | OrderedChunks | 相同几何 | 0.364778 | 0.343893 | 5.73% | 0.343100 → 0.340900 | 0.524200 → 0.359400 |
| 500 | OrderedChunks | 位置交替 | 0.354174 | 0.351833 | 0.66% | 0.340400 → 0.341000 | 0.474800 → 0.458800 |
| 1000 | OrderedChunks | 相同几何 | 0.738609 | 0.692983 | 6.18% | 0.684600 → 0.681400 | 1.050900 → 0.745300 |
| 1000 | OrderedChunks | 位置交替 | 0.725114 | 0.704167 | 2.89% | 0.683300 → 0.685400 | 1.038800 → 0.870700 |
| 1000 | StrictOrderedDraw | 相同几何 | 1.890220 | 0.996116 | 47.30% | 1.797300 → 0.980900 | 2.542400 → 1.095100 |
| 1000 | StrictOrderedDraw | 位置交替 | 1.877828 | 1.007771 | 46.33% | 1.801800 → 0.983200 | 2.296300 → 1.181400 |
| 1000 | OrderedChunks / 交错variant | 相同几何 | 2.000377 | 1.050542 | 47.48% | 1.867600 → 1.030800 | 2.937400 → 1.176300 |
| 1000 | OrderedChunks / 交错variant | 位置交替 | 1.946045 | 1.042757 | 46.42% | 1.857500 → 1.025800 | 2.554000 → 1.143100 |
| 4097 | OrderedChunks | 相同几何 | 2.971588 | 2.838403 | 4.48% | 2.821600 → 2.803300 | 3.887200 → 3.060600 |
| 4097 | OrderedChunks | 位置交替 | 3.016968 | 2.923905 | 3.08% | 2.832900 → 2.821500 | 4.082000 → 3.891500 |

高segment四控制case平均降低46.33～47.48%，其中约0.87～0.95ms/Build。
单段case走原API路径；控制p50近似保持，其mean波动不计为该优化收益。

## 开阶段计时（单位ms）

coarse/detail同时开启，逐command timestamp/marker有观测税；父子phase重叠禁止直接相加。
metadata包含descriptor生成/遍历及native API，不是纯SetSubMeshes API/GPU时间。
full-active1000段metadata mean约1.159～1.209→0.315～0.340ms，
instrumented Build的mean降幅29.87～40.65%；交错variant相同几何after尾部抖动更大。
1000单段开计时Build mean反而上升2.19～2.92%，如实保留；
该组不走新增batch分支，不能把顺序采样噪声无证据归为收益或证明零回归。
本轮没有重复/随机交叉A/B或Android测量，真实生产复核仍开放。

| 命令数 | 模式 | workload | Build before→after | Upload inclusive before→after | metadata inclusive before→after |
|---|---|---|---|---|---|
| 1000 | OrderedChunks | 相同几何 | 1.060003 → 1.083177 | 0.190178 → 0.196692 | 0.001260 → 0.001224 |
| 1000 | OrderedChunks | 位置交替 | 1.041997 → 1.072453 | 0.187036 → 0.192192 | 0.001022 → 0.001098 |
| 1000 | StrictOrderedDraw | 相同几何 | 2.266964 → 1.345444 | 1.404088 → 0.505957 | 1.209282 → 0.315134 |
| 1000 | StrictOrderedDraw | 位置交替 | 2.192358 → 1.366334 | 1.348254 → 0.510219 | 1.158792 → 0.317488 |
| 1000 | OrderedChunks / 交错variant | 相同几何 | 2.260843 → 1.585470 | 1.359932 → 0.544658 | 1.169367 → 0.339554 |
| 1000 | OrderedChunks / 交错variant | 位置交替 | 2.252632 → 1.414166 | 1.355025 → 0.510473 | 1.163378 → 0.317454 |
| 4097 | OrderedChunks | 相同几何 | 4.339940 → 4.290257 | 0.768452 → 0.784609 | 0.003869 → 0.003060 |
| 4097 | OrderedChunks | 位置交替 | 4.516764 → 4.283155 | 0.789194 → 0.781006 | 0.004434 → 0.003144 |

## 0GC、payload、合同及未验收

有效基线Build72000，warmup2560另记，不含回归内部Build；六phase scalar样本172800。
两端40报告各current-thread managed0B、capacityGrowth0，
20配对的stride/顶点API次数/顶点数/字节数/resolved/segment/chunk/计数投影逐项一致。
1000命令每Build4000vertices/176000bytes/1 vertex API；4097两chunk/16388vertices/721072bytes/2API；stride44。
只prepared backend Build/recorder/scalar局部0B，不覆盖catalog/全表现物化/其他线程/native分配/GPU流量。
新两modecase检查稳定ranges、排序映射、实际Mesh vertices/UV/colors、移动bounds、
64→2→0→3→移动3→1、高水位inert tail、Mesh身份/上传计数；
第三新case检查小prefix高水位0B，已有random bounds/chunk recovery/whole-frame reject/旧read lease保持通过。
无新增持久buffer/queue/lease，原Dispose与11阶段关闭不改，不能据EditMode称Scene生命周期已验收。

专项门保持：EXT1 PROPOSED/MODIFY_REQUIRED且未启动专项M0；MONO USER_HOLD；
ATLAS bank/预算/纹理格式/segment merge/fail-closed不改；PERF/ATLAS正文未改。
逻辑33ms/3ms/Host max2、输入/checksum/pass/RNG的源路径不改，未重新跑正式EXE trace，
不把源不变晋升整场逐位runtime对齐。
未执行Play、BattleRuntimeSelfCheck菜单、真实Battle像素/退出重进、完整M0、1000AI、
Profiler/FrameDebugger/GPUcapture、Player/Android构建或设备热稳态。
父M03与34项高12/中14/低8总体不闭合，关闭父项0；Android仍NOT_CERTIFIED。
下一先真实中央物化/publication-alpha显示窗口、像素/完整链0GC与实际收益验证，
再按新鲜成本选择下一最小优化；不因此解除资源/Scene/专项门。

## 审计状态

九个before准确备份SHA一致，393范围外文件保持；HEAD2cccd597...不变，未暂存/提交/push。
首轮static147links/0missing、diff--check0、compiled七源身份0drift。
最终审计23:14：Validator PASSED/1309 records/6 governed current-diff scripts，
0errors、4268全局历史warnings、本Change/fixture scope0warnings；
160本包/主表交叉引用0缺失、九backup/393保护0drift、七compiled源码/meta0drift，
git diff --check exit0、两脚本trailing whitespace0。优先级34=12高/14中/8低。
Operation只表示编辑审计通过，不把RuntimePending变成VERIFIED；随后after清单冻结并只读复查。
