<!-- CHANGE-RECORD
id: NTSD-OPT-M03-SUBMESH-BATCH-UPLOAD-013
status: RUNTIME_PENDING
code-path: Assets/NTSD/Scripts/Animation/Rendering/BattleDynamicMeshBackend.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/Editor/BattleDynamicMeshBackendSubMeshEditorTests.cs
authority: user next optimization batch; scoped presentation metadata API batching only; formal336B44 unchanged
evidence: artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH13-SUBMESH-BATCH-UPLOAD-20261006/REPORT.md
-->

# M-03 稳定多子网格元数据批量更新

[Task](../TASKS/NTSD-OPTIMIZATION-BATCH13-SUBMESH-BATCH-UPLOAD-20261006.md)；[Operation](../FILE-OPERATIONS/NTSD-OPTIMIZATION-BATCH13-SUBMESH-BATCH-UPLOAD-20261006/RECORD.md)。
事前PLANNED，在任何脚本编辑前登记。
原状：稳定range的多segment chunk逐active submesh调用SetSubMesh；
growth/range change已使用SetSubMeshes；子批12阶段证据仅Editor synthetic测量。
目标：Upload稳定多个active submesh复用已有descriptor数组和SetSubMeshes，
减少native metadata调用交互，不改segment数/排序/UV/bounds/尾部、高水位、上传payload。
先增加现有合同的测试，再按现有20case测改前/后；此performance优化无伪造生产bug RED。
精确文件/符号、风险、验收/备份与恢复在Task；无新增owner/lease/资源，11阶段关闭保持。
尚未写代码/编译/执行；追加实际结果，不将Editor收益称Android/真实战斗通过。
TEST_FIRST_CODE_WRITTEN：仅现有SubMeshEditorTests增加两mode移动/descriptor/tail/empty恢复case；
原三个测试保持，生产尚未编辑。所有GetSubMesh/vertices/uv/color数组与断言均在采样窗外。
此合同应在旧算法通过，不制造RED；接下来旧backend新鲜20case基线及新合同实际执行。

改前首次87adbfc11d1645b9a794914db0638778已完成25，新增两case exact translated bounds-size断言失败；
result=null，不虚构23/25或任何baseline数据。仍旧backend，非生产RED。
浮点在新坐标重新计算左右角导致尺寸差异；按现有几何回归模式改为逐轴2e-5容差，
UV/colors及index/range/segment整数仍精确；重跑新合同与旧20，原件保留。
生产hunk范围细化（尚未编辑）：稳定batch仅active>1且active==retained high-water；
缩小active prefix仍原逐项更新，避免把少量active descriptor扩为大tail批量遍历。
新增第三项64→2 stable局部0B回归，仍test-first；旧20基线fixture不改。
改前有效ed521799e0e04d8394b1de51ca854a91：26/26 Passed，68.5763836s。
三新增合同+原三submesh及原20case完成；20报告有效，旧backend/source身份保持。
新fixture compiled后当前26通过，旧算法不是缺陷RED；现在实施最小branch。
CODE_WRITTEN：Upload只增加 fully-active multiple submesh 的batch判据（4插入/1删除）；
现有descriptor生成、range/growth、inert tail、bounds、finite checks、上传和segment生成不改。
小prefix与单segment保持原路径。无新增分配/缓冲/配置，Compile/改后A/B待执行。
COMPILE_PASS：原Editor production DLL23:08:47，刷新/reload后idle/nonPlay，CS过滤0error。
新fixture已编译且改前26通过；改后20case A/B与具名回归尚未运行。
FOCUSED_TEST_PASS：原Editor新旧26/26、改后20/20、相关71/71；117成功执行/91去重Passed。
两端40报告有效72000 Build/2560warmup，局部当前线程0B/growth0、20合同投影完全一致。
1000高segment无诊断mean1.89～2.00→0.996～1.051ms，降46.33～47.48%；
含计时metadata1.159～1.209→0.315～0.340ms，非纯API/GPU。
单段计时mean上升2.19～2.92%保留、无诊断p50近似保持，不无证据归因；顺序A/B非统计/设备认证。
RuntimePending：真实Battle/像素/完整链0GC/退出重进/生产窗口/1000AI/Android未验收；
只4新增/1删除生产行，原有限位/segment/UV/bounds/上传/lease/关闭保持，专项门保持。
新增3合同经旧/新算法皆通过；首次测试浮点exact失败原件保留，不算production RED。
最终治理23:14：Validator0error/本Change scope0warning，全局4268历史warnings未清理；
9backup/393保护SHA稳，160links0missing、compiled七文件身份0drift，diff-check0/trailing0。
状态RUNTIME_PENDING，不将受控Editor局部收益晋升真实Scene/全链0GC或Android。
