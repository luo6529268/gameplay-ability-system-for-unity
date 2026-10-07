# 第27批 H11 有效采样与选定完整表现链

状态：PARTIAL / CALIBRATION_AND_CPU_BRIDGE_PASS / CAMERA_ALLOCATION_FOUND。原Editor四项校准首轮2/4（单位发现及饱和读取工具缺陷，原件保留），最小修后同四项4/4 Passed，job1a5857c5436e41e4abc1cdcdf7d944f2。实际GC.Alloc单位TimeNanoseconds，已知1MiB前后各1event、空范围0；容量2实际12event饱和被拒，不当0GC。两个原完整CPU桥2/2通过，一次原1800camera严格失败见最新终态；没有重启1000AI长窗。

固定容量当前线程事件范围，禁止逐帧求和/循环覆盖/全局Profiler变更；原旧字节值只RAW_UNCALIBRATED，实际单位值不臆报分配字节。校准/输出在selected scope外，全部执行/采样和标量统计在scope内，异常/完成/退出释放；不是其它线程/native/GPU/全PlayerLoop/1000AI或Android证据。

当前只两受控CPU桥有校准0event证据，完整生产camera链未通过0GC、没有新FPS收益；H11 EVIDENCE_PENDING，H07仍未优化完成，六项首阶段4/6。详Task、Change NTSD-OPT-H11-CALIBRATED-SCOPE-027与Operation。

2026-10-07T06:05更新：两个CPU桥job84b6a049ec0f4fa8973108e5d8074d69实际2/2 Passed、41.1195s；各64warm/1800sample，完整selected scope事件0且有效/非饱和/非wrapped，前后正对照各1event/空0。1000Strict/1000活动aux：1803600 CPU DrawMesh、1800上传、316800000bytes，均值6.916339ms；4097Ordered/17aux：9000 CPU DrawMesh、3600上传、1297929600bytes，均值13.106699ms。原源/纹理/Scene/两slot同、0growth/CPUlease0，不当生产RenderPass、自然1000AI、GPU batch/fence或FPS证据。旧byte count0只原始未校准字段。

原Battle camera-01已在确认原Menu clean/测试终态后仅一次加载并调用新菜单，当前正在原Editor生产资源预热，target1800distinct camera；没有刷新、测试或第二实例打断，无相机终态通过结论。Tools/Validate-ChangeLedger.ps1实际exit0/validation PASSED，4277历史warning保留，不称全仓warning0。11准确副本/180非写域保护此前逐项SHA同，HEAD8107196b未变；结束后仍须核关闭、恢复Menu及最终保护。

2026-10-07T06:11最新终态（上段启动为历史）：camera-01 FAIL/DONE，1800distinct camera/tick8→1314，前后正反校准通过、所有1800camera/3600observer scope有效/非饱和/非wrapped。Camera总2event、observer总13event，不能判完整0GC，具体分配调用点尚未定位；不能把2/13事件当作0.7FPS主因或推算字节。所有前置和显示门保持：每camera Foot/Health至少各2、两slot、11548 CPU DrawMesh录制=执行、1800Build/0growth/CPUlease0。十一阶段objects/slots/borrowers0、Battle clean/SHA253B2EBA同；已恢复原Menu。所有原始frames/失败留存，后续不跳过首camera、不放宽窗口、不把诊断观察分配冒充生产修复。旧显示/像素/消重/关闭正例复用。
