# GT08夹具核验

VERIFIED / GT08_TEST_ONLY。正式8297..8355 encoded reset写NativeRuntimeStateCode=1100-code，current/collision action0并清pending，不能覆盖render phase。旧共享mock SimFrameTick只写Frame.N，GT08旧预期把code写HitStun，均不证明实际生产。

准确两测试脚本：SelfCheck三GT08场景使用实际Character/SpecialAttack/current-character-DAT shell推进，新增type3 sealed catalog和统一断言帧镜像/latch/Native state/render/pending/sound；原LateLifecycleSelfCheckEntity不改，GT09等其它用途独立。Editor新增1200/1299在两个后端，先RED2/7，再17/17 PASS（含原2676矩阵）。完整SelfCheck实际越过GT08，最新失败为StateTransformLandingMatrix type2落地方向，原结果见SelfCheck-next-landing-fail.result。

GT09未知已交接STATE9998-SOURCE-DRIVER-WITNESS及STATE9998-LEGACY-CLEANUP-RETIREMENT，不用旧测试认证额外清理。生产与资源/Scene不由本Record修改。总目标/Q06及父frame仍未完成。
