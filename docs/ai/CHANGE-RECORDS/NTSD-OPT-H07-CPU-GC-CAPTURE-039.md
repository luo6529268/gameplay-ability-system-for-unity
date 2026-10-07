<!-- CHANGE-RECORD
id: NTSD-OPT-H07-CPU-GC-CAPTURE-039
status: RUNTIME_PENDING
code-path: Assets/NTSD/Scripts/Test/Editor/BattleOptimizationWindowsAiSuiteEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleOptimizationCpuGcCaptureEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleOptimizationCpuGcCaptureEditorTests.cs
authority: user 2026-10-07 accepts one bounded CPU/GC callstack capture and conditional admission of existing BruteForce candidates; no new battle rules or backend
evidence: artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH39-CPU-GC-CAPTURE-20261007/REPORT.md
-->
# 第39批：单次千人CPU/GC调用栈采集
RUNTIME_PENDING / RECOVERED_CPU_EVIDENCE（自动8帧截止未通过，全线程未知）；限定热点诊断已经交付，不启用任何碰撞候选，不声称FPS或完整0GC改善。原单窗与失败不重跑，下一已有Brute必要生产准入；原8dirty副本/15保护保持。

## 来源、原状与边界
用户在“允许一次当前千人场景短窗口CPU/GC调用栈；允许已有Brute必要一致性通过后接入普通战斗，不换后端/规则或解冻专项门”的确认问题后回复“可以”。这是新范围授权，不取消所有验收。14:55原Editor PID19040/6400 Menu非Play idle，无测试；CPU Profiler disabled、CPU area false、allocation stack false。CLI无Pipeline不是Editor关闭；existing execute_code CodeDom失败（文件名/扩展名太长），不改第三方桥、不安装依赖，改走自有Editor菜单。
截图0.7FPS不是当前活动Play采集。本批复用已固定Combat1000/120warm+180sample一次，开启Profiler的仅8个已完成PlayerLoop帧，捕获耗时受采集干扰，不直接冒充截图或性能合格。
更正：上句“仅8个”是事前目标，不是实际通过结果。实际回调0/45秒截止后从同一raw恢复8有效主帧，自动截止未通过，完整失败见下文和REPORT。

## 改前/改后责任、关闭与副作用
现有Suite增加单独capture-only入口/单run/新输出/Observe与before-shutdown Restore；原6/4/2窗口请求和候选flags默认行为保持。新增Editor static capture owner，无Scene组件，warmup120后实际AI/baseRoster1000才开始；没有GPU/FrameDebugger/EXT1M0。
Profiler保存enabled、binary、log path、allocation callstacks、CPU area、profileEditor、deepProfiling；拒绝已有recording/deep profiling，不切用户已有深度模式。capture仅8 completed frames/45秒边界；完成/退出Play/assembly reload/异常/owner shutdown恢复。恢复与数据导出在战斗World关闭前，不启动新tick/自动恢复任务。Binary和raw JSON只写fresh声明输出；提取sample names/callstack在Profiler恢复后进行，不能污染采集帧。无任何Unity逻辑字段写入、资源布局/segment/预算变更。
新Editor bookkeeping不是0GC生产代码；采集扰动与工具alloc单独标注，原H11严格失败保持。

## 验收与恢复
先新10具名测试RED（反射缺API），再最窄GREEN+旧Suite请求回归，原Editorcompile。唯一一次单Combat窗口：8有效Profiler帧/main+render samples/GC alloc stack或明确no events、普通四flag仍false、实际1000AI/120warm、全300正常窗口与原11阶段关闭/双Scene SHA/Menu clean恢复、Profiler原状态一致。仅现场采集，不要求更多角色或历史全面对齐。
每tick/native生产准入另在下一包冻结，不把末tick或Profiler结果替代；用户已准许该后继范围但本批无正式默认提升。
Tools/Validate-ChangeLedger.ps1以及准确diff检查必跑，历史warning保留。
恢复按本Operation fresh副本逐文件另行获批执行，不reset/restore/delete；新增代码不自动删除。C#改后实际命令/结果/未验证域继续追加。

