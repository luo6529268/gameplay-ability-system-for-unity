# 优化第二批：容量封口操作记录

Operation：NTSD-OPTIMIZATION-BATCH02-CAPACITY-SEAL-20261006 / VERIFIED（仅本批文件操作留痕；Task/Change仍RUNTIME_PENDING）。
授权：用户“开始执行下一批的任务”，沿用已批准34项优化与H-11方案，不解冻专项。
执行者：Codex /root；根目录I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity。
开始时间/逐文件绝对路径、SHA/字节/Git状态见before.json；当前commit不能替代dirty备份。
修改前用Copy-Item -LiteralPath精确11个before路径到各自before-backups/镜像路径，
只创建新备份，目的路径存在即拒绝，逐个核对SHA；不覆盖原件，不删除/移动。
备份之后登记INDEX、Task/Change/STATE/handoff，再写新聚焦测试和生产hunk。

准确代码：BattleCentralRenderSystem、BattleDynamicMeshBackend、BattlePixelFramePlan、
BattlePresentationDisplayMotion（before.json有完整路径）；新增
Assets/NTSD/Scripts/Test/Editor/BattlePresentationCapacitySealEditorTests.cs与Editor生成meta。
准确文档：before.json其余7个路径，只更新本批/父H-11状态，不擦除第一批事实。
新Task/Change/诊断REPORT/raw JSON/本Operation after文件create-only，既有34项和旧两JSONL保留。

范围：生产预热容量转为每slot逻辑硬上限；snapshot CopyFrom之前按已公开count拒绝，
mesh command上限、motion runtime slot上限；生产用无分配预检/静态原因，直接错误调用抛异常。
拒绝整份新submission，保留已有last-good；不改GPU buffer/fence或物理segment/渲染顺序。
不读写BattlePresentationShadowBuild活跃方法体；仅既有外部public消费接口/引用。
不改模拟/33ms/checksum输入/Scene/资源/ProjectSettings/PERF/ATLAS/MONO/EXT-1。
现有EndBattleCapacitySeal解除本包封口，缓存保留；11阶段不重排；GPU完成未知不提升。

拟执行：apply_patch最小hunk；现有TCP6402 UnityConnection原Editor预检/refresh_unity/
run_tests(mode=EditMode,精确新class)/get_test_job；旧预热/网格/LatestFrame回归，无Play。
Tools/Validate-ChangeLedger.ps1、git diff --check、34项/链接/保护SHA/备份SHA/after清单。
无需reset/checkout/clean/stash/提交/push，不安装插件或另开Editor。
恢复：另获批准后，以本Operation备份字节为本批原状只逆向自己的hunk；保留第一批及其它dirty。
父H-11仍有命令物化内部其它缓存/完整上传录制提交0GC/native/GPU/Scene/设备门，不能子批自动关闭。

2026-10-06 实际执行记录：11份备份于18:56:37前置快照后精确创建，修改前全SHA匹配；
apply_patch写新增test先RED16/16预期失败，再四生产hunk，原Editor编译后GREEN16/16、
旧30/30回归。新增test meta由原Editor生成，不动已有Scene/资源。
请求结果、Editor idle/Menu clean/nonPlay/errorCS0、编译/URP状态均存本批诊断目录JSON。
19:11:35静态复核：11份备份SHA全保持、8保护SHA全保持、34唯一条目12高/14中/8低，
94个选中文档本地链接无缺失；scoped git diff --check exit0，新增test无尾随空白。
ChangeLedger实际PASSED，1298 Records、当前6 governed code diff文件均覆盖；历史warning保留，
不是本批新错误。最终原件见change-ledger-validation.json与after.json，不虚构Play/设备结果。

保留第一批全部产物/dirty与两份无关authority-content JSONL；不读取Q06活跃body，
不删除、移动、恢复HEAD、clean/stash、提交或push，无Scene/资源/ProjectSettings/PERF/ATLAS/MONO/EXT-1变更。
新增CPU预检成本未测，不称帧率收益；本批未做GPU/native峰值或全路径0GC。
