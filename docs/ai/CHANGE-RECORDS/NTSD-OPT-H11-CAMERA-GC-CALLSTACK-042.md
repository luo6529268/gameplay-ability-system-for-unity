<!-- CHANGE-RECORD
id: NTSD-OPT-H11-CAMERA-GC-CALLSTACK-042
status: VERIFIED
code-path: Assets/NTSD/Scripts/Test/Editor/BattleCentralProductionWindowSceneProbeEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleOptimizationCpuGcCaptureEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleCameraGcCallstackCaptureEditorTests.cs
authority: user 2026-10-07 stage completion clarification and current six-item contract permit necessary allocation attribution within H11; no production behavior change
evidence: artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH42-CAMERA-GC-CALLSTACK-20261007/REPORT.md
-->
# 第42批早期相机GC调用点
VERIFIED / SCOPED_ALLOCATION_CALLSITE_DIAGNOSTIC_ONLY；H11父项仍FULL_CAMERA_ZERO_GC_FAIL。

需求/原状/准确范围/验收与回滚见[Task](../TASKS/NTSD-OPTIMIZATION-BATCH42-CAMERA-GC-CALLSTACK-20261007.md)，[Operation](../FILE-OPERATIONS/NTSD-OPTIMIZATION-BATCH42-CAMERA-GC-CALLSTACK-20261007/RECORD.md)。35两个事件各100 raw TimeNanoseconds不是bytes；39准入来源已41局部闭合，与此相机site不同。原PID19040/6402 Menu非Play/idle/no tests，CLI无Pipeline不等于Editor关闭。静态URP生产路径确含相机物化/上传/DrawMesh/Execute。

改前：Probe完整1800相机仅计数，无调用点；CPU捕获仅千人warm120后回调8帧/全样本大导出。改后目标：独立8相机诊断，原Probe scope/严格FAIL、旧CPU捕获模式不变；新标记只在camera诊断flag开启，固定8边界、真实相机计数截止、现有settings全恢复后主线程GC子树/栈小导出。capacity/lifecycle/owner如Task，0GC完整验收不以这个instrumented窗口替代。

无生产规则/逻辑字段/Scene/资源/预算/排序/collector/cadence改变。raw history追加保留，输出fresh/CreateNew；不新增Runtime owner或十一阶段，不拿CPU lease作GPU证明。回滚副本在Operation backup，恢复需新批准；无破坏性Git。编译/测试/实际采集及限制在此追加，不凭计划认定通过。

实际test-first：已新增15case测试与固定GUID meta；只有reflection访问新接口，原两个Editor source尚未修改。原PID19040/6402 scripts refresh已提交，等待原domain完成后仅运行该fixture获取缺API RED；未启动相机/Profiler采集。初始两个旧源、所有保护字节不变。

实际实现更新：red-01原domain未导入新fixture，仅0case，明确INVALID保留；查原RefreshUnity scripts scope只请求编译，all scope实际AssetDatabase导入后原Editor加载新fixture。red-02 job287f86062b374bd6a668c6232ac805b5实际15case均缺新API失败，有效RED。随后仅两个声明Editor源补可选8相机模式、固定边界、marker、完整settings恢复、append raw主线程GC子树小导出；旧39模式/原严格camera FAIL未替代。尚无GREEN/新采集/收益。

实际聚焦更新：原Editor加载新DLL UTC10:25:20，compile idle、Console error CS0；green-01 job2d3f5201911647e8948b29a88ff7e253实际33/33 PASS（新15、旧CPU10、字面量8），三个既有热方法IL literals仍0。18保护SHA/HEAD不变，git diff --check exit0。只证明编译/覆盖断言，不证明调用点或0GC；下一仅一次已声明camera-01，不开启GPU/deep，不改生产。

实际camera-01：新8个完整相机完成，unity3230—3237/tick10—19，ordinal1/2各一事件，observer0，两个recorder前后校准均PASS；严格camera FAIL保持。所有8固定boundary合法；raw23915448B保留。Play中LoadProfile(raw,true)返回false，export PARTIAL，无site结论；settings全恢复、原有序关闭objects/slots/borrowers0，Scene clean/hash不变，Editor已idle非Play。不是Goal阻塞/结束，不重采。继续同批准确CPU捕获owner的cold retained raw解析域：追加独立Edit Mode恢复菜单/原partial状态反序列化/原ExportCameraSamples复用，另存cpu-gc-state-recovered.json、既无的camera-gc-callstacks.json，原raw/state/production证据只读不覆盖；复用39已验证的退出Play后解析策略，RequireAvailable/idle/fresh/8boundary条件，不清history、不启Profiler。Task/Operation同步事前登记后才写这个cold方法。

