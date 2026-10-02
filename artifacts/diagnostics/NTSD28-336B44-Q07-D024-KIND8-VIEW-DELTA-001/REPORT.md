# Q07/D-024 kind8 物理纵深投影：代码与编译出口

日期：2026-10-03。Change/Task：`NTSD28-336B44-Q07-D024-KIND8-VIEW-DELTA-001`。最新状态：`FOCUSED_TEST_PASS / ORIGINAL_BATTLE_SCENE_PENDING / MENU_DISK_CHANGE_OWNER_PENDING`，不是 Q07 关闭证书。下方编译后的“原 Editor 未验”是原 Editor 导入前的阶段快照，已由本报告后段覆盖。

正式版本 SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`：李 OID7/action24 对鸣人 OID2/action0 的 X480 近距样本在 tick1 实际应用 kind8，并把李源规则 Z 写到目标 Z+1（543/542）；X1200 远距12tick没有 kind8。源码/正式根两案各260/260字段同态、根 exit0/PASS；[前置报告](../NTSD28-336B44-Q07-D024-KIND8-REACH-001/REPORT.md)保留 LFR、trace 和负例。

Unity 原共用 `BattleKind8ControlRelationWriter.TryApply` 与 ECS `BattleEcsHitExecutionPlan.ProjectWriterEffect` 都写物理目标 Z+1。在用户确认的 2048×1152 固定完整背景视口下，一个原版源纵深像素应在画面域映射为 `1152/730 ≈ 1.578082` 个物理像素；默认 identity 世界则仍为1。既有 kind8 测试只使用 identity，未覆盖这个差异。

本包仅让两个 kind8 物理 Z 出口复用世界已有的 `SpatialProjection.SourceDeltaToViewZ(1.0)`；源规则 Z+1、X/Y 条件同步、DAT、相机、其它 hit 判定未修改。现有 kind8 Editor 测试类增加一个 2048×1152 聚焦例，分别检查计划投影值、实际写值、源规则 Z、整数镜像时点和计划不提前提交；既有 identity 用例保留。测试先写时两生产出口仍是 +1，**原 Editor 未运行，因此没有声称实际 RED**。

验证：测试夹具最终改为用现有投影先将初始源 Z 映射到物理 Z，保证两实体前置坐标同域。其后生成 `Assembly-CSharp-Editor.csproj` 编译 exit0、0错误/256警告，[最终输出](generated-editor-build-final.txt)；首次夹具版本也编译0错/287警告，[首次输出](generated-editor-build.txt)。生成 `Assembly-CSharp.csproj` 编译 exit0、0错误/22警告，[原始输出](generated-runtime-build.txt)。Editor csproj 明确包含改变的测试文件。三个声明的 C# 文件定向 `git diff --check` exit0、逐路径 scoped Change Ledger PASS；全工作树 validator 因本任务以外的 `SettingsPanelController.cs` 与 `UIButton.cs` 未登记差异退出1，不碰这两份用户代码。当前原项目 Unity PID105896 仍打开 Menu；`Library/ScriptAssemblies` 的 Runtime/Editor DLL 时间戳早于本次修改，故**没有原 Editor 编译、具名 NUnit、Battle Scene Play 或 Game View 通过证据**。本机 Unity CLI `status` 返回 `STATUS_NO_INSTANCES`，仅说明没有 Pipeline 连接，不证明 Editor 已关闭；没有启动第二 Editor 或切换用户场景。

后续在安全的原 Editor 通道中先跑新增具名测试与相邻 identity/dvy 测试，再以正式 X480/X1200 输入在原 Battle Scene 逐 tick 核源/物理 Z、碰撞、画面位置与退出状态。若下游首差存在，再独立定位。Q07、Q09、Q12及总目标仍开放；用户 Menu、DAT、图片、Scene和非战斗功能未改。

## 原 Editor 具名测试与磁盘保护补证

同日稍后，项目现有 Unity MCP 桥读取原 Editor：PID105896、`NTSD_Menu`、active scene clean、非 Play、未编译、无测试、idle。只调用一次 `refresh_unity`，参数 `mode=force, scope=scripts, compile=request, wait_for_ready=false`；它返回 `refresh_triggered=false, compile_requested=true`，随后原 Editor 两个 `Library/ScriptAssemblies` DLL 时间戳刷新。没有全资源刷新、场景保存或切换。

原 Editor 的 `run_tests` 请求都显式携带 `mode=EditMode`、精确 `testNames` 数组及 `assemblyNames=[Assembly-CSharp-Editor]`：

- [新测试 job](named-test-job-status.json) `af026246467347289ef031b9fcf59604`：仅 `Kind8_UsesSharedFixedViewDepthProjectionInActualAndHitPlan`，结果 1/1 Passed。Test Runner 内部进度的项目总数 8785 不是执行数；最终 `summary.total=1`。
- [相邻 job](adjacent-test-job-status.json) `6e0abf48479b4d59ad889b6b88f6ee4d`：`Actual_DvySelectsPreciseAxesAndNeverWritesIntegerMirrors` 六个参数化分支与 `HitPlan_ProjectsConditionalTransactionWithoutIntegerWrites` 一例，结果 7/7 Passed、0 failed。两个 job 均有单独的 [启动响应](named-test-job-start.json)、[相邻启动响应](adjacent-test-job-start.json)，未启动全量测试。

磁盘保护核查：[编译/测试前](editor-protection-before.json)与[测试后](editor-protection-after.json)显示 `NTSD_Battle.unity`、`SettingsPanelController.cs`、`UIButton.cs` SHA 未变；`NTSD_Menu.unity` 从 `17422BE6019E10504B5A53C2C8BB02BB5B673075114BB585FAB1BB55E2444F96` 变为 `7C5EB6D57F84C797FD7C926C30A5706984C633093359738FD1BD69036E861AC0`。Menu 文件最后写入 03:04:58.621，本轮第二个 test job 启动于 03:04:58.522，时间相隔约0.1秒。**这只证明同期变化，不足以认定写入者或变化内容由测试造成**；本轮没有保存/回退场景，当前原 Editor 仍显示 Menu。已向用户询问保存归属；在明确前停止进一步 Editor 操作。上述段落前“用户 Menu 未改”仅指当时没有主动编辑或保存，不能再读成磁盘 SHA 稳定。

Q07/D-024 仍缺原 Battle Scene 同正式近/远初态完整 Driver、物理/source Z 与 Game View 及正常退出验证；具名聚焦 PASS 不能代替整链。
