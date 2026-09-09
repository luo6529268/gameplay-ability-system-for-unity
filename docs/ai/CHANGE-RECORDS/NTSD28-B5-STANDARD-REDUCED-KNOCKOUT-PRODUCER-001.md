# NTSD28-B5-STANDARD-REDUCED-KNOCKOUT-PRODUCER-001

<!-- CHANGE-RECORD
id: NTSD28-B5-STANDARD-REDUCED-KNOCKOUT-PRODUCER-001
status: VERIFIED
change-kind: BEHAVIOR_CORRECTION
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5StandardReducedKnockoutProducerEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleHitExecutionPlanEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: NTSD 2.8-Logan battle_world.cpp standard/reduced lethal wrappers 6680..6690 and 7114..7124 plus record_native_knockout 1403..1417; EXE B1E13AE1, closure 39DDDA15.
evidence: Task/Change created before scripts. Focused RED was 1/4: rejection matrix passed while standard/reduced owner chain, redirected source and HitPlan exact KO assertions failed. Added one shared pre-HP actual helper with two calls and StandardCreditKnockoutCount HitPlan capture/projection/difference observable. Final focused 4/4 and related 270/270 passed. Runtime/editor builds are 47 warnings 0 errors and 104 warnings 0 errors. Targeted NTSD_Battle Play passed 4 aggregate cases with Console 0 error and scene SHA 50FD4D8F unchanged, dirty false, roots 13. Full SelfCheck reaches the same unrelated CPoint throw mode0 Vz blocker. Legacy stats and world event/feed remain excluded.
-->

> 状态：`VERIFIED / RED_1_OF_4 / FOCUSED_4_OF_4 / RELATED_270_OF_270 / TARGETED_PLAY_4_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED / EXACT_KO_PRODUCER_READY / TYPE3_WEAPON_FLUTE_STATS_NEXT`

完整范围、Authority、不变量、验收与回滚见
`docs/ai/TASKS/NTSD28-B5-STANDARD-REDUCED-KNOCKOUT-PRODUCER-001.md`。

## 实际改动

- `ApplyNativeStandardHitKnockout()`在HP mutation前检查type0、exact gate、positive HP、effective lethal与
  resolved standard credit，并只对credit `KnockoutCount358++`。
- standard与reduced各调用一次；调用发生在kind4 pending count消费前。
- HitPlan snapshot新增同一credit KO observable，两个lethal projection递增并并入existing bit50 difference group。
- legacy holder/world stats、damage/score、event feed与persistent schema未修改。
- 既有reduced SelfCheck fixture新增exact owner与KO sentinel，原legacy断言继续保留。

## 验证结果

- RED `1/4`：只有拒绝矩阵原绿；standard/reduced owner-chain、redirected source与HitPlan exact KO均失败。
- focused `4/4`；完整HitPlan、+0x2F4、kind4、standard/reduced、unarmored与armor联合`270/270`。
- build：`Assembly-CSharp.csproj`=`47 warnings / 0 errors`；
  `Assembly-CSharp-Editor.csproj`=`104 warnings / 0 errors`。
- 2026-09-09 07:06 +08前后真实`NTSD_Battle` Play通过4个聚合case，覆盖standard、reduced、redirected、
  rejection与HitPlan；每个accepted lethal exact KO只增1。Console 0 error；Scene SHA-256
  `50FD4D8FAF2CC5C630886CEFD4742A5FD068E1F7AADEE89809AD19B7B85AFF3C`、dirty=false、rootCount=13不变。
- full SelfCheck仍在既有CPoint throw mode=0 victim Vz断言停止，与此前阻塞点相同。
