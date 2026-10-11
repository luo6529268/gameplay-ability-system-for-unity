# H10 冰冻退出声音静态链窄读

Task: NTSD-H10-FROZEN-EXIT-AUDIO-COVERAGE-20261011
Creation status: IN_PROGRESS

本次授权：人类在讨论会话01a11fe2-bfaa-72f3-a9ed-c8260f63b193针对三个方法的明确窄读提案回复“开始吧”，通过当前具名指令转交。本授权仅覆盖R7–R9中直接关系冰冻退出的条件/返回/派发片段，不解冻整个Q06。旧未授权提案、越界事件和各失败原件不覆写，不追溯合法化。

唯一写域为本新artifacts/diagnostics目录；CREATE_NEW_ONLY，排他创建TASK、读取清单/原文、SHA/Git保护证据、JSON、报告。既有文件、源码、资源、Scene、Settings、旧报告零写域。无Editor查询、Unity、测试、编译、测量、子代理、工具修复、预算选择、catalog裁剪或生产实施。

## 精确读取许可

- R7：Assets/NTSD/Scripts/Animation/LF2Objects/LF2Character.cs::RunStateExitPhase，当前声明1522。只取frozen/native退出的guard、短路bool返回与直接派发调用，不展开其他状态行为或副作用。
- R8：同文件DispatchCurrentStateEvent，当前声明745。只取state_exit如何选择当前state、进入State_Frozen或被拦截的条件/直接调用，不读其他state方法体。
- R9：Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs::BindNativeC25Action，当前声明6923。只取冰冻state_exit回调/绕行的guard/调用及必要参数语义，不凭commitLatch命名猜测，不整段输出动作绑定或帧事务。

先重核准确声明，再限定于上述三方法区间作定点匹配；逐次输出行号/文本/原SHA与目的。不对runtime广域上下文检索，不输出完整大方法。许可外helper或路由表阻止结论时，保存准确callee/问题并停止该支，不自动扩读或增加任务。

## 复用与判定

复用上一Task获批R5/R6有限行、当前SHA一致的合法DamageStateResolver/NTSDGlobal与旧1000cue映射；R1–R4同代定义静态覆盖不重做。分别判断兼容dispatch、native frame path、事件名和frozen state的前提及短路条件；结论分静态可达、静态绕行、仍未知，无运行不得宣称已观察漏声/可听通过或所有模式永不触发。

如证实漏项，只给最小准确写域/原目标行为/窄验证/回滚供独立审阅，不修代码或资源；如能排除具名差异对当前路径影响，只关闭相应静态层。没有已证bug不转成全规则重新认证。

前后核目标及具名旧证据SHA、HEAD、staged/tracked diff、Task外Git状态。本Task没有代码改动，无新runtime状态/owner/容量/关闭责任，ChangeLedger不适用。Menu保持UNKNOWN_TOOL_QUERY_UNAVAILABLE，预算UNDECIDED，既有PCM/每局所有权与历史失败/平台/全域门保持。完成有限REPORT/RESULT即交回，不恢复Goal/队列/其他点。
