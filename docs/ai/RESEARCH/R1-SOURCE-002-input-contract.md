# R1-SOURCE-002 — C++ 输入、组合键、AI 与逻辑帧边界源码合同

> 状态：COMPLETED（仅静态 source contract）。  
> C++ authority：`J:\QQFile\NTSD2.4\ntsd_release` 中参与 `ntsd_new.exe` release
> build 的 `src/core/main.cpp`、`src/entity/game_tick.cpp`、
> `src/entity/entity_collision.cpp`、`src/input/input_handler.cpp`。  
> Evidence：VERIFIED（source）表示直接读取到当前 C++ release source；未运行
> `ntsd_new.exe`，未取得 runtime trace，未以旧 C# 推定规则。

## 1. 证据边界

本合同只回答“C++ source 写了什么、Unity source 当前在哪里做相同或不同的事”。它不回答：

- 当前 source tree 是否与已有 `ntsd_new.exe` 精确同一 build；该项仍为
  `B-R1-WP02-04`；
- 某一条静态差异是否已在真实战斗场景复现；
- 输入绑定资产里 W/S/A/D/J/K/L 的实际 Inspector/InputAction 配置；那是非脚本
  资产与 Play Mode 验收项，当前证据为 UNKNOWN；
- C++ 400 slot 实现是否应限制 Unity 的生产容量；不应。`Authority400` 只是对照
  profile，`MobileExtended` / `DesktopExtended` 的交付边界保持不变。

## 2. C++ post-cooldown input 的已确认调用合同

| 序号 | C++ source 坐标 | 已确认行为 | 证据 |
|---|---|---|---|
| I00 | `game_tick.cpp:990-1005` | 先 `cooldowns_tick(world)`；重置 `g_dword_44905C`；若 `g_battle_step_mode == 2`，设 gate=1 且 mode=1；只有 callback 存在且不处于 `mode==1 && gate!=1` 时才调用 `post_cooldown_input()`。 | VERIFIED（source） |
| I01 | `main.cpp:4022,4566-4608` | `simulation_tick_driver.step_one_tick` 的第二个 lambda 是该 callback；其中先对 active `p1`、`p2` 调用 `input.poll(...)`。 | VERIFIED（source） |
| I02 | `main.cpp:5505-5522` | 同一 callback 内，`world.game_tick > 1` 时按 `0..MAX_OBJECTS-1` 升序扫描 active、当前 DAT `obj_type == 0` 的实体；AI 先 `prepare_ai_input(e, world, world.input_phase)`，随后所有该类实体调用 `apply_input(e, world.camera_x, &world)`。 | VERIFIED（source） |
| I03 | `game_tick.cpp:1006-1159` | callback 返回后才开始前 20 slot 的 OID 7/8/51 特殊维护。 | VERIFIED（source） |

**I00–I03 的顺序不变量**：同一个 C++ logic tick 中，人类 polling、AI key
generation、combo/direct input resolve 与动作输入分派全部位于 OID 7/8/51 特殊维护
之前。不能只因 Unity 把它们都叫作“input”就把中间位置改变视为无差异。

## 3. C++ human input 的字段合同

`InputHandler::poll(Entity&, InputConfig)`（`input_handler.cpp:1555-1613`）在一次 human
poll 中按以下顺序执行：

1. 将七个 `key_*` 复制到对应 `prev_*`；
2. 从物理输入读取 seven held keys：right/left/up/down/attack/jump/defend；
3. 递减 input-local 的 `cd_right/left/up/down/jump/attack/defend/defend_lock`；
4. 对每个“此前为 0、当前严格等于 1”的新按下边沿，把对应 cooldown 写为 5，且把
   key code 追加到六格 `input_history` 尾部；
5. combo wrapper 不在 polling 中运行，而在稍后的 `apply_input` 中运行。

边沿 code 与 cooldown 交叉写入为：

| logical key | history code | C++ 新边沿写入 |
|---|---:|---|
| right / left / up / down | 6 / 4 / 8 / 2 | 对应 direction cooldown = 5 |
| attack | 9 | `cd_defend = 5` |
| defend | 0 | `cd_jump = 5` |
| jump | 5 | `cd_attack = 5` |

`cooldowns_tick` 不是这一组 input-local cooldown 的替代物：其 source
(`entity_collision.cpp:64-89`) 只处理 active slot 的 Arest 与 attack-exempt 清理。
human input cooldown 是 `poll` 的自身副作用。

## 4. C++ AI / 组合键 / 动作输入合同

### 4.1 AI 输入

- `prepare_ai_input`（`input_handler.cpp:1615-2353`）先在需要时 roll previous key 并
  清 current key；之后做目标扫描、team / HP / state / air-ground 筛选、RNG 决策、移动
  与特殊规则，再由 `tick_new_key_cooldowns` 写 cooldown/history。
- 普通地面目标 scan 的候选遍历为 `0..MAX_OBJECTS-1`，距离为
  `abs(z delta) + abs(x delta)`，仅在 `dist < best_dist` 时替换，故同距时保留先遇到的
  最低 slot（`input_handler.cpp:1680-1706`）。
