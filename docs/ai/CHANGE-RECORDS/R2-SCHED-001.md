# R2-SCHED-001 — C++ T09～T16 主 scheduler/pass 边界

<!-- CHANGE-RECORD
id: R2-SCHED-001
status: RUNTIME_PENDING
code-path: Assets/NTSD/Scripts/Simulation/NTSDBattleTickSystem.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: J:\QQFile\NTSD2.4\ntsd_release\src\entity\game_tick.cpp::game_tick(...) release live path
evidence: SOURCE-CONTRACT-VERIFIED / UNITY-RUNTIME-PENDING
-->

> 创建日期：2026-08-21  
> 最后更新：2026-08-21  
> 类型：battle / scheduler / self-check

## 1. 状态与范围

- 当前状态：`RUNTIME_PENDING`；已取得 Unity 编译和 focused self-check 证据，joint / Play Mode /
  C++ runtime trace 仍未关闭；
- 所属 Work Package：`R2-PASS-01`；
- 覆盖差异：D-SCHED-001、D-SCHED-002、D-SCHED-003、D-SCHED-004；
- 不属于本次范围：D-SCHED-005（R3-INP-01）、D-SCHED-010（R3-INP-02）、R2-PASS-02、
  任何 CPoint/held/collision/link/damage 公式、任何 renderer/input asset/DAT/scene 修改；
- 关联 Change ID：无；`OPS-TRACE-001` 仅为治理工具，不是 gameplay predecessor。

## 2. Authority / 需求依据

- C++ release 文件、类型、函数和 release build 参与性：
  `Makefile` 列入 `src/entity/game_tick.cpp`；权威入口是
  `game_tick.cpp::game_tick(...)`。T09 为 1441-1643，candidate/consume 为 1648-1825，
  T14/T15 为 1825-1846，第二次 clamp/T16 为 1848-2019；
- 用户明确需求：先按 source inventory 的小闭合子模块处理，并严格留下可恢复的工作流证据；
- Evidence 等级：C++ 顺序为 `VERIFIED（source）`；Unity 现状为 `VERIFIED（source）`；
  real runtime equivalence 为 `UNKNOWN / PENDING`。

## 3. Unity 原状与已确认差异

- Unity 文件、类型、方法：
  `NTSDBattleTickSystem.RunFrameAdvancePhase` / `RunInteractionPhase`，
  `SimulationWorld.PreInteractionTickAll` / `HeldObjectProcessAll`；
- 改前执行顺序：初次 Z clamp → CPoint/WeaponSync → positive link → 第二次 Z clamp →
  单轮 held → collision snapshot/candidate → character/random/object consume；
- C++ 目标顺序：初次 Z clamp → held#1 → snapshot/candidate → character/random/object consume →
  CPoint/WeaponSync → positive link → 第二次 Z clamp → held#2；
- 已确认差异：D-SCHED-001～004；
- 依赖模块和前置条件：R1-SOURCE-004/005 已闭合静态 writer/consumer 映射；candidate cleanup 是
  Unity adapter，必须保留其 cleanup 语义，不能把它当 C++ gameplay pass。

## 4. 计划改动

| 文件 | 类型 / 方法 | 改前职责 | 目标职责 |
|---|---|---|---|
| `Assets/NTSD/Scripts/Simulation/NTSDBattleTickSystem.cs` | `RunFrameAdvancePhase` / `RunInteractionPhase` | 将 CPoint/link/held 置于 candidate 前，held 仅一轮。 | 只重排已有 writer 调用为 C++ T09～T16 顺序，并显式区分 held#1 / held#2。 |
| `Assets/NTSD/Scripts/Simulation/SimulationWorld.Passes.partial.cs` | candidate cleanup adapter（仅必要时） | 在 object consume 后结束 candidate carrier。 | 仅维持/澄清 cleanup 边界；不改 collect/consume 算法。 |
| `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs` | scheduler source-contract checks | 含历史 C# “单轮 held / CPoint 早于 candidate”断言。 | 改为 C++ source-contract 的两轮 held 和 CPoint-after-consume 回归断言。 |

## 5. 不可回退边界

- 中央表现 / `CentralOnly` / Texture2DArray / 动态 Mesh：完全不动；
- `Authority400`、`MobileExtended`、`DesktopExtended` 容量合同：完全不动；
- 30 Hz、`FrameInputSet`、slot/generation、SoA/ECS、对象池、worker、0 GC：完全不动；
- 其他已关闭 Change ID：无 gameplay Change ID；不得通过回退 `A-RENDER-001～004` 达成任何测试。

## 6. 实际改动

