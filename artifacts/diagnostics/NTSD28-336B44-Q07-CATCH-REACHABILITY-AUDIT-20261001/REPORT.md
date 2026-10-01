# 336B44 Q07 抓取链增量可达性与静态写者审计

状态：`FORMAL_CONTENT_REACHABLE / STATIC_WRITER_GAPS / RUNTIME_FIRST_DIFFERENCE_PENDING`。本报告是只读源码、当前正式内容及 Unity 写者核对；没有运行新的正式根、原 Battle Scene 或测试，不将静态差异记为运行时首差。

## 当前权威入口与内容

- 当前选定的正式 `NTSD2.8-Logan.exe` SHA-256 是 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。`source/ntsd28_core/src/simulation/battle_world.cpp` 的 `BattleWorld28::advance_catch_relations()` 按 `tick_action_snapshot` 读取抓取者与被抓者的 kind-1/kind-2 CPOINT、处理 `decrease`/输入/投掷；`BattleWorld28::settle_catch_relations()` 随后处理被抓者动作、伤害、停顿和定位。两者须在正式 playable `SimulationTickDriver28::step()` 调用链中同态验证，不能只测单个字段。
- 正式 `resources/runtime/decoded_dat/data/data.txt` 的 OID16 是 `c\\gaa\\gaa.dat`，OID75 是 `c\\bee\\bee.dat`；项目选中 `Assets/NTSD/Content/LoganRuntime/decoded_dat/data/data.txt` 同索引。OID75 的 frame375 有 `throwvx:25 / throwvy:-10 / vaction:181`，frame78 有 `throwvx:232 / throwvy:-2 / vaction:181`；OID16 的 `gaa.dat` 有 `decrease:-3 / throwvx:3 / throwvy:-5 / vaction:180`。这证实字段与角色内容可达，不证明实际对战已经走到分支。
- 既有 C022/C029 的 OID52 跳跃抓取已证自然建立 kind-1/kind-2 关系，原 Battle Scene 32 tick/960 字段一致；该夹具可复用关系入口，但它自身不自动覆盖 OID75/OID16 的投掷或负 `decrease`。

## 候选写者差异

| ID | 正式源码 | Unity 当前写者 | 下一验证门 |
|---|---|---|---|
| C042 | `battle_world.cpp` 投掷分支只把抓取者 `frame.frame_counter=0`；被投者在选定 `vaction` 后保留其原计数。 | `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleCpointWriter.cs` 的 `ApplyThrow` 在写被投者帧后执行 `victim.AttackingCounter=0`；`LF2Entity.ApplyCpointThrowStep10` 另有平行调用路径，须先证明正式生产 owner，避免只修一处。 | 用正式 OID75 投掷帧、被投者非零计数建立同一初态；记录源/正式根/原 Scene 首个投掷 tick 的两方 action、counter、速度、关系与后继 tick。只有实际不同才改共享写者并做聚焦正反例。 |
| C044 | `battle_world.cpp` 负 `decrease` 跨零释放时先写 `pending_hit_impulse` 的计数、X/Y，后续物理消费；本轮不立即改 `motion`。 | `BattleCpointWriter.RunKind1` 释放时同时写 `victim.KnockbackVx/Vy` 和 `victim.Runtime.Vx/Vy`。 | 用正式 OID16 负 `decrease` 帧构造跨零与未跨零控制，逐 pass 观察 pending 与真实速度；不以末帧速度偶然相等证明时序。 |
| C040/C045 | `settle_catch_relations` 用被抓者当前动作帧的 center，定位 CPOINT 则取抓取者 `vaction` 对应的被抓者帧；正伤害后的停顿读 `recover`，`cover` 用于 Z/朝向。 | `BattleCpointWriter.SyncHeldPosition` 从被抓者当前帧同时读 center/CPOINT；`ApplyHeldInjury` 用 `cpoint.Cover` 判停顿，数据契约已有 `Recover`。 | 已检当前暂存 `decoded_dat` 无显式 `recover:`；正式索引 OID65 `ank.dat` line1815 的 CPOINT 为 `injury:100 / cover:1`，因此 C045 的默认 `recover=0` 与 `cover=1` 有字段分歧候选。先证明该帧的自然抓取伤害消费；C040 另找当前帧与 `vaction` 帧 CPOINT 不同的可达帧。不能凭字段名直接修改。 |
| C043 | `advance_catch_relations` reciprocal kind-2 失效时只写抓取者当前动作 0，保留历史关系列；孤立 kind-2 另走当前动作 212 分支。 | `RunKind1` 与 `RunKind2Validation` 有对应早退/失效动作；仅静态看未发现对历史关系列的清零。 | 用关系不匹配及孤立 kind-2 双控制观察原槽和复用后的状态；无运行首差前保持待触发。 |

## 执行顺序

优先做 C042 的正式 OID75 投掷非零被投计数双控制，因为它有选中角色帧和明确的单字段分歧；控制 A 在投掷前给被投者非零计数，控制 B 给零计数。当前正式根 `main.cpp` 所列 LFR 初态覆盖只有 action、facing、MP，没有 frame-counter override；因此不能直接把手设非零计数的源码/Unity受控探针说成正式根同态。先寻找正式根 LFR 正常输入可形成的抓取→投掷入口，并从根 trace 读取计数；若自然入口未形成，则分别标注受控机制证据与正式根前置未证。C044 次之；C040/C045 需先完成字段值差异筛选。所有脚本改动须另建 Task/Change 并先记录首差；不改 DAT、Scene、配置、非战斗功能或旧资源。
