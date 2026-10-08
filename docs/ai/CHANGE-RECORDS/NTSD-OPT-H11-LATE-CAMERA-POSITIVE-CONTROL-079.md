<!-- CHANGE-RECORD
id: NTSD-OPT-H11-LATE-CAMERA-POSITIVE-CONTROL-079
status: PLANNED
code-path: Assets/NTSD/Scripts/Test/Editor/BattleCentralProductionWindowSceneProbeEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleOptimizationCpuGcCaptureEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleCameraGcCallstackCaptureEditorTests.cs
authority: user six-item optimization and current task sections0-8 H11 necessary attribution; known-positive diagnostic only
evidence: artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH79-LATE-CAMERA-POSITIVE-CONTROL-20261008/REPORT.md
-->
# H11 late-camera已知正事件归因资格
状态PLANNED；[Task](../TASKS/NTSD-OPTIMIZATION-BATCH79-LATE-CAMERA-POSITIVE-CONTROL-20261008.md)、[Operation](../FILE-OPERATIONS/NTSD-OPTIMIZATION-BATCH79-LATE-CAMERA-POSITIVE-CONTROL-20261008/RECORD.md)。
Unity原状：47工具fixed8ring可等至1800或真实事件，但唯一窗口无事件，从未执行真实raw正归因；43 ordinal937有效12events仍未知。不要以工具请求/合成boundary测试代替真实栈证据。
预期改动：仅三个Editor现有文件；请求默认关闭的新受控正对照、第9/最多16camera单次已知1MiB分配、固定本包raw恢复入口与12测试；不改正式runtime或旧验收门。
副作用：本正对照故意有GC/Profiler开销，新fresh raw/历史备份/归因输出与Test Runner公共XML更新，均已Operation登记；非基准/FPS/0GC PASS。
生命周期：Report-only flags默认OFF、pure门控、NoInlining helper数据存活至KeepAlive；原Probe停止录制/Dispose、原CPU FinishAndRestore、原11stage shutdown不变，无新正式模块。原owner设置恢复、raw append不得clear；旧history备份保留。
验收/退出：有效12RED→53GREEN（旧33＋literal8），唯一已知正camera或先到自然事件、有校准scope/root totals与raw count对应、known stack（若已注入）、settings恢复/原SceneSHA/关闭0、保护/治理通过。No-event/invalid/部分记录诚实FAIL，不复跑刷PASS；不能据人工事件证明原43已闭。
不可回退边界：既有所有dirty/旧失败证据、Scene/资源/正式权威与专项门；回滚仅Operation副本定位本hunk并另获批准，禁止破坏性Git。
实际文件/符号/命令/结果会按阶段追加；未运行项保持未验证，尚无新收益。
