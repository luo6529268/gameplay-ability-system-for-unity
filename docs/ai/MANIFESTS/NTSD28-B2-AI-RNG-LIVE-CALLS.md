# NTSD 2.8-Logan AI synchronized RNG live-call manifest

> Change ID：`NTSD28-B2-AI-RNG-CALLSITE-CROSSWALK-001`  
> Authority：`source/ntsd28_core/src/simulation/native_ai.cpp`  
> Unity canonical：`AiDecisionKernel` + `AiCharacterDecisionModule`  
> 日期：2026-09-03

## 1. 计数合同

- `native_ai.cpp` 文本共有 42 个 `synchronized_next(...)` 表达式。
- 从 `NativeAi28::step_main` 正式调用闭包排除 4 个不可达/非本域表达式后，live closure 为
  **38 个表达式**：
  - `0x0E`：`step_non_character_hit_fa`，属于 non-character first-pass，不是 AI `step_main`；
  - `0x13`：`select_local_character_target` 内版本，该 helper 未被 `step_main` 调用；
  - `0x14/0x15`：`respond_to_state3000_target` 内版本，该 helper 未被 `step_main` 调用。
- 两个 ordinary spacing 表达式分别动态选择 `0x1A/0x1C` 与 `0x1B/0x1D`，所以 38 个
  live 表达式覆盖 **40 个可能 call-site ID**。
- Unity 当前 `DataOrientedCanonical` 可达代码面共有 **107 个 `.Rand(...)` 表达式**：
  `AiDecisionKernel=61`、`AiCharacterDecisionModule=46`。最多 38 个能与 authority live expression
  建立结构候选，其余 **69 个不得分配 native ID**：
  - 旧 character-profile 分支 66 个表达式中只有 1 个可作为 `0x6C` 候选，剩余 65 个无 live ID；
  - `ProcessSubCallerPrewrite` + `ProcessSubPressurePrewrite` 共 6 个表达式，而 authority low-HP
    continuation 只有 `0x26/0x27` 两个，剩余 4 个无 live ID。

早期 `83 expressions / 80 synchronized IDs / direct CRT 2` 仍是全 production source 的静态库存；
它没有被撤销，但不能替代本表的 AI `step_main` live-closure 口径。

## 2. live 顺序与 Unity 对应

状态口径：`局部同构`只表示当前表达式的 bound/局部条件有直接候选，不表示整条 AI 已对齐；
`行为错位`表示消费点附近的动作、条件或顺序不同；`需合并`表示 Unity 当前以多个表达式表示
authority 单一 site；`缺字段`表示无法用现有 snapshot 表达 authority 前置条件。

Unity legacy AI 的三个字段名与 native 按钮语义交叉，判断动作时必须先应用现有、已测试的桥接：

| Native 语义 | Unity legacy 字段 | exact input index |
|---|---|---:|
| attack | `KeyJump` / `PrevJump` | 4 |
| jump | `KeyDefend` / `PrevDefend` | 5 |
| defend | `KeyAttack` / `PrevAttack` | 6 |

因此下表按桥接后的 native 语义判断，不能按 `Key*` 名称字面判断。

