<!-- CHANGE-RECORD
id: NTSD-OPT-H11-CALIBRATED-SCOPE-027
status: RUNTIME_PENDING
code-path: Assets/NTSD/Scripts/Test/Editor/BattleScopedGcAllocationRecorder.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleRealTextureSubmissionEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleCentralProductionWindowSceneProbeEditor.cs
authority: user approved six-item first stage H11 complete selected presentation CPU path zero-GC; latest continue instruction supersedes count-only stop; formal336 simulation unchanged
evidence: artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH27-CALIBRATED-GC-20261007/REPORT.md
-->
# 第27批 H11 当前线程 GC.Alloc 校准与选定完整链

脚本前PLANNED。[Task](../TASKS/NTSD-OPTIMIZATION-BATCH27-CALIBRATED-GC-20261007.md)已先冻结三个诊断路径及矩阵；[Operation](../FILE-OPERATIONS/NTSD-OPTIMIZATION-BATCH27-CALIBRATED-GC-20261007/RECORD.md)先保存当前脏字节。第26批已PARTIAL/DONE，两个smoke有效、首个1800sample GC边界守卫失败；十一阶段关闭、objects/slots/borrowers0、Scene SHA不变、原Menu恢复。当前原Editor2022.3.62f3/PID19040实际idle/非Play/无测试和编译，重新核验后才编辑。

原状与首差：第25批已知存活1MiB数组仍由GC.GetAllocatedBytesForCurrentThread报0；第23批原0B断言不能认证完整链。第26批帧Memory counter响应，但包括Editor/其它路径，GC.Alloc发现器强制Bytes单位未找到精确组合。只建立固定容量、实际单位可审计、当前线程事件计数的候选，先正/负/饱和校准，不假设可用。

实际职责计划：新Editor-only recorder helper在准备期创建/预暖、明确Reset/Start/Stop/Dispose；有效handle、实际category/unit、当前线程、正对照响应、空操作0、非饱和/非wrapped全部闭合才可认证。先只运行校准；校准有效后才接入两个原CPU桥及一个原相机窗口。旧API原始读数保留但不用判证书；计数/采样开销不豁免热路径分配。

生命周期：不新增Runtime服务或singleton，recorder由Editor测试/既有probe owner持有，异常/测试finally/退出Play/完成均停止并Dispose；不改原十一阶段主序列、CPU/GPU consumer判据或production counters。没有场景序列化或资源绑定变化。

副作用与风险：诊断native recorder的固定存储和采样成本、控制用已知数组与证据导出在范围外；不改Profiler全局设置。0事件仅声明当前线程选定scope的managed allocation，不能证明其它线程/native/GPU/全PlayerLoop/1000AI或Android。marker不存在/校准失败/饱和都记UNKNOWN/INVALID，不强制PASS。

验收：校准具名聚焦结果、两个64warm+1800 CPU桥与原1800 distinct-camera完整链、活动Foot/Health/两slot/全部DrawMesh上传录制提交/release、0增长/CPU lease0/source保持、十一阶段关闭和Scene恢复、保护SHA和ChangeLedger。旧显示/像素/受控A-B证据复用，不重跑全历史。

不回退边界：33ms/3ms/max2、checksum/RNG、publication只读、排序/segment/fail-closed、其它28项与EXT1/Mono/ATLAS门不变。不读Q06活跃方法体，不改production/Scene/资源/settings。工具失败不转成无证据架构或性能达标。

恢复：脚本前脏字节备份及新增文件身份在Operation的script-before manifest；如需恢复另获准确批准，以当前副本作最小patch，不用HEAD覆盖用户内容、不删除旧失败、不进行破坏性Git。

当前仅新helper及四个聚焦case已写，两个既有验收脚本尚未接入。固定256事件容量/当前线程/无逐帧求和或覆盖，实际单位发现、正反/饱和/未校准/Dispose校验；native API预暖与控制/导出在声明范围外。尚未编译或校准，无0GC或FPS结论；脚本前11副本/180保护见script-before.json。

首轮原Editor job b476293780f549c49ef5d9a42afcb4ce：实际4 completed，2通过/2失败；9334只是全仓发现数量。已知1MiB正对照实际得到1 event、空范围0，但handle枚举未报Unit导致UNKNOWN；小容量Count超上限后GetSample(2)越界。原失败/JSON保留。仅修helper从实际recorder.UnitType记录单位，饱和时读取至固定Capacity且仍INVALID，不改验收门或生产路径；请求原Editor重新编译后只复跑同四case。

校准复验原Editor job 1a5857c5436e41e4abc1cdcdf7d944f2：4/4 Passed，actual unit TimeNanoseconds，前后已知1MiB各1 event、空scope0；小容量2实际Count12但只读2 stored sample并判INVALID，无包装0GC。随后接入已声明两个脚本：活动aux CPU桥仅新27输出，完整1800循环/时间和标量采样包含，旧字节API只raw；相机新增隔离菜单、前后校准/完整camera及两侧observer事件统计，原Foot/Health/two-slot/关闭/Scene断言保持。异常、完成、Play退出、domain reload均Dispose，不改全局Profiler或生产路径。接入代码尚待原Editor编译和两CPU桥/相机实际结果，不能因helper4/4就称H11通过。

两个原CPU桥job84b6a049ec0f4fa8973108e5d8074d69实际2/2 Passed：各64warm+1800sample、前后正反校准通过、完整scope GC.Alloc events0/非饱和非wrapped/0growth/两slot/CPUlease0/原纹理和Scene同。1000Strict/1000aux录制1803600中央/辅助DrawMesh，4097Ordered/17aux9000，CPU均值6.9163/13.1067ms，仅受控提交桥，不是1000AI或真实生产RenderPass/FPS/GPU完成。180保护及11副本SHA当前同；下一仅原savedBattle一次1800distinct camera，生产窗口尚未启动/无通过结论。

camera-01实际终态FAIL/DONE：1800 distinct camera/tick8→1314，前后校准通过、单位TimeNanoseconds、全部scope非饱和/非wrapped/有效；camera1800scope总2event，observer3600scope总13event，故完整0GC未通过。活动Foot/Health每camera最少各2，两个slot，11548 CPU DrawMesh录制=执行，1800Build，0growth/CPUlease0；十一阶段对象/slot/borrower0，Battle clean/SHA同，原Menu已恢复。未跳过前几camera或改门槛；camera/observer2/13只能归因各自完整范围，具体调用点仍未知，不断言都是项目生产分配或0.7FPS主因。失败与CPU正例/校准首败均保留。H11 EVIDENCE_PENDING、H07性能FAIL、Goal active/4of6；下一先有据定位，绝不因次数上限或工具修复称优化完成。
