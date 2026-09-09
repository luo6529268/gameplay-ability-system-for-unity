# NTSD28-B6-CATCH-CONTROL-FLOW-FENCES-OWNER-AUDIT-001

<!-- CHANGE-RECORD
id: NTSD28-B6-CATCH-CONTROL-FLOW-FENCES-OWNER-AUDIT-001
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: none
authority: NTSD 2.8-Logan BattleWorld28::advance_catch_relations and settle_catch_relations terminal continue boundaries; Unity BattleCpointWriter and BattleRuntimeSelfCheck; EXE B1E13AE1, closure 39DDDA15.
evidence: reciprocal mismatch and negative-decrease release proven terminal before throw/dircontrol; native frame counter maps AttackingCounter not HitCount; current SelfCheck fallback-tail assertions identified as stale; Unity/release first-kind1 counts 17/1709 with z!=0 and vaction<=0 both 0, negative-decrease+throw 0/6; indexed kind3/type0 join found release OID555 action73->72/130 then vaction517 produces 134 invalid post-vaction target pairs (121 missing frame, 13 no cpoint) versus Unity 0; advance package amended and separate settlement preflight package defined; runtime witnesses pending; production held; no code/content/Scene changes.
-->

> 状态：`VERIFIED / GOVERNANCE_ONLY / ADVANCE_PACKAGE_AMENDED / SETTLEMENT_PACKAGE_DEFINED / RELEASE_TREE_WITNESS / PRODUCTION_HELD`

当前 Authority 的 reciprocal mismatch 与 negative-decrease release 都在写完各自最小状态后立即
`continue`；Unity却继续 fallback throw/dircontrol，且 escape 分支误写 `HitCount` 而不是已验证的
`AttackingCounter`。现有 SelfCheck 对“C++ fallback tail”的断言必须在后续 advance 包中纠正。

settlement 的 hurtable/vaction 分支必须先提交 signed/zero action，再重新验证目标 frame 与首 kind-2 CPoint；
失败后禁止 injury/placement/cover。release `OID555 c\\yam\\a\\tre.dat` 提供 134 个静态 type-0 target
witness（121 缺 frame517、13 的517无CPoint），当前 indexed corpus为0。既有 advance mixed/exact package
吸收 terminal branch 修正，settlement preflight另建 actual-only包；`cpoint.z`保持独立 dormant schema。
当前runtime栈未清，本轮无脚本、content或Scene改动。

## CORPUS CORRECTION（2026-09-08）

current first-kind1/state9/negative/negative+throwvx/throw-or-dircontrol纠正为776/760/204/1/88；OID417
tree产生40个current invalid post-vaction pairs。terminal fences与两production owners保留。
