已完成：原1296/Unity2592与完整SelfCheck PASS证实生产flip正确，旧期望修正。下一WEAPON-PIECE-SPAWN-ADMISSION-EDGE-001；不要再恢复state2000覆盖。

# 当前type2落地方向首差异核验

READY_SOURCE_PHYSICS_AND_C25_WITNESS。完整SelfCheck最新失败CheckStateTransformLandingMatrix（约27288）：已变更为type2的数据对象高速度落地，frame0/vx4/vy-5/Y0/weaponHp19与期望一致，dir实际left但旧测试期待late state2000按最终vx转right。最新原FAIL见C25-EXTRA-DEATH-PRELUDE-RETIREMENT artifacts/SelfCheck-next-landing-fail.result。不得根据测试文字恢复旧state2000表现/帧事件。

先读RunTransformedLandingPasses、CreateTransformedLandingShell、TransformedLandingSelfCheckEntity与实际被调用的physics/frame API；当前fixture已经先完成identity变更，不要把它误写成同tick C25变更前后的物理顺序。跟踪正式physics_integrator.cpp type2落地、battle_world.cpp step_physics及step_frame_slot、SimulationTickDriver28完整step的facing writers。source没有2000字面分支不足以裁决，必须原函数对照。

原函数见证应覆盖type2最终定义、初始facing两面、vx正/负/0、vy阈值两侧/等于、地面与跨floor、state1000/1002/2000及与本fixture一致的data.weapon_hp/drop_hurt、物理端点和完整driver端点；同时记录frame/velocity/YInt/preciseY/weaponHP/facing/RNG，保留物理与之后frame事件的职责区别。确认是旧fixture还是生产缺口后独立准确Change Record实施，不能直接将right换left或为变绿添加无依据转向。

前置结果：C25额外death prelude已退休，6480原source区分frame端点与full tick、四配置各3240向量、64/64联合通过；HP0 state9998的48差异已消除，真实HP0+kind2持有C25/恢复4→4通过。不要重做或恢复旧钩子。后续仍需fragment OID0/999准入与完整driver slot边缘、父frame/其它reader/display-post和Q07正式资源迁移。

保留15/23/26/2/2、raw47/3、用户HUDBg x30和Unity/GAS/非战斗范围。禁止computer-use、不得修改正式EXE/源文件，不能把源诊断当正式EXE画面验收。总目标/Q06 ACTIVE。
