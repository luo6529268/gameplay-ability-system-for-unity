# R2-PASS-01 — C++ 主 scheduler 与 pass 边界对齐

> 建立日期：2026-08-21  
> 状态：RUNTIME_PENDING（静态顺序、Unity 编译和 focused self-check 已通过；联合 / Play Mode / full trace 待后续）  
> Change ID：`R2-SCHED-001`  
> 唯一行为 authority：`J:\QQFile\NTSD2.4\ntsd_release` 的 release live `game_tick(...)`。

## Goal

只恢复 C++ Release 的 T09～T16 相对调度边界，使 Unity 在一次逻辑 tick 内按以下顺序执行：

1. 初次 character Z clamp；
2. T09 第一轮 negative-link held；
3. collision snapshot / pair VRest / candidate collect；
4. character collision consume → random weapon drop → object collision consume；
5. candidate-consumption cleanup adapter；
6. T14 CPoint + weapon sync；
7. T15 positive-link validation；
8. 第二次 character Z clamp；
9. T16 第二轮 negative-link held；
10. 其后才进入既有 PreFrame / stage / RenderDispatch / FramePostProcess / late update。

该顺序只定义调度边界；不在本包修改任一 held、CPoint、WeaponSync、candidate、collision、
damage、link-cleanup 或 render 公式。

## Authority / Evidence

### C++ Release source（VERIFIED source）

- `Makefile` 将 `src/entity/game_tick.cpp`、`frame_advance.cpp`、`collision.cpp` 等列入
  `ntsd_new.exe` release source list；
- `game_tick.cpp:1423-1439`：第一轮 character Z clamp；
- `game_tick.cpp:1441-1643`：T09 第一轮升序 `link_state < 0` held loop；
- `game_tick.cpp:1648-1655`：`prev_frame2` snapshot、candidate collect、character consume；
- `game_tick.cpp:1657-1822`：random weapon drop 与 object consume 前的固定位置；
- `game_tick.cpp:1821-1825`：object collision consume 后才 CPoint / weapon sync；
- `game_tick.cpp:1827-1846`：positive-link validation；
- `game_tick.cpp:1848-1859`：第二次 Z clamp；
- `game_tick.cpp:1860-2019`：T16 第二轮升序 negative-link held loop；
- `game_tick.cpp:2021-2087`：PreFrame / stage / render / postprocess / late 的后续边界。

### Unity 当前映射（VERIFIED source）

- `Assets/NTSD/Scripts/Simulation/NTSDBattleTickSystem.cs:310-333` 当前把
  CPoint/WeaponSync → positive link → second Z clamp → 单轮 held 放在 candidate collect 前；
- `NTSDBattleTickSystem.cs:341-351` 当前在稍后才 character/object consume；
- `SimulationWorld.Passes.partial.cs:2226-2357` 是 CPoint/WeaponSync writer 入口；
- `SimulationQueryAndLinkModule.cs:39-89` 是 negative-held 升序扫描入口；
- `SimulationWorld.Passes.partial.cs:1352-1356` 是 candidate-consumption cleanup adapter。

因此 D-SCHED-001～004 是静态 source 已确认的调度差异；它们尚未有 full C++ runtime trace，
也不能因此被写成“完整行为已验证”。

## Scope

### 本包允许

- 仅在 `NTSDBattleTickSystem` 中重排已有 pass 调用，并将第一/第二轮 held 的意图命名清楚；
- 若 scheduler 调整要求，最小修改 `SimulationWorld.Passes.partial.cs` 的 pass adapter，但不得修改
  CPoint/collision/held 公式；
- 更新或新增只覆盖该顺序的 `BattleRuntimeSelfCheck` 断言；
- 更新 `R2-SCHED-001`、CHANGE-LEDGER、STATE、差异登记册和 handoff。

### 明确延后

- D-SCHED-005：human/AI 与 OID maintenance，归 `R3-INP-01`；
- D-SCHED-010：F1/F2 gate 与 battle-entry clear，归 `R3-INP-02`；
- D-SCHED-006～009、011～012，归 `R2-PASS-02`；
- candidate/hit/CPoint/held/link 的字段和公式修复，归 R3～R5；
- render handoff、CentralOnly、Texture2DArray、Mesh、URP，归 R6；
- T8 默认 `stage.dat` 部署。

