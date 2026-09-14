<!-- CHANGE-RECORD
id: NTSD28-Q06-SPARK-C01-TEST-HOOK-001
status: VERIFIED
change-kind: TEST_FIXTURE_AUTHORITY_CORRECTION
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28NativeSparkC01IntegrationEditorTests.cs
authority: 正式FrameAdvance在nativePhysicsAlreadyApplied时走ExecutePostNativePhysicsSerialForWorldPass，旧SimTU override不再执行；C01应仅在tick前段推进现有记录。
evidence: Fresh full SelfCheck or 22-test lifecycle run failed; original failures archived under HIT-SPARK-UNITY-001.
-->

# 火花测试前置纠正

IN_PROGRESS。保持同tick新增记录不提前推进的断言，fixture发出记录移到已存在C25 SimFrameTick override；改测试命名，不改生产tick/生命周期。

仅精确测试脚本，不改生产、schema、Scene、资源、框架或断言强度。验收对应测试/完整SelfCheck、ChangeLedger。回滚仅本差量并遵守用户授权规则。禁止computer-use。

VERIFIED / TEST_ONLY。fixture实际改SimFrameTick及AddRecordDuringFrameTick命名，断言保持。Unity重新编译成功，8628eedf生命周期22/22 PASS，XML在HIT-SPARK-UNITY-001/spark-lifecycle-22-pass.xml；原21/22 FAIL保留。无生产变化。