## Test-first 实际记录
原PID19040 Editor refresh后短暂6400拒绝连接，随后重新检查PID-owned端口仍为6400并成功重连，未重启或另启Editor。新tests.cs与meta已写，原Editor编译进入idle；run_tests只过滤BattleOptimizationCpuGcCaptureEditorTests，job abd50a490f794f62acdda69a7ca60dc1实际完成10项，10项因缺BuildCpuGcCaptureRequest/IsCaptureReady实现失败（有效RED），未执行全库9443项。原响应见tests-red.json；接着仅实现本批声明的capture-only Suite和Editor owner，GREEN/采集尚未完成。

实现已写：Suite新增cpuGcCaptureOnly/单request/单run/既有Observe和shutdown接入，其他request与四候选未改；新owner快照和恢复CPU/GC、memoryRecordMode，8帧记录回调只写预分配index并关闭录制，恢复后才导出样本/GC metadata和堆栈，45秒/退出/重载同一幂等终止。各线程扫描128上限若触顶标PARTIAL，不猜全线程；样本须有PlayerLoop才计有效主线程帧。deep/GPU profiling原已开启时拒绝采集不抢占，Profiler旧history不清空。无Script/runtime/Scene资源改动。

原Editor refresh/domain reload后PID19040桥迁到6401，只复核PID/port后重连；get_editor_state明确Menu/idle/不compiling。GREEN job 5c0be03ce38c47dc852b8ee8190b1ac7已真实完成77/77、0failed/skipped、4.7720017秒（新10+旧Suite67），完整raw见tests-green.json。原catalog9443不是执行数。状态RUNTIME_PENDING仅指采集还未执行，不再称代码未编译或聚焦未跑；下一唯一39菜单单窗，运行中禁止C#/Assets修改/refresh。

单次窗口实际完成：120+180/1000AI、suite DONE/MEASUREMENTS_COMPLETED，orderedShutdown三残留0/双Scene unchanged/原Menu restored。新binary 127133652B保留；采集state为PARTIAL，NewProfilerFrameRecorded没有回调、45秒保护截止，导出0帧；前后完整ProfilerSettings相同。不是0GC或FPS PASS。原自动截止契约尚未通过，不能写成8帧成功。接着在同一声明Editor脚本内仅添加raw离线恢复菜单，原Editor idle且history为空才keepExisting导入同一raw，跳过首帧取最多8，新recovered输出不覆盖原件；所有提取在disabled/noPlay下，没有第二次实测。恢复编译及样本有效性待。

## 本批交付与剩余（2026-10-07）
raw恢复已执行：Main8/8+Render8/8，线程扫描128上限触顶，state保持PARTIAL，不虚报全线程或自动截止。恢复后原77/77再次通过，job998aaf1e7dba458fa55a5dc0f46ea66e（4.0342941s）；三源最终版本进入原Editor程序集，采集与恢复全部ProfilerSettings同/areas同、原Menu idle。raw前后SHA同，所有失败及新recovered输出留存。未重跑压力/第二次录制。
实测热点：整窗logic475.685/collector439.602ms（92.414%），显示1105.438ms；8帧render waits>99%、中央物化9.565–21.202ms/Execute0.061–0.200ms；CPU中央命令与SetPass约2000，真实GPU batch未验。全部是instrumented诊断，不是FPS收益。发现runtime OPoint admission20B×235与替代声音36B×1；其余大量Editor polls分开，不代替35 camera2事件归因，不改运行时源。
末tick300的20hash与4.2MB完整snapshot同37；这不是逐tick/native准入。15保护/8fresh备份/HEAD同，validator1335/14 PASS（4247历史warning），详细原件和最终audit回链REPORT。本Record保留RUNTIME_PENDING以诚实记录自动捕获护栏与全线程缺口，但热点证据足够进入已授权后继；不重复本版诊断、不机械停Goal、不声称生产已推广。

