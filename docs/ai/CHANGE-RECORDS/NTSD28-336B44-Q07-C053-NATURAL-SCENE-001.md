<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C053-NATURAL-SCENE-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C053NaturalSceneProbeEditor.cs
authority: selected 336B44 playable live OID65 to OID875 and OID702 to OID808 natural OPoint GameSession witness
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C053-NATURAL-SCENE-001.md
-->

# C053 自然 OPoint 原 Battle Scene 探针

脚本前登记。正式源码同种子/同双角色初态的 40 tick 有 tick1 OID808、tick4 OID875/action50、tick5 action55、tick7 单次 `applied/effect2/Uj156`；336B44 根 EXE 同 LFR 的 40 tick×12 字段 480/480 零差。Unity 当前只证人工建立两个 OID875 的另一受控 Scene 案例，尚无这组自然 OPoint Scene 同态。

本包仅增加一个请求式 Editor 测试探针及 `.meta`，准确代码路径见 metadata；无生产脚本、资源、DAT、Scene 或非战斗写入。预期副作用是 Play 内临时 World 状态及唯一 JSON 结果；生产 Driver 有序关闭负责归还。代码用旧双 Uj Scene 探针已验证的启动/退出模式，但**不人工生成 OID875**。必须记录同初态、每 tick 实体动作和源规则位置、OID808 锁存/HP、rest 与首差，源 CSV 与根 trace 是对照而非 Unity 规则定义。

验收和四保护 SHA/Scene clean 门见 Task。若生成工程陈旧，只在 `Temp/` 写 targets 显式纳入新脚本；离线编译与 Editor 导入分别报告。任何场景/角色预热不符保留失败结果，不改生产以迎合案例。回滚只审阅本包新增探针与记录；不碰已有 dirty 文件，不删除旧证据。待代码写入后追加实际路径、命令、结果与未验证边界。

2026-10-02 实际脚本：新增所列 Editor `.cs`/唯一GUID `.meta`，无生产代码/DAT/资源/Scene 改动。第一次离线 `dotnet build` 因仅用于本探针的 Temp targets 被所有引用项目导入而报跨项目类型缺失；targets 限定 `MSBuildProjectName=Assembly-CSharp-Editor` 后，原生成 Editor 工程显式纳入新文件并 build 0 error/292 warning。原Editor `refresh_unity` 已导入，Editor DLL 时间晚于新脚本并含该类型，当前 Scene clean。首轮唯一 `ank610-jira500-natural-scene-01` 完成八tick、退出Play/Scene clean，结果 `FIRST_DIFFERENCE`：tick1正式源安科/自来也 X608/495，Unity为612/505，子体 X503 vs Unity497；探针错把根trace `facing=false` 设为Unity `SwitchDir("left")`。这是**测试前置朝向不等价**，不据此修改生产或判定C053失败。已将探针统一改成 `SwitchDir("right")` 并改第二唯一RunId，首轮结果保留，不覆盖。第二轮编译/Play/逐字段比对待，父项继续开放。

2026-10-02 完成续证（覆盖上段待验快照）：改朝向后同 Temp targets 离线 build 0 error/249 warning，原Editor刷新后DLL晚于脚本、状态idle/Battle clean；第二唯一RunId原Scene 8个完整生产tick `SCOPED_PASS / DONE`，退出Play clean，四保护SHA稳定。源码CSV/根trace实体字段144/144、按当前正式源码seed682973786的CRT递推和同步计数五字段×8的40/40，合计184/184零差；tick7子体action156/HP475、攻击者rest10。根LFR载体默认seed0导致其CRT状态与源码seed不等价，已按 `game_session_lfr.cpp`/`game_session.cpp`/`native_random.cpp`限定解释；不称Unity RNG首差。首轮失败原件保留。本包只关自然单Uj原Scene出口；物理按键、双Uj、逐hit日志、全World仍待，C053/Q07/总目标开放。[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C053-NATURAL-SCENE-001/REPORT.md)。未改生产脚本，故未跑全案例/SelfCheck；本轮运行定向原Scene而非EditMode测试。回滚仅审阅新探针/meta/文档，删除需另行审计。

交付前 `Tools/Validate-ChangeLedger.ps1 -RepositoryRoot $PWD.Path` exit0/PASSED（1116 Records/62当前差异代码文件）；相关已跟踪文档 `git diff --check` exit0；再次本机Bridge确认原Editor非Play、原Battle Scene active/clean。完整 validator 输出见[原件](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C053-NATURAL-SCENE-001/ledger-validation-v1.txt)。

对齐总表 C053 当前行补入自然单Uj证据后再次运行同一validator，exit0/PASSED（1116 Records/62代码文件），见[最终原件](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C053-NATURAL-SCENE-001/ledger-validation-v2.txt)。
