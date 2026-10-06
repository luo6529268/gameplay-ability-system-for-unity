# 第十一批中央 Mesh backend 受控基线（限定通过，父项开放）

> 最终：SCOPED_BACKEND_BASELINE_PASS；Change VERIFIED（Editor fixture限定）；M-03 OPEN/RUNTIME_PENDING。
> 新12/12、相关旧26/26，共38去重Passed。以下COMPILE_PASS/执行中段落为启动时留痕。

Task NTSD-OPTIMIZATION-BATCH11-MESH-BACKEND-BASELINE-20261006；Change NTSD-OPT-M03-MESH-BACKEND-BASELINE-011 / COMPILE_PASS。
本批只新增Editor fixture，生产源码哈希保持；原Editor测试DLL22:33:37晚于fixture22:33:31，
最新Console error-CS0，原Menu clean/8roots/非Play。
12 cases ×64预热＋1800采样，在当前backend上实际执行Build/SetVertexBufferData；
不是真实publication/插值/资源/RenderPass/GPU/1000AI/完整M0。
采样job 1b8bb2c61a514d7b8450b95b60f9fa6c 已启动，未返回PASS或收益，原结果后追加。
首次NonParallelizable、随后descriptor参数两次fixture compile失败保留并已最小纠正；
scripts-only请求未导入新fixture且DLL旧的事实保留，不作为编译证据。

## 实际结果：SCOPED_BACKEND_BASELINE_PASS（父项未关闭）

原Editor job 1b8bb2c61a514d7b8450b95b60f9fa6c：12/12，通过/失败/跳过12/0/0，
实际summary duration 29.8549795s；共21600个采样Build，768个预热Build不计入样本。
相关submesh/segment-bounds/vertex-upload job a841f7adcfca424da9ec6a1c56d5da28：26/26，
实际duration 1.9036779s；两job合计38个fullName去重Passed（不是全项目9231项）。
[原始结果](baseline-result.json) / [12组结构化样本](baseline-samples.json) / [回归原件](regression-result.json)。
最终test source SHA 8479E060ABAFD0E1E7F9BA95437A1E2BFD08DE09A58CFAAD8574F5BAABDE2ABE，
Editor DLL22:33:37晚于source22:33:31，CS0；生产DLL仍22:15:24/生产源SHA保持，未改生产。

具名环境：Windows Editor，Unity2022.3.62f3、URP14.0.11、Direct3D11、
NVIDIA GeForce RTX4070，Intel i5-13600KF（14核/20线程），Gamma/Ultra/renderScale1/HDRtrue/MSAA1保持。
测试前后原saved Menu clean/8roots/非Play；测试样本scenePath为空，
表示Unity EditMode Test Runner的临时未命名测试场景，不是Menu/Battle生产场景测量。
fixture未调用Scene加载/保存/切换、无GameObject/相机创建；仅backend本地Mesh using Dispose，
没有生产World/Registry/槽/lease/GPU draw，未验证真实11阶段关闭/重进。

## 当前代码局部基线

每case64预热后seal，1800采样，位置交替为预建frame的X偏移0.25；
命令预先排序且resolver固定synthetic Resolved/null Texture/Material，不能当真实素材/人物。
CPU elapsed只包围Build（含timestamp开销），managed scope另外包含标量累加；
setup/prepare、断言/场景状态读出、JSON输出均不在采样窗口内。
percentile使用排序后nearest-rank，不是整帧/present/GPU time，未设CPU通过阈值。

| 命令 | 模式 | 位置交替 | variant交错 | segment/Build | API/Build | bytes/Build | p50 ms | p95 ms | p99 ms | max ms |
|---|---|---|---|---|---|---|---|---|---|---|
| 100 | OrderedChunks | false | false | 1 | 1 | 17600 | 0.0714 | 0.0770 | 0.1095 | 0.2522 |
| 100 | OrderedChunks | true | false | 1 | 1 | 17600 | 0.0714 | 0.0782 | 0.1249 | 0.3422 |
| 500 | OrderedChunks | false | false | 1 | 1 | 88000 | 0.3568 | 0.5480 | 0.6109 | 1.1224 |
| 500 | OrderedChunks | true | false | 1 | 1 | 88000 | 0.3517 | 0.4404 | 0.5634 | 1.0002 |
| 1000 | OrderedChunks | false | false | 1 | 1 | 176000 | 0.7001 | 0.8464 | 1.0543 | 1.3513 |
| 1000 | OrderedChunks | true | false | 1 | 1 | 176000 | 0.6974 | 0.7661 | 0.9323 | 1.0684 |
| 1000 | StrictOrderedDraw | false | false | 1000 | 1 | 176000 | 1.7867 | 1.9230 | 2.4471 | 2.9866 |
| 1000 | StrictOrderedDraw | true | false | 1000 | 1 | 176000 | 1.7868 | 1.9130 | 2.4555 | 6.7659 |
| 1000 | OrderedChunks | false | true | 1000 | 1 | 176000 | 1.8494 | 1.9746 | 2.4524 | 3.2380 |
| 1000 | OrderedChunks | true | true | 1000 | 1 | 176000 | 1.8470 | 2.0134 | 3.1717 | 3.8352 |
| 4097 | OrderedChunks | false | false | 2 | 2 | 721072 | 2.8476 | 3.0705 | 4.2106 | 4.8907 |
| 4097 | OrderedChunks | true | false | 2 | 2 | 721072 | 2.8521 | 3.0295 | 4.1043 | 4.4041 |

