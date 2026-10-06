# 第十三批 M-03 多子网格元数据批量更新 Task Contract

Task NTSD-OPTIMIZATION-BATCH13-SUBMESH-BATCH-UPLOAD-20261006 / SCOPED_METADATA_AB_PASS；Change NTSD-OPT-M03-SUBMESH-BATCH-UPLOAD-013 / RUNTIME_PENDING。
需求：用户开始下一批；子批12确认1000段descriptor/native metadata局部mean约1.14～1.20ms。
原Editor PID19040/port6401，Unity2022.3.62f3/URP14.0.11，Menu clean/8roots/idle/nonPlay已现场核实。
仅此连接；不启动第二Editor，不主动切Scene/Play/Profiler或GPU工具。

## 准确代码范围与假设
1. Assets/NTSD/Scripts/Animation/Rendering/Editor/BattleDynamicMeshBackendSubMeshEditorTests.cs：
   先增加稳定多段移动、tail/empty/recover的合同回归，不伪造行为RED；现有合同本就应通过。
2. Assets/NTSD/Scripts/Animation/Rendering/BattleDynamicMeshBackend.cs：
   BattleMeshChunk.Upload中稳定多个active submesh也使用现有SetSubMeshes批量API。
   单active submesh保留原SetSubMesh；growth/range change已有batch语义不变。
   复用既有descriptor数组，不新增counter/持久资源/配置/owner，不改descriptor内容或clear/recovery。
   此优化减少多段元数据native API交互，不是合并segment、DrawMesh/GPU batch减少或顶点上传skip。
3. 既有BattleMeshUploadBaselineEditorTests不改，原20 case改前/改后同窗结构对照。
   64warmup/1800sample；instrumented与null diagnostics分别比较，不相加phase或设噪声阈值。
   顺序A/B非随机配对，synthetic/nullTexture/Material，不等于自然publication/素材/AI/完整M0。
4. 新合同＋既有submesh/bounds/upload/detail/capacity/native recovery/相关UV采样回归，
   allocation0B限定prepared Build/recorder当前线程，不能晋升完整表现链0GC。
5. 当前源/已加载assembly身份冻结，diff/保护哈希/链接/ChangeLedger验证。

## 不变量与风险
33ms/3ms/Host max2、逐位checksum/input/pass/RNG不改。
publication只读/排序/segment/material/纹理/UV/stride44/上传payload/fail-closed/slot/lease不改。
高水位inactive tail必须保持inert，空帧恢复与destroyed mesh保持原策略。
稳定多段调用batch会遍历retained descriptor，单段保留原路径降低无关回归风险；收益由受控数据判定。
不读Q06活跃方法体；不改Scene/Prefab/InputActions/资源/Settings/Server/生成代码/第三方。
EXT1 PROPOSED/MODIFY_REQUIRED且不启动专项M0；MONO USER_HOLD；ATLAS bank/预算/格式/merge专项门不变。
没有新增资源/worker/lease，原backend Dispose及11阶段关闭保持。
若收益或合同失败，停止推进、保留失败证据；未获回滚批准不自动恢复/覆盖。
只有具名focused通过可报告，生产Change及M03父仍RUNTIME_PENDING，真实Battle/像素/设备未跑。

## 审计与恢复
[Operation](../FILE-OPERATIONS/NTSD-OPTIMIZATION-BATCH13-SUBMESH-BATCH-UPLOAD-20261006/RECORD.md)九个before备份及393保护；
[报告](../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH13-SUBMESH-BATCH-UPLOAD-20261006/REPORT.md)记录改前后实际job和所有未验收项。
最小回滚hunk或九个准确备份需另授权，不使用HEAD恢复dirty、不提交/push。
实施前细化：为避免大physical high-water只剩小active prefix时批量反复重传inactive tail，
稳定batch只在active>1且active==retained physical submesh count时启用。
小prefix/单segment维持原逐项路径，growth/range change维持原强制batch。
补第三项小prefix64→2稳态局部0B；这是上述最小策略约束，不改segment或生命周期。
实际限定结果：改前26/26、改后20/20及回归71/71；91去重Passed。
两端20case各1800sample，总72000有效Build，局部0B/growth0/payload/segment投影相同；
1000高segment无诊断mean降46.33～47.48%，metadata inclusive约1.16～1.21→0.315～0.340ms。
顺序same-fixture A/B，非真实publication/AI/GPU/device；单段phase噪声上升如实保留。
原Menu前后clean/8roots/idle/nonPlay，CS过滤0error；实际case计数以raw summary为准。
生产RuntimePending与父OPEN保持，未启动专项M0/Play/设备；恢复另授权，未执行Git操作。
