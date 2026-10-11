# H10 native绑定四个直接helper：回调合同窄读

Task：NTSD-H10-NATIVE-BINDING-CALLBACK-COVERAGE-20261011。

**终态：NATIVE_BINDING_EXIT_ROUTE_UNKNOWN_EDITOR_OBSERVER_AND_SYNC_RUNTIME。** 本轮发现两个实际间接入口：Editor条件编译下的`FrameWriteObserverForDiagnostics?.Invoke`，以及两个同步方法共同调用的`SyncRuntime()`。它们不能被当成已证明无回调的数据写入；但本轮也没有证据证明它们派发旧`state_exit`。因此，**具名native绑定链仍未知，不能宣布静态绕行，也不能宣布正式IceShatter静态可达或已观察漏声。**

没有确认具体生产缺陷，没有合理生产补丁可提交；不删诊断观察者、不加cue、不改规则。缓存查询的本地实现和各计数写入已具体化；剩余问题不是整个Q06或所有模式的重新认证。本有限Task到报告交回为止，不自动扩读或派新任务。

## 1. 本轮批准、原SHA与读取范围

人类对明确的四个直接helper提案回复“开始吧”，本轮仅据此读取必要guard/return/调用与数据操作。旧未批准提案和越界记录原样保留，不追溯合法化。所有写入仅为本新目录新增文件，既有源码/资源/Scene/Settings/报告零写域。

| 源文件 | 当前SHA-256 | 基线范围 |
|---|---|---|
| `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs` | `70e3da17fbdc144076555940f6d37a3b39cd76a14759b30ba15766e46eb7ad5b` | 原保护中已有，前后重核 |
| `Assets/NTSD/Scripts/Animation/Character/LF2FrameCache.cs` | `fe2fc55a5042628a4f1faa9643c526e9ca07b9df3cecaf8b404aeddb640c10c2` | 本轮新纳入，与派发预期一致；不冒称上轮120项已保护 |
| `Assets/NTSD/Scripts/Animation/Character/FrameTransistor.cs` | `29096ccdceffa9443ccba8cd8c5565fe86bfc5e2097336942dd64ae4e12e2f4e` | 同上 |

实际输出27行，见 [read-summary.json](read-summary.json)；逐次目的、SHA及原文见 [read-log.jsonl](read-log.jsonl)。这四个都是小方法，必要行覆盖全部可执行语句，并包括声明及R10条件编译指令；本报告如实说明，不伪称只读单条调用。

| R项 | 当前方法区间 | 实际输出行号 |
|---|---|---|
| R10 WriteCurrentFrameId | 1237–1252 | 1237、1239–1240、1242–1251，共13行 |
| R11 GetNativeFrameDataById | 98–104 | 98、100–103，共5行 |
| R12 SyncDirectFrameData | 80–88 | 80、82–85、87，共6行 |
| R13 SyncWaitCounterFrame | 116–120 | 116、118–119，共3行 |

没有读取周边帧/伤害/RNG/生成/生命周期事务，也没有读取额外helper或setter正文。

## 2. R10：Editor存在明确诊断代理调用

准确先后为：

1. `Frame == null`时立即返回。
2. 仅`UNITY_EDITOR`编译下捕获`previousFrameId = Frame.N`。
3. 写`Frame.N = frameId`。
4. `Runtime != null`时写`Runtime.Frame = frameId`。
5. 仅`UNITY_EDITOR`编译下，若`previousFrameId != frameId`，执行`FrameWriteObserverForDiagnostics?.Invoke(this, previousFrameId, frameId)`。

因此，Editor分支存在**静态有条件可达的诊断代理调用**：Frame非null、帧号变化、观察者非null。代理触发发生在上述两处写入之后，参数包含实体及旧/新frameId。本轮没有读取观察者注册点、订阅者或当前实例值；不知道它当前是否为空，更不能把“有代理调用”直接解释成“会调用state_exit”。

在**未定义UNITY_EDITOR**的编译中，捕获/比较/观察者语句被条件编译排除。这只是源码条件事实，不是实际Player构建或运行验证；也不能消除后述SyncRuntime未知项。

Frame.N与Runtime.Frame的赋值语句已记录，成员声明或setter未额外展开。本报告没有据此发明某个setter副作用或宣称其无副作用；当前两个实际间接入口已足以说明为何不能给出整链无回调证明，不据理论可能性泛开类型审查。

## 3. R11：局部实现是索引读取，没有显式退出派发

`Wrapper?.characterData == null`或`(uint)frameId >= NativeMaxFrameIdExclusive`时返回null。否则返回`_frames[frameId]`；其为null时，只有`(uint)frameId < NativeZeroFrames.Length`才取`NativeZeroFrames[frameId]`，不满足则返回null。

本方法只展示空检查、无符号边界比较、索引读取和空合并，没有显式项目helper调用、event/delegate Invoke或旧退出派发。标准数据操作按既有语义理解，没有因为任意理论副作用去审标准库。**这个局部事实不扩为整个cache生命周期、并发、容量或性能证明。** 不为已经读清的查询体再发明一个优化任务。

