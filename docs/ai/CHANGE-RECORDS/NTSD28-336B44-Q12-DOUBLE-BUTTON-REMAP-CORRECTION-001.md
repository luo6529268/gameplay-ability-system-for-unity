<!-- CHANGE-RECORD
id: NTSD28-336B44-Q12-DOUBLE-BUTTON-REMAP-CORRECTION-001
status: RUNTIME_PENDING
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28NativeInputProducerMigrationEditorTests.cs
code-path: Assets/NTSD/Scripts/Input/NTSDInputStateModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Input/NTSD28InputTwoPassModule.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q09P08SameStateBattlePlayProbeEditor.cs
authority: selected 336B44 root EXE and corresponding playable input_state/input_routing path
evidence: docs/ai/TASKS/NTSD28-336B44-Q12-DOUBLE-BUTTON-REMAP-CORRECTION-001.md
-->

# Q12 自然键发现 Q07 人类三键双重换算

脚本改动前登记。原状态、首差、四个准确代码路径、预期副作用、不可触碰边界、验收与精确回滚见 [Task](../TASKS/NTSD28-336B44-Q12-DOUBLE-BUTTON-REMAP-CORRECTION-001.md)。旧 Q07 Change 的聚焦测试和直接 FrameInputSet 探针没有走 `CharacterInputModule` 的长期物理键交叉适配；本次 Q12 原 Scene 物理 L 被错误转成正式攻击索引4，动作60且 combo1=0。需要 test-first 证明并仅撤去新增的第二次换算，保留既有控制器和正式 producer。执行证据后续追加；当前 `PLANNED` 不代表修复。

2026-10-04 实际修改：`NTSDInputStateModule` 精确撤去 Q07 新增的人类 native 二次三键投影和逆读；`NTSD28InputTwoPassModule` 恢复原 `SyncFromRuntime(runtime)`，两生产文件当前均与 HEAD 同内容。保留 `CharacterInputModule` 长期物理 J/K/L→旧 jump/def/att 缓冲映射及正式 producer 字段索引。`NTSD28NativeInputProducerMigrationEditorTests` 将原来从 buffer 直接注入的三键测试改为从 `CharacterInputModule.SetAttack/Jump/DefendActionPressed` 进入，覆盖 one-hot、按住、释放、正式 proxy 及旧内部状态；`NTSD28Q09P08SameStateBattlePlayProbeEditor` 直接注入 mask 改为与物理 J 相同的 `SimulationInputButtons.Jump`，独立回归请求路径保留。此测试探针修正已编译，22tick 未重复运行；不能以其未经运行的新输出作证。

原 Scene 自然物理键 RED/绿结果：[失败原件](../../../artifacts/diagnostics/NTSD28-336B44-Q12-DOUBLE-BUTTON-REMAP-CORRECTION-001/physical-l-before-fix-20261004.json)起始 tick2103，L 后 FrameInputSet mask16、正式 proxy index4、动作60、combo1=0，八次 L 有限脉冲失败；[修正后原件](../../../artifacts/diagnostics/NTSD28-336B44-Q12-DOUBLE-BUTTON-REMAP-CORRECTION-001/physical-l-after-fix-20261004.json)起始 tick878，防御消费 tick880/action110/combo1，方向 tick882/combo2，跳 tick884/action240，首253 tick911，J 在 tick912/phase0 进 FrameInputSet 和 proxy 并转 action301、PP500→350→250。与正式当前源码首窗口 CSV 从防御消费相对 tick2 至动作301 tick34，动作/PP/combo1/输入相位[132/132同](../../../artifacts/diagnostics/NTSD28-336B44-Q12-DOUBLE-BUTTON-REMAP-CORRECTION-001/first253-paired.json)；严格含相对 tick1 为135/136，唯一差是 Unity 启动时已在站立循环动作3，不能称完整同初态全 World。正式源码/根既有三窗口990/990，修正后 Unity 仅此首窗口通过，不外推另外两窗、人手设备或可见画面。

生成 `dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:q -clp:ErrorsOnly` exit0/0错误/332警告；原 Editor 精确六项 EditMode job `fe325189d9cf4a20b59a06c57487770c` 6/6 PASS，含实际控制器交叉映射、正式三键和 AI/legacy 邻例，[原始 job](../../../artifacts/diagnostics/NTSD28-336B44-Q12-DOUBLE-BUTTON-REMAP-CORRECTION-001/editmode-six-filtered-20261004.json)。首次筛选写错 P1P2 命名空间只执行5/5，之后以正确全名补成6/6；第一次 job 延迟再读返回 result null 的原件保留，未伪造6/6。修正前后各一次原 Battle Scene Play 均退出非Play、Scene clean，四保护文件 SHA 分别完全不变；未保存/覆盖场景。`Tools/Validate-ChangeLedger.ps1` exit0，已有历史声明路径警告；全工作树 `git diff --check` 命中并行 Battle Scene 尾空格及 STATE 新空行，后者将在本包修正，Scene 不动。此 Change 保持 `RUNTIME_PENDING / SCOPED_NATURAL_INPUT_PASS`：直接 Q09 探针改动未运行、Q12 其余矩阵未验；不为提高 Record 状态重复已经通过的自然例。
2026-10-04 交付复核：已修正本任务加入 `STATE`/handoff 的尾部空行；仅对本任务两份脚本及当前总表、STATE/handoff执行 `git diff --check -- <paths>` exit0。全工作树 Battle Scene 仍含其它任务的序列化尾空格，未改动它。原 Editor 最终状态为非Play、唯一 Battle Scene clean，四保护SHA仍与本包基线相同。
