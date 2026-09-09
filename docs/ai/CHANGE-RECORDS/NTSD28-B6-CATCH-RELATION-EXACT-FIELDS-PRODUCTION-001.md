# NTSD28-B6-CATCH-RELATION-EXACT-FIELDS-PRODUCTION-001

<!-- CHANGE-RECORD
id: NTSD28-B6-CATCH-RELATION-EXACT-FIELDS-PRODUCTION-001
status: VERIFIED
change-kind: TEST_FIRST_KIND3_CATCH_RELATION_ATOMIC_ALIGNMENT
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleInteractionWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B6CatchRelationExactFieldsProductionEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: NTSD 2.8-Logan BattleWorld28::resolve_kind3_catch_relation and native_relation_action; battle_world.cpp EB37E8EC; EXE B1E13AE1, closure 39DDDA15.
evidence: RED-0-OF-3 / FOCUSED-19-OF-19 / B6-CATEGORY-41-OF-41 / HITPLAN-185-OF-185 / NTSD28-167-OF-167 / TARGETED-PLAY-19-CASES / BUILDS-0-ERROR / SELFCHECK-BLOCKED-UNRELATED-HELD-ACCOUNTING / CONSOLE-0-ERROR / SCENE-UNCHANGED / EXACT-RELATION-ATOMIC
-->

> 状态：`VERIFIED / RED_0_OF_3 / FOCUSED_19_OF_19 / B6_CATEGORY_41_OF_41 / HITPLAN_185_OF_185 / NTSD28_167_OF_167 / TARGETED_PLAY_19_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED_HELD_ACCOUNTING / CONSOLE_0_ERROR / SCENE_UNCHANGED / EXACT_RELATION_ATOMIC`

## 改前事实

- actual `BattleInteractionWriter.TryApplyGrab()` 对kind1/3共用旧路径：缺action回退标准帧、未验证frame就先清Vx与
  改朝向、忽略signed action、只写compat `CatcherSlotIndex`、timeout硬编码300。
- shadow `ProjectGrabWriterEffect()` 缺 exact `TargetCatchSourceSlot90` capture/projection/diff；frame为空时仍用
  零几何投影，timeout同样硬编码300。
- current corrected corpus kind3为249，全部有action pair且respond0；exact source缺失是current reachable首差，
  signed/missing/respond是必须保留的通用规则。

## 预定实现

- actual仅对kind3走strict helper：解码action/facing、双frame preflight、成功后一次性写geometry/motion/frame/
  exact+compat relation/timeout/Fall。
- shadow对kind3使用相同preflight与投影语义；snapshot加入exact source，bit31同时比较 exact 与 compat。
- kind1继续走原路径，避免本包扩大到未由当前kind3 owner audit授权的兼容语义。

## 实际实现

- `BattleInteractionWriter`只将kind3分流到strict transaction：先读取first action、按负值独立翻双方facing、
  取绝对action并验证双方`FrameCache.HasFrame()`；成功后才写Vx/facing/frame/anchor/exact+compat relation/
  timeout/Fall。kind1继续走原共享兼容路径。
- `WriterEffectSnapshot`增加`TargetCatchSourceSlot90`；capture和strict kind3 projection同步该值，diff与compat
  catcher共用bit31。missing frame在shadow中投影为可比较的no-op，而不是误报pre-state mismatch。
- focused夹具覆盖current `criminal.dat` frame340作为attacker、synthetic character target frame130、slot0/511、
  first values、signed sides、respond、missing frame、kind1、shadow与0B；同夹具由Play runner执行。
- `BattleRuntimeSelfCheck`在既有held accounting阻塞点之前加入exact/signed/missing原子检查，并补强后段
  Audit4 reachable relation断言。

## 验证记录

- Unity EditMode behavioral RED job `8dc42cbbc5a94b65b7bd55515ffc33f7`：`0/3`。
  reachable success的exact source实际`-1`而预期slot0；missing caught frame实际返回`true`；signed
  catching frame实际写`-10`而预期绝对值`10`。三项均为目标旧行为，不是夹具/编译错误。
- 夹具加入后进程外 Editor build为0 error/104 warnings。
- GREEN focused job `090e8002cd684b23899a52195e0221e1`为`19/19`；最终同范围job
  `8ce2b8e2a5114226ba211c76b9a1f65a`同为`19/19`。
- B6 category最终job `c74fc37ddcfc4d8cbbfaecba00452da3`为`41/41`。
- HitPlan全类首次为`184/185`，唯一失败是本包投影对kind1多写exact source；纠正为strict-kind3-only后，
  job `887d89cb4c834a7fb1b96b43af2ca273`为`185/185`。该纠正保持kind1不变量。
- NTSD28 category job `e9cb18acd9c64010957add66d6e9c184`为`167/167`。
- 首次Play在current witness中错误地把`criminal.dat`同时当作target；Play中正式catalog正确将OID300识别为
  non-character，故writer按kind3 target gate返回false。夹具改为criminal attacker + synthetic character
  target（生产代码未因该失败改变）；最终Play结果`state=Passed / cases=19 / currentCriminal340=341/130 /
  shadowDifferenceMask=0 / warmedAllocationBytes=0`。
- Runtime/Editor `dotnet build --no-restore`均0 error；Unity 2022.3.62f3重编译0 error。
- full `BattleRuntimeSelfCheck`已通过本包提前插入的exact/signed/missing检查，下一首差仍是独立held-CPoint
  injury legacy accounting（更新后`BattleRuntimeSelfCheck.cs:11531`）。
- SelfCheck前后的Scene SHA-256/长度均为
  `89AA621640A3818F2C7CD307836C50D72CAB84B6D4F2B6DB333FF7CBF525A673`/216762；最终
  `isDirty=false`、rootCount16。Console在保留SelfCheck结果文件后清理并确认0 error。
- targeted `git diff --check`通过；全树只报告既有用户Scene trailing whitespace；
  `Tools/Validate-ChangeLedger.ps1`通过（424 records / 358 governed code files）。

## 未关闭项

下一严格包为mixed catch advance/control-flow fences；之后是settlement vaction preflight与held accounting。
本包不关闭positive-link retirement、完整resource、carrier/schema或content差异。

## 回滚

反向恢复本包列出的代码与夹具；不得回退任何前置 Change 或用户文件。