| Authority 顺序 | ID | bound / 条件消费 | Unity 当前候选 | 状态与差异 |
|---:|---|---|---|---|
| 1 | `0x11` | scripted target 在左且距离大于250；`scaled_61c+3` | `MoveTowardCoordinate` line 846 | 局部同构；仍受 producer/sample 顺序迁移约束。 |
| 2 | `0x12` | scripted target 在右且距离大于250；`scaled_61c+3` | `MoveTowardCoordinate` line 854 | 局部同构；仍受 producer/sample 顺序迁移约束。 |
| 3 | `0x13` | cached entity active 后先消费30，再检查 definition type0 | `TryEvaluateCore` line 243 | 行为错位；Unity `IsLivingCharacter` 在消费前执行且谓词更强，会改变是否消费。 |
| 4 | `0x14` | 任意已选目标均先消费；`scaled_618+8`；四个 native force-attack 字段命中才 attack | `TryEvaluateCore` line 361 | 缺字段；Unity `KeyJump`正确对应native attack，但错误使用boundary flags替代四个force-attack字段。 |
| 5 | `0x15` | 仅 state3000 且 subject state!=7；`scaled_61c`；逼近时 defend | `PreUpdateTarget3000` line 937 | 消费顺序错位；`KeyAttack`正确对应native defend，但Unity在检查self state7前求值RNG，authority state7不消费。nonpositive bound均不推进。 |
| 6 | `0x16` | pickup state1000/2004，目标在左远距；`scaled_61c+3` | `MoveTowardTarget` line 888 | 行为错位；Unity special-target 状态入口包含1004而非 authority 1000。 |
| 7 | `0x17` | pickup state1000/2004，目标在右远距；`scaled_61c+3`，direction lock 后判断 | `MoveTowardTarget` line 898 | 行为错位；状态入口和 direction-lock 表达方式未闭合。 |
| 8 | `0x18` | abnormal target 在右；`scaled_610+35` | `TryEvaluateCore` line 447 | 局部同构；需随 abnormal target/stage context 联合验证。 |
| 9 | `0x19` | abnormal target 在左/同位；`scaled_610+35` | `TryEvaluateCore` line 453 | 局部同构；需随 abnormal target/stage context 联合验证。 |
| 10 | `0x3C` | ordinary combat 首个 site；`scaled_618+1`；正值立即退出 special-profile | `TryEvaluateCore` line 489 | bound 候选存在但行为错位；Unity 零值后进入旧 66-expression character-profile 树。 |
| 11 | `0x6C` | 仅 `use_ai==33` 或原 oid33、目标有效且`0x3C==0`；bound5 | `AiCharacterDecisionModule.TryUpdateOid33Or19Or16PredictedDua` line 959 | 行为错位；Unity 按 oid 33/19/16 分组且位于旧策略树深处，没有 `use_ai` carrier。combo index2 应按 router 解释为 `hit_Ua`，不得采信 authority 注释中的 `hit_aj` 误标。 |
| 12 | `0x1A` / `0x1C` | ordinary target-right；low-HP spacing 选`1A`，否则`1C`；`scaled_610+35` | `TryEvaluateCore` line 607 | 行为错位；Unity `widePath` 还被 oid18/5/31 强制开启，不等同 low-HP spacing。 |
| 13 | `0x1B` / `0x1D` | ordinary target-left；low-HP spacing 选`1B`，否则`1D`；`scaled_610+35` | `TryEvaluateCore` line 617 | 行为错位；同上，动态 ID 判别不能直接复用 `widePath`。 |
| 14 | `0x28` | held branch 首个 gate；`scaled_61c+1`，正值停止 outer AI | `ProcessHeld` line 1227 | 局部同构。 |
| 15 | `0x29` | subject state2；`scaled_61c+5`；遮挡则 jump，否则 attack | `ProcessHeld` line 1234 | 局部同构；Unity `KeyDefend/KeyJump`分别正确映射native jump/attack。 |
| 16 | `0x2A` | ordinary weapon；predicted abs X<115、dz<6；`scaled_61c+3`；attack | `ProcessHeld` line 1247 | 阈值错位；Unity X 阈值为10000；`KeyJump`动作语义正确。 |
| 17 | `0x2B` | held oid124；`scaled_614+30`；attack | `ProcessHeld` line 1250 | 局部同构；`KeyJump`正确映射native attack。 |
| 18 | `0x2C` | ordinary weapon；`scaled_61c+5`；命中后才检查behavior range并移动 | `ProcessHeld` line 1252 | 局部候选；history/move-mode 条件与 authority flags 仍需改。 |
| 19 | `0x2D` | held oid150/151、无遮挡、predicted abs X<300、dz<6；`scaled_618+7`；attack | `ProcessHeld` line 1278 | 阈值错位；Unity X 阈值5000；`KeyJump`动作语义正确。 |
| 20 | `0x2E` | weapon-run 左边界右移；`scaled_61c+7` | `ProcessHeld` line 1312 | 局部同构。 |
| 21 | `0x2F` | 紧随`0x2E`；`scaled_61c+5`；state2 时 jump | `ProcessHeld` line 1314 | 局部同构；`KeyDefend`正确映射native jump。 |
| 22 | `0x30` | weapon-run 右边界左移；`scaled_61c+7` | `ProcessHeld` line 1321 | 局部同构。 |
| 23 | `0x31` | 紧随`0x30`；`scaled_61c+5`；state2 时 jump | `ProcessHeld` line 1323 | 局部同构；`KeyDefend`正确映射native jump。 |
| 24 | `0x32` | weapon-run 近距目标在右，反向左移；`scaled_61c+4` | `ProcessHeld` line 1333 | 局部同构。 |
| 25 | `0x33` | weapon-run 近距目标在左/同位，反向右移；`scaled_61c+4` | `ProcessHeld` line 1339 | 局部同构。 |
| 26 | `0x34` | weapon-run 远距且非state2；bound5，非零停止 | `ProcessHeld` line 1352 | 局部同构。 |
| 27 | `0x35` | 仅local_flag30==0、oid2/34、MP>150 且`0x34==0`；`scaled_61c+3` | `ProcessHeld` line 1357 | combo分支错位；直接分支的`KeyJump`正确映射native attack，但非直接分支Unity置旧Drj/Dlj，authority置combo index4/5即hit_Da/Dj。 |
| 28 | `0x1E` | 每个 ordinary target 均先消费；`scaled_61c*7+10`；朝向state3/3xx时 defend | `TryEvaluateCore` line 641 | 局部同构；`KeyAttack`正确映射native defend。 |
| 29 | `0x1F` | 仅 behavior-range；`(scaled_618+10)*2`，结果<3才继续 | `TryEvaluateCore` line 648 | bound与短路候选同构。 |
| 30 | `0x20` | 仅`0x1F<3`后消费；bound20，结果<3且target state!=14时 jump | `TryEvaluateCore` line 649 | 局部同构；`KeyDefend`正确映射native jump。 |
| 31 | `0x21` | predicted abs X<80、dz<5；`scaled_61c+3`；attack | `TryEvaluateCore` line 656 | 条件错位；Unity阈值50并附加oid gate；`KeyJump`动作语义正确。 |
| 32 | `0x26` | 单一 low-HP continuation；target delta X<100、dz<80；`scaled_61c+2` | `ProcessSubCallerPrewrite` line 1423 / `ProcessSubPressurePrewrite` line 1471 | 需合并；Unity 两份候选且前置条件拆散，不能各自消费同一 native ID。 |
| 33 | `0x27` | 仅`0x26==0`、subject state!=7 且 behavior-range；bound17；jump | `ProcessSubCallerPrewrite` line 1441 / `ProcessSubPressurePrewrite` line 1479 | 需合并；必须保留单一短路消费。两函数中的另两个 `scaled_61c+3` 表达式没有 live ID。 |
| 34 | `0x37` | profiled combat 入口先做 generic close attack；predicted abs X<80、dz<5；`scaled_61c+3`；attack | `ProcessSubHelper` line 1523 | 表达式/动作局部同构；`KeyJump`正确映射native attack，但Unity在此之前已有额外prewrite RNG。 |
| 35 | `0x38` | direction gate 通过后必消费；`scaled_61c+1`，非零退出 profile tail | `ProcessSubHelper` line 1529 | bound/返回形状局部同构。 |
| 36 | `0x39` | special profile family 且100<predicted abs X<900、dz<5；`scaled_61c+10`；defend | `ProcessSubHelper` line 1536 | profile判别错位；`KeyAttack`正确映射native defend，但Unity只按oid group且缺`use_ai` carrier。 |
| 37 | `0x3A` | special-family chase 中仅 `use_ai==34`/oid34 才消费bound2；零值 jump，否则 attack | `ProcessSubHelper` line 1556 | 动作分支同构；Unity零值`KeyDefend`=native jump、其他`KeyJump`=native attack，但缺`use_ai` carrier。 |
| 38 | `0x3B` | `use_ai==1`/oid1 且100<predicted abs X<300、dz<5；`scaled_618+10`；defend | `ProcessSubHelper` line 1565 | profile判别错位；`KeyAttack`正确映射native defend，但缺`use_ai` carrier。 |