- C++ caller 没有在 `main.cpp:5509-5522` 对 AI self 的 HP 或 negative `link_state`
  进行预过滤；`prepare_ai_input` / `apply_input` 的具体分支自行决定后果。

### 4.2 组合与 direct input

`apply_input`（`input_handler.cpp:2742-3096`）的当前 source 顺序为：

1. 读取 current DAT frame/state；
2. 按 DRA/DLA/DUA/DDA/DRJ/DLJ/DUJ/DDJ/DJA 的既定顺序推进 combo wrapper；
3. 处理 `hit_a`、`hit_d`、`hit_j` 的严格 cooldown 比较；
4. 继续处理 frame 110 facing、state 301/19 lane velocity、held-heavy / crouch /
   recovery / standing / running / jumping / dash 的状态输入；
5. 最后按 resulting frame 写 `dvx/dvy/dvz` tail。

`do_frame_jump`（`input_handler.cpp:15-45`）会验证 frame、在 PP mode 按
`mp % 1000` 扣 PP、按 `(mp / 1000) * 10` 扣 HP/累积 victim combo、仅 PP-mode 成功路径
处理 negative target facing flip，并清七个 action/direction cooldown。direct trigger
即便跳帧失败，仍会将参与比较的单个 direct cooldown 清 0（`47-65`、`2878-2887`）。

### 4.3 F1/F2 step gate

C++ source 已明确：

- F2 `mode==2` 在 I00 变成当 tick 的 `mode==1, gate==1`，因此 callback 可执行；
- F1 wait（`mode==1 && gate!=1`）会跳过 post-cooldown callback，即同时跳过 human
  poll、AI prepare 和全角色 `apply_input`；
- 同一 wait tick 仍继续执行后续 T03–T17，调用 render callback；仅在 render callback
  返回之后、frame postprocess 之前 early return（`game_tick.cpp:2066-2077`）。

## 5. Unity 输入 crosswalk（只读现状）

| Unity source 坐标 | 当前职责 | 对 C++ 的关系 |
|---|---|---|
| `SimulationTickDriver.cs:84-150,461-495` | `LocalSimulationFrameInputProvider` 采集完整 held packet，计算 pressed/released；driver 在 `RunReleaseTick` 前调用 `ApplyFrameInputSet`，但实际消费仍位于 tick 内。 | Unity-native packet adaptation；不能把“写 buffer 的时点”误写成 gameplay consume 时点。 |
| `SimulationFrameInputModule.cs:42-70` | 将 `FrameInputSet.Players[].Buttons` 的 seven logical buttons 写入 each human controller 的 current-tick complete packet。 | 对应 human held-state 供给。 |
| `SimulationWorld.Passes.partial.cs:130-147` | `PostCooldownHumanInputAll` 按 runtime-slot 升序，对 roster-bound human 调用 `RunHumanInputPollPhase`。 | 对应 C++ `input.poll`。 |
| `LF2Character.cs:386-398`、`NTSDInputStateModule.cs:74-122,286-385` | 同步 runtime progress、消费 tick buffer、roll held→prev、递减 input cooldown、写 new-edge cooldown/history、同步回 runtime。 | logical key/cooldown/history mapping 静态相符。 |
| `SimulationWorld.Passes.partial.cs:232-334` | `CharacterInputAll` 在 human poll 后的独立 pass 中按 runtime-slot 升序处理所有 current character DAT。 | 对应 C++ AI prepare + `apply_input` loop，但当前时点不同。 |
| `LF2Character.cs:829-867`、`BattleEcsCharacterInputPass.cs:81-139` | AI 先 `PrepareAiInputBasic`，再 combo/direct/release action resolver，最后 frame velocity tail。 | Unity 的 optimized/data-oriented composition boundary；需以 C++ contract 单独核验，不得因其优化性质跳过。 |
| `SimulationEntityTraversal.cs:51-88` | 使用 runtime slot 从 0 到 logical capacity 的 allocation-free cursor。 | C++ fixed table 的升序行为在当前 profile 内有 static mapping；容量策略本身不是 mismatch。 |

## 6. 输入差异与待处理清单

