# Task Contract — NTSD28-B3-C12-FUSION-BARRIER-PLACEMENT-001

> 状态：`VERIFIED / C12-PLACEMENT / TARGETED-PLAY-PASS / EXISTING-ALGORITHM-PRESERVED`
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B3 C12`
> 依赖：`NTSD28-B3-C11-CANDIDATE-BUILD-PLACEMENT-001 / VERIFIED`、`NTSD28-B3-OID5152-PRODUCTION-SPLIT-001 / VERIFIED`

## 目标

把已验证的C12 `Oid5152FusionScan` production owner从serial remainder后移动到C11 CandidateCollect后、serial前。
只闭合barrier placement和写入可见性；不修改fusion算法、OID内容、4500/900 timer或C25h writer。

## 允许代码路径

- `Assets/NTSD/Scripts/Simulation/Core/NTSDBattleTickSystem.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28BattleActualPhaseSequenceEditorTests.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28C06NestedPhysicsProductionEditorTests.cs`（仅相邻C12断言）
- `Assets/NTSD/Scripts/Test/Editor/NTSD28C07RevivalProductionPlacementEditorTests.cs`（仅相邻C12断言）
- `Assets/NTSD/Scripts/Test/Editor/NTSD28C08StageDepthPlacementEditorTests.cs`（仅相邻C12断言）
- `Assets/NTSD/Scripts/Test/Editor/NTSD28C09HeldRefillPlacementEditorTests.cs`（仅相邻C12断言）
- `Assets/NTSD/Scripts/Test/Editor/NTSD28C10CollisionActionSnapshotPlacementEditorTests.cs`（仅相邻C12断言）
- `Assets/NTSD/Scripts/Test/Editor/NTSD28C11CandidateBuildPlacementEditorTests.cs`（仅相邻C12断言）
- `Assets/NTSD/Scripts/Test/Editor/NTSD28C12FusionBarrierPlacementEditorTests.cs`（新增及meta）
- `Assets/NTSD/Scripts/Test/Editor/NTSD28C12FusionBarrierPlacementPlayModeProbeEditor.cs`（新增及meta）
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`（仅旧phase断言必要修正）
- 本Task/Record、Ledger、STATE、handoff、B3 manifest与总表。

不修改OID5152模块、resolver、资源、C13 active weapon count、hit/catch、Scene/Prefab/Config或authority目录。

## 不变量与验收

- production为C11 CandidateCollect→C12 fusion→serial；C25h timer仍只在late per-slot frame后递减。
- full/partial仍31/4；test-first证明serial从7观察到已融合51；compile、focused、相关、SelfCheck、Play均通过。
- 下一结构首差推进到C13 active weapon count相对serial remainder。

## 回滚

恢复CandidateCollect→serial→RuntimeMaintenance；不回退C01～C11或OID production split。

## 完成证据

- test-first red：production仍为C11→serial→C12时，2项focused分别捕获serial仍看到OID7和phase index 13仍是FrameAdvance。
- production只把既有`Oid5152FusionScan`移到CandidateCollect后、serial前；OID模块、4500/900 timer及C25h writer未改。
- Unity fresh compile为0个`error CS`；final focused job `8255c8bbeeca46bfb5184d5fdf1f6e9a`为2/2 PASS。
- C04～C12与actual sequence job `4c0e61b7920c4b419a74dde7a285ff29`为36/36 PASS；OID production split+C12 job `0b5eaa7c57214ac0a559a704c783a5f6`为6/6 PASS。
- fresh `BattleRuntimeSelfCheck`于2026-09-05 03:47:14 +08写出PASS；full/partial occurrence保持31/4。
- 最终真实`NTSD_Battle` Play probe：tick6中C03按native running route将frame10推进到frame9且state仍为2；C12随后把OID7/8融合为51/frame290，serial观察OID51，C25h后timer=4499，partner dormant，HP/HPBound=200/200，cleanup PASS。结果SHA-256为`FFAD6915C6C9B5C81C05A0C60093367BD64D267D74103458313DA1B7257504DA`。
- 初次Play失败证明fixture只提供frame10时，native route进入缺失frame9并令state不可用；补齐synthetic running frames 9～12后通过。该修正只属于测试数据完整性，不改变生产输入或fusion规则。
- 最终Play已退出，目标Play运行后的Console error为0；Scene SHA-256前后均为`0D74E174D37AF673CB717C699D13F3D115C65EDD332E373B6673757D5E323D77`。
- 下一结构首差为C13 active weapon count相对serial remainder；其writer/语义不属于本Task。