camera-01解析修正失败与仪器R1：cold读取仍false，原recovered PARTIAL/settings恢复保留；Console出现本marker的Missing/Non-matching EndSample。URP14.0.12两个camera callbacks各有独立ProfilingScope，长marker跨callback破坏LIFO；本次2event不能当干净生产复现。方案错误已定位，不以未知/次数结束Goal。事前Task已登记在同两Editor源/测试域改为两个立即配对的point marker、同raw frame时间区间归因、Edit Mode才加载；六边界测试先RED再GREEN33。必要新camera-02一次，原camera-01四证据及recovered不覆盖，不复跑长窗/千人，不改任何production源。原35完整FAIL仍权威当前GC缺口。

R1实际写入：red-03 job86134a82b9654029906116fa1d2667b2实际15case执行、六个新时间边界因缺API失败。随后同源把长marker替换为两个立即闭合Auto point，不再使用跨callback Begin/End；raw parser使用同frame begin-end时刻与end-start时刻之间GC样本、匹配固定boundary/计数/栈元数据，非有限/反序/缺point都PARTIAL。完整捕获先恢复settings/保存RAW_RETAINED_CAMERA_PARSE_PENDING；cold新camera-02菜单读取后新建recovered文件。旧39不变，camera-01原件不覆盖。R1编译/GREEN/实际新采集均待验证。

最终限定验收：R1原Editor加载新DLL/compile idle/error CS0；green-02 job37aac292ce4045588e0245bcb00b7425实际33/33 PASS，旧三个hot observer IL literals0。原saved Battle camera-02完整8camera，URP unity2869—2876/tick10—18；ordinal8一次GC.Alloc，原Scope事件1与raw样本1逐frame全8对应，metadata40B（不是旧TimeNanoseconds raw换算），栈31地址中ResolveSprite→ResolveRuntimeFootMarkerTexture→BattleRenderPass.Execute可解析。原两个recorder前后校准PASS，observer16scope及六子块events/invalid/unattributed0；完整camera严格FAIL不豁免。两个新point profiler warning/error查询0。完整捕获只恢复/留raw，Edit Mode追加载入新raw：previous57之前history保留，import57—65、8完整point pairs，RECOVERED_CAMERA_CALLSITES/settings恢复true。明细见camera-02/camera-gc-callstacks.json与cpu-gc-state-recovered.json。

已观察事实与推断分离：新40B事件属于当前Foot ResolveSprite路径（stack行60是方法入口，不是精确表达式行）；源码该方法无显式托管构造，但按动画帧访问sprite.texture，优先候选为未预持有帧纹理的Unity wrapper首次物化，仍需下一批针对测试确认。不声称它必然解释35全部两事件，也不声称完整0GC/千人/FPS通过。本批只诊断，未修改Foot或production C#。

原十一阶段关闭objects/slots/borrowers0、Scene clean/SHA unchanged，终态原Menu单Scene/clean/idle/nonPlay；8backup/18保护/5拒绝原件SHA与HEAD保持，未读Q06方法体、未动Scene/Prefab/resources/Settings/EXT1/ATLAS/Mono/Server。旧所有失败证据保留。Goal本次tool仍active；20已执行子批＋38仅PLANNED累计21，不是停止上限。下一安全必要工作是Foot六帧纹理预获取/强引用生命周期的窄域test-first，不因子批完成停总目标。最终Ledger验证结果见REPORT与Operation后置审计。

最终验证命令：pwsh -NoProfile -File Tools/Validate-ChangeLedger.ps1 -RepositoryRoot I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity → exit0/error行0/4284匹配WARNING行（历史声明路径非当前diff仍有警告，不称全库零警告）；git -c core.safecrlf=false diff --check → exit0/messages0。Task canonical已同步R1 point pair/cold Edit Mode解析；初始设计与失败过程只在历史追加保留。实际C#最后版本均已由GREEN33与camera-02执行，不把静态结果晋升父H11/Android/FPS。

