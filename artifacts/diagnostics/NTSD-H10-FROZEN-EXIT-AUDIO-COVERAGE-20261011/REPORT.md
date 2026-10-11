# H10 冰冻退出声音链：R7–R9 窄读结果

Task：NTSD-H10-FROZEN-EXIT-AUDIO-COVERAGE-20261011。

**终态：COMPAT_FROZEN_ROUTE_STATIC_CONFIRMED_NATIVE_INDIRECT_CALLBACK_UNKNOWN。** 已确认兼容退出链在明确前提下静态到达 IceShatter；排除了“RunStateExitPhase正常返回true而短路该派发”的猜测，也确定`commitLatch`不是可以凭名称解释的回调开关。已读native绑定方法没有直接调用旧退出派发，但其直接callee的回调合同不在本次许可内，**仍不能证明正式native路径到达或绕过IceShatter**。

没有运行，没有观察到实际漏声，也没有证据宣布所有模式永不触发。本轮不提出生产补丁、不加cue、不改规则；只收口已证的静态层，保留准确的一个间接回调问题。R1–R4的同代定义声音值覆盖继续复用，不重开。

## 授权、指纹与实际读取

人类对明确的三方法窄读提案回复“开始吧”，本轮据此新建TASK及保护证据；旧提案未批准的历史和越界事件未修改或追溯合法化。唯一写域为本新目录，既有文件零写域，无Editor/Unity、测试、编译、测量或子代理。

| 原文件 | 原SHA-256 |
|---|---|
| `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Character.cs` | `7e59a58765dbccd2c214ad976dc177c76a5f54fef34164145395edb59a89fd3c` |
| `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs` | `70e3da17fbdc144076555940f6d37a3b39cd76a14759b30ba15766e46eb7ad5b` |

实际输出共13个不重复源码行，见 [read-summary.json](read-summary.json)；逐次检索目的、方法区间、原SHA、原行文本见 [read-log.jsonl](read-log.jsonl)。方法区间不当作完整方法读取声明。

| R项 | 实际输出行号 | 范围 |
|---|---|---|
| R7 RunStateExitPhase | 1522、1524、1525 | 声明、直接调用、bool返回 |
| R8 DispatchCurrentStateEvent | 745、747、760 | 声明、当前state选择表达式、Frozen一条路由；不读其他state分支正文 |
| R9 BindNativeC25Action | 6923、6925–6930 | 声明以及该短方法的六个直接调用/赋值和guard行；用于确定调用先后、commitLatch参数选择与是否存在直接退出调用，不展开callee或周边事务 |

R9只有这六个可执行语句行，定点调用/guard匹配覆盖了它们；本报告如实列出，不把数量隐藏成只读一条调用。未输出文件或周边帧事务，不读取其他动作/伤害/生命周期实现。

## R7：正常返回必为false，不短路兼容派发

当前精确先后为：

1. `OnLocalInputStateExit()`。
2. `return false`。

复用上一获批的`StateExitEvent:689`：

```csharp
return RunStateExitPhase() || DispatchCurrentStateEvent("state_exit");
```

因此，**当`OnLocalInputStateExit`正常返回时，左侧为false，右侧派发会执行**。这个方法本身没有额外native/frozen guard或返回true的分支。没有展开`OnLocalInputStateExit`，不声称它不修改任何状态或不会抛异常；后续路由结论精确限定为正常返回后、实际派发时的Frame状态，不因此启动全输入审查。

## R8：使用派发当时的Frame.D.state，Frozen会进入已知请求链

选择器为`(Frame.D?.state ?? -1) switch`，Frozen分支直接为：

```csharp
LF2States.Frozen => State_Frozen(eventType, eventData)
```

复用上一获批的`State_Frozen:1625`，它把相同`eventType/eventData`传给`_damageStateResolver.StateFrozen`。此前合法读取的resolver在`eventType == "state_exit"`时请求`NTSDGlobal.Sound.IceShatter`，即`Battle/Ice/Shatter`。

**可以关闭到静态层的条件链：**进入`StateExitEvent`，前置调用正常返回，并且派发当时`Frame.D`非null、`Frame.D.state == LF2States.Frozen`，则会路由到该IceShatter请求。Frame.D为null时选择器为-1，不会选择Frozen分支；其他state分支未展开。本结论不把“退出前曾处于冰冻”替换成“派发时仍是Frozen”，也不证明正式native入口一定调用这个兼容`StateExitEvent`。

状态为 **STATIC_CONDITIONALLY_REACHABLE（兼容路由）**，不是实景播放、事件计数、可听验证或已观察漏声。

## R9：commitLatch的实际局部用途与native证据边界

