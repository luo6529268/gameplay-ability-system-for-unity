# NTSD28 B5 Type3 Post-Hit Action Crosswalk

## 1. Authority identity and order

- 正式 EXE：`B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`。
- playable closure：`39DDDA154F5632C43089E2D5F1A5755ABBFBFD131A85D6B5ABF9AC00E6A46109`。
- 攻击者 post-hit 由 unarmored 与 reduced 两条路径共同调用；target type3 continuation 只在 unarmored
  target type3 分支执行。其后才执行 effect8..16 action override 和 type0 direct post-effect action。

## 2. Attacker post-hit action

| 条件/写入 | Authority | Unity 当前 | 结论 |
|---|---|---|---|
| eligible state | state3000；或 state3007 且当前帧 cover=2/3 | 四个 actual tail 和四个 hit-plan 投影只认 state3000 | `CONFIRMED_DIFFERENCE` |
| action | 当前帧 hit_Fj；0 回退 10 | 固定 10 | `CONFIRMED_DIFFERENCE` |
| counter/X | frame counter=0，motion.x=0 | 基本存在 | 需统一共享 owner |
| Z | 新动作帧的 `dvx` 原样写入 motion.z | standard/special 写 frame10 `dvz`；alternate 不写 Z | `CONFIRMED_DIFFERENCE` |
| content reachability | runtime 中 902 条 state3000 命中；187 条同帧带 hit_Fj | frozen Unity content 不在本审计改写 | 规则现实可达；内容差异仍归 H |
| state3007 cover | 21 条 state3007；未发现同帧 cover2/3 | frame-level cover carrier 不存在 | 规则 carrier 必须补；当前内容可达性待 H |

后续独立 Change：`NTSD28-B5-TYPE3-ATTACKER-POST-HIT-ACTION-001`。允许新增 frame-level cover carrier、
converter 字段、共享 resolver，并替换 actual/hit-plan 的固定 action 10；不处理 target ownership/kind transform。

## 3. Target type3 generic continuation

| 条件/写入 | Authority | Unity 当前 | 结论 |
|---|---|---|---|
| skip | target 当前 state3005 跳过 continuation | 以 state3005/3006 与 attacker state 混合决定 skip | 需重写为 source order |
| impulse | 仅清 pending impulse total XYZ；保留 contribution count | 清 Knockback XYZ、Runtime XYZ 与 AttackingCounter | `CONFIRMED_DIFFERENCE` |
| ownership source | attacker；若 link<0 且 parent active 则 parent | holder resolution 存在 | 部分可复用 |
| ownership writes | battle_group、owner_slot、control_slot=source physical slot | 只复制 RelationTeam/HolderCopySlot | 缺 owner/control 精确事务 |
| latch | special_hit_latch=true | HitConfirm2=1 | B0 绑定可复用，但需同一事务验证 |
| response field | ordinary `(type0 or link<0) && effect!=2/20` 读 target current hit_Fj，0→30；否则读 hit_Uj，0→20 | 直接固定 30/20 | `CONFIRMED_DIFFERENCE` |
| pair/hold tail | 只在 target type3 continuation 后运行 | 有相似 state sync/hold negate | 需保持但按 source order 复核 |

### 3.1 2026-09-06 prerequisite correction

后续 `NTSD28-B5-TYPE3-TARGET-CONTINUATION-PREREQUISITE-AUDIT-001` 读取完整 B0 binding 与 C22 后，
更正本节早期推断：authority `control_slot_000` 已正式绑定 Unity `Runtime.AnimCounter`，其语义是共享
locomotion cycle/特殊关联槽，不是玩家输入槽；`OwnerSlotIndex` 也是已验证 owner carrier。
pending impulse total/count 已由 `KnockbackVx/Vy/Vz + HitCount` 承载。无需新增 runtime carrier，
只需给 hit-plan 增加 `TargetOwnerSlot` 投影并修正生产事务。

## 4. Kind catalog transform

正式 `data/kind.dat` 只有一条记录：effect=209、frame=40、bound={8,209,213}、
respond={200,203,205,206,207,215,216}。命中记录时 target 继承 attacker definition/object id/type、group/owner，
action/action-latch/previous-action 同时写 response frame，counter=0，latch=true。

Unity 当前只有 OID 209/213/8 与 Karasu OID 的硬编码分支，没有通用 catalog parser、record iteration 或完整的
definition/identity/action-history 原子事务。因此该部分独立进入 kind-catalog implementation；Direction B 未决定前
可以实现正式 locked record/runtime carrier，但不得覆盖冻结内容文件。

## 5. Implementation order

1. `NTSD28-B5-TYPE3-ATTACKER-POST-HIT-ACTION-001`：frame cover + hit_Fj/dvx，actual/hit-plan。
2. `NTSD28-B5-TYPE3-TARGET-CONTINUATION-PREREQUISITE-AUDIT-001`：已确认 runtime carrier ready；
   hit-plan 仅缺 TargetOwnerSlot shadow field。
