# Q05 ReleaseTick载体及无效参数退休

状态 READY_FOR_EXACT_PRECHANGE_RECORD。父五reserved步骤2继续，GrabbedBy/TrackerFlag、Mass/Oscillate、2F8/raw已限定验证，不重做。当前参考上一两flag artifact release-tick-next-references.txt，再以当前源逐项确认。

准确范围需事前Record：NTSDEntityRuntime.ReleaseTick/default -1/copy/reset、ECS RuntimeFingerprint、checksum/parity字段，以及LF2WeaponBase/ReleaseFlowResolver/HeldStateResolver、BattleHeldObjectWriter的stampReleaseTick参数与各caller。当前ReleaseFlowResolver函数体只ClearReleasedLinks，stamp参数无读取；其他入口继续逐项证实，不凭名字删真正tick/timer字段。

旧SelfCheck/LegacyReleaseTick测试及多个WPoint/Goal20诊断中的人工preservedTick、expectedReleaseTick、序列字符串、lambda setup和报告须完整迁移，保留有效LinkState/Holder/Target/FrameGuard/速度/动作/物品计数与复用/生命周期断言。不要把旧字段归默认当删除，不用空值伪造旧trace字段相等。

所有剩余WeaponState/HolderCopy仍单独后继，GetResolvedWeaponStateForExternalUse实际frame状态保留；TrackerParent/Owner/Spawner/2F8保持。字段形状变化仍同Q05未发布窗口，步骤3身份/双OPoint guard、步骤4统一13/21/24/2/2、步骤5旧版本拒绝/回放/Play必做，当前12/20/23/1/1不发布。

先absence/无旧参数RED，再相关release/held/throw/WPoint/Goal20和snapshot/ECS/hash/完整SelfCheck、实际Play验证；对旧测试预期修改说明原因并留历史证据。脚本前准确path/symbol/authority/副作用/回滚，无外部Server/Gen/Plugins/UI/Scene/InputActions写入。

保持Unity/GAS、非战斗、33ms/3ms、十一阶段、stage.dat USER_HOLD及例外；禁止computer-use，仅桥接/日志/结果/进程。Foot任务外18删除和新目录、Scene旧精度差异保护。不部署Q07、不清未知文件；回滚须明确批准，仅准确差量。
