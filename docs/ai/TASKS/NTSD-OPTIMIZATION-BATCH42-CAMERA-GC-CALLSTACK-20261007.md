# 第42批：早期相机完整范围分配调用点
状态：SCOPED_ALLOCATION_CALLSITE_DIAGNOSTIC_VERIFIED；H11父项未完成，Goal active。

来源：用户2026-10-07澄清阶段完成而非次数停工；当前六项合同第0—8节允许未解决技术问题的必要最小定位。35唯一1800camera双校准可靠，事件ordinal1/5各1但site未知；39另Combat1000窗口不能回答这两个事件。不重复旧1800窗口、不猜测生产分配、不降低原完整0GC FAIL。

精确代码域：BattleCentralProductionWindowSceneProbeEditor.cs的Report、独立Batch42入口/创建report、STARTUP转OBSERVING、BeginCamera/EndCamera、CompleteWindow与原Dispose链；BattleOptimizationCpuGcCaptureEditor.cs的可选camera mode/边界记录/已有settings保存恢复与camera-only raw export；新增BattleCameraGcCallstackCaptureEditorTests.cs及meta。其余生产、Scene、资源、排序、collector、cadence、Server全部保护。准确路径见Change Record与Operation before.json。

当前固定诊断（已按R1纠正）：原saved Battle自然小roster（不是1000AI），继续35同准备/校准/两observer与六子块；仅观察首8个完整相机。原完整camera Begin/End GC范围不缩；两个point marker分别在各callback内立即配对闭合，且在原GC范围外取begin/end时刻，不跨URP callback建立Profiler scope。CPU GC stack只在READY后开启，无deep/GPU；辅助显示/lease/draw/growth及十一阶段关闭不降低。8帧仅调用点诊断，不是1800/full0GC/FPS验收。

复用既有Profiler settings保存/恢复和RawFrameDataView栈解码；新增相机模式不依赖39失败的NewProfilerFrameRecorded回调，依据真实相机完成计数停止。下一Editor更新只恢复/留raw；原Probe完成关闭并退出Play后，Edit Mode cold解析新raw。只取主线程同raw frame两个point时间区间内GC.Alloc、metadata/栈及实际ordinal/frame/tick/event，8对point/8boundary/逐帧计数必须全匹配；非有限/反序/缺point/PARTIAL不称闭合。不输出573MB全样本、不清旧history，LoadProfile keepExisting=true。原39模式/请求/文件不改，不复跑千人。

owner是既有Editor capture与原Probe，容量8边界固定、热路径不扩容；新字段只诊断、默认false。退出Play/重载/异常/45s采集保护截止均恢复settings，完整World关闭仍由原十一阶段owner执行；保护截止只失败该采集，不停止Goal。域重载不自动重复采集；错误证据CreateNew保留。原Scene必须clean/idle且无其他test/profiler，必要正常打开saved Battle、终态回原Menu，不保存Scene。

当前测试15case：request首8/完整校准observer/prepared/noReplay/noTiming/原root四项；同frame时间区间六边界；完成计数五项。初始子树6case RED/GREEN只作camera-01历史；R1新时间API有效RED后GREEN15＋旧39十项/35八项，共33。R1必要一次新camera-02，原camera-01因LIFO仪器错误拒绝并保留；不按相同错误方法盲重采。判断事件能否有效归因，不把无GC的8帧当H11门通过。

留痕/验证：Record先于C#，Ledger/STATE/handoff/唯一tracker登记；8当前文本backup与18保护SHA/HEAD核同，所有apply_patch；Tools/Validate-ChangeLedger.ps1与git diff --check。回滚须另授权依据当前副本，无删除/Git回退。正式336、33/3ms/max2、checksum/RNG/publication只读、segment/failclosed/两slot与GPU证明不变；EXT1/ATLAS/Mono继续冻结，Q06只hash不读body。

2026-10-07采集后必要解析修正（事前）：唯一camera-01已实际8camera/2event，raw保留；Play中LoadProfile返回false，site仍未知。只在当前声明CPU owner里增加cold Edit Mode retained-camera recovery菜单，读取既有partial state及raw复用原子树parser，新建recovered状态/调用点JSON，不改旧文件、不重采、不清history、不启Profiler；idle/fresh/8合法boundary及settings恢复硬门。原35/本次strict FAIL继续保持，Task不是按失败次数停工。此项是同一次采集的离线解析，不是新M0/新性能窗口或runtime生产改动。

R1必要方向修正（事前）：camera-01的Edit Mode追加读取同样失败；Console明确CameraEnvelope Missing/Non-matching EndSample。当前URP14.0.12 UniversalRenderPipeline.cs:841附近begin callback、875—878 end callback各在独立ProfilingScope内；跨这两个callback保持Profiler栈违反LIFO。camera-01不能作为干净生产分配复现或site证据，全部失败原件保留。不是到次数停工而是诊断方案错误，改为两个callback内立即配对的point marker，原完整GC范围不动；主线程raw按同frame两point的时间区间取GC.Alloc，不再跨callback建嵌套scope。先把六个子树测试改为时间区间边界测试取得缺新API RED，再GREEN33。完整8camera后仅停/恢复并留raw，Edit Mode单独cold解析。camera-02是这个必要仪器修正后唯一新窗口，不是同方案盲重采；原目录禁止覆盖，GPU/deep/千人/长窗仍不运行。before backup/18保护及准确两Editor源/测试域不扩大，原H11 strict FAIL及Goal active保持。

行号纠正（本次重扫）：实际baseCamera begin wrapper为当前URP UniversalRenderPipeline.cs:820—823；end为875—878。上段“841附近”仅粗定位，准确证据以本条为准。camera-01五原件SHA在后置审计保存，禁止覆盖。

验收结果：R1 GREEN33/33；camera-02原8camera/四校准PASS/observer0；raw同frame8point pair与事件全匹配，ordinal8 40B栈在Foot ResolveSprite路径，cold recovery成功并恢复settings。原strict FAIL保留；不称35全部事件已定位或修复。camera-01 instrumentation失败原件保留。终态原Menu clean/idle/nonPlay，11-stage残留0，backup/guards/HEAD稳定。准确证据与后继窄域见Record/REPORT；阶段边界判完成，不按尝试次数停工。

