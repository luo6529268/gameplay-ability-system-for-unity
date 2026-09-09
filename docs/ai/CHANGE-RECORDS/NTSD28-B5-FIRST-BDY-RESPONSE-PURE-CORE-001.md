# NTSD28-B5-FIRST-BDY-RESPONSE-PURE-CORE-001

<!-- CHANGE-RECORD
id: NTSD28-B5-FIRST-BDY-RESPONSE-PURE-CORE-001
status: VERIFIED
change-kind: PURE_LOGIC
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleFirstBodyResponseResolver.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5FirstBodyResponsePureCoreEditorTests.cs
authority: NTSD 2.8-Logan BattleWorld28::resolve_confirmed_unarmored_hit first-current-Bdy branches; EXE B1E13AE1, closure 39DDDA15.
evidence: RED CS0246; focused 39/39; B5 653/653; NTSD28 1118/1118; SelfCheck PASS; compile/Console/Scene/Ledger clean.
-->

> 状态：`VERIFIED / PURE_CORE_READY / BEHAVIOR_UNCONNECTED`

## Authority与原状

Authority在普通unarmored tail前按first-current-BDY kind决定1xxx/2xxx动作/组/hold，或解析encoded
chance/action/effect。carrier包已提供first kind/respond，但Unity没有独立、可零分配验证的纯决策owner。

## 实际修改

- 新增`BattleFirstBodyResponseResolver`及不可变result，表达None、ActionRange、EncodedNeedsRoll、
  EncodedChanceRejected、EncodedApplied。
- 1xxx/2xxx严格边界、1999→-1、respond映射与hold分支均已纯投影。
- encoded按2/3/3/1拆分；1..99 chance在无roll时返回NeedsRoll，提供roll后严格使用`roll < chance`。
- action `<999`、counter reset、effect 0..9组合、group/hold/manual-damage均只输出写意图，不改实体。
- raw injury原样携带；warm 100000次循环实测0 managed allocation。

## TEST-FIRST RED

在production resolver不存在时导入focused test，`Editor.log`得到预期
`NTSD28B5FirstBodyResponsePureCoreEditorTests.cs(254,24): error CS0246`，缺失类型为
`BattleFirstBodyResponseResult`。未出现本包之外的新生产错误；现在允许写最小pure实现。

## 验证证据

- TEST-FIRST RED：`Editor.log`得到预期CS0246，缺失`BattleFirstBodyResponseResult`。
- 首次最小实现暴露一个局部变量遮蔽CS0136；仅重命名action-range局部变量后编译恢复0 error。
- focused：job `85f52fae701045ba8b97ea83a01c79bd`，39/39 PASS。
- B5 broad：job `993856d4becf4f57a3d38f306e6df4bf`，653/653 PASS。
- NTSD28 broad：job `d83d47fcd3664efbaa3d99c284a361b9`，1118/1118 PASS。
- `BattleRuntimeSelfCheck`：2026-09-07 00:17:33，`PASS`；7条预期negative-path错误清理后，
  Unity Console error=0。
- Unity Editor未播放且编译空闲；Scene `NTSD_Battle.unity` dirty=false、rootCount=13，SHA-256仍为
  `50FD4D8FAF2CC5C630886CEFD4742A5FD068E1F7AADEE89809AD19B7B85AFF3C`。
- `git diff --check`通过；`Validate-ChangeLedger.ps1`通过（340 records / 295 governed code files）。

## 边界

resolver尚未由production runner调用，也未提交RNG、实体写入、HitPlan attempt shadow或per-attacker abort。
本包只证明pure decision合同，不能解释为first-BDY响应的生产行为已经对齐。

## 回滚

删除pure resolver与focused tests即可；不触及carrier、内容或场景。
