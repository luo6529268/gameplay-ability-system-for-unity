# NTSD28-B6-LEGACY-WEAPON-STATE-OWNER-AUDIT-001

<!-- CHANGE-RECORD
id: NTSD28-B6-LEGACY-WEAPON-STATE-OWNER-AUDIT-001
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: none
authority: NTSD 2.8-Logan EntityState28, BattleWorld28::settle_held_refill_objects, NativeAi28::step_non_character_hit_fa and PhysicsIntegrator28 current-frame state consumers; Unity NTSDEntityRuntime WeaponState writers/reader/copy/fingerprint/checksum/parity; historical 8101df55/9612bf2f/12949da0 provenance; EXE B1E13AE1, closure 39DDDA15.
evidence: authority has no parallel mutable weapon state or 1002-to-2000-to-3000/Vx-halving prelude; Unity sole gameplay reader and all writers enumerated; current/release OID124 action40..55 state1002 hit_Fa12 loop plus Tenten/Criminal2 direct spawn and current kind2/DVX reachability confirmed; first checksum difference tick1 and motion difference tick2 frozen; behavior retirement separated from user-direction-gated joint ReleaseTick/WeaponState schema disposition; no code/content/Scene changes.
-->

> 状态：`VERIFIED / GOVERNANCE_ONLY / NO_AUTHORITY_PARALLEL_STATE / BEHAVIOR_RETIREMENT_DEFINED / CURRENT_OID124_WITNESS / CARRIER_SCHEMA_DIRECTION_GATED / PRODUCTION_HELD`

`WeaponState`不是当前2.8 Authority字段。Unity以它驱动额外`1002→2000→3000`与Vx减半prelude；
OID124 action40..55的16帧正式`state1002/hit_Fa12`循环使该差异current/release均可达。Tenten/Criminal2
可直接生成action40，kind2 pickup后Naruto clone三条DVX也可进入该循环：tick1先污染checksum，tick2 motion分叉。

后继先原子退休旧behavior与动态producer、暂留reserved-0 carrier；删除carrier及snapshot/checksum schema迁移须与
`ReleaseTick`、Tracker、GrabbedBy与HolderCopySlot联合取得用户方向。完整closure、矩阵与回滚见
`docs/ai/TASKS/NTSD28-B6-LEGACY-WEAPON-STATE-OWNER-AUDIT-001.md`。当前runtime栈未清，本轮无code/content/Scene。
