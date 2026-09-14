<!-- CHANGE-RECORD
id: NTSD28-Q06-GT08-LIFECYCLE-FIXTURE-REBASELINE-001
status: VERIFIED
change-kind: GT08_NATIVE_LIFECYCLE_FIXTURE
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06NativeFrameTransactionEditorTests.cs
authority: Formal playable battle_world.cpp 8297..8355 resolve_pending_lifecycles_range, native step_frame_slot witness2676.
evidence: Full SelfCheck GT08 actual failure; old LateLifecycleSelfCheckEntity only writes Frame.N with no native pending, HitStun expectation contradicts native runtime_state_code.
-->

# GT08生命周期夹具

IN_PROGRESS / TEST_FIRST。仅SelfCheck CheckGameTickLateExitAndCleanupContracts前三个GT08场景及现有frame Editor test参数/encoded断言。共享type3场景改实际LF2SpecialAttack.SimFrameTick -> RunCommonFrameTick -> Native事务，声明catalog/data/0 wait next1299；不改通用LateLifecycleSelfCheckEntity，保留其它历史夹具调用。1100/1200两个实际路径补NativeRuntimeStateCode、sound/碰撞帧镜像与pending消费断言，HitStun按C25表现递减独立验证；不覆盖原生产实现。两个后端新增1200/1299原函数行对应的focused向量，原2676矩阵不重新发明expected。

前置：native encoded1100..1299写state=1100-code、action0、collision snapshot0、pending/code清零，保留latch/counter；driver先镜像previous078。范围不包含任意继承类mock不得制造producer证据。不改Unity/GAS/Scene/资源/schema。验收两新case先RED暴露旧native-state预期后修断言、focused全部原向量、完整SelfCheck真实执行。失败留档；回滚仅两文件本差量且需批准。

GT09审计发现：当前正式core/playable代码没有9998直接分支；既有SerialTickAll state9998是残余旧行为，不能据旧self-check重新认证。需另建原函数完整driver见证任务，不能在本test-only Record顺手退休生产writer。完整父GT08/GT09重新核验尚包含该未知项。

GT08两测试脚本已写：新增1200/1299先RED2/7，源runtime state实测-100/-199；共享场景已换实际LF2SpecialAttack生产帧推进，统一helper断言生命状态和四帧镜像/受击计数/声音/pending。GT09旧测试暂保留，新的原函数见证独立Record已建立。

VERIFIED / GT08_TEST_ONLY；17/17 focused，完整SelfCheck已越过GT08，下一失败为StateTransformLandingMatrix type2朝向。GT09对应生产差异已独立原函数见证与retirement Record接管，不以旧GT09绿灯证明对齐。
