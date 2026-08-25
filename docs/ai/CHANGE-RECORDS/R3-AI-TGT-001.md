# R3-AI-TGT-001 — AI fallback / indexed target-selection fixture

<!-- CHANGE-RECORD
id: R3-AI-TGT-001
status: RUNTIME_PENDING
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: J:\QQFile\NTSD2.4\ntsd_release\src\input\input_handler.cpp release live prepare_ai_input target-selection path
evidence: SOURCE-CONTRACT-VERIFIED / STATIC-PASS / UNITY-COMPILE-PASS / FOCUSED-SELF-CHECK-PASS / RUNTIME-PENDING
-->

> 创建日期：2026-08-22  
> 最后更新：2026-08-22  
> 类型：battle / input / test  
> 所属 Work Package：`R3-AI-TGT-01`

## 1. 状态与范围

- 当前状态：`RUNTIME_PENDING`；代码级 source/static、compile和full self-check已通过，但真实 AI Play Mode
  与 C++ trace保持未关闭。
- 目标：只添加一个 profile-pair self-check，验证 fallback / indexed 对 `D-INP-005` target selection
  的可观察输出一致。
- 允许脚本路径：仅 `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`。
- 不属于本次范围：所有 production AI code、default profile、worker/SoA layout、spatial index、physical input、
  scene/DAT/render、C++、R3-PHY-01、D-MOV、R4～R8。
- 关联 Change ID：`R3-AI-LIFE-001`、`R3-INP-003A-001`、`R3-INP-004-001` 仅为已存在的相邻输入契约，
  不扩大它们的范围。

## 2. Authority / 需求依据

- **C++ release source / live path**：`src/input/input_handler.cpp:1667-1750`。
  - `team_candidate_allowed`：1667-1675；
  - ground scan、strict `<` 和升序 slot：1680-1706；
  - `same_z_lane` 与 air override：1708-1731；
  - cache `unk_360`、存活 character-DAT、一次 `%30`、retain / recache：1733-1749。
- **Unity对应路径**：
  - fallback：`SimulationWorld.AiInput.partial.cs:1427-1579,3045-3245,4925-4946`；
  - indexed：`AiSensingKernel.cs:68-110,488-520`、`AiDecisionKernel.cs:243-318`。
- Evidence 等级：C++ / Unity static contract 为 `VERIFIED`；C++ executable trace 为 `BLOCKED`；本次 fixture
  结果尚为 `PENDING`。

## 3. Unity 原状与已确认差异

- Unity 现有 self-check 分别测试 spatial tie、phase1 target index 和基本 cache deterministic world；它们没有
  作为同一个 profile-pair full input fixture比较 cache decision后的 input signature、RNG state与call count。
- `LegacyCanonical` 与 `DataOrientedCanonical` 都是 Unity implementation；二者相等只能证明 optimized path
  未偏离 Unity fallback，不等同 C++ executable runtime trace。
- 首次 full self-check（2026-08-22 01:45:50）已产生 first difference：cache-retain fixture中
  `legacyTarget=7 / legacyRngCallCount=7`，而 `indexedTarget=4 / indexedRngCallCount=1`。只读定位表明
  fixture在 registration 后直接写了 `Runtime.Unk360`；`BattleCharacterInputStore.Bind` 于绑定时捕获 runtime
  input row，indexed kernel之后读取该 canonical row，因此其 initial cache仍为 bind-time default。当前 production
  source检索未发现本包范围外 post-bind direct `Runtime.Unk360` writer。本项是 fixture initial-state contract
  缺失，不是已确认 production mismatch。
- 第二次 full self-check（2026-08-22 01:51:01）在 fixture自己的 store/mirror assertion处失败：
  `CharacterInputWriter.CommitAiDecisionState` 已更新 store row，但其 `CommitFullRuntimeMirror` 不写
  `Runtime.Unk360`；正式 `BattleAiInputWriter` 才在 indexed decision commit后补写这三个 AI fields。故仍是
  fixture setup缺少 compatibility mirror，不是 profile algorithm mismatch。下一次修改仅会在 canonical commit
  后把同值写给 runtime mirror，再重跑。
- 本 Record 不预设存在 production bug；若 fixture失败，先记录 first difference，停止此 Record，不得在同一
  Record内直接改 production AI behavior。

## 4. 计划改动

