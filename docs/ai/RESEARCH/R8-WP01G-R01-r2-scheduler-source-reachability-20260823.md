# R8-WP01G-R01 — R2 scheduler source/reachability closure

> 日期：2026-08-23  
> 范围：`D-SCHED-006`、`D-SCHED-008`  
> 证据边界：C++ Release source + Unity current source；未运行 C++、未运行 Unity、未修改 gameplay

## 1. 结论

| D-ID | 本包结论 | 说明 |
|---|---|---|
| `D-SCHED-006` | `SOURCE-CLOSED / EQUIVALENT WITH APPROVED CAPACITY ADAPTER / RUNTIME_PENDING` | 两次 character-DAT Z clamp 的相对时点、当前 DAT 资格、slot 升序、double Z clamp 与 `(int)` ZInt 写回均已闭合。Unity 扫描扩展 logical capacity 是已批准容量适配，不回退到 400。没有找到可导致战斗结果不同的额外 writer；C++ runtime trace 仍不可得，因此不升级为 runtime VERIFIED。 |
| `D-SCHED-008` | `SOURCE-CONFIRMED CONDITIONAL DIFFERENCE / UNFIXED` | 完整 tick 下，两边在 object consume 后到 tail 之间没有 candidate reader，Unity 提前失效 adapter range 的结果等价；但 F1/step-wait 在 render 后提前返回并跳过 entity post-frame tail 时，C++ 保留 `mp/mp2/mp3/mp4/hit_confirm2` 及 candidate arrays 到下一 tick，而 Unity 下一次 collect 会先清 `HitCandidateCount`/`HitConfirm2` 并重建 store。该条件分支的下一 tick carrier 内容、cap、顺序和可能的 consume 结果不等价。 |

本结论纠正 `R8-WP01G` 初次综合时“当前没有未修复 source-confirmed difference”的临时结论。
本包没有修改 Unity 或 C++；修复必须进入独立 `R2-CANDIDATE-TAIL-01` Task/Change。

## 2. Authority 与 release build 参与性

- `J:\QQFile\NTSD2.4\ntsd_release\Makefile:18,22-23,32` 把
  `entity_collision.cpp`、`collision.cpp`、`collision_collect.cpp` 与 `game_tick.cpp` 列入 release build；
- 正式入口是 `src/entity/game_tick.cpp:945-2091` 的 `game_tick(...)`；
- 本包只读取 authority source，没有运行、构建、复制、修改或向 authority 目录写入；
- `R1-WP02` full trace 继续 `BLOCKED`，不以旧 C# parity/self-check 代替 C++ runtime 证据。

## 3. D-SCHED-006 — 两次 character Z clamp

### 3.1 C++ source contract

| 项目 | C++ Release source contract |
|---|---|
| 第一次时点 | `game_tick.cpp:1423-1438`：frame advance/death/respawn 后，第一轮 negative-held 前。 |
| 第二次时点 | `game_tick.cpp:1848-1858`：两个 collision consume、CPoint/WeaponSync、positive-link validation 后，第二轮 negative-held 前。 |
| 遍历 | 两处均为 `i=0..MAX_OBJECTS-1` 固定 slot 升序。 |
| 资格 | 两处均要求 `e.active && e.char_data && entity_is_character_dat(e)`；`entity_runtime_views.h:21-23` 将 character-DAT 定义为 current `char_data->obj_type == 0`。 |
| 写入 | 只把 `e.z` clamp 到 `world.bg.zboundary_min/max`，随后执行 `e.z_int=(int32_t)e.z`；不写 X/Y、velocity、Prev2 或 frame。 |
| 新生/身份变化 | 每处都重新读取 live slot/current `char_data`；两处之间成为 active character-DAT 的对象会进入第二次扫描。 |
| inactive | `active=false` 的 free/merge partner 不进入。 |

### 3.2 Unity crosswalk

| C++ 合同 | Unity current source | 结论 |
|---|---|---|
| 两次时点 | `NTSDBattleTickSystem.cs:325-327,351-377` | 调度位置一致。 |
| slot 升序 | `BattleEcsCharacterStageZPass.ExecuteDataOriented` 按 `0..runtimeSlots.LogicalCapacity-1` 读取 `RuntimeSlotTable` | Authority400 范围内与 C++ 相同；高槽是用户批准的 Mobile/Desktop 扩展容量适配。 |
| active/current DAT | `IsEligible` 要求 claimed、entity、PS、`IsActiveForCurrentPassInternal` 与 `IsStageBoundedCharacter()`；后者读取 current DAT type | `PendingFlushDestroy`、pending unregister 与 `OidMergeDormant` 分别映射 C++ inactive。`LF2Entity` 基类初始化非空 `PS`，`PhysicsState.z` 直接绑定 `Runtime.Z`。 |
| double/int 写回 | `runtime.Z = ClampZ(...)`、`runtime.ZInt=(int)clampedZ` | 与 C++ 截断语义一致。 |
| fresh visibility | 每次调用重新读取 runtime slot table/current DAT cache | 两处之间的新 active/current-character 对象可进入第二次 clamp。 |
| compatibility fallback | 非 exact `LF2Character` 调用 `RefreshRuntimeSnapshot()` | 已扫描全部 production override：base/LF2Character 只把 canonical/alias 字段写回相同 Runtime；LF2WeaponBase 额外写的 Picker/WeaponDropHurt 也来自同一 Runtime-backed property。没有找到改变战斗状态的独立 writer；仍只给 source-equivalent 结论，不冒充 runtime trace。 |

### 3.3 D-SCHED-006 disposition

`D-SCHED-006` 不需要 gameplay 修复。它从 `UNKNOWN` 更新为：