## 3. 必须保留的短路顺序

1. `0x13` 只在 cached entity active 后消费，但在 type0 discriminator 之前消费。
2. `0x14` 位于 threat/object scan 之后、所有 state-specific early return 之前。
3. ordinary combat 内先 `0x3C`，可选 `0x6C`，再 movement、held、`0x1E`、`0x1F→0x20`、
   `0x21`、`0x26→0x27`、`0x37`、direction gate、`0x38`、条件 `0x39/0x3A/0x3B`。
4. nonpositive bound 返回0且不推进 counter/index/calls；这对 difficulty 0 下的 `0x15` 尤其重要。
5. shadow、legacy comparison 与 diagnostics 不得提交 synchronized cursor；每个正式 AI producer 只能由
   最终被采纳的 authoritative evaluation 提交一次。

## 4. 实施边界

- 不得给 69 个 surplus `.Rand` 表达式分配伪造 ID。
- `0x14` 所需四个 force-attack 字段以及 `0x3A/0x3B/0x6C` 所需 `use_ai`/profile carrier
  必须先有明确 runtime/snapshot 来源；不能用 boundary flags 或 oid group 静默替代。
- legacy `Key*`字段的字面名不是native动作，必须经三按钮桥接后判断；当前主要差异是消费时点、
  threshold、缺失carrier、combo index与profile tree，不能由RNG adapter掩盖。
