# NTSD28-B5-REDUCED-HIT-DAMAGE-PURE-CORE-001 — reduced-hit damage pure core

<!-- CHANGE-RECORD
id: NTSD28-B5-REDUCED-HIT-DAMAGE-PURE-CORE-001
status: VERIFIED
change-kind: TEST_FIRST_PURE_CORE
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleReducedHitDamageResolver.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5ReducedHitDamagePureCoreEditorTests.cs
authority: NTSD 2.8-Logan damage_resolution.cpp DamageCalculator28::resolve_selected_armor and battle_world.cpp reduced-hit caller 7100-7108; EXE B1E13AE1, closure 39DDDA15.
evidence: RED-20 / FOCUSED-21 / B5-265 / HITPLAN-184 / NTSD28-BROAD-741 / SELFCHECK-PASS / SCENE-UNCHANGED / LEDGER-PASS
-->

> 状态：`VERIFIED / PURE_CORE_READY / PRODUCTION_UNCONNECTED`

## Authority 与 Unity 原状

- Authority 先用 raw injury 做 null/type4 `/10` 或 armor decrease/MP/HP 分流，再只对 HP damage 应用 target `+0x340`。
- Authority reduced-hit 分支不调用 attacker weakness；type3 明确 unsupported。
- Unity `ApplyAlternateDamage` 当前只做 `injury / 10`，并把旧 `FallDamageDiv` 前置到 raw injury；没有 armor 分流或 type3 unsupported。

## 范围、风险与回滚

本包只新增 pure resolver 与 focused tests，不接 production/HitPlan，不改 content/Scene。主要风险是 C# overflow、
负数截断与 C++ native signed division 语义漂移；以边界测试固定。回滚删除 resolver/test 及两个 meta，并移除本包活跃状态。

## 验收状态

- 新增 `BattleReducedHitDamageResolver` 与只读 result，显式实现 supported、HP/MP damage、runtime armor-HP delta，
  并在 HP 输出之后复用 native low-32-bit `*100 / +0x340` 顺序；API 不接受 attacker weak。
- red `34f7ec7beba44288a894151262c00825` 20/20；focused
  `4060253e7c2a4ace8721716f9db548e8` 21/21，含 4096 zero-allocation。
- B5 `e2dc2bd910b44810a0bfa54741f3ceba` 265/265；HitPlan
  `dad94802f2a74b378d35eefa5ad3fe1b` 184/184；NTSD28 broad
  `15a25d2ab1d4410792d689f4bc980747` 741/741。
- Runtime/Editor compile `2026-09-05T22:49:51Z / 22:49:52Z`；22:59:44Z SelfCheck PASS；
  filtered CS0；Scene unchanged；Ledger 284/246 PASS。

production/HitPlan/content 未接；回滚删除 resolver/test 与两个 meta 即可。下一 atomic ordinary-defense/reduced-hit integration。