| 文件 | 类型 / 方法 | 改前职责 | 目标职责 |
|---|---|---|---|
| `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs` | `RunAllChecksStatic`、新的私有 AI target helper | 分散 self-check 覆盖 target selection。 | 增加固定 Unity seed 的 legacy/data-oriented profile-pair fixture，断言 equal tie、air override、cache retain/refresh、team/phase，以及 input/RNG observable parity。 |

## 5. 不可回退边界

- 中央表现 / `CentralOnly` / Texture2DArray / 动态 Mesh：不触碰；
- `Authority400`、`MobileExtended`、`DesktopExtended` 容量合同：不触碰；
- 30 Hz、`FrameInputSet`、slot/generation、SoA/ECS、对象池、worker、0 GC：不改变 production policy；
- 既有 `R3-AI-LIFE-001` 的 no-target self HP eligibility 继续保留；本 Record 不重新引入 self HP early return。

## 6. 实际改动

| 文件 | 类型 / 方法 | 实际改动 | 预期副作用 |
|---|---|---|---|
| `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs` | `RunAllChecksStatic`、`CheckAiTargetFallbackIndexedContract`及私有 helpers | 新增四个 target eligibility search 场景，以及 retain / refresh 两个完整 `CharacterInputAll` profile-pair 场景。每个 profile-pair 对 selected/cache、input signature、RNG state/call count做断言。两次失败均定位为 fixture initial state：先是只写 mirror，随后只写 canonical store。下一步在同一 helper采用“canonical store commit + identical runtime mirror”的正式 writer/mirror precondition。 | 仅增加 Editor/self-check 的固定夹具运行量；不写 production AI 状态，不改变 default profile。 |

## 7. 验收与证据

| 层级 | 命令 / 场景 / 输入 | 实际结果 | 状态 |
|---|---|---|---|
| source/static | C++ `prepare_ai_input` target branch与Unity三条对应路径。 | 已完成 preflight；fixture尚未写。 | `PASS` |
| ledger / whitespace | `Tools/Validate-ChangeLedger.ps1`、`git diff --check` | final run：PASS（10 records / 10 governed code files）；diff check exit 0，只有既有LF/CRLF warning。 | `PASS` |
| Unity compile | existing Editor UnityMCP scripts refresh / filtered `error CS` | final refresh在 2026-08-22 01:53 完成；filtered `error CS`=0。 | `PASS` |
| focused/full self-check | `NTSD/验证/运行战斗运行时自检` | 01:45:50（direct mirror only）与01:51:01（canonical store only）失败均已留档并定位为fixture setup；final canonical-store + runtime-mirror setup于 **01:53:46** 使 `Temp/NTSD_BattleRuntimeSelfCheck.result=PASS`。 | `PASS` |
| Play Mode / scene | 真实 AI target、visual motion与技能交互。 | 不在本包。 | `RUNTIME_PENDING` |
| C++ authority trace | R1-WP02。 | 不运行 C++ executable。 | `BLOCKED` |

## 8. 风险、回滚与未关闭项

- 已知风险：完整 input path 会在 cache 后继续进入 special/combo 分支；fixture必须用最小普通 character
  world隔离该副作用，不能把 special scan的影响误归为 nearest/cache mismatch。
- 已知 fixture precondition：data-oriented canonical store 在 registration 时捕获 input row；test-only post-bind
  cache setup必须通过 `CharacterInputWriter.CommitAiDecisionState` 更新store，并同步同值到 Runtime mirror；
  不可只写任一侧。
- 未关闭项：C++ runtime trace、真实 AI Play Mode、special scan、combo、held/link、collision/lifecycle joint path。
- 回滚方式：仅移除本 Record 新增的 self-check invocation/private helpers和相应文档行；不得触碰既有
  production profile或其它 Record。

## 9. Git / 交接

- 修改前工作树基线：存在用户/历史未提交的 scene、settings、resource、docs和脚本差异；不得回退、覆盖或
  归属给本 Record。
- 实际脚本 diff 范围：仅 `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`（仍待 diff/compile核验）。
- 提交 hash：未提交。
- `Tools/Validate-ChangeLedger.ps1` 结果：final PASS（10 records / 10 governed code files）。
- 交接需优先阅读的文件：本 Record、`TASKS/R3-AI-TGT-01-fallback-indexed-target-contract.md`、
  `RESEARCH/R1-SOURCE-002-input-contract.md`、`RESEARCH/R1-SOURCE-ALL-DIFF-REGISTER.md`。
