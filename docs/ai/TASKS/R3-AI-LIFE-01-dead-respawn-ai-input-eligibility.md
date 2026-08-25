# R3-AI-LIFE-01 — dead / respawn-window AI input eligibility

> 建立日期：2026-08-22  
> 状态：`RUNTIME_PENDING`（source、static、Unity scripts compile、dual-profile focused self-check
> 均已通过；仍缺 joint lifecycle / Play Mode / C++ runtime trace。）  
> 顶层目标：`Assets/NTSD/Docs/cpp-release-vs-unity-battle-realignment-plan.md` 的 R3。  
> 唯一行为 authority：`J:\QQFile\NTSD2.4\ntsd_release` 的 release live runtime。  
> 执行方式：按 `D-009` 连续推进；脚本修改前已建立独立 `R3-AI-LIFE-001` Change Record。

## Goal

只关闭 `D-INP-002` 的 AI self-HP input eligibility 差异：active、current DAT type 为 character 的
AI 自身 `HP <= 0` 时，Unity 不能在 post-cooldown CharacterInput entry 直接跳过 C++ 已执行的
`prepare_ai_input → apply_input` 链。

本包只恢复该 tick 的 AI input roll / clear / no-target input-field lifecycle 与 normal action resolver
调用资格。它不改变 death、respawn、hit-stop、frame advance、AI target policy 或 dead character 的最终
可视表现。

## Scope

允许仅做以下最小变动：

1. `SimulationWorld.AiInput.partial.cs` 的 legacy AI core 保留 null guard，移除 `self.Runtime.HP <= 0`
   overall return；
2. `Simulation/Ai/AiDecisionKernel.cs` 的 indexed/data-oriented kernel 移除 `rows.Hp[self] <= 0` 的
   `InvalidSelf` early exit，使它与 C++ caller 同样进入 existing coordinate/no-target/target decision
   pipeline；
3. `Simulation/Ai/AiSensingKernel.cs` 的 `TryFindNearestCore` 保留 snapshot/self-slot/coordinate
   validity gate，但移除 `rows.Hp[selfSlot] <= 0` 的 self eligibility reject；否则 indexed kernel
   即使已进入，也会在到达既有 `selected < 0` no-target branch 前错误返回 unavailable；
4. `BattleRuntimeSelfCheck` 建立无目标、self HP=0 的 AI fixture，并分别验证
   `LegacyCanonical` 与 `DataOrientedCanonical` profile 都执行 roll/clear 而不是保持 stale current key；
5. 更新 Change Record、ledger、STATE、差异登记、主计划和 handoff。

禁止：

- 改动 AI target scan、RNG policy、candidate/LoosQuadtree、input packet、physical input binding、AI target
  optimization 或 input action resolver；
- 改动 `PostFrameAdvanceDeathCleanupAll`、`PassesRespawnGate`、respawn fields/effect、state14/9998、frame
  advance、held/CPoint/link、collision/hit、opoint 或 render；
- 删除 target HP、hit/respawn、result 等其它 domain-specific `HP <= 0` gate；
- 改动 CentralOnly、Texture2DArray、dynamic Mesh、URP、1.5× scale、fixed-world camera、capacity、30 Hz、
  FrameInputSet、SoA/ECS、pool、worker、0-GC、scene、DAT 或 C++。

## Authority / Evidence

### C++ release live source — VERIFIED(source)

- `src/core/main.cpp:5505-5522`：`world.game_tick > 1` 时对 every active current `obj_type == 0`
  character DAT 调用 AI `prepare_ai_input`，随后调用 `apply_input`；caller 不按 self HP 过滤；
- `src/input/input_handler.cpp:1615-2353`：`prepare_ai_input` 没有 `e.hp <= 0` function-level return；
  target scanning 会过滤 dead **candidate**，但不是 self；
- 同文件 `1728-1760`：no selected target 时仍 `roll_and_clear_keys()`、执行 no-target fallback、
  `tick_new_key_cooldowns()` 并 return；
- `src/entity/game_tick.cpp:1249-1421`：input callback 先于 frame advance、state9998 cleanup 和
  state14/HP/hit-stop respawn gate。因此该 HP=0 input call 的字段写入属于 death/respawn cleanup 前的
  tick behavior。

### Unity current source — VERIFIED(source)

- `SimulationWorld.AiInput.partial.cs:1427-1442`：legacy core 将 `self.Runtime.HP <= 0` 与 null 合并为
  overall return；
- `Simulation/Ai/AiDecisionKernel.cs`：data-oriented indexed kernel 原本有 self-HP `InvalidSelf`
  early exit，移除后可进入 decision pipeline；
- `Simulation/Ai/AiSensingKernel.cs:74-88`：当前 `TryFindNearestCore` 仍将
  `rows.Hp[selfSlot] <= 0` 作为 self reject；首次 dual-profile fixture 已实际证明它会阻止
  `selected < 0` 的既有 no-target branch，并在 UnifiedAuthority 后触发 forbidden fallback；
- `SimulationWorld.Passes.partial.cs:232-329`：CharacterInputAll 在 frame advance / death cleanup 前按
  slot 遍历 current character DAT；
- `SimulationWorld.AiSoaShadow.partial.cs:128-174`：production profile can select
  `DataOrientedCanonical`，因此只改 legacy core 会留下 production path split；
- `SimulationWorld.Passes.partial.cs:744-845` 的 respawn writer 已有独立 source contract，明确不在本包。

### Evidence limits

