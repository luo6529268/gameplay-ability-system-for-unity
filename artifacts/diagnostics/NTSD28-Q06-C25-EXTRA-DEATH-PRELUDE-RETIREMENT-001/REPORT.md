# C25额外死亡前置退休结果

VERIFIED / EXTRA_DEATH_PRELUDE_REMOVAL_ONLY。准确八脚本移除LF2Entity旧death/bounce/drop方法与私有helpers、LF2Character无用包装、Module生产调用及旧CanSkip。真实hit/physics/WPoint和通用ForceReleaseHeldObjectReference未改；旧诊断阶段/snapshot编号保留为无规则边界，NoOp计数表示经过该退休区段，World测试数组9/10保留零值。没有新增runtime字段、服务、队列或shutdown阶段。

原源码见证6480：3240 frame/lifecycle端点保持action/position/motion/active link，另外3240完整tick用于区分其它owner。完整tick有1716次动作变化（未逐例归因为单一物理函数）、384次明确WPoint释放配置产生的关系解除；它们不是本退休范围。完整原消息正常dead输入抑制及terminal primary result保留，frame/lifecycle错误检查通过，复跑字节一致。

Unity新四配置各3240向量RED后0差异：实际Character/共享当前DAT外壳×Legacy/DataOriented两个frame后端，检查动作、位置、速度、持有关系和两个RNG域没有额外消费。新目标SelfCheck通过独立NUnit实际调用完整Late验证，未被后继失败遮蔽。第一联合57/64暴露旧clock/lifecycle fixture前置，分别独立Record修复，没有改生产迁就测试。最终64/64 PASS、85.6953489秒，六个选择器均执行。此前state9998的48次HP0动作差异已消除。

真实Scene旧Unity内容：tick5，站立角色HP0+实际kind2子武器，运行完整World.LateEntityUpdateAll后frame0、Y/Vy0、parentLink1/childLink-1保持；显式回收child、恢复原materializer模式和checksum，4→4。之后World/slots/logic/render borrowers全0，连续两帧Stopped并退出Play。非新命中/完整physics/正式图片表现证据。

完整SelfCheck仍FAIL，已过本轮前段目标检查，首差异仍是CheckStateTransformLandingMatrix type2高速落地方向。不能声明整个SelfCheck或B3/B4/Q06已齐。下一唯一Task TYPE2-LANDING-FACING-AUDIT-001，先原函数物理/完整driver与fixture调用链对照再裁决；随后fragment OID0/999准入、slot完整driver、父frame及资源Q07。

15/23/26/2/2与raw47/3保持，未迁移DAT/图片，未改Unity/GAS框架或非战斗功能。用户HUDBg x30保持，Scene SHA BCD1047BF912C6A4A8BC9F3A76EAF3FA954211AD064E0402B1C01BF3BA0E9FB6。禁止computer-use；无提交/push/文件删除。原红灯、64最终XML、Play/关闭、SelfCheck失败与validator均留证。
