# H10 SyncRuntime：具名 native 绑定链的退出回调疑点已关闭

Task：`NTSD-H10-SYNC-RUNTIME-CALLBACK-COVERAGE-20261011`。

**结论：`NATIVE_BINDING_LEGACY_EXIT_STATIC_BYPASS_UNDER_APPROVED_CONDITION`。** 在非 `UNITY_EDITOR` 编译，或 Editor 的 `FrameWriteObserverForDiagnostics` 未安装这一已明确的前提下，本次检查的 `BindNativeC25Action` 绑定链不会通过这些 helper 派发旧 `state_exit`。最后一个未读 callee `FrameTransistor.SyncRuntime()` 只有空检查、返回及两次数据同步，没有退出事件、委托或项目回调调用。这个具名间接回调疑点在上述条件下已静态关闭，不再以未读 `SyncRuntime` 为由保持 UNKNOWN。

“非 UNITY_EDITOR”是源码预处理结论，不是 Player 运行证书；“Editor 观察者未安装”是适用条件，本轮没有测量当前 Editor 的观察者值。已安装观察者分支不属于此结论，也不由本任务自动引出订阅者审计。没有发现需要补丁的新生产缺陷，不增加 cue，不改变规则。

## 新增证据与读取范围

本轮唯一新增源码正文读取是 [FrameTransistor.cs](../../../Assets/NTSD/Scripts/Animation/Character/FrameTransistor.cs) 的 `private void SyncRuntime()`，方法区间 136–143。实际输出第 136、138、139、141、142 行，共 5 行；覆盖这个小方法的全部可执行语句及声明，省略空行和括号，不读取相邻方法正文。

源码 SHA-256：`29096ccdceffa9443ccba8cd8c5565fe86bfc5e2097336942dd64ae4e12e2f4e`。

| 顺序 | 行 | 实际语句及含义 |
|---|---|---|
| 1 | 138–139 | `_entity == null` 时立即返回。 |
| 2 | 141 | `_entity.Runtime.WaitCounter = waitCounter;` |
| 3 | 142 | `_entity.Runtime.NextFrame = next;` |

这里没有对旧状态退出、事件、委托或其他项目方法的调用。按已批准的数据操作边界，不把普通字段赋值扩展成假设性 setter、框架或标准库副作用审计。原始逐行证据见 [read-log.jsonl](read-log.jsonl)，行数与方法边界见 [read-summary.json](read-summary.json)，精确判定见 [callback-evidence.json](callback-evidence.json)。

## 与已有获批证据合并后的调用顺序

复用上一轮的 R10–R13 和前一轮的 R9；本轮没有重新读取这些方法正文。

1. `BindNativeC25Action` 先调用 `WriteCurrentFrameId(action)`。后者先检查 `Frame == null`，再写帧号；非 Editor 编译排除诊断观察者代码，Editor 观察者未安装时不会经该委托调用订阅者。
2. binder 调用 `GetNativeFrameDataById(action)` 并赋给 `Frame.D`。已核过的查询仅有空检查、无符号边界比较、索引读取和空合并，没有显式退出派发或项目 helper 调用。
3. `Frame.D != null` 时进入 `SyncDirectFrameData`：同步 wait/next，按参数条件同步 waitCounter，然后调用本轮核对的 `SyncRuntime`。
4. 缺帧且 `commitLatch == true` 时进入 `SyncWaitCounterFrame`：写 waitCounter，然后调用同一个 `SyncRuntime`。缺帧且 `commitLatch == false` 时不进入这两个同步方法。

两个同步分支都在本轮确认的数据同步处结束，因此不再存在这条绑定链上未检查的额外项目 callee。`commitLatch` 仍只按既有证据解释为参数/同步分支选择，不能称为退出回调开关。

| 问题 | 本轮结论 |
|---|---|
| `SyncRuntime` 是否另调退出事件或项目回调 | 否；本方法只有上述数据同步。 |
| 具名 native 绑定链，在批准条件下是否到达旧 `state_exit` | 静态旁路；该间接回调疑点关闭。 |
| 当前 Editor 是否确实未安装观察者 | 未查询；仅作为本结论的适用条件。 |
| 已安装观察者的订阅者行为 | 不在本轮范围，不作为批准条件下的未关闭项。 |
| 所有 native 入口、所有模式是否永远不触发 IceShatter | 本任务没有作此全域判断。 |
| Player 实际运行、可听表现或漏声 | 本轮未运行，不能据此声称通过或发生缺陷。 |

旧报告在各自时点记录的 UNKNOWN 保持原件；本报告仅以前述新增证据和最新适用条件推进这个具名问题，不覆盖旧事实。

## IceShatter 与证据复用边界

已有兼容路径结论保持：进入 `StateExitEvent`、`RunStateExitPhase` 正常返回 false，且派发时 `Frame.D.state` 为 Frozen，便能沿既有 Frozen 分支到达 `StateFrozen("state_exit")` 的 `Battle/Ice/Shatter` 请求。这是兼容路径的条件性静态可达证据，不等于本次 native 绑定链会调用它。

原 1000-cue 目录没有 `Battle/Ice/Shatter` 的事实保持；没有将其他文件名视作别名。仅凭这项目录差异及兼容路径存在，不能声称已证明正式 native 路径漏声，也不能据此强行新增预加载 cue。

R1–R4 的同一 prepared generation、无外部变更条件下的值域证据，既有 PCM 一致性、formal 逐局 ownership、全目录 `218.313758 MiB` AudioClip 对象账本均按原范围复用。此次源码/资源没有修改，没有重跑、重解码或重新计算这些测量。它们仍不是 RSS、整局峰值、CPU、设备或全 producer 准入证明。复用原件及哈希见 [reused-evidence.json](reused-evidence.json)。

## 实际核对与未执行项

实际只读核对包括：

- `rg -n '^\s*(public|private|internal|protected).*\bSyncRuntime\s*\(' Assets/NTSD/Scripts/Animation/Character/FrameTransistor.cs`，定位声明为 136 行。
- 内联 Python：核源码 SHA-256，只提取获批方法内必要语句，排他新增读取原件；最终逐行复核日志与源码一致。
- `git status --short --branch`、`git rev-parse HEAD`、`git diff --cached --binary`、`git diff --binary`，前后原件保存在本目录。
- 对 `before.json` 中具名的 166 个既有文件做前后 SHA-256 对比；核对任务外 Git 状态、HEAD、暂存和 tracked diff；检查本报告本地链接。

最终保护结果以 [final-audit.json](final-audit.json) 为准。保护范围是这 166 个具名文件加上述 Git 检查，不扩大为整个磁盘或当前 Editor 内存状态证明。

本轮没有 Unity/Editor 查询、编译、focused tests、Play、性能采样、设备验证或音频加载；没有改项目代码、资源、Scene、Settings、旧证据或治理文件。没有新增运行时 owner、缓存或生命周期责任。只有本 Task 目录内新增文件，未删除或覆盖文件；ChangeLedger validator 因无脚本改动不适用，未运行，不能写成通过。

## 交付状态

本有限 Task 完成，机器结果见 [RESULT.json](RESULT.json)。H10 父项保持 OPEN，预算保持 UNDECIDED，Menu 状态保持既有 `UNKNOWN_TOOL_QUERY_UNAVAILABLE`；历史兼容组 13/14 失败、成本尖峰及其他既有事件原样保留。没有 H07 阶段通过或 Android 最终认证结论，没有恢复 Goal、队列或其他优化任务。本轮不自动生成下一项任务。