所有12组实际stream0 stride44（4097两chunk各44）、当前线程managed0B、
CapacityGrowthCount0，Mesh引用/InstanceID/stride保持，实际resolved、API数/顶点/bytes及segment均复算通过。
仅实体Mesh Build/upload局部；不覆盖publication捕获/真实catalogResolve/插值/Foot/Health/
RenderPass/录制提交/模拟AI/全链0GC或Android。

## 发现、推断和下一步

已测：1000命令兼容Ordered p50约0.70ms；同数量/同176000 bytes/Build，
Strict1000物理segment p50约1.79ms、Ordered交错variant1000segment约1.85ms。
该受控单次运行揭示分段相关CPU成本；不是“draw越少GPU必然越快”，未执行DrawMesh/GPU。
差异涵盖segment生成/bounds/submesh native元数据等，尚不能把全部差异归因某一API；
不凭本次局部median宣布2.6倍普遍收益，不跨chunk合并/改变透明顺序。

已测：强制直接进入Build的完全相同frame仍上传有效顶点全范围；
4097命令实际两chunk/两API、每Build721072 bytes，不是slot容量或GPU流量。
重要：本fixture绕过中央queued同样本门；生产已存在same-sample reuse，
不能据此宣称生产重复显示每帧都会Build。publication/alpha自然比例仍未知，报告43counter未被伪写。

下一小批先复用现有phase timing拆分高segment的Resolve/Write/Upload元数据成本，
再选不改变segment/排序/UV/fail-closed的最小A/B；dirty-chunk/A1仍未来设计。
真实生产基线需另具名原Battle运行窗口/真实素材/workload；本结果不解除该门、
PERF/ATLAS/EXT-1专项M0、MONO、bank/格式/预算/Scene/Settings/Server门。
本batch是受控基线交付，不是生产算法优化完成/收益、120FPS、1000AI或Android认证。

## 审计与失败记录

七个dirty文档before备份逐一核SHA，324范围外文件（含前十批/Scene/settings/活跃Q06 hash-only）受保护；
HEAD2cccd597保留。没有Git add/commit/push/reset/checkout/clean/stash或文件删除/移动。
已有源码只读，新增fixture/.meta；技能用于原Editor验证/管线识别，不套用60Hz/HDR/camera/importer默认。
CodeDom execute_code因长命令失败、unsupported get_pipeline_info、scripts-only未导入、
NonParallelizable与descriptor参数两次fixture编译失败均留原件并最小纠正；
这些不是生产RED，未修改工具/依赖或将失败擦除成首次成功。
最终Ledger、链接/diff、编译身份/保护/备份/after按实际核对另追加；恢复仍需另授权。

## 最终交付核对

Tools/Validate-ChangeLedger.ps1 exit0：1307 Records、4 governed files、4275全库warning、0error；
本新Change/fixture匹配warning0，未扩大处理全库历史警告。原始状态及warning摘录见ledger-validation.json。
22:39:55静态：324保护/7before备份SHA零漂移，138局部文档链接缺失0，
34项高12/中14/低8、git diff --check exit0、fixture尾随空白0，5个compiled manifest文件身份未变。
原Editor22:39最终idle/Menu clean/8roots/非Play/error-CS0，无额外Editor/Play/Profiler/完整M0/GPU/设备测量。
仅本fixture/meta与七个具名文档及本批审计证据；准确after保存于Operation，之后只读核对。
没有改变资源/layout/bank/预算/segment/fail-closed、33ms/3ms/Hostmax2、规则/pass/RNG/checksum或关闭顺序。
