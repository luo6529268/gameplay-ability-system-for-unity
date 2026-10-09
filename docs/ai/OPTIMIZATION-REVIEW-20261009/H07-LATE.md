# H07-LATE：后续实体更新子方案

父项H-07。状态：HYPOTHESIS / Q06_BODY_NOT_READ / EXECUTION_STOPPED。优先级中，先确认细分成本再实施。

91记录LateEntityUpdate约5.8—5.9ms；不能认定这全是RefreshRuntimeSnapshot，也不能假设多数实体不变。本轮只定位到Assets/NTSD/Scripts/Simulation/Passes/LateLifecycle/BattleLateEntityLifecycleModule.cs文件，没有展开方法体。
先利用已有子相位和snapshot类型/消费者合同建立成本与字段表；若必要证据位于活跃Q06方法体，准确标明阻塞，先请求该读范围，不绕过限制搜索输出其正文。

候选方法：只有证明某个纯投影/复制段重复且可安全失效时，才在原所有者处消除重复。脏条件必须覆盖全部消费者需要的字段、tick/epoch/generation、动作/位置之外的状态；publication只读不保证旧snapshot可复用。逻辑lifecycle、事件、计数或清理不得因画面不变而跳过。
依赖：H-09对应状态/事件合同，现有publication所有权；如影响显示，再复用M-03/H-11显示/容量验收。不是依赖先完成全部渲染重构。

最小验证：先确认细分耗时与安全可跳过率；有依据后按字段/失效负例、同输入逐tick state/RNG、slot复用及必要插值/first-visible检查；完整0GC与受影响关闭门。没有跳过机会则结束该具体候选，不重复刷窗。
完成：方案能证明原时点/可观察结果不变且具体成本减少；不预定Late<=3ms、不设随意50%覆盖率作为安全证明。H07最终门保持。回滚保存准确dirty字节，禁止恢复整文件HEAD或清理其他工作。
