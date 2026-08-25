# R3-AI-TGT-01 — fallback / indexed AI target-selection contract

> 建立日期：2026-08-22  
> 状态：`RUNTIME_PENDING`（source/static、Unity scripts compile和full self-check均已通过；真实 AI Play Mode
> 与 C++ trace 保持未关闭。）  
> 顶层目标：`Assets/NTSD/Docs/cpp-release-vs-unity-battle-realignment-plan.md` 的 R3。  
> 唯一行为 authority：`J:\QQFile\NTSD2.4\ntsd_release` 的 release live runtime。  
> 执行方式：按 D-009 连续推进；若进入脚本改动，必须仅使用已建立的 `R3-AI-TGT-001` Change Record。

## Goal

只闭合 `D-INP-005` 的**代码级最小验收**：在相同 Unity fixture / seed 下，验证
`LegacyCanonical` fallback 与 `DataOrientedCanonical` indexed path 对 AI 基础目标选择的可观察结果一致，
并且 fixture 显式覆盖 C++ source 已确认的：

1. ground scan 按 runtime slot 升序、仅在 `dist < bestDist` 时替换，故同距保留低 slot；
2. self 不是 state 9 时，air scan 独立按同一规则选择，并覆盖 `selected`，但 `sameZLane` 仍来自 ground
   selected；
3. `unk_360` 指向存活 character-DAT 时，调用一次 `% 30`；结果大于 0 时保留缓存目标，等于 0 时回写
   当前新选择；缓存无效时直接回写当前新选择；
4. `team_candidate_allowed` 在 phase 1、team 5 与非 phase 1 下的 target eligibility。

夹具若在 entity 注册后设定 `Runtime.Unk360`，必须先经 `CharacterInputWriter.CommitAiDecisionState`
同步到 slot/generation-owned canonical input store，再显式写入同值的 compatibility runtime mirror；直接只写
mirror 不是 data-oriented profile 的合法 initial-state setup，而只写 `CharacterInputWriter` 也不会补 runtime
mirror（正式 indexed decision 的 `BattleAiInputWriter` 才负责该补写）。

这不是 C++ executable trace，也不把两个 Unity profile 的相等性提升为 C++ runtime 已对齐。

## Scope

允许仅：

1. 只读核对 C++ `InputHandler::prepare_ai_input` 与 Unity fallback / indexed / decision path；
2. 只在 `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs` 添加一个 test-only fixture 及必要的私有 helper；
3. 使用固定 Unity seed 分别运行 `BattleAiExecutionProfile.LegacyCanonical` 与
   `BattleAiExecutionProfile.DataOrientedCanonical`，比较 selected/cache、AI input observable state、RNG state
   和 RNG call count；
4. 运行 static、ledger、Unity scripts compile 与完整 self-check，并如实记录结果。

禁止改动：`SimulationWorld.AiInput.partial.cs`、`AiSensingKernel.cs`、`AiDecisionKernel.cs`、AI policy 默认值、
profile default、spatial/broadphase、worker/ECS layout、physical input、scene/DAT/render、pool/capacity、C++ source /
build / executable / config。

## Authority / Evidence

- **C++ source — VERIFIED**：`src/input/input_handler.cpp:1667-1675` 定义 team eligibility；
  `1680-1706` 为 ascending ground scan 和 strict `<` 替换；`1708-1731` 为 ground `same_z_lane` 后的
  air override；`1733-1749` 为 `unk_360` cached target 与一次 `%30` 分支。该文件由 release Makefile 的
  input module 编译链覆盖，且 `game_tick(...)` 的 active character input caller 已在 R1 source inventory
  中定位。
- **Unity source — VERIFIED**：`SimulationWorld.AiInput.partial.cs:3045-3245,4925-4946` 存在 fallback /
  SoA nearest scan、low-slot tie、air override 与 team predicate；`1427-1579` 有 legacy cached target branch；
  `AiSensingKernel.cs:68-110,488-520` 与 `AiDecisionKernel.cs:243-318` 有 indexed counterpart。
- **现有证据 — VERIFIED but insufficient**：`BattleRuntimeSelfCheck` 已分散覆盖 spatial tie、phase1 target
  index 与一般 cache determinism；尚无一个完整 profile-pair fixture 同时覆盖 cache roll、input observable
  state、RNG state/call count。
- **首次夹具执行 — INFERRED fixture-precondition issue**：初版在注册后只写 `Runtime.Unk360=7`，导致
  fallback 读取 runtime mirror 而 indexed canonical 读取 bind 时捕获的 input-store row，产生
  `legacyTarget=7 / indexedTarget=4`。`BattleCharacterInputStore.Bind` 只在 bind 时 `Capture(runtime)`，
  且 `TryEvaluateCanonicalDecision` 读取 store row；生产 source 检索未发现本包外的 post-bind direct
  `Runtime.Unk360` writer。该差异先归类为 fixture precondition，允许本包改用 canonical writer 后重跑，
  不得据此修改 production AI。
