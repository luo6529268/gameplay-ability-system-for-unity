# NTSD28-336B44-Q07-C056-SCENE-SHUTDOWN-001

状态：`VERIFIED_SCOPED`（仅原 Scene 生命周期子门）。父目标：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / BATCH-04 / Q07 / C056`。

当前 336B44 playable `GameSession28` 与原 Unity Battle Scene 已在合体计数 0、停顿 3 的三完整 tick 中对齐 OID213 的出生 1/1/0 与活体 1/2/2。原场景旧探针退出后只能看到引用池为零；域重载使 `shutdownStageAfterExit=-1`，没有证明战斗 Runtime 十一阶段在这条有两个子体的分支上完成。正式根 EXE 的 LFR 也不能注入该受控初态的动作计数/停顿及独立 HP/baseHP，因此本 Task 只补 Unity 生命周期子门，不冒充根 EXE 同态。

唯一脚本所有权：`Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C056FusionHoldBattlePlayProbeEditor.cs`。沿用现有 OID7/8 原 Battle Scene 三 tick 夹具，只加独立 opt-in 请求 `fusion-hold-c0-shutdown-20261004-01` 与唯一新结果目录；该分支的每次保存用递增编号 `FileMode.CreateNew`，不覆盖已有快照，旧两个 runId 和非 opt-in 行为保持原样。在第三 tick 的两个 OID213 活体存在时显式调用既有 `SimulationTickDriver.ShutdownBattleRuntime()`，在其 runtime 阶段完成后用现有 `BattleBootstrap.DisablePresentation()` 清理地图 carrier，再调用 `CompleteBattleRuntimeShutdownAfterMapCleanup`。记录完成阶段、World/槽/池借用/活动 renderer/sprite、pool quiesced、World detached 和 Play 退出后的 live World；任何 hard gate 失败则留在 Play 的 `CLEANUP_BLOCKED`，禁止自动 Scene unload。

验证：先保护 Battle/Menu Scene、GameConfig 和 ProjectBattleModeConfig 四个磁盘 SHA，确认原 Editor 单一干净 Battle、idle、非 Play；生成 Editor 编译 0 error，再在原 Editor 只运行这一个 opt-in Play。所选三 tick 的已有动作、出生和活体数必须与旧 c0 结果同值，关闭状态为 Completed/RuntimeMapCleared，五类残留为零且 Scene/四 SHA 不变。真实根 EXE 受控条件、自然物理键、其它 C056/Q07/Q12 均不由本 Task 关闭。脚本改动后执行 ChangeLedger validator 和 scoped diff check。

风险与回滚：若 Runtime 阶段未完成，不能调用地图清理或退出 Play；保存失败原件并诊断，不通过 `OnDestroy` 补救。只允许审查后反向修正本 Task 的诊断增量，不恢复、删除或覆盖用户已有文件。不得改 DAT、图片、生产战斗脚本、Scene、相机、项目地图、非战斗逻辑或正式 EXE。

请求文件也用新路径 `FileMode.CreateNew` 创建；新 opt-in 不覆写请求确认位，改用 SessionState 消费标记及已有结果目录防重复运行。旧请求分支行为保持原样。

2026-10-04 出口：原 Editor PID105896 编译成功并完成域重载，唯一 opt-in Play 完成三 tick，两个 OID213 活体时十一阶段关闭到 `Completed / RuntimeMapCleared`，五类残留0、退出无live World、Scene clean/四保护 SHA 稳定。首轮编译 CS0136 原件留存、修复后重编0错。证据见 [报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C056-SCENE-SHUTDOWN-001/REPORT.md)。根 EXE 受控初态注入、自然物理键与 C056/Q07/Q12 整组仍开放。
