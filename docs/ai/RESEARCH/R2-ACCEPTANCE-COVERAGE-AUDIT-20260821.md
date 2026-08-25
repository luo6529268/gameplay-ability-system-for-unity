# R2 scheduler / tail 分层验收覆盖审计

> 审计日期：2026-08-21  
> 状态：`READ-ONLY AUDIT COMPLETE / JOINT FIXTURE PENDING`  
> 行为 authority：`J:\QQFile\NTSD2.4\ntsd_release` 的 release live source。  
> 范围：只盘点现有 Unity self-check 对 R2 scheduler/tail 的覆盖；不修改 C++、Unity gameplay、
> 测试脚本、场景、资源或配置。

## 1. 审计问题

R2 总计划的完成条件要求普通输入、空场、单角色、角色+武器、角色+技能对象五类夹具，并要求在
可以独立验证时覆盖 C++ `game_tick(...)` 的 scheduler 边界。此次审计只回答：现有
`BattleRuntimeSelfCheck` 实际覆盖了什么、缺少什么；它不把旧 C# 自检或 Unity 自检提升为 C++
runtime 对齐证据。

## 2. 已确认的现有覆盖

| R2 维度 | 现有 Unity fixture | 实际断言 | 覆盖结论 |
|---|---|---|---|
| 空场 | `CheckAudit4PendingFrameSoundContracts`，`BattleRuntimeSelfCheck.cs:14642-14651` | empty `SimulationWorld` 跑一次 `RunReleaseTick`，确认 tick head 清空旧 `PendingSounds`。 | 有 empty-world tick，但只覆盖声音队列头部，不证明完整 R2 pass sequence。 |
| 单角色 / human poll | `CheckFrameworkCooldownBeforeHumanInputOrder`，`20582-20610` | 单 human character 的 cooldown/rest 在 human poll 前完成。 | 覆盖 cooldown→human poll 局部关系；没有覆盖完整 normal input action 或后续 scheduler 链。 |
| 角色 + held weapon | `CheckReleaseTickRunsHeldStep12Twice`，`11673-11730` | holder + drink 在一次 release tick 中经历 held#1 与 held#2，HP 从 20 到 18。 | 强 S1 证据，覆盖 D-SCHED-004 的双 held pass；不覆盖 candidate/CPoint/link 联合可见性。 |
| candidate → CPoint | `CheckReleaseTickCpointSyncFollowsCandidates`，`12124-12194` | candidate consume 观察到 pre-T14 CPoint state；T14 后才出现同步 frame/position。 | 强 S1 证据，覆盖 D-SCHED-001 的 CPoint 后置；其 topology 是 catcher/victim/target，不是 held+weapon 联合夹具。 |
| 初次 Z clamp → candidate | `CheckReleaseTickZClampPrecedesCandidates`，`12196-12240` | attacker 在 candidate collect 前被 Z clamp，candidate 看见 clamp 后坐标。 | 局部 S1 证据；D-SCHED-006 的 entity filter/double-int/newborn 仍 UNKNOWN。 |
| mode2 tail | `CheckReleaseTickMode2ResetFollowsEntityPostFrameTail`，`12242-12260` | mode2 request 跨 entity postframe tail 可见，随后清零。 | R2-SCHED-002 的 focused code evidence；不关闭 `g_init_stats`、results/ECS shadow 或 next-tick 联合行为。 |
| input-driven 角色 + 技能对象 | `CheckNarutoDdjSixCloneProductionChain`，`24018-24100` 起 | production DAT 的 att/down/def input 触发 Naruto DDJ，持续扫描 clone/wind/poison object 链。 | 覆盖真实 Unity 生产 DAT + skill object 链的强回归样本；它不是通用 R2 pass-order witness，也不是 C++ trace。 |

上述自检在 R2-SCHED-001/002 当前 source 上已实际通过；证据只到 Unity focused/self-check 层。

## 3. 真实缺口

现有夹具是**分散**的。没有一个单 tick 联合 fixture 同时记录并断言以下因果序列：

```text
first Z clamp
→ held#1
→ collision snapshot / candidate collect / character+object consume
→ candidate-end adapter
→ CPoint + weapon sync
→ positive-link validation
→ second Z clamp
→ held#2
```

因此不能把“held test PASS”与“CPoint test PASS”相加，宣称 R2 scheduler 的跨 writer
producer→consumer 关系已经完整验证。

此外，R2 计划规定的五类夹具中：

- 空场和单角色仅有局部 head/poll 行为；
- 角色+武器仅覆盖 held 双 pass；
- 角色+技能对象有生产回归，但未输出统一 pass event / slot state witness；
- 普通输入尚缺一个不依赖 Naruto 专项 combo 的 normal-action fixture；
- Play Mode 与 C++ full trace 仍没有证据。

## 4. 建议的后续验证包 — R2-VERIFY-01

> 状态：`PLANNED / READY_TO_EXECUTE`。这是 test-only Work Package，不改变 gameplay；若实施仍必须
> 先建立独立 Change Record，因为会修改 `BattleRuntimeSelfCheck.cs`。

### Goal

建立一个最小、确定性的 R2 scheduler joint fixture，以诊断 hook / test entity 记录同一逻辑 tick 的
pass-event、slot、frame、position、relation 和 candidate visibility；分别覆盖：

1. empty world；
2. one human character 的普通输入；
3. holder + weapon；
4. CPoint/candidate target；
5. skill object / late spawn 的 R2 scheduler handoff。

### Required assertions

- event 顺序必须精确为 R2 source contract；
- first/second held 的 slot traversal 与 CPoint/link reader 时点可区分；
- candidate 在 T14 CPoint sync 前、后分别可见何种 state；
- 实际 fixture 不得依赖 `Transform`、CentralOnly render、camera 或 Legacy SpriteRenderer；
- fallback/optimized path 若同 fixture 均可跑，必须至少记录其 output 是否一致；否则保持
  `PENDING`，不得默认 optimized 等价。

### Explicit exclusions

- 不修改 CPoint、held、candidate、collision、link、frame、damage、input、opoint 或 render 的
  gameplay writer；
- 不把 test hook 当作 C++ runtime trace；
- 不处理 R3-INP-01、F1/F2、R4/R5、T8 stage.dat、性能或真实 Play Mode。

## 5. 状态结论

| 项目 | 当前证据 | 状态 |
|---|---|---|
| R2 scheduler source mapping | C++ source + Unity static mapping | `VERIFIED(source)` |
| R2-SCHED-001/002 focused code behavior | Unity compile + `BattleRuntimeSelfCheck PASS` | `RUNTIME_PENDING` |
| R2 five-fixture joint matrix | 现有夹具分散，缺统一 witness | `PENDING` |
| R2 Play Mode | 未运行 | `PENDING` |
| R2 C++ full trace | R1-WP02 | `BLOCKED` |

根据 `D-009`，R2-VERIFY-01 与 `R3-INP-01` 都可在各自 Change Record 建立后连续执行；这不改变
它们的最小范围、分层验收或保护边界。