本方法的已读先后是：

1. 调用`WriteCurrentFrameId(action)`。
2. 把`FrameCache.GetNativeFrameDataById(action)`的返回值赋给`Frame.D`。
3. 若`Frame.D != null`，调用`Trans.SyncDirectFrameData(Frame.D.wait, Frame.D.next, commitLatch ? action : Trans.WaitCounter)`。
4. 仅在`Frame.D == null`的else路径，再检查`commitLatch`；为true才调用`Trans.SyncWaitCounterFrame(action)`。为false则没有这两个Trans调用。

所以可以确定：**commitLatch选择第一种同步调用的第三个实参，并控制缺帧时第二种同步调用是否发生。** 本次没有读取同步方法内部，不额外声称它们具体写哪些字段；更不能把bool解释为“是否执行状态退出回调”。

绑定方法中没有直接`StateExitEvent`、`DispatchCurrentStateEvent`或IceShatter调用。此前R6已合法保存的native帧事务条件分支调用这个binder，但“binder没有直接退出调用”**不等于**“这些callee不可能间接派发退出”。本轮不能声称已证明静态绕行。

当前正式路径状态为 **UNKNOWN_INDIRECT_CALLBACK_CONTRACT**。这不代表发现新的性能/规则缺陷，只代表尚无足以排除这条具名清单差异影响的完整静态调用证据。

## 剩余一个准确问题，不自动向外追

问题是：**BindNativeC25Action的这些直接callee是否会派发旧state_exit，还是只完成不派发该回调的直接帧/cache/计数操作？** 准确调用表达式与位置见 [remaining-question.json](remaining-question.json)：

| 已观察callee | 调用行 |
|---|---|
| `WriteCurrentFrameId` | LF2Entity.cs:6925 |
| `FrameCache.GetNativeFrameDataById` | LF2Entity.cs:6926 |
| `Trans.SyncDirectFrameData` | LF2Entity.cs:6928 |
| `Trans.SyncWaitCounterFrame` | LF2Entity.cs:6930 |

这些callee的声明归属及正文未在本轮获批区间外新核，不能猜文件/类归属。若主会话认为必须解决该剩余问题，可先复用合法已有合同；确需正文时再批准准确回调切片。本Task到此停止该支，**不自动新建后继、不申请整个Q06、不把所有可能helper副作用变成无限全规则认证**。

`OnLocalInputStateExit`也未展开，但兼容结论已明确采用“正常返回、派发时确为Frozen”的前提；本轮不以证明它的所有输入副作用为新任务。

## 清单差异、生产决定与证据复用

- 原1000cue没有`Battle/Ice/Shatter`仍按旧精确映射成立；没有因为资源名含066而新增`SFX_066`别名。
- 新证据证明兼容条件路由，而不是正式运行事件；**尚不能把该差异判为正式应预加载的已证漏项，也尚不能排除其对正式路径的影响**。
- 因此没有最小生产补丁可合理提交，不列猜测写域、不新增cue。若主会话后续取得正式触发证据，才按原行为/目标行为与最窄验证另行审阅。
- R1–R4、已应用PCM和formal每局owned成果、唯一完整目录218.313758MiB对象账本继续按原范围复用；没有重跑小组/ABBA/全目录或重算1000映射。
- Menu仍UNKNOWN_TOOL_QUERY_UNAVAILABLE，预算UNDECIDED，256/320MiB不是批准值；完整预加载、声音品质/编码和既有规则不变。
- 原compat13/14单失败、成本尖峰、Menu误毁/恢复限制及旧越界记录保留。本轮不代H10整体、H07、Android或Player验收。

## 实际命令与保护

开始只读CURRENT-AUTHORITY和三符号的精确`rg -n`声明。后续Python按两份准确文件中的三个批准方法区间匹配声明/条件/返回/直接调用，逐行记录；没有`-C/-A/-B`、全runtime正文扫描或未批准helper阅读。上一Task的R5/R6有限行及允许resolver/常量材料通过 [reused-legal-evidence.json](reused-legal-evidence.json)回链，旧越界片段没有用于证据。

只读Git命令为`git status --short --branch`、`git rev-parse HEAD`、`git diff --cached --binary`、`git diff --binary`；文件身份用SHA256。最终具名指纹、逐行文本一致性及Task外Git保护见final-audit.json。本轮没有代码改动，ChangeLedger不适用且未运行；没有任何Editor、测试、编译或测量命令。

本有限Task已完成，机器终态见 [RESULT.json](RESULT.json)，分支证据见 [branch-and-routing-evidence.json](branch-and-routing-evidence.json)。只交回独立审阅，不自动派发或推进其他点。
