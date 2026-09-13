# Q05 双OPoint owner快照边界

状态 IN_PROGRESS / TEST_FIRST。父步骤3继续；语义身份子项先取完整出口，不重做raw/tag/cache/pub/local-session已完成职责。先读取CURRENT-AUTHORITY和Q03 VERSION-IDENTITY-AND-CAPTURE-CONTRACT完整snapshot边界部分。

当前冻结入口：SimulationWorld.TryCaptureBattleStateSnapshot、BattleLockstepSession.TryCaptureBattleStateSnapshot、LockstepSnapshotRing.TryCaptureNext、BattleStateSnapshotRestore；logic-only队列BattleLogicObjectPointRuntime，renderer队列LF2ObjectPointFactory。driver Preparing已捕获_battleObjectPointFactory，必须复用已有owner引用。禁止snapshot校验调用ResolveObjectPointFactoryForSimulation的会创建Instance路径。按当前源码核对字段及所有调用，不凭旧行号直接改。

先准确World/driver/queue/tick/structural scopes观察图和Record，再RED。仅非tick、非structural playback、该World相关队列全部空时允许capture/restore。未满足应明确失败、目标capture buffer无效，但World/队列/原restore snapshot保持；不得Flush、丢任务、materialize、启动异步工作或创建singleton。非本World的全局队列不跨World误判，owner归属必须以实际task/world/parent合同确认。两条owner都验证，不仅PendingEvent或其中一个队列。

复用既有边界/失败枚举/生命周期接口，必要新值先记录准确契约。不要新造队列/序列化pending task、改有序关闭顺序、改恢复策略或网络流程。停机/worker未Join时同样遵守hard gate，测试不能为了capture强行停止/清空正式模拟。

验收：logic-only和renderer各自pending、非本World队列、正在tick/structural播放、拒绝副作用零、空队列成功、World/session/ring/capture/restore入口、raw/claimed复用、warm无分配、完整SelfCheck及必要真实Play。Scene/InputActions/资源/外部Server/Gen/Plugins保持，禁止computer-use，仅桥接/日志/结果。

后继仍需核对新的FrameSounds/profile/centerz/chp/cmp/六double及metadata的相关内容hash消费；当前source+decode语义身份已覆盖原始输入和版本，不能凭它跳过其他存在的内容hash或trace字段扫描。步骤4按Q03统一entity13/aggregate21/checksum24/character2/base2以及trace v3/raw v2/source wrapper v2、50字段2F8；旧6MISSING不无证据晋升。步骤5旧版本拒绝/新capture→restore→同seed/input replay与checksum、pool/slot复用、零残留/Play后才能关闭Q05。当前12/20/23/1/1中间态不发布，不提前Q07。

保持Unity/GAS/非战斗、33ms/3ms、十一阶段、stage.dat USER_HOLD及例外，Scene旧SHA/Foot18既有缺失及用户新图保留；准确Task/Record/预变更SHA先于脚本，回滚需批准仅差量。

## 最新出口

FOCUSED_TEST_PASS / SCOPED_PLAY_PASS，187/SelfCheck/真实暂停World双队列拒绝及空闲capture通过；完整报告与后继见同Change Record。
