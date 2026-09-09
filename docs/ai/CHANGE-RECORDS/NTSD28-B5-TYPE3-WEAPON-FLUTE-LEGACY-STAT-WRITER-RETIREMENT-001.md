# NTSD28-B5-TYPE3-WEAPON-FLUTE-LEGACY-STAT-WRITER-RETIREMENT-001

<!-- CHANGE-RECORD
id: NTSD28-B5-TYPE3-WEAPON-FLUTE-LEGACY-STAT-WRITER-RETIREMENT-001
status: VERIFIED
change-kind: BEHAVIOR_CORRECTION
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterHitResolver.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterDatHitResolver.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5Type3WeaponFluteLegacyStatRetirementEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleHitExecutionPlanEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5NonCharUnarmoredDamageScaleEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5OrdinaryCreditGate2F4CorrectionEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleCollisionHitDamagePlayModeProbeEditor.cs
authority: NTSD 2.8-Logan type3/other and weapon normal hurt exact HP accounting plus kind10/11 flute transaction contain no Unity legacy entity/world stat writes; EXE B1E13AE1, closure 39DDDA15.
evidence: Task/Change created before scripts. Focused RED was exactly 0/4 across type3/weapon, concrete/shared flute, HitPlan and source closure. Removed type3/weapon ComboCountVic and DamageStats actual/projections plus flute ComboCountAtk and DamageStats in both actual compatibility resolvers and HitPlan. Focused 4/4 and all B5 plus HitPlan related 777/777 passed. Assembly-CSharp built with 47 warnings/0 errors and Assembly-CSharp-Editor with 104 warnings/0 errors. Targeted Play passed 4 cases and the production live collision matrix passed with weapon/special combo 0, global stats restored, Console 0 error, and unchanged scene SHA 50FD4D8F. Full SelfCheck again reached only the pre-existing CPoint raw throw mode=0 Vz assertion at line 10939. Standard/reduced/CPoint/input/recovery and carrier/schema remain excluded.
-->

> 状态：`VERIFIED / RED_0_OF_4 / FOCUSED_4_OF_4 / RELATED_B5_777_OF_777 / TARGETED_PLAY_4_CASES / LIVE_COLLISION_MATRIX_PASS / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED / SCOPED_LEGACY_STATS_RETIRED / INPUT_COMPAT_AUDIT_NEXT`

完整范围、Authority、不变量、验收与回滚见
`docs/ai/TASKS/NTSD28-B5-TYPE3-WEAPON-FLUTE-LEGACY-STAT-WRITER-RETIREMENT-001.md`。

## 实际改动

- type3/other与weapon type1/2/4 actual helper和HitPlan projection移除victim ComboCountVic/world DamageStats。
- concrete/shared kind10/11 flute与HitPlan移除holder ComboCountAtk/world DamageStats。
- exact HP/InputHpConsumed、flute WeaponCount/frame/motion/sound及standard/reduced/CPoint旧stats保持。
- 相关旧fixture改为非零哨兵保持；前置+0x2F4源码闭包断言同步确认flute最后一处legacy gate已归零。

## 验证

- focused RED `0/4`；实施后最终 focused `4/4`。
- type3/weapon子集 `264/264`；全部67个B5 test class加HitPlan联合矩阵 `777/777`。
- `dotnet build Assembly-CSharp.csproj --no-restore --nologo --verbosity minimal`：47 warning，0 error。
- `dotnet build Assembly-CSharp-Editor.csproj --no-restore --nologo --verbosity minimal`：104 warning，0 error。
- `NTSD_Battle`真实Play专项4 cases通过；生产live collision probe通过，weapon/special legacy combo均保持0，global stats恢复，cleanup完成。
- Play后Console 0 error；Scene仍为13 roots、dirty false，SHA-256保持`50FD4D8FAF2CC5C630886CEFD4742A5FD068E1F7AADEE89809AD19B7B85AFF3C`。
- full SelfCheck仍仅在本包之前执行的既有CPoint raw throw mode=0 victim Vz断言（`BattleRuntimeSelfCheck.cs:10939`）失败；未到本包后段断言，故只报告`SELFCHECK_BLOCKED_UNRELATED`。
