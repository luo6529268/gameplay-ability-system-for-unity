> VERIFIED / DECLARED_CPOINT_RAW_BINDING_AND_FOLLOWING_TICK。SelfCheck/Play1568+864/回放112场景/关闭均通过；仅关闭同Record声明范围，不关闭Q06。下一NATIVE-INPUT-ACTION-COST-FRAME-READERS-001。

# CPoint throw 原定义快照与原生 raw 绑定

READY_SOURCE_WITNESS_AND_EXACT_RECORD。下一最小reader任务，禁止重做kind2 pickup/kind3 entry/C25/collision。

已读生产链：BattleInteractionPipeline → LF2Entity.RunCpointAdvanceStep10/RunCpointCheckStep10 → BattleCpointWriter.RunKind1/ApplyThrow。ApplyThrow原catcherFrame.next预取仍HasFrame/GetFrameDataById；LF2Entity.SetCpointRawFramePreserveWait因旧HasFrame门可拒绝原生有效frame，而SetCpointRawPrevFrame2仍写snapshot号并旧getter绑定，形成action/snapshot不一致。

源authority：当前playable battle_world.cpp advance_catch_relations约5751，冻结原catcher frame；输入换动作后可读新frame的cpoint.vaction，但throw阶段center/next/旧cpoint使用原frame，直接写双方action/snapshot/counter，不以destination存在性拒绝raw写。

范围候选：BattleCpointWriter.ApplyThrow原next预取，LF2Entity两个raw setter；必须先新独立Task/Change源码见证与准确Record后修改。保护RunKind2Validation调用212，旧protected ApplyCpointThrowStep10副本无live caller则不碰。不全局替换getter，不动Scene/资源/非战斗/框架/Server。

验证：next/vaction普通声明、隐式零、高900、声明/缺失999、1000、负raw action；立即action/snapshot/descriptor/counter/latch/wait-next overrides及完整后继tick与当前C25一致。输入改动作后throw证明原center/next身份保持；关系/速度/朝向/environment不丢。throwinjury=-1定义转换需明确原source descriptor生命周期，不能随意换新定义。沿原架构，无新持久schema或关闭owner。compile/focused/source对照/selfcheck/真实Play/关闭后限定验收。

当前纠正：已建同ID精确Record。当前正式source不包含throwinjury==-1变身分支，前文旧definition转换保护前提作废；必须移除Unity该无依据live入口，保留无live caller旧helper。392源矩阵/双跑/110742独立检查PASS，source after不变身份、双方counter清零；新增Unity RED测试进行中，生产尚未修改。后续完整driver/Replay/Play/关闭与SelfCheck旧oracle按同记录推进。

当前IN_PROGRESS / IMMEDIATE_PASS_FULL_TICK_DEPENDENCY。392×两profile立即零差异、SelfCheck17:43:37Z PASS，fulltick剩28隐式99+attack场景，独立依赖NATIVE-INPUT-MISSING-STATE-ROUTING-001先处理，再回本Task完成完整driver/Play/replay/关闭。原fulltick夹具前置错误均保留证据，当前fixture已匹配nonAI/native profile/canonical input。
