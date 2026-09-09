# Task Contract — NTSD28-B3-RESIDUAL-SERIAL-TAIL-REHOME-001

> 状态：`VERIFIED / C14-C15-ADJACENCY / SERIAL-AFTER-C20 / TARGETED-PLAY-PASS / BODY-PRESERVED`
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B3 C15前置清障`
> 依赖：`NTSD28-B3-C14-TYPE0-HIT-PLACEMENT-001 / VERIFIED`

## 目标

消除Unity生产顺序中夹在Authority C14 type-zero hit与用户保留C15 random-drop之间的旧`SerialTickAll` remainder。

本包只把现有兼容逻辑后移到C15～C20完成后，作为后续C25逐slot事务实施前的临时tail proxy；不重写type3 state-entry/state15/death、runtime snapshot或state9998 cleanup算法。

## 已确认事实

- 当前`SerialTickAll(... nativePhysicsAlreadyApplied:true)`不会再执行Transit/physics；普通实体没有业务writer。
- 剩余业务writer只有`LF2SpecialAttack.RunPostNativePhysicsSerialForWorldPass`中的type3 state-entry、state15横速与死亡跳帧，以及全局`CleanupState9998Entities()`。
- Authority C14后立即进入C15，C16～C20随后执行；frame/state/lifecycle属于C25逐slot tail，terminal lifecycle在同slot frame/opoint/particle/fragment之后。
- Unity的`frame.state == 9998`全局清理不是Authority terminal-action lifecycle的直接等价实现；本包仅后移，不把它误报为已对齐。

## 允许代码路径

- `Assets/NTSD/Scripts/Simulation/Core/NTSDBattleTickSystem.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28BattleActualPhaseSequenceEditorTests.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28C14TypeZeroHitPlacementEditorTests.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28ResidualSerialTailRehomeEditorTests.cs`（新增及meta）
- `Assets/NTSD/Scripts/Test/Editor/NTSD28ResidualSerialTailRehomePlayModeProbeEditor.cs`（新增及meta，若静态/Editor证据不足）
- C04～C14 placement测试（仅修正serial sentinel的相邻顺序断言）
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`（仅phase契约必要修正）
- 本Task/Record、Ledger、STATE、handoff、B3 manifest与总表。

不修改`LF2SpecialAttack`、`SimulationWorld.SerialTickAll`/`CleanupState9998Entities`、hit consumer、random-drop、catch/held/stage算法、Scene/Prefab/Config或authority目录。

## 不变量与验收

- C13→C14→C15例外→C16，二者之间不再出现`FrameAdvance`/serial。
- serial remainder在C20第二次held之后才执行；现有type3/state9998行为本体不变。
- C15 random-drop实现、RNG、候选、slot与用户例外标记不变。
- full/partial occurrence数量保持32/4；下一结构首差应推进到C21对临时serial proxy。
- 必须取得test-first red、Unity compile、focused/相关、SelfCheck与目标Play；Scene保持不变且Play退出。

## 风险与回滚

- 风险：serial后移会改变type3死亡跳帧或state9998对象在C15～C20的同tick可见性；这是Authority顺序要求，也是目标Play必须覆盖的观察点。
- 回滚：只把`FrameAdvanceAll`及其phase occurrence移回C14后；不得回退C01～C14，也不得改C15例外算法。

## 完成证据

- test-first精确job `9df9b1f1d5f84e7294f64c0e2c60eda4`为0/2：phase16仍为`FrameAdvance`，serial观察到RNG 0而不是C15后的1。
- production只移动`FrameAdvanceAll` caller：从C14后移到current C20第二次held后；`SerialTickAll`、type3、state9998 cleanup和C15实现均未改。
- fresh Unity compile为0个`error CS`；focused job `3325a61e0a5341798d362687554af4f3`为2/2 PASS。
- C04～C14/actual phase相关job `152c405251a04838bdeb598a4ff1e652`为44/44 PASS；frame/special/state9998/lifecycle相关job `86dc1dfbdf9245889db3f32275f0ea0f`为289/289 PASS。
- fresh `BattleRuntimeSelfCheck`于2026-09-05 05:07:12 +08写出PASS；full/partial occurrence保持32/4。
- 真实`NTSD_Battle` Play probe：tick6，C14 execution=3，C15 RNG 5→6，serial观察6；phase15/16/17/22/23为C14/C15/C16/current-C20/serial proxy；cleanup PASS。artifact SHA-256为`E3A0B326AE09E08AE1835DD6A3F632C054395D653D3691BA23B71C5763829D58`。
- 清空Console后第二次Play仍PASS，Play已退出、Console error=0；Scene SHA-256及mtime保持`0D74E174D37AF673CB717C699D13F3D115C65EDD332E373B6673757D5E323D77` / `2026-09-04T13:12:45.1526434Z`。
- 下一结构首差推进到Authority C21相对临时serial proxy；C17～C20具体catch/held算法仍分别归B6验证，C25逐slot tail尚未实施。
