# NTSD28-B5-TYPE3-LEGACY-HOLDERCOPY-WRITER-RETIREMENT-001

<!-- CHANGE-RECORD
id: NTSD28-B5-TYPE3-LEGACY-HOLDERCOPY-WRITER-RETIREMENT-001
status: VERIFIED
change-kind: BEHAVIOR_CORRECTION
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5Type3LegacyHolderCopyWriterRetirementEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleHitExecutionPlanEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: NTSD 2.8-Logan type3 kind9/generic/kind-transform target transactions do not copy any legacy HolderCopy field; exact group/owner/control/action/motion writes in BattleWorld28 remain authoritative; EXE B1E13AE1, closure 39DDDA15.
evidence: Task/Change created before scripts. Focused RED exactly 0/3: actual kind9 changed target HolderCopy 99 to 77, HitPlan changed 99 to 2, and source guard found the four forbidden writes. Removed only BattleDamageWriter.CopyRelation HolderCopy assignment and three type3 HitPlan projections; RelationTeam and pickup writer remain. Final focused 4/4 and related 284/284 passed. Runtime/editor builds are 47 warnings 0 errors and 104 warnings 0 errors. Targeted NTSD_Battle Play passed 3 cases with Console 0 error and scene SHA 50FD4D8F unchanged, dirty false, roots 13. Full SelfCheck reaches the same unrelated CPoint throw mode0 Vz blocker. Scope excludes non-type3 HolderCopy producers/carrier/schema.
-->

> 状态：`VERIFIED / RED_0_OF_3 / FOCUSED_4_OF_4 / RELATED_284_OF_284 / TARGETED_PLAY_3_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED / FOUR_TYPE3_WRITERS_RETIRED / TYPE3_SPECIFIC_FAMILY_EXIT_READY / ORDINARY_CREDIT_GATE_2F4_NEXT`

完整范围、Authority、不变量、验收与回滚见
`docs/ai/TASKS/NTSD28-B5-TYPE3-LEGACY-HOLDERCOPY-WRITER-RETIREMENT-001.md`。

## 实际改动

- `BattleDamageWriter.CopyRelation()`保留`RelationTeam`复制并移除legacy `HolderCopySlot`复制。
- HitPlan kind9、legacy D1与active-D1 projection各移除一处`TargetHolderCopySlot`覆盖。
- 修正test-first fixture定位：普通character damage用例恢复既有holder-slot设置，`77/99` sentinel只设置在
  `ShadowCompare_Type3Kind9WriterEffectMatchesAuthorityState`；首次相关回归的2个失败因此记为测试夹具问题，
  生产改动没有扩大。
- pickup `attackerSlot -> TargetHolderCopySlot`与其余carrier/schema均未修改。

## 验证结果

- RED：首次focused为`0/3`；actual sentinel `99→77`、HitPlan `99→2`，source guard命中四处。
- GREEN：最终focused `4/4`，其中直接执行既有`CheckSpecialAttackHitResolveAuditContracts()`以锁定SelfCheck
  sentinel；HitPlan、kind5、pair snapshot、hit-group与collision shadow联合回归`284/284`。
- build：`Assembly-CSharp.csproj`为`47 warnings / 0 errors`；
  `Assembly-CSharp-Editor.csproj`为`104 warnings / 0 errors`。
- 2026-09-09 06:04 +08前后真实`NTSD_Battle` Play通过3个case；Console为0 error；Scene SHA-256
  `50FD4D8FAF2CC5C630886CEFD4742A5FD068E1F7AADEE89809AD19B7B85AFF3C`、dirty=false、rootCount=13不变。
- full SelfCheck仍在既有`CheckCpointThrowRawAndTransformMatrix()`的character raw throw mode=0 victim Vz断言
  停止；阻塞早于本包BATTLE-C31检查且与本包无关。本包的BATTLE-C31已由focused reflection实际通过。
- 首次相关回归曾因sentinel误放入普通character fixture出现2个`ComboCountAtk 9 vs 4`失败；修正fixture范围后
  最终联合回归全绿，未据此扩大生产改动。