`SOURCE-CLOSED / EQUIVALENT WITH APPROVED CAPACITY ADAPTER / RUNTIME_PENDING`。

现有 Legacy-vs-DataOriented shadow/test 只能作为 Unity 内部回归证据；C++ full trace 不可得时，不能写成
“C++ runtime VERIFIED”。

## 4. D-SCHED-008 — candidate carrier lifecycle

### 4.1 C++ normal completed tick

1. 上一完整 tick 的 `run_entity_postframe_tail` 在 `game_tick.cpp:310-350` 对每个 active entity 调用
   `clear_hit_candidate_carriers()`；`game_world.h:202-206` 写：
   `mp=0`、`mp2/mp3/mp4=1000`、`hit_confirm2=0`；candidate arrays 不必清零，因为 `mp=0` 使其不可消费。
2. `collision_collect_candidates` 在 `game_tick.cpp:1648-1652` / `collision_collect.cpp:363-372`
   按 slot/pair 顺序向当前 `mp` 追加 candidate；nearest/kind1 selection 同时使用 `mp2/mp3`。
3. `collision.cpp:32-47` 的两个 consume pass 读取同一 `mp` 和 candidate arrays。
4. object consume 后，CPoint、WeaponSync、held、preframe、render、postprocess/late 没有读取 candidate carrier；
   全仓 live entity/render source read inventory只在 collect/consume读这些字段。
5. 正常 tick 最后再次由 entity post-frame tail 清 carrier。

### 4.2 Unity normal completed tick

1. `BruteForceSceneQuery.CollectCollisionCandidates()` 在 collect 起点失效旧 range、归还旧 list，并由
   `ResetCandidateCollectionState()` 清 `HitConfirm2`、`HitCandidateCount` 与 selection distances；
2. collect 写 `HitCandidateCount` 与 candidate store/list；两个 consume pass读取同一冻结 range；
3. `NTSDBattleTickSystem.RunInteractionPhase` 在 object consume 后调用
   `EndCollisionCandidateConsumption()`；它只失效 range、归还 list、结束 store visibility，不写 Runtime count；
4. `EntityPostFrameTailAll` 清 `HitConfirm2` 与 `TransientMp*`；实际 store count `HitCandidateCount` 保留到下次 collect，
   但 range 已无效且该区间无 reader；
5. 因此**正常完整 tick 的可消费行为等价**，虽然 Unity adapter storage 的物理释放时点早于 C++ carrier reset。

### 4.3 条件性 first difference：F1/step-wait early return

C++ `game_tick.cpp:2067-2077` 在 preframe/stage/render 后判断 F1 slow wait；命中时直接 `return`，不会执行
`run_entity_postframe_tail`。因此：

- 当前 tick 的 `mp`、candidate arrays、`mp2/mp3/mp4` 与 `hit_confirm2` 保留；
- 下一次 `game_tick` 的 collect 没有 begin-reset，会从保留的 `mp` 继续执行 cap/nearest/tie/append；
- 该行为可改变下一 tick candidate ordinal、20-cap、RNG tie 与 consume 结果。

Unity `RunPresentationAndCleanupPhase` 同样在 step-wait 下跳过 postprocess/late/entity tail，但
`EndCollisionCandidateConsumption()` 已在更早的 interaction phase 无条件执行；下一次 collect 又无条件
`ResetCandidateCollectionState()`。因此 Unity 丢弃了 C++ 应保留的 carrier。这个差异由双方 source 直接证明，
不依赖可选 C++ runtime trace。

### 4.4 不能采用的伪修复

- 不能只在 tail 把 `HitCandidateCount=0`；它会让 normal tick字段更整齐，却不恢复 early-return carry；
- 不能只删除 `EndCollisionCandidateConsumption()`；下一次 collect仍会 invalidate/reset，且 list pool/store visibility
  可能泄漏或破坏0-GC；
- 不能把 F1 路径当“仅调试”而忽略；它是 `game_tick(...)` live control flow的一部分；
- 不能回退 candidate store、SoA/ECS、pool、role-aware/fallback、扩展容量或30Hz。

## 5. 下一修复包

建议进入：`R2-CANDIDATE-TAIL-01 — step-wait candidate carrier retention`。

它必须先建立独立 Change Record，再设计一个显式 lifecycle state，至少同时满足：

1. normal completed tick：entity tail 后 count/selection/hit-confirm 归默认，store/list可归池；
2. F1/step-wait early return：carrier与ordered entries跨 tick保留；
3. 下一 paused tick：按 C++ source从保留 count继续cap/selection/append，不重复或重排旧 entries；
4. 恢复 normal tick 后：两个 consume pass仍读同一 carrier，并在真正 tail 后清理；
5. LegacyOracle、StoreAuthority、fallback/optimized结果、RNG state/call count和candidate ordinal一致；
6. warmed normal与paused路径0 B，不破坏pool/generation/capacity；
7. 不修改 C++、render、input semantics、CPoint/held/link、DAT、T8或IL2CPP。

## 6. 验证与边界

- C++ Makefile/live caller/callee/field read-write inventory：`PASS（source）`；
- Unity scheduler/store/runtime/tail crosswalk：`PASS（source）`；
- D-SCHED-006：`SOURCE EQUIVALENT`，runtime trace仍 `BLOCKED`；
- D-SCHED-008：`SOURCE-CONFIRMED CONDITIONAL DIFFERENCE`，Unity尚未修复；
- Unity compile/self-check/Play Mode：本只读包未运行；
- Unity/C++ gameplay脚本diff：本包新增为0；
- T8默认`stage.dat`仍暂缓；IL2CPP按用户决定排除。
