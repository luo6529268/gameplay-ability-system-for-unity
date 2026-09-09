# NTSD28-B5-FIRST-BDY-RESPONSE-ATOMIC-PRODUCTION-INTEGRATION-001

<!-- CHANGE-RECORD
id: NTSD28-B5-FIRST-BDY-RESPONSE-ATOMIC-PRODUCTION-INTEGRATION-001
status: VERIFIED
change-kind: BATTLE_BEHAVIOR
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/BattleHitCandidateSequenceRunner.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleFirstBodyResponseWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSD28NativeRandom.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5FirstBodyResponseAtomicProductionIntegrationEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleCollisionHitDamagePlayModeProbeEditor.cs
authority: NTSD 2.8-Logan BattleWorld28::resolve_confirmed_unarmored_hit first-current-Bdy response and SimulationTickDriver28 per-attacker abort; EXE B1E13AE1, closure 39DDDA15.
evidence: TEST-FIRST missing writer/HitPlan RED plus formal criminal OID300 RED 33-vs-30; final focused17, B5-138, HitPlan185, NTSD28-318, SelfCheck PASS, formal criminal Play11-candidate PASS, Console0 and Scene unchanged.
-->

> 状态：`VERIFIED / FIRST_BDY_RESPONSE_PRODUCTION_ALIGNED / FORMAL_CRIMINAL_PLAY_PASS`

## Authority与原状

carrier与pure core已验证，但shared runner仍在任何first-BDY响应前执行consume-effects和普通dispatch；同步RNG、
实际写入、当前attacker中止以及HitPlan attempt均不存在。因此production可观察行为尚未对齐。

## 计划修改

- new writer统一做route/first-frame读取、两阶段pure resolve、同步RNG提交和实际写入。
- shared runner在kind0 unarmored continuation的pre-consume位置调用，并对成功返回current-attacker
  termination；Unity提前分类的`Oid300Redirect`必须与`Damage`一同覆盖。
- HitPlan新增独立attempt projection/observation/snapshot及成功skip；NativeRandom scalar补generation identity。
- focused tests先RED，再分层做最小实现并在同一Change闭合。

## TEST-FIRST RED

focused fixture先修正自身缺少`NTSD.Extensions`与named-argument顺序，随后重新编译。剩余错误仅为预期生产缺口：
`BattleFirstBodyResponseWriter`、`BattleFirstBodyResponseAttemptResult`以及HitPlan diagnostics的
`ObservedFirstBodyResponseAttemptCount`/`LastFirstBodyResponseDifferenceMask`不存在。现在允许写最小原子实现。

## 实施中权威更正

准备使用冻结`criminal.dat`（OID300，22个1xxx first-BDY帧）做正式Play见证时发现：C++
`resolve_confirmed_unarmored_hit`先执行first-BDY响应，之后才进入non-character/OID特有普通尾；Unity则在共享
runner中提前把kind0/OID300分类成`Oid300Redirect`。因此原合同仅写`Damage`会错误绕过全部criminal响应。
本Change在继续改脚本前已将eligible disposition更正为`Damage || Oid300Redirect`，其余kind/disposition仍排除；
新增focused RED必须先证明当前OID300未响应，再做最小修复并同步HitPlan投影。

为取得真实Play证据，本Change允许只在既有`BattleCollisionHitDamagePlayModeProbeEditor`中增加一个读取
`CharacterAnimtorManager.GetCharacterConfig(300)`的formal criminal frame30矩阵；不得复制/改写配置，且必须沿用
probe现有暂停、baseline、owned-entity清理与Scene不保存合同。

## 实际修改

- `BattleFirstBodyResponseWriter`成为route、同步RNG提交和action/counter/group/hold/manual-damage原子写入的
  单一owner；broken type1 fallback先保留`RuntimeArmorHp118=-1`。
- shared candidate runner在consume-effects与普通dispatch之前调用writer；响应成功只结束当前attacker的
  candidate序列，chance失败与未识别kind继续普通路径。四种shell仍只调用同一runner。
- 实施中以正式`criminal.dat`发现并修正早期合同：Unity `Oid300Redirect`只是提前分类，不能越过C++位于
  non-character/OID尾部之前的first-BDY分支。统一predicate现覆盖`Damage || Oid300Redirect`。
- HitPlan新增first-body attempt projection/observation、完整实体/RNG snapshot、difference mask、成功abort/skip；
  World暴露对应桥接，NativeRandom scalar补同步表generation identity。
- focused fixture覆盖1xxx/encoded、chance成功/失败、manual stats、defense/type1/non-character、普通续行、
  per-attacker abort/next-attacker、DataOriented、ShadowCompare、zero-allocation和冻结criminal OID300。
- live collision probe新增从`CharacterAnimtorManager.GetCharacterConfig(300)`读取正式criminal frame30的
  见证；不复制或修改配置，probe结束恢复World统计/RNG/rest/sound/mode并回收全部owned entity。

## 验证证据

- TEST-FIRST：初始缺writer/result/HitPlan diagnostics编译RED；随后候选夹具修正后focused转绿；正式
  criminal OID300测试再次稳定RED为`expected frame33 / actual frame30`，predicate最小修复后GREEN。
- 编译：最后一次Unity refresh/domain reload成功，最终focused可发现并执行17项，0 error。
- focused：job `bd512d61d646462da2764130c8298e3b`，`17/17`，含formal criminal runner与ShadowCompare。
- HitPlan：job `9bf67a390581405e891c344fad4e525e`，`185/185`。
- B5：job `110352a126e448bf9b98e1c143ab8fc2`，`138/138`。
- NTSD28：job `5dc636f37aad407f83b7466b0405f716`，`318/318`。
- SelfCheck：2026-09-07 01:37:51，`PASS`。
- 真实Play：`Temp/NTSD_R8_WP01C_04_CollisionHitDamage.result.json`于01:34:39写出`PASS`，
  11 candidates；formal OID300/frame30/kind1033结果为frame33、group1、hold3/-3、counter77、HP100、
  vrest0，cleanup全部true；artifact SHA-256 `F5286842F6296489341E43199BBEE0BDC972BC6E1DFD936C89C8A11371747476`。
- 最终环境：已退出Play；Console error `0`；`NTSD_Battle` dirty=false/root13，Scene SHA-256仍为
  `50FD4D8FAF2CC5C630886CEFD4742A5FD068E1F7AADEE89809AD19B7B85AFF3C`；
  `criminal.dat` SHA-256 `B30BE71693077F21AB3F6599F120FB8DAA271B4FF8B68E1D9AE7AFFC1C12DFEA`。
- Change Ledger validator：`PASS`，341 Records / 297 governed code files in diff。

## 未扩大范围

未改Scene、Prefab、Config、资源、ProjectSettings、Slot容量、普通HUD或内容权威。下一步回到
`NTSD28-B5-REMAINING-EXIT-AUDIT-009`重新定位B5余下首差；本包不宣称整个4.7/B5已完成。

## 回滚

按Task Contract撤销本包七个生产/测试路径的增量；carrier和pure resolver不回退。
