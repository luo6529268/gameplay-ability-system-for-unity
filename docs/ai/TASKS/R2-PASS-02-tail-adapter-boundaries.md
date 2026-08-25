# R2-PASS-02 — tail / adapter 边界审计与最小 mode2 reset 对齐

> 建立日期：2026-08-21  
> 状态：RUNTIME_PENDING（仅 `R2-SCHED-002` 的 mode2 reset 时点已完成代码级验证；其余条目保持 source-disposition）  
> Change ID：`R2-SCHED-002`  
> 唯一行为 authority：`J:\QQFile\NTSD2.4\ntsd_release` 的 release live `game_tick(...)`。

## Goal

在已完成的 `R2-SCHED-001` 调度骨架上，重新审计 D-SCHED-006～009、011～012。
本批只在 C++ source contract 完整闭合且不触及 input、collision writer、CPoint/held/link、
render 或容量策略时改动 Unity。

当前唯一允许的脚本行为改动是 D-SCHED-011 的一个子边界：令 Unity `Mode2Request`
像 C++ `g_game_mode2` 一样保持到 `EntityPostFrameTailAll` 完成之后，再在 `BattleResultsFlow`
之前清零。

## Authority / Evidence

### C++ Release source（VERIFIED source）

- `src/entity/game_tick.cpp:310-350`：`run_entity_postframe_tail` 在每个 active entity 上处理
  mode2、heal/catch、candidate carrier；
- `game_tick.cpp:2078-2089`：严格顺序是 frame postprocess → late entity update →
  `run_mode2_random_weapon_drop` → `run_entity_postframe_tail` →
  `g_init_stats = 0` → `g_game_mode2 = 0`；
- `src/core/main.cpp:187-201`：`g_init_stats` 来自 F7，`g_game_mode2` 来自 F8/F9；
- `src/entity/collision_collect.cpp:363-376`、`collision.cpp:32-47`：C++ candidate 先建立、
  再由两个 consume loop 使用同一 carrier；
- `game_tick.cpp:1423-1438`：两次 character Z clamp 的 C++ 筛选、double clamp 与 int 写回依据；
- `game_tick.cpp:2183-2188`：普通 `spawn` 选择最低空闲 slot。

### Unity static evidence（VERIFIED source）

- `NTSDBattleTickSystem` 的后半段目前为 postprocess → late entity → mode2 tail →
  entity postframe tail → results；
- `SimulationWorld.Passes.partial.cs:2676-2698` 当前在 mode2 tail 内立即
  `SetMode2Request(0)`，早于 entity postframe tail；
- `EntityPostFrameTailAll:1798-1865` 才清 candidate carrier；
- `BruteForceSceneQuery.EndCollisionCandidateConsumption:5149-5154` 只释放 Unity
  candidate cache / visibility，不写 runtime candidate carrier；
- `BattleEcsCharacterStageZPass` 已有 legacy / data-oriented shadow 边界；
- `RuntimeSlotAllocator.AllocateLowest` 使用最小空闲 slot，扩展容量是用户已批准 adapter。

## Item Disposition

| ID | 本次结论 | 是否改代码 | 原因 / 后续 owner |
|---|---|---:|---|
| D-SCHED-006 | `UNKNOWN`（eligibility / derived DAT / newborn 仍未闭合） | 否 | R3-frame / R5 fixture；不得将 legacy-vs-ECS shadow 当 C++ proof。 |
| D-SCHED-007 | 已映射，candidate adapter 待联合验收 | 否 | snapshot + pair-vrest 是 Unity adapter，保留给 R4-COL-01。 |
| D-SCHED-008 | source mapping 已补强，运行时待测 | 否 | C++ carrier 在 entity postframe tail 清；Unity cache-end 只释放 adapter cache，不能删除。 |
| D-SCHED-009 | scheduler shape 已映射，表现待测 | 否 | fixed-world camera / CentralOnly 是批准的 Unity adaptation；R6 负责可观察 render contract。 |
| D-SCHED-011 | `Mode2Request` reset 时点已写入并 compile + focused self-check PASS；`g_init_stats` 输入/数据契约仍 UNKNOWN | 是，仅 mode2 reset | 仍缺 joint fixture / Play Mode / C++ trace；F7 / init-stats 归 input / tail 专项包，禁止在本批补造字段。 |
| D-SCHED-012 | 容量为批准 adapter；lowest-slot allocator 已映射，cursor/newborn 待测 | 否 | R5-LIFE-01；不得回退扩展容量或 generation/pool。 |

## Scope

### 本包允许

- 仅创建 `R2-SCHED-002` Change Record 后，移动 `Mode2Request` 的 reset 到 entity postframe
  tail 之后、results flow 之前；
- 为该时序补充 focused `BattleRuntimeSelfCheck`；
- 更新 Ledger、STATE、差异登记册、总计划与 handoff。

### 明确排除

- 任何 `g_init_stats` / F7 输入支持；
- 任何 Stage-Z eligibility、candidate 算法、snapshot、pair-vrest、cache/generation、slot allocator、
  CPoint、held、link、opoint、render、scene、DAT、资源、profile 或性能修改；
- D-SCHED-005、D-SCHED-010 和全部 R3 input 工作。

## Acceptance Contract

| 层级 | 验收内容 | 当前状态 |
|---|---|---|
| S0 source order | mode2 tail → entity postframe tail → mode2 reset → results flow。 | `PASS`（C++ source + Unity static order） |
| S1 focused self-check | mode2 request 在 mode2 tail / entity tail 之间仍可见，reset 后为 0。 | `PASS`（2026-08-21 22:58:30） |
| S2 compile | 现有 Unity Editor 编译为 0 error。 | `PASS`（UnityMCP scripts refresh；`error CS` 0 项） |
| S3 BattleRuntimeSelfCheck | post-refresh request result 为 PASS。 | `PASS` |
| S4 Play Mode | F8/F9 / mode2 可见性需要输入合同；本包不伪造。 | `PENDING / out of scope` |
| S5 C++ full trace | R1-WP02 仍 BLOCKED。 | `BLOCKED` |

## Stop Conditions

立即停止并记录，而不是扩大修改，若：

1. `Mode2Request` 在 Unity 的 entity postframe tail 中存在未审计 consumer；
2. 完成顺序需要补造 `InitStats`、F7 input、result state 或任何 input API；
3. 修复需要移动或修改 candidate、snapshot、pair-vrest、Stage-Z、allocator 或 render writer；
4. 编译/self-check 失败且根因超出这三个文件；
5. C++ source 与上述顺序依据冲突。

## Protected Unity boundaries

- CentralOnly、Texture2DArray、dynamic Mesh、URP、1.5× visual scale 与 fixed-world camera 保持；
- Authority400、MobileExtended、DesktopExtended、slot/generation、SoA/ECS、pool、worker、0-GC 方向保持；
- T8 默认 `stage.dat` 继续暂缓；
- C++ Release 严格只读，不运行、不构建、不写入。

## 本次执行结论

`R2-SCHED-002` 只关闭了 D-SCHED-011 中 **mode2 reset 时点的代码级层次**：在现有
Unity Editor 的 scripts refresh/compile 后，request 驱动的 `BattleRuntimeSelfCheck` 返回
`PASS`。它没有关闭 `g_init_stats` / F7、tail 的 joint fixture、F8/F9 Play Mode 或 C++ full
trace；本 Work Package 因此保持 `RUNTIME_PENDING`，不得借此进入 R3 或改动其他 D-SCHED 项。