| ID | C++ source contract | Unity current source | 状态 | 后续最小验收 |
|---|---|---|---|---|
| D-SCHED-005 | I01/I02 完整 callback（human poll + AI prepare + all `apply_input`）结束后才进入 T03 OID 7/8/51。 | `NTSDBattleTickSystem.cs:257-259` human poll 后，`282-284` 先运行 OID maintenance，`294-296` 才运行 `CharacterInputAll`。 | **待处理（静态顺序差异已确认）** | R2/R3 联合：same tick OID 7/8/51 + human/AI input + relevant frame，记录 frame、key/prev/cd、unk_338、spawn/relation、副作用排序。 |
| D-SCHED-010 | F1 wait 跳过 input/AI，但仍进入 T03–render，render 后才 early return；F2 单 tick open gate 可让 input 执行。 | `NeedClearInput` 在 cooldown/human poll/oid maintenance 后清输入，并于 `NTSDBattleTickSystem.cs:285-291` 在 CharacterInput、frame、interaction、render 前返回；source 中未找到同等的 mode=2→gate transition 或 render-after-wait path。 | **待处理（静态语义差异已确认）** | R3：分别写 F1 wait、F2 one-step、battle-entry clear 的三份 fixture；不得继续把 `NeedClearInput` 当同义 gate。 |
| D-INP-001 | C++ `apply_input` 入口未以 `link_state < 0` 直接 return；combo/direct/state paths 只在局部分支检查 `link_state == 2` / relation field。 | `LF2Character.RunCharacterInputPhase*` 和 base shell 在 `Runtime.LinkState < 0` 时整段 return。 | **待处理（静态分支差异已确认）** | R3/R5 联合：negative held/caught relation 下输入字段、frame、combo/cooldown、holder/target 的 tick-by-tick state contract。 |
| D-INP-002 | C++ caller 对 active character DAT AI 没有 self HP prefilter。 | `PrepareAiInputBasicLegacyCore` 在 `self.Runtime.HP <= 0` 时 return（`AiInput.partial.cs:1441-1442`）。 | **待处理（静态分支差异已确认）** | R3/R5：active HP=0 / respawn boundary 的 AI key/prev/history、frame与 death cleanup 顺序；不可假定“死亡时没影响”。 |
| D-INP-003 | C++ human poll 的 observable input state 是 current/prev/cd/history，按每逻辑 tick 采样。 | `FrameInputSet` 载有 `PressedButtons`/`ReleasedButtons`，但 `SimulationFrameInputModule.ApplyFrameInputSet` 仅用 `Buttons`；edges 由 `NTSDInputStateModule` 从完整 held packet重建。 | **逻辑已写 / compile+self-check PASS / runtime pending** | `R3-INP-03A` 已以 press/hold/release/same-tick multi-key fixture验证 full packet contract；metadata 不是 C++ gameplay input truth。physical asset / C++ trace 仍独立待处理。 |
| D-INP-004 | C++ callback 直接 poll P1/P2，随后对所有 active character DAT apply。 | Unity roster/provider 支持最多 8 local player slots，再按 bound runtime slot poll。 | **不适用（Unity extension），待测试** | 2-human authority fixture 下固定 P1/P2 映射；3+ human 只能作为 Unity 扩展，不可反向定义 C++ rule。 |
| D-INP-005 | C++ ground nearest target tie 按 ascending scan 的 first match，等距为低 slot。 | Unity brute path `FindNearestAiTargetSlotBrute` 也显式保持 low-slot tie（`AiInput.partial.cs:3043-3117`）；optimized indexed/SoA path仍需其独立 source/fixture核验。 | **逻辑已映射，待测试** | R3：同距 ground/air、cached target、team=5/input phase 的 fixed seed fixture；分别跑 fallback 与 optimized Unity path。 |
| D-INP-006 | C++ physical key mapping来自 `InputConfig`/SDL；本次只读源码已闭合 logical keys，不闭合实际平台 binding asset。 | Unity action-to-logical crossing在 `CharacterInputModule.cs:63-76,308-325`；具体 action asset / Inspector binding 未在本 Work Package 审计。 | **UNKNOWN（非脚本绑定）** | 用户/后续 Play Mode：确认 W/S/A/D/J/K/L 到 logical keys 的最终绑定；不应由 source 阅读或旧 self-check 宣称完成。 |

## 7. R3 输入子流程的验收合同与 Change ID 边界

R1 不修改代码。R3 在用户确认后应拆为至少下列独立 Change ID，而不是一次性改 input：

1. **R3-INP-01 — callback/pass boundary**：先处理 D-SCHED-005；允许改动仅限
   `NTSDBattleTickSystem` / input-pass adapter 的顺序与无分配分段，不允许触碰
   CentralOnly、renderer、slot profile、pool 或 worker。
2. **R3-INP-02 — F1/F2 与 battle-entry clear 分离**：处理 D-SCHED-010；必须保留
   `NeedClearInput` 的 bootstrap 清输入职责，不得将其粗暴重命名成 F1。
3. **R3-HOLD-INP-01 — held/caught input gate**：处理 D-INP-001，依赖 R1-SOURCE-005 的
   link/held contract。
4. **R3-AI-LIFE-01 — dead/respawn AI gate**：处理 D-INP-002，依赖 R1-SOURCE-003/005。
5. **R3-INP-03A — canonical full-held packet semantics**：只关闭 D-INP-003；
   `D-INP-004`、`005`、`006` 按 D-011 分别进入容量、AI target、physical binding 包，且不改变
   Unity local input UX。

每个 Change ID 都必须在修改脚本之前建立 `docs/ai/CHANGE-RECORDS/<ID>.md`，并在
交付前运行 `Tools/Validate-ChangeLedger.ps1`。当前没有创建这些 Change ID，也没有
授权任何脚本改动。
