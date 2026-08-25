# R7-AI-01 — AI sensing / indexed target recertification

> 日期：2026-08-22  
> 状态：`RUNTIME_PENDING / NO-PRODUCTION-CODE CONDITIONAL CERTIFICATION`

## 1. Boundary

本次只重新认证 `prepare_ai_input(...)` 的 sensing 前半段及其 Unity optimized adapter：

- difficulty / first-10 move-mode context；
- ground target scan；
- ground-derived `best_dist` / `same_z_lane`；
- air override；
- cached target retain / refresh及其单次 `% 30` RNG；
- same-team guard summary；
- slot20+ special object scan；
- fallback / SoA / indexed结果、RNG和input facts等价性。

**不包括** C++ `input_handler.cpp:1900+` 的完整角色/OID decision tree、combo、post-special
main decision或真实AI战斗表现。后者必须由独立 `R7-AI-02` 继续逐段审计。

## 2. C++ release authority contract

Authority文件参与release `Makefile`构建；本次只读：

- `src/input/input_handler.cpp:1209-1235`：phase1且self team非5时，只扫描slot0..9，
  排除self/inactive/no-DAT/dead/non-character，以严格`>`保留最低slot同X tie，计算move mode 0/1/2；
- `:1615-1759`：roll/clear、difficulty、ground ascending strict `<`、ground lane、air override、
  cache retain/refresh与no-target exit；
- `:1761-1898`：same-team guard、slot20..399 special scan、C8/D3/D4 threat方向、
  OID100..199/D5 selection、C8 post-selection、7A force与C8 restore；
- `src/entity/game_tick.cpp`：character input在death/respawn cleanup前执行，因此self HP=0
  不是 sensing callback global skip。

## 3. Unity mapping

| C++ contract | Unity fallback / optimized mapping | Result |
|---|---|---|
| ground slot ascending + strict low-slot tie | `AiSensingKernel.FindLinearGround` / `FindIndexedGround`，indexed equal-distance显式选低slot | source-mapped |
| `best_dist`与`same_z_lane`在air override前固定 | `TryFindNearestCore`先保存ground facts，再仅替换selected air slot | source-mapped |
| air strict `<40` Z / `<250` X、strict lower distance | linear/indexed air kernels | source-mapped |
| team/input-phase predicate | `TeamAllowed`与role-team summary | source-mapped |
| cache `%30`一次及retain/refresh | `AiDecisionKernel`与legacy core cached target段 | source-mapped + tests |
| first10 move mode | snapshot top/second或full scan；self为top时选second，保持低slot strict tie | source-mapped + tests |
| slot20+ special scan | `SpecialSlots`只索引OID100..199、C8、D3、D4、D5；其他OID在C++ loop无side effect | source-mapped |
| special scan slot order | authority/unified snapshot按runtime slot升序capture，indexed list保持升序 | source-mapped + tests |
| same-team full scan | indexed team count/minHP summary；fallback保留full scan | source-mapped + tests |
| active self HP=0 | `R3-AI-LIFE-001`三处self-HP gate已移除 | source-mapped；test contract corrected |

生产配置已确认：

- `GameConfig.asset`：`BattleAiExecutionProfileName: DataOrientedCanonical`；
- `NTSD_Battle.unity`：`effectiveAiExecutionProfile: DataOrientedCanonical`；
- profile resolver默认同样为`DataOrientedCanonical`；
- 该profile原子启用SoA sensing、indexed canonical decision和unified snapshot authority。

## 4. Test evidence

### 4.1 First failure and correction

初始AI筛选job `6fdd44f773344cffbce04404bfddfd86` 在旧测试
`DecisionRemainder_IneligibleCharacterInput_DoesNotCountAttempt("dead")` 失败：expected 0 / actual 1。
静态核对确认production符合C++，旧Editor fixture陈旧。独立 `R7-AI-TEST-001` 在脚本修改前建立，
只拆分dead/coordinate测试，不改production AI。

### 4.2 Fresh results

- fresh `Assembly-CSharp-Editor.dll`：2026-08-22 21:01:39；Console error 0；
- exact correction job `8c74d8e0a76e427fac3fd7920f5ac234`：2/2 PASS；
- sensing/profile job `5c6bad85dc0b43c2a6949d03cfd256fc`：111/111 PASS；
- 2026-08-22 21:04:52 full `BattleRuntimeSelfCheck`：PASS；
- ledger validator：45 records / 32 governed files PASS；scoped diff check PASS。

111 tests覆盖target tie、team/phase、air absolute boundaries/override、cache RNG、special OID/index、
empty-special fast path、same-team summary、snapshot mutation/fail-closed、move-mode first10、profile coherence、
fallback/indexed differential与warmed 0 B。它们证明Unity内部等价性与回归，不是C++ runtime trace。

## 5. Result

- 在本包定义的sensing/index范围内，未发现新的production source-confirmed difference；
- `D-INP-005` 可提升为 `R7-AI-01 conditional source/test certification`，最高仍是
  `RUNTIME_PENDING`；
- 唯一发现项是陈旧Editor fixture，已由独立test-only Change Record修正并验证；
- 完整AI decision tree尚未重新认证，不能宣称“AI已完全对齐”。

## 6. Unknowns / reopen conditions

- C++ `input_handler.cpp:1900+` OID-specific decision/RNG/input edge完整顺序；
- 真实AI Play Mode、1000 AI行为与C++ runtime trace；
- Unity扩展slot >399参与AI感知是已批准容量adapter，但其扩展语义不是C++ 400-slot规则；
- mod/mutable DAT、profile override或snapshot/index构建策略变化；
- 任一新测试出现target/input/RNG first difference。

## 7. Next

建立独立 `R7-AI-02`：按C++ source顺序逐段复核 post-special完整decision tree与
`AiDecisionKernel` / canonical store direct writer。若发现差异，先登记并建立新的Task/Change Record；
不得借 `R7-AI-TEST-001` 修改production。

