# Task Contract — NTSD28-B3-C14-TYPE0-HIT-PLACEMENT-001

> 状态：`VERIFIED / C14-PLACEMENT / TARGETED-PLAY-PASS / HIT-ALGORITHM-PRESERVED`
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B3 C14`
> 依赖：`NTSD28-B3-C13-ACTIVE-WEAPON-COUNT-PLACEMENT-001 / VERIFIED`

## 目标

把现有type-0 `PostInteractionTickAll` caller从serial remainder后移动到C13 active weapon count后、serial前，只闭合C14 caller placement与写入可见性。

不修改candidate store、consumer body、kind/effect/relation/armor/damage/termination算法，不移动C16 non-type-0 caller，也不改变用户批准的C15 Unity随机掉武器例外。

## 允许代码路径

- `Assets/NTSD/Scripts/Simulation/Core/NTSDBattleTickSystem.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28BattleActualPhaseSequenceEditorTests.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28C06NestedPhysicsProductionEditorTests.cs`～`NTSD28C13ActiveWeaponCountPlacementEditorTests.cs`（仅相邻phase断言）
- `Assets/NTSD/Scripts/Test/Editor/NTSD28C14TypeZeroHitPlacementEditorTests.cs`（新增及meta）
- `Assets/NTSD/Scripts/Test/Editor/NTSD28C14TypeZeroHitPlacementPlayModeProbeEditor.cs`（新增及meta）
- `Assets/NTSD/Scripts/Test/Editor/W06CollisionHitResolveWitnessEditorTests.cs`（只修正旧私有InteractionPhase边界断言）
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`（仅phase契约必要修正）
- 本Task/Record、Ledger、STATE、handoff、B3 manifest与总表。

不修改InteractionPipeline、hit execution plan、resolver/writer、entity行为、random drop、Scene/Prefab/Config或authority目录。

## 不变量与验收

- production为C12 fusion→C13 count→C14 type-zero hit→serial remainder→用户例外drop→C16 non-type-zero hit。
- C14使用现有`PostInteractionTickAll`唯一caller；direct façade与consumer实现不变。
- serial remainder可观察到C14已执行；type-3 serial tail和state9998 cleanup不提前到C14前。
- full/partial occurrence保持32/4；compile、focused、相关、SelfCheck、Play和Scene/Console门通过。
- 下一结构首差为C15 random drop相对serial remainder；因C15是用户例外，只审计隔离边界，不擅自删除或改写。

## 回滚

把CharacterHitConsumePostInteraction移回RunInteractionPhase的RandomWeaponDrop之前；不得回退C01～C13。

## 完成证据

- test-first red job `3228a7e5791f46ff9fc7fbc34d8aed56`为0/2：serial观察hit caller count 0而不是1，phase index15为FrameAdvance而不是CharacterHit。
- production只把现有`ResolvePostInteractions/PostInteractionTickAll` caller移到C13后、serial前；InteractionPipeline、candidate/consumer/resolver/writer均未改。
- Unity fresh compile为0个`error CS`；focused job `bf02a6f05e9a4a55b21fefa1e90bcaa0`为2/2 PASS。
- C04～C14/actual sequence job `b2fdbe2272bc4aa08b2a2c63c26c929b`为42/42 PASS。
- hit execution/empty-pass/W06相关job `7f31c1396d52446fadc579c0cf3b9e9a`为196/196 PASS。首次相关job准确捕获旧W06私有InteractionPhase断言；该断言已改为禁止C14重复执行并保留drop→C16边界。
- fresh `BattleRuntimeSelfCheck`于2026-09-05 04:33:34 +08写出PASS；full/partial occurrence保持32/4。
- 最终真实`NTSD_Battle` Play probe：tick6，serial看到已完成的3个type0 caller executions；最终仍为3。phase14～18依次为ActiveWeaponCount、CharacterHitConsumePostInteraction、FrameAdvance、RandomWeaponDrop、ObjectHitConsume；cleanup PASS。结果SHA-256为`3695117EDDB5C81334A7D0ADD3DE74851DC25FD03F8C47E3DD100CDE373C6A95`。
- 最终Play已退出、Console error为0；Scene SHA-256前后均为`0D74E174D37AF673CB717C699D13F3D115C65EDD332E373B6673757D5E323D77`。
- 下一结构首差为Authority C15 random drop对Unity残留serial remainder；C15用户例外本体不改，下一包先审计并拆分serial剩余职责。

> 后续状态（2026-09-05）：`NTSD28-B3-RESIDUAL-SERIAL-TAIL-REHOME-001 / VERIFIED`已把serial caller从C14/C15之间后移到current C20后；这里保留的是C14包关闭时历史快照。
