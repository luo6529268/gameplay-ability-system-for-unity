<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C056-SCENE-SHUTDOWN-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C056FusionHoldBattlePlayProbeEditor.cs
authority: 336B44 playable and root identity, Unity ordered battle runtime shutdown contract
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C056-SCENE-SHUTDOWN-001.md
-->

# NTSD28-336B44-Q07-C056-SCENE-SHUTDOWN-001

脚本改动前建立。已有受控合体 c0 原 Scene 三 tick 出生 1/1/0、活体 1/2/2 与当前正式源码一致；原探针直接 ExitPlaymode，域重载后关闭阶段为 -1。本包仅在原 Editor 探针增加独立 opt-in 显式有序关闭见证，调用现有生产关闭 API，不修改其顺序或战斗中的逻辑。实际代码路径、结果、失败与未验出口随后追加。

预期副作用仅是新 runId 的 Editor Play 诊断及新唯一结果文件。保持旧请求分支、所有 DAT/图片、生产脚本、Scene 与非战斗模块不变。前置状态、验收及前向回滚见同 ID Task；关闭失败必须保持 `CLEANUP_BLOCKED`，不得用退出 Play 掩盖残留。

脚本前文件操作更正：新 opt-in 结果改为唯一目录内的逐次 `FileMode.CreateNew` 快照，不覆盖已有结果文件；旧请求的写法不在本包修改。该更正避免触发文件覆盖审计，并保留每一步原始状态。

2026-10-04 代码已写：实际只修改 `Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C056FusionHoldBattlePlayProbeEditor.cs` 的新 c0 opt-in、逐次不可覆盖结果快照、第三 tick 后显式调用既有 `ShutdownBattleRuntime`/`DisablePresentation`/`CompleteBattleRuntimeShutdownAfterMapCleanup`、结果和失败保护；旧 c0/c7 请求分支继续沿用。首轮原 Editor 编译发现新增 `Save()` 分支内局部 `path` 与旧分支同名，CS0136，未产出新程序集；已将新增分支变量更名为 `snapshotPath`，重编译及 Play 待。生产逻辑、DAT、图片、Scene 均未修改。回滚时仅移除该 opt-in 和相关结果文件，但结果/本次编译操作均按文件操作记录保留，不擅自删除。

2026-10-04 编译更正：原项目运行中的 Unity Editor PID `105896` 经 `refresh_unity(mode=force,scope=scripts,compile=request)` 成功生成 `Assembly-CSharp-Editor.dll` SHA `31607177...B9308B1`，完成域重载并回 idle；成功重载之后日志无 `error CS`/Tundra failed，Battle Scene 查询 `isDirty=false`，两个 Scene 磁盘 SHA 稳定。前一次 CS0136 属已修复失败历史，未冒充一次即过。编译产物预备覆盖见 `docs/ai/FILE-OPERATIONS/NTSD28-336B44-Q07-C056-SCENE-SHUTDOWN-EDITOR-COMPILE-001/RECORD.md`；定向 Play 尚未运行。

2026-10-04 定向运行补证：原 Editor 在原 Battle Scene 唯一 opt-in Play 完成三完整生产 tick，源/Unity已配对出生1/1/0、活体1/2/2；两个子体活着时显式走有序关闭并得 `Completed / RuntimeMapCleared`、五类残留0、pool quiesced、World detached，退出无live World。原 Editor 最终 idle、非Play、Battle `isDirty=false`；Battle/Menu/GameConfig/ProjectBattleModeConfig 四SHA本轮不变。`snapshot-0007.json` 最终 `SCOPED_PASS`，此前0000～0006原件保留。[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C056-SCENE-SHUTDOWN-001/REPORT.md)。本 Change `VERIFIED` 仅代表原 Scene 生命周期子门；C056父项、Q07/Q12和根EXE受控初态仍待。`git diff --check` 已通过；ChangeLedger validator 待最终运行。

后续状态更正：`Tools/Validate-ChangeLedger.ps1` 实际 exit0/PASSED，1217 Records、当前9个受治理代码差异文件，完整输出见 `artifacts/diagnostics/NTSD28-336B44-Q07-C056-SCENE-SHUTDOWN-001/change-ledger-validation.txt`。Play结束即时四SHA复核时 Menu 仍为 `F01144C9...16329F`；之后 2026-10-03 19:21:57 UTC Menu Scene 在本报告整理中另被写入，当前 `9EAAA0B4...C1BA`，写入者未证。本包未改/回退 Menu；本次 Battle 生命周期证书只据 Play 内快照和即时复核，当前四SHA已非全稳，详报告末段。
