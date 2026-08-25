# R7-AI-02B — current-frame `hit_j` optimized data contract

> 日期：2026-08-22  
> 状态：`RUNTIME_PENDING`

## Goal

为后续恢复 C++ OID11 frame290 side effect 建立完整、零歧义的 Unity 数据合同：AI fallback、SoA sensing 与
UnifiedAuthority 在同一可见时点读取当前逻辑帧 DAT 的 `hit_j`。本包只提供字段，不接入任何 OID helper。

## Scope

- `AiSensingSnapshot` 新增 `HitJ` 列，并覆盖初始化、增长复制与既有 snapshot 生命周期；
- legacy/SoA/unified initial capture 都从 `entity.GetFrameDataById(currentFrame)?.hit_j ?? 0` 采集；
- fallback/legacy允许在capture边界读取DAT；UnifiedAuthority必须由frame-motion canonical store在bind或Frame writer
  边界解析一次并保存派生HitJ，consumer capture不得回读Entity/DAT引用；
- CharacterInput 后的 ascending-slot refresh 将该 canonical `hit_j` 与 Frame/State 一起提交；
- unified full/refresh comparison 与 published-state validation 纳入 `HitJ`；
- 提供只读 AI accessor，供后续 02C～02F 使用，但本包不得调用它改变 gameplay；
- 添加 focused Editor tests，覆盖 initial capture、same-pass frame change、publisher commit、grow/copy 和 warmed 0 B。

## Authority / Evidence

- C++ authority：`J:\QQFile\NTSD2.4\ntsd_release\src\input\input_handler.cpp:569-580`。
- `ai_frame_hit_j` 通过 `obj.char_data->get_frame(obj.frame)` 读取当前逻辑帧；缺 data/frame 时返回 0。
- OID11 side effect 只在该值为 290 且 target `y_int < 0` 时写 defend key；该行为属于后续 helper 包，不在本包。
- Unity 当前 `AiSensingSnapshot`、SoA capture、UnifiedAuthority publisher 都只有 `Frame`/`State`，没有 `HitJ`。

## Files likely involved

- `Assets/NTSD/Scripts/Simulation/Ai/AiSensingSnapshot.cs`
- `Assets/NTSD/Scripts/Simulation/Ecs/BattleAiUnifiedRowPublisher.cs`
- `Assets/NTSD/Scripts/Simulation/Ecs/BattleFrameMotionStore.cs`
- `Assets/NTSD/Scripts/Simulation/Ecs/BattleFrameMotionWriter.cs`
- `Assets/NTSD/Scripts/Simulation/SimulationWorld.cs`
- `Assets/NTSD/Scripts/Simulation/SimulationWorld.Registry.partial.cs`
- `Assets/NTSD/Scripts/Simulation/SimulationWorld.AiSoaShadow.partial.cs`
- `Assets/NTSD/Scripts/Simulation/SimulationWorld.AiDecisionShadow.partial.cs`
- `Assets/NTSD/Scripts/Simulation/SimulationWorld.AiInput.partial.cs`
- `Assets/NTSD/Scripts/Test/Editor/AiDecisionSoAShadowEditorTests.cs`
- `Assets/NTSD/Scripts/Test/Editor/CharacterInputLiveSlotLoopEditorTests.cs`

## Unknowns

- C++ runtime trace 仍受 R1-WP02 blocker 限制；本包只能取得 source/Unity 自动证据。
- OID11 helper 的最终行为与真实 DAT Play Mode 留给 02C/02F/R8。

## Deliverables

1. HitJ snapshot/unified row contract；
2. same-pass refresh 与 grow/copy tests；
3. Unity compile、focused regression、AI baseline、full self-check、ledger validator 证据；
4. 更新差异登记、STATE、Change Record 与 handoff。

## Verification

1. fresh Unity script compile：0 error；
2. exact focused test class PASS；
3. existing AI sensing/unified suites PASS；
4. `BattleRuntimeSelfCheck` PASS；
5. `Tools/Validate-ChangeLedger.ps1` PASS；
6. warmed update path 0 B（若测试环境的 NUnit 本身分配，必须把测量区限制到纯 publisher/capture loop）。

## Stop conditions

- 需要改变 AI helper/gate/RNG/order；
- 需要把 DAT 数据反写到 runtime 或表现层；
- 必须修改 C++ authority；
- first difference 指向 02C～02F 或其他模块。

## Out of scope

- OID11 side effect 本身；
- 39-position dispatcher、其他 OID helper、RNG gate；
- render、collision、CPoint、held/link、opoint、capacity policy；
- C++ 运行、构建、插桩或写入；
- Play Mode 与 R8。