3. `NTSD28-B5-TYPE3-TARGET-GENERIC-CONTINUATION-001`：state3005 skip、ownership、只清 pending total、
   hit_Fj/hit_Uj、pair/hold order。
4. kind-catalog transform：通用 record 与原子 identity/action-history transaction。

每包均需 test-first、fresh compile、focused tests、相关 hit-plan/B5 tests、SelfCheck、Console、Scene 并发保护与
Change Ledger validator。涉及正式内容可达性和具体角色操作时再补同 seed/tick trace 与 Play 证据。

## 6. Generic continuation implementation result

`NTSD28-B5-TYPE3-TARGET-GENERIC-CONTINUATION-001 / VERIFIED` 已完成第3步：state3005精确跳过，
direct/active negative-link parent的group/owner/control写入，special latch，只清pending impulse total并保留
count/runtime velocity，以及target当前帧hit_Fj/hit_Uj与30/20 fallback均已同步actual/hit-plan。state1002/2000
的kind9会在候选预处理转换为kind0，因此也按generic语义验证；真正未转换kind9和locked kind record未被本包改写。
证据：focused11、HitPlan182、B5+HitPlan359、exact100/735、SelfCheck PASS、filtered CS0、Scene/Ledger PASS。
下一步固定为`NTSD28-B5-TYPE3-KIND-CATALOG-TRANSFORM-AUDIT-001`。

该审计现已由`NTSD28-B5-TYPE3-KIND-CATALOG-TRANSFORM-AUDIT-001 / VERIFIED`取代：candidate gate等价，
transform事务为confirmed difference；下一实施`NTSD28-B5-TYPE3-KIND-CATALOG-TRANSFORM-001`，详细字段矩阵见
`docs/ai/MANIFESTS/NTSD28-B5-TYPE3-KIND-CATALOG-TRANSFORM.md`。

实现现已由`NTSD28-B5-TYPE3-KIND-CATALOG-TRANSFORM-001 / VERIFIED`闭合；type3 attacker、generic target与
locked kind transform三部分均具备actual/HitPlan证据。下一只读执行type3 post-hit exit audit，再选择B5剩余owner。

## 7. Exit audit correction

`NTSD28-B5-TYPE3-POST-HIT-EXIT-AUDIT-001 / VERIFIED`确认family仍有连续尾段差异：matching 3005/3006
pair必须逐实体读取action-latch帧hit_Uj、0回退20并只清pending impulse；motion-hold release必须在每次type3
continuation后运行；Unity legacy `ApplyType3EffectTail`的effect5000/6000/23对type3目标无正式分支。
下一`NTSD28-B5-TYPE3-PAIR-RESET-HOLD-EFFECT-TAIL-001`，完成后重跑退出门。

## 2026-09-06 tail implementation closure

`NTSD28-B5-TYPE3-PAIR-RESET-HOLD-EFFECT-TAIL-001 / VERIFIED`已闭合上述三项：pair按双方latch-frame
`hit_Uj`且只清pending impulse；motion-hold无条件后置；legacy type3 effect5000/6000/23退役。
最终证据为focused7/7、HitPlan183/183、B5+HitPlan374/374、exact102类749/749、SelfCheck PASS、
Scene/Ledger PASS。family是否可退出仍由`NTSD28-B5-TYPE3-POST-HIT-EXIT-AUDIT-002`独立裁决。

## 8. Re-exit audit 002

`NTSD28-B5-TYPE3-POST-HIT-EXIT-AUDIT-002 / VERIFIED`复核确认上述已实现段保持，但发现正式
`battle_world.cpp:6615-6651`的initial matching 3005/3006 early-return尚未移植。该路径在普通
damage/resource/status/audio/hit-record之前只应用standard rests、pair reset和hold release；Unity当前先伤害再
尾部reset。下一独立包`NTSD28-B5-TYPE3-MATCHED-PAIR-EARLY-BRANCH-001`，完成后仍需第三次退出审计。

## 9. Early-branch implementation closure

`NTSD28-B5-TYPE3-MATCHED-PAIR-EARLY-BRANCH-001 / VERIFIED`已把initial matching3005/3006 gate前置到
sound/vital/status/join/object-hurt/effect/hit-record之前；仅提交现有standard rests、双方latch-hit_Uj reset和
hold release。actual focused4、HitPlan183、B5+HitPlan378、exact103/753、SelfCheck/Scene/Ledger均PASS。
standard-rest全局timing/recover数值仍由B5/F11共同owner处理，不在本包伪装为已完成。

## 10. Type3-specific family exit

`NTSD28-B5-TYPE3-POST-HIT-EXIT-AUDIT-003 / VERIFIED`逐段确认Type3-specific candidate、initial early branch、
attacker、generic/transform、late pair/hold和effect handoff均已闭合，允许该specific family退出。共同
standard-rest数值仍归B5/F11，audio/spark归B10，relation/cpoint归B6，content归H；这些owner关闭前不得把
具体Type3战斗场景写成整体完全一致。B5下一owner为`NTSD28-B5-STANDARD-HIT-REST-RECOVER-AUDIT-001`。
