# 第47批：迟发相机分配事件调用点诊断

本次有界窗口已完成：DIAGNOSTIC_NO_EVENT / CALLSITE_NOT_REPRODUCED，1800完整camera、四校准PASS、settings/关闭/Scene保护通过；50/50＋R1末33/33（51去重）通过。没有自然正事件新调用点，旧43FAIL保留，不重复本窗。R1 current只读cache/先history备份，未知容量拒绝；窗口仍绑定pre-camera源，不混称最终R1版本已再测。下一H07实际collector有据候选、先准确留痕，不等待按次数重新启动，Goal active。

状态 PLANNED；H11完整0GC保留43首次12event FAIL/重进0event PASS，H07未达，Goal active。当前阶段合同第0—8节授权范围内必要分配归因；本包不是EXT1专项M0/新FPS或生产重构。

采集后保护修正（R1，事前声明）：camera-01已1800/0event、DIAGNOSTIC_NO_EVENT，旧失败不消去，不再采样。源码复核发现ProfilerUserSettings.frameCount getter在缓存为0时会调用SetMaxFrameHistoryLength；这只是潜在副作用，不证明本次旧history丢失。仅将当前同一CPU owner改为先保存旧history，再只读m_FrameCount缓存（0/未知拒绝，不初始化、不改Prefs/原生长度），追加一个只读/首尾索引不变的聚焦测试。只复验相机fixture33case；旧CPU/literal18通过证据依赖不受此新late setup helper影响可复用，不重采/不改Probe运行中冻结版本。当前camera证据绑定pre-camera源SHA，最终源另记，不混称同版本。下方property-getter方案是R1前历史，不定义当前保护实现。

## 原状与必要性

42工具只固定首8camera，无法解释43 ordinal937有效事件。采用事件触发CPU调用栈诊断而非第三次同版0GC刷PASS；不预设第937帧复现、不猜测MCP因果。先具名测试再实现，旧39/42原模式与完整scope保持。

## 精确域和寿命

只改三个Editor C#：BattleOptimizationCpuGcCaptureEditor.cs的可选late mode/固定ring/冷history保存与现有栈导出；BattleCentralProductionWindowSceneProbeEditor.cs的Report、新47入口/创建report、可选启动/EndCamera停止和diagnostic完成判据；BattleCameraGcCallstackCaptureEditorTests.cs追加新request/ring/结束/容量/边界测试。准确路径在Record及before.json。六治理文档只状态/链接追加；新Task/Record/Operation/diagnostics属于本包。生产/默认/规则/Scene资源Settings全不写，Q06只hash。

既有Editor capture与Probe是owner，无新runtime manager/queue。READY后原两个recorder先校准；完整camera Begin/End scope不缩，两立即配对point marker仍在scope外。最多1800camera或300秒capture保护截止（原startup600/总900保持），首次有效camera事件结束后停止并保留；invalid scope终止且PARTIAL。固定8条边界ring只供诊断保留末尾信息，原Probe仍预热2048 samples全范围，不热扩容、不复跑下一cycle。结束/异常/重载/退出均先停止/恢复Profiler，再沿原十一阶段owner关闭。

CPU allocation stacks/no deep/no GPU。使用Unity2022.3 ProfilerDriver.SaveProfile（void，必须核新文件存在/非空），不是新API master bool。不开1800帧全量binary流；沿用现有Profiler history，冷读ProfilerUserSettings.frameCount，要求<=300帧，不变更EditorPrefs或清history，开启前将旧history保存到fresh profiler-history-before.raw。记录原Profiler.maxUsedMemory，只描述传输buffer上限，不冒充history/RSS硬预算。若环境不满足或原history无法备份，拒绝采集，保留错误；不偷用其它Profiler。captured cpu-gc.raw只保存有界当前history，冷Edit Mode恢复解析，新输出CreateNew，旧证据不覆盖。诊断内存不修改ATLAS稳态/过渡合同。

原history可能被引擎rolling自然淘汰，但开启前完整raw备份；不调用ClearAllFrames、ResetHistory或改变history长度，后续LoadProfile(raw,true)仅追加，不抹现有history。只在新raw的单一LateAllocationEvent point所在主线程frame取camera两point之间GC.Alloc，必须同frame、有序、计数等于calibrated recorder且metadata/调用栈完整。缺失/重复event marker、截断/丢帧/校准失败一律PARTIAL，不猜补。事件marker在camera GC范围外，不增加验收内事件。

## 冻结验证与退出

新具名request不Replay/不Timing/不DynamicInput、不修改workload；最多1800，不以无事件诊断为0GC验收。测试覆盖request合同、ordinal937 ring留存/固定容量、事件/invalid/max1800停止、负数/不完整边界、旧8完成门不回归、history guard及外部settings字段。先仅追加测试获得有效缺API RED；实现后新fixture及既有39 capture/35 literal受影响回归一次，不重跑46/235历史。GREEN、编译/备份保护/治理通过后才允许一次原saved Battle自然小roster的late事件窗口；无MCP/脚本写入during Play，正常退出/回原Menu且不保存Scene。事件无复现记NO_EVENT_OBSERVED/归因未得，不能称H11通过；有调用栈只诊断，修复另有据包。若本轮尚未取得安全采集窗口，保留pending转其它范围内动作，不重复同状态。

准确current-dirty副本9个在Operation backup，guards/HEAD事前后核对。所有编辑apply_patch、输出fresh；Tools/Validate-ChangeLedger.ps1与git diff --check按实际结果记录。不删除/移动/恢复/commit/push，回滚须另获批准以本副本为准。正式336、33/3ms/max2、checksum/RNG、publication readonly、segment/failclosed/两slot/GPU证明与十一阶段Join不变，EXT1/ATLAS/Mono保持冻结。次数只复盘，阶段未达不complete/blocked/paused。

官方API依据（本次核对）：[Unity2022.3 ProfilerAPI](https://github.com/Unity-Technologies/UnityCsReference/blob/2022.3/Modules/ProfilerEditor/Public/ProfilerAPI.bindings.cs)；[ProfilerSettings](https://github.com/Unity-Technologies/UnityCsReference/blob/2022.3/Modules/ProfilerEditor/Public/ProfilerSettings.cs)；[maxUsedMemory](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Profiling.Profiler-maxUsedMemory.html)。

## 必要收尾核对（2026-10-07T14:18:58.7414177Z）

FINITE_DIAGNOSTIC_COMPLETED / CALLSITE_NOT_REPRODUCED保持。42guards/9dirty副本/HEAD核同，原Menu8roots clean、Editor idle/nonPlay/noTest。最终validator1343/14、4257历史warning、0error/exit0，diff0；准确R1后source与设置/Scene保护见本批terminal-source-audit、editor-final-r1、validation-final。不重复1800窗口，不称调用点/生产性能已修，H07/H11仍OPEN、Goal active。
