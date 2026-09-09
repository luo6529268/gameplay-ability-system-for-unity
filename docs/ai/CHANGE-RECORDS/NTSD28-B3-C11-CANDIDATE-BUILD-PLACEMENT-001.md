# NTSD28-B3-C11-CANDIDATE-BUILD-PLACEMENT-001 — C11 candidate transaction placement

<!-- CHANGE-RECORD
id: NTSD28-B3-C11-CANDIDATE-BUILD-PLACEMENT-001
status: VERIFIED
change-kind: TEST_FIRST_BEHAVIOR
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSDBattleTickSystem.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28BattleActualPhaseSequenceEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C06NestedPhysicsProductionEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C07RevivalProductionPlacementEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C08StageDepthPlacementEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C09HeldRefillPlacementEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C10CollisionActionSnapshotPlacementEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C10CollisionActionSnapshotPlacementPlayModeProbeEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C11CandidateBuildPlacementEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C11CandidateBuildPlacementPlayModeProbeEditor.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: Promoted NTSD 2.8-Logan SimulationTickDriver28 C11 rebuild_geometric_hit_candidates immediately after C10 and before C12; BattleWorld28 candidate transaction includes rest prelude and directed-pair vrest; EXE B1E13AE1, playable closure 39DDDA15.
evidence: TASK-CONTRACT-CREATED / AUTHORITY-C11-CONTIGUOUS-AFTER-C10 / RED-2-OF-2 / UNITY-COMPILE-0 / FOCUSED-2-OF-2-JOB-306A2A4F / C04-C11-ACTUAL-34-OF-34-JOB-CD807867 / REST-COLLISION-CANDIDATE-HIT-28-OF-28-JOB-1FE8FE71 / SELFCHECK-2026-09-05T03-08-13-PASS / TARGETED-PLAY-PASS / SERIAL-REST-0 / PAIR-VISIT-5 / RESULT-SHA-3C43ADC9 / CLEANUP-PASS / SCENE-SHA-0D74E174-UNCHANGED / PLAY-EXITED / CONSOLE-0 / FULL-31-PARTIAL-4 / ALGORITHM-PENDING-B5 / NEXT-C12 / AUTHORITY-READ-ONLY
-->

> 状态：`VERIFIED / C11-PLACEMENT / TARGETED-PLAY-PASS / ALGORITHM-PENDING-B5`

## 改前事实

- Authority C11在C10后连续执行rest prelude、directed-pair vrest与candidate geometry，然后进入C12 fusion。
- Unity已有`PrepareAttackerRestForCandidateAll`、`TickCollisionPairVRestAll`、`CollectCollisionCandidatesAll`，但
  当前在C10后先执行serial remainder，导致C11 transaction被延后。
- 本包只移动既有owner，不以静态顺序证明B5算法等价。

## 计划

- 写serial可见性与phase连续性red tests。
- 整体前移C11三个步骤，保持内部次序。
- compile、focused、相关、SelfCheck与真实Play后记录C12首差。

## 实际改动与验证

- C11三个既有步骤整体前移到C10后、serial前，内部顺序和算法未改。
- red2→compile0；focused2 job `306a2a4fc9c14d6e921d566ef1f1d3b2`；C04～C11/actual34 job
  `cd80786752674c78be22af533f966e33`；相关28 job `1fe8fe71354b4093abc2f390322ffad8`。
- SelfCheck `2026-09-05 03:08:13 +08:00` PASS；Play中serial观察rest0、PairVRest visit5，cleanup PASS，
  SHA `3C43ADC9E2FA7D012D2AFDEE27F7C39010828C8A9C566FC90572D5E1493D0C5F`。
- Scene unchanged、Play exited、Console0，full/partial31/4。

## 未关闭边界

- 本Record只证明placement/transaction连续性；candidate geometry、platform/kind/rest细节仍归B5。
- 下一结构首差为C12 fusion相对serial remainder。
