# 第17批同显示样本透明像素验收报告

结论：SCOPED_SAME_SAMPLE_PIXEL_PASS。新增Editor夹具6/6＋既有54/54，60去重具名Passed，0失败/跳过。只完成M-03当前backend同显示样本的限定正确性出口，无新production改动、无新性能收益。父项M-03/H-11保持OPEN/RUNTIME_PENDING，Android未认证。

## 实际执行与结果

原Editor PID19040/localhost6401，Unity2022.3.62f3，当前URP14/Gamma配置，非Null桌面GPU。未启动第二Editor、Play、Profiler、FrameDebugger、性能GPU capture或专项M0；TestRunner临时场景属框架自动执行，未主动改Scene。技能要求先确认实际管线、只修已证问题；本批未发现必须生产修改的首差，因此保留原UV/PPU/camera/importer/44-byte backend stride。

- refresh_unity(mode=force, scope=all, compile=request)，正常domain reload后read_console(error CS)0。一次reload retry没有启动测试。
- run_tests(EditMode, NTSD.Test.Editor.BattleCentralSameSamplePixelEditorTests)，job 02ff380553534cf69405f591bb79c52e：6/6 Passed，1.9125128s（测试时长非性能收益）。
- run_tests(EditMode, 已登记精确八组选项)，job4d24bb88b9034ecdb28a3e355428c69b：54/54 Passed，2.1449858s。
- get_test_job完整新鲜原件：[像素](pixel-test-result.json) / [回归](regression-test-result.json)；发现总数9262不是已执行数。
- 原Menu恢复clean/8roots/idle/nonPlay、编译未忙/CS0；[终态](editor-final-state.json)。
- git diff --check最终exit0。Validate-ChangeLedger最终使用pwsh -NoProfile -File Tools/Validate-ChangeLedger.ps1 -RepositoryRoot I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity，exit0/1313Records/10governed脚本覆盖、本Change0warning。全库历史warning不属于新失败。初两次Windows PowerShell5默认参数PSScriptRoot为空exit1，验证未开始；未改validator，仅使用现有PowerShell7并显式RepositoryRoot，保留[执行证据](validation.json)。

## 像素/取样覆盖

| 维度 | 实际覆盖 |
|---|---|
| binding | SourceTexture2D、AtlasPageTexture2D、Texture2DArray双slice，均为测试内8×8合成纹理/实际shader |
| 模式 | OrderedChunks两个连续兼容物理segment；StrictOrderedDraw四segment；缩小为两个command后分别1/2段 |
| 显示时刻 | alpha=0/0.25/0.5/0.75/1，同一tick11publication的captured副本 |
| 动态metadata | 每alpha stable重复Build、4→2缩小、2→4恢复；Mesh identity保持 |
| 透明payload | 四重叠quad、XY双翻转、部分alpha/tint、UV裁剪边界、位置/depth、双array slice |
| oracle | 独立按命令构造quad Mesh/索引/UV/颜色/采样边界，用每quad一次DrawMesh按相同顺序绘制；不复制backend顶点/descriptor |
| 插值 | 独立正值AwayFromZero取整与view投影公式核位置；原publication位置/PreciseX/tick保持，身份/flip/color/local sequence不变 |
| 负控制 | alpha0.5反转canonical四quad绘制顺序，每组819像素差异，避免无重叠或空输出假PASS |

6组各15样本×9216像素，合计90样本/829440像素，最大RGBA通道误差0。每样本至少846非背景像素。各组保存actual/reference/reverse-order-negative-control三个96×96 PNG和result.json。PNG是离屏小夹具，不是Game View/整场截图；无全相机/GPU时间/带宽结果。

54回归：Deferral14、Latest13、MotionSampler4、DisplayMotion纯三项、CapacitySeal16、OrderedShutdown4。包含warm排队/旧lease拒绝/latest publication/跨World/同UnityFrame去重、关系/generation/跳tick拒绝、整份容量拒绝、十一阶段关闭。未运行两个会写旧结果的DisplayMotion UnityTest，也不把空World fixture升级为正式整场checksum回放。

## 边界与下一必要门

本GPU路径是正常离屏CommandBuffer/ReadPixels正确性测试，不是production RenderPass/真正GPU batch计数/设备性能，shader共享意味着不独立证明shader数学或正式EXE像素规格。固定已排序输入消费顺序正确，不读取/验证Q06活跃排序器；sorter、自然first-visible资格、全类别weapon/shadow/health和实际Battle透明重叠仍待确认。新测试包含显式分配/同步GPU readback，不能当战斗完整0GC证明或新增收益A/B。

publication age、真实显示latency没有本轮新实测；已有deferral/latest与pure motion契约回归通过只证明受控边界。下批先在原saved Battle只读观察同显示时刻的alpha/publication时间戳/可见几何，严格区分记录延迟与GPU扫描输出延迟；再按现有门完成高负载/1800全链0GC/1000AI/120FPS/Android，不重跑这些同样本case代替未覆盖门。

A1 dirty-chunk跳过仍未来设计；不跨chunk合并/改segment或failclosed，EXT1 PROPOSED/MODIFY_REQUIRED且无专项M0，MONO USER_HOLD，ATLAS bank/预算/格式/正文不变。33ms/输入/World真值/RNG/checksum/有序关闭及旧battlecampaign scoped closure未改。

## 文件与恢复审计

唯一新代码 [Editor测试](../../../Assets/NTSD/Scripts/Test/Editor/BattleCentralSameSamplePixelEditorTests.cs)＋meta(GUID b2e7c0830e814fdbb73791008265226a)。七份进度/治理文档定向追加。7准确当前字节备份、611保护文件SHA零漂移，无文件删除/移动/旧证据覆盖/Git丢弃/提交/push；[Operation](../../../docs/ai/FILE-OPERATIONS/NTSD-OPTIMIZATION-BATCH17-SAME-SAMPLE-PIXELS-20261007/RECORD.md)和before/after manifest记录。
生产DLL仍A49C69CE527D7E0268661F96F6A5A28B0C79E6204872BDA6EAF1A1930270AE8D；EditorDLL4DF2B444D50ED1A52F6BF6F16A512432C8272706E0ADCFC80D6DAC1CDDBB926C。正式EXE SHA336B44...EB7BD3保持，Scene两文件SHA与事前相同，HEAD2cccd597...不变/staged空。文件审计VERIFIED不等于父项性能VERIFIED。
[Task](../../../docs/ai/TASKS/NTSD-OPTIMIZATION-BATCH17-SAME-SAMPLE-PIXELS-20261007.md) / [Change](../../../docs/ai/CHANGE-RECORDS/NTSD-OPT-M03-SAME-SAMPLE-PIXELS-017.md)。

