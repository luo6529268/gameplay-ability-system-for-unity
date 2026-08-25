# R1-SOURCE-002 — Unity 输入实现 Crosswalk 与差异登记

> 状态：COMPLETED（静态 mapping / difference inventory）。  
> 结论边界：本文件不把 source reading、旧 C# self-check、Unity checksum 或性能
> diagnostic 写成 C++ runtime 行为验证。

## 1. Unity logical input 的数据路径

```text
Unity InputSystem callback
    → CharacterInputModule held mirror + direct SimInputBuffer edge（本地来源）
    → LocalSimulationFrameInputProvider.CaptureHeldSimulationButtons()
    → FrameInputSet[tick]（完整 held packet + 边沿 metadata）
    → SimulationWorld.ApplyFrameInputSet()
    → SimInputBuffer.EnqueueCompletePacketKeyForTick()
    → PostCooldownHumanInputAll / NTSDInputStateModule.PollFromBuffer()
    → Runtime Key / Prev / Cd / InputHistory
    → CharacterInputAll
    → AI prepare（如 AI）+ combo/direct/release action + frame velocity tail
```

前四段是 Unity / lockstep packet 适配；从 `PollFromBuffer` 开始才是 C++ `poll` / 
`apply_input` 可比较的逻辑状态边界。渲染帧回调、Transform、InputAction object 都不能
成为 battle runtime 真相。

## 2. 逐项静态映射

| C++ rule | Unity evidence | 静态结论 | 运行时状态 |
|---|---|---|---|
| post-cooldown 才输入 | `NTSDBattleTickSystem.cs:254-259`，cooldown 后调用 `PostCooldownHumanInput` | human poll 位置有对应 pass | 待测试 |
| previous ← held、complete held packet、edge → 5 tick cooldown/history | `NTSDInputStateModule.cs:74-122,286-368` | logical 规则和 C++ `poll` 结构匹配 | 待测试 |
| history code 6/4/8/2/9/0/5 | `NTSDInputStateModule.cs:337-368`、`BattleCharacterInputWriter.cs:295-330` | code / cross-cooldown mapping 与 C++ source 对应 | 待测试 |
| active character DAT 依 runtime slot 升序 | `SimulationEntityTraversal.cs:69-87` + `SimulationWorld.Passes.partial.cs:261-320` | current Unity cursor 由 slot 0 单调向上 | 待测试 |
| AI prepare 后同 tick action resolve | `LF2Character.cs:829-867`、`BattleEcsCharacterInputPass.cs:102-138` | Unity per-entity internal order可映射 | 待测试 |
| combo → direct hit → action/release → dv tail | `BattleCharacterInputActionResolver.cs:55-87,130-475`、`BattleCharacterActionWriter.cs:34-109` | Unity 被拆到 resolver/writer，但存在对应子段 | 需 R3 state matrix |
| C++ physical P1/P2 polling | `SimulationFrameInputModule.cs:72-103,179-218` | Unity使用 roster binding/FrameInputSet；不是同一 API | 2-human fixture 待测试 |

## 3. 已确认的静态断点

### 3.1 C++ callback 与 Unity pass 的分裂点

C++ `main.cpp` 的 post-cooldown callback 在返回 `game_tick` 后续 T03 前完成**全部**
input processing。Unity 当前将它拆成：

```text
Unity: Cooldown → HumanInput → OID 7/8/51 maintenance → CharacterInput
C++  : Cooldown → Human poll → (AI prepare + all character apply_input) → OID 7/8/51 maintenance
```

这不是“同一个 callback 被拆成多个函数”的中性重构，因为 OID maintenance 位于两个
input 子段之间。因此列为 `D-SCHED-005：待处理`。

### 3.2 `NeedClearInput` 与 C++ F1/F2 不是同义物

当前 Unity `NeedClearInput` 由 bootstrap 设置（`SimulationTickDriver.cs:1213`），在
`NTSDBattleTickSystem` 中清完输入后完整 early return。C++ F1 wait 则不清 input、跳过
callback、仍进入 OID/frame/collision/preframe/render，render 后才 early return。两者不能
互相承担行为对齐职责，故 `D-SCHED-010：待处理`。

### 3.3 negative link / dead AI 的前置 return

Unity 目前有 C++ caller/source 没有的两个整体前置 return：

- `Runtime.LinkState < 0` 不进入 CharacterInput；
- `Runtime.HP <= 0` 不进入 AI prepare legacy core。

它们可能在 held、caught、death、respawn、opoint 链上造成输入/history/cooldown 与 frame
差异；必须等 R1-SOURCE-003/005 闭合相关生命周期后再修改。

## 4. 不应被误判为差异的 Unity-native 层

- `FrameInputSet`、`SimInputBuffer`、lockstep journal、worker、SoA writer 和中央表现
  都是 Unity 实现边界。只要输入消费时点、fields、状态变化和可观察行为满足 C++ 合同，
  它们不需要回退到 C++ 形式。
- `PressedButtons`/`ReleasedButtons` 暂未直接被 `ApplyFrameInputSet` 读取，是输入 journal
  contract 风险，不证明 actual move/combo 已错误；需要规定完整 held packet 后才可裁决。
- Unity 的 8 local player buffer、MobileExtended 1,000 active 与 DesktopExtended dynamic
  growth 是扩展能力，不得因 C++ P1/P2 或 400 fixed table 而删除；C++ 对照仍只在
  Authority400 / 2-human fixture 内进行。

## 5. 后续引用

- 完整 C++ input source contract：`docs/ai/RESEARCH/R1-SOURCE-002-input-contract.md`
- 主 pass 顺序差异：`docs/ai/RESEARCH/R1-SOURCE-001-unity-crosswalk-and-diff-inventory.md`
- 对应后续修复阶段：R3，但前置仍包括 R1-SOURCE-003、004、005、007。