## Files likely involved

| 文件 | 预期职责 |
|---|---|
| `Assets/NTSD/Scripts/Simulation/NTSDBattleTickSystem.cs` | R2 的唯一 scheduler 调整点。 |
| `Assets/NTSD/Scripts/Simulation/SimulationWorld.Passes.partial.cs` | 仅当 pass adapter 的 cleanup/flush contract 必须局部澄清。 |
| `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs` | 更新旧的“单轮 held / CPoint 在 candidate 前”历史断言，并新增 source-contract 回归。 |

不得修改 C++ Release、DAT、scene、resource、input asset、renderer、性能架构、slot/profile 或
其他 R3+ 代码域。

## Acceptance Contract

| 层级 | 验收内容 | 当前状态 |
|---|---|---|
| S0 静态 source | Unity 调用顺序逐项对应 C++ T09→T16；D-SCHED-001～004 无旧位置残留。 | PASS：顺序和两次 clamp/held multiplicity 已脚本化核验。 |
| S1 focused self-check | 同一 release tick 运行 held#1 与 held#2；CPoint/WeaponSync 不再先于 candidate/object consume。 | PASS：post-refresh `BattleRuntimeSelfCheck` result = PASS。 |
| S2 联合 fixture | held#1 → candidate/consume → CPoint/link → held#2 的 relation/slot/position/history 依赖。 | 待 R4/R5；不得伪造为本包完成。 |
| S3 Unity compile + BattleRuntimeSelfCheck | 脚本为 0 error，目标断言实际通过。 | PASS：UnityMCP `refresh_unity(force/scripts/compile)` 成功、Console 0 error、更新后的 `Assembly-CSharp.dll` 与 self-check PASS。 |
| S4 Play Mode | 仅在 S3 成功后，用 held weapon / CPoint / opoint 关联场景复验。 | PENDING |
| S5 C++ full trace | 若 R1-WP02 将来解除 blocker，再以同 fixture first-difference 对照。 | BLOCKED |

## 实施记录

- `R2-SCHED-001` 已把第一轮 negative-held 放到首次 Z clamp 后、collision snapshot 前；
- candidate collect 与 character/random/object consume 保持先完成，随后才 CPoint/WeaponSync、
  positive link、第二次 Z clamp 和第二轮 negative-held；
- 没有修改 `SimulationWorld.Passes.partial.cs`、CPoint/held/candidate/link writer 公式、renderer、
  input 或资源；
- `BattleRuntimeSelfCheck` 已将历史“held 只跑一次”和“CPoint 先于 candidate”的断言改为
  C++ source-contract 方向；刷新后的实际 result 于 2026-08-21 22:12:49 返回 PASS。

## Protected Unity boundaries

- 不回退 CentralOnly、Texture2DArray、dynamic Mesh、URP 或 Legacy SpriteRenderer；
- 不改变 `BattleVisualScale = 1.5`、fixed-world logic camera/display separation；
- 不回退 Authority400 fixture 与 MobileExtended/DesktopExtended 容量合同；
- 不改变 30 Hz、FrameInputSet、slot/generation、SoA/ECS、pool、worker 和 zero-GC 方向；
- T8 默认 `stage.dat` 继续暂缓。

## Stop Conditions

立即停止并更新 Change Record / handoff，若：

1. 为恢复顺序而需要修改 CPoint、held、candidate、collision、link 或 damage 公式；
2. candidate cleanup adapter 在 object consume 后不能安全结束，且需要扩大到 R4；
3. 旧 self-check 无法改写为 C++ source-contract 验收、需要伪造 C++ runtime 结论；
4. 发现用户批准的 Unity adaptation 会被回退；
5. C++ source evidence 与 R1 合同冲突，或用户提出新的 Change Request。

## Out of Scope

- 不宣称本包完成后 Unity 已完整对齐 C++；
- 不运行或修改任何 C++ executable/source/build/config/resource；
- 不自行解除 R1-WP02；
- 不开始 R2-PASS-02 或 R3～R8。
