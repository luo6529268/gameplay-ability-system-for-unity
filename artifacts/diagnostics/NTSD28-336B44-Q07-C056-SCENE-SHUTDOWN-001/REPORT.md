# Q07/C056 原 Battle Scene 双子体有序关闭（2026-10-04）

**限定结论：通过 Unity Scene 生命周期子门。** 使用原项目已运行的 Unity Editor PID 105896 和保存的 `NTSD_Battle.unity`，只运行独立 opt-in `fusion-hold-c0-shutdown-20261004-01`。正式规则权威仍为根目录 SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 的 EXE 与对应 playable 源码；该受控初态的正式侧结果来自完整 `GameSession28` 重链探针，并非根 EXE 直接注入实测。

Unity Play 角色 OID7/8、合体动作计数0、停顿3、seed682973786、mode0、难度0；使用暂存正式 `Assets/NTSD/Content/LoganRuntime` 内容和项目自有地图。完整生产 Driver 三 tick：OID51/action290，计数0/0/1，停顿2/1/0，OID213结构出生1/1/0、活体1/2/2；与已保存正式源码及原 Scene 的受控结果一致。第三 tick 全部完成后，两个活体 OID213 仍存在，引用池活跃借用4。

此时探针显式调用既有 `SimulationTickDriver.ShutdownBattleRuntime()`，Runtime 阶段完成后经现有 `BattleBootstrap.DisablePresentation()` 清理地图 carrier，再调用 `CompleteBattleRuntimeShutdownAfterMapCleanup`。返回 `Completed / RuntimeMapCleared`，World 对象、runtime slot、池借用、活动池对象、活动池 Sprite 五项均为0；pool quiesced、World detached、退出 Play 后无 live World。Play 退出后原 Battle Scene `isDirty=false`，磁盘 SHA 前后均 `93448372834A1BEAF2C9ACD90E2EF17E2EA487E19F601B9815A907A974D7BF60`；Menu、GameConfig 和 ProjectBattleModeConfig 的保护 SHA 也未变化。原 Editor 回 idle/非 Play。最终原始快照为 [snapshot-0007.json](../NTSD28-336B44-Q07-C056-FUSION-SCENE-PLAY-001/fusion-hold-c0-shutdown-20261004-01/snapshot-0007.json)，前六份快照同目录保留。

初次编译新增诊断局部变量重名 CS0136，已最小修正；第二轮原 Editor 生成 Editor 程序集 SHA `31607177AC1103ADA72499904A35D96C2FE78D750755EBA186769DB03B9308B1`，完成域重载，成功重载后的日志无 C# 错误。编译产物覆盖及备份见 [文件操作记录](../../../docs/ai/FILE-OPERATIONS/NTSD28-336B44-Q07-C056-SCENE-SHUTDOWN-EDITOR-COMPILE-001/RECORD.md)。只修改既有 Editor 诊断脚本，不修改生产战斗脚本、DAT、图片、场景或非战斗逻辑。

这关闭 C056 的原 Scene 双子体生命周期子门；根 EXE 现有 LFR 无法完整注入本案的独立 HP/baseHP、动作计数和停顿，真实自然物理键可达性、其它 C056 条件及 Q07/Q12 总出口仍开放。`shutdownStageAfterExit=-1` 是域重载后旧引用不可读，不能替代本次 Play 内已记录的 `Completed / RuntimeMapCleared`；后者才是关闭阶段证据。

**运行后异步文件变化更正：** 最终 Play 快照写于 2026-10-03 19:12:34 UTC，紧接该次 Play 的四文件哈希复核中 Menu 仍为 `F01144C94B46B400DBC1FE9061E42B983E49135CD5DD6C1B626B98C45116329F`。在报告整理期间，Menu Scene 磁盘写入时间变为 19:21:57 UTC，当前 SHA 为 `9EAAA0B4782974D74A017C367C9D5C77326C31D4281A2D820D1CBBA76986C1BA`；diff 含菜单文字/字体与层级变化，写入者未证。本任务没有回退、保存或覆盖 Menu。当前状态不能再写为“四文件一直稳定”；可确认的是本次 Play 前后即时复核稳定，以及目前 Battle/两配置仍与前值相同。Menu 后续变化需按其自身任务处理，不并入 C056 战斗逻辑结果。
