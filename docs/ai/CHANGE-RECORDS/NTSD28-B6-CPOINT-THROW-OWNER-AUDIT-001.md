# NTSD28-B6-CPOINT-THROW-OWNER-AUDIT-001

<!-- CHANGE-RECORD
id: NTSD28-B6-CPOINT-THROW-OWNER-AUDIT-001
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: none
authority: NTSD 2.8-Logan CPoint kind-1 throw tail in BattleWorld28::advance_catch_relations; EXE B1E13AE1, closure 39DDDA15.
evidence: actual/formal/environment/parity/test owners closed; B5 readiness contracts rechecked and full MP transaction remains blocked by B7/B8/B11/H; environment/Vz package selected; HitPlan excluded; no behavior writes.
-->

> 状态：`VERIFIED / GOVERNANCE_ONLY / TWO_STAGE_SPLIT_DEFINED / FULL_RESOURCE_DEFERRED`

已冻结 CPoint throw settlement 的 formal data、actual writer、resource helper、environment carriers、
HitPlan/trace 与测试责任；没有修改脚本、内容或 Scene。

## 结论

- actual 只改 `BattleCpointWriter.ApplyThrow()`；CPoint advance 不进入 hit execution plan，故不新增
  HitPlan 分支。
- 本阶段只复用 `ResolveNativeHitResourceAttacker()` 作为 display lead 前置，并复用
  `ApplyNativeHitDisplaySteps()`；不复制 B5 算法。
- 完整 MP transaction 重新读取 B5 readiness 后确认仍依赖 B7 child suppression、B8 selected-mode
  override 与 B11/H authoritative baseMax；B6 禁止用 `MPMax` 或默认值提前接线。
- 正 `throwinjury` 在资源事务后写 caught `EnvironmentState320` 与 self
  `EnvironmentSourceSlot160`；`WeaponCount` 保持原值。
- Vz 只在 depth up/down 恰一项为真时写；两者同真或同假均保留原值。
- 已有 parity/checksum 完整覆盖所有受影响字段，无 persistent schema 变更。
- focused test先建立错误生产路径 RED；随后更新 SelfCheck 的两处 clear-Vz断言和
  WeaponCount断言。Play probe作为 production runtime 证据后置于代码绿灯。

`NTSD28-B6-CPOINT-THROW-ATOMIC-PRODUCTION-001` 因错误包含被阻塞的完整 MP transaction 已被
`SUPERSEDED`，不得作为证据。下一包：
`NTSD28-B6-CPOINT-THROW-ENVIRONMENT-VZ-PRODUCTION-001 / IN_PROGRESS / TEST_FIRST`。
