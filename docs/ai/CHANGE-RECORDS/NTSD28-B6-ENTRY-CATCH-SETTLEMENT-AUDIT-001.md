# NTSD28-B6-ENTRY-CATCH-SETTLEMENT-AUDIT-001

<!-- CHANGE-RECORD
id: NTSD28-B6-ENTRY-CATCH-SETTLEMENT-AUDIT-001
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: none
authority: NTSD 2.8-Logan post-hit catch relation/settlement and held/cpoint/impulse live path; EXE B1E13AE1, closure 39DDDA15.
evidence: authority post-hit order and both catch passes closed; current 4019-CPoint corpus measured; first post-hit catch difference is throw injury environment/resource plus no-depth Vz preservation; global B6 first-difference wording later corrected by NTSD28-B6-HELD-WPOINT-ENTRY-CORRECTION-AUDIT-001 because C09 held settlement runs before geometry; no behavior writes.
-->

> 状态：`VERIFIED / GOVERNANCE_ONLY / POST_HIT_SCOPE_ONLY / CPOINT_THROW_SETTLEMENT_ROUTED`

> Schema口径补充：4019是release decoded corpus；Direction-B当前33个CPoint blocks含A/T各1条。
> 后继`NTSD28-B6-CPOINT-27-SCALAR-SCHEMA-OWNER-AUDIT-001`另确认release三条`drain=600`与19→27字段缺口。

> **2026-09-08 纠正：** 本审计闭合的是 post-hit catch 子链。B6 全局顺序中更早还有
> C09 `settle_held_refill_objects()`；其 WPoint kind-3 release 差异已由
> `NTSD28-B6-HELD-WPOINT-ENTRY-CORRECTION-AUDIT-001` 路由。因此本文中“首差”均应读为
> “post-hit catch 子链首差”；既有 throw 修正本身仍有效。

只读核对Authority catch relation→settlement→held/stage→impulse及Unity抓取/CPoint owner；没有修改脚本、
内容或Scene。caughtact combo与cpoint resource必须等待真实B6事件，不能伪造近似接点。

## 已观察事实

- `SimulationTickDriver28::step()` 在两类 hit pass 后依次运行 catch advance、catch settlement、
  caughtact combo、第二次 stage/held settlement 和 impulse finalizer。
- `advance_catch_relations()` 的 kind-1 路径使用 tick-action snapshot；orphan kind-2 validation
  单独使用 current action。关系失配仅把 catcher action 写 0，不清历史关系列。
- 输入选择顺序是 A → T → D → UZ → DZ → facing-relative F/B → J，后匹配覆盖前匹配。
  Unity 只实现 A/T/J。直接扫描正式 `resources/runtime/decoded_dat` 得到 4019 条 CPoint；
  A/J/D/T/F/B/UZ/DZ 均无显式正式记录，因此该缺口保留但不冒充本轮 first difference。
- 正式 corpus 有 75 条最终非零 `throwvx`；去除 duplicate-scalar last-write 后有 9 条最终正
  `throwinjury`，其中 6 条同时具有非零 `throwvx`。Authority 对这 6 条先运行原生资源事务，
  再把 caught 的 `environment_state_320` 写为 injury、`environment_source_slot_160` 写为其
  自身 slot。Unity `BattleCpointWriter.ApplyThrow()` 写的是 `victim.WeaponCount`，没有资源事务。
- Authority 只有 depth-up/down 恰一项为真时才改 thrown Vz；Unity 先无条件 `Vz=0`，所以
  75 条 throw 在无 depth 输入且 victim 入场 Vz 非零时会产生可观察首差。
- 后续 post-hit settlement 仍有 held injury resource、cover hold timers、caughtact combo event 等
  独立差异，但它们晚于本次选出的 throw tail，不能越序实施。

## 结论与下一包

`POST_HIT_SCOPE_ONLY / CPOINT_THROW_SETTLEMENT_ROUTED`。当时的下一 post-hit 子包是
`NTSD28-B6-CPOINT-THROW-OWNER-AUDIT-001 / IN_PROGRESS / GOVERNANCE_ONLY`；先冻结复用 B5
resource pure core、Environment carriers、formal CPoint/legacy adapter、actual writer、HitPlan 与
测试边界，再决定最小 production 拆分。

## CORPUS CORRECTION（2026-09-08）

Direction-B current CPoint纠正为1426、A/T各9、throwvx58、positive throwinjury48。release75/9/6不变；
Authority规则与后续owner保留。
