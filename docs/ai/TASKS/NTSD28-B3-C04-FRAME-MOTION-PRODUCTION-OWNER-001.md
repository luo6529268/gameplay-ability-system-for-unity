# Task Contract — NTSD28-B3-C04-FRAME-MOTION-PRODUCTION-OWNER-001

> 状态：`VERIFIED / BASIC-C04-OWNER / REAL-PLAY-PASS / FULL-C04-B4-PENDING`
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B3 C04`
> 依赖：`NTSD28-B3-FRAME-MOTION-OWNER-AUDIT-001 / VERIFIED`

## 目标

建立独立的 production C04 全 slot frame-motion owner：C03 route 完成后、C05 teleport 前，按升序 active
slot 恰好一次应用现有 `dvx/dvy/dvz` 算法。production C03 不再写 frame motion，后续 non-character
`SimTU` 不再重复写；direct `CharacterInputAll` 与 direct `SimTU` 保持既有 combined compatibility。

本包只闭合 B3 owner/placement，不宣称已实现 C04 缺失的 linked-platform、delay scale、frame
`dx/dy/dz` positional motion。

## 允许代码路径

- `Assets/NTSD/Scripts/Simulation/Core/NTSDBattleTickSystem.cs`
- `Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs`
- `Assets/NTSD/Scripts/Simulation/Ecs/Passes/BattleEcsCharacterInputPass.cs`
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs`
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Character.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28BattleActualPhaseSequenceEditorTests.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28C04FrameMotionProductionOwnerEditorTests.cs`（新增及meta）
- `Assets/NTSD/Scripts/Test/Editor/CharacterInputLiveSlotLoopEditorTests.cs`（仅兼容断言必要修正）
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`（仅旧placement断言必要修正）
- 本 Task/Record、Ledger、STATE、handoff、B3 manifest 与总表。

不修改C04算法常量、C05 teleport、C06 physics、state500/501、Scene/Prefab/Config/资源或authority目录。

## 不变量

- production C03完成后velocity仍是输入动作结果，C04才读取最终current frame/input并写frame velocity。
- C04不受frame delay、link/cpoint或C06 physics gate影响；lifecycle-inactive slot仍排除。
- production后续SimTU不得重复应用 additive dvy；direct SimTU仍可使用原combined入口。
- direct `CharacterInputAll`保持现有输入+frame-motion compatibility，production使用专用route-only开关。
- full phase occurrence新增显式FrameMotion；NeedClear partial仍为4项且不进入C04。

## 验收

1. test-first覆盖production C03不写、C04写一次、frame-delay不阻断、Serial不重复、direct兼容与phase位置。
2. Unity compile0；focused与输入/frame/worker相关回归、SelfCheck通过。
3. 真实kind0 Play通过且Scene unchanged、Play退出、Console0。
4. Ledger validator与scoped diff check通过；下一首差推进到C05 native teleport/combined EarlyFrameAdvance。

## 回滚

移除显式C04调用和route/Serial兼容开关，恢复production C03与SimTU旧内嵌writer；不回退已验证C01～C03、
cooldown、OID split或新权威。
