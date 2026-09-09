# NTSD28-B4-F04-DERIVED-WEAPON-OWNER-AUDIT-001 — derived weapon owner audit

<!-- CHANGE-RECORD
id: NTSD28-B4-F04-DERIVED-WEAPON-OWNER-AUDIT-001
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: none
authority: NTSD 2.8-Logan PhysicsIntegrator28 and playable per-slot physics owner; EXE B1E13AE1, closure 39DDDA15.
evidence: DERIVED-CALLER-CLOSED / LEGACY-BOOL-LOSS-CONFIRMED / VIRTUAL-LANDING-SEAM-DEFINED / NO-CODE-CHANGE
-->

> 状态：`VERIFIED / GOVERNANCE_ONLY / MIGRATION_SEAM_DEFINED`

只读证据确认 production 正常 pooled `LF2Weapon` 的 type 与 pool type 相等时不会进入 shared
owner，而是由 `LF2WeaponBase.RunFrameAdvancePhysics` 在 specialization 后调用 legacy
`WeaponDynamics`。该 bool seam 固定 y=0，并以绝对 `Y < -0.0001` 判空中，随后无参
`OnLanded()` 又进入四参数 landing wrapper，因此新 carrier 对正常武器仍不可见。

迁移必须保留 `WeaponFlightPhysics` 的 identity/gravity specialization、virtual `OnLanded`、
`OnInFlightFrameUpdate` 与 `_lastLandingVyBeforeClamp` 既有 snapshot。最小实现不新增持久字段：
result core 在同一调用栈把 collision reference 交给带参数 virtual overload；基类 overload
fallback 到旧无参 virtual，以兼容潜在 subclass。type3/OID999、producer、Audio/content另包。

无代码、Scene、Prefab、DAT、资源或 Authority 写入；无需运行时测试。