| 文件 | 类型 / 方法 | 实际改动 | 预期副作用 |
|---|---|---|---|
| `Assets/NTSD/Scripts/Simulation/NTSDBattleTickSystem.cs` | `RunFrameAdvancePhase` / `RunInteractionPhase` | 移除 candidate 前的 CPoint/positive-link/第二次 clamp/单轮 held；插入 held#1，并在 object consume + candidate cleanup 后插入 CPoint/positive-link/clamp/held#2。 | C++ T09～T16 相对时点恢复；旧 Unity single-held 行为将改变。 |
| `Assets/NTSD/Scripts/Simulation/NTSDBattleTickSystem.cs` | private pass wrappers / class contract comment | 将 generic held/preinteraction/link helper 改名为 first-held、CPoint+WeaponSync、positive-link、second-held，以避免未来再次混淆 C++ pass。 | 仅私有名称与调用点变化；不改 writer 公式。 |
| `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs` | R2 scheduler focused checks | 将历史 C# single-held / CPoint-before-candidate 断言改为 C++ 两轮 held 和 CPoint-after-consume 断言。 | 旧错误基线应不再作为回归标准。 |

`SimulationWorld.Passes.partial.cs` 在计划中曾作为备选 adapter；经 source review 不需要改动，实际未修改，
故不再列为 Change Record 的 metadata code-path。

## 7. 验收与证据

| 层级 | 命令 / 场景 / 输入 | 实际结果 | 状态 |
|---|---|---|---|
| 静态顺序检查 | C++ `game_tick.cpp` T09～T16 对照 Unity scheduler | first clamp → held#1 → snapshot/pair/candidate → character/random/object consume → cleanup → CPoint/WeaponSync → positive link → second clamp → held#2；两次 clamp、两次 held、一次 CPoint 均 PASS。 | `PASS` |
| focused self-check | 两轮 held、CPoint/WeaponSync 后置、Z clamp/candidate 顺序 | 更新后的 source-contract checks 已由 post-refresh `BattleRuntimeSelfCheck` 覆盖。 | `PASS` |
| Unity 编译 | 当前已打开 Unity Editor 的 UnityMCP `refresh_unity(force/scripts/compile)` | refresh 因 domain reload 短暂断开后恢复 ready；`Assembly-CSharp.dll` 更新为 2026-08-21 22:10:02；Console 读取 0 error。 | `PASS` |
| BattleRuntimeSelfCheck | 编译完成后 request file | `Temp/NTSD_BattleRuntimeSelfCheck.result` 于 2026-08-21 22:12:49 返回 `PASS`。 | `PASS` |
| Play Mode / 集成 | held relation、CPoint、weapon/opoint 场景 | 尚未运行 | `PENDING` |
| C++ authority 对照 | source coordinates 已记录；full runtime trace 无安全通道 | source only | `PENDING` |
| 可选 full trace | R1-WP02 | B-R1-WP02-01～04 | `BLOCKED` |

S2 联合 fixture 依赖 R4/R5，不能在本 Record 中写为已验证。

## 8. 风险、回滚与未关闭项

- 已知风险：两轮 held 会使先前错误的 Unity single-pass 行为、历史 self-check 和某些暂存
  candidate/relationship 可见性发生变化；这是 C++ source 指向的预期风险，不自动等于 bug 已修复；
- 未关闭项：candidate cleanup 的 exact C++ visibility、CPoint/held/link writer 公式、full trace、
  Play Mode 和 C++ runtime witness；
- Stop conditions：若调用移动要求修改 CPoint/held/candidate/collision/link/damage 公式、candidate
  cleanup 不可维持在 object consume 后、C++ 依据冲突、或需要回退已批准 Unity adaptation，则停止
  本 Record 并创建后续 correction / dedicated Work Package；
- 回滚方式：若本包静态/编译/聚焦验证失败，不使用破坏性 Git 操作；创建 correction / rollback
  Change Record，仅反向撤销本 Record 所列 scheduler 调用移动，并保留失败证据；
- 若 superseded，后续 Change ID：待定。

## 9. Git / 交接

- 修改前工作树基线：branch `NTSD_2_4_C++`，HEAD
  `2c53f1eb0086ef76c892fa335bfe1adfdd87facc`；已存在用户/历史的文档、场景、资源 meta、
  项目设置和未跟踪目录改动，绝不归属给本 Record；
- 实际脚本 diff 范围：`Assets/NTSD/Scripts/Simulation/NTSDBattleTickSystem.cs` 与
  `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`；配套文档为本 Record、
  `CHANGE-LEDGER.md`、`STATE.md`、R2 task contract、差异登记册、重新对齐总计划与本 handoff。
  既有用户/历史的其他工作树修改不归属给本 Record；
- 提交 hash（若已提交）：无；
- `Tools/Validate-ChangeLedger.ps1` 结果：代码改后与交接前已运行，均为 PASS；最终交接核验覆盖
  两个 R2 脚本，未发现无 Change Record 覆盖的当前脚本 diff；
- 交接需优先阅读的文件：
  `docs/ai/TASKS/R2-PASS-01-scheduler-pass-boundary.md`、
  `docs/ai/RESEARCH/R1-SOURCE-007-dependency-graph-and-repair-batches.md`、
  `docs/ai/RESEARCH/R1-SOURCE-007-subflow-acceptance-matrix.md`、
  `docs/ai/RESEARCH/R1-SOURCE-005-cpp-cpoint-held-link-opoint-lifecycle-contract.md`。
