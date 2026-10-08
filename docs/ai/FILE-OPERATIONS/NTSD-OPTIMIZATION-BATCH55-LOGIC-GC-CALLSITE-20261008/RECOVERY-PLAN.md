# 第55批已有 raw 恢复计划

PLANNED；同 Change NTSD-OPT-H07-LOGIC-GC-CALLSITE-055 的必要工具恢复，不新增子批、不重采。

授权：有限 Goal 当前第0—8节；原55窗口已终态，原 Editor 19040/6402 idle/Menu，不再进入 Play。11个准确写路径见 recovery-before-01.json，69个只hash保护含原 raw/旧history/state/空samples；备份在本 Operation 的 recovery-backups-01，逐SHA核后修改。禁止覆盖既有输出、清空history、启动Profiler、变更上限或任何生产路径/Scene/资源。

准确源码：BattleOptimizationCpuGcCaptureEditor 增 RecoverRetainedLogicRaw、IsLogicAppendedRange、IsLogicDescendant 及仅离线报告结构；BattleLogicGcCallsiteWindowEditorTests 增边界/新入口聚焦测试。旧39入口、8frame采集、默认参数及Suite完全保持。实际恢复先保存当前非空history到新 profiler-history-before-logic-recovery.raw，再 LoadProfile(cpu-gc.raw,true)，只解析大于原last的新增帧；最多300帧，超界拒绝，不静默截断。逐Main Thread导出 Driver.StepOneTick marker和其连续子树中的GC metadata/栈，root包括54 observer Begin/End，因此必须按实际栈区分生产与仪器；不能把包含observer的marker直接称纯logic0GC，也不能自动给root分配绝对tick。

先反射RED→GREEN，必要旧影响域；Settings before/after全字段相同，fresh logic-gc-callstacks-recovered.json 与 logic-gc-recovery-state.json，原PARTIAL/失败保持。没有event/没有栈/输入不全只说明未知，不刷PASS。恢复菜单执行后只读结果/保护hash/原Scene clean，运行 validator/diff-check。不启动新性能/GC测量，不认证FPS。回滚仅recovery-backups-01准确现状，另需用户授权。

官方2022.3 RawFrameDataView.GetSampleChildrenCountRecursive 文档明确下一兄弟下标=sampleIndex+recursiveChildren+1；用它限制连续子树而非单独时间重叠。https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Profiling.RawFrameDataView.GetSampleChildrenCountRecursive.html

两次只读PowerShell表达式错误（数组索引/foreach管道）退出1，没有写文件或提交Unity操作；更正后只读核验成功，不用错误结果作证。
