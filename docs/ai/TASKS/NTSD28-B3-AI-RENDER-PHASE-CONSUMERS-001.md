# Task Contract — NTSD28-B3-AI-RENDER-PHASE-CONSUMERS-001

> 状态：`VERIFIED / AI_RENDER_PHASE_CONSUMERS / PHYSICAL_Y_PRESERVED`
> 依赖：`NTSD28-B3-AI-RENDER-PHASE-CONSUMER-AUDIT-001 / VERIFIED`

## 目标

将 current Authority 中明确读取 `render_phase_008` 的 AI target role/index、abnormal dispatch、held blocker与synchronized state17 consumer，从Unity旧`Y`投影迁到已经验证的`HitStop`载体，同时保留真正读取`position.y`的角色专项分支及其RNG顺序。

## 允许路径

- `Assets/NTSD/Scripts/Simulation/Ai/Kernel/AiSensingKernel.cs`
- `Assets/NTSD/Scripts/Simulation/Ai/Kernel/AiDecisionKernel.cs`
- `Assets/NTSD/Scripts/Simulation/Ai/Runtime/SimulationAiInputModule.cs`
- `Assets/NTSD/Scripts/Simulation/Ai/Runtime/SimulationAiSensingModule.cs`
- `Assets/NTSD/Scripts/Simulation/Ai/Runtime/SimulationAiDecisionModule.cs`
- `Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs`仅新增明确只读HitStop seam。
- focused test及被新绑定证据supersede的旧Y fixture/断言。
- 本Task/Record/Ledger/STATE/handoff/总表和审计manifest。

## 不变量

- 不改变snapshot中物理`Y`的采集、复制或相等性验证；`HitStop`已有AoS/SoA传播链，不加重复carrier。
- normal role：HP>0、state!=14且`abs(HitStop)<=2`；abnormal role：state14或`abs(HitStop)>2`。
- cached/full/indexed/spatial role和fallback必须使用同一predicate。
- held line blocker使用candidate HitStop；weapon-run state17使用subject HitStop。
- OID11、action271、state12/-40、OID1/21/17空中跳跃等继续读取物理Y。
- RNG call count/site/order/hash不得因字段迁移改变，除非Y/HitStop互异导致Authority规定的控制流改变；相同predicate结果时必须bit-identical。

## 不做

- 不修改其他AI算法、RNG primitive/input producer、C25g/C25i、Config/资源/Scene/Authority。
- 不把所有`rows.Y`或`world.Y`机械替换。

## 验收

- Y/HitStop互异测试先红后绿，覆盖normal/abnormal role、full/indexed一致、SoA/legacy、held blocker/state17。
- 物理Y保留fixture与source guard通过；相同逻辑predicate下RNG witness不变。
- compile0、focused、all-AI、NTSD28 broad、SelfCheck通过；不要求Play，Scene unchanged、Console0。

## 回滚

只撤销本包明确的consumer字段读取、HitStop只读seam及测试修订；不得回退render-phase binding或C25h owner。

## 实施结果

- normal/abnormal target role、cached/primary scan、C8/air dispatch、held blocker与weapon-run state17均改读已验证的`HitStop -> render_phase_008`载体。
- AoS/SoA、indexed/full、legacy compatibility和self-check helper统一使用同一render-phase predicate；物理Y继续保留给OID11/action271/state12/-40和OID1/21/17高度分支。
- 旧测试中以Y隐式表示render phase的fixture已改为显式设置HitStop；真实Y与render phase可以互异。

## 验收结果

- test-first job `5b666972b48345b68e2e1d70a4dcc9ec`：22项中4项按预期红灯；首次实现后`59c909af70144067b9ac1bcdfa035a71`为22/22 PASS。
- all-AI首次`13a89474bc374f579e85b588898520a5`仅暴露9个旧Y fixture；修正后`1bd44b4ee9c8459d8663bd042a27547a`为385/385 PASS。
- 相关索引/夹具组`47399a167ec94d1a88614d2cbeaf0607`为121/121 PASS；NTSD28 broad `37f79b473c6747c7a27b44971c2e0639`为428/428 PASS。
- 最终compile后`a80ce2815906478a95f60b3bbee77075`为24/24、`8665718577b64724ba0d43259cdd68d6`为3/3；2026-09-05 10:29:46 BattleRuntimeSelfCheck fresh PASS。
- SelfCheck先后定位并修正3组旧Y-as-render-phase夹具；最终故意产生的7条rest-binding防御日志清除后Console error=0。
- `NTSD_Battle.unity`保持SHA-256 `0D74E174D37AF673CB717C699D13F3D115C65EDD332E373B6673757D5E323D77`、203477 bytes、UTC mtime `2026-09-04T13:12:45.1526434Z`；未要求Play。