## 4. R12/R13：具体计数操作后共同进入SyncRuntime

R12 `SyncDirectFrameData`：

1. `wait = frameWait < 0 ? 0 : frameWait`。
2. `next = frameNext`。
3. 只有`directWaitCounter != int.MinValue`时，写`waitCounter = directWaitCounter`。
4. 随后调用`SyncRuntime()`。

R13 `SyncWaitCounterFrame`：先写`waitCounter = frameId`，随后同样调用`SyncRuntime()`。

因此已证实上一轮binder的`commitLatch`参数如何影响这些局部写入：非null帧路径传入action或当时的Trans.WaitCounter，缺帧且commitLatch为true的路径调用R13。两个方法都没有直接旧state_exit调用，但**均到达同一个尚未获批读取的项目helper `FrameTransistor.SyncRuntime`**。不能由名字推断它只写字段或绝不回调。

准确结果见 [helper-callback-evidence.json](helper-callback-evidence.json)。R12/R13共享的是一个未证helper合同，不是两个待实现的新优化点。

## 5. 对具名native链的决定

复用上轮获批binder顺序：WriteCurrentFrameId → native帧查询/赋值 → 按帧是否存在和commitLatch进入两个Trans同步方法之一；原native帧事务对binder的七个有限调用片段不重读。结合本轮证据：

| 路径/层级 | 本轮可以证明 | 不能声称 |
|---|---|---|
| Editor写frameId | 符合条件时可Invoke诊断观察者 | 当前观察者为空、其订阅者无state_exit或一定派发state_exit |
| 无UNITY_EDITOR编译 | 诊断观察者分支不编入 | 整个Player路径已验证无退出事件 |
| 帧查询方法 | 没有显式退出/代理调用的索引返回实现 | 所有cache或其他路径都无回调 |
| 两个Trans同步方法 | 写入所列计数后调用SyncRuntime | SyncRuntime内部无回调或有state_exit |
| 具名正式native绑定链 | 仍为UNKNOWN_INDIRECT_CALLBACK_CONTRACT | 静态证明绕行、正式IceShatter静态可达、实测漏声或所有模式永不触发 |

上轮的兼容条件链仍有效：StateExitEvent进入、OnLocalInputStateExit正常返回，派发时Frame.D.state确为Frozen，则到达已知IceShatter请求。该兼容链与正式native入口之间的连接，不能用本轮代理存在或方法命名补造。

原1000cue未含`Battle/Ice/Shatter`的差异保留，没有新增SFX_066别名。当前证据尚不能把它判为正式应预加载的漏项，也尚不能排除此差异对所有相关路径的影响。**没有已证具体bug，就没有合理的生产修复写域/目标行为可提出；不为消除UNKNOWN改规则或删除观察者。**

## 6. 精确剩余问题与停止位置

完整记录见 [remaining-question.json](remaining-question.json)：

- **`FrameTransistor.SyncRuntime`**：已证调用点为FrameTransistor.cs:87和:119。只缺它是否派发旧退出/其他回调，还是数据同步的合同。正文和声明位置未在批准方法区间外新核，不自动读取。
- **`LF2Entity.FrameWriteObserverForDiagnostics`的订阅者合同/实际是否启用**：已证调用点为LF2Entity.cs:1250，仅Editor且帧号改变、观察者非null时执行。若主会话要决定当前Editor链的行为，需要准确的合法订阅者证据；本轮无Editor查询或观察者注册点读取授权。它不是Player分支的问题。

是否必须解决这些残余、能否复用合法已有合同，由主会话独立决定。本报告不自动派新任务，不请求整个Q06，也不重开R1–R4、旧1000cue图或任意外部string输入。

## 7. 复用、命令与保护

本轮只使用CURRENT-AUTHORITY、四个批准方法、原获批有限行与已有精确报告；旧越界片段未作依据。复用文件身份见 [reused-evidence.json](reused-evidence.json)。PCM及formal每局owned成果、原完整目录218.313758MiB与释放证据按原范围保持，没有重跑全目录、小组或ABBA。

实际只读命令：`rg -n`在三个准确文件上核四声明；Python按四方法区间输出必要语句并逐行留痕；SHA256核具名文件；`git status --short --branch`、`git rev-parse HEAD`、`git diff --cached --binary`、`git diff --binary`用于前后保护。没有广域-C/-A/-B或其他方法体检索。无代码变更，ChangeLedger不适用且未运行。

最终指纹、27行文本一致性与HEAD/staged/tracked/任务外状态见final-audit.json。所有新增物只在本Task目录；无Editor、测试、编译、测量、源码/资源/Scene/Settings修改、清理、push或子代理。

Menu UNKNOWN_TOOL_QUERY_UNAVAILABLE、预算UNDECIDED及既有阶段边界保持；不选256/320MiB、不改音质/编码、不裁catalog或lazy声音。旧compat13/14失败、成本尖峰、Menu误毁/恢复限制和越界事件全部保留。本有限Task完成即交回，机器终态见 [RESULT.json](RESULT.json)，不代H10整体/H07/Android验收，不恢复Goal/队列或其他点。