- **第二次夹具执行 — VERIFIED writer/mirror split**：改用 `CharacterInputWriter.CommitAiDecisionState` 后，
  store row 已更新但 `Runtime.Unk360` 仍保持默认；`BattleCharacterInputWriter.CommitAiDecisionState` 的
  `CommitFullRuntimeMirror` 只镜像 key/prev/cooldown/combo，正式 `BattleAiInputWriter` 在其后才写
  `runtime.Unk360 = input.Unk360`。因此测试初始化必须先写 canonical store、再显式同步 runtime mirror，仍不
  需要改 production AI。
- **C++ runtime trace — BLOCKED**：R1-WP02 尚未获得不修改 C++ Release 的安全观察方式；不得运行任何 C++
  executable 以试图补证。

## Planned fixture contract

| 子情形 | 初始状态 | 必须断言 | 不得外推 |
|---|---|---|---|
| Equal ground / air | self slot0；同距 ground/air 候选置于不同低/高 slot | ground、air 都按低 slot；air 覆盖 selected；`sameZLane` 仍对应 ground | special-object scan、combo 行为 |
| Cache retain | `unk_360` 为存活 character-DAT；固定 Unity seed 的 cache roll >0 | 最终 target 保留 cache，legacy/indexed input signature 和 RNG state/call count一致 | C++ RNG implementation / exact C++ seed |
| Cache refresh | 同一 fixture但 cache roll =0 或缓存无效 | `unk_360` 回写 nearest selected，两个 Unity profile一致 | 后续 special scan 改写 target |
| Team / input phase | phase1 team5、phase1 非team5、phase2/4 foreign target | target eligibility与 C++ predicate对应，并且 fallback/indexed一致 | physical InputAction binding |

## Acceptance

| 层级 | 最小验收 | 初始状态 |
|---|---|---|
| S0 source | 上述 C++ / Unity branch 顺序和字段语义已定位。 | `PASS` |
| S1 fixture | profile-pair 覆盖 tie、air override、cache retain/refresh 和 team/phase selection。 | `PASS` |
| S2 observable parity | selected/cache、input observable state、RNG state / call count均相等。 | `PASS` |
| S3 compile/self-check | ledger、diff、Unity compile、`error CS`、full self-check。 | `PASS` |
| S4 Play Mode | 真实 AI scenario / target visual movement。 | `RUNTIME_PENDING`，不在本包执行 |
| S5 C++ trace | same fixture input trace。 | `BLOCKED / R1-WP02` |

## 实际验证结果（2026-08-22）

- **source/static**：C++ `prepare_ai_input` target/cached predicate、Unity fallback/indexed/canonical-store
  crosswalk均通过标记检查；`STATIC_TARGET_AND_CANONICAL_STORE_GUARD=PASS`。
- **first-difference纪律**：初版先后只设置 Runtime mirror、只设置 canonical store，分别在 01:45:50 与
  01:51:01 失败。两次均记录了实际输出和 writer/mirror source原因；没有修改 production AI。
- **合法 fixture initial state**：最终以 `CharacterInputWriter.CommitAiDecisionState` 写入 canonical row，
  并同步同值 `Runtime.Unk360` compatibility mirror。这样不改变运行时 policy，只让两条 profile收到同一
  initial state。
- **Unity compile**：existing Editor 的 UnityMCP `refresh_unity(force/scripts/compile)` 成功完成 domain
  reload，随后 filtered `error CS` 为 0。
- **full self-check**：`NTSD/验证/运行战斗运行时自检` 的结果文件
  `Temp/NTSD_BattleRuntimeSelfCheck.result` 于 **2026-08-22 01:53:46 +08:00** 写入 `PASS`。
- **ledger/diff**：`Tools/Validate-ChangeLedger.ps1` PASS（10 records / 10 governed code files）；
  `git diff --check` exit 0（仅既有 LF/CRLF warning）。

上述 PASS 只证明本固定 Unity fixture的 fallback/data-oriented observable parity；不证明 C++ executable
trace、真实场景 AI、技能表现或所有 target topology 已完成对齐。

## Stop conditions

停止并建立新 Record，若：

- profile-pair fixture 暴露 production fallback / indexed 行为不一致，且修复需要修改任一 production AI 文件；
- 需要改变 cache RNG call order、input phase policy、spatial index、worker / ECS policy 或 default profile；
- 目标差异只可由 C++ executable trace、physical binding 或 scene Play Mode 判定；
- fixture 被 special-object scan、combo、held/link、collision 或 lifecycle writer 污染而无法保持此最小边界。

## Out of scope

AI special-object scan、move-mode、combo decision、dead/respawn caller（`R3-AI-LIFE-01`）、physical binding
（`R3-PHY-01`）、所有 D-MOV、R4～R8、R1-WP02、T8 default `stage.dat`、服务器与 Android。