- production 接入应拆为：call-site-aware stream seam；native prefix/target/ordinary core；held sites；
  profile sites与缺失carrier；authoritative-only commit；最后同 seed/input/tick joint trace。

## 5. 当前实施进度

- production capture/commit：`NTSD28-B2-AI-SYNC-RNG-PRODUCTION-COMMIT-001`已通过focused；
  `IndexedCanonical`每AI从`world.NativeRandom`捕获cursor，只有accepted writer在input/flow写入前
  唯一提交。same-generation stale origin被拒绝，Full oracle/DeepShadow/SharedShadow/fallback不提交，
  accepted sync path不改legacy `world.Rng`。call-site/bound/raw/value/order/count均进入严格比较。
- `0x28..0x35`：`NTSD28-B2-AI-HELD-RNG-001`已通过focused；14 expressions、原生返回/
  early-stop、line-cover、threshold、state17与combo index4/5均有candidate断言。
- `0x1A/0x1C`、`0x1B/0x1D`、`0x1E..0x21`、`0x26/0x27`、`0x37..0x3B`：
  `NTSD28-B2-AI-ORDINARY-NONHELD-RNG-001`已通过focused；静态`battle_mode`与cadence
  `InputPhase`已分离，正式playable的global direction lock按默认0实现。full non-held candidate
  到达Complete；同步路径不再进入两个旧prewrite/旧profile helper，全部69个surplus已隔离。
- `0x11/0x12`：`NTSD28-B2-AI-SCRIPTED-RNG-TRANSACTION-001`已通过candidate transaction focused。
- `0x13/0x14/0x15/0x18/0x19`：`NTSD28-B2-AI-TARGET-PREFIX-RNG-001`已通过candidate
  condition/order focused。
- `0x16/0x17`：`NTSD28-B2-AI-PICKUP-RNG-001`已通过native1000/2004 candidate gate与site focused；
  legacy1004/2004入口保持。
- `0x3C/0x6C`：`NTSD28-B2-AI-SPECIAL-PROFILE-RNG-001`已通过；synchronized candidate
  不再进入旧profile树，65个无live ID表达式已隔离。
- 全部38 live expressions / 40 possible IDs现已具备synchronized candidate实现，69个surplus已隔离，
  production snapshot/accepted-only commit也已接入。仍不能声明B2或完整AI已对齐：native input
  producer/action迁移及同seed/input/tick联合trace尚未完成。
