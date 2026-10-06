# 第十四批 M-03 原 Battle 生产窗口与关闭重进 Task Contract

Task NTSD-OPTIMIZATION-BATCH14-PRODUCTION-WINDOW-20261006 / SCOPED_PRODUCTION_WINDOW_PASS；Change NTSD-OPT-M03-PRODUCTION-WINDOW-014 / VERIFIED（Editor观察入口限定）。
需求：用户“开始执行下一批的任务”；承接子批13的真实物化/publication-alpha/完整链与重进门。
原Editor port6401/PID19040、Unity2022.3.62f3/URP14.0.11，Windows/D3D11；当前Menu clean/8roots/idle/nonPlay。

## 准确范围
唯一新增脚本 Assets/NTSD/Scripts/Test/Editor/BattleCentralProductionWindowSceneProbeEditor.cs 与其meta：
InitializeOnLoad+菜单显式启动；仅一个保存的原Battle Scene，原场景配置/roster/input/自然Driver不替换。
两轮自然窗口各96逻辑tick；至少tick8及已打开battle allocation window后开始，最多2048 world-camera frame记录。
仅订阅begin/endCameraRendering观察：读取已发布plan/captured/publication、现有分组计数、backend scalar及Mesh submesh descriptor。
不额外Build/上传，不获取新submission lease，不做GPU readback/capture，不改backend/相机/绑定/配置。
窗口端点显式报告和JSON在热观察段之外；统计是production自然窗口，不是EXT1专项M0或1000AI。
GC.GetAllocatedBytesForCurrentThread跨world-camera begin/end仅整相机render envelope，不能归因中央方法或当完整链0GC；
Driver已有tick/Update/LateUpdate/PlayerLoop分配计数单独读，窗口差值不相加。
按现有11阶段ShutdownBattleRuntime→map presentation cleanup→CompleteShutdown...顺序正常退出，
两轮各检验world objects/runtime slots/pool borrowers=0、Scene clean/SHA不变。
测试入口不新增Runtime owner/worker/queue，Editor观察停止于EXITING；域重载用SessionState仅保存非热状态。
新证据只CreateNew/唯一run路径；不覆盖既有结果，不创建/保存Scene、Prefab、资源或Settings。
原Menu安全切到原saved Battle，仅Editor打开状态、两次Play验证，结束回原Menu；不得打断任何外部Play或dirty Scene。
测试副作用更正：既有DisplayMotion两UnityTest会先另两次Play，暂时30/60/120显示采样和受控实体位置，
不写Scene asset；这些受控回归不算本新探针自然窗口。第二项固定旧play-trace日志写入由Operation独立before/backup留痕，
这一个普通测试日志是“证据不覆盖”的具名例外，旧字节保留；其它文件仍不覆盖，不手动恢复日志。
若需另生产修复/新增统计/配置或扩大workload，先停止本包，独立记录；本批不改生产算法。
维护七份进度治理文档，不改PERF/ATLAS/EXT1/MONO正文、Q06活跃方法体、Server/Plugins/Gen。

## 验收与风险
Compile新Editor程序集0error；相关LatestFrame/DisplayMotion/Report/SubMesh/Capacity/MemoryBoundary聚焦回归；
原Battle两窗口central plan/submitted command、publication与alpha分组实测、stride44、0growth、有限bounds、
物理segment command范围有序且合法，source publication无commands/order物化反写；记录实际观察的1～2 slot，不强制交替。
首轮只有end-camera观察会因每camera两次Build产生slot采样混叠；修正补begin-camera计划标量，
旧FAIL与v1源保留，重验使用新corrected-02子目录，两周期各96tick，不覆盖原件。
只验证记录到的自然小roster，不保证高segment46%收益、first-visible/透明重叠/GPU像素、严格Host wallclock、1000AI或Android。
分配非0如实报告；Editor PlayerLoop含诊断/Editor系统，不能单独判生产失败或认证0GC。
启动最多600秒/总900秒；超时/用户停Play/脏Scene/错误显式FAIL，不隐藏失败/自动重跑。
33ms/3ms/max2、bitwise逻辑/RNG/input/pass、publication只读、segment/failclosed、slot/lease、11阶段关闭不变。
EXT1 PROPOSED/MODIFY_REQUIRED无专项M0，MONO USER_HOLD、ATLAS bank/预算/格式/merge门不变。

## 审计与恢复
[Operation](../FILE-OPERATIONS/NTSD-OPTIMIZATION-BATCH14-PRODUCTION-WINDOW-20261006/RECORD.md)：7份dirty当前字节准确备份，447保护。
[报告](../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH14-PRODUCTION-WINDOW-20261006/REPORT.md)记录真实命令/原件/边界。
回滚新增脚本或七文档只在另授权后按准确before恢复，禁止HEAD/reset/checkout/clean/stash，
不Git add/commit/push，不启动第二Editor。

第二轮启动补记：corrected-02 cycle01 PASS/128samples/256Build，退出Battle clean/nonPlay；
未观察到第二轮Play或cycle02证据，自动delayCall→EnterPlay衔接是否被Unity退出周期忽略待确认。
只新增显式“Second Cycle When Idle”菜单：要求第一轮文件PASS、第二轮文件不存在、
真实Runner停止、原Scene clean/nonPlay；仅允许无session或cycle02 STARTUP，禁止重跑第一轮或打断观察。
修改前保存v2源备份/SHA；不改热观察代码、生产流程、容量或输出01，不把自动衔接失败当生产错误。

## 2026-10-07实际收尾

原Editor新DLL/实际菜单通过；生产DLL和447保护不变。已确认101去重case Passed。
corrected-02两自然窗口均PASS，各96tick、128/136camera、256/272Build（publication96/96、alpha160/176）。
两slot、stride44、publication隔离、合法segment/bounds、0growth/0failed/0rejected均通过；
观察与camera envelope及已有计数分开全0B，非完整链/其他线程/GPU/1000AI证书。
两次正常11阶段关闭objects/slots/borrowers0、Scene clean/SHA同，原Menu clean/8roots/nonPlay已恢复。
自动cycle02启动未生效的内部原因仍待确认，但显式接续有证据，原结果/失败均保留。
新鲜同显示帧两物化候选归入M03，不实施删除早期消费者；父M03/H11仍OPEN/RUNTIME_PENDING。
新增Editor入口验证范围已达成；生产高负载收益/像素/完整0GC/120FPS/1000AI/Android未执行或未知，
未SelfCheck/正式EXE全World回放重验，不宣称重新对齐。无Scene/资源/设置/旧任务或专项门改动，无Git写入/丢弃操作。

