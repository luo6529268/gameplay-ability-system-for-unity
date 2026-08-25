# HANDOFF — R2-SCHED-001 C++ T09～T16 scheduler/pass 边界

> 交接日期：2026-08-21  
> Change ID：`R2-SCHED-001`  
> 状态：`RUNTIME_PENDING`  
> 可以宣称：scheduler 代码、Unity 编译与 focused self-check 已通过。  
> 不可以宣称：C++ runtime full trace、joint fixture、Play Mode 或整个战斗系统已对齐。

## 1. 已完成的闭合范围

本批只实现 D-SCHED-001～004 的主调度骨架，且只改了：

1. `Assets/NTSD/Scripts/Simulation/NTSDBattleTickSystem.cs`；
2. `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`。

没有修改：

- C++ Release source / executable / Makefile / resource / configuration；
- `SimulationWorld.Passes.partial.cs`、candidate、collision、CPoint、held、link、damage 或 render
  writer 的任何公式；
- `D-SCHED-005`（human/AI 与 OID maintenance）和 `D-SCHED-010`（F1/F2 gate）；两者仍属于
  `R3-INP-01/02`；
- CentralOnly、Texture2DArray、dynamic Mesh、URP、1.5× visual scale、fixed-world camera、
  Authority400/MobileExtended/DesktopExtended、30 Hz、FrameInputSet、SoA/ECS、pool、worker、
  zero-GC 和 T8 暂缓边界。

## 2. C++ authority 与 Unity 调整

权威 C++ Release `game_tick(...)` 的关键顺序：

1. `game_tick.cpp:1423-1439`：first character Z clamp；
2. `1441-1643`：T09 first negative-link held scan；
3. `1648-1655`：`prev_frame2`、candidate collect、character consume；
4. `1657-1822`：random weapon drop 与 object consume；
5. `1821-1825`：CPoint + weapon sync；
6. `1827-1846`：positive-link validation；
7. `1848-1859`：second Z clamp；
8. `1860-2019`：T16 second negative-link held scan；
9. `2021-2087`：PreFrame / stage / render / postprocess / late tail。

调整后的 Unity scheduler 是：

    first Z clamp
      -> held#1
      -> collision snapshot / pair VRest / candidate collect
      -> character consume / random weapon drop / object consume
      -> candidate-consumption cleanup adapter
      -> CPoint + WeaponSync
      -> positive-link validation
      -> second Z clamp
      -> held#2
      -> existing PreFrame / stage / RenderDispatch / FramePostProcess / late tail

`EndCollisionCandidateConsumption` 仍是 Unity adapter，不被误写为 C++ gameplay pass；它保留在
object consume 之后、T14 CPoint 前。其 exact C++ carrier visibility 尚未 closed，保留给
R2-PASS-02 / R4。

## 3. 代码与回归断言

### Scheduler

- `RunFrameAdvancePhase` 已在 first clamp 后调用
  `ProcessNegativeHeldObjectsFirstPass`，再进入 snapshot/pair/candidate；
- `RunInteractionPhase` 已在 object consume 与 cleanup 后执行
  `ResolveCpointAndWeaponSync` → `ValidatePositiveHeldLinks` → second clamp →
  `ProcessNegativeHeldObjectsSecondPass`；
- private wrapper 明确区分 first/second held、CPoint+WeaponSync、positive link，避免再由
  历史 `PreInteraction` / generic held 名称误读时序；
- profiler 的 `BattleTickPhase.HeldProcess` 暂保持聚合名称，以避免无关的诊断 schema / stress
  report 改动；两个 source-contract wrapper 已在代码层清晰区分，性能细分不属于本 batch。

### Self-check

- 历史 `CheckReleaseTickRunsHeldStep12Once` 已替换为
  `CheckReleaseTickRunsHeldStep12Twice`：同一 tick 的 drink HP 从 20 下降到 18；
- 历史 `CheckReleaseTickCpointSyncPrecedesCandidates` 已替换为
  `CheckReleaseTickCpointSyncFollowsCandidates`：candidate consume 观察 pre-T14 state，T14
  之后才写 CPoint/WeaponSync 的 frame/position；
- `BattleRuntimeSelfCheck` 类说明已从“C# authority”更正为 C++ Release source contract。

## 4. 实际验证证据

| 层级 | 证据 | 结果 |
|---|---|---|
| S0 static order | 对 `NTSDBattleTickSystem` 的 isolate body 执行顺序检查。 | PASS：first clamp → held#1 → snapshot → pair → candidate → character/random/object → cleanup → CPoint → positive link → second clamp → held#2。 |
| S0 multiplicity | 同一 scheduler body 的 call count。 | PASS：Z clamp=2、held#1=1、CPoint=1、held#2=1；无 legacy private helper call。 |
| Ledger | `Tools/Validate-ChangeLedger.ps1`。 | PASS：两个 R2 脚本均由 `R2-SCHED-001` 覆盖；无未登记 script diff。 |
| Unity compile | UnityMCP 对唯一已打开实例 `gameplay-ability-system-for-unity@b1b02287` 调用 `refresh_unity(force, scripts, request)`。 | PASS：domain reload 后 ready；`Library/ScriptAssemblies/Assembly-CSharp.dll` 更新为 2026-08-21 22:10:02。 |
| Unity Console | UnityMCP `read_console(types=[error])`。 | PASS：0 条 error。 |
| focused self-check | `Temp/NTSD_BattleRuntimeSelfCheck.result`，在上述刷新后请求。 | PASS：2026-08-21 22:12:49，内容为 `PASS`。 |

没有运行 C++ executable、C++ build、C++ trace、Unity Play Mode、性能测试或独立 R4/R5 fixture。

## 5. 仍未关闭的项

1. D-SCHED-001～004 目前是“逻辑已写 / compile+self-check PASS / joint 待测”，不是 `VERIFIED`；
2. held#1 → candidate/consume → CPoint/link → held#2 的真实 relation、slot、position、frame-history
   chain 需 R4/R5 的 joint fixture；
3. CPoint、held、link、candidate 的字段公式没有在本批改动；
4. candidate cleanup adapter 的 exact C++ lifecycle 可观察性仍为 UNKNOWN；
5. Play Mode 不能单独证明这段内部顺序，必须在 R4/R5 依赖闭合后按夹具联合验收；
6. R1-WP02 full C++ trace 仍 BLOCKED（B-R1-WP02-01～04）。

## 6. UnityMCP 连接注意

本次 UnityMCP 成功发现并控制了唯一实例，完成 refresh 和 Console 查询。后续 self-check 完成后，
最后一次连接查询短暂返回“未发现 Unity instance”；这没有否定已落盘的 assembly timestamp、Console
和 self-check result，但下一包开始前必须重新读取 `mcpforunity://editor/state`，不要复用旧连接或
假设 bridge 仍在线。

## 7. 后续边界

下一步推荐的是 `R2-PASS-02`，但只能在用户明确确认后创建新的 Change Record。它负责
D-SCHED-006～009、011～012 的 adapter / clamp / candidate lifecycle / tail / cursor 审计；
不得把 D-SCHED-005/010 或任何 R3 输入行为混入。

若用户选择先做 R3 或要求 Play Mode，先依据本 handoff 和
`docs/ai/RESEARCH/R1-SOURCE-007-subflow-acceptance-matrix.md` 重新确认依赖与验收边界。
