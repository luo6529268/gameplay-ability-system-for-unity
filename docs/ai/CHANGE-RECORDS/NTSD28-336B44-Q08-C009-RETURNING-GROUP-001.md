<!-- CHANGE-RECORD
id: NTSD28-336B44-Q08-C009-RETURNING-GROUP-001
status: FOCUSED_TEST_PASS
change-kind: CODE
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Results/BattleResultsOutcomeHostWriter.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q08BattleFlowRedProbeEditorTests.cs
code-path: Tools/NTSD28Q08Diagnostics/returning_group_lfr_probe.cpp
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q08C009ReturningGroupBattlePlayProbeEditor.cs
authority: user-selected formal NTSD 2.8-Logan 336B44 release; playable BattleFlow28::step C009 returning living-group rule
evidence: docs/ai/TASKS/NTSD28-336B44-Q08-C009-RETURNING-GROUP-001.md
-->

# NTSD28-336B44-Q08-C009-RETURNING-GROUP-001

2026-10-01 原 Scene 探针实际验证：新增声明的 Editor 脚本及 Unity 自动 `.meta`，原 Editor 新程序集晚于脚本且定向受控 Play 运行 `PASS`。正式 OID56/team1 + 生产工厂生成 OID304/type3/action11/team2，12 tick 连续生产 Driver 的 timer/output/mask/出生/action 对正式源码 72/72 同态；退出 Play、Scene clean、Battle/Menu/GameConfig/Mode 四 SHA 稳。无生产行为或 DAT 改动；该证据不覆盖物理按键自然选招、独立赢家画面或再次单组的 Scene 后继。[原件](../../../artifacts/diagnostics/NTSD28-336B44-Q08-C009-RETURNING-GROUP-001/REPORT.md)。Record 保持 `FOCUSED_TEST_PASS`，并附 `UNITY_SCENE_SCOPED_PASS` 限定证书；Q08 未关闭。

2026-10-01 原 Scene probe 脚本前记录：仅新增声明的 Editor Play 探针，复用生产 `BattleTestBootstrap`、`LF2ObjectPointFactory.CreateObjectImmediate`、`SimulationTickDriver.StepOneTick`；受影响代码职责限于请求文件轮询、Play 受控初态、逐 tick 观察与退出清场。期望输出是新的 JSON 证据；风险为 Play 中暂时生成角色/非角色实体及资源加载，必须遵循原 Battle Runtime 有序关闭、不保存 Scene。验收为原 Editor 编译零错、正式内容绑定、timer/mask/生成实体 12 tick 同条件、Play 退出/四保护 SHA/场景 clean，未达即保留失败原件。回滚只处理新增探针及其 `.meta`，任何删除需遵守用户要求的事前记录与批准；不更动 DAT、生产行为或非战斗模块。

2026-10-01 正式根自然见证：声明的 `Tools/NTSD28Q08Diagnostics/returning_group_lfr_probe.cpp` 已写，沿正式 28 Core+playable 源编译 exit0/stderr0。正式源 OID304/action11→action1→OID56 第二组在 host2 出生，host3～12 timer2 保持、赢家 -1；同一 LFR 经 SHA 336B44 根程序回放 exit0、报告通过，逐 tick 7 字段 84/84 零差。首次比较把前置 BattleFlow 组数与后置 World 组数混比造成 tick2 假差，原件保留，时点修正后比较零差。[报告与全部原件](../../../artifacts/diagnostics/NTSD28-336B44-Q08-C009-RETURNING-GROUP-001/REPORT.md)。原 Battle Scene Play/独立赢家画面及再单组完整后继仍待，Record 维持 `FOCUSED_TEST_PASS`。

2026-10-01 权威见证补充范围（工具脚本修改前）：新增独立 `returning_group_lfr_probe.cpp`，仅实例化正式 `GameSession28` 的 OID56 单存活组与 OID304/action11 发射者，逐 host 记录结果 timer、存活组、赢家和 OID56 生成，并用正式 `GameSessionLfr28` 录制 LFR。预期副作用只在新的诊断输出目录写 TSV/LFR；不改现有正式源码、DAT、Unity 生产脚本、Scene、GAS 或非战斗逻辑。验收为源诊断零编译错误、至少 timer>0→双组 timer 保留的自然证据、根正式 EXE 同一 LFR 的逐字段对照；任一前置失败则记录失败而不标通过。回滚限于此新工具，需按仓库删除审批规则处理，不自动删除文件。

治理复核：`Tools/Validate-ChangeLedger.ps1 -RepositoryRoot $PWD.Path` 已通过；本包脚本的 `git diff --check` 退出码为 0。正式根自然返组和原 Battle Scene Play 仍待，记录状态保持 `FOCUSED_TEST_PASS`。

脚本前记录。正式版前置、Unity 原状、受影响符号、预期副作用、保护边界、验收与回滚见对应 [Task](../TASKS/NTSD28-336B44-Q08-C009-RETURNING-GROUP-001.md)。尚未修改脚本或取得 Unity RED/GREEN；旧测试的 timer2/旧 mask 断言只是待更正的旧 oracle。

测试先行：已在声明的 `NTSD28Q08BattleFlowRedProbeEditorTests` 中把旧“返组仍计数并锁单组”预期改为返组时 timer1/双组 mask、再次单组时 timer2。原项目现有 Editor PID 105896 经其现有本地 Unity-MCP bridge `refresh_unity(force/all/compile=request)` 返回编译请求并完成 domain reload；定向 EditMode 作业 `5fb1070ec9134ac3831b5dc56b0c3698` 实际完成 1 个目标测试，状态 failed，首个断言 `Expected: 1 / But was: 2`，确认为共用 writer 首差。[RED 原件](../../../artifacts/diagnostics/NTSD28-336B44-Q08-C009-RETURNING-GROUP-001/red-job-v1.json)。`progress.total=8775` 是 Test Runner 的发现总数，`completed=1` 才是本次执行数。尚未改生产或得到 GREEN。

生产修复已写：`BattleResultsOutcomeHostWriter.AdvanceNativeFlowBeforeCombat` 对任何 active 阶段的多存活组直接更新当前 mask、保留 stored/output timer 并返回；恢复为零/单组时也在递增前刷新 mask。之前只在 timer0 暂停/更新，导致已开始结果计时后返组继续计数且赢家载体滞留。没有新增状态字段、改变快照 schema、改结果页 host 或影响非战斗逻辑。原 Editor 编译、GREEN、相邻结果与正式根自然入口尚待。

后续实际验证：原 Editor 再次强制刷新/请求编译，生产 `Assembly-CSharp.dll` 更新时间晚于修复脚本；定向作业 `ea3d0eff54aa40d48ced4fb5ab30e45d` 实际1/1 GREEN，结果类作业 `695d64d38c68427dabca5fb3c8458b65` 13/13 PASS，快照/校验和作业 `b9e9bd414c414df2b9ed76d114f70487` 1/1 PASS。原件与限制见[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q08-C009-RETURNING-GROUP-001/REPORT.md)。正式根自然返组、原Battle Scene Play与全状态仍待；本 Change 只推进为 `FOCUSED_TEST_PASS`。
