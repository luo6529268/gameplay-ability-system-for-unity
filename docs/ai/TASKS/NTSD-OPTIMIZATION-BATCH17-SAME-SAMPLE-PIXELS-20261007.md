# 第17批：同显示样本中央透明像素回归

状态：SCOPED_SAME_SAMPLE_PIXEL_PASS。Change NTSD-OPT-M03-SAME-SAMPLE-PIXELS-017。需求：用户开始下一批优化；M-03要求优化前后像素/排序一致，承接子批13 metadata与15 host/camera消重，16自然动态publication证据保持。

## 范围

仅新建 Assets/NTSD/Scripts/Test/Editor/BattleCentralSameSamplePixelEditorTests.cs 及meta。固定同一captured显示样本，用正常CommandBuffer绘制现有backend，与独立构造quad/每submesh描述符的canonical Mesh逐像素比对；OrderedChunks/Strict、SourceTexture2D/AtlasPageTexture2D/TextureArray双slice、5alpha、重叠/flip/tint、stable metadata/range shrink/recovery。纯motion测试验证离散取样/拒绝关系与generation；复用既有deferral/latest/clock聚焦测试，不运行写旧结果的UnityTest。输出新目录，拒绝覆盖旧数据。

生产不改，不读取Q06活跃方法体、改排序/first-visible定义；透明顺序只输入已排序命令序列的消费一致性，不证明Q06排序正确或完整原Battle GPU画面。不启动专项M0、Profiler/FrameDebugger/performance capture，无新GPUinstancing/资源布局/asmdef。33ms/checksum/input/有序关闭合同与EXT1/MONO/ATLAS门不变。

## 预期副作用及验证

Editor测试内Texture/Material/Mesh/RenderTexture/CommandBuffer分配和ReadPixels GPU同步仅离线正确性，不进入战斗热路径、不报告0GC/耗时收益。所有临时对象try/finally释放/销毁，恢复RenderTexture.active，测试不创建Scene组件。alpha只复制publication后取样，不反写原frame。shader两种实际存在且显卡非Null才执行，空输出禁止PASS。

验收：新测试全部具名完成；相同样本canonical/backend像素最大通道误差0，反序负控制产生非零差异，场景clean/同SHA/8roots恢复，生产脚本/正式EXE/611保护不变，ledger/diff-check通过。只SCOPED_SAME_SAMPLE_PIXEL_PASS，父OPEN/RUNTIME_PENDING；自然整场首可见/latency/透明重叠、1800全chain0GC、1000AI/120FPS/Android继续开放。
恢复：七文档从Operation当前字节备份做获批定向patch，新fixture仅显式再次授权可移除。无不可逆生产副作用。证据 [报告](../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH17-SAME-SAMPLE-PIXELS-20261007/REPORT.md)；[Operation](../FILE-OPERATIONS/NTSD-OPTIMIZATION-BATCH17-SAME-SAMPLE-PIXELS-20261007/RECORD.md)。

