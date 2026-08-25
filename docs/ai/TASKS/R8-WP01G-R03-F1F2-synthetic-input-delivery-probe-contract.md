# R8-WP01G-R03-F1/F2 — deterministic synthetic physical-input delivery probe correction

> 建立日期：2026-08-23  
> 状态：`VERIFIED / TEST-ONLY`  
> Change ID：`R8-JOINTINPUT-PROBE-002`

## Goal

修正Editor/MCP自动验证中一次性`InputSystem.QueueStateEvent`可能落在`FrameInputSet`采样边界之外、导致
当前Temp报告与曾通过的R03证据互相矛盾的问题。探针必须继续经过真实Input System、
`CharacterInputModule`和canonical `FrameInputSet`，不得直接写runtime input/combo/frame/motion。

## Scope

仅允许修改：

- `Assets/NTSD/Scripts/Test/Editor/BattleComboPlayModeProbeEditor.cs`；
- `Assets/NTSD/Scripts/Test/Editor/BattlePhysicalMovementPlayModeProbeEditor.cs`；
- 同一Change的Ledger、Record、STATE、R03 research/task/handoff。

## Required behavior

1. 每个待确认物理边沿先正常queue一次；
2. 若下一逻辑tick的canonical输入仍未确认该边沿，则以“release state→下一tick重新press”的有限脉冲重试；
3. 每个阶段最多8次press attempt，超过即FAIL，不能无限重送；
4. 一旦`FrameInputSet`/runtime确认边沿，立即停止该阶段重试；
5. 报告每个阶段的press attempt数；
6. probe结束时强制queue neutral并注销callback；
7. 不调用`InputSystem.Update()`、不写input buffer、runtime key、combo、frame、position或velocity。

## Verification

- fresh compile 0 error；
- fresh Play中DDJ、DRA与D/K movement三份当前Temp报告均为PASS；
- 报告有有限attempt计数，且最终keyboard release；
- focused input/movement tests PASS；
- full `BattleRuntimeSelfCheck` PASS；
- Change Ledger validator与scoped diff-check PASS。

## Stop conditions

- 有限脉冲仍无法到达canonical `FrameInputSet`；
- 需要改production input、30Hz、worker、InputAction asset或直接写runtime；
- 首差指向真实input binding/action enable，而非探针采样时点。

## Out of scope

production gameplay、InputAction asset、C++、T8、IL2CPP、Android、服务器、F1/F2 debug。

## Result — 2026-08-23

- fresh compile0；
- F2 D/K attempt2/1 PASS；DDJ L/S/K attempt1/1/1→frame271 PASS；DRA L/D/J attempt1/1/1→frame263 PASS；
- 当前三份Temp报告均为fresh PASS；
- focused最终257/257 PASS；full self-check 17:33:15 PASS；Console error0；validator79/93 PASS；
- production gameplay与InputAction asset均未修改。