- `R1-WP02` C++ full trace 仍 BLOCKED；本包不能把 source/fixture 结果写为 C++ runtime equality；
- source 证明 self HP 不是 caller gate，但每个 OID/state 的 full AI decision / RNG behavior 仍由后续
  AI / lifecycle joint fixture 验收；
- focused fixture 故意选择 no-target 情形，以验证 C++ 明确的 roll/clear path，避免把 target-selection
  和 death/respawn policy 混入本包。

## Proposed minimal implementation contract

```text
C++ active character-DAT AI, self HP=0:
  caller → prepare_ai_input → [no target: roll/clear + no-target fallback + edge write]
         → apply_input → later frame/death/respawn passes

Unity target:
  legacy core / indexed kernel both enter the same existing decision pipeline
  → no-target roll/clear result reaches current input store/runtime
  → existing CharacterInput action resolver runs
  → later frame/death/respawn passes remain unchanged
```

Do not substitute a dead-AI global no-op with `InvalidSelf`; do not make any claim about a fully live target or
respawn visual sequence without the later joint fixtures.

## Files likely involved

| 类别 | 文件 | 允许职责 |
|---|---|---|
| legacy AI | `Assets/NTSD/Scripts/Simulation/SimulationWorld.AiInput.partial.cs` | 只移除 self-HP overall return。 |
| optimized AI | `Assets/NTSD/Scripts/Simulation/Ai/AiDecisionKernel.cs` | 只移除 `InvalidSelf` self-HP early exit。 |
| indexed sensing | `Assets/NTSD/Scripts/Simulation/Ai/AiSensingKernel.cs` | 只移除 self HP eligibility reject，保留其他 snapshot / coordinate validity gate。 |
| focused test | `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs` | 仅添加 dual-profile no-target dead-AI input fixture。 |
| records | `docs/ai/...` | 按 D-008 留痕。 |

## Unknowns

1. self HP=0 with a live target, held/caught relation or state-specific OID behavior has no C++ runtime trace;
2. respawn same-tick versus next-tick scene visibility is R1-SOURCE-003/005 / R5 work, not input eligibility;
3. physical AI configuration, 1000 AI performance and C++ trace remain outside this task.

## Deliverables

1. `R3-AI-LIFE-001` Change Record，先于脚本修改建立；
2. 双 profile 最小 patch 和 no-target focused fixture；
3. Ledger / STATE / diff register / plan / handoff 更新；
4. static、Unity compile、filtered `error CS` 和 BattleRuntimeSelfCheck 的实际结果。

## Verification

| 层级 | 最小验收 | 初始状态 |
|---|---|---|
| S0 source | C++ self-HP caller / no-target path 与 Unity three early eligibility gates 闭合。 | `PASS` |
| S1 legacy fixture | HP=0 no-target AI rolls previous key and clears current key; not global skip. | `PASS` |
| S2 data-oriented fixture | 同一 no-target contract 在 IndexedCanonical / unified profile 也成立。 | `PASS` |
| S3 static | 仅三个 specified self-HP input gates 被移除；respawn、target HP 和 snapshot/coordinate gates retained. | `PASS` |
| S4 compile / self-check | Editor scripts compile、filtered `error CS`、BattleRuntimeSelfCheck request。 | `PASS` |
| S5 joint / Play Mode | dead → respawn input/lifecycle / visible behavior。 | `PENDING / R3-R5` |
| S6 C++ trace | same fixture trace。 | `BLOCKED (R1-WP02)` |

## Stop conditions

立即停止并记录 blocker，若：

1. 必须修改 target selection、AI RNG ordering、death/respawn writer、frame advance、held/CPoint/link 或
   input resolver 才能让 focused no-target fixture成立；
2. kernel 在 self HP=0 no-target branch 出现 C++ source 未能闭合的 policy / arithmetic contract；
3. 需要修改 C++ source/build/executable/config，或运行 C++ executable；
4. fixture指向 performance architecture、physical binding 或 scene/DAT 依赖。

## Out of scope

`D-INP-003`～`006`、所有 `D-MOV-*`、`D-SCHED-004`、所有 `D-LINK-*`、`D-HOLD-*`、`D-CPT-*`、
`D-OP-*`、R4～R8、R1-WP02 trace、T8 default `stage.dat`、服务器与 Android 验收。

## 实际验证结果（2026-08-22）

- 首次 `DataOrientedCanonical` fixture 没有被忽略：它实际揭示 `AiSensingKernel.TryFindNearestCore`
  的第三个 self-HP reject，导致已发布 unified snapshot 后走 forbidden fallback；该现象在**写入前**
  已扩展到本 Contract / Change Record，而没有放宽 fallback 防护；
- 三处 self eligibility gate 移除后，legacy 与 data-oriented fixture 都验证 `PrevJump: 0 → 1`、
  `KeyJump: 1 → 0`，且 `CdAttack=5`、`Frame=0` 未被这个 no-target fixture 擅自改写；
- `Tools/Validate-ChangeLedger.ps1`：PASS（7 records、10 governed code files）；
- `git diff --check`：exit 0（仅现有 LF/CRLF warning）；
- 现有 Unity Editor 的 UnityMCP force scripts refresh / compile：成功，filtered `error CS` 为 0；
- `NTSD/验证/运行战斗运行时自检`：`Temp/NTSD_BattleRuntimeSelfCheck.result = PASS`，
  文件最后写入为 `2026-08-22 01:02:51 +08:00`。

这些结果只证明 source contract 的最小 Unity adapter 和 editor fixture 可运行；它们不证明 dead→respawn
完整 scene lifecycle、target-bearing dead AI、RNG/target policy、C++ executable runtime 或完整战斗已对齐。
